# Mailer
A simple console email sender to send reports from batch files without any configs.

## Make
Just compile with your target .NET (2.0, 3.5, 4.0+) on your Windows by its bundled C# compiler:
```
C:\Windows\Microsoft.NET\Framework\...\scs.exe Mailer.cs
```

Use enclosed simple *compile.cmd* with preset paths for these .NET.

## Usage
Run without arguments (or with '?' in any place) to get help.
```
Mailer.exe ?
```

Use at least one argument in such order:
```
Mailer to [subject [body [attach]]]
```

If instead *to* is '-', it will be used the parameter from source (see *Parameters in source code* below).
You can write a few recipients separated by ','.

If *subject* or *body* starts with:
* '-' take content from a file in *DOS 866*
* '=' take content from a file in *Windows 1251*
* '\*' take content from a file in *UTF-8*

Additionally after *subject* ':' you can specify a number of line in that file (from top or bottom, if negative). Default is 1 (the first line from top of file).

Same after *body* you can specify a number of lines of that file (from top or bottom, if negative). Default is 0 - entire file.

In *attach* you can specify a few filenames separated by ',' or ';' (it is adjustable - see *Parameters in source code* below).

If an argument contains spaces, it must be enclosed with quotes "".

If no arguments or there is '?' somewhere, it will be shown this Usage text.

## Examples
```
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
```

## Exit codes for ERRORLEVEL
* 0 - Normal
* 1 - Email sending was canceled
* 2 - Shown Usage
* 3 - Wrong argument
* 4 - File not found

## Parameters in source code
```
// The email server's IP or DNS host name.
const string HOST = "192.168.0.1";

// The TCP port of SMTP. Default is 25.
const int PORT = 25;

// Use the secured connection.
const bool SSL = false;

// A login to the email server and sender's email (usually they are equal).
const string USER = "sender@bank.ru";

// A textual sender's name to see in email message.
const string NAME = "Report Sender";

// A password encoded in Base64. Do not store any passwords as a plain text!
const string PASS = "c2VuZGVy";

// Email(s) by default. Maybe separated by ',' signs.
const string TO = "admin@bank.ru";

// An array of chars to be list separators.
const string LIST = ",;";

// An array of signatures to read files in the proper encoding: -DOS 866 (0), =Windows 1251 (1), *UTF8 (2).
const string MODE = "-=*"; // DOS 866, Windows 1251, UTF8
```

## License
Licensed under the Apache License, Version 2.0.
