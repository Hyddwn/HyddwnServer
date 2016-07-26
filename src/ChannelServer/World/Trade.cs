// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System.Threading;

namespace Aura.Channel.World
{

	public class Trade
	{
		private static long _ids;

		/// <summary>
		/// Id of this trade session.
		/// </summary>
		public long Id { get; private set; }

		/// <summary>
		/// Creature that initiated the trade.
		/// </summary>
		public Creature Creature1 { get; private set; }

		/// <summary>
		/// Creature that received the trade request.
		/// </summary>
		public Creature Creature2 { get; private set; }

		/// <summary>
		/// Returns true if creature 2 accepted the request.
		/// </summary>
		public bool Accepted { get; private set; }

		/// <summary>
		/// Creates a new trade session.
		/// </summary>
		/// <param name="creature1"></param>
		/// <param name="creature2"></param>
		public Trade(Creature creature1, Creature creature2)
		{
			this.Id = GetNewId();
			this.Creature1 = creature1;
			this.Creature2 = creature2;

			this.Creature1.Temp.ActiveTrade = this;
			this.Creature2.Temp.ActiveTrade = this;
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
		/// Initiates the trade, sending information to creature 1 and
		/// a trade request to creature 2.
		/// </summary>
		public void Initiate()
		{
			Send.TradeInfo(this.Creature1, this.Id, this.Creature2.EntityId);
			Send.TradeRequest(this.Creature2, this.Creature1.EntityId, this.Creature1.Name);
		}

		/// <summary>
		/// Cancels trade, returning all items and closing the trade window.
		/// </summary>
		/// <param name="requestee"></param>
		public void Cancel(Creature requestee)
		{
			this.Creature1.Temp.ActiveTrade = null;
			MoveAllItemsFromTradeToInv(this.Creature1);
			Send.TradeRequestCanceled(this.Creature1, true);
			Send.TradeCancelR(this.Creature1, true);

			this.Creature2.Temp.ActiveTrade = null;
			MoveAllItemsFromTradeToInv(this.Creature2);
			Send.TradeRequestCanceled(this.Creature2, true);
			Send.TradeCancelR(this.Creature2, true);
		}

		/// <summary>
		/// Moves all items creature has in the Trade pocket to the main
		/// inventory.
		/// </summary>
		/// <param name="creature"></param>
		private static void MoveAllItemsFromTradeToInv(Creature creature)
		{
			var items = creature.Inventory.GetItems(a => a.Info.Pocket == Pocket.Trade);
			foreach (var item in items)
			{
				creature.Inventory.Remove(item);
				creature.Inventory.Add(item, true);
			}
		}

		/// <summary>
		/// Accepts trade request from creature 2.
		/// </summary>
		public void Accept()
		{
			if (this.Accepted)
			{
				Log.Warning("Trade.Accept: Trade between 0x{0:X16} and 0x{1:X16} has already been accepted.", this.Creature1.EntityId, this.Creature2.EntityId);
				Send.TradeAcceptRequestR(this.Creature2, true);
				return;
			}

			Send.TradePartnerInfo(this.Creature1, this.Creature2.EntityId, this.Creature2.Name);

			Send.TradeInfo(this.Creature2, this.Id, this.Creature1.EntityId);
			Send.TradePartnerInfo(this.Creature2, this.Creature1.EntityId, this.Creature1.Name);
			Send.TradeAcceptRequestR(this.Creature2, true);
		}
	}
}
