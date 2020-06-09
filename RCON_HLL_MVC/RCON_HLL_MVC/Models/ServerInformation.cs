namespace RCON_HLL_MVC.Models
{
    public struct ServerInformation
    {
        private RconGetter getter;
        private string[] data;

        public ServerInformation(RconGetter rconGetter, string[] dataArray)
        {
            getter = rconGetter;
            data = dataArray;
        }

        public string Name
        {
            get
            {
                return getter.DisplayName;
            }
        }

        public string Data
        {
            get
            {
                if (!getter.IsArray || data.Length <= 1)
                {
                    return data[0];
                }

                string str1 = "";
                foreach (string str2 in this.data)
                {
                    str1 = str1 + str2 + "\n";
                }
                return str1;
            }
        }
    }
}
