using System;
using System.IO;
using System.Net;
using Microsoft.Azure.WebJobs;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace FetchWeatherData
{
    public class Functions
    {
        private static int runs = 0;
        private static string URL = "http://api.openweathermap.org/data/2.5/weather?q=Turku,fi&units=metric&APPID=5cedb909eecd599fe08212841796f86c";
        private static String connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }


        // listener starts for a particular TimerTrigger function, a blob lease (the Singleton Lock) is taken.
        // ensures that only a single instance of your scheduled function is running at any time. If you kill your console app,
        // that lease will still be held until it expires naturally. 
        // https://github.com/Azure/azure-webjobs-sdk-extensions/issues/25
        // In Azure, if the JobHost shuts down (e.g role restarts, etc.) graceful shutdown logic will release the lock immediately
        public static void GetAndSaveWeatherData([TimerTrigger("*/5 * * * * *", RunOnStartup = true)] TimerInfo timerInfo)
        {
            Console.WriteLine("Timer job fired! For the: " + runs + "th time");
            string temperature = GetTemperature();
            DateTime timestamp = DateTime.UtcNow;
            SaveData(temperature, timestamp);
            runs++;
        }

        private static void SaveData(string temperature, DateTime timestamp)
        {
            Console.WriteLine("Temp: " + temperature + ". Timestamp: " + timestamp);
            
            using (var connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"insert into Weather (Temperature, UpdateTime) values(@temperatureValue, @timestampValue)";
                    command.Parameters.Add("@temperatureValue", SqlDbType.Decimal).Value = temperature;
                    command.Parameters.Add("@timestampValue", SqlDbType.DateTime).Value = timestamp;    //SQL Timestamp deprecated
                    connection.Open();
                    command.ExecuteNonQuery();

                }
            }
        }

        private static string GetTemperature()
        {
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
                return parsedJson.main.temp;
               
            }
        }      

        
        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

    }
}
