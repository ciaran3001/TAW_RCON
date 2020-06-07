using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestingRCONCMD.Models;

namespace TestingRCONCMD.Helpers
{
    class InputHelper
    {

        public InputHelper()
        {
            
        }

        //Try and identify command and parameters for RconCommand 
        public bool ProcessInput(string input, out List<string> parameters,out string command)
        {
            List<int> Spaces = new List<int>();
            List<string> Parameters = new List<string>();

            int startingPos = 0;
            for(int i = startingPos; i < input.Length; i++)
            {
                // if index is not the first or last char and the index char is a space
                if(input[i] == ' ' && i < input.Length && i != 0)
                {
                    Spaces.Add(i);
                }
            }
            Spaces.Add(input.Length);

            string _command = input.Substring(0, Spaces[0]);
            Console.WriteLine("command: " + _command);
            Console.WriteLine(Spaces.Count - 1 + " Paramters found.");
            
            for(var e = 0; e <= Spaces.Count; e++)
            {
                string paramTMP;
                if (e < Spaces.Count-1)
                {
                    paramTMP = input.Substring(Spaces[e], Spaces[e + 1]-Spaces[e]);
                    Console.WriteLine("Param: " + paramTMP);
                    Parameters.Add(paramTMP);
                }
             
            }
            command = _command;
            parameters = Parameters;
            return true; 
        }

        public bool FindRconCommand(out RconCommand FoundRconCommand, string command)
        {
            List<RconCommand> AvailableCommands = RconStaticLibrary.AvailableCommands;
            List<RconGetter> AvailableGetters = RconStaticLibrary.AvailableGetters;

            foreach(var _command in AvailableCommands)
            {
              //  Console.WriteLine("Does: " + command + " equal Rcon command: " + _command.m_name);
                if (_command.m_name.Equals(command)){
                    Console.Write("Found Rcon Command");
                    FoundRconCommand = _command;
                    return true;
                }
            }
            Console.Write("Not in command library.");
            FoundRconCommand = null;
            return false;
        }

        public bool FindRconGetter(out RconGetter FoundRconCommand, string getter)
        {
            List<RconGetter> AvailableGetters = RconStaticLibrary.AvailableGetters;

            foreach (var _getter in AvailableGetters)
            {
               // Console.WriteLine("Does: " + _getter + " equal Rcon getter: " + _getter.Name);
                if (_getter.Name.Equals(getter))
                {
                    Console.WriteLine("Found Getter.");
                    FoundRconCommand = _getter;
                    return true;
                }
            }
            Console.Write("Not in Getter Library.");
            FoundRconCommand = null;
            return false;
        }
    }
}
