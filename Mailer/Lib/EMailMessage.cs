// Copyright (c) 2016-2017 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Lib
{
    class EmailMessage : MailMessage
    {
        public EmailMessage()
        {
        }

        public EmailMessage(string recipients, string subject, string body, string[] files,
            string mode, string list)
        {
            this.From = new MailAddress("noreply", App.Name, Encoding.UTF8);

            this.To.Add(recipients.Replace(';', ','));
            // this.CC
            // this.Bcc
            // this.ReplyToList;

            // this.IsBodyHtml = true;
            // this.Priority = MailPriority.High;

            if (subject.Length > 0)
            {
                int m = mode.IndexOf(subject[0]);
                if (m > -1)
                {
                    this.Subject = SubjBuilder(m, subject);
                }
                else
                {
                    this.Subject = subject;
                }
            }

            if (body.Length > 0)
            {
                if (mode.IndexOf(body[0]) > -1)
                {
                    string[] bodyList = body.Split(list.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    body = string.Empty;
                    foreach (string item in bodyList)
                    {
                        int m = mode.IndexOf(item[0]);
                        if (m > -1)
                        {
                            this.Body += BodyBuilder(m, item);
                        }
                    }
                }
                else
                {
                    this.Body = body;
                }
            }

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
        static string SubjBuilder(int mode, string item)
        {
            int line = -1; // the last line by default
            string file = item.Substring(1);

            int pos = item.LastIndexOf(':');
            if (pos > 2) // *C:\
            {
                file = item.Substring(1, pos - 1);
                int.TryParse(item.Substring(pos + 1), out line);
            }

            Encoding enc = GetModeEncoding(mode);
            return GetSubjContent(file, enc, line);
        }

        static string BodyBuilder(int mode, string item)
        {
            int lines = 0; // all content
            string file = item.Substring(1);

            int pos = item.LastIndexOf(':');
            if (pos > 2) // *C:\
            {
                file = item.Substring(1, pos - 1);
                int.TryParse(item.Substring(pos + 1), out lines);
            }

            Encoding enc = GetModeEncoding(mode);
            return GetBodyContent(file, enc, lines);
        }

        static Encoding GetModeEncoding(int mode)
        {
            switch (mode)
            {
                case 0: // "-"
                    return Encoding.GetEncoding(866);

                case 1: // "="
                    return Encoding.GetEncoding(1251);

                default: // "*"
                    return Encoding.UTF8;
            }
        }

        static string GetSubjContent(string file, Encoding enc, int line)
        {
            if (!File.Exists(file))
            {
                return string.Format("файл {0} не найден", file);
            }

            string[] content = File.ReadAllLines(file, enc);

            if (line > 0)
            {
                return content[line - 1];
            }
            else
            {
                return content[content.Length + line];
            }
        }

        static string GetBodyContent(string file, Encoding enc, int lines)
        {
            if (!File.Exists(file))
            {
                return string.Format("---- файл {0} не найден ----\n", file);
            }

            if (lines == 0) // entire file
            {
                return File.ReadAllText(file, enc);
            }

            if (lines == 1 || lines == -1)
            {
                return GetSubjContent(file, enc, lines);
            }

            string[] content = File.ReadAllLines(file, enc);
            int L = content.Length;

            int n = lines > 0 ? lines : -lines;
            n = (n < L) ? n : L;
            StringBuilder sb = new StringBuilder(n * 100);
            sb.AppendFormat("---- файл {0}", file);

            if (lines > 0)
            {
                if (n < L)
                {
                    sb.AppendFormat(", первые {0} из {1} строк ----", n, L);
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(" ----");
                }

                for (int i = 0; i < n; i++)
                {
                    sb.AppendLine(content[i]);
                }

            }
            else
            {
                if (n < L)
                {
                    sb.AppendFormat(", последние {0} из {1} строк ----", n, L);
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(" ----");
                    sb.AppendLine();
                }

                for (int i = L - n; i < L; i++)
                {
                    sb.AppendLine(content[i]);
                }
            }
            FileInfo fi = new FileInfo(file);
            sb.AppendFormat("---- eof {0}, {1}B, {2} ----", file, fi.Length, fi.LastWriteTime);
            sb.AppendLine();
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
