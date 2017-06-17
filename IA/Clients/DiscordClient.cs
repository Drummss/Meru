﻿using Discord;
using Discord.WebSocket;

using Meru.Addons;
using Meru.Events;
using Meru.FileHandling;
using Meru.SDK;
using Meru.SDK.Events;
using Meru.SDK.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Meru
{
    public class DiscordClient : Client
    {
        private static DiscordClient _instance;
        public static DiscordClient Instance => _instance;

        public DiscordShardedClient Client { private set; get; }

        private string currentPath = Directory.GetCurrentDirectory();

        public DiscordClient()
        {
            if (!File.Exists(currentPath + "/preferences.config"))
            {
                clientInformation = InitializePreferencesFile();
            }
            else
            {
                clientInformation = LoadPreferenceFile();
            }
            Init().GetAwaiter().GetResult();
        }
        public DiscordClient(Client c)
        {
            Addons = c.Addons;
            Events = c.Events;
            Init().GetAwaiter().GetResult();
        }
        public DiscordClient(ClientInformation info)
        {
            clientInformation = info;
            Init().GetAwaiter().GetResult();
        }
        public DiscordClient(Action<ClientInformation> info)
        {
            clientInformation = new ClientInformation();
            info.Invoke(clientInformation);
            Init().GetAwaiter().GetResult();
        }

        public void AddDeveloper(ulong id)
        {
            Events.Developers.Add(id);
        }
        public void AddDeveloper(IDiscordUser user)
        {
            Events.Developers.Add(user.Id);
        }
        public void AddDeveloper(IUser user)
        {
            Events.Developers.Add(user.Id);
        }

        public async Task ConnectAsync()
        {
            await Client.LoginAsync(TokenType.Bot, clientInformation.Token);

            foreach(DiscordSocketClient client in Client.Shards)
            {
                await client.StartAsync();
                // 10 seconds wait
                await Task.Delay(10000);
            }

            await Task.Delay(-1);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public int GetShardId()
        {
            return clientInformation.ShardId;
        }

        public int GetTotalShards()
        {
            return clientInformation.ShardCount;
        }

        private ClientInformation InitializePreferencesFile()
        {
            ClientInformation outputBotInfo = new ClientInformation();
            FileWriter file = new FileWriter("preferences", "config");
            file.WriteComment("Meru v" + VersionNumber + " preferences file");
            file.WriteComment("Please do not change this file except to change\n# except to change your settings");
            file.WriteComment("Bot Name");
            Console.WriteLine("Enter bot name: ");
            string inputString = Console.ReadLine();
            file.Write(inputString);
            outputBotInfo.Name = inputString;

            file.WriteComment("Bot Token");
            Console.WriteLine("Enter bot token: ");
            inputString = Console.ReadLine();
            file.Write(inputString);
            outputBotInfo.Token = inputString;

            file.WriteComment("Shard count");
            Console.WriteLine("Shards [1-25565]:");
            inputString = Console.ReadLine();
            outputBotInfo.ShardCount = int.Parse(inputString);
            if (outputBotInfo.ShardCount < 1)
            {
                outputBotInfo.ShardCount = 1;
            }
            else if (outputBotInfo.ShardCount > 25565)
            {
                outputBotInfo.ShardCount = 25565;
            }

            file.Finish();

            return outputBotInfo;
        }

        private ClientInformation LoadPreferenceFile()
        {
            ClientInformation outputBotInfo = new ClientInformation();
            FileReader file = new FileReader("preferences", "config");
            outputBotInfo.Name = file.ReadLine();
            outputBotInfo.Token = file.ReadLine();
            file.Finish();
            return outputBotInfo;
        }

        protected override async Task Init()
        {
            Log.InitializeLogging(clientInformation);

            Log.Message("IA v" + VersionNumber);

            Client.JoinedGuild += Client_JoinedGuild;
            Client.LeftGuild += Client_LeftGuild;

            foreach (DiscordSocketClient c in Client.Shards)
            {
                c.Ready += async () =>
                {
                    Log.Message($"shard {c.ShardId} ready!");
                    await c.SetGameAsync($"{c.ShardId}/{GetTotalShards()} | >help");
                };

                c.Connected += async () =>
                {
                    Log.Message($"shard {c.ShardId} connected!");
                };

                c.MessageReceived += async (e) =>
                {
                    Task.Run(() => Client_MessageReceived(e));
                };

                c.Disconnected += async (e) =>
                {
                    Log.ErrorAt(c.ShardId + "| Disconnected", e.Message);
                };
            }

            Client.Log += Client_Log;
        }

        private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Error(e.Exception.Message + "\n\n" + e.Exception.StackTrace);
        }

        // Events
        private async Task Client_JoinedGuild(IGuild arg)
        {
            RuntimeGuild g = new RuntimeGuild(arg);

            Task.Run(() => Events.OnGuildJoin(g));
        }

        private async Task Client_LeftGuild(IGuild arg)
        {
            RuntimeGuild g = new RuntimeGuild(arg);

            Task.Run(() => Events.OnGuildLeave(g));
        }

        private async Task Client_Log(LogMessage arg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(arg.Message);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(arg.Exception);
            await Task.CompletedTask;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            try
            {
                RuntimeMessage r = new RuntimeMessage(arg, Client.GetShardFor((((arg as IUserMessage).Channel) as IGuildChannel).Guild));

                if (r.Content.Contains(r.Bot.Id.ToString()))
                {
                    await Events.OnMention(r);
                }

                if (r.Guild != null)
                {
                    await Events.OnMessageRecieved(r);
                }
                else
                {
                    await Events.OnPrivateMessage(r);
                }
            }
            catch (Exception e)
            {
                Log.ErrorAt("messagerecieved", e.Message);
            }
        }
    }
}