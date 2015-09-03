// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using System;
using System.Collections.Generic;

namespace Aura.Channel.World
{
	/// <summary>
	/// This has a range because parties have to keep their ID when moving from channel to channel.
	/// Whilst NONE of that functionality is currently in place, that was a consideration taken into account,
	/// and so I decided (with help, of course) that giving each channel a range of 100k, and allowing parties to traverse
	/// in this way, as a full entity, would be the best course of action.
	/// 
	/// They'll need to be given space in the DB at a later date (I really hope I'm not the one who has to write that MySQL DB update..)
	/// </summary>
	public class PartyManager
	{
		private object _sync = new object();

		private long _offset = 0;
		private int _range = 0;
		private const int _maxPartyRange = 100000;

		/// <summary>
		/// The last ID used by a party.
		/// </summary>
		/// <remarks>I'm not really sure what this would be used for,
		/// but I feel like I'd regret not putting in place..?</remarks>
		public long CurrentID
		{
			get
			{
				lock (_sync)
					return MabiId.Parties + _offset + _range;
			}
		}

		/// <summary>
		/// Initializes party manager.
		/// </summary>
		public void Initialize()
		{
			ChannelServer.Instance.Events.PlayerDisconnect += PlayerDisconnect;
		}

		/// <summary>
		/// This removes players who disconnect from any party they're in.
		/// </summary>
		/// <param name="creature"></param>
		public void PlayerDisconnect(Creature creature)
		{
			if (creature.IsInParty)
				creature.Party.DisconnectedMember(creature);
		}

		/// <summary>
		/// Passes the next available ID for use in a party.
		/// </summary>
		/// <returns></returns>
		public long GetNextPartyId()
		{
			lock (_sync)
			{
				_offset++;
				if (_offset >= _maxPartyRange)
					_offset = 0;

				return MabiId.Parties + _offset + _range;
			}
		}

		// TODO: Implement the below >_>
		/// <summary>
		/// This is for setting the range of party IDs, which is set by the server
		/// upon connecting to the Login Server.
		/// </summary>
		/// <param name="range"></param>
		//public void RangeSet(int range)
		//{
		//	_range = range * _maxPartyRange;
		//}
	}
}
