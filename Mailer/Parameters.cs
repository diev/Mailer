// Copyright (c) 2016-2021 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/Mailer

namespace Mailer
{
    public static class Parameters
    {
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
    }
}
