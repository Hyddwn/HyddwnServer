// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Mabi;
using Aura.Channel.World.Entities;
using Aura.Channel.Skills;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Channel.World.Dungeons;
using Aura.Channel.World.Quests;

namespace Aura.Channel.World
{
	public class EventManager
	{
		/// <summary>
		/// Raised when there's a security violation
		/// </summary>
		public event Action<SecurityViolationEventArgs> SecurityViolation;
		public void OnSecurityViolation(SecurityViolationEventArgs args) { SecurityViolation.Raise(args); }

		// -------------------------------------------------------------

		/// <summary>
		/// Raised every second in real time.
		/// </summary>
		public event Action<ErinnTime> SecondsTimeTick;
		public void OnSecondsTimeTick(ErinnTime now) { SecondsTimeTick.Raise(now); }

		/// <summary>
		/// Raised every minute in real time.
		/// </summary>
		public event Action<ErinnTime> MinutesTimeTick;
		public void OnMinutesTimeTick(ErinnTime now) { MinutesTimeTick.Raise(now); }

		/// <summary>
		/// Raised every hour in real time.
		/// </summary>
		public event Action<ErinnTime> HoursTimeTick;
		public void OnHoursTimeTick(ErinnTime now) { HoursTimeTick.Raise(now); }

		/// <summary>
		/// Raised every 1.5s (1min Erinn time).
		/// </summary>
		public event Action<ErinnTime> ErinnTimeTick;
		public void OnErinnTimeTick(ErinnTime now) { ErinnTimeTick.Raise(now); }

		/// <summary>
		/// Raised every 18min (1/2 day Erinn time).
		/// </summary>
		public event Action<ErinnTime> ErinnDaytimeTick;
		public void OnErinnDaytimeTick(ErinnTime now) { ErinnDaytimeTick.Raise(now); }

		/// <summary>
		/// Raised at 00:00am Erinn time.
		/// </summary>
		public event Action<ErinnTime> ErinnMidnightTick;
		public void OnErinnMidnightTick(ErinnTime now) { ErinnMidnightTick.Raise(now); }

		/// <summary>
		/// Raised every 5 minutes in real time.
		/// </summary>
		public event Action<ErinnTime> MabiTick;
		public void OnMabiTick(ErinnTime now) { MabiTick.Raise(now); }

		/// <summary>
		/// Raised every 9 minutes in real time.
		/// </summary>
		public event Action<ErinnTime> PlayTimeTick;
		public void OnPlayTimeTick(ErinnTime now) { PlayTimeTick.Raise(now); }

		// ------------------------------------------------------------------

		/// <summary>
		// For sending any packets that need to be sent
		// to each and every character on login
		// Examples: Enabling/disabling client features
		/// </summary>
		public event Action<Creature> CreatureConnecting;
		public void OnCreatureConnecting(Creature creature) { CreatureConnecting.Raise(creature); }

		// For sending packets that need to be sent
		// to specific characters on login
		// Examples: Initial values for enabled features
		public event Action<Creature> CreatureConnected;
		public void OnCreatureConnected(Creature creature) { CreatureConnected.Raise(creature); }

		/// <summary>
		/// Raised a few seconds after player logged in.
		/// </summary>
		public event Action<Creature> PlayerLoggedIn;
		public void OnPlayerLoggedIn(Creature creature) { PlayerLoggedIn.Raise(creature); }

		/// <summary>
		/// Raised when a player disconnects from server.
		/// </summary>
		public event Action<Creature> PlayerDisconnect;
		public void OnPlayerDisconnect(Creature creature) { PlayerDisconnect.Raise(creature); }

		/// <summary>
		/// Raised when player enters a region.
		/// </summary>
		public event Action<Creature> PlayerEntersRegion;
		public void OnPlayerEntersRegion(Creature creature) { PlayerEntersRegion.Raise(creature); }

		/// <summary>
		/// Raised when player leaves a region.
		/// </summary>
		public event Action<Creature> PlayerLeavesRegion;
		public void OnPlayerLeavesRegion(Creature creature) { PlayerLeavesRegion.Raise(creature); }

		/// <summary>
		/// Raised when player drops, destroys, sells,
		/// uses (decrements), etcs an item.
		/// </summary>
		public event Action<Creature, int, int> PlayerRemovesItem;
		public void OnPlayerRemovesItem(Creature creature, int itemId, int amount) { PlayerRemovesItem.Raise(creature, itemId, amount); }

		/// <summary>
		/// Raised when player receives an item in any way.
		/// </summary>
		public event Action<Creature, int, int> PlayerReceivesItem;
		public void OnPlayerReceivesItem(Creature creature, int itemId, int amount) { PlayerReceivesItem.Raise(creature, itemId, amount); }

		/// <summary>
		/// Raised when player uses an item.
		/// </summary>
		public event Action<Creature, Item> PlayerUsesItem;
		public void OnPlayerUsesItem(Creature creature, Item item) { PlayerUsesItem.Raise(creature, item); }

