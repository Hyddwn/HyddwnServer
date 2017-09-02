// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Dungeons;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities
{
    /// <summary>
    ///     Base class for creatures controlled by players.
    /// </summary>
    public abstract class PlayerCreature : Creature
    {
        private readonly object _lookAroundLock = new object();
        private List<Entity> _visibleEntities = new List<Entity>();

        /// <summary>
        ///     Creatures new PlayerCreature.
        /// </summary>
        public PlayerCreature()
        {
            Watching = true;
        }

        /// <summary>
        ///     Creature id, for creature database.
        /// </summary>
        public long CreatureId { get; set; }

        /// <summary>
        ///     Server this creature exists on.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        ///     Specifies whether to update visible creatures or not.
        /// </summary>
        public bool Watching { get; set; }

        /// <summary>
        ///     Set to true if creature is supposed to be saved.
        /// </summary>
        public virtual bool Save { get; set; }

        /// <summary>
        ///     Player's CP, based on stats and skills.
        /// </summary>
        public override float CombatPower
        {
            get
            {
                var cp = 0f;

                cp += Skills.HighestSkillCp;
                cp += Skills.SecondHighestSkillCp * 0.5f;
                cp += LifeMaxBase;
                cp += ManaMaxBase * 0.5f;
                cp += StaminaMaxBase * 0.5f;
                cp += StrBase;
                cp += IntBase * 0.2f;
                cp += DexBase * 0.1f;
                cp += WillBase * 0.5f;
                cp += LuckBase * 0.1f;

                return cp;
            }
        }

        /// <summary>
        ///     Instructs client to move to target location.
        ///     Returns false if region doesn't exist.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Warp(int regionId, int x, int y)
        {
            if (regionId == RegionId && Region != Region.Limbo)
                if (Temp.CurrentCutscene == null || Region.IsDungeon)
                {
                    Jump(x, y);
                    return true;
                }

            var targetRegion = ChannelServer.Instance.World.GetRegion(regionId);
            if (targetRegion == null)
            {
                Send.ServerMessage(this, "Warp failed, region doesn't exist.");
                Log.Error("PC.Warp: Region '{0}' doesn't exist.", regionId);
                return false;
            }

            // When RP characters warp out of dungeons, their session ends.
            // This should happen for shadow missions as well once we get them.
            if (IsRpCharacter && Region.IsDungeon && !targetRegion.IsDungeon)
            {
                var rpCharacter = this as RpCharacter;
                rpCharacter.End();

                return true;
            }

            var currentRegionId = RegionId;
            var loc = new Location(currentRegionId, GetPosition());

            LastLocation = loc;
            WarpLocation = new Location(regionId, x, y);
            Warping = true;
            Lock(Locks.Default, true);

            // TODO: We don't have to send the "create warps" every time,
            //   only when the player is warped there for the first time.

            // Dynamic Region warp
            var dynamicRegion = targetRegion as DynamicRegion;
            if (dynamicRegion != null)
            {
                if (!Region.IsTemp)
                    FallbackLocation = loc;

                Send.EnterDynamicRegion(this, currentRegionId, targetRegion, x, y);

                return true;
            }

            // Dungeon warp
            var dungeonRegion = targetRegion as DungeonRegion;
            if (dungeonRegion != null)
            {
                if (!Region.IsTemp)
                {
                    FallbackLocation = loc;
                    DungeonSaveLocation = WarpLocation;
                }

                Send.DungeonInfo(this, dungeonRegion.Dungeon);

                return true;
            }

            // Normal warp
            Send.EnterRegion(this, regionId, x, y);

            return true;
        }

        /// <summary>
        ///     Updates visible creatures, sends Entities(Dis)Appear.
        /// </summary>
        public void LookAround()
        {
            if (!Watching)
                return;

            lock (_lookAroundLock)
            {
                var currentlyVisible = Region.GetVisibleEntities(this);

                var appear = currentlyVisible.Except(_visibleEntities);
                var disappear = _visibleEntities.Except(currentlyVisible);

                Send.EntitiesAppear(Client, appear);
                Send.EntitiesDisappear(Client, disappear);

                _visibleEntities = currentlyVisible;
            }
        }

        // TODO: Start using Start- and StopLookAround everywhere,
        //   to properly manage the visible entities. Right now we have
        //   hack-ish appear and disappear calls all over the place.

        /// <summary>
        ///     Starts auto-update of visible entities nearby, sending the first
        ///     list of visible entities right away.
        /// </summary>
        public void StartLookAround()
        {
            Watching = true;
            LookAround();
        }

        /// <summary>
        ///     Stops auto-update of visible entities nearby,
        ///     clearing all currently visible entities.
        /// </summary>
        public void StopLookAround()
        {
            Watching = false;

            lock (_lookAroundLock)
            {
                Send.EntitiesDisappear(Client, _visibleEntities);
                _visibleEntities.Clear();
            }
        }

        /// <summary>
        ///     Returns whether player can target the given creature.
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
        ///     Players survive when they had more than half of their life left.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="from"></param>
        /// <param name="lifeBefore"></param>
        /// <returns></returns>
        protected override bool ShouldSurvive(float damage, Creature from, float lifeBefore)
        {
            return lifeBefore >= LifeMax / 2;
        }

        /// <summary>
        ///     Aggroes target, setting target and putting creature in battle stance.
        /// </summary>
        /// <param name="creature"></param>
        public override void Aggro(Creature target)
        {
            IsInBattleStance = true;
        }
    }
}