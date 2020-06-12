using RCON_HLL_MVC.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RCON_HLL_MVC.Helpers
{
    public class ServerSession : INotifyPropertyChanged
    {
        private static int s_messageBufferSize = 8196;
        public static string s_rconLoginCommand = "login {0}";
        private bool m_authenticated;
        private bool m_statsAuthenticated;
        private ServerConnectionDetails m_connectionDetails;
        private Dictionary<RconGetter, string[]> m_cachedGetterData;
        private string m_statusMessage;
        private TcpClient m_client;
        private byte[] m_xorKey;
        private bool m_lastCommandSucceeded;
        public static Mutex m_communicationMutex;

        //Getter , Setter for authenticated. 
        public bool Authenticated
        {
            get
            {
                return this.m_authenticated;
            }
            private set
            {
                m_authenticated = value;
                OnPropertyChanged(nameof(Authenticated));
            }
        }

        //Getter setter.  Re-Sets if value changes. 
        public bool StatsAuthenticated
        {
            get
            {
                return m_statsAuthenticated;
            }
            private set
            {
                m_statsAuthenticated = value;
                OnPropertyChanged(nameof(StatsAuthenticated));
            }
        }
        //Getter setter.  Re-Sets if value changes. 
        public ServerConnectionDetails ConnectionDetails
        {
            get
            {
                return m_connectionDetails;
            }
            private set
            {
                m_connectionDetails = value;
                OnPropertyChanged(nameof(ConnectionDetails));
            }
        }
        //Return a collection of server information objects.
        public ObservableCollection<ServerInformation> ServerInfo
        {
            get
            {
                return new ObservableCollection<ServerInformation>(m_cachedGetterData.Select<KeyValuePair<RconGetter, string[]>, ServerInformation>((Func<KeyValuePair<RconGetter, string[]>, ServerInformation>)(entry => new ServerInformation(entry.Key, entry.Value))).ToList<ServerInformation>());
            }
        }

        //Return false if connected, return true if connected.
        public bool Disconnected
        {
            get
            {
                if (m_client != null && m_client.Connected)
                {
                    return !Authenticated;
                }
                return true;
            }
        }

        //Return error if disconnected, OK if last command worked, and warning if last command failed. 
        public Status Status
        {
            get
            {
                if (Disconnected)
                {
                    return Status.Error;
                }
                return m_lastCommandSucceeded ? Status.Ok : Status.Warning;
            }
        }

        //Geter setter for StatusMessage
        public string StatusMessage
        {
            get
            {
                return m_statusMessage;
            }
            private set
            {
                m_statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ServerSession(ServerConnectionDetails serverConnectionDetails)
        {
            ConnectionDetails = serverConnectionDetails;
            m_cachedGetterData = new Dictionary<RconGetter, string[]>();
            m_communicationMutex = new Mutex();
            Connect();
        }

        ~ServerSession()
        {
            if (m_client == null)
                return;
            m_client.Close();
        }

        //Calls sendBytes method, returns result. (bool) Encodes message in UTF8 before send. 
        public bool SendMessage(string message, bool encrypted = true)
        {
            return SendBytes(Encoding.UTF8.GetBytes(message), encrypted);
        }

        private bool SendBytes(byte[] message, bool encrypted = true)
        {
            Console.WriteLine("Sending Bytes to server: " + Encoding.Default.GetString(message));
            //If connection times out (5 seconds) return false 
            if (!m_communicationMutex.WaitOne(5000))
            {
                return false;
            }

            try
            {
                //Get open TCP connection.
                NetworkStream stream = m_client.GetStream();
                //If encrypted, encrypt. 
                if (encrypted)
                {
                    message = XORMessage(message);
                }

                //Writes the message byte[] to the TCP connection.
                byte[] buffer = message;
                int length = message.Length;
                stream.Write(buffer, 0, length);
                return true;
            }
            catch (Exception ex)
            {
                //Return false if theres an error. 
                Console.WriteLine(ex.Message);
                OnConnectionProblems("Disconnected from the server.");
                m_communicationMutex.ReleaseMutex();

                return false;
            }
        }

        public bool ReceiveMessage(out string receivedMessage, bool decrypted = true, bool isCommand = true)
        {
            
            receivedMessage = "";
            byte[] receivedBytes;
            bool bytes = ReceiveBytes(out receivedBytes, decrypted);
            Console.WriteLine("Receiving Message: " + Encoding.Default.GetString(receivedBytes));
            if (!bytes)
            {
                return false;
            }

            try
            {
                receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
            }
            catch (DecoderFallbackException ex)
            {
                if (isCommand)
                {
                    StatusMessage = "Failed to decode server response.";
                    m_lastCommandSucceeded = false;
                    OnPropertyChanged("Status");
                }
                return false;
            }
            if (isCommand)
            {
                StatusMessage = receivedMessage;
                m_lastCommandSucceeded = RconStaticLibrary.IsSuccessReply(receivedMessage);
                OnPropertyChanged("Status");
            }
            Console.WriteLine("Received: " + bytes);
            return bytes;
        }

        private bool ReceiveBytes(out byte[] receivedBytes, bool decrypted = true)
        {
            receivedBytes = new byte[ServerSession.s_messageBufferSize];
            int newSize;
            try
            {
                newSize = m_client.GetStream().Read(receivedBytes, 0, receivedBytes.Length);
            }
            catch (Exception ex)
            {
                OnConnectionProblems("Disconnected from the server.");
                m_communicationMutex.ReleaseMutex();
                return false;
            }
            Array.Resize<byte>(ref receivedBytes, newSize);
            if (decrypted)
            {
                receivedBytes = XORMessage(receivedBytes);
            }
            m_communicationMutex.ReleaseMutex();
            return true;
        }

        //Encrypt message
        private byte[] XORMessage(byte[] message)
        {
            for (int index = 0; index < message.Length; ++index)
                message[index] ^= m_xorKey[index % m_xorKey.Length];
            return message;
        }

        private void OnConnectionProblems(string errorMessage)
        {
            StatusMessage = errorMessage;
            Authenticated = false;
            OnPropertyChanged("Disconnected");
            OnPropertyChanged("Status");
        }

        //Note
        public bool Connect()
        {
            try
            {
                m_client = new TcpClient();
                TcpClient client = m_client;
                ServerConnectionDetails connectionDetails = ConnectionDetails;
                string serverAddress = connectionDetails.ServerAddress;
                connectionDetails = ConnectionDetails;
                int serverPort = connectionDetails.ServerPort;
                client.ConnectAsync(serverAddress, serverPort).ContinueWith((Action<Task>)(t => OnConnected(t)));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ServerSession.Connect Error:: " + ex.Message);
                //Simplified Error handling to decrease code size. 
                return false;
                OnConnectionProblems("ServerSession.Connect Error: " + ex.Message);
            }
        }

        protected void OnConnected(Task connectionTask)
        {
            try
            {
                try
                {
                    connectionTask.Wait();
                }
                catch (AggregateException ex)
                {
                    ex.Handle((Func<Exception, bool>)(x =>
                    {
                        throw x;
                    }));
                }
            }
            catch (Exception ex)
            {
                //Simplified Error handling to decrease code size. 
                Console.WriteLine("ServerSession.OnConnected Error: " + ex.Message);
                OnConnectionProblems("ServerSession.OnConnected Error: " + ex.Message);
            }

            try
            {
                m_communicationMutex.WaitOne();
                if (!ReceiveBytes(out m_xorKey, false))
                {
                    Console.WriteLine("No response from the server. Are the address and port correct?");
                    StatusMessage = "No response from the server. Are the address and port correct?";
                }
                else
                {
                    SendMessage(string.Format(ServerSession.s_rconLoginCommand, (object)RconCommand.QuoteString(ConnectionDetails.ServerPassword)), true);
                    string receivedMessage;
                    m_lastCommandSucceeded = ReceiveMessage(out receivedMessage, true, true) && RconStaticLibrary.IsSuccessReply(receivedMessage);
                    Authenticated = m_lastCommandSucceeded;
                    OnPropertyChanged("Disconnected");
                    OnPropertyChanged("Status");
                    if (Authenticated)
                    {
                        Console.WriteLine("Login successful.");
                        StatusMessage = "Login successful.";
                        UpdateServerInfo();
                    }
                    else
                        Console.WriteLine("Login failed.");
                    StatusMessage = "Login failed. Is your password correct?";
                }
            }
            catch (Exception ex)
            {
                //Simplified Error handling to decrease code size. 
                Console.WriteLine("Server Session Error: " + ex.Message);
                OnConnectionProblems("Server Session Error: " + ex.Message);
            }
        }

        public async void UpdateServerInfo()
        {
            if (Disconnected)
            {
                return;
            }

            foreach (RconGetter rconGetter in RconStaticLibrary.AvailableGetters.Where<RconGetter>((Func<RconGetter, bool>)(getter => getter.AutoRefresh)).ToList<RconGetter>())
            {
                RconGetter getter = rconGetter;
                string[] data = new string[1] { "" };
                if (!await Task.Run<bool>((Func<bool>)(() => getter.GetData(this, out data))))
                    data = new string[1] { "Failed to get data." };
                else if (RconStaticLibrary.IsFailReply(data[0]))
                    data = new string[1]
                    {
            "Getter not supported by the server."
                    };
                if (m_cachedGetterData.ContainsKey(getter))
                    m_cachedGetterData[getter] = data;
                else
                    m_cachedGetterData.Add(getter, data);
            }
            this.OnPropertyChanged("ServerInfo");
        }

        public bool GetData(string getterName, out string[] data)
        {
            RconGetter index = m_cachedGetterData.Select<KeyValuePair<RconGetter, string[]>, RconGetter>((Func<KeyValuePair<RconGetter, string[]>, RconGetter>)(entry => entry.Key)).Where<RconGetter>((Func<RconGetter, bool>)(key => key.Name.Equals(getterName))).FirstOrDefault<RconGetter>();
            if (index == null)
            {
                data = new string[0];
                return RconStaticLibrary.FindGetterByName(getterName).GetData(this, out data);
            }
            data = m_cachedGetterData[index];
            return true;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }


            propertyChanged((object)this, new PropertyChangedEventArgs(name));

        }
    }
}
