using System;
using System.Diagnostics;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Set current process priority to RealTime
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log($"Unable to set process priority: {ex.Message}");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //// Check if launched from toast activation
            //if (args != null && args.Any(arg => arg.Contains("action=thecaclcifyNotification")))
            //{

            //    // App launched by toast click — open main form or do something special
            //    Application.Run(new thecalcify(args));  // Or any other form you want to show
            //}
            //else
            //{
            //    // Normal app startup
            Application.Run(new Login());
            //}
        }
    }
}
