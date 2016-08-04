// Copyright (c) 2016 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.

// Compile with your target C:\Windows\Microsoft.NET\Framework\...\scs.exe Mailer.cs
// Run Mailer.exe without parameters (or with '?') to get help.
// Change yours parameters below inside this source file and then just recompile.

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;

namespace Mailer
{
    class Program
    {
        #region app.config
        ////////////// Change your parameters below this line! /////////////////////////////////////////

        /// <summary>
        /// The email server's IP or DNS host name.
        /// </summary>
        const string HOST = "192.168.0.1";

        /// <summary>
        /// The TCP port of SMTP. Default is 25.
        /// </summary>
        const int PORT = 25;

        /// <summary>
        /// Use the secured connection.
        /// </summary>
        const bool SSL = false;

        /// <summary>
        /// A login to the email server and sender's email (usually they are equal).
        /// </summary>
        const string USER = "sender@bank.ru";

        /// <summary>
        /// A textual sender's name to see in email message.
        /// </summary>
        const string NAME = "Report Sender";

        /// <summary>
        /// A password encoded in Base64. Do not store any passwords as a plain text!
        /// </summary>
        const string PASS = "c2VuZGVy";

        /// <summary>
        /// Email(s) by default. Maybe separated by ',' signs.
        /// </summary>
        const string TO = "admin@bank.ru";

        /// <summary>
        /// An array of chars to be list separators.
        /// </summary>
        const string LIST = ",;";

        /// <summary>
        /// An array of signatures to read files in the proper encoding: -DOS 866 (0), =Windows 1251 (1), *UTF8 (2).
        /// </summary>
        const string MODE = "-=*"; // DOS 866, Windows 1251, UTF8

        ////////////// Do not change anything below this line! /////////////////////////////////////////
        #endregion app.config

        /// <summary>
        /// A boolean flag if email is sent.
        /// </summary>
        static bool mailSent = false;

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
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
            mailSent = true;
        }

        /// <summary>
        /// The main entry point to the Application.
        /// </summary>
        /// <param name="args">The array of strings passed as optional parameters to execute.</param>
        public static void Main(string[] args)
        {
            if (args.Length == 0 || Environment.CommandLine.Contains("?"))
                Usage(string.Empty, 2);

            string to = args[0].Equals("-") ? TO : args[0];
            string subj = (args.Length > 1) ? args[1] : string.Empty; // "Test message";
            string body = (args.Length > 2) ? args[2] : string.Empty; // "This is a test e-mail message.";
            string attach = (args.Length > 3) ? args[3] : string.Empty;

            int mode;
            int start = 1; // 1: top, -1: last, n: number +/-
            int lines = 0; // count of lines, 0: all

            if (subj.Length > 0 && (mode = MODE.IndexOf(subj[0])) > -1)
            {
                string file = subj.Substring(1);
                int pos = subj.LastIndexOf(':');
                if (pos > 2) // -c:
                {
                    file = subj.Substring(1, pos);
                    int.TryParse(subj.Substring(pos + 1), out start);
                }
                subj = TakeFile(mode, file, start, 1);
            }

            if (body.Length > 0 && (mode = MODE.IndexOf(body[0])) > -1)
            {
                string file = body.Substring(1);
                int pos = body.LastIndexOf(':');
                if (pos > 2) // -c:
                {
                    file = body.Substring(1, pos);
                    int.TryParse(body.Substring(pos + 1), out lines);
                    if (lines < 0)
                    {
                        start = -1;
                        lines = -lines;
                    }
                }
                body = TakeFile(mode, file, start, lines);
            }

            SmtpClient client = new SmtpClient(HOST, PORT);
            MailAddress from = new MailAddress(USER, NAME, Encoding.UTF8);
            MailMessage message = new MailMessage();

            message.From = from;
            message.To.Add(to);
            message.Subject = subj;
            message.Body = body;

            if (attach.Length > 0)
            {
                char[] sep = LIST.ToCharArray();
                string[] attachList = attach.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                foreach (string file in attachList)
                {
                    FileInfo fi = new FileInfo(file.Trim());
                    if (fi.Exists)
                    {
                        Attachment attachment = new Attachment(fi.FullName);
                        ContentDisposition disposition = attachment.ContentDisposition;
                        disposition.CreationDate = fi.CreationTime;
                        disposition.ModificationDate = fi.LastWriteTime;
                        disposition.ReadDate = fi.LastAccessTime;
                        message.Attachments.Add(attachment);
                    }
                }
            }

            // Set the method that is called back when the send operation ends.
            client.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
            // The userState can be any object that allows your callback 
            // method to identify this send operation.
            // For this example, the userToken is a string constant.
            string userState = "message1";

            client.Credentials = new NetworkCredential(USER, FromBase64(PASS, 0));
            client.EnableSsl = SSL;

            client.SendAsync(message, userState);

            Console.WriteLine("Sending message... press Esc to cancel.");
            int exit = 0;
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
            while (!mailSent)
            {
                if (Console.KeyAvailable)
                {
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        client.SendAsyncCancel();
                        exit = 1;
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(250);
                }
            }

            // Clean up.
            message.Dispose();

            Environment.Exit(exit);
        }

