using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheerPrintMaster
{
    class Program
    {
        static void Main(string[] args)
        {

            CheerLib.ServiceHelper<Service.MainTaskService>.getInstance().runService(args);
        }
    }
}
