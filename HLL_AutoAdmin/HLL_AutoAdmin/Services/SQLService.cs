using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_AutoAdmin.Services
{
    class SQLService
    {
        string connectionString = @"Data Source = VPS-ZAP65083-7\SQLEXPRESS; Initial Catalog = HLL_Logs; User ID = DataService; Password = Password2020";

        public int IsAdminOn()
        {
            int _tmp = 0;
            var query = "SELECT TOP(1) Killed,COUNT(Killed) AS 'value_occurrence'FROM[HLL_Logs].[dbo].[HLLKills]where convert(varchar(10), DateTime, 102) = convert(varchar(10), getdate(), 102)GROUP BY[Killed]ORDER BY 'value_occurrence' DESC";
            DataTable results = await GetDataSetAsync(connectionString, query);
            string killed = " ";
            foreach (DataRow dr in results.Rows)
            {

                _tmp = Convert.ToInt32(dr["value_occurrence"]);
            }


            return _tmp;
        }
        public async Task<string> GetTopKilled()
        {
            var query = "SELECT TOP(1) Killed,COUNT(Killed) AS 'value_occurrence'FROM[HLL_Logs].[dbo].[HLLKills]where convert(varchar(10), DateTime, 102) = convert(varchar(10), getdate(), 102)GROUP BY[Killed]ORDER BY 'value_occurrence' DESC";
            DataTable results = await GetDataSetAsync(connectionString, query);
            string killed = " ";
            foreach (DataRow dr in results.Rows)
            {

                killed = dr["Killed"].ToString() + " has been killed " + Convert.ToInt32(dr["value_occurrence"]) + " times.";
            }

            if (killed.Equals(" "))
            {
                return "No one has been killed yet today.";
            }

            return killed;
        }

        public async Task<string> GetTopKiller()
        {
            var query = "SELECT TOP(1) Killer,COUNT(Killer) AS 'value_occurrence'FROM[HLL_Logs].[dbo].[HLLKills]where convert(varchar(10), DateTime, 102) = convert(varchar(10), getdate(), 102) AND TEAMKILL = 0GROUP BY[Killer]ORDER BY 'value_occurrence' DESC";
            DataTable results = await GetDataSetAsync(connectionString, query);
            string killer = " ";
            foreach (DataRow dr in results.Rows)
            {

                killer = dr["Killer"].ToString() + " has killed " + Convert.ToInt32(dr["value_occurrence"]) + " people today.";
            }

            if (killer.Equals(" "))
            {
                return "No one has been killed yet today.";
            }

            return killer;

        }

        public async Task<int> GetTotalTKs()
        {
            var query = "SELECT TeamKill,COUNT(TeamKill) AS 'value_occurrence'FROM[HLL_Logs].[dbo].[HLLKills]where TeamKill = 1 and convert(varchar(10), DateTime, 102) = convert(varchar(10), getdate(), 102)GROUP BY TeamKill";
            DataTable results = await GetDataSetAsync(connectionString, query);
            int total = 0;
            foreach (DataRow dr in results.Rows)
            {

                total = Convert.ToInt32(dr["value_occurrence"]);
            }

            return total;
        }

        public Task<DataTable> GetDataSetAsync(string sConnectionString, string sSQL)
        {
            return Task.Run(() =>
            {
                using (var newConnection = new SqlConnection(sConnectionString))
                using (var mySQLAdapter = new SqlDataAdapter(sSQL, newConnection))
                {
                    mySQLAdapter.SelectCommand.CommandType = CommandType.Text;

                    DataTable myDataSet = new DataTable();
                    mySQLAdapter.Fill(myDataSet);
                    return myDataSet;
                }
            });
        }

        public async Task<string> GetTodaysPunishments()
        {
            var query = "SELECT * FROM [HLL_Logs].[dbo].[AutoPunishments] WHERE convert(varchar(10), Date, 102) = convert(varchar(10), getdate(), 102)";
            DataTable results = await GetDataSetAsync(connectionString, query);
            string _tmp = " ";

            foreach (DataRow dr in results.Rows)
            {

                _tmp += (" \n :arrow_forward: " + dr["Player"].ToString() + " punished with a " + dr["Punishment"].ToString() + ". '" + dr["Comment"].ToString() + "'");
            }

            return _tmp;
        }
    }
}
