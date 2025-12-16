using System.IO;
using System.Management;

namespace thecalcify_Update_Service
{
    public static class UpdateApiUrl
    {
        static string UserName()
        {
            string userName = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            foreach (ManagementBaseObject user in searcher.Get())
            {
                userName = user["UserName"].ToString().Split('\\')[1];
            }
            return userName;
        }

        public static string SetupVersionPath => "http://thecalcify.com/Setup/version.json";

        public static string LocalVersionPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "version.txt");

        public static string InstallationPath => @"C:\Program Files\thecalcify\thecalcify\thecalcify.exe";


        public static string TempPath => Path.Combine("C:\\Users", UserName(), "AppData\\Local\\Temp");
    }
}