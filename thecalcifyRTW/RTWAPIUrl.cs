using System;
using System.IO;

namespace thecalcifyRTW
{
    public static class RTWAPIUrl
    {
        // Deployment stage Endpoints
        public static string ProdUrl => "http://api.thecalcify.com/excel?user=thecalcify&auth=Starline@1008&type=Desktop";
        public static string LocalUrl => "http://192.168.3.84:1122/dev-excel?user=thecalcify&auth=Starline@1008&type=Desktop";
        public static string UATUrl => "http://35.176.5.121:1008/qa-excel?user=thecalcify&auth=Starline@1008&type=Desktop";
        public static string LocalMarketURL => "http://localhost:5000/market";

        public static string SharedConfigFilePath =>
         Path.Combine(
             Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
             "thecalcify",
             "rtw_config.json"
         );
    }
}