        /// <summary>
        /// The Usage output if any errors or no parameters.
        /// </summary>
        /// <param name="err">An optional string to display the error.</param>
        /// <param name="exit">The exit code for OS.</param>
        static void Usage(string err, int exit)
        {
            Console.WriteLine();
            Console.WriteLine("Программа отправки email от {0} ({1}) на {2}:{3}", USER, NAME, HOST, PORT);
            Console.WriteLine("(все константы можно настроить в тексте программы и перекомпилировать с помощью");
            Console.WriteLine(@"{0}\Microsoft.NET\Framework\...\scs.exe в Вашем Windows).", 
                Environment.GetEnvironmentVariable("SystemRoot"));
            Console.WriteLine();

            if (err.Length > 0)
            {
                Console.WriteLine("** Ошибка {0}: {1} **", exit, err);
                Console.WriteLine();
            }

            Console.WriteLine("Параметры:    to [subject [body [attach]]]");
            Console.WriteLine();
            Console.WriteLine("Если вместо to \'-\', то будет подставлено \"{0}\",", TO);
            Console.WriteLine("также через запятую можно указать несколько адресатов.");
            Console.WriteLine();
            Console.WriteLine("Если subject или body начинается с:");
            Console.WriteLine("  \'{0}\'  взять из файла DOS 866", MODE[0]);
            Console.WriteLine("  \'{0}\'  взять из файла Windows 1251", MODE[1]);
            Console.WriteLine("  \'{0}\'  взять из файла UTF-8", MODE[2]);
            Console.WriteLine("дополнительно для subject после \':\' можно указать номер строки,");
            Console.WriteLine("а для body - число строк от начала или конца файла.");
            Console.WriteLine();
            Console.Write("В attach можно указать несколько файлов через \'{0}\'", LIST[0]);
            for (int i = 1; i < LIST.Length; i++)
            {
                Console.Write(" или \'{0}\'", LIST[i]);
            }
            Console.WriteLine(".");
            Console.WriteLine();
            Console.WriteLine("Если параметр содержит пробелы, его надо \"заключить в кавычки\".");
            Console.WriteLine("Если параметров нет или в них есть \'?\', то показывается эта помощь.");
            Console.WriteLine();

            Console.WriteLine("Примеры параметров:");
            //string a = string.Format("{0}@{1}.ru", Environment.GetEnvironmentVariable("USERNAME"),
            //    Environment.GetEnvironmentVariable("USERDOMAIN")).ToLower();
            string a = "-";
            Console.WriteLine("  {0} {1}", TO, "Test");
            Console.WriteLine("  {0},b.{0},c.{0} {1} {2}", TO, "Test", "Body-text");
            Console.WriteLine("  {0} \"{1}\" \"{2}\"", a, "Test subject", "Long body text with spaces.");
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test DOS file", "filename.txt", MODE[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test Win file", "filename.txt", MODE[1]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test UTF file", "filename.txt", MODE[2]);
            Console.WriteLine("  {0} \"{1}\" \"{3}{2}\"", a, "Test include", "Body from DOS 866.txt", MODE[0]);
            Console.WriteLine("  {0} \"{3}{1}\" \"{4}{2}\"", a, "Subject from UTF-8.txt", "Body from DOS 866.txt", MODE[2], MODE[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}:10", a, "Top 10 lines", @"c:\filename.txt", MODE[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}:-5", a, "Last 5 lines", @"c:\filename.txt", MODE[0]);
            Console.WriteLine("  {0} {1} \"\" {1}", a, "report.txt");
            Console.WriteLine("  {0} {4}{1}:2 \"{2}\" {3}", a, "report.txt", "Date in subj, file attached.", "report.txt", MODE[0]);
            Console.WriteLine("  {0} {3}{1}:-1 \"{2}\"", a, "error.log", "Last error in subj!", MODE[1]);
            Console.WriteLine("  {0} {1} \"{2}\" file1.txt{3}file2.txt{3}file3.txt", a, "Files", "3 attachments.", LIST[0]);
            Console.WriteLine("  {0} \"{1}\" \"\" \"Report 2016.xlsm{2} My Doc.docx\"", a, "Just files", LIST[0]);

            Environment.Exit(exit);
        }

