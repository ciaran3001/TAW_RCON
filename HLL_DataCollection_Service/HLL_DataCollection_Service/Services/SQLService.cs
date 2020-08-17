using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_DataCollection_Service.Services
{
    class SQLService
    {
        public  void CreateRecord()
        {
            Console.WriteLine("SQL ROW:");
        }

        public void StoreLogs(byte[] data)
        {
        
            try
            {
                string command;  // CHAT, TEAM KILL, KILL, CONNECTED, DISCONNECTED 
                DateTime myDateTime = DateTime.Now;
                string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                string DataAsString = Encoding.ASCII.GetString(data);
                string rowAsString;
                //Store entire log
                //Identify and store command

                int LineBreak = 0;
                int iterator = 0;
                int LastStart = 0;

                if (DataAsString.Equals("EMPTY"))
                {
                    return;
                }

                foreach (byte ch in data)
                {

                    if (ch == 10) //10 = line break
                    {
                        LastStart = LineBreak;
                        LineBreak = iterator;
                        Console.Write(ch + System.Text.Encoding.ASCII.GetString(new[] { ch }) + " ");

                        byte[] RowAsBytes = new byte[iterator];
                        for (var i = 0; i < iterator; i++)
                        {
                            RowAsBytes[i] = data[i];
                        }
                        rowAsString = Encoding.ASCII.GetString(RowAsBytes);

                        string query = "INSERT INTO [dbo].[AdminLogs]([Date],[AdminLog])";
                        query += "VALUES('"+ sqlFormattedDate  + "', '" + rowAsString+"')";

                        string connectionString = @"Data Source = VPS-ZAP65083-7\SQLEXPRESS; Initial Catalog = HLL_Logs; User ID = DataService; Password = Password2020";
                        SqlConnection myConnection = new SqlConnection(connectionString);
                        myConnection.Open();

                        SqlCommand myCommand = new SqlCommand(query, myConnection);

                        //myCommand.Parameters.AddWithValue("@DATE", "20200822");//sqlFormattedDate);
                        myCommand.Parameters.AddWithValue("@LOG", rowAsString);
                        // ... other parameters

                        myCommand.ExecuteNonQuery();

                    }
                    else
                    {
                        Console.Write(System.Text.Encoding.ASCII.GetString(new[] { ch }));
                    }

                    iterator++;
                }
            }catch(Exception e)
            {
                Console.WriteLine("SQL ERROR: " + e.Message);
            }

        }

        public void StoreResponse(byte[] response)      //string sConnectionString, string sSQL, params SqlParameter[] parameters
        {
                try
                {
                    List<string> entries = new List<string>();
                    int entryStart = 0;
                    int entryend;
                    int iterator = 0;
                    //Sort response into a list of strings
                    foreach (var b in response)
                    {
                        if (b == 10)
                        {
                            Console.WriteLine("ROW END");
                            entryend = iterator;
                            byte[] _entryTmp = new byte[entryend - entryStart];
                            int _iter = 0;
                            for (var e = entryStart; e < entryend; e++)
                            {
                                _entryTmp[_iter] = response[e];
                                _iter++;
                            }
                            entryStart = entryend;
                            entries.Add(Encoding.ASCII.GetString(_entryTmp));
                        }
                        iterator++;
                    }
                    //store each list entry into SQL
                    
                        foreach (var str in entries)
                        {
                            DateTime myDateTime = DateTime.Now;
                            string sqlFormattedDate = myDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            string query = "INSERT INTO [dbo].[AdminLogs]([Date],[AdminLog])";
                            query += "VALUES('" + sqlFormattedDate + "', '" + str + "')";
                        using (var newConnection = new SqlConnection(@"Data Source = VPS-ZAP65083-7\SQLEXPRESS; Initial Catalog = HLL_Logs; User ID = DataService; Password = Password2020"))
                        using (var newCommand = new SqlCommand(query, newConnection))
                            {
                                newCommand.CommandType = CommandType.Text;
                                newConnection.Open();
                                newCommand.ExecuteNonQuery();
                                newConnection.Close();
                            }
                        }
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    LoggingService.Log(e.Message);
                }
        }





    }
}
