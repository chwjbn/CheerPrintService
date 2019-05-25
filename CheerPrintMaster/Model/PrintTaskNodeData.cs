using CheerLib.Model;

namespace CheerPrintMaster.Model
{
    public class PrintTaskNodeData:JsonData
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public string id = string.Empty;

        /// <summary>
        /// 任务UUID
        /// </summary>
        public string uuid = string.Empty;

        /// <summary>
        /// html文件url地址
        /// </summary>
        public string html_file_url = string.Empty;

        /// <summary>
        /// 任务回调地址,POST http://xxxx?check=1&error_code=xxx&msg=xxx&data=base64(filecontent)
        /// check:1健康度检查,0正式提交数据
        /// error_code:200正常,400以上错误
        /// msg:提示信息
        /// data:当check=0且error_code=200时候表示PDF内容,base64编码
        /// </summary>
        public string task_callback_url = string.Empty;

        /// <summary>
        /// html渲染窗口宽度,单位px
        /// </summary>
        public int html_window_width = 0;

        /// <summary>
        /// html渲染窗口高度,单位px
        /// </summary>
        public int html_window_height = 0;


        /// <summary>
        /// 页面宽度
        /// </summary>
        public double page_width = 0;

        /// <summary>
        /// 页面高度
        /// </summary>
        public double page_height = 0;

        /// <summary>
        /// 页边距，顶部，单位毫米
        /// </summary>
        public double margin_top = 0;

        /// <summary>
        /// 页边距，底部，单位毫米
        /// </summary>
        public double margin_bottom = 0;

        /// <summary>
        /// 页边距，左部，单位毫米
        /// </summary>
        public double margin_left = 0;

        /// <summary>
        /// 页边距，右部，单位毫米
        /// </summary>
        public double margin_right = 0;


        /// <summary>
        /// 竖向打印标志,1竖向打印,0横向打印
        /// </summary>
        public int orientation_flag = 1;


        

    }
}
