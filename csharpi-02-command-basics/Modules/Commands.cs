using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using csharpi.Services;
using System.Threading;

namespace csharpi.Modules
{
    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class Commands : ModuleBase
    {
        [Command("hello")]
        public async Task HelloCommand()
        {
            // initialize empty string builder for reply
            var sb = new StringBuilder();

            // get user info from the Context
            var user = Context.User;
            
            // build out the reply
            sb.AppendLine($"You are -> [{user.Username}]");
            sb.AppendLine("I must now say, World!");

            // send simple string reply
            await ReplyAsync(sb.ToString());
        }

        [Command("8ball")]
        [Alias("ask")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task AskEightBall([Remainder]string args = null)
        {
            // I like using StringBuilder to build out the reply
            var sb = new StringBuilder();
            // let's use an embed for this one!
            var embed = new EmbedBuilder();

            // now to create a list of possible replies
            var replies = new List<string>();

            // add our possible replies
            replies.Add("yes");
            replies.Add("no");
            replies.Add("maybe");
            replies.Add("hazzzzy....");

            // time to add some options to the embed (like color and title)
            embed.WithColor(new Color(0, 255,0));
            embed.Title = "Welcome to the 8-ball!";
            
            // we can get lots of information from the Context that is passed into the commands
            // here I'm setting up the preface with the user's name and a comma
            sb.AppendLine($"{Context.User.Username},");
            sb.AppendLine();

            // let's make sure the supplied question isn't null 
            if (args == null)
            {
                // if no question is asked (args are null), reply with the below text
                sb.AppendLine("Sorry, can't answer a question you didn't ask!");
            }
            else 
            {
                // if we have a question, let's give an answer!
                // get a random number to index our list with (arrays start at zero so we subtract 1 from the count)
                var answer = replies[new Random().Next(replies.Count - 1)];
                
                // build out our reply with the handy StringBuilder
                sb.AppendLine($"You asked: [**{args}**]...");
                sb.AppendLine();
                sb.AppendLine($"...your answer is [**{answer}**]");

                // bonus - let's switch out the reply and change the color based on it
                switch (answer) 
                {
                    case "yes":
                    {
                        embed.WithColor(new Color(0, 255, 0));
                        break;
                    }
                    case "no":
                    {
                        embed.WithColor(new Color(255, 0, 0));
                        break;
                    }
                    case "maybe":
                    {
                        embed.WithColor(new Color(255,255,0));
                        break;
                    }
                    case "hazzzzy....":
                    {
                        embed.WithColor(new Color(255,0,255));
                        break;
                    }
                }
            }

            // now we can assign the description of the embed to the contents of the StringBuilder we created
            embed.Description = sb.ToString();

            // this will reply with the embed
            await ReplyAsync(null, false, embed.Build());
        }


        [Command("ServerStats")]
        public async Task ServerInfo()
        {
            SQLHandler sql = new SQLHandler();

            // initialize empty string builder for reply
            var sb = new StringBuilder();
            var teamkillCount = await sql.GetTotalTKs(); ;
            var totalKillCount = 0;  //TODO implement SQL getter. 
            var topKiller = await sql.GetTopKiller();
            var topKilled = await sql.GetTopKilled();
            string TeamKillers = await sql.GetTopTeamKillers();

            // get user info from the Context
            var user = Context.User;

            // build out the reply
            sb.AppendLine($"Hi, {user.Username}!");
            sb.AppendLine("Server stats coming right up! :slight_smile:");
            sb.AppendLine(" ");
            sb.AppendLine(":arrow_forward: There were " + teamkillCount + " teamkills today.");
            sb.AppendLine(":arrow_forward: Today's Most Kills: " + topKiller);
            sb.AppendLine(":arrow_forward: Today's Most Killed: " + topKilled);
            sb.AppendLine(" ");
            sb.AppendLine(":arrow_forward: Todays TeamKillers are: \n \n " + TeamKillers);

            // send simple string reply
            await ReplyAsync(sb.ToString());
        }


        [Command("TodaysPunishments")]
        public async Task TodaysPunishments()
        {
            // initialize empty string builder for reply
            var sb = new StringBuilder();
            SQLHandler sql = new SQLHandler();
            var user = Context.User;

            // get user info from the Context
            var punishments = await sql.GetTodaysPunishments();

            // build out the reply
            sb.AppendLine($"Hi, [{user.Username}]");
            sb.AppendLine(":tired_face:  Auto punishments we had to give out today are: :tired_face: " + punishments);


            // send simple string reply
            await ReplyAsync(sb.ToString());
        }

        [Command("ScanForClones")]
        public async Task ScanForClones()
        {
            // TAW community running joke - easter egg command! ;) 
            var sb = new StringBuilder();
            SQLHandler sql = new SQLHandler();
            var user = Context.User;

            // get user info from the Context
            var punishments = await sql.GetTodaysPunishments();

            // build out the reply
            sb.AppendLine($"Hi, [{user.Username}]");
            sb.AppendLine("Scanning for clones... ");
            await ReplyAsync(sb.ToString());

            Thread.Sleep(2000);
            sb = new StringBuilder();
            sb.AppendLine(":worried:  Clone found:  Kegso   :worried: ");
            sb.AppendLine("Please return origonal Kegso to discord.");
            sb.AppendLine(" ");
            sb.AppendLine("SCAN TERMINATED");
            // send simple string reply
            await ReplyAsync(sb.ToString());
            
        }

    }
}
