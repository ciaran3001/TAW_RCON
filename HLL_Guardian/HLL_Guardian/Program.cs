using Discord;
using Discord.WebSocket;
using HLL_Guardian.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLL_Guardian
{
    class Program
    {
        private DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = "NzQ0MTg2NDE2MzM0MTEwODEx.Xzfjmw.FGrqRHMfFodmwh_JPreaAPPTh2Y";

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();


            await Announce("Herro");
            // Block this task until the program is closed.
            await Task.Delay(-1);
            

        }


        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task Announce(string msg) // 1
        {
            Console.WriteLine("Saying: " + msg);
            //DiscordSocketClient _client = new DiscordSocketClient(); // 2
            ulong id = 744186137479741500; // 3
            var chnl = _client.GetChannel(id) as IMessageChannel; // 4
            await chnl.SendMessageAsync(msg); // 5
        }
    }
}
