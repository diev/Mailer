// Copyright (c) 2016 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/Mailer

namespace Mailer
{
    class Parameters
    {
        /// <summary>
        /// The email server's IP or DNS host name.
        /// </summary>
        public const string HOST = "192.168.0.1";

        /// <summary>
        /// The TCP port of SMTP. Default is 25.
        /// </summary>
        public const int PORT = 25;

        /// <summary>
        /// Use the secured connection.
        /// </summary>
        public const bool SSL = false;

        /// <summary>
        /// A login to the email server and sender's email (usually they are equal).
        /// </summary>
        public const string USER = "sender@bank.ru";

        /// <summary>
        /// A password encoded in Base64. Do not store any passwords as a plain text!
        /// </summary>
        public const string PASS = "c2VuZGVy";

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