using RconClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TAWRcon_HLL_WPF.Forms;
using TAWRcon_HLL_WPF.Models;


namespace TAWRcon_HLL_WPF.Models
{
    class RconCommand
    {
            private List<RconCommandParameter> m_parameters = new List<RconCommandParameter>();
            private string m_messageTemplate;

            public string m_name { get; private set; }

            public RconCommand(XElement commandNode)
            {
                this.m_name = (string)commandNode.Attribute((XName)"name");
                this.m_messageTemplate = (string)commandNode.Attribute((XName)"messagetemplate");
                foreach (XElement element in commandNode.Elements((XName)"Parameter"))
                    this.m_parameters.Add(new RconCommandParameter(element));
            }

            public void StartExecuting(ServerSession serverSession)
            {
                List<string> stringList = new List<string>();
                if (this.m_parameters.Count > 0)
                {
                    ParameterDialog parameterDialog = new ParameterDialog(this.m_parameters, serverSession);
                    bool? nullable = parameterDialog.ShowDialog();
                    bool flag = false;
                    if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) != 0)
                        return;
                    foreach (RconCommandParameter parameter in this.m_parameters)
                    {
                        string s = parameterDialog.ParameterToUserInput[parameter].Text;
                        if (parameter.Quoted)
                            s = RconCommand.QuoteString(s);
                        stringList.Add(s);
                    }
                }
                if (!serverSession.SendMessage(string.Format(this.m_messageTemplate, (object[])stringList.ToArray()), true))
                    return;
                string receivedMessage = "";
                serverSession.ReceiveMessage(out receivedMessage, true, true);
            }

            public static string QuoteString(string s)
            {
                string str = "\"";
                for (int index = 0; index < s.Length; ++index)
                    str = s[index] != '"' ? (s[index] != '\\' ? str + s[index].ToString() : str + "\\\\") : str + "\\\"";
                return str + "\"";
            }
        }
    }
