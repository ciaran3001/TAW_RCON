using RCON_HLL_MVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;

namespace RCON_HLL_MVC.Helpers
{
    public static class HLLService
    {
        private static int s_messageBufferSize = 8196;
        public static string s_rconLoginCommand = "login {0}";
        private static bool m_authenticated;
        private static bool m_statsAuthenticated;
        //private ServerConnectionDetails m_connectionDetails;  //Move to config file
        //  private Dictionary<RconGetter, string[]> m_cachedGetterData;
        private static string m_statusMessage;
        private static TcpClient m_client;
        private static byte[] m_xorKey;
        private static bool m_lastCommandSucceeded;
        public static string StatusMessage;
        static string _trace = "";


        static ServerConnectionDetails connectionDetails;
        public static bool Authenticated = false;

        public static event PropertyChangedEventHandler PropertyChanged;
        public static Mutex m_communicationMutex = new Mutex();


        static HLLService()
        {

        }

        public static bool Connect(string ip, int port, string password, out string tracing)
        {
            try
            {
                _trace = "";
                m_client = new TcpClient();
                TcpClient client = m_client;
                ServerConnectionDetails _connectionDetails = new ServerConnectionDetails(ip, port, password);
                string serverAddress = _connectionDetails.ServerAddress;
                int serverPort = _connectionDetails.ServerPort;

                connectionDetails = _connectionDetails;
                client.Connect(serverAddress, serverPort);
                _trace += "Connected: " + client.Connected + "; ";
                Login();
                // GetAdminLogs();

                tracing = _trace;
                return true;
            }
            catch (Exception e)
            {
                tracing = _trace;
                return false;
                
            }
        }
        public static void GetAdminLogs()
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
              //  Program.StartService(); TODO: Restart connection
            }

            //Thread.Sleep(300000);
            // GetAdminLogs();
        }
        public static void Login()
        {

            try
            {
                m_communicationMutex.WaitOne();
                if (!ReceiveBytes(out m_xorKey, false))
                {
                    Console.WriteLine("No response from the server. Are the address and port correct?");
                    StatusMessage = "No response from the server. Are the address and port correct?";
                    _trace += "No response from the server. Are the address and port correct?; ";
                }
                else
                {

                    SendMessage(string.Format("login {0}", (object)QuoteString(connectionDetails.ServerPassword)), true);

                    string receivedMessage;
                    m_lastCommandSucceeded = ReceiveMessage(out receivedMessage, true, true) && IsSuccessReply(receivedMessage);
                    Authenticated = m_lastCommandSucceeded;

                    if (Authenticated)
                    {
                        Console.WriteLine("Login successful.");
                        StatusMessage = "Login successful.";
                        _trace += "Login successful.; ";
                    }
                    else
                    {
                        Console.WriteLine("Login failed.");
                        _trace += "Login failed.";
                        StatusMessage = "Login failed. Is your password correct?";
                    }
                }

                //Call Get Admin Logs Function

            }
            catch (Exception ex)
            {
                //Simplified Error handling to decrease code size. 
                Console.WriteLine("Server Session Error: " + ex.Message);
                OnConnectionProblems("Server Session Error: " + ex.Message);
                _trace += "ERROR: "+ ex.Message + "; ";
            }


        }
        public static void RunGetters(string IP, int port, string PW)
        {

        }


        #region
        //TCP functions

        private static bool SendBytes(byte[] message, bool encrypted = true)
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

        public static bool ReceiveMessage(out string receivedMessage, bool decrypted = true, bool isCommand = true)
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

        private static bool ReceiveBytes(out byte[] receivedBytes, bool decrypted = true)
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

        private static byte[] XORMessage(byte[] message)
        {
            for (int index = 0; index < message.Length; ++index)
                message[index] ^= m_xorKey[index % m_xorKey.Length];
            return message;
        }

        private static void OnConnectionProblems(string errorMessage)
        {
            _trace += errorMessage;
            StatusMessage = errorMessage;
            Authenticated = false;
        }
        #endregion

        public static bool SendMessage(string message, bool encrypted = true)
        {
            return SendBytes(Encoding.UTF8.GetBytes(message), encrypted);
        }

        public static bool IsSuccessReply(string reply)
        {
            return reply.Equals("SUCCESS");
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