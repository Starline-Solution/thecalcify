using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Threading;

namespace thecalcify_Update_Service
{
    [RunInstaller(true)]
    public partial class thecalcify_Update_Installer : Installer
    {
        private const string ServiceNameConst = "thecalcifyUpdate";

        public thecalcify_Update_Installer()
        {
            InitializeComponent();

            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem,
            };

            ServiceInstaller serviceInstaller = new ServiceInstaller
            {
                ServiceName = ServiceNameConst,
                DisplayName = "thecalcify Update Service",
                StartType = ServiceStartMode.Automatic,
                Description = "Service that handles automatic updates for thecalcify application.",
            };

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }

        public override void Uninstall(IDictionary savedState)
        {
            StopServiceIfRunning(ServiceNameConst);

            base.Uninstall(savedState);
        }

        private static void StopServiceIfRunning(string serviceName)
        {
            try
            {
                using (ServiceController sc = new ServiceController(serviceName))
                {
                    if (sc.Status != ServiceControllerStatus.Stopped &&
                        sc.Status != ServiceControllerStatus.StopPending)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Service does not exist → already uninstalled
            }
        }
    }
}
