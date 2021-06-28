// Copyright (c) 2016-2021 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/

using System;
using System.Diagnostics;
using System.Text;

namespace Lib
{
    public class Password
    {
        /// <summary>
        /// The base encoding for text strings. 0: UTF8, 866: DOS, 1251: Windows, etc.
        /// </summary>
        public static int EnCoding = 0;

        /// <summary>
        /// Decodes a Base64 string to a plain text string.
        /// </summary>
        /// <param name="toDecode">The Base64 coded string to decode.</param>
        /// <returns>The plain text string ready to use.</returns>
        public static string Decode(string toDecode)
        {
            if (toDecode.Contains("*"))
            {
                Trace.TraceWarning("Звездочки вместо пароля.");
                return toDecode;
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(toDecode);
                Encoding encoding = (EnCoding == 0) ? Encoding.UTF8 : Encoding.GetEncoding(EnCoding);
                return encoding.GetString(bytes);
            }
            catch (Exception)
            {
                Trace.TraceError("Пароль поврежден.");
                //ex.MessageError of Base64:
                //Входные данные не являются действительной строкой Base - 64, поскольку содержат символ в кодировке,
                //отличной от Base 64, больше двух символов заполнения или недопустимый символ среди символов заполнения.
            }
            return string.Empty;
        }

        /// <summary>
        /// Encodes the string to Base64.
        /// </summary>
        /// <param name="toEncode">The source string to encode.</param>
        /// <returns>The string encoded to store.</returns>
        public static string Encode(string toEncode)
        {
            //byte[] bytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            Encoding encoding = (EnCoding == 0) ? Encoding.UTF8 : Encoding.GetEncoding(EnCoding);
            byte[] bytes = encoding.GetBytes(toEncode);
            return Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
        }
    }
}
