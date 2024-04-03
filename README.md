# [Mailer]

A simple console email sender to send reports from batch files without 
any configs. Mailer gives you a powerful way to replace the former favorite 
Blat when it becomes out of use.

Простой консольный почтовый клиент для отправки почтовых сообщений из 
консольных коммандных файлов, когда программа Blat, некогда популярная, 
уже не удовлетворяет.

## Settings

*Windows Credential Manager* (find it in *Control Panel*):

Add a target name: `SMTP {host} {port} [tls]`,
with user name: `{sender@host}`, password: `{password}`.

## Usage

Run without arguments (or with '?' in any place) to get help.

    Mailer.exe ?

Use at least one argument in such order:

    Mailer to subject body attach

If '-' is placed instead *to*, the *sender*'s address will be used.
You can write a few recipients separated 
by ',' (or ';', they will be properly replaced).

If *subject* or *body* starts with:

  * '-' take content from a file in *DOS 866*
  * '=' take content from a file in *Windows 1251*
  * '\*' take content from a file in *UTF-8*

Additionally after *subject* ':' you can specify a number of a line
in that file (from top otherwise bottom, if negative).
Default is 1 (the first line from top of file).

Same after *body* you can specify a number of lines of that file
(from top otherwise bottom, if negative). Default is 0 - entire file.

In *attach* you can specify a few filenames separated by ',' and ';' 

If an argument contains spaces, it must be enclosed with quotes "".

If no arguments or there is '?' somewhere, it will be shown this Usage text.

## Examples

    Mailer - Test
    Mailer a,b,c Test Body-text
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

## Requirements

- .Net Framework 4.8

## License

Licensed under the [Apache License, Version 2.0].

[Mailer]: https://diev.github.io/Mailer/
[Apache License, Version 2.0]: LICENSE
