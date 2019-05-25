using CheerPrintMaster.Service.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheerPrintMaster.Service
{
    public class MainTaskService : CheerLib.Service.MainTaskService
    {

        private PrintExcuteService mPrintExcuteService = CheerLib.ServiceHelper<PrintExcuteService>.getInstance();

        public override void showStatus()
        {
            this.mPrintExcuteService.showStatus();
        }

        public override void startService()
        {
            this.mPrintExcuteService.startService();
        }
    }
}
