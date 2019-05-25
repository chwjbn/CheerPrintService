using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using CheeLibExt.MessageQueue.Redis;
using CheerLib.Model;

namespace CheerPrintMaster.Model
{
    public class PrintTaskExcutor
    {

        private string mDataRoot = string.Empty;  //数据目录

        private string mHtmlPath = string.Empty;   //HTML文件下载路径

        private string mPdfPath = string.Empty;   //PDF文件输出路径

        private string mWorkerTaskXmlPath = string.Empty;  //worker任务xml路径

        private string mWorkerExcutorPath = string.Empty; //worker文件路径

        private PrintTaskNodeData mPrintTaskNodeData = new PrintTaskNodeData();  //打印数据


        private string mChannelId = string.Empty;  //订阅频道ID

        private ReaderWriterLockSlim mOppLock = new ReaderWriterLockSlim();  //操作锁,一个频道一次只能跑一个任务


        /// <summary>
        /// 打印任务状态
        /// </summary>
        public enum PrintTaskStatus
        {
            None=0,
            InitEnv=1,
            DownLoadHtmlFile=2,
            ProcessPrint=3,
            DataReport=4
        }

        public class ResultMessage:JsonData
        {
            public int error_code = 500;
            public string msg = "faild.";
        }


        private PrintTaskStatus mCurrentPrintTaskStatus = PrintTaskStatus.None;  //当前任务状态

        private ResultMessage mLastResultMessage = new ResultMessage(); //最后的结果信息


        public PrintTaskExcutor(string iChannelId)
        {
            this.mChannelId = iChannelId;
        }

