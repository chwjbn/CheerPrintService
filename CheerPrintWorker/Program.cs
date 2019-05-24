using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gecko;

namespace CheerPrintWorker
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ApplicationExit += Application_ApplicationExit;


            InitBrw();

            Application.Run(new MainForm(args));
        }

        /// <summary>
        /// 当应用退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            UnInitBrw();
        }

        /// <summary>
        /// 未处理异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CheerLib.LogWriter.Error("Program.CurrentDomain_UnhandledException");
            CheerLib.LogWriter.Log(e.ToString());
        }

        /// <summary>
        /// 主线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            CheerLib.LogWriter.Error("Program.Application_ThreadException");
            CheerLib.LogWriter.Log(e.ToString());
        }

        /// <summary>
        /// 初始化浏览器环境
        /// </summary>
        private static void InitBrw()
        {
            try
            {
                var runRoot = AppDomain.CurrentDomain.BaseDirectory + "Firefox64";
                if (!Directory.Exists(runRoot))
                {
                    Directory.CreateDirectory(runRoot);
                }

                if (Xpcom.IsInitialized)
                {
                    return;
                }

                Xpcom.Initialize(runRoot);

            
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 反向初始化浏览器环境
        /// </summary>
        private static void UnInitBrw()
        {
            try
            {
                Xpcom.Shutdown();
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }
    }
}
