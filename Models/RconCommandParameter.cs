using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TAWRcon_HLL_WPF.Models
{
    public class RconCommandParameter
    {
        public string Hint { get; private set; }

        public string Type { get; private set; }

        public bool Quoted { get; private set; }

        public bool Optional { get; private set; }

        public string GetterToUse { get; private set; }

        public RconCommandParameter(XElement parameterNode)
        {
            this.Hint = (string)parameterNode.Attribute((XName)"hint");
            this.Type = (string)parameterNode.Attribute((XName)"type");
            this.GetterToUse = (string)parameterNode.Attribute((XName)"usegetter");
            this.Quoted = (string)parameterNode.Attribute((XName)"quoted") == "true";
            this.Optional = (string)parameterNode.Attribute((XName)"optional") == "true";
        }

        public bool VerifyUserInput(string userInput)
        {
            if (string.IsNullOrEmpty(userInput) && !this.Optional)
                return false;
            string type = this.Type;
            if (type == "int")
            {
                int result;
                return int.TryParse(userInput, out result);
            }
            if (type == "bool" || type == "string")
                return true;
            if (type == "password")
                return !userInput.Any<char>(new Func<char, bool>(char.IsWhiteSpace));
            return false;
        }
    }
}
