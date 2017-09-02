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
		/// The current status of the trade.
		/// </summary>
		public TradeStatus Status { get; private set; }

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
		public void Cancel()
		{
			this.Creature1.Temp.ActiveTrade = null;
			this.Creature1.Inventory.MoveItemsToInvFrom(Pocket.Trade);
			Send.TradeRequestCanceled(this.Creature1, true);
			Send.TradeCancelR(this.Creature1, true);

			this.Creature2.Temp.ActiveTrade = null;
			this.Creature2.Inventory.MoveItemsToInvFrom(Pocket.Trade);
			Send.TradeRequestCanceled(this.Creature2, true);
			Send.TradeCancelR(this.Creature2, true);
		}

		/// <summary>
		/// Moves all items from one creature's trade window to
		/// another's inventory.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		private static void TradeItems(Creature from, Creature to)
		{
			var items = from.Inventory.GetItems(a => a.Info.Pocket == Pocket.Trade);
			foreach (var item in items)
			{
				from.Inventory.Remove(item);
				to.Inventory.Add(new Item(item), true);
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

		/// <summary>
		/// Adds item to trade partner's window.
		/// </summary>
		/// <param name="creature">Creature that added the item.</param>
		/// <param name="item">Item added.</param>
		public void AddItem(Creature creature, Item item)
		{
			var partner = (creature == this.Creature1 ? this.Creature2 : this.Creature1);
			Send.TradeItemAdded(partner, item);

			this.Wait(creature);
		}

		/// <summary>
		/// Removes item from trade partner's window.
		/// </summary>
		/// <param name="creature">Creature that removed the item.</param>
		/// <param name="item">Item removed.</param>
		public void RemoveItem(Creature creature, Item item)
		{
			var partner = (creature == this.Creature1 ? this.Creature2 : this.Creature1);
			Send.TradeItemRemoved(partner, item.EntityId);

			this.Wait(creature);
		}

		/// <summary>
		/// Puts creature into waiting mode.
		/// </summary>
		/// <param name="creature"></param>
		private void Wait(Creature creature)
		{
			this.Status = TradeStatus.NotReady;

			Send.TradeWait(this.Creature1, 4000);
			Send.TradeWait(this.Creature2, 4000);
		}

		/// <summary>
		/// Puts creature into ready mode.
		/// </summary>
		/// <param name="creature"></param>
		public void Ready(Creature creature)
		{
			this.Status++;

			Send.TradeReadied(this.Creature1, creature.EntityId);
			Send.TradeReadied(this.Creature2, creature.EntityId);

			if (this.Status == TradeStatus.BothReady)
				this.Complete();

			Send.TradeReadyR(creature, true);
		}

		/// <summary>
		/// Completes trade session.
		/// </summary>
		private void Complete()
		{
			this.Creature1.Temp.ActiveTrade = null;
			this.Creature2.Temp.ActiveTrade = null;

			TradeItems(this.Creature1, this.Creature2);
			TradeItems(this.Creature2, this.Creature1);

			Send.TradeComplete(this.Creature1);
			Send.TradeComplete(this.Creature2);
		}
	}

	public enum TradeStatus
	{
		NotReady = 0,
		OneReady = 1,
		BothReady = 2,
	}
}