		/// <summary>
		/// Raised when player equips an item.
		/// </summary>
		public event Action<Creature, Item> PlayerEquipsItem;
		public void OnPlayerEquipsItem(Creature creature, Item item) { PlayerEquipsItem.Raise(creature, item); }

		/// <summary>
		/// Raised when player unequips an item.
		/// </summary>
		public event Action<Creature, Item> PlayerUnequipsItem;
		public void OnPlayerUnequipsItem(Creature creature, Item item) { PlayerUnequipsItem.Raise(creature, item); }

		/// <summary>
		/// Raised when player completes a quest.
		/// </summary>
		public event Action<Creature, int> PlayerCompletesQuest;
		public void OnPlayerCompletesQuest(Creature creature, int questId) { PlayerCompletesQuest.Raise(creature, questId); }

		/// <summary>
		/// Raised when skill rank changes.
		/// </summary>
		public event Action<Creature, Skill> SkillRankChanged;
		public void OnSkillRankChanged(Creature creature, Skill skill) { SkillRankChanged.Raise(creature, skill); }

		/// <summary>
		/// Raised when player used skill.
		/// </summary>
		public event Action<Creature, Skill> PlayerUsedSkill;
		public void OnPlayerUsedSkill(Creature creature, Skill skill) { PlayerUsedSkill.Raise(creature, skill); }

		/// <summary>
		/// Raised when player cleared a dungeon.
		/// </summary>
		public event Action<Creature, Dungeon> PlayerClearedDungeon;
		public void OnPlayerClearedDungeon(Creature creature, Dungeon dungeon) { PlayerClearedDungeon.Raise(creature, dungeon); }

		/// <summary>
		/// Raised when player heals someone, before the heal is actually applied.
		/// </summary>
		public event Action<Creature, Creature, Skill> PlayerHealsCreature;
		public void OnPlayerHealsCreature(Creature creature, Creature target, Skill skill) { PlayerHealsCreature.Raise(creature, target, skill); }

		// ------------------------------------------------------------------

		/// <summary>
		/// Raised when a creature is killed by something.
		/// </summary>
		public event Action<Creature, Creature> CreatureKilled;
		public void OnCreatureKilled(Creature creature, Creature killer) { CreatureKilled.Raise(creature, killer); }

		/// <summary>
		/// Raised when a creature is killed by a player.
		/// </summary>
		public event Action<Creature, Creature> CreatureKilledByPlayer;
		public void OnCreatureKilledByPlayer(Creature creature, Creature killer) { CreatureKilledByPlayer.Raise(creature, killer); }

		/// <summary>
		/// Raised when a creature is attacked by a player.
		/// </summary>
		public event Action<TargetAction> CreatureAttackedByPlayer;
		public void OnCreatureAttackedByPlayer(TargetAction action) { CreatureAttackedByPlayer.Raise(action); }

		/// <summary>
		/// Raised when a creature is attacked.
		/// </summary>
		public event Action<TargetAction> CreatureAttack;
		public void OnCreatureAttacked(TargetAction action) { CreatureAttack.Raise(action); }

		/// <summary>
		/// Raised when a creature attacks some creature.
		/// </summary>
		/// TODO: Improve the names of these events.
		public event Action<AttackerAction> CreatureAttacks;
		public void OnCreatureAttacks(AttackerAction action) { CreatureAttacks.Raise(action); }

		/// <summary>
		/// Raised when a creature's level increases.
		/// </summary>
		public event Action<Creature> CreatureLevelUp;
		public void OnCreatureLevelUp(Creature creature) { CreatureLevelUp.Raise(creature); }

		/// <summary>
		/// Raised when a creature gets a new keyword.
		/// </summary>
		public event Action<Creature, int> CreatureGotKeyword;
		public void OnCreatureGotKeyword(Creature creature, int keywordId) { CreatureGotKeyword.Raise(creature, keywordId); }

		/// <summary>
		/// Raised when a creature gathers items.
		/// </summary>
		public event Action<CollectEventArgs> CreatureGathered;
		public void OnCreatureGathered(CollectEventArgs args) { CreatureGathered.Raise(args); }

		/// <summary>
		/// Raised while handling the combat action pack.
		/// </summary>
		public event Action<CombatActionPack> HandlingCombatActionPack;
		public void OnHandlingCombatActionPack(CombatActionPack pack) { HandlingCombatActionPack.Raise(pack); }

		/// <summary>
		/// Raised when a creature produces material (e.g. Weaving, Handicraft, Potion Making).
		/// </summary>
		public event Action<ProductionEventArgs> CreatureProducedItem;
		public void OnCreatureProducedItem(ProductionEventArgs args) { CreatureProducedItem.Raise(args); }

