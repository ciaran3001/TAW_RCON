using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RCON_HLL_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RCON_HLL_MVC.Helpers
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

        public bool ConvertJSONToCommand(string json, out RconCommand FoundCommand, out List<RconCommandParameter> Params)
        {
            JObject _CommandAsJson = JObject.Parse(json);
            JToken _CommandNameNode = _CommandAsJson["command"];
            string _CommandName = _CommandAsJson["command"].ToString();
            JToken ParamsAsJson = _CommandAsJson["parameters"];


            var ParamterResults = AllChildren(JObject.Parse(json))
           .First(c => c.Type == JTokenType.Array && c.Path.Contains("parameters"))
           .Children<JObject>();

            List<JSONParameter> Parameters = new List<JSONParameter>();

            /*Desired JSON Format
             * {
                "command" :"Kick",
                "paramters" : [
                    "param1","param2"
                ]
                ,"requestor" : "username"
            }
             */
            List<JProperty> dumpProperties = new List<JProperty>();
            foreach (JObject result in ParamterResults)
            {
                JSONParameter _tmp = new JSONParameter();
                
                foreach (JProperty property in result.Properties())
                {                 
                    // do something with the property belonging to result
                    if(property.Name == "Hint")
                    {
                        _tmp.Hint = property.Value.ToString();
                    }else if (property.Name == "Value")
                    {
                        _tmp.Value = property.Value.ToString();
                    }
                    else if (property.Name == "Type")
                    {
                        _tmp.Type = property.Value.ToString();
                    }

                    dumpProperties.Add(property);
                }
                Parameters.Add(_tmp);
            }

            foreach (var cmd in RconStaticLibrary.AvailableCommands)
            {
                if(cmd.m_name == _CommandName)
                {
                    FoundCommand = cmd;
                    if(cmd.TryPopulateParamters(Parameters, out Params))
                    {
                        return true;
                    }
                }
            }
            //No Command Found
            FoundCommand = null;
            Params = null;
            return false;



        }

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }
    }
}
