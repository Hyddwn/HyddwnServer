// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Text;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities.Creatures;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using Aura.Shared.Util;
using Aura.Channel.Scripting;
using Aura.Channel.World.Inventory;
using Aura.Channel.Skills.Life;
using System.Collections.Generic;
using Aura.Channel.Skills;
using System.Threading;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Base class for all "creaturly" entities.
	/// </summary>
	public abstract class Creature : Entity, IDisposable
	{
		public const float BaseMagicBalance = 0.3f;
		public const float MinStability = -10, MaxStability = 100;

		private const float MinWeight = 0.7f, MaxWeight = 1.5f;
		private const float MaxFoodStatBonus = 100;
		private const float MaxStatBonus = 100;

		public const int BareHandStaminaUsage = 2;

		public const int MaxElementalAffinity = 9;

		private byte _inquiryId;
		private Dictionary<byte, Action<Creature>> _inquiryCallbacks;

		public override DataType DataType { get { return DataType.Creature; } }

		// General
		// ------------------------------------------------------------------

		public ChannelClient Client { get; set; }

		public string Name { get; set; }

		public CreatureStates State { get; set; }
		public CreatureStatesEx StateEx { get; set; }

		public int RaceId { get; set; }
		public RaceData RaceData { get; protected set; }

		public Creature Master { get; set; }
		public Creature Pet { get; set; }

		public CreatureTemp Temp { get; protected set; }
		public CreatureKeywords Keywords { get; protected set; }
		public CreatureTitles Titles { get; protected set; }
		public CreatureSkills Skills { get; protected set; }
		public CreatureRegen Regens { get; protected set; }
		public CreatureStatMods StatMods { get; protected set; }
		public CreatureConditions Conditions { get; protected set; }
		public CreatureQuests Quests { get; protected set; }
		public CreatureDrops Drops { get; protected set; }
		public CreatureDeadMenu DeadMenu { get; protected set; }
		public AimMeter AimMeter { get; protected set; }

		public int InventoryWidth { get; set; }
		public int InventoryHeight { get; set; }

		/// <summary>
		/// Temporary and permanent variables, exclusive to this creature.
		/// </summary>
		/// <remarks>
		/// Permanent variables are saved across relogs, if the creature
		/// is a player creature. NPCs and monster variables aren't saved.
		/// </remarks>
		public ScriptVariables Vars { get; protected set; }

		/// <summary>
		/// Returns true if creature is a Character or Pet.
		/// </summary>
		public bool IsPlayer { get { return (this.IsCharacter || this.IsPet); } }

		/// <summary>
		/// Returns true if creature is a character, i.e. a player creature,
		/// but not a pet/partner.
		/// </summary>
		public bool IsCharacter { get { return (this is Character); } }

		/// <summary>
		/// Returns true if creature is a pet.
		/// </summary>
		public bool IsPet { get { return (this is Pet); } }

		/// <summary>
		/// Returns true if creature is a partner, i.e. a pet with an entity
		/// id in a certain range.
		/// </summary>
		public bool IsPartner { get { return (this.IsPet && this.EntityId >= MabiId.Partners); } }

		/// <summary>
		/// Returns true if creature is a human, based on the race id.
		/// </summary>
		public bool IsHuman { get { return (this.RaceId == 10001 || this.RaceId == 10002); } }

		/// <summary>
		/// Returns true if creature is an elf, based on the race id.
		/// </summary>
		public bool IsElf { get { return (this.RaceId == 9001 || this.RaceId == 9002); } }

		/// <summary>
		/// Returns true if creature is a giant, based on the race id.
		/// </summary>
		public bool IsGiant { get { return (this.RaceId == 8001 || this.RaceId == 8002); } }

		/// <summary>
		/// Returns true if creature is male, based on its race data.
		/// </summary>
		public bool IsMale { get { return (this.RaceData != null && this.RaceData.Gender == Gender.Male); } }

		/// <summary>
		/// Returns true if creature is female, based on its race data.
		/// </summary>
		public bool IsFemale { get { return (this.RaceData != null && this.RaceData.Gender == Gender.Female); } }

		/// <summary>
		/// The region the creature is currently in.
		/// </summary>
		/// <remarks>
		/// During warps, this value is the region id of the previous region,
		/// only after the warp is done, it's set to the new region.
		/// </remarks>
		public override int RegionId { get; set; }

		/// <summary>
		/// Lock handler, for prohibiting the creature from doing certain things.
		/// </summary>
		public Locks Locks { get; protected set; }

		/// <summary>
		/// Returns whether creature is able to learn skills automatically
		/// (e.g. Counterattack).
		/// </summary>
		public virtual bool LearningSkillsEnabled { get { return false; } }

		/// <summary>
		/// Time at which the creature was created.
		/// </summary>
		public DateTime CreationTime { get; set; }

		/// <summary>
		/// Time of last rebirth.
		/// </summary>
		public DateTime LastRebirth { get; set; }

		/// <summary>
		/// Time of last aging.
		/// </summary>
		public DateTime LastAging { get; set; }

		/// <summary>
		/// Time of last login.
		/// </summary>
		public DateTime LastLogin { get; set; }

		/// <summary>
		/// How many times the character rebirthed.
		/// </summary>
		public int RebirthCount { get; set; }

		/// <summary>
		/// Returns true if creature has the devCAT title selected.
		/// </summary>
		public bool IsDev { get { return (this.Titles.SelectedTitle == TitleId.devCAT); } }

		/// <summary>
		/// Gets or sets the amount of "cash" points on the creature's
		/// account and updates the client accordingly.
		/// </summary>
		public int Points
		{
			get { return (this.Client.Account != null ? this.Client.Account.Points : 0); }
			set
			{
				if (this.Client.Account == null)
					return;

				var points = Math2.Clamp(0, int.MaxValue, value);
				this.Client.Account.Points = points;
				Send.PointsUpdate(this, points);
			}
		}

		// Look
		// ------------------------------------------------------------------

		public byte SkinColor { get; set; }
		public short EyeType { get; set; }
		public byte EyeColor { get; set; }
		public byte MouthType { get; set; }

		private float _weight, _upper, _lower;
		public float Height { get; set; }
		public float Weight { get { return _weight; } set { _weight = Math2.Clamp(MinWeight, MaxWeight, value); } }
		public float Upper { get { return _upper; } set { _upper = Math2.Clamp(MinWeight, MaxWeight, value); } }
		public float Lower { get { return _lower; } set { _lower = Math2.Clamp(MinWeight, MaxWeight, value); } }

		public float BodyScale { get { return (this.Height * 0.4f + 0.6f); } }

		public string StandStyle { get; set; }
		public string StandStyleTalking { get; set; }

		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }

		/// <summary>
		/// Returns body proportions
		/// </summary>
		public BodyProportions Body
		{
			get
			{
				BodyProportions result;
				result.Height = this.Height;
				result.Weight = this.Weight;
				result.Upper = this.Upper;
				result.Lower = this.Lower;
				return result;
			}
		}

		// Inventory
		// ------------------------------------------------------------------

		public CreatureInventory Inventory { get; protected set; }
		public Item RightHand { get { return this.Inventory.RightHand; } }
		public Item LeftHand { get { return this.Inventory.LeftHand; } }
		public Item Magazine { get { return this.Inventory.Magazine; } }
		public bool HandsFree { get { return (this.RightHand == null && this.LeftHand == null && this.Magazine == null); } }

		/// <summary>
		/// Returns whether the creature is wielding main weapons on both hands.
		/// Shields and similar items are not considered main weapons.
		/// </summary>
		public bool IsDualWielding { get { return this.RightHand != null && this.LeftHand != null && this.LeftHand.Data.WeaponType != 0; } }

		// Movement
		// ------------------------------------------------------------------

		private Position _position, _destination;
		private DateTime _moveStartTime;
		private double _moveDuration, _movementX, _movementY;

		public byte Direction { get; set; }
		public bool IsMoving { get { return (_position != _destination); } }
		public bool IsWalking { get; protected set; }
		public double MoveDuration { get { return _moveDuration; } }

		/// <summary>
		/// Location to warp to.
		/// </summary>
		public Location WarpLocation { get; set; }

		/// <summary>
		/// Location of the creature before the warp.
		/// </summary>
		public Location LastLocation { get; set; }

		/// <summary>
		/// Location to fall back to, when saving in a temp region.
		/// </summary>
		public Location FallbackLocation { get; set; }

		/// <summary>
		/// Location the player has saved at in a dungeon.
		/// </summary>
		public Location DungeonSaveLocation { get; set; }

		/// <summary>
		/// Event path to last visited town.
		/// </summary>
		public string LastTown
		{
			get { return (string.IsNullOrWhiteSpace(_lastTown) ? "Uladh_main/town_TirChonaill/TirChonaill_Spawn_A" : _lastTown); }
			set { _lastTown = value; }
		}
		private string _lastTown;

		/// <summary>
		/// True while character is warping somewhere.
		/// </summary>
		public bool Warping { get; set; }

		// Combat
		// ------------------------------------------------------------------

		protected bool _battleStance;
		/// <summary>
		/// Changes stance and broadcasts update.
		/// </summary>
		public bool IsInBattleStance { get { return _battleStance; } set { _battleStance = value; Send.ChangeStance(this); } }
		public Creature Target { get; set; }

		private int _stun;
		private DateTime _stunChange;
		/// <summary>
		/// Amount of ms before creature can do something again.
		/// </summary>
		/// <remarks>
		/// Max stun animation duration for monsters seems to be about 3s.
		/// </remarks>
		public int Stun
		{
			get
			{
				if (_stun <= 0)
					return 0;

				var diff = (DateTime.Now - _stunChange).TotalMilliseconds;
				if (diff < _stun)
					return (int)(_stun - diff);

				return (_stun = 0);
			}
			set
			{
				_stun = Math2.Clamp(0, short.MaxValue, value);
				_stunChange = DateTime.Now;
			}
		}

		private float _stability;
		private DateTime _stabilityChange;
		/// <summary>
		/// Creature's stability, once it goes below a certain value the creature
		/// becomes "unstable" and might have to be knocked back.
		/// </summary>
		/// <remarks>
		/// The more you hit a creature with heavy weapons,
		/// the higher this value. If it goes above 100 the creature
		/// is knocked back/down.
		/// This is also used for the knock down gauge.
		/// </remarks>
		public float Stability
		{
			get
			{
				if (_stability >= MaxStability)
					return MaxStability;

				var result = _stability + ((DateTime.Now - _stabilityChange).TotalMilliseconds / 60f);
				if (result >= MaxStability)
					result = _stability = MaxStability;

				return (float)result;
			}
			set
			{
				_stability = Math2.Clamp(MinStability, MaxStability, value);
				_stabilityChange = DateTime.Now;
			}
		}

		/// <summary>
		/// Returns true if Stability is lower than or equal to 0,
		/// at which point the creature should be knocked back.
		/// </summary>
		public bool IsUnstable { get { return (this.Stability <= 0); } }

		public bool IsDead { get { return this.Has(CreatureStates.Dead); } }
		public bool IsStunned { get { return (this.Stun > 0); } }

		public bool WasKnockedBack { get; set; }

		/// <summary>
		/// Returns average knock count of both equipped weapons, or race's
		/// if none are equipped.
		/// </summary>
		public int AverageKnockCount
		{
			get
			{
				var result = this.RaceData.KnockCount + 1;

				if (this.RightHand != null)
				{
					result = this.RightHand.Info.KnockCount + 1;
					if (this.LeftHand != null)
					{
						result += this.LeftHand.Info.KnockCount + 1;
						result /= 2;
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Returns weapon's attack speed or the race's if not weapon
		/// is equipped.
		/// </summary>
		public AttackSpeed AttackSpeed { get { return (this.RightHand != null ? this.RightHand.Data.AttackSpeed : (AttackSpeed)this.RaceData.AttackSpeed); } }

		/// <summary>
		/// Returns average attack speed of both equipped weapons, or race's
		/// if none are equipped.
		/// </summary>
		public AttackSpeed AverageAttackSpeed
		{
			get
			{
				var result = this.RaceData.AttackSpeed;

				if (this.RightHand != null)
				{
					result = (int)this.RightHand.Data.AttackSpeed;
					if (this.LeftHand != null)
					{
						result += (int)this.LeftHand.Data.AttackSpeed;
						result /= 2;
					}
				}

				return (AttackSpeed)result;
			}
		}

		/// <summary>
		/// Holds the time at which the knock down is over.
		/// </summary>
		public DateTime KnockDownTime { get; set; }

		/// <summary>
		/// Returns true if creature is currently knocked down.
		/// </summary>
		public bool IsKnockedDown { get { return (DateTime.Now < this.KnockDownTime); } }

		/// <summary>
		/// Returns true if creature is able to run while having ranged loaded,
		/// e.g. because its and elf or has a crossbow equipped.
		/// </summary>
		public bool CanRunWithRanged { get { return (this.IsElf || (this.RightHand != null && this.RightHand.HasTag("/crossbow/"))); } }

		public Dictionary<long, HitTracker> _hitTrackers;
		public int _totalHits;

		// Stats
		// ------------------------------------------------------------------

		public short Level { get; set; }
		public int TotalLevel { get; set; }
		public long Exp { get; set; }

		public short Age { get; set; }

		private short _ap;
		public short AbilityPoints { get { return _ap; } set { _ap = Math.Max((short)0, value); } }

		public virtual float CombatPower { get { return Math2.Clamp(1, 9999, (this.RaceData != null ? this.RaceData.CombatPower : 1) + this.StatMods.Get(Stat.CombatPowerMod)); } }

		public float StrBaseSkill { get; set; }
		public float DexBaseSkill { get; set; }
		public float IntBaseSkill { get; set; }
		public float WillBaseSkill { get; set; }
		public float LuckBaseSkill { get; set; }

		public float StrMod { get { return this.StatMods.Get(Stat.StrMod); } }
		public float DexMod { get { return this.StatMods.Get(Stat.DexMod); } }
		public float IntMod { get { return this.StatMods.Get(Stat.IntMod); } }
		public float WillMod { get { return this.StatMods.Get(Stat.WillMod); } }
		public float LuckMod { get { return this.StatMods.Get(Stat.LuckMod); } }

		public float StrBase { get; set; }
		public float DexBase { get; set; }
		public float IntBase { get; set; }
		public float WillBase { get; set; }
		public float LuckBase { get; set; }
		public float StrBaseTotal { get { return this.StrBase + this.StrBaseSkill + this.StrBonus; } }
		public float DexBaseTotal { get { return this.DexBase + this.DexBaseSkill + this.DexBonus; } }
		public float IntBaseTotal { get { return this.IntBase + this.IntBaseSkill + this.IntBonus; } }
		public float WillBaseTotal { get { return this.WillBase + this.WillBaseSkill + this.WillBonus; } }
		public float LuckBaseTotal { get { return this.LuckBase + this.LuckBaseSkill + this.LuckBonus; } }
		public float Str { get { return this.StrBaseTotal + this.StrMod + this.StrFoodMod; } }
		public float Dex { get { return this.DexBaseTotal + this.DexMod + this.DexFoodMod; } }
		public float Int { get { return this.IntBaseTotal + this.IntMod + this.IntFoodMod; } }
		public float Will { get { return this.WillBaseTotal + this.WillMod + this.WillFoodMod; } }
		public float Luck { get { return this.LuckBaseTotal + this.LuckMod + this.LuckFoodMod; } }

		/// <summary>
		/// Rate from monster.
		/// </summary>
		public int BalanceBase { get { return (this.RightHand == null ? this.RaceData.BalanceBase : 0); } }

		/// <summary>
		/// Rate from race.
		/// </summary>
		public int BalanceBaseMod { get { return (this.RightHand == null ? this.RaceData.BalanceBaseMod : 0); } }

		/// <summary>
		/// Balance bonus from enchants and other sources.
		/// </summary>
		public int BalanceMod { get { return (int)this.StatMods.Get(Stat.BalanceMod); } }

		/// <summary>
		/// Balance of right hand weapon.
		/// </summary>
		public int RightBalanceMod { get { return (this.RightHand != null ? this.RightHand.OptionInfo.Balance : 0); } }

		/// <summary>
		/// Balance of left hand weapon.
		/// </summary>
		public int LeftBalanceMod { get { return (this.LeftHand != null ? this.LeftHand.OptionInfo.Balance : 0); } }

		/// <summary>
		/// Critical from monster.
		/// </summary>
		public float CriticalBase { get { return (this.RightHand == null ? this.RaceData.CriticalBase : 0); } }

		/// <summary>
		/// Critical from race.
		/// </summary>
		public float CriticalBaseMod { get { return (this.RightHand == null ? this.RaceData.CriticalBaseMod : 0); } }

		/// <summary>
		/// Critical bonus from enchants and other sources.
		/// </summary>
		public int CriticalMod { get { return (int)this.StatMods.Get(Stat.CriticalMod); } }

		/// <summary>
		/// Critical of right hand weapon.
		/// </summary>
		public float RightCriticalMod { get { return (this.RightHand != null ? this.RightHand.OptionInfo.Critical : 0); } }

		/// <summary>
		/// Critical of left hand weapon.
		/// </summary>
		public float LeftCriticalMod { get { return (this.LeftHand != null ? this.LeftHand.OptionInfo.Critical : 0); } }

		/// <summary>
		/// AttMin from monster.
		/// </summary>
		/// <remarks>
		/// This seems to count towards the creature's damage even if a weapon
		/// is equipped. This assumption is based on the fact that Golems
		/// have a 0 attack weapon, that would make them do almost no damage.
		/// </remarks>
		public int AttackMinBase { get { return this.RaceData.AttackMinBase; } }

		/// <summary>
		/// AttMax from monster.
		/// </summary>
		/// <remarks>
		/// This seems to count towards the creature's damage even if a weapon
		/// is equipped. This assumption is based on the fact that Golems
		/// have a 0 attack weapon, that would make them do almost no damage.
		/// </remarks>
		public int AttackMaxBase { get { return this.RaceData.AttackMaxBase; } }

		/// <summary>
		/// AttackMin from race.
		/// </summary>
		public int AttackMinBaseMod { get { return (this.RightHand == null ? this.RaceData.AttackMinBaseMod : 0); } }

		/// <summary>
		/// AttackMax from race.
		/// </summary>
		public int AttackMaxBaseMod { get { return (this.RightHand == null ? this.RaceData.AttackMaxBaseMod : 0); } }

		/// <summary>
		/// Par_AttackMin from itemdb, for right hand weapon.
		/// </summary>
		/// <remarks>
		/// Officials differentiate between 1H and 2H (e.g. two-hand swords
		/// and bows) weapons, in that they don't contribute to the Right...
		/// and Left... properties, but ...BaseMod. While this makes sense,
		/// it adds unnecessary complexity, as the client will display the
		/// correct values the way we do it as well, since it simply uses
		/// the values to calculate the stats, independent of the kind of
		/// weapon you have equipped.
		/// </remarks>
		public int RightAttackMinMod { get { return (this.RightHand != null ? this.RightHand.OptionInfo.AttackMin : 0); } }

		/// <summary>
		/// Par_AttackMax from itemdb, for right hand weapon.
		/// </summary>
		public int RightAttackMaxMod { get { return (this.RightHand != null ? this.RightHand.OptionInfo.AttackMax : 0); } }

		/// <summary>
		/// Par_AttackMin from itemdb, for left hand weapon.
		/// </summary>
		public int LeftAttackMinMod { get { return (this.LeftHand != null ? this.LeftHand.OptionInfo.AttackMin : 0); } }

		/// <summary>
		/// Par_AttackMax from itemdb, for left hand weapon.
		/// </summary>
		public int LeftAttackMaxMod { get { return (this.LeftHand != null ? this.LeftHand.OptionInfo.AttackMax : 0); } }

		/// <summary>
		/// Used for title bonuses.
		/// </summary>
		public int AttackMinMod { get { return (int)this.StatMods.Get(Stat.AttackMinMod); } }

		/// <summary>
		/// Used for title bonuses.
		/// </summary>
		public int AttackMaxMod { get { return (int)this.StatMods.Get(Stat.AttackMaxMod); } }

		/// <summary>
		/// WAttMin from monster.
		/// </summary>
		public int InjuryMinBase { get { return this.RaceData.InjuryMinBase; } }

		/// <summary>
		/// WAttMax from monster.
		/// </summary>
		public int InjuryMaxBase { get { return this.RaceData.InjuryMaxBase; } }

		/// <summary>
		/// WAttackMin from race.
		/// </summary>
		public int InjuryMinBaseMod { get { return this.RaceData.InjuryMinBaseMod; } }

		/// <summary>
		/// WAttackMax from race.
		/// </summary>
		public int InjuryMaxBaseMod { get { return this.RaceData.InjuryMaxBaseMod; } }

		/// <summary>
		/// Par_WAttackMin from itemdb.
		/// </summary>
		public int RightInjuryMinMod { get { return (this.RightHand != null ? this.RightHand.OptionInfo.InjuryMin : 0); } }

		/// <summary>
		/// Par_WAttackMax from itemdb.
		/// </summary>
		public int RightInjuryMaxMod { get { return (this.RightHand != null ? this.RightHand.OptionInfo.InjuryMax : 0); } }

		/// <summary>
		/// Par_WAttackMin from itemdb.
		/// </summary>
		public int LeftInjuryMinMod { get { return (this.LeftHand != null ? this.LeftHand.OptionInfo.InjuryMin : 0); } }

		/// <summary>
		/// Par_WAttackMax from itemdb.
		/// </summary>
		public int LeftInjuryMaxMod { get { return (this.LeftHand != null ? this.LeftHand.OptionInfo.InjuryMax : 0); } }

		/// <summary>
		/// Title bonuses?
		/// </summary>
		public int InjuryMinMod { get { return (int)this.StatMods.Get(Stat.InjuryMinMod); } }

		/// <summary>
		/// Title bonuses?
		/// </summary>
		public int InjuryMaxMod { get { return (int)this.StatMods.Get(Stat.InjuryMaxMod); } }

		/// <summary>
		/// Returns total min injury.
		/// </summary>
		public int InjuryMin
		{
			get
			{
				var result = ((this.Dex - 10) * 0.05f) + ((this.Will - 10) * 0.05f);
				result += this.InjuryMinBase;
				result += this.InjuryMinBaseMod;
				result += this.InjuryMinMod;

				// Add average of both weapons
				if (this.RightHand != null)
				{
					var weapons = (float)RightInjuryMinMod;
					if (this.LeftHand != null)
						weapons = (weapons + this.LeftInjuryMinMod) / 2;

					result += weapons;
				}

				return (int)Math2.Clamp(0, 100, result);
			}
		}

		/// <summary>
		/// Returns total max injury.
		/// </summary>
		/// <remarks>
		/// Something is missing in this function, the max injury rate shown on
		/// the client tends to be *slightly* different, in the range of +-1~4.
		/// </remarks>
		public int InjuryMax
		{
			get
			{
				var result = ((this.Dex - 10) * 0.1f) + ((this.Will - 10) * 0.2f);
				result += this.InjuryMaxBase;
				result += this.InjuryMaxBaseMod;
				result += this.InjuryMaxMod;

				// Add average of both weapons
				if (this.RightHand != null)
				{
					var weapons = (float)RightInjuryMaxMod;
					if (this.LeftHand != null)
						weapons = (weapons + this.LeftInjuryMaxMod) / 2;

					result += weapons;
				}

				return (int)Math2.Clamp(0, 100, result);
			}
		}

		/// <summary>
		/// Returns total magic attack, based on stats, equipment, etc.
		/// </summary>
		public float MagicAttack
		{
			get
			{
				var result = (Math.Max(0, this.Int - 10) / 5f);

				// TODO: Bonuses

				return result;
			}
		}

		/// <summary>
		/// MagicAttack bonus from enchants and other sources.
		/// </summary>
		public int MagicAttackMod { get { return (int)this.StatMods.Get(Stat.MagicAttackMod); } }

		/// <summary>
		/// Returns total magic defense, based on stats, equipment, etc.
		/// </summary>
		public float MagicDefense
		{
			get
			{
				var result = (Math.Max(0, this.Will - 10) / 5f);

				// TODO: Bonuses

				return result;
			}
		}

		/// <summary>
		/// MagicDefense bonus from enchants and other sources.
		/// </summary>
		public int MagicDefenseMod { get { return (int)this.StatMods.Get(Stat.MagicDefenseMod); } }

		/// <summary>
		/// Returns total magic protection, based on stats, equipment, etc.
		/// </summary>
		public float MagicProtection
		{
			get
			{
				var result = (Math.Max(0, this.Int - 10) / 20f);

				// TODO: Bonuses

				return result;
			}
		}

		/// <summary>
		/// MagicProtection bonus from enchants and other sources.
		/// </summary>
		public int MagicProtectionMod { get { return (int)this.StatMods.Get(Stat.MagicProtectionMod); } }

		/// <summary>
		/// Creature's affinity to the element lightning.
		/// </summary>
		public int ElementLightning { get { return (this.RaceData.ElementLightning + (int)this.StatMods.Get(Stat.ElementLightning)); } }

		/// <summary>
		/// Creature's affinity to the element fire.
		/// </summary>
		public int ElementFire { get { return (this.RaceData.ElementFire + (int)this.StatMods.Get(Stat.ElementFire)); } }

		/// <summary>
		/// Creature's affinity to the element ice.
		/// </summary>
		public int ElementIce { get { return (this.RaceData.ElementIce + (int)this.StatMods.Get(Stat.ElementIce)); } }

		// Food Mods
		// ------------------------------------------------------------------

		private float _lifeFoodMod, _manaFoodMod, _staminaFoodMod;
		private float _strFoodMod, _intFoodMod, _dexFoodMod, _willFoodMod, _luckFoodMod;

		public float LifeFoodMod { get { return _lifeFoodMod; } set { _lifeFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float ManaFoodMod { get { return _manaFoodMod; } set { _manaFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float StaminaFoodMod { get { return _staminaFoodMod; } set { _staminaFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float StrFoodMod { get { return _strFoodMod; } set { _strFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float IntFoodMod { get { return _intFoodMod; } set { _intFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float DexFoodMod { get { return _dexFoodMod; } set { _dexFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float WillFoodMod { get { return _willFoodMod; } set { _willFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }
		public float LuckFoodMod { get { return _luckFoodMod; } set { _luckFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value); } }

		// Stat Bonuses
		// ------------------------------------------------------------------

		private float _strBonus, _intBonus, _dexBonus, _willBonus, _luckBonus;

		public float StrBonus { get { return _strBonus; } set { _strBonus = Math2.Clamp(0, MaxStatBonus, value); } }
		public float IntBonus { get { return _intBonus; } set { _intBonus = Math2.Clamp(0, MaxStatBonus, value); } }
		public float DexBonus { get { return _dexBonus; } set { _dexBonus = Math2.Clamp(0, MaxStatBonus, value); } }
		public float WillBonus { get { return _willBonus; } set { _willBonus = Math2.Clamp(0, MaxStatBonus, value); } }
		public float LuckBonus { get { return _luckBonus; } set { _luckBonus = Math2.Clamp(0, MaxStatBonus, value); } }

		// Defense/Protection
		// ------------------------------------------------------------------

		/// <summary>
		/// Defense from monster xml
		/// </summary>
		public int DefenseBase { get { return this.RaceData.Defense; } }
		public int DefenseBaseMod { get { return (int)this.StatMods.Get(Stat.DefenseBaseMod); } } // Skills, Titles, etc?
		public int DefenseMod { get { return (int)this.StatMods.Get(Stat.DefenseMod); } } // eg Reforging? (yellow)
		public int Defense
		{
			get
			{
				var result = this.DefenseBase + this.DefenseBaseMod + this.DefenseMod;

				// Str defense is displayed automatically on the client side
				result += (int)Math.Max(0, (this.Str - 10f) / 10f);

				// Add bonus for healing
				if (this.Skills.IsActive(SkillId.Healing))
				{
					if (this.Skills.ActiveSkill.Stacks >= 1) result += 5;
					if (this.Skills.ActiveSkill.Stacks >= 2) result += 3;
					if (this.Skills.ActiveSkill.Stacks >= 3) result += 4;
					if (this.Skills.ActiveSkill.Stacks >= 4) result += 4;
					if (this.Skills.ActiveSkill.Stacks >= 5) result += 4;
				}

				return result;
			}
		}

		/// <summary>
		/// Protect from monster xml
		/// </summary>
		public float ProtectionBase { get { return this.RaceData.Protection; } }
		public float ProtectionBaseMod { get { return this.StatMods.Get(Stat.ProtectionBaseMod); } }
		public float ProtectionMod { get { return this.StatMods.Get(Stat.ProtectionMod); } }
		public float Protection
		{
			get
			{
				var result = this.ProtectionBase + this.ProtectionBaseMod + this.ProtectionMod;

				return (result / 100f);
			}
		}

		public short ArmorPierce { get { return (short)Math.Max(0, ((this.Dex - 10) / 15)); } }

		// Life
		// ------------------------------------------------------------------

		private float _life, _injuries;
		public float Life
		{
			get { return _life; }
			set
			{
				var before = _life;

				_life = Math2.Clamp(-this.LifeMax, this.LifeInjured, value);

				if (this.Region != Region.Limbo)
				{
					if ((_life < 0 && before >= 0) && !this.Conditions.Has(ConditionsA.Deadly))
						this.Conditions.Activate(ConditionsA.Deadly);
					else if ((_life >= 0 && before < 0) && this.Conditions.Has(ConditionsA.Deadly))
						this.Conditions.Deactivate(ConditionsA.Deadly);
				}
			}
		}
		public float Injuries
		{
			get { return _injuries; }
			set { _injuries = Math2.Clamp(0, this.LifeMax, value); }
		}
		public float LifeMaxBase { get; set; }
		public float LifeMaxBaseSkill { get; set; }
		public float LifeMaxBaseTotal { get { return this.LifeMaxBase + this.LifeMaxBaseSkill; } }
		public float LifeMaxMod { get { return this.StatMods.Get(Stat.LifeMaxMod); } }
		public float LifeMax { get { return Math.Max(1, this.LifeMaxBaseTotal + this.LifeMaxMod + this.LifeFoodMod); } }
		public float LifeInjured { get { return this.LifeMax - _injuries; } }

		// Mana
		// ------------------------------------------------------------------

		private float _mana;
		public float Mana
		{
			get { return _mana; }
			set { _mana = Math2.Clamp(0, this.ManaMax, value); }
		}
		public float ManaMaxBase { get; set; }
		public float ManaMaxBaseSkill { get; set; }
		public float ManaMaxBaseTotal { get { return this.ManaMaxBase + this.ManaMaxBaseSkill; } }
		public float ManaMaxMod { get { return this.StatMods.Get(Stat.ManaMaxMod); } }
		public float ManaMax { get { return Math.Max(1, ManaMaxBaseTotal + this.ManaMaxMod + this.ManaFoodMod); } }

		// Stamina
		// ------------------------------------------------------------------

		private float _stamina, _hunger;
		public float Stamina
		{
			get { return _stamina; }
			set { _stamina = Math2.Clamp(0, this.StaminaMax, value); }
		}
		/// <summary>
		/// The amount of stamina that's not usable because of hunger.
		/// </summary>
		/// <remarks>
		/// While regen is limited to 50%, hunger can actually go higher.
		/// </remarks>
		public float Hunger
		{
			get { return _hunger; }
			set { _hunger = Math2.Clamp(0, this.StaminaMax, value); }
		}
		public float StaminaMaxBase { get; set; }
		public float StaminaMaxBaseSkill { get; set; }
		public float StaminaMaxBaseTotal { get { return this.StaminaMaxBase + this.StaminaMaxBaseSkill; } }
		public float StaminaMaxMod { get { return this.StatMods.Get(Stat.StaminaMaxMod); } }
		public float StaminaMax { get { return Math.Max(1, this.StaminaMaxBaseTotal + this.StaminaMaxMod + this.StaminaFoodMod); } }
		public float StaminaHunger { get { return this.StaminaMax - _hunger; } }

		/// <summary>
		/// Returns multiplicator to be used when regenerating stamina.
		/// </summary>
		public float StaminaRegenMultiplicator { get { return (this.Stamina < this.StaminaHunger ? 1f : 0.2f); } }

		/// <summary>
		/// Returns the rigth hand weapon stamina usage, or bare hand stamina usage if no such weapon.
		/// </summary>
		public float RightHandStaminaUsage { get { return this.RightHand != null ? this.RightHand.Data.StaminaUsage : BareHandStaminaUsage; } }

		/// <summary>
		/// Returns the left hand weapon stamina usage if the creature is dual wielding, 0 otherwise.
		/// <seealso cref="Creature.IsDualWielding"/>
		/// </summary>
		public float LeftHandStaminaUsage { get { return this.IsDualWielding ? this.LeftHand.Data.StaminaUsage : 0; } }

		// Events
		// ------------------------------------------------------------------

		/// <summary>
		/// Raised when creature dies.
		/// </summary>
		public event Action<Creature, Creature> Death;

		/// <summary>
		/// Raised when creature levels up.
		/// </summary>
		/// <remarks>
		/// Raised only once, even if there are multiple level ups.
		/// 
		/// Parameters:
		/// - The creature leveling up.
		/// - The level before the level up process.
		/// </remarks>
		public event Action<Creature, int> LeveledUp;

		// Parties
		// ------------------------------------------------------------------

		/// <summary>
		/// The party the creature is a part of. If creature is not in a party,
		/// a dummy party is created, consisting of only the creature.
		/// </summary>
		public Party Party { get; set; }

		/// <summary>
		/// The number in the party this player occupies.
		/// </summary>
		public int PartyPosition { get; set; }

		/// <summary>
		/// Returns true if creature is in an actual party.
		/// </summary>
		public bool IsInParty { get { return this.Party.Id != 0; } }

		// ------------------------------------------------------------------

		protected Creature()
		{
			this.Client = new DummyClient();

			this.Temp = new CreatureTemp();
			this.Titles = new CreatureTitles(this);
			this.Keywords = new CreatureKeywords(this);
			this.Regens = new CreatureRegen(this);
			this.Skills = new CreatureSkills(this);
			this.StatMods = new CreatureStatMods(this);
			this.Conditions = new CreatureConditions(this);
			this.Quests = new CreatureQuests(this);
			this.Drops = new CreatureDrops(this);
			this.DeadMenu = new CreatureDeadMenu(this);
			this.AimMeter = new AimMeter(this);
			this.Party = Party.CreateDummy(this);
			this.Inventory = new CreatureInventory(this);

			this.Vars = new ScriptVariables();

			_inquiryCallbacks = new Dictionary<byte, Action<Creature>>();
			_hitTrackers = new Dictionary<long, HitTracker>();
		}

		/// <summary>
		/// Loads race and handles some basic stuff, like adding regens.
		/// </summary>
		/// <remarks>
		/// Has to be called before anything is actually done with the creature,
		/// as it initializes the race data, the inventory, and other vital
		/// parts.
		/// </remarks>
		public virtual void LoadDefault()
		{
			if (this.RaceId == 0)
				throw new Exception("Set race before calling LoadDefault.");

			this.RaceData = AuraData.RaceDb.Find(this.RaceId);
			if (this.RaceData == null)
			{
				// Try to default to Human
				this.RaceData = AuraData.RaceDb.Find(10000);
				if (this.RaceData == null)
					throw new Exception("Unable to load race data, race '" + this.RaceId.ToString() + "' not found.");

				Log.Warning("Creature.LoadDefault: Race '{0}' not found, using human instead.", this.RaceId);
			}

			if (this.InventoryWidth == 0)
			{
				this.InventoryWidth = this.RaceData.InventoryWidth;
			}

			if (this.InventoryHeight == 0)
			{
				this.InventoryHeight = this.RaceData.InventoryHeight;
			}

			// Add inventory
			this.Inventory.AddMainInventory();
			this.Inventory.StartLiveUpdate();

			// Add regens
			// The wiki says it's 0.125 life, but the packets have 0.12.
			this.Regens.Add(Stat.Life, 0.12f, this.LifeMax);
			this.Regens.Add(Stat.Mana, 0.05f, this.ManaMax);
			this.Regens.Add(Stat.Stamina, 0.4f, this.StaminaMax);
			if (ChannelServer.Instance.Conf.World.EnableHunger)
				this.Regens.Add(Stat.Hunger, 0.01f, this.StaminaMax);

			// Subscribe to time events
			ChannelServer.Instance.Events.SecondsTimeTick += this.OnSecondsTimeTick;
			ChannelServer.Instance.Events.MabiTick += this.OnMabiTick;
		}

		/// <summary>
		/// Called when creature is removed from the server.
		/// (Killed NPC, disconnect, etc)
		/// </summary>
		public virtual void Dispose()
		{
			ChannelServer.Instance.Events.SecondsTimeTick -= this.OnSecondsTimeTick;
			ChannelServer.Instance.Events.MabiTick -= this.OnMabiTick;

			// Stop rest, so character doesn't appear sitting anymore
			// and chair props are removed.
			// Do this in dispose because we can't expect a clean logout.
			if (this.Has(CreatureStates.SitDown))
			{
				var restHandler = ChannelServer.Instance.SkillManager.GetHandler<Rest>(SkillId.Rest);
				if (restHandler != null)
					restHandler.Stop(this, this.Skills.Get(SkillId.Rest));
			}

			// Cancel any active skills
			if (this.Skills.ActiveSkill != null)
				this.Skills.CancelActiveSkill();
		}

		public void Activate(CreatureStates state) { this.State |= state; }
		public void Activate(CreatureStatesEx state) { this.StateEx |= state; }
		public void Deactivate(CreatureStates state) { this.State &= ~state; }
		public void Deactivate(CreatureStatesEx state) { this.StateEx &= ~state; }
		public bool Has(CreatureStates state) { return ((this.State & state) != 0); }
		public bool Is(RaceStands stand) { return ((this.RaceData.Stand & stand) != 0); }

		/// <summary>
		/// Returns current position.
		/// </summary>
		/// <returns></returns>
		public override Position GetPosition()
		{
			if (!this.IsMoving)
				return _position;

			var passed = (DateTime.Now - _moveStartTime).TotalSeconds;
			if (passed >= _moveDuration)
				return this.SetPosition(_destination.X, _destination.Y);

			var xt = _position.X + (_movementX * passed);
			var yt = _position.Y + (_movementY * passed);

			return new Position((int)xt, (int)yt);
		}

		/// <summary>
		/// Sets region, x, and y, to be near entity.
		/// Also randomizes direction.
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="range"></param>
		public void SetLocationNear(Entity entity, int range)
		{
			var rnd = RandomProvider.Get();
			var pos = entity.GetPosition();
			var target = pos.GetRandomInRange(range, rnd);
			var dir = (byte)rnd.Next(256);

			this.SetLocation(entity.RegionId, target.X, target.Y);
			this.Direction = dir;
		}

		/// <summary>
		/// Returns current destination.
		/// </summary>
		/// <returns></returns>
		public Position GetDestination()
		{
			return _destination;
		}

		/// <summary>
		/// Sets position and destination.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Position SetPosition(int x, int y)
		{
			return _position = _destination = new Position(x, y);
		}

		/// <summary>
		/// Sets RegionId and position.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void SetLocation(int region, int x, int y)
		{
			this.RegionId = region;
			this.SetPosition(x, y);
		}

		/// <summary>
		/// Sets RegionId and position.
		/// </summary>
		/// <param name="loc"></param>
		public void SetLocation(Location loc)
		{
			this.SetLocation(loc.RegionId, loc.X, loc.Y);
		}

		/// <summary>
		/// Starts movement from current position to destination.
		/// Sends Running|Walking.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="walking"></param>
		public void Move(Position destination, bool walking)
		{
			_position = this.GetPosition();
			_destination = destination;
			_moveStartTime = DateTime.Now;
			this.IsWalking = walking;

			var diffX = _destination.X - _position.X;
			var diffY = _destination.Y - _position.Y;
			_moveDuration = Math.Sqrt(diffX * diffX + diffY * diffY) / this.GetSpeed();
			_movementX = diffX / _moveDuration;
			_movementY = diffY / _moveDuration;

			this.Direction = MabiMath.DirectionToByte(_movementX, _movementY);

			Send.Move(this, _position, _destination, walking);
		}

		/// <summary>
		/// Returns current movement speed (x/s).
		/// </summary>
		/// <returns></returns>
		public float GetSpeed()
		{
			var speed = (!this.IsWalking ? this.RaceData.RunningSpeed : this.RaceData.WalkingSpeed);

			// RaceSpeedFactor
			if (!this.IsWalking)
				speed *= this.RaceData.RunSpeedFactor;

			// Hurry condition
			var hurry = this.Conditions.GetExtraVal(169);
			speed *= 1 + (hurry / 100f);

			return speed;
		}

		/// <summary>
		/// Stops movement, returning new current position.
		/// Sends Force(Walk|Run)To.
		/// </summary>
		/// <returns></returns>
		public Position StopMove()
		{
			if (!this.IsMoving)
				return _position;

			var pos = this.GetPosition();
			this.SetPosition(pos.X, pos.Y);

			if (this.IsWalking)
				Send.ForceWalkTo(this, pos);
			else
				Send.ForceRunTo(this, pos);

			return pos;
		}

		/// <summary>
		/// Stops movement and resets creature to the given position.
		/// </summary>
		/// <param name="pos"></param>
		public void ResetPosition(Position pos)
		{
			this.SetPosition(pos.X, pos.Y);
			Send.SetLocation(this, pos.X, pos.Y);
		}

		/// <summary>
		/// Warps creature to target location,
		/// returns false if warp is unsuccessful.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public abstract bool Warp(int regionId, int x, int y);

		/// <summary>
		/// Warps creature to target location,
		/// returns false if warp is unsuccessful.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		public bool Warp(Location loc)
		{
			return this.Warp(loc.RegionId, loc.X, loc.Y);
		}

		/// <summary>
		/// Warps creature to target location,
		/// returns false if warp is unsuccessful.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public bool Warp(int regionId, Position position)
		{
			return this.Warp(regionId, position.X, position.Y);
		}

		/// <summary>
		/// Warps creature to target location path,
		/// returns false if warp is unsuccessful.
		/// </summary>
		/// <remarks>
		/// Parses location paths like Uladh_main/SomeArea/SomeEvent
		/// and calls Warp with the resulting values.
		/// </remarks>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool Warp(string locationPath)
		{
			try
			{
				var loc = new Location(locationPath);
				return this.Warp(loc.RegionId, loc.X, loc.Y);
			}
			catch (Exception ex)
			{
				Send.ServerMessage(this, "Warp error: {0}", ex.Message);
				Log.Exception(ex, "Creature.Warp: Location parse error for '{0}'.", locationPath);
				return false;
			}
		}

		/// <summary>
		/// Warps creature to given coordinates in its current region.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Jump(int x, int y)
		{
			this.SetPosition(x, y);
			Send.SetLocation(this, x, y);

			// TODO: Warp followers?
		}

		/// <summary>
		/// Warps creature to given coordinates in its current region.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Jump(Position position)
		{
			this.Jump(position.X, position.Y);
		}

		/// <summary>
		/// Called every 5 minutes, checks changes through food.
		/// </summary>
		/// <param name="time"></param>
		public void OnMabiTick(ErinnTime time)
		{
			this.UpdateBody();
			this.EquipmentDecay();
		}

		/// <summary>
		/// Called once per second, running updates on different
		/// creature components.
		/// </summary>
		/// <param name="obj"></param>
		private void OnSecondsTimeTick(ErinnTime time)
		{
			// TODO: General creature components in a list, with Update interface?
			this.Regens.OnSecondsTimeTick(time);
			this.StatMods.OnSecondsTimeTick(time);
		}

		/// <summary>
		/// Called regularly to reduce equipments durability.
		/// </summary>
		/// <remarks>
		/// http://wiki.mabinogiworld.com/view/Durability#Per_Tick
		/// The loss actually doesn't seem to be fixed, I've logged
		/// varying values on NA. However, *most* of the time the
		/// values below are used.
		/// </remarks>
		private void EquipmentDecay()
		{
			if (ChannelServer.Instance.Conf.World.NoDurabilityLoss)
				return;

			var equipment = this.Inventory.GetEquipment();
			var update = new List<Item>();
			var loss = 0;

			foreach (var item in equipment.Where(a => a.Durability > 0))
			{
				// Going by the name, I assume items with this tag don't lose
				// durability regularly.
				if (item.HasTag("/no_abrasion/"))
					continue;

				switch (item.Info.Pocket)
				{
					case Pocket.Head: loss = 3; break;
					case Pocket.Armor: loss = 16; break; // 6
					case Pocket.Shoe: loss = 14; break; // 13
					case Pocket.Glove: loss = 10; break; // 9
					case Pocket.Robe: loss = 10; break;

					case Pocket.RightHand1:
						if (this.Inventory.WeaponSet != WeaponSet.First)
							continue;
						loss = 3;
						break;
					case Pocket.RightHand2:
						if (this.Inventory.WeaponSet != WeaponSet.Second)
							continue;
						loss = 3;
						break;

					case Pocket.LeftHand1:
						if (this.Inventory.WeaponSet != WeaponSet.First)
							continue;
						loss = 6;
						break;
					case Pocket.LeftHand2:
						if (this.Inventory.WeaponSet != WeaponSet.Second)
							continue;
						loss = 6;
						break;

					default:
						continue;
				}

				// Half dura loss if blessed
				if (item.IsBlessed)
					loss = Math.Max(1, loss / 2);

				item.Durability -= loss;
				update.Add(item);
			}

			if (update.Count != 0)
				Send.ItemDurabilityUpdate(this, update);
		}

		/// <summary>
		/// Called regularly to update body, based on what the creature ate.
		/// </summary>
		private void UpdateBody()
		{
			var weight = this.Temp.WeightFoodChange;
			var upper = this.Temp.UpperFoodChange;
			var lower = this.Temp.LowerFoodChange;
			var life = this.Temp.LifeFoodChange;
			var mana = this.Temp.ManaFoodChange;
			var stm = this.Temp.StaminaFoodChange;
			var str = this.Temp.StrFoodChange;
			var int_ = this.Temp.IntFoodChange;
			var dex = this.Temp.DexFoodChange;
			var will = this.Temp.WillFoodChange;
			var luck = this.Temp.LuckFoodChange;
			var changes = false;

			var sb = new StringBuilder();

			if (ChannelServer.Instance.Conf.World.YouAreWhatYouEat)
			{
				if (weight != 0)
				{
					changes = true;
					this.Weight += weight;
					sb.Append(weight > 0 ? Localization.Get("You gained some weight.") : Localization.Get("You lost some weight.") + "\r\n");
				}

				if (upper != 0)
				{
					changes = true;
					this.Upper += upper;
					sb.Append(upper > 0 ? Localization.Get("Your upper body got bigger.") : Localization.Get("Your upper body got slimmer.") + "\r\n");
				}

				if (lower != 0)
				{
					changes = true;
					this.Lower += lower;
					sb.Append(lower > 0 ? Localization.Get("Your legs got bigger.") : Localization.Get("Your legs got slimmer.") + "\r\n");
				}
			}

			if (life != 0)
			{
				changes = true;
				this.LifeFoodMod += life;
			}

			if (mana != 0)
			{
				changes = true;
				this.ManaFoodMod += mana;
			}

			if (stm != 0)
			{
				changes = true;
				this.StaminaFoodMod += stm;
			}

			if (str != 0)
			{
				changes = true;
				this.StrFoodMod += str;
			}

			if (int_ != 0)
			{
				changes = true;
				this.IntFoodMod += int_;
			}

			if (dex != 0)
			{
				changes = true;
				this.DexFoodMod += dex;
			}

			if (will != 0)
			{
				changes = true;
				this.WillFoodMod += will;
			}

			if (luck != 0)
			{
				changes = true;
				this.LuckFoodMod += luck;
			}

			if (!changes)
				return;

			this.Temp.WeightFoodChange = 0;
			this.Temp.UpperFoodChange = 0;
			this.Temp.LowerFoodChange = 0;
			this.Temp.LifeFoodChange = 0;
			this.Temp.ManaFoodChange = 0;
			this.Temp.StaminaFoodChange = 0;
			this.Temp.StrFoodChange = 0;
			this.Temp.IntFoodChange = 0;
			this.Temp.DexFoodChange = 0;
			this.Temp.WillFoodChange = 0;
			this.Temp.LuckFoodChange = 0;

			Send.StatUpdate(this, StatUpdateType.Private, Stat.LifeMaxFoodMod, Stat.ManaMaxFoodMod, Stat.StaminaMaxFoodMod, Stat.StrFoodMod, Stat.IntFoodMod, Stat.DexFoodMod, Stat.WillFoodMod, Stat.LuckFoodMod);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.LifeMaxFoodMod);
			Send.CreatureBodyUpdate(this);

			if (sb.Length > 0)
				Send.Notice(this, sb.ToString());
		}

		/// <summary>
		/// Returns true if creature is able to attack this creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public virtual bool CanTarget(Creature creature)
		{
			if (this.IsDead || creature.IsDead || creature == this)
				return false;

			return true;
		}

		/// <summary>
		/// Returns the max distance the creature can have to attack.
		/// </summary>
		/// <remarks>
		/// This might not be the 100% accurate formula, but it should be
		/// good enough to work with for now.
		/// </remarks>
		/// <param name="target"></param>
		/// <returns></returns>
		public int AttackRangeFor(Creature target)
		{
			var attackerRange = this.RaceData.AttackRange * this.BodyScale;
			var targetRange = target.RaceData.AttackRange * target.BodyScale;

			var result = 156f; // Default found in the client (for reference)

			if ((attackerRange < 300 && targetRange < 300) || (attackerRange >= 300 && attackerRange > targetRange))
				result = ((attackerRange + targetRange) / 2);
			else
				result = targetRange;

			// A little something extra
			result += 25;

			return (int)result;
		}

		/// <summary>
		/// Calculates random damage for the right hand (default).
		/// </summary>
		/// <remarks>
		/// Method is used for bare hand attacks as well, if right hand is
		/// empty, use bare hand attack values.
		/// 
		/// Uses bare hand damage if creature has 0 stamina.
		/// </remarks>
		/// <returns></returns>
		public virtual float GetRndRightHandDamage()
		{
			var min = this.AttackMinBase + this.AttackMinBaseMod;
			var max = this.AttackMaxBase + this.AttackMaxBaseMod;
			var balance = this.BalanceBase + this.BalanceBaseMod + this.RightBalanceMod;

			// Add potential weapon dmg only if creature has enough stamina
			if (this.RightHand != null && this.Stamina >= this.RightHand.Data.StaminaUsage)
			{
				min += this.RightAttackMinMod;
				max += this.RightAttackMaxMod;
			}

			return this.GetRndDamage(min, max, balance);
		}

		/// <summary>
		/// Calculates random damage for the left hand (off-hand).
		/// </summary>
		/// <remarks>
		/// Uses bare hand damage if creature has 0 stamina.
		/// </remarks>
		/// <returns></returns>
		public virtual float GetRndLeftHandDamage()
		{
			var min = this.AttackMinBase + this.AttackMinBaseMod;
			var max = this.AttackMaxBase + this.AttackMaxBaseMod;
			var balance = this.BalanceBase + this.BalanceBaseMod + this.LeftBalanceMod;

			// Add potential weapon dmg only if creature has enough stamina
			if (this.LeftHand != null && this.Stamina >= this.LeftHand.Data.StaminaUsage)
			{
				min += this.LeftAttackMinMod;
				max += this.LeftAttackMaxMod;
			}

			return this.GetRndDamage(min, max, balance);
		}

		/// <summary>
		/// Calculates random damage with the given min/max and balance values.
		/// Adds attack mods and stat bonuses automatically and randomizes
		/// the balance.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <param name="balance"></param>
		/// <returns></returns>
		protected virtual float GetRndDamage(float min, float max, int balance)
		{
			var balanceMultiplicator = this.GetRndBalance(balance) / 100f;

			min += this.AttackMinMod;
			min += (Math.Max(0, this.Str - 10) / 3.0f);

			max += this.AttackMaxMod;
			max += (Math.Max(0, this.Str - 10) / 2.5f);

			if (min > max)
				min = max;

			return (min + ((max - min) * balanceMultiplicator));
		}

		/// <summary>
		/// Calculates the damage of left-and-right slots together
		/// </summary>
		/// <returns></returns>
		public float GetRndTotalDamage()
		{
			var balance = 0;
			if (this.RightHand == null)
				balance = this.BalanceBase + this.BalanceBaseMod;
			else
			{
				balance = this.RightBalanceMod;
				if (this.LeftHand != null)
					balance = (balance + this.LeftBalanceMod) / 2;
			}

			var min = this.AttackMinBase + this.AttackMinBaseMod + this.RightAttackMinMod;
			if (this.LeftHand != null)
				min = (min + this.LeftAttackMinMod) / 2;

			var max = this.AttackMaxBase + this.AttackMaxBaseMod + this.RightAttackMaxMod;
			if (this.LeftHand != null)
				max = (max + this.LeftAttackMaxMod) / 2;

			return this.GetRndDamage(min, max, balance);
		}

		/// <summary>
		/// Calculates random balance with given base balance, adding the
		/// dex bonus along the way.
		/// </summary>
		/// <param name="baseBalance"></param>
		/// <returns></returns>
		protected int GetRndBalance(int baseBalance)
		{
			var rnd = RandomProvider.Get();
			var balance = baseBalance;

			// Dex
			balance = (int)Math2.Clamp(0, 80, balance + ((this.Dex - 10) / 4f));

			// Randomization
			var diff = 100 - balance;
			var min = Math.Max(0, balance - diff);
			var max = Math.Max(100, balance + diff);

			balance = rnd.Next(min, max + 1);

			return Math2.Clamp(0, 80, balance);
		}

		/// <summary>
		/// Calculates random magic balance (0.0~1.0).
		/// </summary>
		/// <returns></returns>
		public float GetRndMagicBalance(float baseBalance = BaseMagicBalance)
		{
			var rnd = RandomProvider.Get();
			var balance = baseBalance;

			// Int
			balance += (Math.Max(0, this.Int - 10) / 4) / 100f;

			// Randomization, balance+-(100-balance), eg 80 = 60~100
			var diff = 1.0f - balance;
			balance += ((diff - (diff * 2 * (float)rnd.NextDouble())) * (float)rnd.NextDouble());

			return Math2.Clamp(0f, 1f, balance);
		}

		/// <summary>
		/// Calculates random base Magic damage for skill, using the given values.
		/// </summary>
		/// <remarks>
		/// http://wiki.mabinogiworld.com/view/Stats#Magic_Damage
		/// </remarks>
		/// <param name="skill"></param>
		/// <param name="baseMin"></param>
		/// <param name="baseMax"></param>
		/// <returns></returns>
		public float GetRndMagicDamage(Skill skill, float baseMin, float baseMax)
		{
			var rnd = RandomProvider.Get();

			var baseDamage = rnd.Between(baseMin, baseMax);
			var factor = rnd.Between(skill.RankData.FactorMin, skill.RankData.FactorMax);
			var totalMagicAttack = this.MagicAttack + this.MagicAttackMod;

			var wandBonus = 0f;
			var chargeMultiplier = 0f;

			if (skill.Info.Id == SkillId.Icebolt && this.RightHand != null && this.RightHand.HasTag("/ice_wand/"))
				wandBonus = 5;
			else if (skill.Info.Id == SkillId.Firebolt && this.RightHand != null && this.RightHand.HasTag("/fire_wand/"))
				wandBonus = 5;
			else if (skill.Info.Id == SkillId.Lightningbolt && this.RightHand != null && this.RightHand.HasTag("/lightning_wand/"))
				wandBonus = 3.5f;

			if (skill.Info.Id == SkillId.Firebolt || skill.Info.Id == SkillId.IceSpear || skill.Info.Id == SkillId.HailStorm)
				chargeMultiplier = skill.Stacks;

			var damage = (float)(baseDamage + Math.Floor(wandBonus * (1 + chargeMultiplier)) + (factor * totalMagicAttack));

			return (damage * this.GetRndMagicBalance());
		}

		/// <summary>
		/// Returns random base damage for a ranged attack,
		/// e.g. Ranged Attack or Magnum Shot, based on race, weapon, etc.
		/// </summary>
		/// <returns></returns>
		public float GetRndRangedDamage()
		{
			// Base damage
			float min = this.AttackMinBase;
			float max = this.AttackMaxBase;

			// Weapon
			min += (this.RightHand == null ? 0 : this.RightHand.OptionInfo.AttackMin);
			max += (this.RightHand == null ? 0 : this.RightHand.OptionInfo.AttackMax);

			// Dex bonus
			min += (this.Dex - 10) / 3.5f;
			max += (this.Dex - 10) / 2.5f;

			// Base balance
			var balance = (this.RightHand == null ? this.BalanceBase + this.BalanceBaseMod : this.RightHand.OptionInfo.Balance);

			// Ranged balance bonus
			var skill = this.Skills.Get(SkillId.RangedAttack);
			if (skill != null)
				balance += (int)skill.RankData.Var5;

			// Random balance multiplier
			var multiplier = this.GetRndBalance(balance) / 100f;

			if (min > max)
				min = max;

			return (min + (max - min) * multiplier);
		}

		/// <summary>
		/// Applies damage to Life, kills creature if necessary.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="from"></param>
		public void TakeDamage(float damage, Creature from)
		{
			var lifeBefore = this.Life;

			this.Life -= damage;

			// Track hit
			if (from != null)
			{
				HitTracker tracker;
				lock (_hitTrackers)
				{
					// Create new tracker if there is none yet
					if (!_hitTrackers.TryGetValue(from.EntityId, out tracker))
						_hitTrackers[from.EntityId] = (tracker = new HitTracker(this, from));
				}
				tracker.RegisterHit(damage);
				_totalHits = Interlocked.Increment(ref _totalHits);
			}

			// Kill if life too low
			if (this.Life < 0 && !this.ShouldSurvive(damage, from, lifeBefore))
				this.Kill(from);
		}

		/// <summary>
		/// Returns true if creature should go into deadly by the attack.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="from"></param>
		/// <param name="lifeBefore"></param>
		/// <returns></returns>
		protected abstract bool ShouldSurvive(float damage, Creature from, float lifeBefore);

		/// <summary>
		/// Kills creature.
		/// </summary>
		/// <param name="killer"></param>
		public virtual void Kill(Creature killer)
		{
			if (this.Conditions.Has(ConditionsA.Deadly))
				this.Conditions.Deactivate(ConditionsA.Deadly);
			this.Activate(CreatureStates.Dead);

			//Send.SetFinisher(this, killer.EntityId);
			//Send.SetFinisher2(this);
			Send.IsNowDead(this);
			Send.SetFinisher(this, 0);

			ChannelServer.Instance.Events.OnCreatureKilled(this, killer);
			if (killer != null && killer.IsPlayer)
				ChannelServer.Instance.Events.OnCreatureKilledByPlayer(this, killer);
			this.Death.Raise(this, killer);

			if (this.Skills.ActiveSkill != null)
				this.Skills.CancelActiveSkill();

			var rnd = RandomProvider.Get();
			var pos = this.GetPosition();

			// Gold
			if (rnd.NextDouble() < ChannelServer.Instance.Conf.World.GoldDropChance)
			{
				// Random base amount
				var amount = rnd.Next(this.Drops.GoldMin, this.Drops.GoldMax + 1);

				if (amount > 0)
				{
					// Lucky Finish
					var luckyChance = rnd.NextDouble();
					if (luckyChance < ChannelServer.Instance.Conf.World.HugeLuckyFinishChance)
					{
						amount *= 100;

						if (amount >= 2000) killer.Titles.Enable(23); // the Lucky

						Send.CombatMessage(killer, Localization.Get("Huge Lucky Finish!!"));
						Send.Notice(killer, Localization.Get("Huge Lucky Finish!!"));
					}
					else if (luckyChance < ChannelServer.Instance.Conf.World.BigLuckyFinishChance)
					{
						amount *= 5;

						if (amount >= 2000) killer.Titles.Enable(23); // the Lucky

						Send.CombatMessage(killer, Localization.Get("Big Lucky Finish!!"));
						Send.Notice(killer, Localization.Get("Big Lucky Finish!!"));
					}
					else if (luckyChance < ChannelServer.Instance.Conf.World.LuckyFinishChance)
					{
						amount *= 2;

						if (amount >= 2000) killer.Titles.Enable(23); // the Lucky

						Send.CombatMessage(killer, Localization.Get("Lucky Finish!!"));
						Send.Notice(killer, Localization.Get("Lucky Finish!!"));
					}

					// Drop rate muliplicator
					amount = Math.Min(21000, (int)(amount * ChannelServer.Instance.Conf.World.GoldDropRate));

					// Drop stack for stack
					var i = 0;
					var pattern = (amount == 21000);
					do
					{
						Position dropPos;
						if (!pattern)
						{
							dropPos = pos.GetRandomInRange(Item.DropRadius, rnd);
						}
						else
						{
							dropPos = new Position(pos.X + CreatureDrops.MaxGoldPattern[i, 0], pos.Y + CreatureDrops.MaxGoldPattern[i, 1]);
							i++;
						}

						var gold = Item.CreateGold(Math.Min(1000, amount));
						gold.Drop(this.Region, dropPos, 0, killer, false);

						amount -= gold.Info.Amount;
					}
					while (amount > 0);
				}
			}

			// Drops
			var dropped = new HashSet<int>();
			foreach (var dropData in this.Drops.Drops)
			{
				if (dropData == null || !AuraData.ItemDb.Exists(dropData.ItemId))
				{
					Log.Warning("Creature.Kill: Invalid drop '{0}' from '{1}'.", (dropData == null ? "null" : dropData.ItemId.ToString()), this.RaceId);
					continue;
				}

				if (rnd.NextDouble() * 100 < dropData.Chance * ChannelServer.Instance.Conf.World.DropRate)
				{
					// Only drop any item once
					if (dropped.Contains(dropData.ItemId))
						continue;

					var item = new Item(dropData);
					item.Drop(this.Region, pos, Item.DropRadius, killer, false);

					dropped.Add(dropData.ItemId);
				}
			}

			foreach (var item in this.Drops.StaticDrops)
				item.Drop(this.Region, pos, Item.DropRadius, killer, false);

			this.Drops.ClearStaticDrops();

			this.DeadMenu.Update();
		}

		/// <summary>
		/// Increases exp and levels up creature if appropriate.
		/// </summary>
		/// <param name="val"></param>
		public void GiveExp(long val)
		{
			this.Exp += val;

			var levelStats = AuraData.StatsLevelUpDb.Find(this.RaceId, this.Age);
			if (levelStats == null)
			{
				if ((levelStats = AuraData.StatsLevelUpDb.Find(10000, 17)) == null)
				{
					Log.Error("Creature.GiveExp: No valid level up stats found for race {0}, age {1}.", this.RaceId, this.Age);
				}
				else
				{
					// Only warn when creature was a player, we'll let NPCs fall
					// back to Human 17 silently, until we know if they
					// have specific level up stats.
					if (this.IsPlayer)
						Log.Warning("Creature.GiveExp: Level up stats missing for race {0}, age {1}. Falling back to Human 17.", this.RaceId, this.Age);
				}
			}

			var prevLevel = this.Level;
			float ap = this.AbilityPoints;
			float life = this.LifeMaxBase;
			float mana = this.ManaMaxBase;
			float stamina = this.StaminaMaxBase;
			float str = this.StrBase;
			float int_ = this.IntBase;
			float dex = this.DexBase;
			float will = this.WillBase;
			float luck = this.LuckBase;

			while (this.Level < AuraData.ExpDb.MaxLevel && this.Exp >= AuraData.ExpDb.GetTotalForNextLevel(this.Level))
			{
				this.Level++;
				this.TotalLevel++;

				if (levelStats == null)
					continue;

				this.AbilityPoints += (short)Math2.Clamp(0, short.MaxValue, levelStats.AP * ChannelServer.Instance.Conf.World.LevelApRate);
				this.LifeMaxBase += levelStats.Life;
				this.ManaMaxBase += levelStats.Mana;
				this.StaminaMaxBase += levelStats.Stamina;
				this.StrBase += levelStats.Str;
				this.IntBase += levelStats.Int;
				this.DexBase += levelStats.Dex;
				this.WillBase += levelStats.Will;
				this.LuckBase += levelStats.Luck;
			}

			// Only notify on level up
			if (prevLevel < this.Level)
			{
				this.FullHeal();

				Send.StatUpdateDefault(this);
				Send.LevelUp(this);

				// Only send aquire if stat crosses the X.0 border.
				// Eg, 50.9 -> 51.1
				float diff = 0;
				if ((diff = (this.AbilityPoints - (int)ap)) >= 1) Send.SimpleAcquireInfo(this, "ap", diff);
				if ((diff = (this.LifeMaxBase - (int)life)) >= 1) Send.SimpleAcquireInfo(this, "life", diff);
				if ((diff = (this.ManaMaxBase - (int)mana)) >= 1) Send.SimpleAcquireInfo(this, "mana", diff);
				if ((diff = (this.StaminaMaxBase - (int)stamina)) >= 1) Send.SimpleAcquireInfo(this, "stamina", diff);
				if ((diff = (this.StrBase - (int)str)) >= 1) Send.SimpleAcquireInfo(this, "str", diff);
				if ((diff = (this.IntBase - (int)int_)) >= 1) Send.SimpleAcquireInfo(this, "int", diff);
				if ((diff = (this.DexBase - (int)dex)) >= 1) Send.SimpleAcquireInfo(this, "dex", diff);
				if ((diff = (this.WillBase - (int)will)) >= 1) Send.SimpleAcquireInfo(this, "will", diff);
				if ((diff = (this.LuckBase - (int)luck)) >= 1) Send.SimpleAcquireInfo(this, "luck", diff);

				ChannelServer.Instance.Events.OnCreatureLevelUp(this);
				this.LeveledUp.Raise(this, prevLevel);
			}
			else
				Send.StatUpdate(this, StatUpdateType.Private, Stat.Experience);
		}

		/// <summary>
		/// Increases age by years and sends update packets.
		/// </summary>
		/// <param name="years"></param>
		public void AgeUp(short years)
		{
			if (years < 0 || this.Age + years > short.MaxValue)
				return;

			float life = 0, mana = 0, stamina = 0, str = 0, dex = 0, int_ = 0, will = 0, luck = 0;
			var ap = 0;

			var newAge = this.Age + years;
			while (this.Age < newAge)
			{
				// Increase age before requestin statUp, we want the stats
				// for the next age.
				this.Age++;

				var statUp = AuraData.StatsAgeUpDb.Find(this.RaceId, this.Age);
				if (statUp == null)
				{
					if ((statUp = AuraData.StatsAgeUpDb.Find(10000, 17)) == null)
					{
						Log.Error("Creature.AgeUp: No valid age up stats found for race {0}, age {1}.", this.RaceId, this.Age);
					}
					else
					{
						// Only warn when creature was a player, we'll let NPCs fall
						// back to Human 17 silently.
						if (this.IsPlayer)
							Log.Warning("Creature.AgeUp: Age up stats missing for race {0}, age {1}. Falling back to Human 17.", this.RaceId, this.Age);
					}
				}

				// Collect bonuses for multi aging
				life += statUp.Life;
				mana += statUp.Mana;
				stamina += statUp.Stamina;
				str += statUp.Str;
				dex += statUp.Dex;
				int_ += statUp.Int;
				will += statUp.Will;
				luck += statUp.Luck;
				ap += statUp.AP;
			}

			// Apply stat bonuses
			this.LifeMaxBase += life;
			this.Life += life;
			this.ManaMaxBase += mana;
			this.Mana += mana;
			this.StaminaMaxBase += stamina;
			this.Stamina += stamina;
			this.StrBase += str;
			this.DexBase += dex;
			this.IntBase += int_;
			this.WillBase += will;
			this.LuckBase += luck;
			this.AbilityPoints += (short)Math2.Clamp(0, short.MaxValue, ap * ChannelServer.Instance.Conf.World.AgeApRate);

			this.LastAging = DateTime.Now;

			if (this is Character)
				this.Height = Math.Min(1.0f, 1.0f / 7.0f * (this.Age - 10.0f)); // 0 ~ 1.0

			// Send stat bonuses
			if (life != 0) Send.SimpleAcquireInfo(this, "life", mana);
			if (mana != 0) Send.SimpleAcquireInfo(this, "mana", mana);
			if (stamina != 0) Send.SimpleAcquireInfo(this, "stamina", stamina);
			if (str != 0) Send.SimpleAcquireInfo(this, "str", str);
			if (dex != 0) Send.SimpleAcquireInfo(this, "dex", dex);
			if (int_ != 0) Send.SimpleAcquireInfo(this, "int", int_);
			if (will != 0) Send.SimpleAcquireInfo(this, "will", will);
			if (luck != 0) Send.SimpleAcquireInfo(this, "luck", luck);
			if (ap != 0) Send.SimpleAcquireInfo(this, "ap", ap);

			Send.StatUpdateDefault(this);

			// XXX: Replace with effect and notice to allow something to happen past age 25?
			Send.AgeUpEffect(this, this.Age);
		}

		/// <summary>
		/// Heals all life, mana, stamina, hunger, and wounds and updates client.
		/// </summary>
		public void FullHeal()
		{
			this.Injuries = 0;
			this.Hunger = 0;
			this.Life = this.LifeMax;
			this.Mana = this.ManaMax;
			this.Stamina = this.StaminaMax;

			Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.Stamina, Stat.Hunger, Stat.Mana);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
		}

		/// <summary>
		/// Fully heals life and updates client.
		/// </summary>
		public void FullLifeHeal()
		{
			this.Injuries = 0;
			this.Life = this.LifeMax;

			Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
		}

		/// <summary>
		/// Changes life and sends stat update.
		/// </summary>
		/// <param name="amount"></param>
		public void ModifyLife(float amount)
		{
			this.Life += amount;
			Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
		}

		/// <summary>
		/// Increases AP and updates client.
		/// </summary>
		/// <param name="amount"></param>
		public void GiveAp(int amount)
		{
			this.AbilityPoints += (short)Math2.Clamp(short.MinValue, short.MaxValue, amount);
			Send.StatUpdate(this, StatUpdateType.Private, Stat.AbilityPoints);
		}

		/// <summary>
		/// Revives creature.
		/// </summary>
		/// <param name="option">Determines the penalty and stat recovery.</param>
		public void Revive(ReviveOptions option)
		{
			if (!this.IsDead)
				return;

			// Get and check exp penalty
			// "Here" wil be disabled by the client if not enough exp are
			// available, nothing else though, so we send an error message
			// if creature doesn't have enough exp, instead of issuing a
			// warning.
			var expPenalty = this.DeadMenu.GetExpPenalty(this.Level, option);
			var minExp = AuraData.ExpDb.GetTotalForNextLevel(this.Level - 2);

			if (this.Exp - expPenalty < minExp)
			{
				// Unofficial
				Send.Notice(this, NoticeType.MiddleSystem, Localization.Get("Insufficient EXP."));
				Send.DeadFeather(this);
				Send.Revived(this);
				return;
			}

			switch (option)
			{
				case ReviveOptions.Town:
				case ReviveOptions.TirChonaill:
				case ReviveOptions.DungeonEntrance:
				case ReviveOptions.BarriLobby:
					// 100% life and 50% injury recovery
					this.Injuries -= this.Injuries * 0.50f;
					this.Life = this.LifeMax;
					break;

				case ReviveOptions.Here:
					// 5 life recovery and 50% additional injuries
					this.Injuries += this.LifeInjured * 0.50f;
					this.Life = 5;
					break;

				case ReviveOptions.HereNoPenalty:
					// 100% life and 100% injury recovery
					this.Injuries = 0;
					this.Life = this.LifeMax;
					break;

				case ReviveOptions.ArenaLobby:
				case ReviveOptions.ArenaWaitingRoom:
					// 100% life, 100% injury, and 100% stamina recovery
					this.Injuries = 0;
					this.Life = this.LifeMax;
					this.Stamina = this.StaminaMax;
					break;

				case ReviveOptions.ArenaSide:
					// 50% life, 20% injury, and 50% stamina recovery
					this.Injuries -= this.Injuries * 0.20f;
					this.Life = this.LifeMax * 0.50f;
					this.Stamina = this.StaminaMax * 0.50f;
					break;

				case ReviveOptions.InCamp:
				case ReviveOptions.StatueOfGoddess:
					// 25% life recovery and 10% additional injuries
					this.Injuries += this.LifeInjured * 0.10f;
					this.Life = this.LifeMax * 0.25f;
					break;

				case ReviveOptions.PhoenixFeather:
					// 10% additional injuries
					this.Injuries += this.LifeInjured * 0.10f;
					this.Life = 1;
					break;

				case ReviveOptions.WaitForRescue:
					this.DeadMenu.Options ^= ReviveOptions.PhoenixFeather;
					Send.DeadFeather(this);
					Send.Revived(this);
					return;

				case ReviveOptions.NaoStone:
					this.DeadMenu.Options = ReviveOptions.NaoStoneRevive;
					Send.DeadFeather(this);
					Send.NaoRevivalEntrance(this);
					Send.Revived(this);
					return;

				case ReviveOptions.NaoStoneRevive:
					// First try beginner stones, then normals
					var item = this.Inventory.GetItem(a => a.HasTag("/notTransServer/nao_coupon/"), StartAt.BottomRight);
					if (item == null)
					{
						item = this.Inventory.GetItem(a => a.HasTag("/nao_coupon/"), StartAt.BottomRight);
						if (item == null)
						{
							Log.Error("Creature.Revive: Unable to remove Nao Soul Stone, none found.");
							return;
						}
					}

					// 100% life and 100% injury recovery
					this.Injuries = 0;
					this.Life = this.LifeMax;

					// Blessing of all items
					this.BlessAll();

					// Remove Soul Stone
					this.Inventory.Decrement(item);

					Send.NaoRevivalExit(this);
					break;

				default:
					Log.Warning("Creature.Revive: Unknown revive option: {0}", option);

					// Fallback, set Life to something positive.
					if (this.Life <= 1)
						this.Life = 1;
					break;
			}

			this.Deactivate(CreatureStates.Dead);
			this.DeadMenu.Clear();

			if (expPenalty != 0)
			{
				this.Exp = Math.Max(minExp, this.Exp - expPenalty);
				Send.StatUpdate(this, StatUpdateType.Private, Stat.Experience);
			}

			Send.RemoveDeathScreen(this);
			Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod, Stat.Stamina, Stat.Hunger);
			Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod);
			Send.RiseFromTheDead(this);
			Send.DeadFeather(this);
			Send.Revived(this);
		}

		/// <summary>
		/// Returns the power rating (Weak, Boss, etc) of
		/// compareCreature towards creature.
		/// </summary>
		/// <param name="compareCreature">Creature to compare to</param>
		/// <returns></returns>
		public PowerRating GetPowerRating(Creature compareCreature)
		{
			var cp = this.CombatPower;
			var otherCp = compareCreature.CombatPower;

			if (otherCp < cp * 0.8f) return PowerRating.Weakest;
			if (otherCp < cp * 1.0f) return PowerRating.Weak;
			if (otherCp < cp * 1.4f) return PowerRating.Normal;
			if (otherCp < cp * 2.0f) return PowerRating.Strong;
			if (otherCp < cp * 3.0f) return PowerRating.Awful;
			return PowerRating.Boss;
		}

		/// <summary>
		/// Calculates right (or bare) hand crit chance, taking stat bonuses
		/// and given protection into consideration.
		/// </summary>
		/// <param name="protection"></param>
		/// <returns></returns>
		public float GetRightCritChance(float protection)
		{
			var crit = this.CriticalBase + this.CriticalBaseMod + this.RightCriticalMod;
			return this.GetCritChance(crit, protection);
		}

		/// <summary>
		/// Calculates left hand crit chance, taking stat bonuses
		/// and given protection into consideration.
		/// </summary>
		/// <param name="protection"></param>
		/// <returns></returns>
		public float GetLeftCritChance(float protection)
		{
			if (this.LeftHand == null)
				return 0;

			return this.GetCritChance(this.LeftCriticalMod, protection);
		}

		/// <summary>
		/// Calculates total crit chance, taking stat bonuses
		/// and given protection and bonus into consideration.
		/// </summary>
		/// <param name="protection"></param>
		/// <returns></returns>
		public float GetTotalCritChance(float protection)
		{
			var crit = 0f;
			if (this.RightHand == null)
				crit = this.CriticalBase + this.CriticalBaseMod;
			else
			{
				crit = this.RightCriticalMod;
				if (this.LeftHand != null)
					crit = (crit + this.LeftCriticalMod) / 2;
			}

			return this.GetCritChance(crit, protection);
		}

		/// <summary>
		/// Adds stat bonuses to base and calculates crit chance,
		/// taking protection into consideration.
		/// </summary>
		/// <param name="baseCritical"></param>
		/// <param name="protection"></param>
		/// <returns></returns>
		private float GetCritChance(float baseCritical, float protection)
		{
			baseCritical += ((this.Will - 10) / 10f);
			baseCritical += ((this.Luck - 10) / 5f);

			return Math.Max(0, baseCritical - protection);
		}

		/// <summary>
		/// Returns Rest pose based on skill's rank.
		/// </summary>
		/// <returns></returns>
		public byte GetRestPose()
		{
			byte pose = 0;

			var skill = this.Skills.Get(SkillId.Rest);
			if (skill != null)
			{
				if (skill.Info.Rank >= SkillRank.R9)
					pose = 4;
				// Deactivated until we know how to keep the pose up.
				//if (skill.Info.Rank >= SkillRank.R1)
				//	pose = 5;
			}

			return pose;
		}

		/// <summary>
		/// Sets new position for target, based on attacker's position
		/// and the distance, takes collision into consideration.
		/// </summary>
		/// <param name="target">Entity to be knocked back</param>
		/// <param name="distance">Distance to knock back the target</param>
		/// <returns>New position</returns>
		public Position Shove(Creature target, int distance)
		{
			var attackerPosition = this.GetPosition();
			var targetPosition = target.GetPosition();

			var newPos = attackerPosition.GetRelative(targetPosition, distance);

			Position intersection;
			if (target.Region.Collisions.Find(targetPosition, newPos, out intersection))
				newPos = targetPosition.GetRelative(intersection, -50);

			target.SetPosition(newPos.X, newPos.Y);

			return newPos;
		}

		/// <summary>
		///  Returns true if creature's race data has the tag.
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public override bool HasTag(string tag)
		{
			if (this.RaceData == null)
				return false;

			return this.RaceData.HasTag(tag);
		}

		/// <summary>
		/// Returns targetable creatures in given range around creature.
		/// </summary>
		/// <param name="range">Radius around position.</param>
		/// <param name="options">Options to change the result.</param>
		/// <returns></returns>
		public ICollection<Creature> GetTargetableCreaturesInRange(int range, TargetableOptions options = TargetableOptions.None)
		{
			return this.GetTargetableCreaturesAround(this.GetPosition(), range, options);
		}

		/// <summary>
		/// Returns targetable creatures in given range around position.
		/// Optionally factors in attack range.
		/// </summary>
		/// <param name="position">Reference position.</param>
		/// <param name="range">Radius around position.</param>
		/// <param name="options">Options to change the result.</param>
		/// <returns></returns>
		public ICollection<Creature> GetTargetableCreaturesAround(Position position, int range, TargetableOptions options = TargetableOptions.None)
		{
			var targetable = this.Region.GetCreatures(target =>
			{
				var targetPos = target.GetPosition();
				var radius = range;
				if ((options & TargetableOptions.AddAttackRange) != 0)
				{
					// This is unofficial, the target's "hitbox" should be
					// factored in, but the total attack range is too much.
					// Using 50% for now until we know more.
					radius += this.AttackRangeFor(target) / 2;
				}

				return target != this // Exclude creature
					&& this.CanTarget(target) // Check targetability
					&& ((!this.Has(CreatureStates.Npc) || !target.Has(CreatureStates.Npc)) || this.Target == target) // Allow NPC on NPC only if it's the creature's target
					&& targetPos.InRange(position, radius) // Check range
					&& (((options & TargetableOptions.IgnoreWalls) != 0) || !this.Region.Collisions.Any(position, targetPos)) // Check collisions between positions
					&& !target.Conditions.Has(ConditionsA.Invisible); // Check visiblility (GM)
			});

			return targetable;
		}

		/// <summary>
		/// Aggroes target, setting target and putting creature in battle stance.
		/// </summary>
		/// <param name="creature"></param>
		public abstract void Aggro(Creature target);

		/// <summary>
		/// Disposes creature and removes it from its current region.
		/// </summary>
		public override void Disappear()
		{
			this.Dispose();

			if (this.Region != Region.Limbo)
				this.Region.RemoveCreature(this);

			base.Disappear();
		}

		/// <summary>
		/// Turns creature in direction of position.
		/// </summary>
		/// <param name="pos"></param>
		public void TurnTo(Position pos)
		{
			var creaturePos = this.GetPosition();
			var x = pos.X - creaturePos.X;
			var y = pos.Y - creaturePos.Y;

			this.TurnTo(x, y);
		}

		/// <summary>
		/// Turns creature in given direction.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public void TurnTo(float x, float y)
		{
			this.Direction = MabiMath.DirectionToByte(x, y);
			Send.TurnTo(this, x, y);
		}

		/// <summary>
		/// Starts dialog with NPC, returns false if NPC couldn't be found.
		/// </summary>
		/// <param name="npcName">The ident for the NPC, e.g. _duncan.</param>
		/// <param name="npcNameLocal">Defaults to npcName if null.</param>
		/// <returns></returns>
		public bool TalkToNpc(string npcName, string npcNameLocal = null)
		{
			npcNameLocal = npcNameLocal ?? npcName;

			var target = ChannelServer.Instance.World.GetNpc(npcName);
			if (target == null)
				return false;

			Send.NpcInitiateDialog(this, target.EntityId, npcName, npcNameLocal);
			this.Client.NpcSession.StartTalk(target, this);

			return true;
		}

		/// <summary>
		/// Adds item to creature's inventory.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool GiveItem(int itemId, int amount = 1)
		{
			var item = new Item(itemId);
			item.Amount = amount;

			return this.GiveItem(item);
		}

		/// <summary>
		/// Adds item to creature's inventory.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool GiveItem(Item item)
		{
			return this.Inventory.Add(item, true);
		}

		/// <summary>
		/// Adds item to creature's inventory and shows it above head.
		/// </summary>
		/// <param name="item"></param>
		public void GiveItemWithEffect(Item item)
		{
			this.GiveItem(item);
			Send.Effect(this, Effect.PickUpItem, (byte)1, item.Info.Id, item.Info.Color1, item.Info.Color2, item.Info.Color3);
		}

		/// <summary>
		/// Adds item to creature's inventory and shows an acquire window.
		/// </summary>
		/// <param name="item"></param>
		public void AcquireItem(Item item)
		{
			this.GiveItem(item);
			Send.AcquireItemInfo(this, item.EntityId);
		}

		/// <summary>
		/// Removes items with the given id from the creature's inventory.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		public void RemoveItem(int itemId, int amount = 1)
		{
			this.Inventory.Remove(itemId, amount);
		}

		/// <summary>
		/// Activates given Locks for creature.
		/// </summary>
		/// <remarks>
		/// Some locks are lifted automatically on Warp, SkillComplete,
		/// and SkillCancel.
		/// 
		/// Only sending the locks when they actually changed can cause problems,
		/// e.g. if a lock is removed during a cutscene (skill running out)
		/// the unlock after the cutscene isn't sent.
		/// The client actually has counted locks, unlike us atm.
		/// Implementing those will fix the problem. TODO.
		/// </remarks>
		/// <param name="locks">Locks to activate.</param>
		/// <param name="updateClient">Sends CharacterLock to client if true.</param>
		/// <returns>Creature's current lock value after activating given locks.</returns>
		public Locks Lock(Locks locks, bool updateClient = false)
		{
			var prev = this.Locks;
			this.Locks |= locks;

			if (updateClient /*&& prev != this.Locks*/)
				Send.CharacterLock(this, locks);

			return this.Locks;
		}

		/// <summary>
		/// Deactivates given Locks for creature.
		/// </summary>
		/// <remarks>
		/// Unlocking movement on the client apparently resets skill stuns.
		/// </remarks>
		/// <param name="locks">Locks to deactivate.</param>
		/// <param name="updateClient">Sends CharacterUnlock to client if true.</param>
		/// <returns>Creature's current lock value after deactivating given locks.</returns>
		public Locks Unlock(Locks locks, bool updateClient = false)
		{
			var prev = this.Locks;
			this.Locks &= ~locks;

			if (updateClient /*&& prev != this.Locks*/)
				Send.CharacterUnlock(this, locks);

			return this.Locks;
		}

		/// <summary>
		/// Returns true if given lock isn't activated.
		/// </summary>
		/// <param name="locks"></param>
		/// <returns></returns>
		public bool Can(Locks locks)
		{
			return ((this.Locks & locks) == 0);
		}

		/// <summary>
		/// Sends a msg box to creature's client, asking a question.
		/// The callback is executed if the box is answered with OK.
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Inquiry(Action<Creature> callback, string format, params object[] args)
		{
			byte id;

			lock (_inquiryCallbacks)
			{
				_inquiryId++;
				if (_inquiryId == 0)
					_inquiryId = 1;

				id = _inquiryId;

				_inquiryCallbacks[id] = callback;
			}

			Send.Inquiry(this, id, format, args);
		}

		/// <summary>
		/// Calls inquiry callback for id if there is one.
		/// </summary>
		/// <param name="id"></param>
		public void HandleInquiry(byte id)
		{
			Action<Creature> action;
			lock (_inquiryCallbacks)
			{
				if (!_inquiryCallbacks.TryGetValue(id, out action) || action == null)
					return;
			}

			action(this);
		}

		/// <summary>
		/// Calculates and returns general production skill success chance.
		/// </summary>
		/// <remarks>
		/// Unofficial, but seems to work fine in most cases.
		/// Dex bonus: http://mabination.com/threads/57123-Chaos-Life-Skill-Guide-Refining
		/// </remarks>
		/// <returns></returns>
		public float GetProductionSuccessChance(Skill skill, ProductionCategory category, int baseChance, int rainBonus)
		{
			// Base
			float result = baseChance;
			if (skill.Info.Id != SkillId.PotionMaking && skill.Info.Id != SkillId.Milling)
				result += (this.Dex - 60) * (baseChance / 300f);

			// Production Mastery bonus
			var pm = this.Skills.Get(SkillId.ProductionMastery);
			if (pm != null)
				result += (byte)pm.Info.Rank;

			// Weather bonus
			if (ChannelServer.Instance.Weather.GetWeatherType(this.RegionId) == WeatherType.Rain)
			{
				if (category == ProductionCategory.Weaving)
					result += rainBonus * 2;
				else
					result *= 1 + (rainBonus / 100f);
			}

			// Party bonus
			result += this.GetProductionPartyBonus(skill);

			return Math2.Clamp(0, 99, result);
		}

		/// <summary>
		/// Returns party production bonus for the given skill if creature
		/// is in a party.
		/// </summary>
		/// <remarks>
		// http://wiki.mabinogiworld.com/view/Party#Production_Bonus
		/// </remarks>
		/// <param name="skill"></param>
		/// <returns></returns>
		public float GetProductionPartyBonus(Skill skill)
		{
			// No bonus when not in party
			if (!this.IsInParty)
				return 0;

			var result = 0f;

			var members = this.Party.GetMembers();
			foreach (var member in members)
			{
				// Exclude this creature
				if (member == this)
					continue;

				// Exclude members that don't have Production Master rF+
				var productionMastery = member.Skills.Get(SkillId.ProductionMastery);
				if (productionMastery == null || productionMastery.Info.Rank < SkillRank.RF)
					continue;

				// Exclude members that don't have the production skill on rF+
				var memberSkill = member.Skills.Get(skill.Info.Id);
				if (memberSkill == null || memberSkill.Info.Rank < SkillRank.RF)
					continue;

				// +1% if member has the skill on a lower rank
				if (memberSkill.Info.Rank < skill.Info.Rank)
					result += 1;
				// +5% if member has the skill on same or higher rank
				else
					result += 5;
			}

			// Cap at 35
			return Math.Min(35, result);
		}

		/// <summary>
		/// Returns the tracker for the creature that did the most hits.
		/// </summary>
		/// <returns></returns>
		public HitTracker GetTopHitter()
		{
			HitTracker result = null;
			var top = 0;

			lock (_hitTrackers)
			{
				foreach (var tracker in _hitTrackers.Values)
				{
					if (tracker.Hits > top)
					{
						result = tracker;
						top = tracker.Hits;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the tracker for the creature that did the most damage.
		/// </summary>
		/// <returns></returns>
		public HitTracker GetTopDamageDealer()
		{
			HitTracker result = null;
			var top = 0f;

			lock (_hitTrackers)
			{
				foreach (var tracker in _hitTrackers.Values)
				{
					if (tracker.Damage > top)
					{
						result = tracker;
						top = tracker.Damage;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Returns the total number of hits the creature took.
		/// </summary>
		/// <returns></returns>
		public int GetTotalHits()
		{
			return _totalHits;
		}

		/// <summary>
		/// Returns whether creature is allowed to pick up the given item
		/// from the ground.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool CanPickUp(Item item)
		{
			// Check if item is actually on the ground
			if (item.RegionId == 0)
				return false;

			// Check if it's actually protected
			if (item.OwnerId == 0 || item.ProtectionLimit == null || item.ProtectionLimit < DateTime.Now)
				return true;

			// Return whether creature is the owner
			return (item.OwnerId == this.EntityId);
		}

		/// <summary>
		/// Calculates the creature's elemental damage modifier, against the
		/// target's elements.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public float CalculateElementalDamageMultiplier(Creature target)
		{
			return this.CalculateElementalDamageMultiplier(this.ElementLightning, this.ElementFire, this.ElementIce, target.ElementLightning, target.ElementFire, target.ElementIce);
		}

		/// <summary>
		/// Calculates the elemental damage modifier, based on the given
		/// affinities, against target.
		/// </summary>
		/// <param name="myLightning"></param>
		/// <param name="myFire"></param>
		/// <param name="myIce"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public float CalculateElementalDamageMultiplier(int myLightning, int myFire, int myIce, Creature target)
		{
			return this.CalculateElementalDamageMultiplier(myLightning, myFire, myIce, target.ElementLightning, target.ElementFire, target.ElementIce);
		}

		/// <summary>
		/// Calculates the elemental damage modifier, between the "my" and
		/// the "target" affinities.
		/// </summary>
		/// <remarks>
		/// Since nobody seems to know the exact way elementals work,
		/// this function is mostly based on guess.
		/// 
		/// First, all elementals are stacked against each other.
		/// If you have 1 affinity for an element that the enemy has as well,
		/// you lose 11.1% damage. Afterwards, you gain 11.1% for each very
		/// effective combination, and 3.7% for each slightly effective
		/// combination.
		/// 
		/// The basic idea is that the same elements cancel each other out,
		/// while Fire and Ice are very, and Ice and Lightning are slightly
		/// effective against each other, as is hinted at in the in-game
		/// book "Understanding Elementals". The book also mentions that
		/// Fire and Lightning don't affect each other.
		/// 
		/// The acual values, 11.1 and 3.7 (11.1 / 3) are based on the max
		/// affinity number 9, 11.1 * 9 being 99.9, and findings of the
		/// community, stating the need for at least 3 affinity for a
		/// noticible effect.
		/// </remarks>
		/// <param name="myLightning"></param>
		/// <param name="myFire"></param>
		/// <param name="mytIce"></param>
		/// <param name="targetLightning"></param>
		/// <param name="targetFire"></param>
		/// <param name="targetIce"></param>
		/// <returns></returns>
		public float CalculateElementalDamageMultiplier(int myLightning, int myFire, int myIce, int targetLightning, int targetFire, int targetIce)
		{
			var result = 0f;

			// Element vs Element
			result -= Math.Min(myLightning, targetLightning);
			result -= Math.Min(myFire, targetFire);
			result -= Math.Min(myIce, targetIce);

			// Fire >> Ice
			result += Math.Min(myFire, targetIce);

			// Ice >> Fire
			result += Math.Min(myIce, targetFire);

			// Ice > Lightning
			result += Math.Min(myIce, targetLightning) / 3f;

			// Lightning > Ice
			result += Math.Min(myLightning, targetIce) / 3f;

			return 1f + (result / 9f);
		}

		/// <summary>
		/// Blesses all main equip items and updates client.
		/// </summary>
		public void BlessAll()
		{
			foreach (var item in this.Inventory.GetMainEquipment())
			{
				item.OptionInfo.Flags |= ItemFlags.Blessed;
				Send.ItemBlessed(this, item);
			}
		}
	}

	public enum TargetableOptions
	{
		None,

		/// <summary>
		/// Adds attack range of creature to the given range.
		/// </summary>
		AddAttackRange,

		/// <summary>
		/// Ignores collision lines between creature and potential targets.
		/// </summary>
		IgnoreWalls,
	}
}
