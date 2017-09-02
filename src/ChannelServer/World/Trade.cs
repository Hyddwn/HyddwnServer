// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Threading;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World
{
    public class Trade
    {
        private static long _ids;

        /// <summary>
        ///     Creates a new trade session.
        /// </summary>
        /// <param name="creature1"></param>
        /// <param name="creature2"></param>
        public Trade(Creature creature1, Creature creature2)
        {
            Id = GetNewId();
            Creature1 = creature1;
            Creature2 = creature2;

            Creature1.Temp.ActiveTrade = this;
            Creature2.Temp.ActiveTrade = this;
        }

        /// <summary>
        ///     Id of this trade session.
        /// </summary>
        public long Id { get; }

        /// <summary>
        ///     Creature that initiated the trade.
        /// </summary>
        public Creature Creature1 { get; }

        /// <summary>
        ///     Creature that received the trade request.
        /// </summary>
        public Creature Creature2 { get; }

        /// <summary>
        ///     Returns true if creature 2 accepted the request.
        /// </summary>
        public bool Accepted { get; private set; }

        /// <summary>
        ///     The current status of the trade.
        /// </summary>
        public TradeStatus Status { get; private set; }

        /// <summary>
        ///     Returns a new trade id.
        /// </summary>
        /// <returns></returns>
        private static long GetNewId()
        {
            return Interlocked.Increment(ref _ids);
        }

        /// <summary>
        ///     Initiates the trade, sending information to creature 1 and
        ///     a trade request to creature 2.
        /// </summary>
        public void Initiate()
        {
            Send.TradeInfo(Creature1, Id, Creature2.EntityId);
            Send.TradeRequest(Creature2, Creature1.EntityId, Creature1.Name);
        }

        /// <summary>
        ///     Cancels trade, returning all items and closing the trade window.
        /// </summary>
        public void Cancel()
        {
            Creature1.Temp.ActiveTrade = null;
            Creature1.Inventory.MoveItemsToInvFrom(Pocket.Trade);
            Send.TradeRequestCanceled(Creature1, true);
            Send.TradeCancelR(Creature1, true);

            Creature2.Temp.ActiveTrade = null;
            Creature2.Inventory.MoveItemsToInvFrom(Pocket.Trade);
            Send.TradeRequestCanceled(Creature2, true);
            Send.TradeCancelR(Creature2, true);
        }

        /// <summary>
        ///     Moves all items from one creature's trade window to
        ///     another's inventory.
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
        ///     Accepts trade request from creature 2.
        /// </summary>
        public void Accept()
        {
            if (Accepted)
            {
                Log.Warning("Trade.Accept: Trade between 0x{0:X16} and 0x{1:X16} has already been accepted.",
                    Creature1.EntityId, Creature2.EntityId);
                Send.TradeAcceptRequestR(Creature2, true);
                return;
            }

            Send.TradePartnerInfo(Creature1, Creature2.EntityId, Creature2.Name);

            Send.TradeInfo(Creature2, Id, Creature1.EntityId);
            Send.TradePartnerInfo(Creature2, Creature1.EntityId, Creature1.Name);
            Send.TradeAcceptRequestR(Creature2, true);
        }

        /// <summary>
        ///     Adds item to trade partner's window.
        /// </summary>
        /// <param name="creature">Creature that added the item.</param>
        /// <param name="item">Item added.</param>
        public void AddItem(Creature creature, Item item)
        {
            var partner = creature == Creature1 ? Creature2 : Creature1;
            Send.TradeItemAdded(partner, item);

            Wait(creature);
        }

        /// <summary>
        ///     Removes item from trade partner's window.
        /// </summary>
        /// <param name="creature">Creature that removed the item.</param>
        /// <param name="item">Item removed.</param>
        public void RemoveItem(Creature creature, Item item)
        {
            var partner = creature == Creature1 ? Creature2 : Creature1;
            Send.TradeItemRemoved(partner, item.EntityId);

            Wait(creature);
        }

        /// <summary>
        ///     Puts creature into waiting mode.
        /// </summary>
        /// <param name="creature"></param>
        private void Wait(Creature creature)
        {
            Status = TradeStatus.NotReady;

            Send.TradeWait(Creature1, 4000);
            Send.TradeWait(Creature2, 4000);
        }

        /// <summary>
        ///     Puts creature into ready mode.
        /// </summary>
        /// <param name="creature"></param>
        public void Ready(Creature creature)
        {
            Status++;

            Send.TradeReadied(Creature1, creature.EntityId);
            Send.TradeReadied(Creature2, creature.EntityId);

            if (Status == TradeStatus.BothReady)
                Complete();

            Send.TradeReadyR(creature, true);
        }

        /// <summary>
        ///     Completes trade session.
        /// </summary>
        private void Complete()
        {
            Creature1.Temp.ActiveTrade = null;
            Creature2.Temp.ActiveTrade = null;

            TradeItems(Creature1, Creature2);
            TradeItems(Creature2, Creature1);

            Send.TradeComplete(Creature1);
            Send.TradeComplete(Creature2);
        }
    }

    public enum TradeStatus
    {
        NotReady = 0,
        OneReady = 1,
        BothReady = 2
    }
}