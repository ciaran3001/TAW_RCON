using HLL_DataProcessor.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_DataProcessor.Services
{
    class SQLService
    {
        string getUnprocessedLogsQuery = @"SELECT  *FROM [HLL_Logs].[dbo].[AdminLogs]WHERE Processed IS NULL";
        List<Row> UnProcessedLogs = new List<Row>();


        public void GetAndProcessLogs()
        {
            
            //Get unprocessed Rows
            string connectionString = @"Data Source = VPS-ZAP65083-7\SQLEXPRESS; Initial Catalog = HLL_Logs; User ID = DataService; Password = Password2020";
            SqlConnection myConnection = new SqlConnection(connectionString);
            myConnection.Open();
            SqlCommand myCommand = new SqlCommand(getUnprocessedLogsQuery, myConnection);
            myCommand.ExecuteNonQuery();
            SqlDataAdapter da = new SqlDataAdapter(myCommand);

            DataTable dt = new DataTable();
            da.Fill(dt);
            myConnection.Close();
            //Fill unprocessed rows into List. 
            foreach (DataRow dr in dt.Rows)
            {
                Row _tmp = new Row();
                _tmp.Data = dr["AdminLog"].ToString();
                _tmp.ID = Convert.ToInt32(dr["ID"]);
                _tmp.error = false;
                _tmp.Logged = DateTime.Parse((dr["Date"].ToString()));

                UnProcessedLogs.Add(_tmp);
            }

            //Process Rows

            foreach (var row in UnProcessedLogs)
            {
                //Identify Log Type

                //KILL : 10, 15
                //TEAM KILL 10, 20
                //CHAT : 10,21
                //Disconnected: 10, 22
                //connected: 10,21
                string Command = row.Data; //.Substring(11, row.Data.Length - 11);

                //Process Data
                #region TEAMKILL   
                if (Command.Contains("TEAM KILL"))
                {
                    row.Command = "TEAM KILL";
                    row.TeamKill = true;
                    //
                    int arrowPosition = 0;
                    int withPosition = 0;
                    //Killer
                    for(int i = 0; i < row.Data.Length; i++)
                    {
                        if(row.Data[i] == '-' && row.Data[i+1] == '>')
                        {
                            arrowPosition = i;
                            row.Killer = row.Data.Substring(21, i - 21);
                        }
                    }

                    //Killed
                    for (int i = arrowPosition; i < row.Data.Length; i++)
                    {
                        if (row.Data[i] == 'w' && row.Data[i + 1] == 'i' && row.Data[i + 2] == 't' && row.Data[i + 3] == 'h')
                        {
                            withPosition = i;
                            row.Killed = row.Data.Substring(arrowPosition +2, i - (arrowPosition + 1) );
                        }
                    }

                    //Weapon
                    row.Weapon = row.Data.Substring(withPosition + 4, row.Data.Length - (withPosition + 4));


                }
                #endregion
                #region Kill
                else if (Command.Contains("KILL"))
                {
                    row.Command = "KILL";
                    row.TeamKill = false;
                    //
                    int arrowPosition = 0;
                    int withPosition = 0;
                    //Killer
                    for (int i = 0; i < row.Data.Length; i++)
                    {
                        if (row.Data[i] == '-' && row.Data[i + 1] == '>')
                        {
                            arrowPosition = i;
                            row.Killer = row.Data.Substring(21, i - 21);
                        }
                    }

                    //Killed
                    for (int i = arrowPosition; i < row.Data.Length; i++)
                    {
                        if (row.Data[i] == 'w' && row.Data[i + 1] == 'i' && row.Data[i + 2] == 't' && row.Data[i + 3] == 'h')
                        {
                            withPosition = i;
                            row.Killed = row.Data.Substring(arrowPosition + 2, i - (arrowPosition + 1));
                        }
                    }

                    //Weapon
                    row.Weapon = row.Data.Substring(withPosition + 4, row.Data.Length - (withPosition + 4));
                }
                #endregion
                #region chat
                else if (Command.Contains("CHAT"))
                {
                    row.Command = "CHAT";
                    int NameEnd = 0;

                    //player
                    try
                    {
                        for (int i = 0; i < row.Data.Length; i++)
                        {
                            if (row.Data[i] == ')' && row.Data[i + 1] == ']' && row.Data[i + 2] == ':')
                            {
                                NameEnd = i;
                                row.Player = row.Data.Substring(22, i - 22);
                            }
                        }
                    }catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        continue;
                    }

                    //Message
                    row.Message = row.Data.Substring(NameEnd + 2, row.Data.Length - (NameEnd + 2));

                }
                #endregion chat
                #region Disconnected
                else if (Command.Contains("DISCON"))
                {
                    row.Command = "DISCONNECTED";
                }
                #endregion
                #region connected
                else if (Command.Contains("CONNECTED"))
                {
                    row.Command = "CONNECTED";
                }
                #endregion



            }
            //Insert into SQL
            var iterator = 1;
            foreach (var r in UnProcessedLogs)
            {

                // Console.WriteLine(iterator + " / " + UnProcessedLogs.Count + " Processed.");
                Console.Write("\r{0} out of " + UnProcessedLogs.Count, iterator);
                r.SaveToSQL();
                iterator++;
                //Console.Clear();
            }
        }
        public void InsertIntoSQL(string query)
        {
            string connectionString = @"Data Source = VPS-ZAP65083-7\SQLEXPRESS; Initial Catalog = HLL_Logs; User ID = DataService; Password = Password2020";
            SqlConnection myConnection = new SqlConnection(connectionString);
            myConnection.Open();
            SqlCommand myCommand = new SqlCommand(query, myConnection);
            myCommand.ExecuteNonQuery();
            myConnection.Close();
        }

        public void UpdateAsProcessed(int id)
        {
            string query = " UPDATE [dbo].[AdminLogs] SET[Processed] = 1 WHERE ID = " + id + ";";
            string connectionString = @"Data Source = VPS-ZAP65083-7\SQLEXPRESS; Initial Catalog = HLL_Logs; User ID = DataService; Password = Password2020";
            SqlConnection myConnection = new SqlConnection(connectionString);
            myConnection.Open();
            SqlCommand myCommand = new SqlCommand(query, myConnection);
            myCommand.ExecuteNonQuery();
            myConnection.Close();
        }
    }
}
