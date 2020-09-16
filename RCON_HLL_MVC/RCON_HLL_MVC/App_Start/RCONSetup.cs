using RCON_HLL_MVC.Helpers;
using RCON_HLL_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RCON_HLL_MVC.App_Start
{
    public static class RCONSetup
    {
       // public static ServerSession RCONSession;
        public static RconStaticLibrary library;

        public static void Setup()
        {

            /*ServerConnectionDetails servrDetails = new ServerConnectionDetails("176.57.168.232", 28316, "TAWadminJune06");  //IP,Port,PW
            ServerSession _session = new ServerSession(servrDetails);
            _session.Connect();
            RCONSession = _session;
            RconStaticLibrary.UpdateAvailableCommandsAndGetters();*/

         //   HLLService HLL = new HLLService();
        //    if (HLL.Connect("176.57.168.232", 28316, "PASSWORD REDACTED"))
          //  {    
                RconStaticLibrary.UpdateAvailableCommandsAndGetters();
//                HLLConnection = HLL;
        //    }
      //      else Setup();
            
        }
    }
}