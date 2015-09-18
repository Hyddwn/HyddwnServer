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
		public string AccountId { get; set; }
		public string ChannelName { get; set; }
		public ChatOptions ChatOptions { get; set; }

		public User()
			: base()
		{
			this.ChatOptions = ChatOptions.NotifyOnFriendLogIn;
		}
	}
}
