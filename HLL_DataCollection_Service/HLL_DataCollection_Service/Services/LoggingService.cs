using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_DataCollection_Service.Services
{
    static class LoggingService
    {
        public static void Log(string msg)
        {
            //TODO: Write to today's log file.
                DateTime timestamp = DateTime.Now;
                String log_item = (timestamp + ": " + msg);

                String todays_file = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + ".txt";

                //TODO: differant file per day.
                // eg name: yyyy.mm.dd.txt

                string path = @"./Logs/" + todays_file;
                if (!File.Exists(path))
                {
                    File.Create(path);
                    using (var tw = new StreamWriter(path, true))
                    {
                        tw.Write(log_item);
                        tw.Close();
                    }
                }
                else if (File.Exists(path))
                {
                    using (var tw = new StreamWriter(path, true))
                    {
                        tw.WriteLine(log_item);
                        tw.Close();
                    }
                }
            
        }
    }
}
