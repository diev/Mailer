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

#define TRACE
//#define DEBUG

using System;
using System.Diagnostics;

using Lib;

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
                : $"Message from {Parameters.NAME}.";

            string body = (args.Length > 2) 
                ? args[2] 
                : $"This is a message from {Parameters.NAME}.";

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
            Console.WriteLine($"{App.Version} - {App.Description}");
            Console.WriteLine($"{App.Copyright}");
            Console.WriteLine();

            if (err.Length > 0)
            {
                Console.WriteLine($"** Ошибка {exit}: {err} **");
                Console.WriteLine();
            }

            string to = Parameters.TO;
            string mode = Parameters.MODE;
            string list = Parameters.LIST;

            Console.WriteLine($"Из Windows Credential Manager прочитаны параметры SMTP:");
            Console.WriteLine($@"{Parameters.FROM} ({Parameters.NAME}) шлет на {Parameters.HOST}:{Parameters.PORT}, TLS:{(Parameters.TLS ? "вкл" : "выкл")}.");
            Console.WriteLine($"Параметры ком.строки: to subject body attach (обязателен только первый из 4).");
            Console.WriteLine();
            Console.WriteLine($"to можно заменить на -, тогда будет подставлено {to}");
            Console.WriteLine($"  Через запятую можно указать несколько адресатов.");
            Console.WriteLine();
            Console.WriteLine($"subject и body можно взять из файл(ов), если они начинаются с:");
            Console.WriteLine($"  {mode[0]}  кодировка DOS 866");
            Console.WriteLine($"  {mode[1]}  кодировка Windows 1251");
            Console.WriteLine($"  {mode[2]}  кодировка UTF-8");
            Console.WriteLine($"subject[:N] - взять строку N (по-умолчанию -1 - последнюю).");
            Console.WriteLine($"body[:N] - число строк от начала или конца файла (0 - все).");
            Console.WriteLine();
            Console.Write($"В body и attach можно указать несколько файлов через {list[0]}");
            for (int i = 1; i < list.Length; i++)
            {
                Console.Write($" или {list[i]}");
            }   Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($@"Если параметр содержит пробелы или пустой, его надо ""заключить в кавычки"".");
            Console.WriteLine($"Если параметров нет вовсе или в них есть ?, то показывается эта помощь.");
            Console.WriteLine();

            Console.WriteLine($"Примеры параметров:");
            Console.WriteLine();
            Console.WriteLine($"  {to} Test");
            Console.WriteLine($"  {to},b.{to},c.{to} Test Body-text");
            Console.WriteLine($@"  - ""Test subject"" ""Long body text with spaces.""");
            Console.WriteLine($@"  - ""Test DOS file"" {mode[0]}filename.txt");
            Console.WriteLine($@"  - ""Test WIN file"" {mode[1]}filename.txt");
            Console.WriteLine($@"  - ""Test UTF file"" {mode[2]}filename.txt");
            Console.WriteLine($@"  - ""Test include"" ""{mode[0]}Body from DOS 866.txt""");
            Console.WriteLine($@"  - ""{mode[2]}Subject from UTF-8.txt"" ""{mode[0]}Body from DOS 866.txt""");
            Console.WriteLine($@"  - ""Top 10 lines"" {mode[0]}c:\filename.txt:10");
            Console.WriteLine($@"  - ""Last 5 lines"" {mode[0]}c:\filename.txt:-5");
            Console.WriteLine($@"  - report.txt """" report.txt");
            Console.WriteLine($@"  - {mode[0]}report.txt:2 ""Date in subj, file attached."" report.txt");
            Console.WriteLine($@"  - {mode[1]}error.log:-1 ""Last error in subj!""");
            Console.WriteLine($@"  - Files ""3 attachments."" file1.txt{list[0]}file2.txt{list[0]}file3.txt");
            Console.WriteLine($@"  - ""Just files"" """" ""Report 2016.xlsm{list[0]}My Doc.docx""");

            Environment.Exit(exit);
        }
    }
}
