using Gecko;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheerPrintWorker.Model
{
    public static class CheerBrwError
    {

        /// <summary>
        /// 获取浏览器com组件错误信息
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <returns></returns>
        public static string getErrorMsg(string errorCode)
        {

            var sRet = string.Empty;

            try
            {
                var typeObj=typeof(GeckoError);

                foreach (var fi in typeObj.GetFields())
                {
                    var fName = fi.Name;
                    var fVal = string.Format("{0}",fi.GetValue(typeObj));

                    if (fVal==errorCode)
                    {
                        sRet = fName;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                CheerLib.LogWriter.Log(ex.ToString());
            }
            


            return sRet;
        }

    }
}
