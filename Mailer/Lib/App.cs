// Copyright (c) 2016 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Lib
{
    /// <summary>
    /// Properties of application
    /// </summary>
    class App
    {
        #region Info
        /// <summary>
        /// Файл приложения
        /// </summary>
        public static readonly string Exe = Assembly.GetCallingAssembly().Location;
        /// <summary>
        /// Путь к размещению файла приложения
        /// </summary>
        public static readonly string Dir = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// Файл записи лога приложения
        /// </summary>
        public static string Log = Path.ChangeExtension(App.Exe, string.Format("{0:yyyyMMdd}.log", DateTime.Now));

        static Assembly assembly = Assembly.GetCallingAssembly();
        static AssemblyName assemblyName = assembly.GetName();
        public static readonly string Name = assemblyName.Name;

        /// <summary>
        /// Версия приложения вида 1.1.60805.0
        /// </summary>
        public static readonly Version Ver = assemblyName.Version;
        /// <summary>
        /// Дата версии приложения
        /// </summary>
        public static readonly DateTime Dated = DateTime.Parse(string.Format("201{0}-{1}-{2}", Ver.Build / 10000, Ver.Build % 10000 / 100, Ver.Build % 100));
        /// <summary>
        /// Строка названием и версией приложения
        /// </summary>
        public static readonly string Version = string.Format("{0} v{1}", Name, Ver); //Ver.ToString(2), Ver.Build, Ver.Revision

        //public static readonly string attribute = (attribute == null) ? string.Empty : attribute;
        static AssemblyDescriptionAttribute descriptionAttribute = AssemblyDescriptionAttribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
        public static readonly string Description = descriptionAttribute.Description;

        static AssemblyCompanyAttribute companyAttribute = AssemblyCompanyAttribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
        /// <summary>
        /// Компания разработчика приложения
        /// </summary>
        public static readonly string Company = companyAttribute.Company;

        static AssemblyCopyrightAttribute copyrightAttribute = AssemblyCopyrightAttribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
        /// <summary>
        /// Авторские права на приложение
        /// </summary>
        public static readonly string Copyright = copyrightAttribute.Copyright;
        #endregion Info

        #region Paths
        /// <summary>
        /// Checks if a directory exists. If not, tries to create.
        /// </summary>
        /// <param name="dir">A directory to check or create.</param>
        /// <returns>Returns the checked or created directory.</returns>
        public static string CheckDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (DirectoryNotFoundException)
                {
                    ExitError(2, "Ошибка создания директории " + dir);
                    //If wrong, creates in the App directory.
                    //Trace.TraceError("Директорию не создать:" + ex.Message);
                    //dir = Path.Combine(App.Dir, "_Recovery", Path.GetDirectoryName(dir));
                    //Directory.CreateDirectory(dir);
                    //Trace.TraceWarning("Recovery directory {0} created.", dir);
                }
                catch (IOException)
                {
                    ExitError(2, "Нет диска для директории " + dir);
                    //Trace.TraceError("Сетевой диск не доступен: " + ex.Message);
                    //dir = Path.Combine(App.Dir, "_Recovery", Path.GetDirectoryName(dir));
                    //Directory.CreateDirectory(dir);
                    //Trace.TraceWarning("Recovery directory {0} created.", dir);
                }
                catch (Exception ex)
                {
                    ExitError(2, "Другая ошибка создания директории " + dir +
                        Environment.NewLine + ex.Message);
                }
            }
            return dir;
        }
        #endregion Paths

        #region Exit
        /// <summary>
        /// Завершить приложение с указанными кодом и информационным сообщением
        /// </summary>
        /// <param name="code">Код завершения</param>
        /// <param name="msg">Текст информации</param>
        public static void ExitInformation(int code, string msg)
        {
            Trace.TraceInformation(ExitMessage(code, msg));
            Exit(code);
        }

        /// <summary>
        /// Завершить приложение с указанными кодом и предупреждающим сообщением
        /// </summary>
        /// <param name="code">Код завершения</param>
        /// <param name="msg">Текст предупреждения</param>
        public static void ExitWarning(int code, string msg)
        {
            Trace.TraceWarning(ExitMessage(code, msg));
            Exit(code);
        }

        /// <summary>
        /// Завершить приложение с указанными кодом и сообщением об ошибке
        /// </summary>
        /// <param name="code">Код завершения</param>
        /// <param name="msg">Текст ошибки</param>
        public static void ExitError(int code, string msg)
        {
            Trace.TraceError(ExitMessage(code, msg));
            Exit(code);
        }

        /// <summary>
        /// Завершить приложение с указанными кодом
        /// </summary>
        /// <param name="code">Код завершения</param>
        public static void Exit(int code)
        {
            Trace.Close();
            Environment.Exit(code);
        }

        public static string ExitMessage(int code, string msg)
        {
            StringBuilder sb = new StringBuilder();
            if (code > 0)
            {
                sb.Append("Exit ");
                sb.Append(code);
            }
            if (msg != null)
            {
                sb.Append(": ");
                sb.Append(msg);
            }
            return sb.ToString();
        }
        #endregion Exit
    }
}