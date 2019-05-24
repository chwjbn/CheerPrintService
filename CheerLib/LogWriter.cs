using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CheerLib
{
    public class LogWriter
    {
        private static object lockObjet = new object();


        public static void Log(string data)
        {
            lock(lockObjet)
            {
                string filePath = String.Format("{0}log", AppDomain.CurrentDomain.BaseDirectory);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                filePath = string.Format("{0}/CheerLib.{1}.{2}.log",filePath,DateTime.Now.ToString("yyyyMMdd"), Process.GetCurrentProcess().Id);
                data = "["+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]"+data+ Environment.NewLine;

                File.AppendAllText(filePath, data);

                Console.Write(data);

            }
        }

        public static void Log(string logLevel,string data, params object[] pms)
        {
            data = "["+logLevel+"]"+data;
            data = string.Format(data, pms);
            Log(data);
        }

        public static void Info(string data, params object[] pms)
        {
            Log("Info",data,pms);
        }

        public static void Debug(string data, params object[] pms)
        {
            Log("Debug", data, pms);
        }

        public static void Warn(string data, params object[] pms)
        {
            Log("Warn", data, pms);
        }

        public static void Error(string data, params object[] pms)
        {
            Log("Error", data, pms);
        }
        


    }
}
