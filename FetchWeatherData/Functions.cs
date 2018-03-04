using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Newtonsoft.Json;

namespace FetchWeatherData
{
    public class Functions
    {
        private static int number = 0;
        private static string URL = "http://api.openweathermap.org/data/2.5/weather?q=London,uk&APPID=5cedb909eecd599fe08212841796f86c";

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }

        [Singleton]
        public static void CronJob([TimerTrigger("*/5 * * * * *", RunOnStartup = true)] TimerInfo timerInfo)
        {
            Console.WriteLine("Timer job fired! " + number);
            GetJSON();
            number++;
        }
        
        private static void GetJSON()
        {
            Console.WriteLine("Fetching json");
            using(var webClient = new WebClient())
            {
                var jsonData = string.Empty;
                try
                {
                    jsonData = webClient.DownloadString(URL);
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                dynamic parsedJson = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
                //Console.WriteLine(jsonData);
                Console.WriteLine("\n" + parsedJson.wind.deg);
            }
        }

       
    }
}
