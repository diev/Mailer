// Copyright (c) 2016-2017 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lib
{
    class AppTraceListener : DefaultTraceListener
    {
        public static TraceSwitch TraceConsole = new TraceSwitch("TraceConsole", "Trace level for console in config", "Error");
        public static TraceSwitch TraceLog = new TraceSwitch("TraceLog", "Trace level for log in config", "Verbose");

        public AppTraceListener()
        {
        }

        public AppTraceListener(string file)
        {
            base.LogFileName = file;

            string path = Path.GetDirectoryName(file);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            this.TraceEvent(eventCache, source, eventType, id, string.Format(format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            string msg = BuildMessage(eventType, message);

            //byte[] bytes = Encoding.GetEncoding(1251).GetBytes(sb.ToString().ToCharArray());
            if (string.IsNullOrEmpty(base.LogFileName))
            {
                if (TraceConsole.TraceVerbose)
                {
                    Console.Write(msg);
                }
            }
            else
            {
                if (TraceLog.TraceVerbose)
                {
                    File.AppendAllText(base.LogFileName, msg, Encoding.GetEncoding(1251));
                }
            }
        }

        private static string BuildMessage(TraceEventType eventType, string message)
        {
            StringBuilder sb = new StringBuilder();
            
            //sb.AppendFormat("{0:dd.MM.yyyy HH:mm:ss.fff} ", DateTime.Now);
            sb.AppendFormat("{0:dd.MM.yy HH:mm:ss} ", DateTime.Now);

            //if (eventType != TraceEventType.Information)
            //{
            //    sb.AppendFormat("{0} ", eventType); //Warning || Error
            //}

            //switch (eventType)
            //{
            //    case TraceEventType.Verbose:
            //        sb.Append("  ");
            //        break;
            //    case TraceEventType.Information:
            //        sb.Append("i ");
            //        break;
            //    case TraceEventType.Warning:
            //        sb.Append("w ");
            //        break;
            //    case TraceEventType.Error:
            //        sb.Append("e ");
            //        break;
            //    case TraceEventType.Critical:
            //        sb.Append("E ");
            //        break;
            //}

            if (Trace.IndentLevel > 0)
            {
                sb.Append(" ".PadRight(Trace.IndentLevel * Trace.IndentSize));
            }

            return sb.AppendLine(message).ToString();
        }
    }
}