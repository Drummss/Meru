using Discord;
using Discord.WebSocket;
using Meru.SDK;
using Meru.SDK.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meru.API
{
    class DiscordMessageAPI : IMessageAPI
    {
        DiscordSocketClient client;

        public int Latency => client.Latency;

        public List<IMeruServer> Servers => GetGuilds();

        private List<IMeruServer> GetGuilds()
        {
            List<IMeruServer> guilds = new List<IMeruServer>();
            foreach(IGuild g in client.Guilds)
            {
                guilds.Add(new RuntimeGuild(g));
            }
            return guilds;
        }
    }
}
