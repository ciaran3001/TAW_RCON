using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCON_HLL_MVC.Models
{
    public class RconCommandParameter
    {
        //Hint text for parameter.. 
        public string Hint { get; private set; }
        //Type of data for the paramter.
        public string Type { get; private set; }
        //Does paramter have quotes. (string)
        public bool Quoted { get; private set; }

        //If this paramter is optional or not.
        public bool Optional { get; private set; }

        public string GetterToUse { get; private set; }

        //Gets data from Commands.xml
        public RconCommandParameter(XElement parameterNode)
        {

         /* Example XML Command:
         *     
            <Command name="Kick" messagetemplate="kick {0} {1}" >
              <Parameter hint="Player to kick" type="string" quoted="true" usegetter="PlayerNames" />
              <Parameter hint="Reason" type="string" quoted="true" optional="true" />
            </Command>
        */
            Hint = (string)parameterNode.Attribute((XName)"hint");  //eg: "Player to kick"
            Type = (string)parameterNode.Attribute((XName)"type"); //eg: string
            GetterToUse = (string)parameterNode.Attribute((XName)"usegetter"); //eg. PlayerNames  
            Quoted = (string)parameterNode.Attribute((XName)"quoted") == "true"; // eg. true
            Optional = (string)parameterNode.Attribute((XName)"optional") == "true"; // eg. true
        }

        //Return true if input is correct, false if input is incorrect. (Validation)
        public bool VerifyUserInput(string userInput)
        {
            if (string.IsNullOrEmpty(userInput) && !Optional)
            {
                return false;
            }

            if (Type == "int")
            {
                int result;
                return int.TryParse(userInput, out result);
            }
            if (Type == "bool" || Type == "string")
            {
                return true;
            }
            if (Type == "password")
            {
                return !userInput.Any<char>(new Func<char, bool>(char.IsWhiteSpace));
            }

            return false;
        }

        public string value { get; set; }
    }
}
