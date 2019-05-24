using CheerPrintWorker.Model;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheerPrintWorker
{
    public partial class MainForm : Form
    {

        private string[] mArgs = new string[] { };
        private CheerHtmlToPdfComponent mCheerHtmlToPdfComponent;   //HTML打印PDF组件

        public MainForm(string[] args)
        {
            this.mArgs = args;

            InitializeComponent();

            this.InitCheerHtmlToPdf();
        }


        /// <summary>
        /// 初始化打印组件
        /// </summary>
        private void InitCheerHtmlToPdf()
        {
            try
            {
                this.mCheerHtmlToPdfComponent = new CheerHtmlToPdfComponent();
                

                this.mCheerHtmlToPdfComponent.mFinished += MCheerHtmlToPdfComponent_mFinished;
                this.mCheerHtmlToPdfComponent.mStatusChanged += MCheerHtmlToPdfComponent_mStatusChanged;

    
                this.components.Add(this.mCheerHtmlToPdfComponent);  

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

        /// <summary>
        /// 组件状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MCheerHtmlToPdfComponent_mStatusChanged(object sender, CheerHtmlToPdfComponent.PdfMakingStatus e)
        {
            this.process_bar_main.Value = e.percentage;
            this.status_text_bar.Text = e.statusText;
        }

        /// <summary>
        /// 组件任务完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MCheerHtmlToPdfComponent_mFinished(object sender, EventArgs e)
        {
            this.Close();
        }

        

        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {

            if (this.mArgs==null)
            {
                this.status_text_bar.Text = "Invalid Run Args!";
                return;
            }

            if (this.mArgs.Length>1|| this.mArgs.Length < 1)
            {
                this.status_text_bar.Text = "Run Args Count Must Be 1!";
                return;
            }

            var xmlFile = this.mArgs[0];

            var xCheerPrintArgs = new CheerPrintArgs();

            xCheerPrintArgs.LoadFromXml(xmlFile);

            this.mCheerHtmlToPdfComponent.StartTask(xCheerPrintArgs);
        }
    }
}
