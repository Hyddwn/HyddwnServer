// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using System;
using System.Collections.Generic;

namespace Aura.Shared.Network
{
	public class ServerInfo
	{
		public string Name { get; private set; }
		public Dictionary<string, ChannelInfo> Channels { get; private set; }

		public ServerInfo(string name)
		{
			this.Name = name;
			this.Channels = new Dictionary<string, ChannelInfo>();
		}

		/// <summary>
		/// Returns channel info or null, if it doesn't exist.
		/// </summary>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public ChannelInfo Get(string channelName)
		{
			ChannelInfo result;
			this.Channels.TryGetValue(channelName, out result);
			return result;
		}
	}

	public class ChannelInfo
	{
		public string Name { get; set; }
		public string ServerName { get; set; }
		public string FullName { get; set; }
		public string Host { get; set; }
		public int Port { get; set; }
		public DateTime LastUpdate { get; set; }

		/// <summary>
		/// Status of the channel
		/// </summary>
		/// <remarks>
		/// This needs to be update to reflect the stress value.
		/// </remarks>
		public ChannelState State { get; set; }

		/// <summary>
		/// What events are going on
		/// </summary>
		public ChannelEvent Events { get; set; }

		/// <summary>
		/// 0-100%, in increments of 5%.
		/// </summary>
		public short Stress
		{
			get
			{
				var val = (this.Users * 100) / this.MaxUsers;
				return (short)(val - val % 5);
			}
		}

		/// <summary>
		/// Current users
		/// </summary>
		public int Users { get; set; }

		/// <summary>
		/// Max users able to connect
		/// </summary>
		public int MaxUsers { get; set; }

		public ChannelInfo(string name, string server, string ip, int port)
		{
			this.Name = name;
			this.ServerName = server;
			this.FullName = name + "@" + server;
			this.Host = ip;
			this.Port = port;

			this.State = ChannelState.Normal;
			this.Events = ChannelEvent.None;

			this.LastUpdate = DateTime.MinValue;
		}
	}
}
