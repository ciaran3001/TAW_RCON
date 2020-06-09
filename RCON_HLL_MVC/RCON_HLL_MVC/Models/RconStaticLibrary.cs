using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCON_HLL_MVC.Models
{
    public class RconStaticLibrary
    {
        //Static variables.
        private static string s_rconSuccessReply = "SUCCESS";
        private static string s_rconFailReply = "FAIL";
        private static string s_commandsFilename = "Commands.xml";
        private static string path = @"C:\inetpub\RCON\";

        public static List<RconCommand> s_rconCommands = new List<RconCommand>();
        public static List<RconGetter> s_rconGetters = new List<RconGetter>();

        public static List<RconCommand> AvailableCommands
        {
            get
            {
                return RconStaticLibrary.s_rconCommands;
            }
        }

        public static List<RconGetter> AvailableGetters
        {
            get
            {
                return RconStaticLibrary.s_rconGetters;
            }
        }

        public static void UpdateAvailableCommandsAndGetters()
        {
            //Load xml file.
            XElement xelement = XElement.Load(path+s_commandsFilename);

            //Get all command tags into Commands list. 
            foreach (XElement element in xelement.Elements((XName)"Commands").Elements<XElement>())
            {
                RconCommand tmp = new RconCommand(element);
                s_rconCommands.Add(tmp);
                Console.WriteLine("Added command: " + tmp.m_name);
            }

            //Get all Getters into getter list. 
            foreach (XElement element in xelement.Elements((XName)"Getters").Elements<XElement>())
            {
                s_rconGetters.Add(new RconGetter(element));
            }
        }

        public static RconGetter FindGetterByName(string name)
        {
            try
            {
                return RconStaticLibrary.s_rconGetters.Where<RconGetter>((Func<RconGetter, bool>)(getter => getter.Name.Equals(name))).First<RconGetter>();
            }
            catch (Exception ex)
            {
                return (RconGetter)null;
            }
        }

        public static bool IsSuccessReply(string reply)
        {
            return reply.Equals(RconStaticLibrary.s_rconSuccessReply);
        }

        public static bool IsFailReply(string reply)
        {
            return reply.Equals(RconStaticLibrary.s_rconFailReply);
        }
    }
}
