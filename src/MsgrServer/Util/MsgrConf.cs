// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;
using System;

namespace Aura.Msgr.Util
{
	public class MsgrConf : BaseConf
	{
		/// <summary>
		/// msgr.conf
		/// </summary>
		public MsgrConfFile Msgr { get; protected set; }

		public MsgrConf()
		{
			this.Msgr = new MsgrConfFile();
		}

		public override void Load()
		{
			this.LoadDefault();
			this.Msgr.Load();
		}
	}

	/// <summary>
	/// Represents msgr.conf
	/// </summary>
	public class MsgrConfFile : ConfFile
	{
		public int Port { get; protected set; }
		public int MaxFriends { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/msgr.conf");

			this.Port = this.GetInt("port", 8002);
			this.MaxFriends = Math.Max(0, this.GetInt("max_friends", 0));
		}
	}
}
