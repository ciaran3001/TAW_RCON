using HLL_DataCollection_Service.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_DataCollection_Service
{
    static class Program
    {
        static string HLLIP = ConfigurationManager.AppSettings["HLLServerIp"];
        static int RCONPORT = Convert.ToInt32(ConfigurationManager.AppSettings["HLLRconPort"]);
        private static string RCONPassword = ConfigurationManager.AppSettings["HllPassword"];

        static void Main(string[] args)
        {
            StartService();


           /* LoggingService.Log("Getting Data From Server");
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Rcon.RunGetters(HLLIP, RCONPORT, RCONPassword);
            var ElapsedTime = watch.ElapsedMilliseconds;
            LoggingService.Log("Connected after " + ElapsedTime + "MS");
            */

        }

        public static void StartService()
        {
            Console.WriteLine("Starting RCON service");
            HLLService Rcon = new HLLService();
            try
            {
                Rcon.Connect(HLLIP, RCONPORT, RCONPassword);
               // Console.ReadLine();

            }
            catch (Exception e)
            {
                LoggingService.Log("Issue connecting: " + e.Message);

            }
        }
    }
}
