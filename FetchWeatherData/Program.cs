using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Files;
using Microsoft.Azure.WebJobs.Extensions.Timers;

namespace FetchWeatherData
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                // https://github.com/Azure/azure-webjobs-sdk/wiki/Running-Locally
                config.UseDevelopmentSettings();
            }

            config.Tracing.ConsoleLevel = TraceLevel.Verbose;
            try
            {
                config.Singleton.ListenerLockPeriod = TimeSpan.FromSeconds(15);
                config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(2);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            config.UseTimers();
            var host = new JobHost(config);            
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();

        }


        
    }
}