        /// <summary>
        /// Takes a file content instead of a text string command line parameter.
        /// </summary>
        /// <param name="mode">Encoding of file: 0: DOS 866, 1: Windows 1251, 2: UTF8.</param>
        /// <param name="file">File to take its content.</param>
        /// <param name="start">The Start line of file (positive from top [1], negative from bottom [-1]).</param>
        /// <param name="lines">The Number of lines to take from the file (1 for the subject, 0 for the entire file).</param>
        /// <returns>The String to use instead of the parameter.</returns>
        static string TakeFile(int mode, string file, int start, int lines)
        {
            if (!File.Exists(file))
            {
                Usage("Файл " + file + " не найден!", 4);
            }

            Encoding enc = Encoding.UTF8;
            switch (mode)
            {
                case 0: // "-"
                    enc = Encoding.GetEncoding(866);
                    break;

                case 1: // "="
                    enc = Encoding.GetEncoding(1251);
                    break;

                case 2: // "*"
                    break;

                default: // unreal to be here
                    Usage("Неверный параметр в имени файла!", 3);
                    break;
            }

            if (lines == 0) // entire file, body only
            {
                return File.ReadAllText(file, enc);
            }

            string[] content = File.ReadAllLines(file, enc);

            if (lines == 1) // subject or body
            {
                if (start > 0)
                {
                    return content[start - 1];
                }
                else
                {
                    return content[content.Length + start];
                }
            }

            // body only
            int L = content.Length;
            int n = (lines < L) ? lines : L;
            StringBuilder sb = new StringBuilder(n * 100);

            if (start > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    sb.AppendLine(content[i]);
                }

                if (n < L)
                {
                    sb.AppendFormat("-- первые {0} из {1} строк файла --", n, L);
                }
            }
            else
            {
                if (n < L)
                {
                    sb.AppendFormat("-- последние {0} из {1} строк файла --\n", n, L);
                }

                for (int i = L - n; i < L; i++)
                {
                    sb.AppendLine(content[i]);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Decodes a Base64 string to a plain text string in a specified encoding.
        /// </summary>
        /// <param name="toDecode">The Base64 string to decode.</param>
        /// <param name="enc">The encoding for text string: 0: UTF8, 866: DOS, 1251: Windows, etc.</param>
        /// <returns>The Plain text string.</returns>
        static string FromBase64(string toDecode, int enc) // 0: UTF8, 866: DOS, 1251: Windows
        {
            byte[] bytes = Convert.FromBase64String(toDecode);
            Encoding encoding = (enc == 0) ? Encoding.UTF8 : Encoding.GetEncoding(enc);
            return encoding.GetString(bytes);
        }
    }
}
