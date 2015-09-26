// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Msgr.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Msgr
{
	public class UserManager
	{
		private Dictionary<int, User> _users;

		/// <summary>
		/// Creates new user manager.
		/// </summary>
		public UserManager()
		{
			_users = new Dictionary<int, User>();
		}

		/// <summary>
		/// Adds user to manager.
		/// </summary>
		/// <param name="user"></param>
		public void Add(User user)
		{
			lock (_users)
				_users[user.Id] = user;
		}

		/// <summary>
		/// Removes user from manager.
		/// </summary>
		/// <param name="user"></param>
		public void Remove(User user)
		{
			lock (_users)
				_users.Remove(user.Id);
		}

		/// <summary>
		/// Returns user with given id, or null if it doesn't exist/isn't online.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		public User Get(int userId)
		{
			User result = null;
			lock (_users)
				_users.TryGetValue(userId, out result);
			return result;
		}

		/// <summary>
		/// Returns list of users with given ids.
		/// </summary>
		/// <param name="friendIds"></param>
		/// <returns></returns>
		public List<User> Get(IEnumerable<int> ids)
		{
			var result = new List<User>();

			foreach (var id in ids)
			{
				var user = this.Get(id);
				if (user != null)
					result.Add(user);
			}

			return result;
		}
	}
}
