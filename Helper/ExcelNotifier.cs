using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace thecalcify.Helper
{
    public class ExcelNotifier
    {
        public static void NotifyExcel(string symbol, Dictionary<string, object> dict)
        {
            try
            {
                bool pipeExists;
                try
                {
                    pipeExists = new DirectoryInfo(@"\\.\pipe\").GetFiles().Any(p => p.Name.Equals("theCalcifyPipe", StringComparison.OrdinalIgnoreCase));
                }
                catch (Exception)
                {
                    pipeExists = false;
                }

                if (pipeExists)
                {
                    using (var client = new NamedPipeClientStream(".", "theCalcifyPipe", PipeDirection.Out))
                    {
                        try
                        {
                            client.Connect(100); // wait few sec
                            using (var writer = new StreamWriter(client) { AutoFlush = true })
                            {

                                string fields = string.Join("|", dict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                                string msg = $"{symbol}|{fields}";

                                // Send message
                                writer.WriteLine(msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[App] Pipe error: " + ex.Message);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }
    }
}