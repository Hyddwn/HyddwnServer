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

		/// <summary>
		/// Creates new user.
		/// </summary>
		public User()
			: base()
		{
			this.ChatOptions = ChatOptions.NotifyOnFriendLogIn;
			this.Groups = new HashSet<int>();
			this.Friends = new List<Friend>();
		}

		/// <summary>
		/// Returns friend with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Friend GetFriend(int id)
		{
			lock (this.Friends)
				return this.Friends.FirstOrDefault(a => a.Id == id);
		}

		/// <summary>
		/// Returns list of all friend's ids.
		/// </summary>
		/// <returns></returns>
		public int[] GetFriendIds()
		{
			lock (this.Friends)
				return this.Friends.Select(a => a.Id).ToArray();
		}

		/// <summary>
		/// Returns list of all friend's ids with status Normal.
		/// </summary>
		/// <returns></returns>
		public int[] GetNormalFriendIds()
		{
			lock (this.Friends)
				return this.Friends.Where(a => a.FriendshipStatus == FriendshipStatus.Normal).Select(a => a.Id).ToArray();
		}

		/// <summary>
		/// Returns friendship status from user to given contact.
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public FriendshipStatus GetFriendshipStatus(int contactId)
		{
			var friend = this.GetFriend(contactId);
			if (friend == null)
				return FriendshipStatus.Blocked;

			return friend.FriendshipStatus;
		}

		/// <summary>
		/// Sets friendship status from user to given contact.
		/// Returns true if successful, false if the friend doesn't exist.
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public bool SetFriendshipStatus(int contactId, FriendshipStatus status)
		{
			var friend = this.GetFriend(contactId);
			if (friend == null)
				return false;

			friend.FriendshipStatus = status;
			return true;
		}
	}
}
