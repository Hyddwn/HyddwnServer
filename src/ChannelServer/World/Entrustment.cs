// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System.Threading;

namespace Aura.Channel.World
{
	public class Entrustment
	{
		private static long _ids;

		/// <summary>
		/// Id of this entrustment session.
		/// </summary>
		public long Id { get; private set; }

		/// <summary>
		/// Creature that initiated the entrustment.
		/// </summary>
		public Creature Creature1 { get; private set; }

		/// <summary>
		/// Creature that received the entrustment request.
		/// </summary>
		public Creature Creature2 { get; private set; }

		/// <summary>
		/// Returns true if creature 2 accepted the request.
		/// </summary>
		public bool Accepted { get; private set; }

		/// <summary>
		/// The current status of the entrustment.
		/// </summary>
		public EntrustmentStatus Status { get; private set; }

		/// <summary>
		/// Creates a new entrustment session.
		/// </summary>
		/// <param name="creature1"></param>
		/// <param name="creature2"></param>
		public Entrustment(Creature creature1, Creature creature2)
		{
			this.Id = GetNewId();
			this.Creature1 = creature1;
			this.Creature2 = creature2;

			this.Creature1.Temp.ActiveEntrustment = this;
			this.Creature2.Temp.ActiveEntrustment = this;
		}

		/// <summary>
		/// Returns a new trade id.
		/// </summary>
		/// <returns></returns>
		private static long GetNewId()
		{
			return Interlocked.Increment(ref _ids);
		}

		/// <summary>
		/// Initiates entrustment, sending a request to creature 2.
		/// </summary>
		public void Initiate()
		{
			Send.EntrustedEnchantRequest(this.Creature2, this.Creature1.EntityId, 0);
		}
	}
}
