#region License
/*
Copyright 2022-2024 Dmitrii Evdokimov
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

using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Lib
{
    public class Smtp : IDisposable
    {
        private readonly SmtpClient _client;

        public string UserName { get; }
        public string DisplayName { get; set; } = $"{App.Name} {Environment.MachineName}";

        /// <summary>
        /// Create from Windows credentials manager.
        /// </summary>
        public Smtp()
        {
            string host;
            int port;
            bool useTls;

            string filter = "SMTP *";
            var cred = CredentialManager.ReadCredential(filter);
            string name = cred.TargetName;

            try
            {
                var p = name.Split(' ');

                host = p[1];
                port = p.Length > 2 ? int.Parse(p[2]) : 25;

                useTls = name.EndsWith("tls", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw new Exception($"Windows Credential Manager '{name}' has wrong format.", ex);
            }

            UserName = cred.UserName
                ?? throw new Exception($"Windows Credential Manager '{name}' has no UserName.");
            string pass = cred.Password
                ?? throw new Exception($"Windows Credential Manager '{name}' has no Password.");

            _client = new SmtpClient(host, port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(UserName, pass),
                EnableSsl = useTls
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _client != null)
            {
                _client.Dispose();
            }
        }

        public async Task SendMessageAsync(string emails, string subj = "", string body = "", string[] files = null)
        {
            if (emails is null || emails.Length == 0 || _client is null)
                return;
            
            MailMessage mail = null;

            try
            {
                mail = new EmailMessage(UserName, DisplayName,
                    emails,
                    subj,
                    $"{body}{Environment.NewLine}--{Environment.NewLine}{App.Title}",
                    files);

                await _client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sending mail '{subj}' failed.");
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                mail.Dispose();
            }
        }
    }
}
