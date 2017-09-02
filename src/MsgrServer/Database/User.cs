// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Mabi.Const;
using Aura.Msgr.Network;

namespace Aura.Msgr.Database
{
    public class User : Contact
    {
        /// <summary>
        ///     Creates new user.
        /// </summary>
        public User()
        {
            ChatOptions = ChatOptions.NotifyOnFriendLogIn;
            Groups = new HashSet<int>();
            Friends = new List<Friend>();
        }

        public MsgrClient Client { get; set; }
        public string ChannelName { get; set; }
        public ChatOptions ChatOptions { get; set; }
        public HashSet<int> Groups { get; }
        public List<Friend> Friends { get; }

        /// <summary>
        ///     Returns friend with the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Friend GetFriend(int id)
        {
            lock (Friends)
            {
                return Friends.FirstOrDefault(a => a.Id == id);
            }
        }

        /// <summary>
        ///     Returns list of all friend's ids.
        /// </summary>
        /// <returns></returns>
        public int[] GetFriendIds()
        {
            lock (Friends)
            {
                return Friends.Select(a => a.Id).ToArray();
            }
        }

        /// <summary>
        ///     Returns list of all friend's ids with status Normal.
        /// </summary>
        /// <returns></returns>
        public int[] GetNormalFriendIds()
        {
            lock (Friends)
            {
                return Friends.Where(a => a.FriendshipStatus == FriendshipStatus.Normal).Select(a => a.Id).ToArray();
            }
        }

        /// <summary>
        ///     Returns friendship status from user to given contact.
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public FriendshipStatus GetFriendshipStatus(int contactId)
        {
            var friend = GetFriend(contactId);
            if (friend == null)
                return FriendshipStatus.Blocked;

            return friend.FriendshipStatus;
        }

        /// <summary>
        ///     Sets friendship status from user to given contact.
        ///     Returns true if successful, false if the friend doesn't exist.
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public bool SetFriendshipStatus(int contactId, FriendshipStatus status)
        {
            var friend = GetFriend(contactId);
            if (friend == null)
                return false;

            friend.FriendshipStatus = status;
            return true;
        }
    }
}