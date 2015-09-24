// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;

namespace Aura.Msgr.Database
{
	public class Friend : Contact
	{
		public int GroupId { get; set; }
		public FriendshipStatus FriendshipStatus { get; set; }

		public Friend()
		{
			this.GroupId = -1;
		}
	}
}
