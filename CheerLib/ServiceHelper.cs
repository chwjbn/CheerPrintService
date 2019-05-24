using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheerLib.Service;

namespace CheerLib
{
    public class ServiceHelper<T> where T : InterfaceService,new()
    {
        private static T mInstance = new T();
        public static T getInstance()
        {
            return mInstance;
        }
    }
}
