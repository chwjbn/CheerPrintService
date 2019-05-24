using Gecko;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheerPrintWorker.Model
{
    public partial class CheerHtmlToPdfComponent : Component, nsIWebProgressListener
    {

        private GeckoWebBrowser mWebBrowser = null;

        private CheerPrintArgs mCheerPrintArgs = new CheerPrintArgs();

        private string mDataRoot = string.Empty;
        private string mHtmlPdfPath = string.Empty;
        private string mPdfPath = string.Empty;


        public event EventHandler<PdfMakingStatus> mStatusChanged = null;
        public event EventHandler mFinished = null;

        public CheerHtmlToPdfComponent()
        {
            InitializeComponent();
        }

        public CheerHtmlToPdfComponent(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }


        public void StartTask(CheerPrintArgs iCheerPrintArgs)
        {
            if (!Gecko.Xpcom.IsInitialized)
            {
                CheerLib.LogWriter.Error("{0}.StartTask Faild,Gecko.Xpcom.IsInitialized==false", this.GetType().FullName);
                return;
            }

            var dateTimeStr = DateTime.Now.ToString("yyyyMMddHHmmss");
            var pid = Process.GetCurrentProcess().Id;

            this.mDataRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfData", string.Format("{0}_{1}", dateTimeStr, pid));

            if (!Directory.Exists(this.mDataRoot))
            {
                Directory.CreateDirectory(this.mDataRoot);
            }

            this.mHtmlPdfPath= Path.Combine(this.mDataRoot, "html_pdf.pdf");

            this.mPdfPath = Path.Combine(this.mDataRoot, "pdf_pdf.pdf");


            this.mCheerPrintArgs = iCheerPrintArgs;

            Gecko.GeckoPreferences.User["gfx.direct2d.disabled"] = true;

            this.mWebBrowser = new GeckoWebBrowser();
            this.components.Add(this.mWebBrowser);

            this.mWebBrowser.ConsoleMessage += MWebBrowser_ConsoleMessage;
            this.mWebBrowser.DocumentCompleted += MWebBrowser_DocumentCompleted;

 
            this.mWebBrowser.Size = new Size(1920, 1320);

            var url = this.mCheerPrintArgs.htmlInputFilePath;

            this.mWebBrowser.Navigate(url);

            var xPdfMakingStatus = new PdfMakingStatus();
            xPdfMakingStatus.percentage = 0;
            xPdfMakingStatus.statusText = "Loading Html...";

            this.RaiseStatusChanged(xPdfMakingStatus);

        }

        public void StartPrint()
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


                var xPdfMakingStatus = new PdfMakingStatus();
                xPdfMakingStatus.percentage = 100;
                xPdfMakingStatus.statusText = "Making PDF File Xpcom Error:" + errorMsg;

                this.RaiseStatusChanged(xPdfMakingStatus);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());


                var xPdfMakingStatus = new PdfMakingStatus();
                xPdfMakingStatus.percentage = 100;
                xPdfMakingStatus.statusText = "Making PDF File Error!";

                this.RaiseStatusChanged(xPdfMakingStatus);

            }
        }

        public void OnLocationChange(nsIWebProgress aWebProgress, nsIRequest aRequest, nsIURI aLocation, uint aFlags)
        {

        }

        public void OnProgressChange(nsIWebProgress aWebProgress, nsIRequest aRequest, int aCurSelfProgress, int aMaxSelfProgress, int aCurTotalProgress, int aMaxTotalProgress)
        {
            if (aMaxTotalProgress <= 0)
            {
                return;
            }

            var xPdfMakingStatus = new PdfMakingStatus();
            xPdfMakingStatus.percentage = aCurTotalProgress;
            xPdfMakingStatus.statusText = "Making PDF File...";

            this.RaiseStatusChanged(xPdfMakingStatus);
        }

        public void OnSecurityChange(nsIWebProgress aWebProgress, nsIRequest aRequest, uint aState)
        {

        }

        public void OnStateChange(nsIWebProgress aWebProgress, nsIRequest aRequest, uint aStateFlags, int aStatus)
        {

            var bFinished = ((aStateFlags & nsIWebProgressListenerConstants.STATE_STOP) != 0);

            //CheerLib.LogWriter.Info("{0}.OnStateChange aStateFlags={1},aStatus={2}", this.GetType().FullName, aStateFlags, aStatus);

            if (!bFinished)
            {
                return;
            }

            if (this.brwPrintCheckTimer.Enabled)
            {
                return;
            }

            this.brwPrintCheckTimer.Enabled = true;

        }

        public void OnStatusChange(nsIWebProgress aWebProgress, nsIRequest aRequest, int aStatus, string aMessage)
        {

        }


        private void MWebBrowser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            CheerLib.LogWriter.Info("{0}.MWebBrowser_ConsoleMessage", this.GetType().FullName);
            CheerLib.LogWriter.Log(e.Message);
        }


        protected virtual void RaiseStatusChanged(PdfMakingStatus e)
        {
            CheerLib.LogWriter.Info("{0}.RaiseStatusChanged percentage={1},statusText={2}", this.GetType().FullName, e.percentage, e.statusText);

            var handler = this.mStatusChanged;

            if (handler == null)
            {
                return;
            }

            handler(this, e);
        }

        protected virtual void RaiseFinished()
        {

            CheerLib.LogWriter.Info("{0}.RaiseFinished", this.GetType().FullName);


            var xPdfMakingStatus = new PdfMakingStatus();
            xPdfMakingStatus.percentage = 100;
            xPdfMakingStatus.statusText = "Making PDF File Finished!";
            this.RaiseStatusChanged(xPdfMakingStatus);

            var handler = this.mFinished;

            if (handler == null)
            {
                return;
            }

            handler(this, EventArgs.Empty);
        }

        public class PdfMakingStatus : EventArgs
        {
            public int percentage;
            public string statusText;
        }


        private void MWebBrowser_DocumentCompleted(object sender, Gecko.Events.GeckoDocumentCompletedEventArgs e)
        {
            this.brwDocCheckTimer.Enabled = true;
        }

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
                    this.StartPrint();
                    this.brwDocCheckTimer.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 打印页码
        /// </summary>
        private void excutePageNumberPrint()
        {
            try
            {
                if (!File.Exists(this.mHtmlPdfPath))
                {
                    return;
                }


                if (PdfReader.TestPdfFile(this.mHtmlPdfPath) <=0)
                {
                    CheerLib.LogWriter.Error("{0}.excutePageNumberPrint PdfReader.TestPdfFile Faild!",this.GetType().FullName);
                    return;
                }


                var htmlPdfFileInfo = new FileInfo(this.mHtmlPdfPath);

                CheerLib.LogWriter.Info("{0}.excutePageNumberPrint [{1}] Size={2}kb", this.GetType().FullName,this.mHtmlPdfPath, htmlPdfFileInfo.Length/1024);

                File.Copy(this.mHtmlPdfPath, this.mPdfPath);

                var document = PdfReader.Open(this.mPdfPath, PdfDocumentOpenMode.Modify);

                var pageCount=document.PageCount;

                CheerLib.LogWriter.Error("{0}.excutePageNumberPrint pageCount={1}", this.GetType().FullName,pageCount);


                var fontName = string.Empty;

                //找本机上的字体列表
                var fontList = new InstalledFontCollection();
                foreach (var fontItem in fontList.Families)
                {
                    var fontItemName=fontItem.Name;
                    
                    //CheerLib.LogWriter.Log(fontItemName);

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

                for (var i=0;i<pageCount;++i)
                {

                    var pageText = string.Format("第{0}页",i+1);

                    var pdfPage=document.Pages[i];

                    var pageW=pdfPage.Width;
                    var pageH = pdfPage.Height;

                    var gfx = XGraphics.FromPdfPage(pdfPage, XGraphicsPdfPageOptions.Prepend);

                    var rect =pdfPage.MediaBox.ToXRect();
                    rect.Inflate(0, -10);

                    gfx.DrawString(pageText,font, XBrushes.Black, rect, format);
                }

                document.Save(this.mPdfPath);

                document.Close();

                //复制到指定路径
                File.Copy(this.mPdfPath, this.mCheerPrintArgs.pdfOutputFilePath,true);
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }


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

                CheerLib.LogWriter.Log(this.GetType().FullName+ ".isFileUsed:"+ex.Message);
            }

            return bRet;
        }

        private void brwPrintCheckTimer_Tick(object sender, EventArgs e)
        {
            if (this.isFileUsed(this.mHtmlPdfPath))
            {
                return;
            }

            this.brwPrintCheckTimer.Enabled = false;

            this.excutePageNumberPrint();

            this.RaiseFinished();
        }
    }
}
