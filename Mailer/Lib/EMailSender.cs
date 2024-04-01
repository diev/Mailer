#region License
/*
Copyright 2016-2024 Dmitrii Evdokimov
Open source software

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using Mailer;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace Lib
{
    public class EmailSender
    {
        public string Host;
        public int Port;
        public bool Tls;

        public string Username;
        public string Password;

        public int Timeout;

        public EmailSender()
        {
            Host = Parameters.HOST ?? Gateway.DefaultGateway();
            Port = Parameters.PORT;
            Tls = Parameters.TLS;
            Username = Parameters.USER;
            Password = Lib.Password.Decode(Parameters.PASS);
            Timeout = Parameters.TIMEOUT;
        }

        public void SendWait(EmailMessage email)
        {
            SmtpClient client = new SmtpClient(Host, Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Username, Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = Tls
            };
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
            string token = (string)e.UserState;

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

        protected class StatusChecker
        {
            private int invokeCount;
            private readonly int maxCount;

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
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
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
