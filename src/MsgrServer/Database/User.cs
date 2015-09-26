// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Msgr.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Msgr.Database
{
	public class User : Contact
	{
		public MsgrClient Client { get; set; }
		public string ChannelName { get; set; }
		public ChatOptions ChatOptions { get; set; }
		public HashSet<int> Groups { get; private set; }
		public List<Friend> Friends { get; private set; }

		public User()
			: base()
		{
			this.ChatOptions = ChatOptions.NotifyOnFriendLogIn;
			this.Groups = new HashSet<int>();
			this.Friends = new List<Friend>();
		}

		public Friend GetFriend(int id)
		{
			return this.Friends.FirstOrDefault(a => a.Id == id);
		}
	}
}
