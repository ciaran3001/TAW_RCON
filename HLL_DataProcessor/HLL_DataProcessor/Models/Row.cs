using HLL_DataProcessor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_DataProcessor.Models
{
    class Row
    {
        public string Data { get; set; }
        public DateTime Logged { get; set;}
        public DateTime ActualTime { get; set; }
        public int ID { get; set; }
        public bool error { get; set; }
        public bool Processed { get; set; }
        public string Command { get; set; }


        //Data to come from processing:
        //Kill
        public string Killer { get; set; }
        public string Killed { get; set; }
        public string Weapon { get; set; }
        public bool TeamKill { get; set; }


        //Message
        public string Player { get; set; }
        public string Message { get; set; }

        //Joining Leaving
        //Player
        public int Direction { get; set; }


        public void SaveToSQL()
        {
            if(Command == null)
            {
                //Console.WriteLine("Command is null");
                return;
            }

            string query = " ";
            if (Command.Equals("TEAM KILL"))
            {
                //string query = "INSERT INTO [dbo].[AdminLogs]([Date],[AdminLog])";
                //query += "VALUES('" + sqlFormattedDate + "', '" + rowAsString + "')";

                query = "INSERT INTO [dbo].[HLLKills]([Killer],[Killed],[DateTime],[Weapon],[TeamKill])";
                query += "VALUES('"+ Killer +"','"+Killed + "','" + Logged+"','"+ Weapon+"',1);";
            }else if (Command.Equals("KILL"))
            {
                query = "INSERT INTO [dbo].[HLLKills]([Killer],[Killed],[DateTime],[Weapon],[TeamKill])";
                query += "VALUES('" + Killer + "','" + Killed + "','" + Logged + "','" + Weapon + "',0);";
            }
            else if (Command.Equals("CHAT"))
            {
                query = "INSERT INTO [dbo].[ChatLogs]([Player],[Message],[Time])";
                query += "VALUES('"+Player+"','"+Message+"','"+Logged+"')";
            }


            if(!query.Equals(" "))
            {
                SQLService SQL = new SQLService();
                SQL.InsertIntoSQL(query);
                SQL.UpdateAsProcessed(ID);
            }

        }
    }
}
