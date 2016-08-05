// Copyright (c) 2016 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace Lib
{
    class EmailMessage : MailMessage
    {
        public EmailMessage() { }

        public EmailMessage(string recipients, string subject, string body, string[] files)
        {
            this.From = new MailAddress("noreply", App.Name, Encoding.UTF8);

            this.To.Add(recipients.Replace(';', ','));
            // this.CC
            // this.Bcc
            // this.ReplyToList;

            this.Subject = subject;
            this.Body = body;
            // this.IsBodyHtml = true;
            // this.Priority = MailPriority.High;

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file.Trim());
                if (fi.Exists)
                {
                    Attachment attachment = new Attachment(fi.FullName);
                    ContentDisposition disposition = attachment.ContentDisposition;
                    disposition.CreationDate = fi.CreationTime;
                    disposition.ModificationDate = fi.LastWriteTime;
                    disposition.ReadDate = fi.LastAccessTime;
                    this.Attachments.Add(attachment);
                }
                else
                {
                    Trace.TraceWarning("Attachment file " + fi.FullName + " not found!");
                }
            }
        }
    }

    class EmailServer
    {
        public string Host;
        public int Port = 25;
        public bool Ssl = false;

        public string Username;
        public string Password;

        public int Timeout = 5;

        public EmailServer(string host, int port, bool ssl, string user, string pass)
        {
            Host = host;
            Port = port;
            Ssl = ssl;
            Username = user;
            Password = pass;
        }

        public void SendWait(EmailMessage email)
        {
            SmtpClient client = new SmtpClient(Host, Port);

            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(Username, Password);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = Ssl;
            // client.Timeout = Timeout * 1000; // Ignored for Async

            // The userState can be any object that allows your callback 
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            string userState = email.Subject;

            // Set the method that is called back when the send operation ends.
            client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

            int ms = 250; // 1/4 second to sleep

            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);
            var statusChecker = new StatusChecker(Timeout * 1000 / ms);

            // Create a timer that invokes CheckStatus after ms and every ms thereafter.
            // Console.WriteLine("{0:h:mm:ss.fff} Creating timer.\n", DateTime.Now);
            var stateTimer = new Timer(statusChecker.CheckStatus, autoEvent, ms, ms);

            Console.WriteLine("Sending message... wait {0} sec or press Esc to cancel.", Timeout);
            
            client.SendAsync(email, userState);

            // When autoEvent signals time is out, dispose of the timer.
            autoEvent.WaitOne();
            stateTimer.Dispose();
            if (statusChecker.Canceled)
            {
                client.SendAsyncCancel();
                Trace.TraceWarning("Отправка прервана пользователем.");
            }
            else if (statusChecker.TimedOut)
            {
                client.SendAsyncCancel();
                Trace.TraceWarning("Отправка прервана по таймауту.");
            }
            // Clean up.
            email.Dispose();
        }

        /// <summary>
        /// A callback routine if the email is sent or canceled.
        /// </summary>
        /// <param name="sender">The object sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            String token = (string)e.UserState;

            if (e.Cancelled)
            {
                Trace.TraceWarning("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Trace.TraceWarning("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Trace.TraceInformation("Message sent.");
            }
        }

        class StatusChecker
        {
            private int invokeCount;
            private int maxCount;

            public bool Canceled = false;
            public bool TimedOut = false;

            public StatusChecker(int count)
            {
                invokeCount = 0;
                maxCount = count;
            }

            // This method is called by the timer delegate.
            public void CheckStatus(Object stateInfo)
            {
                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
                invokeCount++;
                // Console.WriteLine("{0:h:mm:ss.fff} Checking status {1,2}.", DateTime.Now, invokeCount);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        // Reset the counter and signal the waiting thread.
                        invokeCount = 0;
                        Canceled = true;
                        autoEvent.Set();
                    }
                }
                else if (invokeCount == maxCount)
                {
                    // Reset the counter and signal the waiting thread.
                    invokeCount = 0;
                    TimedOut = true;
                    autoEvent.Set();
                }
            }
        }
    }
}
