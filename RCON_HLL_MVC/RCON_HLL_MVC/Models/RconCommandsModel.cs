using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RCON_HLL_MVC.Models
{
    public class RconCommandsModel
    {
        public List<RconCommand> FoundCommands { get; set; }

        public RconCommandsModel()
        {
            FoundCommands = RconStaticLibrary.s_rconCommands;
        }
    }
}