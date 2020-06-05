using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TAWRcon_HLL_WPF.Helpers;

namespace TAWRcon_HLL_WPF.Models
{
    public class ServerSession : INotifyPropertyChanged
    {
        private static int s_messageBufferSize = 8196;
        private static string s_rconLoginCommand = "login {0}";
        private bool m_authenticated;
        private bool m_statsAuthenticated;
        private ServerConnectionDetails m_connectionDetails;
        private Dictionary<RconGetter, string[]> m_cachedGetterData;
        private string m_statusMessage;
        private TcpClient m_client;
        private byte[] m_xorKey;
        private bool m_lastCommandSucceeded;
        private Mutex m_communicationMutex;

        public bool Authenticated
        {
            get
            {
                return this.m_authenticated;
            }
            private set
            {
                this.m_authenticated = value;
                this.OnPropertyChanged(nameof(Authenticated));
            }
        }

        public bool StatsAuthenticated
        {
            get
            {
                return this.m_statsAuthenticated;
            }
            private set
            {
                this.m_statsAuthenticated = value;
                this.OnPropertyChanged(nameof(StatsAuthenticated));
            }
        }

        public ServerConnectionDetails ConnectionDetails
        {
            get
            {
                return this.m_connectionDetails;
            }
            private set
            {
                this.m_connectionDetails = value;
                this.OnPropertyChanged(nameof(ConnectionDetails));
            }
        }

        public ObservableCollection<ServerInformation> ServerInfo
        {
            get
            {
                return new ObservableCollection<ServerInformation>(this.m_cachedGetterData.Select<KeyValuePair<RconGetter, string[]>, ServerInformation>((Func<KeyValuePair<RconGetter, string[]>, ServerInformation>)(entry => new ServerInformation(entry.Key, entry.Value))).ToList<ServerInformation>());
            }
        }

        public bool Disconnected
        {
            get
            {
                if (this.m_client != null && this.m_client.Connected)
                    return !this.Authenticated;
                return true;
            }
        }

        public Status Status
        {
            get
            {
                if (this.Disconnected)
                    return Status.Error;
                return this.m_lastCommandSucceeded ? Status.Ok : Status.Warning;
            }
        }