		/// <summary>
		/// Raised when a creature created an item (Tailoring, Blacksmithing).
		/// </summary>
		public event Action<CreationEventArgs> CreatureCreatedItem;
		public void OnCreatureCreatedItem(CreationEventArgs args) { CreatureCreatedItem.Raise(args); }

		/// <summary>
		/// Raised when a creature cooked something.
		/// </summary>
		public event Action<CookingEventArgs> CreatureCookedMeal;
		public void OnCreatureCookedMeal(CookingEventArgs args) { CreatureCookedMeal.Raise(args); }

		/// <summary>
		/// Raised when a creature tried to produce or create something.
		/// </summary>
		public event Action<Creature, bool> CreatureFinishedProductionOrCollection;
		public void OnCreatureFinishedProductionOrCollection(Creature creature, bool success) { CreatureFinishedProductionOrCollection.Raise(creature, success); }

		/// <summary>
		/// Raised when a creature fished pulled hook out of the water,
		/// regardless of success. On fail, Item is null.
		/// </summary>
		public event Action<Creature, Item> CreatureFished;
		public void OnCreatureFished(Creature creature, Item item) { CreatureFished.Raise(creature, item); }

		/// <summary>
		/// Raised when a creature fished pulled hook out of the water,
		/// regardless of success. On fail, Item is null.
		/// </summary>
		public event Action<Creature, LuckyFinish, int> CreatureGotLuckyFinish;
		public void OnCreatureGotLuckyFinish(Creature creature, LuckyFinish finish, int amount) { CreatureGotLuckyFinish.Raise(creature, finish, amount); }

		/// <summary>
		/// Raised when a creature started a PTJ.
		/// </summary>
		public event Action<Creature, PtjType> CreatureStartedPtj;
		public void OnCreatureStartedPtj(Creature creature, PtjType type) { CreatureStartedPtj.Raise(creature, type); }

		/// <summary>
		/// Raised when a creature completed a PTJ.
		/// </summary>
		public event Action<Creature, PtjType> CreatureCompletedPtj;
		public void OnCreatureCompletedPtj(Creature creature, PtjType type) { CreatureCompletedPtj.Raise(creature, type); }

		/// <summary>
		/// Raised when a creature aged.
		/// </summary>
		public event Action<Creature, int> CreatureAged;
		public void OnCreatureAged(Creature creature, int prevAge) { CreatureAged.Raise(creature, prevAge); }
	}

	public static class EventHandlerExtensions
	{
		/// <summary>
		/// Raises event with thread and null-ref safety.
		/// </summary>
		public static void Raise<T>(this Action<T> handler, T args)
		{
			if (handler != null)
				handler(args);
		}

		/// <summary>
		/// Raises event with thread and null-ref safety.
		/// </summary>
		public static void Raise<T1, T2>(this Action<T1, T2> handler, T1 args1, T2 args2)
		{
			if (handler != null)
				handler(args1, args2);
		}

		/// <summary>
		/// Raises event with thread and null-ref safety.
		/// </summary>
		public static void Raise<T1, T2, T3>(this Action<T1, T2, T3> handler, T1 args1, T2 args2, T3 args3)
		{
			if (handler != null)
				handler(args1, args2, args3);
		}
	}

	public class CollectEventArgs : EventArgs
	{
		public Creature Creature { get; set; }
		public CollectingData CollectData { get; set; }
		public bool Success { get; set; }
		public int ItemId { get; set; }

		public CollectEventArgs(Creature creature, CollectingData collectData, bool success, int itemId)
		{
			this.Creature = creature;
			this.CollectData = collectData;
			this.Success = success;
			this.ItemId = itemId;
		}
	}

	public class ProductionEventArgs : EventArgs
	{
		public Creature Creature { get; set; }
		public ProductionData ProductionData { get; set; }
		public bool Success { get; set; }
		public Item Item { get; set; }

		public ProductionEventArgs(Creature creature, ProductionData data, bool success, Item item)
		{
			this.Creature = creature;
			this.ProductionData = data;
			this.Success = success;
			this.Item = item;
		}
	}

	public class CreationEventArgs : EventArgs
	{
		public Creature Creature { get; set; }
		public CreationMethod Method { get; set; }
		public Item Item { get; set; }
		public SkillRank Rank { get; set; }

		public CreationEventArgs(Creature creature, CreationMethod method, Item item, SkillRank rank)
		{
			this.Creature = creature;
			this.Method = method;
			this.Item = item;
			this.Rank = rank;
		}
	}

	public class CookingEventArgs : EventArgs
	{
		public Creature Creature { get; set; }
		public RecipeData Recipe { get; set; }
		public bool Success { get; set; }
		public Item Item { get; set; }

		public CookingEventArgs(Creature creature, RecipeData recipe, bool success, Item item)
		{
			this.Creature = creature;
			this.Recipe = recipe;
			this.Success = success;
			this.Item = item;
		}
	}

	public enum CreationMethod
	{
		Tailoring,
		Blacksmithing,
	}
}
