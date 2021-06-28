// Copyright (c) 2016-2021 Dmitrii Evdokimov. All rights reserved.
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

using Lib;
using System;
using System.Diagnostics;

namespace Mailer
{
    public static class Program
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

            string to = args[0].Equals("-") 
                ? Parameters.TO 
                : args[0];

            string subj = (args.Length > 1) 
                ? args[1] 
                : "Message from " + Parameters.NAME;

            string body = (args.Length > 2) 
                ? args[2] 
                : "This is a message from " + Parameters.NAME;

            string attach = (args.Length > 3) 
                ? args[3] 
                : string.Empty;

            EmailSender sender = new EmailSender();

            string[] attachList = null;
            if (attach.Length > 0)
            {
                char[] sep = Parameters.LIST.ToCharArray();
                attachList = attach.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            }

            sender.SendWait(new EmailMessage(to, subj, body, attachList));

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

            Console.WriteLine("Все константы прописываются в тексте программы, затем она компилируется с помощью");
            //Console.WriteLine(@"{0}\Microsoft.NET\Framework\...\scs.exe в Вашем Windows).",
            //    Environment.GetEnvironmentVariable("SystemRoot"));
            Console.WriteLine(@"%SystemRoot%\Microsoft.NET\Framework\...\scs.exe в Вашей системе Windows.");
            Console.WriteLine();

            if (err.Length > 0)
            {
                Console.WriteLine("** Ошибка {0}: {1} **", exit, err);
                Console.WriteLine();
            }

            string to = Parameters.TO;
            string mode = Parameters.MODE;
            string list = Parameters.LIST;

            Console.WriteLine("Константы: {0} ({1}) шлет на {2}:{3}, SSL:{4}, TimeOut:{5}s.",
                Parameters.FROM, Parameters.NAME, Parameters.HOST, Parameters.PORT, Parameters.SSL, Parameters.TIMEOUT / 1000);
            Console.WriteLine("Параметры: to subject body attach (обязателен только первый из них).");
            Console.WriteLine();
            Console.WriteLine("to можно заменить на -, тогда будет подставлено {0}", to);
            Console.WriteLine("  из констант. Через запятую можно указать несколько адресатов.");
            Console.WriteLine();
            Console.WriteLine("subject и body можно взять из файл(ов), если они начинаются с:");
            Console.WriteLine("  {0}  кодировка DOS 866", mode[0]);
            Console.WriteLine("  {0}  кодировка Windows 1251", mode[1]);
            Console.WriteLine("  {0}  кодировка UTF-8", mode[2]);
            Console.WriteLine("subject[:N] - взять строку N (по-умолчанию -1 - последнюю).");
            Console.WriteLine("body[:N] - число строк от начала или конца файла (0 - все).");
            Console.WriteLine();
            Console.Write("В body и attach можно указать несколько файлов через {0}", list[0]);
            for (int i = 1; i < list.Length; i++)
            {
                Console.Write(" или {0}", list[i]);
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Если параметр содержит пробелы или пустой, его надо \"заключить в кавычки\".");
            Console.WriteLine("Если параметров нет вовсе или в них есть ?, то показывается эта помощь.");
            Console.WriteLine();

            Console.WriteLine("Примеры параметров:");
            //string a = string.Format("{0}@{1}.ru", Environment.GetEnvironmentVariable("USERNAME"),
            //    Environment.GetEnvironmentVariable("USERDOMAIN")).ToLower();
            
            string a = "-";

            Console.WriteLine("  {0} {1}", to, "Test");
            Console.WriteLine("  {0},b.{0},c.{0} {1} {2}", to, "Test", "Body-text");
            Console.WriteLine("  {0} \"{1}\" \"{2}\"", a, "Test subject", "Long body text with spaces.");
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test DOS file", "filename.txt", mode[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test Win file", "filename.txt", mode[1]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}", a, "Test UTF file", "filename.txt", mode[2]);
            Console.WriteLine("  {0} \"{1}\" \"{3}{2}\"", a, "Test include", "Body from DOS 866.txt", mode[0]);
            Console.WriteLine("  {0} \"{3}{1}\" \"{4}{2}\"", a, "Subject from UTF-8.txt", "Body from DOS 866.txt", mode[2], mode[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}:10", a, "Top 10 lines", @"c:\filename.txt", mode[0]);
            Console.WriteLine("  {0} \"{1}\" {3}{2}:-5", a, "Last 5 lines", @"c:\filename.txt", mode[0]);
            Console.WriteLine("  {0} {1} \"\" {1}", a, "report.txt");
            Console.WriteLine("  {0} {4}{1}:2 \"{2}\" {3}", a, "report.txt", "Date in subj, file attached.", "report.txt", mode[0]);
            Console.WriteLine("  {0} {3}{1}:-1 \"{2}\"", a, "error.log", "Last error in subj!", mode[1]);
            Console.WriteLine("  {0} {1} \"{2}\" file1.txt{3}file2.txt{3}file3.txt", a, "Files", "3 attachments.", list[0]);
            Console.WriteLine("  {0} \"{1}\" \"\" \"Report 2016.xlsm{2} My Doc.docx\"", a, "Just files", list[0]);

            Environment.Exit(exit);
        }
    }
}
