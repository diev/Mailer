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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Lib
{
    /// <summary>
    /// Properties of application
    /// </summary>
    public class App
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
        public static string Log = Path.ChangeExtension(Exe, string.Format("{0:yyyyMMdd}.log", DateTime.Now));

        static readonly Assembly _assembly = Assembly.GetCallingAssembly();
        static readonly AssemblyName _assemblyName = _assembly.GetName();
        public static readonly string Name = _assemblyName.Name;

        // Major.Minor.Build.Revision
        /// <summary>
        /// Версия программы
        /// </summary>
        public static readonly Version Ver = _assemblyName.Version;
        /// <summary>
        /// Название и версия программы в виде строки (если есть - с номером построения)
        /// </summary>
        public static readonly string Version = string.Format("{0} v{1}", Name, Ver.ToString(3)) + 
            (Ver.Revision > 0 ? " build " + Ver.Revision.ToString() : string.Empty);
 
        //public static readonly string attribute = (attribute == null) ? string.Empty : attribute;
        static readonly AssemblyDescriptionAttribute descriptionAttribute = 
            Attribute.GetCustomAttribute(_assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
        public static readonly string Description = descriptionAttribute.Description;

        static readonly AssemblyCompanyAttribute _companyAttribute = 
            Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
        /// <summary>
        /// Компания разработчика приложения
        /// </summary>
        public static readonly string Company = _companyAttribute.Company;

        static readonly AssemblyCopyrightAttribute _copyrightAttribute = 
            Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
        /// <summary>
        /// Авторские права на приложение
        /// </summary>
        public static readonly string Copyright = _copyrightAttribute.Copyright;

        public static readonly string Title = Name + " v" + Ver;
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
                sb.Append("Exit ").Append(code);
            }
            if (msg != null)
            {
                sb.Append(": ").Append(msg);
            }
            return sb.ToString();
        }
        #endregion Exit
    }
}
