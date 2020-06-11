using RCON_HLL_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RCON_HLL_MVC.Helpers
{
    public static class RconHelper
    {
        static string  ReceivedMessage;
        static ServerSession session;

        public static bool Setup()
        {
            try
            {
                //TODO: Move connection details to encrypted file. 
                ServerConnectionDetails servrDetails = new ServerConnectionDetails("176.57.168.232", 28316, "TAWadminJune06");  //IP,Port,PW

                Console.WriteLine("Attempting to open connection to: 176.57.168.232");
                ServerSession _session = new ServerSession(servrDetails);
                if (_session.Connect())
                {
                    Console.WriteLine("Connected.");
                    session = _session;
                }
                else
                {
                    Console.WriteLine("Unable to connect");
                }

                //Get Static library to get commands from xml
                RconStaticLibrary.UpdateAvailableCommandsAndGetters();

                Console.WriteLine("Commands Loaded");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Depricated in web app. 
        /// Only used in CLI. 
        /// </summary>
        public static void GetAndProcessInput()
        {
           /*
            Console.WriteLine("Please input a command:");
            string input = Console.ReadLine();
            List<string> parameters;
            string command;
            string[] GetterData;

            Console.WriteLine("You inputted: " + input);
            Console.WriteLine("Attempting to find RCon Command");

            InputHelper inputHelper = new InputHelper();
            inputHelper.ProcessInput(input, out parameters, out command);

            RconCommand _rconcommand = null;
            RconGetter _rcongetter = null;

            if (inputHelper.FindRconCommand(out _rconcommand, command))
            {
                Console.WriteLine("Found RconCommand.");
                List<RconCommandParameter> CommandParameters;
                if (_rconcommand.TryPopulateParamters(parameters, out CommandParameters))
                {
                    _rconcommand.StartExecuting(CommandParameters, session);
                }
                else
                {
                    Console.WriteLine("There as an issue with your paramters.");
                }

            }
            else if (inputHelper.FindRconGetter(out _rcongetter, command))
            {
                if (_rcongetter.GetData(session, out GetterData))
                {
                    Console.WriteLine("Success");
                }
                else
                {
                    Console.WriteLine("Failed to run Getter.");
                }
                Console.WriteLine("Returned data: ");

                var test = GetterData;
                foreach (var d in GetterData.ToList<string>())
                {
                    Console.Write(d);
                }
            }
            GetAndProcessInput();
            */
        }
    }
}