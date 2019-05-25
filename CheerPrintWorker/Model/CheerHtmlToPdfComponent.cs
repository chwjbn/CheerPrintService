using Gecko;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CheerPrintWorker.Model
{
    public partial class CheerHtmlToPdfComponent : Component, nsIWebProgressListener
    {
        /// <summary>
        /// PDF生成事件
        /// </summary>
        public class PdfMakingStatus : EventArgs
        {
            public int percentage;    //进度
            public string statusText;   //状态信息
        }

        private GeckoWebBrowser mWebBrowser = null;   //火狐浏览器控件

        private CheerPrintArgs mCheerPrintArgs = new CheerPrintArgs();   //打印参数

        private string mDataRoot = string.Empty;   //数据目录  
        private string mHtmlPdfPath = string.Empty;   //html打印成pdf路径
        private string mPdfPath = string.Empty;    //pdf二次处理后pdf路径


        public event EventHandler<PdfMakingStatus> mStatusChanged = null;  //进度变化事件
        public event EventHandler<int> mStatusFinished = null;     //完成事件

        public CheerHtmlToPdfComponent()
        {
            InitializeComponent();
        }

        public CheerHtmlToPdfComponent(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }


        /// <summary>
        /// 启动任务
        /// </summary>
        /// <param name="iCheerPrintArgs"></param>
        public void StartTask(CheerPrintArgs iCheerPrintArgs)
        {
            //如果浏览器环境没有初始化
            if (!Gecko.Xpcom.IsInitialized)
            {
                CheerLib.LogWriter.Error("{0}.StartTask Faild,Gecko.Xpcom.IsInitialized==false", this.GetType().FullName);

                this.RaiseStatusFinished(501);

                return;
            }

            Gecko.GeckoPreferences.User["gfx.direct2d.disabled"] = true;

            //准备数据目录、路径
            var dateTimeStr = DateTime.Now.ToString("yyyyMMddHHmmss");
            var pid = Process.GetCurrentProcess().Id;

            this.mDataRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfData", string.Format("{0}_{1}", dateTimeStr, pid));

            if (!Directory.Exists(this.mDataRoot))
            {
                Directory.CreateDirectory(this.mDataRoot);
            }
            this.mHtmlPdfPath= Path.Combine(this.mDataRoot, "html_pdf.pdf");
            this.mPdfPath = Path.Combine(this.mDataRoot, "pdf_pdf.pdf");

            //外部参数
            this.mCheerPrintArgs = iCheerPrintArgs;

            
            //创建浏览器对象
            this.mWebBrowser = new GeckoWebBrowser();
            this.components.Add(this.mWebBrowser);
            this.mWebBrowser.Size = new Size(this.mCheerPrintArgs.htmlWindowWidth, this.mCheerPrintArgs.htmlWindowHeight);

            this.mWebBrowser.ConsoleMessage += MWebBrowser_ConsoleMessage;   //控制台消息
            this.mWebBrowser.DocumentCompleted += MWebBrowser_DocumentCompleted;   //当文档加载完成
            

            var htmlPath = this.mCheerPrintArgs.htmlInputFilePath;

            if (!File.Exists(htmlPath))
            {
                this.RaiseStatusChanged(0, "Loading Html File Not Exitsts!");

                this.RaiseStatusFinished(501);
                return;
            }

            var htmlContent = File.ReadAllText(htmlPath, Encoding.UTF8);

            this.mWebBrowser.LoadHtml(htmlContent);  //浏览器访问html文件地址

            this.RaiseStatusChanged(0, "Loading Html...");

            //下一步到浏览器文档加载完成
        }

        /// <summary>
        /// 启动PDF打印
        /// </summary>
        public void StartPdfPrint()
        {
            try
            {

                const double kMillimetersPerInch = 25.4;

                var mPrint = Xpcom.QueryInterface<nsIWebBrowserPrint>(this.mWebBrowser.Window.DomWindow);

                var printService = Xpcom.GetService<nsIPrintSettingsService>("@mozilla.org/gfx/printsettings-service;1");
                var printSettings = printService.GetGlobalPrintSettingsAttribute();

                //输出文件
                printSettings.SetOutputFormatAttribute(nsIPrintSettingsConsts.kOutputFormatPDF);  //文件格式:pdf
                printSettings.SetToFileNameAttribute(new nsAString(this.mHtmlPdfPath));
                printSettings.SetPrintToFileAttribute(true);



                //静默打印
                printSettings.SetPrintSilentAttribute(true);
                printSettings.SetShowPrintProgressAttribute(false);

                //页面大小单位
                printSettings.SetPaperSizeUnitAttribute(nsIPrintSettingsConsts.kPaperSizeMillimeters);  //单位:mm

                //页面高度
                printSettings.SetPaperHeightAttribute(this.mCheerPrintArgs.pageHeight);

                //页面宽度
                printSettings.SetPaperWidthAttribute(this.mCheerPrintArgs.pageWidth);

                //边距,单位：英寸,1英寸=25.4毫米
                printSettings.SetUnwriteableMarginTopAttribute(0d);
                printSettings.SetUnwriteableMarginBottomAttribute(0d);
                printSettings.SetUnwriteableMarginLeftAttribute(0d);
                printSettings.SetUnwriteableMarginRightAttribute(0d);

                //边距,单位：英寸,1英寸=25.4毫米
                printSettings.SetMarginTopAttribute(this.mCheerPrintArgs.marginTop / kMillimetersPerInch);
                printSettings.SetMarginBottomAttribute(this.mCheerPrintArgs.marginBottom / kMillimetersPerInch);
                printSettings.SetMarginLeftAttribute(this.mCheerPrintArgs.marginLeft / kMillimetersPerInch);
                printSettings.SetMarginRightAttribute(this.mCheerPrintArgs.marginRight / kMillimetersPerInch);

                printSettings.SetEdgeTopAttribute(0d);
                printSettings.SetEdgeBottomAttribute(0d);
                printSettings.SetEdgeLeftAttribute(0d);
                printSettings.SetEdgeRightAttribute(0d);


                //横竖,0竖向,1横向
                if (this.mCheerPrintArgs.portraitOrientation == 1)
                {
                    printSettings.SetOrientationAttribute(nsIPrintSettingsConsts.kPortraitOrientation);
                }
                else
                {
                    printSettings.SetOrientationAttribute(nsIPrintSettingsConsts.kLandscapeOrientation);
                }


                //页眉
                printSettings.SetHeaderStrCenterAttribute(new nsAString(string.Empty));
                printSettings.SetHeaderStrLeftAttribute(new nsAString(string.Empty));
                printSettings.SetHeaderStrRightAttribute(new nsAString(string.Empty));


                //页脚
                printSettings.SetFooterStrRightAttribute(new nsAString(string.Empty));
                printSettings.SetFooterStrLeftAttribute(new nsAString(string.Empty));
                printSettings.SetFooterStrCenterAttribute(new nsAString(string.Empty));


                //打印背景颜色
                printSettings.SetPrintBGColorsAttribute(true);

                //打印背景图
                printSettings.SetPrintBGImagesAttribute(true);

                //网页缩放适应页面
                printSettings.SetShrinkToFitAttribute(true);

                mPrint.Print(printSettings, this);

                Marshal.ReleaseComObject(mPrint);
            }
            catch (COMException ex)
            {
                //GeckoError
                var errorCode = string.Format("{0}", ex.ErrorCode);
                var errorMsg = CheerBrwError.getErrorMsg(errorCode);


                CheerLib.LogWriter.Log(ex.ToString());

                CheerLib.LogWriter.Error("Making PDF File Xpcom Error:{0}", errorMsg);

                var statusText = "Making PDF File Xpcom Error:" + errorMsg;

                this.RaiseStatusChanged(100,statusText);

                this.RaiseStatusFinished(502);

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
                var statusText = "Making PDF File Error!";
                this.RaiseStatusChanged(100,statusText);

                this.RaiseStatusFinished(503);
            }

            //下一步到打印回调函数
        }


        /// <summary>
        /// 启动PDF处理
        /// </summary>
        private void StartPdfProcess()
        {
            try
            {
                if (!File.Exists(this.mHtmlPdfPath))
                {
                    return;
                }


                if (PdfReader.TestPdfFile(this.mHtmlPdfPath) <= 0)
                {
                    CheerLib.LogWriter.Error("{0}.StartPdfProcess PdfReader.TestPdfFile Faild!", this.GetType().FullName);
                    return;
                }


                var htmlPdfFileInfo = new FileInfo(this.mHtmlPdfPath);

                CheerLib.LogWriter.Info("{0}.StartPdfProcess [{1}] Size={2}kb", this.GetType().FullName, this.mHtmlPdfPath, htmlPdfFileInfo.Length / 1024);

                File.Copy(this.mHtmlPdfPath, this.mPdfPath);

                var document = PdfReader.Open(this.mPdfPath, PdfDocumentOpenMode.Modify);

                var pageCount = document.PageCount;

                CheerLib.LogWriter.Error("{0}.StartPdfProcess pageCount={1}", this.GetType().FullName, pageCount);


                var fontName = string.Empty;

                //找本机上的字体列表
                var fontList = new InstalledFontCollection();
                foreach (var fontItem in fontList.Families)
                {
                    var fontItemName = fontItem.Name;
                    if (fontItemName.Contains("宋体"))
                    {
                        fontName = fontItemName;
                        break;
                    }
                }

                var font = new XFont(fontName, 9, XFontStyle.Regular);

                var format = new XStringFormat();
                format.Alignment = XStringAlignment.Center;
                format.LineAlignment = XLineAlignment.Far;

                double bottomHeight = this.mCheerPrintArgs.marginBottom;

                bottomHeight = -1 * bottomHeight;

                for (var i = 0; i < pageCount; ++i)
                {

                    var pageText = string.Format("第{0}页，共{1}页", i + 1,pageCount);

                    var pdfPage = document.Pages[i];

                    var pageW = pdfPage.Width;
                    var pageH = pdfPage.Height;

                    var gfx = XGraphics.FromPdfPage(pdfPage, XGraphicsPdfPageOptions.Prepend);

                    var rect = pdfPage.MediaBox.ToXRect();
                    rect.Inflate(0, bottomHeight);

                    gfx.DrawString(pageText, font, XBrushes.Black, rect, format); 
                }

                document.Save(this.mPdfPath);

                document.Close();

                //复制到指定路径
                File.Copy(this.mPdfPath, this.mCheerPrintArgs.pdfOutputFilePath, true);

                this.RaiseStatusFinished(200);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());

                this.RaiseStatusFinished(510);
            }
        }


        

        /// <summary>
        /// PDF打印进度变化回调
        /// </summary>
        /// <param name="aWebProgress"></param>
        /// <param name="aRequest"></param>
        /// <param name="aCurSelfProgress"></param>
        /// <param name="aMaxSelfProgress"></param>
        /// <param name="aCurTotalProgress"></param>
        /// <param name="aMaxTotalProgress"></param>
        public void OnProgressChange(nsIWebProgress aWebProgress, nsIRequest aRequest, int aCurSelfProgress, int aMaxSelfProgress, int aCurTotalProgress, int aMaxTotalProgress)
        {
            if (aMaxTotalProgress <= 0)
            {
                return;
            }

            var statusText = "Making PDF File...";
            this.RaiseStatusChanged(aCurTotalProgress,statusText);

        }
 
        /// <summary>
        /// PDF打印状态回调
        /// </summary>
        /// <param name="aWebProgress"></param>
        /// <param name="aRequest"></param>
        /// <param name="aStateFlags"></param>
        /// <param name="aStatus"></param>
        public void OnStateChange(nsIWebProgress aWebProgress, nsIRequest aRequest, uint aStateFlags, int aStatus)
        {
            var bFinished = ((aStateFlags & nsIWebProgressListenerConstants.STATE_STOP) != 0);

            if (!bFinished)
            {
                return;
            }

            //回调过不再回调
            if (this.brwPrintCheckTimer.Enabled)
            {
                return;
            }

            //启动定时器检查打印是否结束，下一步到定时器回调
            this.brwPrintCheckTimer.Enabled = true;

        }

        public void OnStatusChange(nsIWebProgress aWebProgress, nsIRequest aRequest, int aStatus, string aMessage)
        {
            return;
        }

        public void OnLocationChange(nsIWebProgress aWebProgress, nsIRequest aRequest, nsIURI aLocation, uint aFlags)
        {
            return;
        }

        public void OnSecurityChange(nsIWebProgress aWebProgress, nsIRequest aRequest, uint aState)
        {
            return;
        }


        /// <summary>
        /// 浏览器控制台消息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MWebBrowser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            CheerLib.LogWriter.Info("{0}.MWebBrowser_ConsoleMessage", this.GetType().FullName);
            CheerLib.LogWriter.Log(e.Message);
        }


        /// <summary>
        /// 浏览器文档加载完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MWebBrowser_DocumentCompleted(object sender, Gecko.Events.GeckoDocumentCompletedEventArgs e)
        {
            //启动文档检测定时器
            this.brwDocCheckTimer.Enabled = true;

            //下一步到文档加载定时器回调方法
        }

        /// <summary>
        /// 文档加载检测定时器方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void brwDocCheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (this.mWebBrowser.Document==null)
                {
                    return;
                }

                if (this.mWebBrowser.Document.ReadyState== "complete")
                {
                    this.brwDocCheckTimer.Enabled = false;

                    //启动打印
                    this.StartPdfPrint();       
                }

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }


        /// <summary>
        /// 打印检测定时器工作方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void brwPrintCheckTimer_Tick(object sender, EventArgs e)
        {
            //PDF文件被占用就一直没有结束
            if (this.isFileUsed(this.mHtmlPdfPath))
            {
                return;
            }
     
            this.brwPrintCheckTimer.Enabled = false;

            //开始PDF后期处理
            this.StartPdfProcess();

        }


        /// <summary>
        /// 触发状态变化事件
        /// </summary>
        /// <param name="processValue">进度值,0-100</param>
        /// <param name="statusText">状态信息</param>
        protected virtual void RaiseStatusChanged(int processValue, string statusText)
        {
            if (processValue < 0)
            {
                processValue = 0;
            }

            if (processValue > 100)
            {
                processValue = 100;
            }

            var xEvent = new PdfMakingStatus();
            xEvent.percentage = processValue;
            xEvent.statusText = statusText;

            CheerLib.LogWriter.Info("{0}.RaiseStatusChanged percentage={1},statusText={2}", this.GetType().FullName, xEvent.percentage, xEvent.statusText);

            var handler = this.mStatusChanged;

            if (handler == null)
            {
                return;
            }

            handler(this, xEvent);
        }

        protected virtual void RaiseStatusFinished(int resultCode)
        {
            CheerLib.LogWriter.Info("{0}.RaiseStatusFinished", this.GetType().FullName);

            this.RaiseStatusChanged(100, "Making PDF File Finished!");

            var handler = this.mStatusFinished;

            if (handler == null)
            {
                return;
            }

            handler(this, resultCode);
        }


        /// <summary>
        /// 判断文件是否占用
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool isFileUsed(string fileName)
        {
            var bRet = true;

            try
            {
                if (!File.Exists(fileName))
                {
                    return bRet;
                }

                var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);

                bRet = false;

                fs.Close();

            }
            catch (Exception ex)
            {
                bRet = true;

                CheerLib.LogWriter.Log(this.GetType().FullName + ".isFileUsed:" + ex.Message);
            }

            return bRet;
        }
    }
}
