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
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace Lib
{
    public class EmailMessage : MailMessage
    {
        public EmailMessage(string from, string name, string recipients, string subject = "", string body = "", string[] files = null)
        {
            From = new MailAddress(from, name, Encoding.UTF8);

            To.Add(recipients.Equals('-')
                ? from
                : recipients.Replace(';', ','));

            // this.CC
            // this.Bcc
            // this.ReplyToList;

            // this.IsBodyHtml = true;
            // this.Priority = MailPriority.High;

            string mode = Program.MODE;

            if (subject.Length > 0)
            {
                int m = mode.IndexOf(subject[0]);

                if (m > -1)
                {
                    Subject = SubjBuilder(m, subject);
                }
                else
                {
                    Subject = subject;
                }
            }

            if (body.Length > 0)
            {
                if (mode.IndexOf(body[0]) > -1)
                {
                    string list = Program.LIST;
                    string[] bodyList = body.Split(list.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    foreach (string item in bodyList)
                    {
                        int m = mode.IndexOf(item[0]);
                        if (m > -1)
                        {
                            Body += BodyBuilder(m, item);
                        }
                    }
                }
                else
                {
                    Body = body;
                }
            }

            if (files != null)
            {
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

                        Attachments.Add(attachment);
                    }
                    else
                    {
                        Trace.TraceWarning("Attachment file " + fi.FullName + " not found!");
                    }
                }
            }
        }

        private static string SubjBuilder(int mode, string item)
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

        private static string BodyBuilder(int mode, string item)
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

        private static Encoding GetModeEncoding(int mode)
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

        private static string GetSubjContent(string file, Encoding enc, int line)
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

        private static string GetBodyContent(string file, Encoding enc, int lines)
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
