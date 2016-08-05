// Copyright (c) 2016 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/Mailer

// How to use:
// Compile with your target
// C:\Windows\Microsoft.NET\Framework\...\scs.exe /out:Mailer.exe /recurse:*.cs
// Run Mailer.exe without arguments (or with '?') to get the usage help.
// Change your const strings in Parameters.cs and just recompile after.
// You will get your own preconfigured Mailer for your batch scripts.

#define TRACE
//#define DEBUG

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using Lib;

namespace Mailer
{
    class Program
    {
        /// <summary>
        /// The main entry point to the Application.
        /// </summary>
        /// <param name="args">The array of strings passed as optional parameters to execute.</param>
        public static void Main(string[] args)
        {
            Trace.Listeners.Add(new AppTraceListener(App.Log));

            if (args.Length == 0 || Environment.CommandLine.Contains("?"))
                Usage(string.Empty, 2);

            string to = args[0].Equals("-") ? Parameters.TO : args[0];
            string subj = (args.Length > 1) ? args[1] : string.Empty; // "Test message";
            string body = (args.Length > 2) ? args[2] : string.Empty; // "This is a test e-mail message.";
            string attach = (args.Length > 3) ? args[3] : string.Empty;

            int mode;
            int start = 1; // 1: top, -1: last, n: number +/-
            int lines = 0; // count of lines, 0: all

            if (subj.Length > 0 && (mode = Parameters.MODE.IndexOf(subj[0])) > -1)
            {
                string file = subj.Substring(1);
                int pos = subj.LastIndexOf(':');
                if (pos > 2) // -c:...
                {
                    file = subj.Substring(1, pos);
                    int.TryParse(subj.Substring(pos + 1), out start);
                }
                subj = TakeFile(mode, file, start, 1);
            }

            if (body.Length > 0 && (mode = Parameters.MODE.IndexOf(body[0])) > -1)
            {
                string file = body.Substring(1);
                int pos = body.LastIndexOf(':');
                if (pos > 2) // -c:...
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

            EmailServer server = new EmailServer(
                Parameters.HOST, Parameters.PORT, Parameters.SSL,
                Parameters.USER, Password.Decode(Parameters.PASS));

            string[] attachList = null;
            if (attach.Length > 0)
            {
                char[] sep = Parameters.LIST.ToCharArray();
                attachList = attach.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            }

            server.SendWait(new EmailMessage(to, subj, body, attachList));

            Environment.Exit(0);
        }

        /// <summary>
        /// The Usage output if any errors or no parameters.
        /// </summary>
        /// <param name="err">An optional string to display the error.</param>
        /// <param name="exit">The exit code for OS.</param>
        static void Usage(string err, int exit)
        {
            Console.WriteLine();
            Console.WriteLine(App.Version + " - " + App.Description);
            Console.WriteLine(App.Copyright + " - " + "Source https://github.com/diev/Mailer");
            Console.WriteLine();

            if (err.Length > 0)
            {
                Console.WriteLine("** Ошибка {0}: {1} **", exit, err);
                Console.WriteLine();
            }

            Console.WriteLine("Параметры:    to subject body attach");
            Console.WriteLine();
            Console.WriteLine("Параметр to (или \'-\' вместо него) обязателен.");
            Console.WriteLine("Вместо \'-\' будет подставлено [{0}] из Parameters.cs,", Parameters.TO);
            Console.WriteLine("также через запятую можно указать несколько адресатов.");
            Console.WriteLine();
            Console.WriteLine("Если subject или body начинается с:");
            Console.WriteLine("  [\'{0}\']  взять из файла DOS 866", Parameters.MODE[0]);
            Console.WriteLine("  [\'{0}\']  взять из файла Windows 1251", Parameters.MODE[1]);
            Console.WriteLine("  [\'{0}\']  взять из файла UTF-8", Parameters.MODE[2]);
            Console.WriteLine("дополнительно для subject после \':\' можно указать номер строки,");
            Console.WriteLine("а для body - число строк от начала или конца файла.");
            Console.WriteLine();
            Console.Write("В attach можно указать несколько файлов через [\'{0}\'", Parameters.LIST[0]);
            for (int i = 1; i < Parameters.LIST.Length; i++)
            {
                Console.Write(" или \'{0}\'", Parameters.LIST[i]);
            }
            Console.WriteLine("].");
            Console.WriteLine();
            Console.WriteLine("Если параметр содержит пробелы, его надо \"заключить в кавычки\".");
            Console.WriteLine("Если параметров нет или в них есть \'?\', то показывается эта помощь.");
            Console.WriteLine();
            Console.WriteLine("Программа отправляет почту на [{0}:{1}] от имени {2}.", Parameters.HOST, Parameters.PORT, App.Name);
            Console.WriteLine("Все [константы] можно настроить в Parameters.cs и перекомпилировать с помощью");
            Console.WriteLine(@"%SystemRoot%\Microsoft.NET\Framework\...\scs.exe /recurse:*.cs в Вашем Windows.");
                //Environment.GetEnvironmentVariable("SystemRoot"));
            Console.WriteLine();

            Console.WriteLine("Примеры параметров:");
            //string a = string.Format("{0}@{1}.ru", Environment.GetEnvironmentVariable("USERNAME"),
            //    Environment.GetEnvironmentVariable("USERDOMAIN")).ToLower();
            string a = "-";
            Console.WriteLine("  {0} {1}", Parameters.TO, "Test");
            Console.WriteLine("  {0},b.{0},c.{0} {1} {2}", Parameters.TO, "Test", "Body-text");
            Console.WriteLine("  {0} \"{1}\" \"{2}\"", a, "Test subject", "Long body text with spaces.");
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test DOS file", "filename.txt", Parameters.MODE[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test Win file", "filename.txt", Parameters.MODE[1]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test UTF file", "filename.txt", Parameters.MODE[2]);
            Console.WriteLine("  {0} \"{1}\" \"{3}{2}\"", a, "Test include", "Body from DOS 866.txt", Parameters.MODE[0]);
            Console.WriteLine("  {0} \"{3}{1}\" \"{4}{2}\"", a, "Subject from UTF-8.txt", "Body from DOS 866.txt", Parameters.MODE[2], Parameters.MODE[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}:10", a, "Top 10 lines", @"c:\filename.txt", Parameters.MODE[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}:-5", a, "Last 5 lines", @"c:\filename.txt", Parameters.MODE[0]);
            Console.WriteLine("  {0} {1} \"\" {1}", a, "report.txt");
            Console.WriteLine("  {0} {4}{1}:2 \"{2}\" {3}", a, "report.txt", "Date in subj, file attached.", "report.txt", Parameters.MODE[0]);
            Console.WriteLine("  {0} {3}{1}:-1 \"{2}\"", a, "error.log", "Last error in subj!", Parameters.MODE[1]);
            Console.WriteLine("  {0} {1} \"{2}\" file1.txt{3}file2.txt{3}file3.txt", a, "Files", "3 attachments.", Parameters.LIST[0]);
            Console.WriteLine("  {0} \"{1}\" \"\" \"Report 2016.xlsm{2} My Doc.docx\"", a, "Just files", Parameters.LIST[0]);

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
    }
}