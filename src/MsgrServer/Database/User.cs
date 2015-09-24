// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Msgr.Database
{
	public class User : Contact
	{
		public string ChannelName { get; set; }
		public ChatOptions ChatOptions { get; set; }
		public List<Group> Groups { get; private set; }
		public List<Friend> Friends { get; private set; }

		public User()
			: base()
		{
			this.ChatOptions = ChatOptions.NotifyOnFriendLogIn;
			this.Groups = new List<Group>();
			this.Friends = new List<Friend>();
		}

		public Group GetGroup(int id)
		{
			return this.Groups.FirstOrDefault(a => a.Id == id);
		}

		public Friend GetFriend(int id)
		{
			return this.Friends.FirstOrDefault(a => a.Id == id);
		}
	}
}
