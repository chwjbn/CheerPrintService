using System;
using System.IO;

namespace CheerLib.Service
{
    public abstract class MainTaskService:InterfaceService
    {

        public MainTaskService()
        {
            initTaskMonitor();
        }


        //获取当前运行的应用的名称
        private string getCurrentTaskAppName()
        {
            string sRet = string.Empty;

            sRet = AppDomain.CurrentDomain.SetupInformation.ApplicationName;

            sRet = Path.GetFileNameWithoutExtension(sRet);

            return sRet;
        }

      
        private void initTaskMonitor()
        {
            try
            {
                var appName = this.getCurrentTaskAppName();
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        public void runService(string[] args)
        {

            CheerLib.LogWriter.Info("{0}.startService begin.",this.GetType().FullName);

            this.startService();

            CheerLib.LogWriter.Info("{0}.startService end.", this.GetType().FullName);

            while (true)
            {
                string standardInput = Console.ReadLine();

                if(string.IsNullOrEmpty(standardInput))
                {
                    continue;
                }

                if (standardInput == "status")
                {
                    this.showStatus();
                    continue;
                }

                if (standardInput == "clear")
                {
                    Console.Clear();
                    continue;
                }

                if(standardInput=="exit")
                {
                    break;
                }


            }
        }

        public abstract void startService();
        public abstract void showStatus();
        
    }
}
