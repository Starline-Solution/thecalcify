using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace thecalcify_Update_Service
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
//#if DEBUG
//            // Run as console app for debugging
//            thecalcifyUpdate svc = new thecalcifyUpdate();
//            svc.StartDebug(args);

//            Console.WriteLine("Service running in DEBUG mode. Press ENTER to stop.");
//            Console.ReadLine();

//            svc.StopDebug();
//#else
            // Normal Windows Service startup
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                    new thecalcifyUpdate()
            };
            ServiceBase.Run(ServicesToRun);
//#endif
        }

    }
}
