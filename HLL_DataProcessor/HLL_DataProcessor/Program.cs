using HLL_DataProcessor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_DataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLService SQL = new SQLService();
            SQL.GetAndProcessLogs();
        }
    }
}
