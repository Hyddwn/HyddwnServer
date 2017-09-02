// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Mabi.Const;

namespace Aura.Shared.Network
{
    public class ServerInfo
    {
        public ServerInfo(string name)
        {
            Name = name;
            Channels = new Dictionary<string, ChannelInfo>();
        }

        public string Name { get; }
        public Dictionary<string, ChannelInfo> Channels { get; }

        /// <summary>
        ///     Returns channel info or null, if it doesn't exist.
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public ChannelInfo Get(string channelName)
        {
            ChannelInfo result;
            Channels.TryGetValue(channelName, out result);
            return result;
        }
    }

    public class ChannelInfo
    {
        public ChannelInfo(string name, string server, string ip, int port)
        {
            Name = name;
            ServerName = server;
            FullName = name + "@" + server;
            Host = ip;
            Port = port;

            State = ChannelState.Normal;
            Events = ChannelEvent.None;

            LastUpdate = DateTime.MinValue;
        }

        public string Name { get; set; }
        public string ServerName { get; set; }
        public string FullName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public DateTime LastUpdate { get; set; }

        /// <summary>
        ///     Status of the channel
        /// </summary>
        /// <remarks>
        ///     This needs to be update to reflect the stress value.
        /// </remarks>
        public ChannelState State { get; set; }

        /// <summary>
        ///     What events are going on
        /// </summary>
        public ChannelEvent Events { get; set; }

        /// <summary>
        ///     0-100%, in increments of 5%.
        /// </summary>
        public short Stress
        {
            get
            {
                var val = Users * 100 / MaxUsers;
                return (short) (val - val % 5);
            }
        }

        /// <summary>
        ///     Current users
        /// </summary>
        public int Users { get; set; }

        /// <summary>
        ///     Max users able to connect
        /// </summary>
        public int MaxUsers { get; set; }
    }
}