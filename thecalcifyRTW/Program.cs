using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace thecalcifyRTW
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<MarketDataWorker>();

            // 👇 IMPORTANT
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "thecalcifyRTW";
            });

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
