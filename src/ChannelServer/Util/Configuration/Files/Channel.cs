// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration.Files
{
    public class ChannelConfFile : ConfFile
    {
        public string LoginHost { get; protected set; }
        public int LoginPort { get; protected set; }

        public string ChannelServer { get; protected set; }
        public string ChannelName { get; protected set; }
        public string ChannelHost { get; protected set; }
        public int ChannelPort { get; protected set; }
        public int MaxUsers { get; protected set; }

        public void Load()
        {
            Require("system/conf/channel.conf");

            LoginHost = GetString("login_host", "127.0.0.1");
            LoginPort = GetInt("login_port", 11000);

            ChannelServer = GetString("channel_server", "Aura");
            ChannelName = GetString("channel_name", "Ch1");
            ChannelHost = GetString("channel_host", "127.0.0.1");
            ChannelPort = GetInt("channel_port", 11020);
            MaxUsers = Math.Max(1, GetInt("max_users", 20));
        }
    }
}