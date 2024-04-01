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

using System;

namespace Mailer
{
    public static class Parameters
    {
        /// <summary>
        /// The email server's IP or DNS host name (if empty: gateway).
        /// </summary>
        public static string HOST { get; }

        /// <summary>
        /// The TCP port of SMTP. Default is 25.
        /// </summary>
        public static int PORT { get; }

        /// <summary>
        /// Use the secured connection.
        /// </summary>
        public static bool TLS { get; }

        /// <summary>
        /// The timeout in ms.
        /// </summary>
        public static int TIMEOUT { get; } = 60000;

        /// <summary>
        /// The sender's name.
        /// </summary>
        public static string NAME { get; } = Environment.MachineName;

        /// <summary>
        /// The sender's email.
        /// </summary>
        public static string FROM { get; }

        /// <summary>
        /// A login to the SMTP server.
        /// </summary>
        public static string USER { get; }

        /// <summary>
        /// A password encoded in Base64. Do not store any passwords as a plain text!
        /// </summary>
        public static string PASS { get; }

        /// <summary>
        /// Email(s) by default. Maybe separated by ',' signs.
        /// </summary>
        public static string TO { get; }

        /// <summary>
        /// An array of chars to be list separators.
        /// </summary>
        public static string LIST { get; } = ",;";

        /// <summary>
        /// An array of signatures to read files in the proper encoding: -DOS 866 (0), =Windows 1251 (1), *UTF8 (2).
        /// </summary>
        public static string MODE { get; } = "-=*"; // DOS 866, Windows 1251, UTF8

        static Parameters()
        {
            string filter = "SMTP *";
            var cred = Lib.CredentialManager.ReadCredential(filter);
            string name = cred.TargetName;

            try
            {
                var p = name.Split(' ');

                HOST = p[1];
                PORT = p.Length > 2 ? int.Parse(p[2]) : 25;

                TLS = name.EndsWith("tls", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw new Exception($"Windows Credential Manager '{name}' has wrong format.", ex);
            }

            USER = cred.UserName
                ?? throw new Exception($"Windows Credential Manager '{name}' has no UserName.");
            PASS = cred.Password
                ?? throw new Exception($"Windows Credential Manager '{name}' has no Password.");

            FROM = USER;
            TO = USER;
        }
    }
}
