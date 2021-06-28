# Mailer

[![Build status]][appveyor]
[![GitHub Release]][releases]

A simple console email sender to send reports from batch files without 
any configs.

## Make

Just compile with your target .NET (2.0, 3.5, 4.0+) on your Windows by 
its bundled C# compiler:

    C:\Windows\Microsoft.NET\Framework\...\scs.exe /out:Mailer.exe /recurse:*.cs

Use enclosed simple *make.cmd* with preset paths for these .NET.
Of course you can use MSBuild itself.

## Usage

Run without arguments (or with '?' in any place) to get help.

    Mailer.exe ?

Use at least one argument in such order:

    Mailer to subject body attach

If instead *to* is '-', it will be used the parameter from source 
(see *Parameters.cs* below). You can write a few recipients separated 
by ',' (or ';', they will be properly replaced).

If *subject* or *body* starts with:

  * '-' take content from a file in *DOS 866*
  * '=' take content from a file in *Windows 1251*
  * '\*' take content from a file in *UTF-8*

Additionally after *subject* ':' you can specify a number of line in that 
file (from top or bottom, if negative). Default is 1 (the first line from 
top of file).

Same after *body* you can specify a number of lines of that file (from top 
or bottom, if negative). Default is 0 - entire file.

In *attach* you can specify a few filenames separated by ',' and ';' 
(it is adjustable - see *Parameters.ps* below).

If an argument contains spaces, it must be enclosed with quotes "".

If no arguments or there is '?' somewhere, it will be shown this Usage text.

## Examples

    Mailer admin@bank.ru Test
    Mailer admin@bank.ru,b.admin@bank.ru,c.admin@bank.ru Test Body-text
    Mailer - "Test subject" "Long body text with spaces."
    Mailer - "Test DOS file" -filename.txt
    Mailer - "Test Win file" =filename.txt
    Mailer - "Test UTF file" *filename.txt
    Mailer - "Test include" "-Body from DOS 866.txt"
    Mailer - "*Subject from UTF-8.txt" "-Body from DOS 866.txt"
    Mailer - "Top 10 lines" -c:\filename.txt:10
    Mailer - "Last 5 lines" -c:\filename.txt:-5
    Mailer - report.txt "" report.txt
    Mailer - -report.txt:2 "Date in subj, file attached." report.txt
    Mailer - =error.log:-1 "Last error in subj!"
    Mailer - Files "3 attachments." file1.txt,file2.txt,file3.txt
    Mailer - "Just files" "" "Report 2016.xlsm, My Doc.docx"

## Exit codes for ERRORLEVEL

(They still can be changed during the further development.)

  * 0 - Normal
  * 1 - Email sending was canceled
  * 2 - Shown Usage
  * 3 - Wrong argument
  * 4 - File not found

## Parameters.cs

```cs
        /// <summary>
        /// The email server's IP or DNS host name (if empty: gateway).
        /// </summary>
        public const string HOST = "";

        /// <summary>
        /// The TCP port of SMTP. Default is 25.
        /// </summary>
        public const int PORT = 25;

        /// <summary>
        /// Use the secured connection.
        /// </summary>
        public const bool SSL = true;

        /// <summary>
        /// The timeout in ms.
        /// </summary>
        public const int TIMEOUT = 60000;

        /// <summary>
        /// The sender's name.
        /// </summary>
        public const string NAME = "Sender";

        /// <summary>
        /// The sender's email.
        /// </summary>
        public const string FROM = "noreply@bank.ru";

        /// <summary>
        /// A login to the SMTP server.
        /// </summary>
        public const string USER = "sender@bank.ru";

        /// <summary>
        /// A password encoded in Base64. Do not store any passwords as a plain text!
        /// </summary>
        public const string PASS = "********";

        /// <summary>
        /// Email(s) by default. Maybe separated by ',' signs.
        /// </summary>
        public const string TO = "admin@bank.ru";

        /// <summary>
        /// An array of chars to be list separators.
        /// </summary>
        public const string LIST = ",;";

        /// <summary>
        /// An array of signatures to read files in the proper encoding: -DOS 866 (0), =Windows 1251 (1), *UTF8 (2).
        /// </summary>
        public const string MODE = "-=*"; // DOS 866, Windows 1251, UTF8
```

## License

Licensed under the [Apache License, Version 2.0].

[Mailer]: https://diev.github.io/Mailer/
[Apache License, Version 2.0]: http://www.apache.org/licenses/LICENSE-2.0 "LICENSE"

[appveyor]: https://ci.appveyor.com/project/diev/mailer/branch/master
[releases]: https://github.com/diev/Mailer/releases/latest

[Build status]: https://ci.appveyor.com/api/projects/status/ukoqyhda8b706p02/branch/master?svg=true
[GitHub Release]: https://img.shields.io/github/release/diev/Mailer.svg
