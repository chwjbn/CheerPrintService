using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace CheerPrintWorker.Model
{
    public class CheerPrintArgs:CheerLib.Model.JsonData
    {

        /// <summary>
        /// HTML渲染窗口宽度,单位px
        /// </summary>
        public int htmlWindowWidth = 1440;

        /// <summary>
        /// HTML渲染窗口高度,单位px
        /// </summary>
        public int htmlWindowHeight = 900;

        /// <summary>
        /// 输入Html文件路径
        /// </summary>
        public string htmlInputFilePath = string.Empty;


        /// <summary>
        /// 输出PDF文件路径
        /// </summary>
        public string pdfOutputFilePath = string.Empty;


        /// <summary>
        /// 竖向打印标志,1竖向,0横向
        /// </summary>
        public int portraitOrientation = 1;

        /// <summary>
        /// 页面高度,单位mm
        /// </summary>
        public double pageHeight = 297d;


        /// <summary>
        /// 页面宽度,单位mm
        /// </summary>
        public double pageWidth = 210d;


        /// <summary>
        /// 上页边距,单位mm
        /// </summary>
        public double marginTop = 0d;


        /// <summary>
        /// 下页边距,单位mm
        /// </summary>
        public double marginBottom = 0d;

        /// <summary>
        /// 左页边距,单位mm
        /// </summary>
        public double marginLeft = 0d;


        /// <summary>
        /// 右页边距,单位mm
        /// </summary>
        public double marginRight = 0d;


        /// <summary>
        /// 通过加载xml初始化参数
        /// </summary>
        /// <param name="xmlFilePath">xml文件路径</param>
        public void LoadFromXml(string xmlFilePath)
        {
            try
            {
                if (!File.Exists(xmlFilePath))
                {
                    CheerLib.LogWriter.Error("{0}.LoadFromXml File Not Exists={1}",this.GetType().FullName,xmlFilePath);
                    return;
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                this.htmlWindowWidth = int.Parse(xmlDoc.SelectSingleNode("print/html_window_width").InnerXml);
                this.htmlWindowHeight = int.Parse(xmlDoc.SelectSingleNode("print/html_window_height").InnerXml);

                this.htmlInputFilePath = xmlDoc.SelectSingleNode("print/input_html_path").InnerXml;
                this.pdfOutputFilePath = xmlDoc.SelectSingleNode("print/output_pdf_path").InnerXml;

                this.portraitOrientation = int.Parse(xmlDoc.SelectSingleNode("print/portrait").InnerXml);

                this.pageWidth = double.Parse(xmlDoc.SelectSingleNode("print/page_width").InnerXml);
                this.pageHeight = double.Parse(xmlDoc.SelectSingleNode("print/page_height").InnerXml);

                this.marginTop = double.Parse(xmlDoc.SelectSingleNode("print/margin_top").InnerXml);
                this.marginBottom = double.Parse(xmlDoc.SelectSingleNode("print/margin_bottom").InnerXml);
                this.marginLeft = double.Parse(xmlDoc.SelectSingleNode("print/margin_left").InnerXml);
                this.marginRight = double.Parse(xmlDoc.SelectSingleNode("print/margin_right").InnerXml);

            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
        }

    }
}
