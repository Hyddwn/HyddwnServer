// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Util.Configuration.Files;
using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration
{
    public sealed class ChannelConf : BaseConf
    {
        public ChannelConf()
        {
            Autoban = new AutobanConfFile();
            Channel = new ChannelConfFile();
            Commands = new CommandsConfFile();
            World = new WorldConfFile();
        }

        /// <summary>
        ///     autoban.conf
        /// </summary>
        public AutobanConfFile Autoban { get; }

        /// <summary>
        ///     channel.conf
        /// </summary>
        public ChannelConfFile Channel { get; }

        /// <summary>
        ///     channel.conf
        /// </summary>
        public CommandsConfFile Commands { get; }

        /// <summary>
        ///     channel.conf
        /// </summary>
        public WorldConfFile World { get; }

        public override void Load()
        {
            LoadDefault();

            Autoban.Load();
            Channel.Load();
            Commands.Load();
            World.Load();
        }
    }
}