using System.IO;

namespace thecalcify_Update_Service
{
    public static class UpdateApiUrl
    {
        public static string SetupVersionPath => "http://thecalcify.com/Setup/version.json";

        public static string LocalVersionPath => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "version.txt");

        public static string InstallationPath => @"C:\Program Files\thecalcify\thecalcify\thecalcify.exe";
    }
}