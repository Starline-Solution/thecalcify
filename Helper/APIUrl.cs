using System;
using System.IO;

namespace thecalcify.Helper
{
    public static class APIUrl
    {
        // Deployment stage Endpoints
        public static string ProdUrl => "http://api.thecalcify.com/";
        public static string LocalUrl => "http://192.168.3.84:1122/";
        public static string UATUrl => "http://35.176.5.121:1008/";
        public static string LocalMarketURL => "http://localhost:5000/market";

        public static string ApplicationURL => $"{ProdUrl}";
        public static string SignalRConnection => $"excel?type=Desktop"; //UAT :- qa-excel?type=Desktop | Prod :- excel?type=Desktop | Local :- dev-excel?type=Desktop

        public static string RtwConfigPath =>
         Path.Combine(
             Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
             "thecalcify",
             "rtw_config.json"
         );

        public static string InstallationPath => @"C:\Program Files\thecalcify\thecalcify\thecalcify.exe";

        //public static string SystemTempPath =>
        //    Path.Combine(
        //    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        //    "SystemTemp"
        //);

        public static string TempPath => Path.GetTempPath();


    }
}
