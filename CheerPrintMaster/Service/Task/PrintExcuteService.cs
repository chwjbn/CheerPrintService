using CheeLibExt.MessageQueue.Redis;
using CheerLib.Model;
using CheerPrintMaster.Model;
using System;
using System.Threading;

namespace CheerPrintMaster.Service.Task
{
    public class PrintExcuteService : CheerLib.Service.InterfaceService
    {

        public void showStatus()
        {
            
        }

        public void startService()
        {
            for (var i=0;i<=9;++i)
            {
                var iPrintTaskExcutor = new PrintTaskExcutor(string.Format("{0}",i));
                iPrintTaskExcutor.RunExcutor();
            }
        }

       
    }
}
