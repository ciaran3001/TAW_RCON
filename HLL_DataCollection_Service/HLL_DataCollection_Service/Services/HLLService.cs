using HLL_DataCollection_Service.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HLL_DataCollection_Service.Services
{


    class HLLService
    {

        private static int s_messageBufferSize = 8196;
        public static string s_rconLoginCommand = "login {0}";
        private bool m_authenticated;
        private bool m_statsAuthenticated;
        //private ServerConnectionDetails m_connectionDetails;  //Move to config file
      //  private Dictionary<RconGetter, string[]> m_cachedGetterData;
        private string m_statusMessage;
        private TcpClient m_client;
        private byte[] m_xorKey;
        private bool m_lastCommandSucceeded;
        public string StatusMessage;


        SQLService SQL = new SQLService();
        ServerConnectionDetails connectionDetails; 
        public bool Authenticated = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public static Mutex  m_communicationMutex = new Mutex();


        public HLLService()
        {

        }

        public bool Connect(string ip, int port, string password)
        {
            try
            {
                m_client = new TcpClient();
                TcpClient client = m_client;
                ServerConnectionDetails _connectionDetails = new ServerConnectionDetails(ip, port, password);
                string serverAddress = _connectionDetails.ServerAddress;
                int serverPort = _connectionDetails.ServerPort;

                connectionDetails = _connectionDetails;
                client.Connect(serverAddress, serverPort);
                Login();
                GetAdminLogs();
                return true;
            }catch(Exception e)
            {
                LoggingService.Log(e.Message);
                return false;
            }
        }
        public void GetAdminLogs()
        {

            try
            {
                Console.WriteLine("Getting admin logs");
                SendMessage(string.Format("showlog {0} {1}", 5, " ", true));
                string receivedMessage;
                m_lastCommandSucceeded = ReceiveMessage(out receivedMessage, true, true) && IsSuccessReply(receivedMessage);
            }
            catch (Exception e)
            {
                LoggingService.Log("Error getting admin logs - restarting service.");
                Program.StartService();
            }

           //Thread.Sleep(300000);
           // GetAdminLogs();
        }
        public void Login()
        {

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
                    
                    SendMessage(string.Format("login {0}", (object)QuoteString(connectionDetails.ServerPassword)), true);
                    string receivedMessage;
                    m_lastCommandSucceeded = ReceiveMessage(out receivedMessage, true, true) && IsSuccessReply(receivedMessage);
                    Authenticated = m_lastCommandSucceeded;

                    OnPropertyChanged("Disconnected");
                    OnPropertyChanged("Status");
                    if (Authenticated)
                    {
                        Console.WriteLine("Login successful.");
                        StatusMessage = "Login successful.";
                    }
                    else
                        Console.WriteLine("Login failed.");
                    StatusMessage = "Login failed. Is your password correct?";
                }

                //Call Get Admin Logs Function

            }
            catch (Exception ex)
            {
                //Simplified Error handling to decrease code size. 
                Console.WriteLine("Server Session Error: " + ex.Message);
                OnConnectionProblems("Server Session Error: " + ex.Message);
            }
        

        }
        public void RunGetters(string IP, int port, string PW)
        {

        }


        #region
        //TCP functions

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
            LoggingService.Log("Received: " + Encoding.Default.GetString(receivedBytes));
            SQL.StoreResponse(receivedBytes);

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
                    m_lastCommandSucceeded = false;
                }
                return false;
            }
            if (isCommand)
            {
                m_lastCommandSucceeded = IsSuccessReply(receivedMessage);
            }
            Console.WriteLine("Received: " + bytes);
            return bytes;
        }

        private bool ReceiveBytes(out byte[] receivedBytes, bool decrypted = true)
        {
            receivedBytes = new byte[s_messageBufferSize];
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
        #endregion

        public bool SendMessage(string message, bool encrypted = true)
        {
            return SendBytes(Encoding.UTF8.GetBytes(message), encrypted);
        }

        public static bool IsSuccessReply(string reply)
        {
            return reply.Equals("SUCCESS");
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

        public static string QuoteString(string s)
        {
            string str = "\"";
            for (int index = 0; index < s.Length; ++index)
                str = s[index] != '"' ? (s[index] != '\\' ? str + s[index].ToString() : str + "\\\\") : str + "\\\"";
            return str + "\"";
        }
    }
}