        public void RunExcutor()
        {
            try
            {
                var channelName = string.Format("cheer_print_task_{0}", this.mChannelId);
                var iTaskMessageRecv = new TaskMessageQueueReciever(channelName);
                iTaskMessageRecv.OnTaskMessage += ITaskMessageRecv_OnTaskMessage;
                iTaskMessageRecv.start();
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 任务消息回调
        /// </summary>
        /// <param name="msgContent"></param>
        private void ITaskMessageRecv_OnTaskMessage(string msgContent)
        {
            this.mOppLock.EnterWriteLock();
            try
            {
                CheerLib.LogWriter.Info("{0}.processTask Begin,channelId={1}", this.GetType().FullName, this.mChannelId);

                var xPrintTaskNodeData = JsonData.FromJson<PrintTaskNodeData>(msgContent);
                this.processTask(xPrintTaskNodeData);

                CheerLib.LogWriter.Info("{0}.processTask End,channelId={1}", this.GetType().FullName, this.mChannelId);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
            this.mOppLock.ExitWriteLock();
        }

        private void processTask(PrintTaskNodeData iPrintTaskNodeData)
        {
            try
            {
                this.mPrintTaskNodeData = iPrintTaskNodeData;

                CheerLib.LogWriter.Info("{0}.processTask Begin", this.GetType().FullName);

                mCurrentPrintTaskStatus = PrintTaskStatus.InitEnv;
                if (!this.stepInitEnv())
                {
                    CheerLib.LogWriter.Error("{0}.runTask Faild,stepInitEnv==false", this.GetType().FullName);
                    mCurrentPrintTaskStatus = PrintTaskStatus.DataReport;

                    this.mLastResultMessage.error_code = 501;
                    this.mLastResultMessage.msg = "PrintMaster stepInitEnv Faild!";
                }
                else
                {
                    mCurrentPrintTaskStatus = PrintTaskStatus.DownLoadHtmlFile;
                }


                if (!this.stepDownLoadHtmlFile())
                {
                    CheerLib.LogWriter.Error("{0}.runTask Faild,stepDownLoadHtmlFile==false", this.GetType().FullName);
                    mCurrentPrintTaskStatus = PrintTaskStatus.DataReport;

                    this.mLastResultMessage.error_code = 502;
                    this.mLastResultMessage.msg = "PrintMaster stepDownLoadHtmlFile Faild!";
                }
                else
                {
                    mCurrentPrintTaskStatus = PrintTaskStatus.ProcessPrint;
                }

                if (!this.stepProcessPrint())
                {
                    CheerLib.LogWriter.Error("{0}.runTask Faild,stepProcessPrint==false", this.GetType().FullName);
                    mCurrentPrintTaskStatus = PrintTaskStatus.DataReport;

                    this.mLastResultMessage.error_code = 503;
                    this.mLastResultMessage.msg = "PrintMaster stepProcessPrint Faild!";
                }
                else
                {
                    mCurrentPrintTaskStatus = PrintTaskStatus.DataReport;
                }


                this.stepDataReport();

                CheerLib.LogWriter.Info("{0}.processTask End", this.GetType().FullName);

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 初始化环境
        /// </summary>
        private bool stepInitEnv()
        {
            var bRet = false;

            if (this.mCurrentPrintTaskStatus!=PrintTaskStatus.InitEnv)
            {
                bRet = true;
                return bRet;
            }

            CheerLib.LogWriter.Info("{0}.stepInitEnv Begin",this.GetType().FullName);

            try
            {
                this.mDataRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskData", string.Format("Task_{0}_{1}_{2}", this.mPrintTaskNodeData.uuid, this.mPrintTaskNodeData.id, DateTime.Now.ToString("yyyyMMddHHmmss")));

                if (!Directory.Exists(this.mDataRoot))
                {
                    Directory.CreateDirectory(this.mDataRoot);
                }

                this.mHtmlPath = Path.Combine(this.mDataRoot, "task_html.html");
                this.mPdfPath = Path.Combine(this.mDataRoot, "task_pdf.pdf");
                this.mWorkerTaskXmlPath = Path.Combine(this.mDataRoot, "task_worker.xml");

                var currentParentPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,".."));

                this.mWorkerExcutorPath = Path.Combine(currentParentPath, "CheerPrintWorker", "CheerPrintWorker.exe");

                if (!File.Exists(this.mWorkerExcutorPath))
                {
                    CheerLib.LogWriter.Error("{0}.stepInitEnv [{1}] not exsist!",this.mWorkerExcutorPath);
                    return bRet;
                }

                bRet = true;
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return bRet;

        }

        /// <summary>
        /// 下载HTML文件
        /// </summary>
        /// <returns></returns>
        private bool stepDownLoadHtmlFile()
        {
            var bRet = false;

            if (this.mCurrentPrintTaskStatus != PrintTaskStatus.DownLoadHtmlFile)
            {
                bRet = true;
                return bRet;
            }

            CheerLib.LogWriter.Info("{0}.stepDownLoadHtmlFile Begin", this.GetType().FullName);

            try
            {
                var htmlContent = this.httpGet(this.mPrintTaskNodeData.html_file_url);

                if (string.IsNullOrEmpty(htmlContent))
                {
                    return bRet;
                }

                File.WriteAllText(this.mHtmlPath, htmlContent, Encoding.UTF8);

                bRet = true;
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return bRet;


        }

        /// <summary>
        /// 处理打印
        /// </summary>
        /// <returns></returns>
        private bool stepProcessPrint()
        {
            var bRet = false;

            if (this.mCurrentPrintTaskStatus != PrintTaskStatus.ProcessPrint)
            {
                bRet = true;
                return bRet;
            }

            CheerLib.LogWriter.Info("{0}.stepProcessPrint Begin", this.GetType().FullName);

            try
            {

                //生成
                this.makeWorkerXml();

                if (!File.Exists(this.mWorkerTaskXmlPath))
                {
                    CheerLib.LogWriter.Info("{0}.stepProcessPrint mWorkerTaskXmlPath=[{1}] not exists!", this.GetType().FullName, this.mWorkerTaskXmlPath);
                    return bRet;
                }

                //跑进程
                int exitCode = this.runProcess(this.mWorkerExcutorPath, this.mWorkerTaskXmlPath);

                CheerLib.LogWriter.Info("{0}.stepProcessPrint runProcess.exitCode={1}",this.GetType().FullName,exitCode);

                if (!File.Exists(this.mPdfPath))
                {
                    CheerLib.LogWriter.Info("{0}.stepProcessPrint mPdfPath=[{1}] not exists!", this.GetType().FullName, this.mPdfPath);
                    return bRet;
                }

                bRet = true;
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return bRet;
        }

        /// <summary>
        /// 开始数据上报
        /// </summary>
        /// <returns></returns>
        private bool stepDataReport()
        {
            var bRet = false;

            if (this.mCurrentPrintTaskStatus != PrintTaskStatus.DataReport)
            {
                bRet = true;
                return bRet;
            }

            CheerLib.LogWriter.Info("{0}.stepDataReport Begin", this.GetType().FullName);

            try
            {
                int errorCode = this.mLastResultMessage.error_code;
                string msg = this.mLastResultMessage.msg;
                var data = string.Empty;

                if (errorCode<300)
                {
                    data = this.readOutPdfContent();
                }

                var url = this.mPrintTaskNodeData.task_callback_url;

                var postKvData = new Dictionary<string, string>();
                postKvData.Add("check",string.Format("{0}",0));
                postKvData.Add("error_code", string.Format("{0}", errorCode));
                postKvData.Add("msg",msg);
                postKvData.Add("data", data);

                var  nTryTimes = 5;

                while (true)
                {
                    nTryTimes--;

                    if (nTryTimes<0)
                    {
                        break;
                    }

                    var resultContent = this.httpPost(url, postKvData);

                    CheerLib.LogWriter.Log("resultContent="+resultContent);

                    if (!string.IsNullOrEmpty(resultContent))
                    {
                        var resultMsg = JsonData.FromJson<ResultMessage>(resultContent);
                        if (resultMsg.error_code == 200)
                        {
                            break;
                        }
                    }

                    Thread.Sleep(1000*5);
                }
                

                bRet = true;
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return bRet;
        }


        /// <summary>
        /// 读取输出PDF文件内容
        /// </summary>
        /// <returns></returns>
        private string readOutPdfContent()
        {
            var data = string.Empty;
            try
            {
                if (!File.Exists(this.mPdfPath))
                {
                    return data;
                }

                var dataBuff = File.ReadAllBytes(this.mPdfPath);
                data = Convert.ToBase64String(dataBuff);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return data;
        }

        /// <summary>
        /// 运行进程
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="runArgs"></param>
        /// <returns></returns>
        private int runProcess(string filePath,string runArgs)
        {
            int nRet = 0;

            try
            {
                var startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.FileName = filePath;
                startInfo.Arguments = runArgs;

                var proc = Process.Start(startInfo);

                bool bRunRet = proc.WaitForExit(1000 * 60 * 3);  //最长跑3分钟

                //超时
                if (!bRunRet)
                {
                    //杀掉进程
                    this.killProcess(proc);

                    nRet = -100;
                    return nRet;
                }

                nRet = proc.ExitCode;

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return nRet;
        }

        /// <summary>
        /// 杀进程
        /// </summary>
        /// <param name="proc"></param>
        private void killProcess(Process proc)
        {
            try
            {
                var pid=proc.Id;

                var startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.StandardErrorEncoding = Encoding.UTF8;
                startInfo.StandardOutputEncoding = Encoding.UTF8;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = string.Format("/c TASKKILL /PID {0} /T /F", pid);

                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                var killProc = Process.Start(startInfo);

                killProc.WaitForExit(1000 * 60);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }


        /// <summary>
        /// 生成打印worker配置xml
        /// </summary>
        private void makeWorkerXml()
        {
            try
            {
                var xmlDoc = new XmlDocument();

                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));

                var printNode = xmlDoc.CreateElement("print");

                printNode.AppendChild(this.createXmlElement(xmlDoc, "input_html_path",this.mHtmlPath,true));
                printNode.AppendChild(this.createXmlElement(xmlDoc, "output_pdf_path", this.mPdfPath, true));

                printNode.AppendChild(this.createXmlElement(xmlDoc, "html_window_width", this.mPrintTaskNodeData.html_window_width, false));
                printNode.AppendChild(this.createXmlElement(xmlDoc, "html_window_height", this.mPrintTaskNodeData.html_window_height, false));

                printNode.AppendChild(this.createXmlElement(xmlDoc, "portrait", this.mPrintTaskNodeData.orientation_flag, false));

                printNode.AppendChild(this.createXmlElement(xmlDoc, "page_width", this.mPrintTaskNodeData.page_width, false));
                printNode.AppendChild(this.createXmlElement(xmlDoc, "page_height", this.mPrintTaskNodeData.page_height, false));

                printNode.AppendChild(this.createXmlElement(xmlDoc, "margin_top", this.mPrintTaskNodeData.margin_top, false));
                printNode.AppendChild(this.createXmlElement(xmlDoc, "margin_bottom", this.mPrintTaskNodeData.margin_bottom, false));
                printNode.AppendChild(this.createXmlElement(xmlDoc, "margin_left", this.mPrintTaskNodeData.margin_left, false));
                printNode.AppendChild(this.createXmlElement(xmlDoc, "margin_right", this.mPrintTaskNodeData.margin_right, false));

                xmlDoc.AppendChild(printNode);

                xmlDoc.Save(this.mWorkerTaskXmlPath);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 创建XML节点元素
        /// </summary>
        /// <param name="xmlDoc">文档</param>
        /// <param name="eleName">元素名称</param>
        /// <param name="eleValue">元素值</param>
        /// <param name="useCData">是否使用CDATA</param>
        /// <returns></returns>
        private XmlElement createXmlElement(XmlDocument xmlDoc,string eleName,object eleValue,bool useCData=false)
        {
            var xmlNode = xmlDoc.CreateElement(eleName);

            var eleVal = string.Format("{0}",eleValue);

            if (useCData)
            {
                xmlNode.AppendChild(xmlDoc.CreateCDataSection(eleVal));
            }
            else
            {
                xmlNode.AppendChild(xmlDoc.CreateTextNode(eleVal));
            }

            return xmlNode;
        }

        /// <summary>
        /// POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        private string httpPost(string url,Dictionary<string,string>postKvData)
        {
            var sRet = string.Empty;

            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.75 Safari/537.36");

                var postData = new MultipartFormDataContent();

                foreach (var kv in postKvData)
                {
                    postData.Add(new StringContent(kv.Value),kv.Key);
                }
                
                var resp = httpClient.PostAsync(url, postData).Result;

                if (!resp.IsSuccessStatusCode)
                {
                    return sRet;
                }

                var dataBuffer = resp.Content.ReadAsByteArrayAsync().Result;

                sRet = Encoding.UTF8.GetString(dataBuffer);

                httpClient.Dispose();

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return sRet;
        }

        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string httpGet(string url)
        {
            var sRet = string.Empty;

            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.75 Safari/537.36");

                var resp = httpClient.GetAsync(url).Result;

                if (!resp.IsSuccessStatusCode)
                {
                    return sRet;
                }

                var dataBuffer = resp.Content.ReadAsByteArrayAsync().Result;

                sRet = Encoding.UTF8.GetString(dataBuffer);

                httpClient.Dispose();

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }

            return sRet;
        }
    }
}
