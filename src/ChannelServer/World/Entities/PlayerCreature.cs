// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Dungeons;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Base class for creatures controlled by players.
	/// </summary>
	public abstract class PlayerCreature : Creature
	{
		private List<Entity> _visibleEntities = new List<Entity>();
		private object _lookAroundLock = new Object();

		/// <summary>
		/// Creature id, for creature database.
		/// </summary>
		public long CreatureId { get; set; }

		/// <summary>
		/// Server this creature exists on.
		/// </summary>
		public string Server { get; set; }

		/// <summary>
		/// Time at which the creature can be deleted.
		/// </summary>
		public DateTime DeletionTime { get; set; }

		/// <summary>
		/// Specifies whether to update visible creatures or not.
		/// </summary>
		public bool Watching { get; set; }

		/// <summary>
		/// Set to true if creature is supposed to be saved.
		/// </summary>
		public bool Save { get; set; }

		/// <summary>
		/// Player's CP, based on stats and skills.
		/// </summary>
		public override float CombatPower
		{
			get
			{
				var cp = 0f;

				cp += this.Skills.HighestSkillCp;
				cp += this.Skills.SecondHighestSkillCp * 0.5f;
				cp += this.LifeMaxBase;
				cp += this.ManaMaxBase * 0.5f;
				cp += this.StaminaMaxBase * 0.5f;
				cp += this.StrBase;
				cp += this.IntBase * 0.2f;
				cp += this.DexBase * 0.1f;
				cp += this.WillBase * 0.5f;
				cp += this.LuckBase * 0.1f;

				return cp;
			}
		}

		/// <summary>
		/// Creatures new PlayerCreature.
		/// </summary>
		public PlayerCreature()
		{
			this.Watching = true;
		}

		/// <summary>
		/// Instructs client to move to target location.
		/// Returns false if region doesn't exist.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public override bool Warp(int regionId, int x, int y)
		{
			if (regionId == this.RegionId && this.Region != Region.Limbo)
			{
				// The Tin cutscene at the very beginning requires a Warp to
				// fade away, otherwise the screen will stay black. However,
				// the cutscene in the first dungeon includes a jump to the
				// treasure room, where we can't warp, because that would drop
				// the treasure chest key. This is basically a hack until we
				// find a better way to properly get out of the Tin cutscene.
				if (this.Temp.CurrentCutscene == null || this.Region.IsDungeon)
				{
					this.Jump(x, y);
					return true;
				}
			}

			var targetRegion = ChannelServer.Instance.World.GetRegion(regionId);
			if (targetRegion == null)
			{
				Send.ServerMessage(this, "Warp failed, region doesn't exist.");
				Log.Error("PC.Warp: Region '{0}' doesn't exist.", regionId);
				return false;
			}

			var currentRegionId = this.RegionId;
			var loc = new Location(currentRegionId, this.GetPosition());

			this.LastLocation = loc;
			this.WarpLocation = new Location(regionId, x, y);
			this.Warping = true;
			this.Lock(Locks.Default, true);

			// TODO: We don't have to send the "create warps" every time,
			//   only when the player is warped there for the first time.

			// Dynamic Region warp
			var dynamicRegion = targetRegion as DynamicRegion;
			if (dynamicRegion != null)
			{
				if (!this.Region.IsTemp)
					this.FallbackLocation = loc;

				Send.EnterDynamicRegion(this, currentRegionId, targetRegion, x, y);

				return true;
			}

			// Dungeon warp
			var dungeonRegion = targetRegion as DungeonRegion;
			if (dungeonRegion != null)
			{
				if (!this.Region.IsTemp)
				{
					this.FallbackLocation = loc;
					this.DungeonSaveLocation = this.WarpLocation;
				}

				Send.DungeonInfo(this, dungeonRegion.Dungeon);

				return true;
			}

			// Normal warp
			Send.EnterRegion(this, regionId, x, y);

			return true;
		}

		/// <summary>
		/// Updates visible creatures, sends Entities(Dis)Appear.
		/// </summary>
		public void LookAround()
		{
			if (!this.Watching)
				return;

			lock (_lookAroundLock)
			{
				var currentlyVisible = this.Region.GetVisibleEntities(this);

				var appear = currentlyVisible.Except(_visibleEntities);
				var disappear = _visibleEntities.Except(currentlyVisible);

				Send.EntitiesAppear(this.Client, appear);
				Send.EntitiesDisappear(this.Client, disappear);

				_visibleEntities = currentlyVisible;
			}
		}

		/// <summary>
		/// Returns whether player can target the given creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public override bool CanTarget(Creature creature)
		{
			if (!base.CanTarget(creature))
				return false;

			// Players can only target "bad" NPCs.
			if (creature.Has(CreatureStates.GoodNpc))
				return false;

			// Players can't target players (outside of PvP, TODO)
			if (creature.IsPlayer)
				return false;

			return true;
		}

		/// <summary>
		/// Players survive when they had more than half of their life left.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="from"></param>
		/// <param name="lifeBefore"></param>
		/// <returns></returns>
		protected override bool ShouldSurvive(float damage, Creature from, float lifeBefore)
		{
			return (lifeBefore >= this.LifeMax / 2);
		}

		/// <summary>
		/// Aggroes target, setting target and putting creature in battle stance.
		/// </summary>
		/// <param name="creature"></param>
		public override void Aggro(Creature target)
		{
			this.IsInBattleStance = true;
		}
	}
}
