using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TestingRCONCMD.Helpers
{
    public class RconGetter
    {
        private string m_messageTemplate;

        public string Name { get; private set; }

        public string DisplayName { get; }

        public bool ShowInServerInfo
        {
            get
            {
                return !string.IsNullOrEmpty(this.DisplayName);
            }
        }

        public bool IsArray { get; }

        public bool AutoRefresh { get; }

        public RconGetter(XElement commandNode)
        {
            Name = (string)commandNode.Attribute((XName)"name");
            DisplayName = (string)commandNode.Attribute((XName)"showinserverinfoas");
            IsArray = HasAttribute(commandNode, "isarray");
            AutoRefresh = HasAttribute(commandNode, "autorefresh");
            m_messageTemplate = (string)commandNode.Attribute((XName)"messagetemplate");
        }

        public bool GetData(ServerSession serverSession, out string[] data)
        {
            data = new string[0];
            string receivedMessage = "";
            if (!serverSession.SendMessage(m_messageTemplate, true) || !serverSession.ReceiveMessage(out receivedMessage, true, false))
            {
                return false;
            }

            if (this.IsArray)
            {
                string[] strArray = Regex.Split(receivedMessage, "\t");
                int count = int.Parse(strArray[0]);
                data = ((IEnumerable<string>)strArray).Skip<string>(1).Take<string>(count).ToArray<string>();
            }
            else
                data = new string[1] { receivedMessage };
            return true;
        }

        private bool HasAttribute(XElement node, string attributename)
        {
            XAttribute xattribute = node.Attribute((XName)attributename);
            try
            {
                return xattribute != null && (bool)xattribute;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
