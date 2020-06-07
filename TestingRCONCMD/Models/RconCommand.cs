using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TestingRCONCMD.Helpers;

namespace TestingRCONCMD.Models
{

    //Model for RCON command
    public class RconCommand
    {
        //Stores attibutes for each command XML Node.
        private List<RconCommandParameter> m_parameters = new List<RconCommandParameter>();
        //keyword and structure for command.  eg. showlog {0} {1}
        private string m_messageTemplate;
        //Label for command. EG. "Get Player Info"
        public string m_name { get; private set; }

        //Constructor for Command. 
        public RconCommand(XElement commandNode)
        {
            m_name = (string)commandNode.Attribute((XName)"name");
            m_messageTemplate = (string)commandNode.Attribute((XName)"messagetemplate");
            foreach (XElement element in commandNode.Elements((XName)"Parameter")) // For each Paramter tag. 
                m_parameters.Add(new RconCommandParameter(element));
        }
        
        //Populate paramters into Command, and execute. 
        public void StartExecuting(List<RconCommandParameter> parameters, ServerSession serverSession)
        {
            List<string> stringList = new List<string>();
            if (m_parameters.Count > 0)
            {
             //   Dialogue paramterers: List<RconCommandParameter> parameters, ServerSession serverSession
             //   ParameterDialog parameterDialog = new ParameterDialog(m_parameters, serverSession);
            //  bool? nullable = parameterDialog.ShowDialog();
             bool flag = false;

             /*   if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                {
                    return;
                }*/


                foreach (RconCommandParameter parameter in parameters)
                {
                    //string s = parameterDialog.ParameterToUserInput[parameter].Text;
                    string s = parameter.value;
                    if (parameter.Quoted)
                    {
                        s = RconCommand.QuoteString(s);
                    }
                    Console.Write("Added " + s);
                    stringList.Add(s);
                }
            }
            Console.WriteLine(string.Format(m_messageTemplate, (object[])stringList.ToArray()));
            if (!serverSession.SendMessage(string.Format(m_messageTemplate, (object[])stringList.ToArray()), true))
            {
                Console.WriteLine("Failed,");
                return;
            }

            string receivedMessage = "";
            serverSession.ReceiveMessage(out receivedMessage, true, true);
        }
        //Add quote marks to string. 
        public static string QuoteString(string s)
        {
            string str = "\"";
            for (int index = 0; index < s.Length; ++index)
                str = s[index] != '"' ? (s[index] != '\\' ? str + s[index].ToString() : str + "\\\\") : str + "\\\"";
            return str + "\"";
        }

        public bool TryPopulateParamters(List<string> ParamsAsString, out List<RconCommandParameter> PopulatedParams)
        {
            try
            {
                //TODO: List parameter objects for command, parse params as strings into correct data types.
                List<RconCommandParameter> _tmp = new List<RconCommandParameter>();


                Console.WriteLine("Paramters found for " + m_name + ":  \n =============================================");
                var i = 0;
                foreach(var _par in m_parameters)
                {
                    Console.WriteLine(_par.Hint + ".  Optional: " + _par.Optional + ". Type: " + _par.Type + ". Quoted: " + _par.Quoted);
                    _par.value = ParamsAsString[i];
                    i++;

                    _tmp.Add(_par);

                }

                PopulatedParams = _tmp;
                return true;
            }catch(Exception ex)
            {
                PopulatedParams = null;
                Console.WriteLine(ex.Message);
                return false; 
            }
        }
    }
}
