using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TAWRcon_HLL_WPF.Models;

namespace TAWRcon_HLL_WPF.Helpers
{
    internal class RconStaticLibrary
    {
        private static string s_rconSuccessReply = "SUCCESS";
        private static string s_rconFailReply = "FAIL";
        private static string s_commandsFilename = "Commands.xml";
        private static List<RconCommand> s_rconCommands = new List<RconCommand>();
        private static List<RconGetter> s_rconGetters = new List<RconGetter>();

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
            XElement xelement = XElement.Load(RconStaticLibrary.s_commandsFilename);
            foreach (XElement element in xelement.Elements((XName)"Commands").Elements<XElement>())
                RconStaticLibrary.s_rconCommands.Add(new RconCommand(element));
            foreach (XElement element in xelement.Elements((XName)"Getters").Elements<XElement>())
                RconStaticLibrary.s_rconGetters.Add(new RconGetter(element));
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