        public string StatusMessage
        {
            get
            {
                return this.m_statusMessage;
            }
            private set
            {
                this.m_statusMessage = value;
                this.OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ServerSession(ServerConnectionDetails serverConnectionDetails)
        {
            this.ConnectionDetails = serverConnectionDetails;
            this.m_cachedGetterData = new Dictionary<RconGetter, string[]>();
            this.m_communicationMutex = new Mutex();
            this.Connect();
        }

        ~ServerSession()
        {
            if (this.m_client == null)
                return;
            this.m_client.Close();
        }

        public bool SendMessage(string message, bool encrypted = true)
        {
            return this.SendBytes(Encoding.UTF8.GetBytes(message), encrypted);
        }

        private bool SendBytes(byte[] message, bool encrypted = true)
        {
            if (!this.m_communicationMutex.WaitOne(5000))
                return false;
            try
            {
                NetworkStream stream = this.m_client.GetStream();
                if (encrypted)
                    message = this.XORMessage(message);
                byte[] buffer = message;
                int length = message.Length;
                stream.Write(buffer, 0, length);
                return true;
            }
            catch (Exception ex)
            {
                this.OnConnectionProblems("Disconnected from the server.");
                this.m_communicationMutex.ReleaseMutex();
                return false;
            }
        }

        public bool ReceiveMessage(out string receivedMessage, bool decrypted = true, bool isCommand = true)
        {
            receivedMessage = "";
            byte[] receivedBytes;
            bool bytes = this.ReceiveBytes(out receivedBytes, decrypted);
            if (!bytes)
                return false;
            try
            {
                receivedMessage = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);
            }
            catch (DecoderFallbackException ex)
            {
                if (isCommand)
                {
                    this.StatusMessage = "Failed to decode server response.";
                    this.m_lastCommandSucceeded = false;
                    this.OnPropertyChanged("Status");
                }
                return false;
            }
            if (isCommand)
            {
                this.StatusMessage = receivedMessage;
                this.m_lastCommandSucceeded = RconStaticLibrary.IsSuccessReply(receivedMessage);
                this.OnPropertyChanged("Status");
            }
            return bytes;
        }

        private bool ReceiveBytes(out byte[] receivedBytes, bool decrypted = true)
        {
            receivedBytes = new byte[ServerSession.s_messageBufferSize];
            int newSize;
            try
            {
                newSize = this.m_client.GetStream().Read(receivedBytes, 0, receivedBytes.Length);
            }
            catch (Exception ex)
            {
                this.OnConnectionProblems("Disconnected from the server.");
                this.m_communicationMutex.ReleaseMutex();
                return false;
            }
            Array.Resize<byte>(ref receivedBytes, newSize);
            if (decrypted)
                receivedBytes = this.XORMessage(receivedBytes);
            this.m_communicationMutex.ReleaseMutex();
            return true;
        }

        private byte[] XORMessage(byte[] message)
        {
            for (int index = 0; index < message.Length; ++index)
                message[index] ^= this.m_xorKey[index % this.m_xorKey.Length];
            return message;
        }

        private void OnConnectionProblems(string errorMessage)
        {
            this.StatusMessage = errorMessage;
            this.Authenticated = false;
            this.OnPropertyChanged("Disconnected");
            this.OnPropertyChanged("Status");
        }

        public void Connect()
        {
            try
            {
                this.m_client = new TcpClient();
                TcpClient client = this.m_client;
                ServerConnectionDetails connectionDetails = this.ConnectionDetails;
                string serverAddress = connectionDetails.ServerAddress;
                connectionDetails = this.ConnectionDetails;
                int serverPort = connectionDetails.ServerPort;
                client.ConnectAsync(serverAddress, serverPort).ContinueWith((Action<Task>)(t => this.OnConnected(t)));
            }
            catch (ArgumentNullException ex)
            {
                this.OnConnectionProblems("Provided hostname is null.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                this.OnConnectionProblems("Invalid portnumber provided.");
            }
            catch (SocketException ex)
            {
                this.OnConnectionProblems("The provided address and port are inaccessible.");
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
            catch (ArgumentNullException ex)
            {
                this.OnConnectionProblems("Provided hostname is null.");
                return;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                this.OnConnectionProblems("Invalid portnumber provided.");
                return;
            }
            catch (SocketException ex)
            {
                this.OnConnectionProblems("The provided address and port are inaccessible.");
                return;
            }
            try
            {
                this.m_communicationMutex.WaitOne();
                if (!this.ReceiveBytes(out this.m_xorKey, false))
                {
                    this.StatusMessage = "No response from the server. Are the address and port correct?";
                }
                else
                {
                    this.SendMessage(string.Format(ServerSession.s_rconLoginCommand, (object)RconCommand.QuoteString(this.ConnectionDetails.ServerPassword)), true);
                    string receivedMessage;
                    this.m_lastCommandSucceeded = this.ReceiveMessage(out receivedMessage, true, true) && RconStaticLibrary.IsSuccessReply(receivedMessage);
                    this.Authenticated = this.m_lastCommandSucceeded;
                    this.OnPropertyChanged("Disconnected");
                    this.OnPropertyChanged("Status");
                    if (this.Authenticated)
                    {
                        this.StatusMessage = "Login successful.";
                        this.UpdateServerInfo();
                    }
                    else
                        this.StatusMessage = "Login failed. Is your password correct?";
                }
            }
            catch (ObjectDisposedException ex)
            {
                this.OnConnectionProblems("Internal error: mutex has been disposed.");
            }
            catch (AbandonedMutexException ex)
            {
                this.OnConnectionProblems("Internal error: mutex has been abandoned.");
            }
            catch (InvalidOperationException ex)
            {
                this.OnConnectionProblems("Internal error: invalid mutex operation.");
            }
            catch (ArgumentNullException ex)
            {
                this.OnConnectionProblems("Empty login string provided.");
                this.m_communicationMutex.ReleaseMutex();
            }
            catch (FormatException ex)
            {
                this.OnConnectionProblems("Invalid login string provided.");
                this.m_communicationMutex.ReleaseMutex();
            }
        }

        public async void UpdateServerInfo()
        {
            if (this.Disconnected)
                return;
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
                if (this.m_cachedGetterData.ContainsKey(getter))
                    this.m_cachedGetterData[getter] = data;
                else
                    this.m_cachedGetterData.Add(getter, data);
            }
            this.OnPropertyChanged("ServerInfo");
        }

        public bool GetData(string getterName, out string[] data)
        {
            RconGetter index = this.m_cachedGetterData.Select<KeyValuePair<RconGetter, string[]>, RconGetter>((Func<KeyValuePair<RconGetter, string[]>, RconGetter>)(entry => entry.Key)).Where<RconGetter>((Func<RconGetter, bool>)(key => key.Name.Equals(getterName))).FirstOrDefault<RconGetter>();
            if (index == null)
            {
                data = new string[0];
                return RconStaticLibrary.FindGetterByName(getterName).GetData(this, out data);
            }
            data = this.m_cachedGetterData[index];
            return true;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object)this, new PropertyChangedEventArgs(name));
        }

    }
}