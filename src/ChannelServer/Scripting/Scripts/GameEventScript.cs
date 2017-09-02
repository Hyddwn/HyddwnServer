// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.GameEvents;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
    /// <summary>
    ///     Script for in-game events, like Double Rainbow.
    /// </summary>
    public class GameEventScript : GeneralScript
    {
        private readonly List<ActivationSpan> _activationSpans = new List<ActivationSpan>();

        /// <summary>
        ///     The event's unique id.
        /// </summary>
        /// <remarks>
        ///     Sent to client, some ids activate special client behavior.
        /// </remarks>
        public string Id { get; private set; }

        /// <summary>
        ///     The event's name, which is used in notices and broadcasts.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Returns the current state of the event.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        ///     Loads and sets up event.
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            Load();

            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(Name))
            {
                Log.Error("Id or name not set for event script '{0}'.", GetType().Name);
                return false;
            }

            ChannelServer.Instance.GameEventManager.Register(this);

            AfterLoad();

            return true;
        }

        public override void Dispose()
        {
            ChannelServer.Instance.GameEventManager.Unregister(Id);
            End();

            base.Dispose();
        }

        /// <summary>
        ///     Called after script was registered, so it can schedule itself.
        /// </summary>
        public virtual void AfterLoad()
        {
        }

        /// <summary>
        ///     Sets event's id.
        /// </summary>
        /// <param name="id"></param>
        public void SetId(string id)
        {
            Id = id;
        }

        /// <summary>
        ///     Sets event's name, which is used for notices and broadcasts.
        /// </summary>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Starts event if it's not active yet.
        /// </summary>
        public void Start()
        {
            if (IsActive)
                return;

            IsActive = true;
            OnStart();

            Send.Notice(NoticeType.Middle, L("The {0} Event is now in progress."), Name);
            Send.GameEventStateUpdate(Id, IsActive);
        }

        /// <summary>
        ///     Stops event if it's active.
        /// </summary>
        public void End()
        {
            if (!IsActive)
                return;

            IsActive = false;
            OnEnd();

            Send.Notice(NoticeType.Middle, L("The {0} Event has ended."), Name);
            Send.GameEventStateUpdate(Id, IsActive);
        }

        /// <summary>
        ///     Called when the event is activated.
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        ///     Called when the event is deactivated.
        /// </summary>
        protected virtual void OnEnd()
        {
        }

        /// <summary>
        ///     Adds the given activation span to the event, in which it's
        ///     supposed to be active.
        /// </summary>
        /// <param name="span"></param>
        public void AddActivationSpan(ActivationSpan span)
        {
            lock (_activationSpans)
            {
                _activationSpans.Add(span);
            }

            var now = DateTime.Now;

            // Active time
            if (now >= span.Start && now < span.End)
                Start();
            // Inactive time
            else
                End();
        }

        /// <summary>
        ///     Returns true if the event is supposed to be active at the given
        ///     time, based on its activation spans.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsActiveTime(DateTime time)
        {
            lock (_activationSpans)
            {
                return _activationSpans.Any(a => time >= a.Start && time < a.End);
            }
        }

        /// <summary>
        ///     Adds global bonus.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="multiplier"></param>
        protected void AddGlobalBonus(GlobalBonusStat stat, float multiplier)
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.AddBonus(Id, Name, stat, multiplier);
        }

        /// <summary>
        ///     Removes all global bonuses associated with this event.
        /// </summary>
        protected void RemoveGlobalBonuses()
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.RemoveBonuses(Id);
        }

        /// <summary>
        ///     Adds global drop by race id.
        /// </summary>
        /// <param name="raceId"></param>
        /// <param name="data"></param>
        protected void AddGlobalDrop(int raceId, DropData data)
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.AddDrop(Id, new GlobalDropById(Id, raceId, data));
        }

        /// <summary>
        ///     Adds global drop by race tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        protected void AddGlobalDrop(string tag, DropData data)
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.AddDrop(Id, new GlobalDropByTag(Id, tag, data));
        }

        /// <summary>
        ///     Adds global drop by type.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        protected void AddGlobalDrop(GlobalDropType type, DropData data)
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.AddDrop(Id, new GlobalDropByType(Id, type, data));
        }

        /// <summary>
        ///     Adds global drop by region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="data"></param>
        protected void AddGlobalDropByRegion(int regionId, DropData data)
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.AddDrop(Id,
                new GlobalDropByRegion(Id, regionId, data));
        }

        /// <summary>
        ///     Removes all global drops associated with this event.
        /// </summary>
        protected void RemoveGlobalDrops()
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.RemoveAllDrops(Id);
        }

        /// <summary>
        ///     Adds fishing ground, to be considered when players fish.
        /// </summary>
        /// <remarks>
        ///     The fishing grounds describe where a player can fish under which
        ///     circumstances, and what they can catch. If multiple fishing
        ///     grounds exist for a location, the priority and the chance
        ///     determine which ground is used.
        ///     For example, if you want to override the Tir fishing spots during
        ///     and event, you could do the following, which would add a new
        ///     ground for the Tir spots, with a high priority and a 100% chance,
        ///     so it always gets selected over the default.
        /// </remarks>
        /// <example>
        ///     // Override Tir for the apple fishing event
        ///     AddFishingGround(
        ///     priority: 100000,
        ///     chance: 100,
        ///     locations: new[]
        ///     {
        ///     "Uladh_main/town_TirChonaill/fish_tircho_res_",
        ///     "Uladh_main/field_Tir_S_aa/fish_tircho_stream_",
        ///     "Uladh_main/town_TirChonaill/fish_tircho_stream_",
        ///     },
        ///     items: new[]
        ///     {
        ///     new DropData(itemId: 50003, chance: 250), // Apple
        ///     new DropData(itemId: 12241, chance: 50),  // Golden Apple
        ///     new DropData(itemId: 75463, chance: 1),   // Avon Apple
        ///     }
        ///     );
        /// </example>
        /// <param name="priority"></param>
        /// <param name="chance"></param>
        /// <param name="rod"></param>
        /// <param name="bait"></param>
        /// <param name="locations"></param>
        /// <param name="items"></param>
        protected void AddFishingGround(int priority, double chance, IEnumerable<string> locations,
            IEnumerable<DropData> items, int rod = 0, int bait = 0)
        {
            var fishingGroundData = new FishingGroundData();
            fishingGroundData.Name = Id;
            fishingGroundData.Priority = priority;
            fishingGroundData.Chance = (float) chance;
            fishingGroundData.Rod = rod;
            fishingGroundData.Bait = bait;
            fishingGroundData.Locations = locations.ToArray();
            fishingGroundData.Items = items.ToArray();
            fishingGroundData.TotalItemChance = items.Sum(a => a.Chance);

            ChannelServer.Instance.GameEventManager.GlobalBonuses.AddFishingGround(Id, fishingGroundData);
        }

        /// <summary>
        ///     Removes all event fishing grounds associated with this event.
        /// </summary>
        protected void RemoveFishingGrounds()
        {
            ChannelServer.Instance.GameEventManager.GlobalBonuses.RemoveAllFishingGrounds(Id);
        }

        /// <summary>
        ///     Schedules this event to be active during the given time span.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="till"></param>
        protected void ScheduleEvent(DateTime from, DateTime till)
        {
            var gameEventId = Id;
            ScheduleEvent(gameEventId, from, till);
        }

        /// <summary>
        ///     Schedules this event to be active during the given time span.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="timeSpan"></param>
        protected void ScheduleEvent(DateTime from, TimeSpan timeSpan)
        {
            var gameEventId = Id;
            ScheduleEvent(gameEventId, from, timeSpan);
        }

        /// <summary>
        ///     Adds the the item to the given shop.
        /// </summary>
        /// <param name="shopName"></param>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <param name="stock"></param>
        protected void AddEventItemToShop(string shopName, int itemId, int amount = 1, int price = -1, int stock = -1)
        {
            var shop = ChannelServer.Instance.ScriptManager.NpcShopScripts.Get(shopName);
            if (shop == null)
            {
                Log.Error("{0}.AddEventItemToShop: Shop '{1}' not found.", GetType().Name, shopName);
                return;
            }

            shop.Add(Localization.Get("Event"), itemId, amount, price, stock);
        }

        /// <summary>
        ///     Removes all event items from the given shop.
        /// </summary>
        /// <param name="shopName"></param>
        protected void RemoveEventItemsFromShop(string shopName)
        {
            var shop = ChannelServer.Instance.ScriptManager.NpcShopScripts.Get(shopName);
            if (shop == null)
            {
                Log.Error("{0}.RemoveEventItemsFromShop: Shop '{1}' not found.", GetType().Name, shopName);
                return;
            }

            shop.ClearTab(Localization.Get("Event"));
        }
    }

    public class ActivationSpan
    {
        public string Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}