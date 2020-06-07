using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingRCONCMD.Models
{
    public struct ServerConnectionDetails
    {
        //The IP address of the game server.
        public string ServerAddress { get; private set; }

        //The RCON port for the game server. 
        public int ServerPort { get; private set; }

        //The RCON password.
        public string ServerPassword { get; private set; }

        //Constructor for this model. 
        public ServerConnectionDetails(string serverAddress, int serverPort, string serverPassword)
        {
            ServerAddress = serverAddress;
            ServerPort = serverPort;
            ServerPassword = serverPassword;
        }
    }
}
