using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAWRcon_HLL_WPF.Models
{
    public struct ServerConnectionDetails
    {
        public string ServerAddress { get; private set; }

        public int ServerPort { get; private set; }

        public string ServerPassword { get; private set; }

        public ServerConnectionDetails(string serverAddress, int serverPort, string serverPassword)
        {
            this.ServerAddress = serverAddress;
            this.ServerPort = serverPort;
            this.ServerPassword = serverPassword;
        }
    }
}
