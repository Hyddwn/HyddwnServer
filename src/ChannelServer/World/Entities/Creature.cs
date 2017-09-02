// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Skills;
using Aura.Channel.Skills.Life;
using Aura.Channel.World.Dungeons;
using Aura.Channel.World.Entities.Creatures;
using Aura.Channel.World.GameEvents;
using Aura.Channel.World.Inventory;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using Aura.Shared.Database;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities
{
    /// <summary>
    ///     Base class for all "creaturly" entities.
    /// </summary>
    public abstract class Creature : Entity, IDisposable
    {
        public const int BaseMagicBalance = 30;
        public const float MinStability = -10, MaxStability = 100;

        private const float MinWeight = 0.7f, MaxWeight = 1.5f;
        private const float MaxFoodStatBonus = 100;
        private const float MaxStatBonus = 100;

        public const int BareHandStaminaUsage = 2;

        public const int MaxElementalAffinity = 9;

        public const int PrefabEquipmentSwapKit = 86038;

        public const float ZombieSpeed = 28.6525f;

        // Use a static list to go through, so LeftHand is guaranteed to
        // come before RightHand. Otherwise the auto-equip code could remove
        // a second sword or arrows before they can be swapped.
        private static readonly ExtraSlots[] _swapSlots =
        {
            ExtraSlots.Armor, ExtraSlots.Glove, ExtraSlots.Shoe, ExtraSlots.Head, ExtraSlots.Robe,
            ExtraSlots.LeftHand, ExtraSlots.RightHand, ExtraSlots.Accessory1, ExtraSlots.Accessory2
        };

        private short _ap;

        // Combat
        // ------------------------------------------------------------------

        protected bool _battleStance;

        public long _hitTrackerIds;
        public Dictionary<long, HitTracker> _hitTrackers;
        private readonly Dictionary<byte, Action<Creature>> _inquiryCallbacks;

        private byte _inquiryId;
        private string _lastTown;

        // Life
        // ------------------------------------------------------------------

        private float _life, _injuries;

        // Food Mods
        // ------------------------------------------------------------------

        private float _lifeFoodMod, _manaFoodMod, _staminaFoodMod;

        // Mana
        // ------------------------------------------------------------------

        private float _mana;
        private double _movementX, _movementY;
        private DateTime _moveStartTime;

        // Movement
        // ------------------------------------------------------------------

        private Position _position, _destination;

        private float _stability;
        private DateTime _stabilityChange;

        // Stamina
        // ------------------------------------------------------------------

        private float _stamina, _hunger;

        // Stat Bonuses
        // ------------------------------------------------------------------

        private float _strBonus, _intBonus, _dexBonus, _willBonus, _luckBonus;
        private float _strFoodMod, _intFoodMod, _dexFoodMod, _willFoodMod, _luckFoodMod;

        private int _stun;
        private DateTime _stunChange;
        public int _totalHits;

        private float _weight, _upper, _lower;

        // ------------------------------------------------------------------

        protected Creature()
        {
            Client = new DummyClient();

            Temp = new CreatureTemp();
            Titles = new CreatureTitles(this);
            Keywords = new CreatureKeywords(this);
            Regens = new CreatureRegen(this);
            Skills = new CreatureSkills(this);
            StatMods = new CreatureStatMods(this);
            Conditions = new CreatureConditions(this);
            Quests = new CreatureQuests(this);
            Drops = new CreatureDrops(this);
            DeadMenu = new CreatureDeadMenu(this);
            AimMeter = new AimMeter(this);
            Party = Party.CreateDummy(this);
            Inventory = new CreatureInventory(this);
            CoolDowns = new CreatureCoolDowns(this);

            Vars = new ScriptVariables();

            _inquiryCallbacks = new Dictionary<byte, Action<Creature>>();
            _hitTrackers = new Dictionary<long, HitTracker>();
        }

        public override DataType DataType => DataType.Creature;

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
        public CreatureCoolDowns CoolDowns { get; protected set; }

        public int InventoryWidth { get; set; }
        public int InventoryHeight { get; set; }

        /// <summary>
        ///     Gets or sets currently selected equipment set.
        ///     Does not update client.
        /// </summary>
        public EquipmentSet CurrentEquipmentSet
        {
            get => (EquipmentSet) ((VariableManager) Vars.Perm).Get("CurrentEquipmentSet", (int) EquipmentSet.Original);
            set => ((VariableManager) Vars.Perm)["CurrentEquipmentSet"] = (int) value;
        }

        /// <summary>
        ///     Gets or sets the time in which the extra equip slots can be used.
        ///     Does not update client.
        /// </summary>
        public DateTime ExtraEquipmentSetsEnd
        {
            get => ((VariableManager) Vars.Perm).Get("ExtraSetsEnd", DateTime.MinValue);
            set => ((VariableManager) Vars.Perm)["ExtraSetsEnd"] = value;
        }

        /// <summary>
        ///     Returns whether the extra equip slots can currently be used.
        /// </summary>
        public bool ExtraEquipmentSlotsAvailable => DateTime.Now < ExtraEquipmentSetsEnd;

        /// <summary>
        ///     Returns number of available extra equipment sets.
        /// </summary>
        public int ExtraEquipmentSetsCount => Inventory.Count(PrefabEquipmentSwapKit);

        /// <summary>
        ///     Temporary and permanent variables exclusive to this creature.
        /// </summary>
        /// <remarks>
        ///     Permanent variables are saved across relogs if the creature
        ///     is a player creature. NPCs and monster variables aren't saved.
        /// </remarks>
        public ScriptVariables Vars { get; protected set; }

        /// <summary>
        ///     Returns true if creature is a Character, RpCharacter, or Pet.
        /// </summary>
        /// <remarks>
        ///     This propery is frequently used to determine if a creature should
        ///     get special treatment because it's a player. Player creatures
        ///     can level up, have special events, can keep dynamic regions open,
        ///     and other things.
        ///     TODO: Dedicated properties might be better to determine what a
        ///     creature can do, as they would be more descriptive and easier
        ///     to maintain.
        /// </remarks>
        public bool IsPlayer => IsCharacter || IsPet || IsRpCharacter;

        /// <summary>
        ///     Returns true if creature is a character, i.e. a player creature,
        ///     but not a pet/partner.
        /// </summary>
        public bool IsCharacter => this is Character;

        /// <summary>
        ///     Returns true if creature is an role-playing character.
        /// </summary>
        public bool IsRpCharacter => this is RpCharacter;

        /// <summary>
        ///     Returns true if creature is a pet.
        /// </summary>
        public bool IsPet => this is Pet;

        /// <summary>
        ///     Returns true if creature is a partner, i.e. a pet with an entity
        ///     id in a certain range.
        /// </summary>
        public bool IsPartner => IsPet && EntityId >= MabiId.Partners;

        /// <summary>
        ///     Returns true if creature is a human, based on the race id.
        /// </summary>
        public bool IsHuman => RaceId == 10001 || RaceId == 10002;

        /// <summary>
        ///     Returns true if creature is an elf, based on the race id.
        /// </summary>
        public bool IsElf => RaceId == 9001 || RaceId == 9002;

        /// <summary>
        ///     Returns true if creature is a giant, based on the race id.
        /// </summary>
        public bool IsGiant => RaceId == 8001 || RaceId == 8002;

        /// <summary>
        ///     Returns true if creature is male, based on its race data.
        /// </summary>
        public bool IsMale => RaceData != null && RaceData.Gender == Gender.Male;

        /// <summary>
        ///     Returns true if creature is female, based on its race data.
        /// </summary>
        public bool IsFemale => RaceData != null && RaceData.Gender == Gender.Female;

        /// <summary>
        ///     The region the creature is currently in.
        /// </summary>
        /// <remarks>
        ///     During warps, this value is the region id of the previous region,
        ///     only after the warp is done, it's set to the new region.
        /// </remarks>
        public override int RegionId { get; set; }

        /// <summary>
        ///     Lock handler, for prohibiting the creature from doing certain things.
        /// </summary>
        public Locks Locks { get; protected set; }

        /// <summary>
        ///     Returns whether creature is able to learn skills automatically
        ///     (e.g. Counterattack).
        /// </summary>
        public virtual bool LearningSkillsEnabled => false;

        /// <summary>
        ///     Time at which the creature was created.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        ///     Time of last rebirth.
        /// </summary>
        public DateTime LastRebirth { get; set; }

        /// <summary>
        ///     Time of last aging.
        /// </summary>
        public DateTime LastAging { get; set; }

        /// <summary>
        ///     Time of last login.
        /// </summary>
        public DateTime LastLogin { get; set; }

        /// <summary>
        ///     The time the creature has been active in seconds.
        /// </summary>
        public long PlayTime { get; set; }

        /// <summary>
        ///     How many times the character rebirthed.
        /// </summary>
        public int RebirthCount { get; set; }

        /// <summary>
        ///     Returns true if creature has the devCAT title selected.
        /// </summary>
        public bool IsDev => Titles.SelectedTitle == TitleId.devCAT;

        /// <summary>
        ///     Gets or sets the amount of "cash" points on the creature's
        ///     account and updates the client accordingly.
        /// </summary>
        public int Points
        {
            get => Client.Account != null ? Client.Account.Points : 0;
            set
            {
                if (Client.Account == null)
                    return;

                var points = Math2.Clamp(0, int.MaxValue, value);
                Client.Account.Points = points;
                Send.PointsUpdate(this, points);
            }
        }

        /// <summary>
        ///     The outfit Nao wears when reviving the creature.
        /// </summary>
        public NaoOutfit NaoOutfit { get; set; }


        /// <summary>
        ///     Returns true if creature has ever set foot in any actual region.
        /// </summary>
        /// <returns></returns>
        public bool HasEverEnteredWorld => Has(CreatureStates.EverEnteredWorld);

        /// <summary>
        ///     Returns true if it's the creature's birthday and it hasn't
        ///     received a birthday present yet today.
        /// </summary>
        /// <returns></returns>
        public bool CanReceiveBirthdayPresent
        {
            get
            {
                // Only players with active premium service can receive gifts.
                if (!Client.Account.PremiumServices.HasPremiumService)
                    return false;

                // Dead characters should not be moved to the Soul Stream,
                // as they will get stuck there, not being able to move
                // or revive.
                if (IsDead)
                    return false;

                var now = DateTime.Now;

                // No present if today is not the player's birthday or the character
                // was just created.
                var isBirthday = CreationTime.DayOfWeek == now.DayOfWeek;
                var isBirth = CreationTime.Date == now.Date;
                if (!isBirthday || isBirth)
                    return false;

                // If no last date, we never got one before and get it now.
                var last = Vars.Perm["NaoLastPresentDate"];
                if (last == null)
                    return true;

                // Only allow present if player didn't already receive one today.
                return last.Date < now.Date;
            }
        }

        /// <summary>
        ///     Returns true if creature is in Tir Na Nog or a dungeon there.
        /// </summary>
        public bool IsInTirNaNog
        {
            get
            {
                // Check non-dynamic regions
                var regionId = Region.Id;
                if (regionId >= 35 && regionId <= 46)
                    return true;

                // Check dungeon regions
                var dungeonRegion = Region as DungeonRegion;
                if (dungeonRegion != null && dungeonRegion.Dungeon.Name.ToLower().Contains("tirnanog"))
                    return true;

                // Check dynamic regions
                var dynamicRegion = Region as DynamicRegion;
                if (dynamicRegion != null)
                {
                    regionId = dynamicRegion.BaseId;
                    if (regionId >= 35 && regionId <= 46)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Returns amount of gold in creature's inventory.
        /// </summary>
        public int Gold => Inventory.Gold;

        /// <summary>
        ///     Returns id of creature's current title.
        /// </summary>
        public int Title => Titles.SelectedTitle;

        // Look
        // ------------------------------------------------------------------

        public byte SkinColor { get; set; }
        public short EyeType { get; set; }
        public byte EyeColor { get; set; }
        public byte MouthType { get; set; }
        public float Height { get; set; }

        public float Weight
        {
            get => _weight;
            set => _weight = Math2.Clamp(MinWeight, MaxWeight, value);
        }

        public float Upper
        {
            get => _upper;
            set => _upper = Math2.Clamp(MinWeight, MaxWeight, value);
        }

        public float Lower
        {
            get => _lower;
            set => _lower = Math2.Clamp(MinWeight, MaxWeight, value);
        }

        public float BodyScale => Height * 0.4f + 0.6f;

        public string StandStyle { get; set; }
        public string StandStyleTalking { get; set; }

        public uint Color1 { get; set; }
        public uint Color2 { get; set; }
        public uint Color3 { get; set; }

        /// <summary>
        ///     Returns body proportions
        /// </summary>
        public BodyProportions Body
        {
            get
            {
                BodyProportions result;
                result.Height = Height;
                result.Weight = Weight;
                result.Upper = Upper;
                result.Lower = Lower;
                return result;
            }
        }

        // Transformation
        // ------------------------------------------------------------------

        /// <summary>
        ///     Returns the creature's current transformation.
        /// </summary>
        public Transformation Transformation { get; set; }

        /// <summary>
        ///     Returns the rank of the creature's current transformation's
        ///     skill.
        /// </summary>
        public SkillRank TransformationSkillRank { get; set; }

        /// <summary>
        ///     Reuturns the level of the creature's current transformation.
        /// </summary>
        public TransformationLevel TransformationLevel { get; set; }

        /// <summary>
        ///     Returns true if creature is currently transformed.
        /// </summary>
        public bool IsTransformed => Transformation != Transformation.None;

        // Inventory
        // ------------------------------------------------------------------

        public CreatureInventory Inventory { get; protected set; }
        public Item RightHand => Inventory.RightHand;
        public Item LeftHand => Inventory.LeftHand;
        public Item Magazine => Inventory.Magazine;
        public bool HandsFree => RightHand == null && LeftHand == null && Magazine == null;

        /// <summary>
        ///     Returns whether the creature is wielding main weapons on both hands.
        ///     Shields and similar items are not considered main weapons.
        /// </summary>
        public bool IsDualWielding => RightHand != null && LeftHand != null && LeftHand.Data.WeaponType != 0;

        /// <summary>
        ///     Returns whether the creature is naturally able to equip/unequip
        ///     items, based on its class.
        /// </summary>
        public virtual bool CanMoveEquip => true;

        public byte Direction { get; set; }
        public bool IsMoving => _position != _destination;
        public bool IsWalking { get; protected set; }
        public double MoveDuration { get; private set; }

        /// <summary>
        ///     Location to warp to.
        /// </summary>
        public Location WarpLocation { get; set; }

        /// <summary>
        ///     Location of the creature before the warp.
        /// </summary>
        public Location LastLocation { get; set; }

        /// <summary>
        ///     Location to fall back to, when saving in a temp region.
        /// </summary>
        public Location FallbackLocation { get; set; }

        /// <summary>
        ///     Location the player has saved at in a dungeon.
        /// </summary>
        public Location DungeonSaveLocation { get; set; }

        /// <summary>
        ///     Event path to last visited town.
        /// </summary>
        public string LastTown
        {
            get => string.IsNullOrWhiteSpace(_lastTown) ? "Uladh_main/town_TirChonaill/TirChonaill_Spawn_A" : _lastTown;
            set => _lastTown = value;
        }

        /// <summary>
        ///     True while character is warping somewhere.
        /// </summary>
        public bool Warping { get; set; }

        /// <summary>
        ///     Changes stance and broadcasts update.
        /// </summary>
        public bool IsInBattleStance
        {
            get => _battleStance;
            set
            {
                _battleStance = value;
                Send.ChangeStance(this);
            }
        }

        public Creature Target { get; set; }

        /// <summary>
        ///     Amount of ms before creature can do something again.
        /// </summary>
        /// <remarks>
        ///     Max stun animation duration for monsters seems to be about 3s.
        /// </remarks>
        public int Stun
        {
            get
            {
                if (_stun <= 0)
                    return 0;

                var diff = (DateTime.Now - _stunChange).TotalMilliseconds;
                if (diff < _stun)
                    return (int) (_stun - diff);

                return _stun = 0;
            }
            set
            {
                _stun = Math2.Clamp(0, short.MaxValue, value);
                _stunChange = DateTime.Now;
            }
        }

        /// <summary>
        ///     Creature's stability, once it goes below a certain value the creature
        ///     becomes "unstable" and might have to be knocked back.
        /// </summary>
        /// <remarks>
        ///     The more you hit a creature with heavy weapons,
        ///     the higher this value. If it goes above 100 the creature
        ///     is knocked back/down.
        ///     This is also used for the knock down gauge.
        /// </remarks>
        public float Stability
        {
            get
            {
                if (_stability >= MaxStability)
                    return MaxStability;

                var result = _stability + (DateTime.Now - _stabilityChange).TotalMilliseconds / 60f;
                if (result >= MaxStability)
                    result = _stability = MaxStability;

                return (float) result;
            }
            set
            {
                _stability = Math2.Clamp(MinStability, MaxStability, value);
                _stabilityChange = DateTime.Now;
            }
        }

        /// <summary>
        ///     Returns true if Stability is lower than or equal to 0,
        ///     at which point the creature should be knocked back.
        /// </summary>
        public bool IsUnstable => Stability <= 0;

        public bool IsDead => Has(CreatureStates.Dead);
        public bool IsStunned => Stun > 0;

        public bool WasKnockedBack { get; set; }

        /// <summary>
        ///     Returns average knock count of both equipped weapons, or race's
        ///     if none are equipped.
        /// </summary>
        public int AverageKnockCount
        {
            get
            {
                var result = RaceData.KnockCount + 1;

                if (RightHand != null)
                {
                    result = RightHand.Info.KnockCount + 1;
                    if (LeftHand != null)
                    {
                        result += LeftHand.Info.KnockCount + 1;
                        result /= 2;
                    }
                }

                return result;
            }
        }

        /// <summary>
        ///     Returns weapon's attack speed or the race's if not weapon
        ///     is equipped.
        /// </summary>
        public AttackSpeed AttackSpeed =>
            RightHand != null ? RightHand.Data.AttackSpeed : (AttackSpeed) RaceData.AttackSpeed;

        /// <summary>
        ///     Returns average attack speed of both equipped weapons, or race's
        ///     if none are equipped.
        /// </summary>
        public AttackSpeed AverageAttackSpeed
        {
            get
            {
                var result = RaceData.AttackSpeed;

                if (RightHand != null)
                {
                    result = (int) RightHand.Data.AttackSpeed;
                    if (LeftHand != null)
                    {
                        result += (int) LeftHand.Data.AttackSpeed;
                        result /= 2;
                    }
                }

                return (AttackSpeed) result;
            }
        }

        /// <summary>
        ///     Holds the time at which the knock down is over.
        /// </summary>
        public DateTime KnockDownTime { get; set; }

        /// <summary>
        ///     Returns true if creature is currently knocked down.
        /// </summary>
        public bool IsKnockedDown => DateTime.Now < KnockDownTime;

        /// <summary>
        ///     Returns true if creature is able to run while having ranged loaded,
        ///     e.g. because its and elf or has a crossbow equipped.
        /// </summary>
        public bool CanRunWithRanged => IsElf || RightHand != null && RightHand.HasTag("/crossbow/");

        public long FinisherId { get; private set; }
        public bool IsFinished { get; private set; }

        // Stats
        // ------------------------------------------------------------------

        public short Level { get; set; }
        public int TotalLevel { get; set; }
        public long Exp { get; set; }

        public short Age { get; set; }

        public short AbilityPoints
        {
            get => _ap;
            set => _ap = Math.Max((short) 0, value);
        }

        public virtual float CombatPower => Math2.Clamp(1, 9999,
            (RaceData != null ? RaceData.CombatPower : 1) + StatMods.Get(Stat.CombatPowerMod));

        public float StrBaseSkill { get; set; }
        public float DexBaseSkill { get; set; }
        public float IntBaseSkill { get; set; }
        public float WillBaseSkill { get; set; }
        public float LuckBaseSkill { get; set; }

        public float StrMod => StatMods.Get(Stat.StrMod);
        public float DexMod => StatMods.Get(Stat.DexMod);
        public float IntMod => StatMods.Get(Stat.IntMod);
        public float WillMod => StatMods.Get(Stat.WillMod);
        public float LuckMod => StatMods.Get(Stat.LuckMod);

        public float StrBase { get; set; }
        public float DexBase { get; set; }
        public float IntBase { get; set; }
        public float WillBase { get; set; }
        public float LuckBase { get; set; }
        public float StrBaseTotal => StrBase + StrBaseSkill + StrBonus;
        public float DexBaseTotal => DexBase + DexBaseSkill + DexBonus;
        public float IntBaseTotal => IntBase + IntBaseSkill + IntBonus;
        public float WillBaseTotal => WillBase + WillBaseSkill + WillBonus;
        public float LuckBaseTotal => LuckBase + LuckBaseSkill + LuckBonus;
        public float Str => StrBaseTotal + StrMod + StrFoodMod;
        public float Dex => DexBaseTotal + DexMod + DexFoodMod;
        public float Int => IntBaseTotal + IntMod + IntFoodMod;
        public float Will => WillBaseTotal + WillMod + WillFoodMod;
        public float Luck => LuckBaseTotal + LuckMod + LuckFoodMod;

        /// <summary>
        ///     Rate from monster.
        /// </summary>
        public int BalanceBase => RightHand == null ? RaceData.BalanceBase : 0;

        /// <summary>
        ///     Rate from race.
        /// </summary>
        public int BalanceBaseMod => RightHand == null ? RaceData.BalanceBaseMod : 0;

        /// <summary>
        ///     Balance bonus from enchants and other sources.
        /// </summary>
        public int BalanceMod => (int) StatMods.Get(Stat.BalanceMod);

        /// <summary>
        ///     Balance of right hand weapon.
        /// </summary>
        public int RightBalanceMod => RightHand != null ? RightHand.OptionInfo.Balance : 0;

        /// <summary>
        ///     Balance of left hand weapon.
        /// </summary>
        public int LeftBalanceMod => LeftHand != null ? LeftHand.OptionInfo.Balance : 0;

        /// <summary>
        ///     Critical from monster.
        /// </summary>
        public float CriticalBase => RightHand == null ? RaceData.CriticalBase : 0;

        /// <summary>
        ///     Critical from race.
        /// </summary>
        public float CriticalBaseMod => RightHand == null ? RaceData.CriticalBaseMod : 0;

        /// <summary>
        ///     Critical bonus from enchants and other sources.
        /// </summary>
        public int CriticalMod => (int) StatMods.Get(Stat.CriticalMod);

        /// <summary>
        ///     Critical of right hand weapon.
        /// </summary>
        public float RightCriticalMod => RightHand != null ? RightHand.OptionInfo.Critical : 0;

        /// <summary>
        ///     Critical of left hand weapon.
        /// </summary>
        public float LeftCriticalMod => LeftHand != null ? LeftHand.OptionInfo.Critical : 0;

        /// <summary>
        ///     AttMin from monster.
        /// </summary>
        /// <remarks>
        ///     This seems to count towards the creature's damage even if a weapon
        ///     is equipped. This assumption is based on the fact that Golems
        ///     have a 0 attack weapon, that would make them do almost no damage.
        /// </remarks>
        public int AttackMinBase => RaceData.AttackMinBase;

        /// <summary>
        ///     AttMax from monster.
        /// </summary>
        /// <remarks>
        ///     This seems to count towards the creature's damage even if a weapon
        ///     is equipped. This assumption is based on the fact that Golems
        ///     have a 0 attack weapon, that would make them do almost no damage.
        /// </remarks>
        public int AttackMaxBase => RaceData.AttackMaxBase;

        /// <summary>
        ///     AttackMin from race.
        /// </summary>
        public int AttackMinBaseMod => RightHand == null ? RaceData.AttackMinBaseMod : 0;

        /// <summary>
        ///     AttackMax from race.
        /// </summary>
        public int AttackMaxBaseMod => RightHand == null ? RaceData.AttackMaxBaseMod : 0;

        /// <summary>
        ///     Par_AttackMin from itemdb, for right hand weapon.
        /// </summary>
        /// <remarks>
        ///     Officials differentiate between 1H and 2H (e.g. two-hand swords
        ///     and bows) weapons, in that they don't contribute to the Right...
        ///     and Left... properties, but ...BaseMod. While this makes sense,
        ///     it adds unnecessary complexity, as the client will display the
        ///     correct values the way we do it as well, since it simply uses
        ///     the values to calculate the stats, independent of the kind of
        ///     weapon you have equipped.
        /// </remarks>
        public int RightAttackMinMod => RightHand != null ? RightHand.OptionInfo.AttackMin : 0;

        /// <summary>
        ///     Par_AttackMax from itemdb, for right hand weapon.
        /// </summary>
        public int RightAttackMaxMod => RightHand != null ? RightHand.OptionInfo.AttackMax : 0;

        /// <summary>
        ///     Par_AttackMin from itemdb, for left hand weapon.
        /// </summary>
        public int LeftAttackMinMod => LeftHand != null ? LeftHand.OptionInfo.AttackMin : 0;

        /// <summary>
        ///     Par_AttackMax from itemdb, for left hand weapon.
        /// </summary>
        public int LeftAttackMaxMod => LeftHand != null ? LeftHand.OptionInfo.AttackMax : 0;

        /// <summary>
        ///     Used for title, enchant, and other bonuses.
        /// </summary>
        public int AttackMinMod => (int) StatMods.Get(Stat.AttackMinMod);

        /// <summary>
        ///     Used for title, enchant, and other bonuses.
        /// </summary>
        public int AttackMaxMod => (int) StatMods.Get(Stat.AttackMaxMod);

        /// <summary>
        ///     WAttMin from monster.
        /// </summary>
        public int InjuryMinBase => RaceData.InjuryMinBase;

        /// <summary>
        ///     WAttMax from monster.
        /// </summary>
        public int InjuryMaxBase => RaceData.InjuryMaxBase;

        /// <summary>
        ///     WAttackMin from race.
        /// </summary>
        public int InjuryMinBaseMod => RaceData.InjuryMinBaseMod;

        /// <summary>
        ///     WAttackMax from race.
        /// </summary>
        public int InjuryMaxBaseMod => RaceData.InjuryMaxBaseMod;

        /// <summary>
        ///     Par_WAttackMin from itemdb.
        /// </summary>
        public int RightInjuryMinMod => RightHand != null ? RightHand.OptionInfo.InjuryMin : 0;

        /// <summary>
        ///     Par_WAttackMax from itemdb.
        /// </summary>
        public int RightInjuryMaxMod => RightHand != null ? RightHand.OptionInfo.InjuryMax : 0;

        /// <summary>
        ///     Par_WAttackMin from itemdb.
        /// </summary>
        public int LeftInjuryMinMod => LeftHand != null ? LeftHand.OptionInfo.InjuryMin : 0;

        /// <summary>
        ///     Par_WAttackMax from itemdb.
        /// </summary>
        public int LeftInjuryMaxMod => LeftHand != null ? LeftHand.OptionInfo.InjuryMax : 0;

        /// <summary>
        ///     Title bonuses?
        /// </summary>
        public int InjuryMinMod => (int) StatMods.Get(Stat.InjuryMinMod);

        /// <summary>
        ///     Title bonuses?
        /// </summary>
        public int InjuryMaxMod => (int) StatMods.Get(Stat.InjuryMaxMod);

        /// <summary>
        ///     Returns total min injury.
        /// </summary>
        public int InjuryMin
        {
            get
            {
                var result = (Dex - 10) * 0.05f + (Will - 10) * 0.05f;
                result += InjuryMinBase;
                result += InjuryMinBaseMod;
                result += InjuryMinMod;

                // Add average of both weapons
                if (RightHand != null)
                {
                    var weapons = (float) RightInjuryMinMod;
                    if (LeftHand != null)
                        weapons = (weapons + LeftInjuryMinMod) / 2;

                    result += weapons;
                }

                return (int) Math2.Clamp(0, 100, result);
            }
        }

        /// <summary>
        ///     Returns total max injury.
        /// </summary>
        /// <remarks>
        ///     Something is missing in this function, the max injury rate shown on
        ///     the client tends to be *slightly* different, in the range of +-1~4.
        /// </remarks>
        public int InjuryMax
        {
            get
            {
                var result = (Dex - 10) * 0.1f + (Will - 10) * 0.2f;
                result += InjuryMaxBase;
                result += InjuryMaxBaseMod;
                result += InjuryMaxMod;

                // Add average of both weapons
                if (RightHand != null)
                {
                    var weapons = (float) RightInjuryMaxMod;
                    if (LeftHand != null)
                        weapons = (weapons + LeftInjuryMaxMod) / 2;

                    result += weapons;
                }

                return (int) Math2.Clamp(0, 100, result);
            }
        }

        /// <summary>
        ///     Returns total magic attack, based on stats, equipment, etc.
        /// </summary>
        public float MagicAttack
        {
            get
            {
                var result = Math.Max(0, Int - 10) / 5f;

                // TODO: Bonuses

                return result;
            }
        }

        /// <summary>
        ///     MagicAttack bonus from enchants and other sources.
        /// </summary>
        public int MagicAttackMod => (int) StatMods.Get(Stat.MagicAttackMod);

        /// <summary>
        ///     Returns total magic defense, based on stats, equipment, etc.
        /// </summary>
        public float MagicDefense
        {
            get
            {
                var result = Math.Max(0, Will - 10) / 5f;

                // TODO: Bonuses

                return result;
            }
        }

        /// <summary>
        ///     MagicDefense bonus from enchants and other sources.
        /// </summary>
        public int MagicDefenseMod => (int) StatMods.Get(Stat.MagicDefenseMod);

        /// <summary>
        ///     Returns total magic protection, based on stats, equipment, etc.
        /// </summary>
        public float MagicProtection
        {
            get
            {
                var result = Math.Max(0, Int - 10) / 20f;

                // TODO: Bonuses

                return result;
            }
        }

        /// <summary>
        ///     MagicProtection bonus from enchants and other sources.
        /// </summary>
        public int MagicProtectionMod => (int) StatMods.Get(Stat.MagicProtectionMod);

        /// <summary>
        ///     Creature's affinity to the element lightning.
        /// </summary>
        public int ElementLightning => RaceData.ElementLightning + (int) StatMods.Get(Stat.ElementLightning);

        /// <summary>
        ///     Creature's affinity to the element fire.
        /// </summary>
        public int ElementFire => RaceData.ElementFire + (int) StatMods.Get(Stat.ElementFire);

        /// <summary>
        ///     Creature's affinity to the element ice.
        /// </summary>
        public int ElementIce => RaceData.ElementIce + (int) StatMods.Get(Stat.ElementIce);

        /// <summary>
        ///     Creature's toxicity level.
        /// </summary>
        public float Toxic { get; set; }

        /// <summary>
        ///     Creature's Str toxicity reduction.
        /// </summary>
        public float ToxicStr { get; set; }

        /// <summary>
        ///     Creature's Int toxicity reduction.
        /// </summary>
        public float ToxicInt { get; set; }

        /// <summary>
        ///     Creature's Dex toxicity reduction.
        /// </summary>
        public float ToxicDex { get; set; }

        /// <summary>
        ///     Creature's Will toxicity reduction.
        /// </summary>
        public float ToxicWill { get; set; }

        /// <summary>
        ///     Creature's Luck toxicity reduction.
        /// </summary>
        public float ToxicLuck { get; set; }

        public float LifeFoodMod
        {
            get => _lifeFoodMod;
            set => _lifeFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float ManaFoodMod
        {
            get => _manaFoodMod;
            set => _manaFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float StaminaFoodMod
        {
            get => _staminaFoodMod;
            set => _staminaFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float StrFoodMod
        {
            get => _strFoodMod;
            set => _strFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float IntFoodMod
        {
            get => _intFoodMod;
            set => _intFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float DexFoodMod
        {
            get => _dexFoodMod;
            set => _dexFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float WillFoodMod
        {
            get => _willFoodMod;
            set => _willFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float LuckFoodMod
        {
            get => _luckFoodMod;
            set => _luckFoodMod = Math2.Clamp(0, MaxFoodStatBonus, value);
        }

        public float StrBonus
        {
            get => _strBonus;
            set => _strBonus = Math2.Clamp(0, MaxStatBonus, value);
        }

        public float IntBonus
        {
            get => _intBonus;
            set => _intBonus = Math2.Clamp(0, MaxStatBonus, value);
        }

        public float DexBonus
        {
            get => _dexBonus;
            set => _dexBonus = Math2.Clamp(0, MaxStatBonus, value);
        }

        public float WillBonus
        {
            get => _willBonus;
            set => _willBonus = Math2.Clamp(0, MaxStatBonus, value);
        }

        public float LuckBonus
        {
            get => _luckBonus;
            set => _luckBonus = Math2.Clamp(0, MaxStatBonus, value);
        }

        // Defense/Protection
        // ------------------------------------------------------------------

        /// <summary>
        ///     Defense from monster xml
        /// </summary>
        public int DefenseBase => RaceData.Defense;

        public int DefenseBaseMod // Skills, Titles, etc?
            => (int) StatMods.Get(Stat.DefenseBaseMod);

        public int DefenseMod // eg Reforging? (yellow)
            => (int) StatMods.Get(Stat.DefenseMod);

        public int Defense
        {
            get
            {
                var result = DefenseBase + DefenseBaseMod + DefenseMod;

                // Str defense is displayed automatically on the client side
                result += (int) Math.Max(0, (Str - 10f) / 10f);

                // Add bonus for healing
                if (Skills.IsActive(SkillId.Healing))
                {
                    if (Skills.ActiveSkill.Stacks >= 1) result += 5;
                    if (Skills.ActiveSkill.Stacks >= 2) result += 3;
                    if (Skills.ActiveSkill.Stacks >= 3) result += 4;
                    if (Skills.ActiveSkill.Stacks >= 4) result += 4;
                    if (Skills.ActiveSkill.Stacks >= 5) result += 4;
                }

                return result;
            }
        }

        /// <summary>
        ///     Protect from monster xml
        /// </summary>
        public float ProtectionBase => RaceData.Protection;

        public float ProtectionBaseMod => StatMods.Get(Stat.ProtectionBaseMod);
        public float ProtectionMod => StatMods.Get(Stat.ProtectionMod);

        public float Protection
        {
            get
            {
                var result = ProtectionBase + ProtectionBaseMod + ProtectionMod;

                return result / 100f;
            }
        }

        public short ArmorPierce => (short) Math.Max(0, (Dex - 10) / 15);

        public float Life
        {
            get => _life;
            set
            {
                var before = _life;

                _life = Math2.Clamp(-LifeMax, LifeInjured, value);

                if (Region != Region.Limbo)
                    if (_life < 0 && before >= 0 && !Conditions.Has(ConditionsA.Deadly))
                        Conditions.Activate(ConditionsA.Deadly);
                    else if (_life >= 0 && before < 0 && Conditions.Has(ConditionsA.Deadly))
                        Conditions.Deactivate(ConditionsA.Deadly);
            }
        }

        public float Injuries
        {
            get => _injuries;
            set => _injuries = Math2.Clamp(0, LifeMax, value);
        }

        public float LifeMaxBase { get; set; }
        public float LifeMaxBaseSkill { get; set; }
        public float LifeMaxBaseTotal => LifeMaxBase + LifeMaxBaseSkill;
        public float LifeMaxMod => StatMods.Get(Stat.LifeMaxMod);
        public float LifeMax => Math.Max(1, LifeMaxBaseTotal + LifeMaxMod + LifeFoodMod);
        public float LifeInjured => LifeMax - _injuries;

        public float Mana
        {
            get => _mana;
            set => _mana = Math2.Clamp(0, ManaMax, value);
        }

        public float ManaMaxBase { get; set; }
        public float ManaMaxBaseSkill { get; set; }
        public float ManaMaxBaseTotal => ManaMaxBase + ManaMaxBaseSkill;
        public float ManaMaxMod => StatMods.Get(Stat.ManaMaxMod);
        public float ManaMax => Math.Max(1, ManaMaxBaseTotal + ManaMaxMod + ManaFoodMod);

        public float Stamina
        {
            get => _stamina;
            set => _stamina = Math2.Clamp(0, StaminaMax, value);
        }

        /// <summary>
        ///     The amount of stamina that's not usable because of hunger.
        /// </summary>
        /// <remarks>
        ///     While regen is limited to 50%, hunger can actually go higher.
        /// </remarks>
        public float Hunger
        {
            get => _hunger;
            set => _hunger = Math2.Clamp(0, StaminaMax, value);
        }

        public float StaminaMaxBase { get; set; }
        public float StaminaMaxBaseSkill { get; set; }
        public float StaminaMaxBaseTotal => StaminaMaxBase + StaminaMaxBaseSkill;
        public float StaminaMaxMod => StatMods.Get(Stat.StaminaMaxMod);
        public float StaminaMax => Math.Max(1, StaminaMaxBaseTotal + StaminaMaxMod + StaminaFoodMod);
        public float StaminaHunger => StaminaMax - _hunger;

        /// <summary>
        ///     Returns multiplicator to be used when regenerating stamina.
        /// </summary>
        public float StaminaRegenMultiplicator => Stamina < StaminaHunger ? 1f : 0.2f;

        /// <summary>
        ///     Returns the rigth hand weapon stamina usage, or bare hand stamina usage if no such weapon.
        /// </summary>
        public float RightHandStaminaUsage => RightHand != null ? RightHand.Data.StaminaUsage : BareHandStaminaUsage;

        /// <summary>
        ///     Returns the left hand weapon stamina usage if the creature is dual wielding, 0 otherwise.
        ///     <seealso cref="Creature.IsDualWielding" />
        /// </summary>
        public float LeftHandStaminaUsage => IsDualWielding ? LeftHand.Data.StaminaUsage : 0;

        // Parties
        // ------------------------------------------------------------------

        /// <summary>
        ///     The party the creature is a part of. If creature is not in a party,
        ///     a dummy party is created, consisting of only the creature.
        /// </summary>
        public Party Party { get; set; }

        /// <summary>
        ///     The number in the party this player occupies.
        /// </summary>
        public int PartyPosition { get; set; }

        /// <summary>
        ///     Returns true if creature is in an actual party.
        /// </summary>
        public bool IsInParty => Party.Id != 0;

        // Guild
        // ------------------------------------------------------------------

        public long GuildId => Guild != null ? Guild.Id : 0;
        public Guild Guild { get; set; }
        public GuildMember GuildMember { get; set; }

        public int PlayPoints { get; set; }

        /// <summary>
        ///     Called when creature is removed from the server.
        ///     (Killed NPC, disconnect, etc)
        /// </summary>
        public virtual void Dispose()
        {
            ChannelServer.Instance.Events.SecondsTimeTick -= OnSecondsTimeTick;
            ChannelServer.Instance.Events.MabiTick -= OnMabiTick;
            ChannelServer.Instance.Events.PlayTimeTick -= OnPlayTimeTick;

            // Stop rest, so character doesn't appear sitting anymore
            // and chair props are removed.
            // Do this in dispose because we can't expect a clean logout.
            if (Has(CreatureStates.SitDown))
            {
                var restHandler = ChannelServer.Instance.SkillManager.GetHandler<Rest>(SkillId.Rest);
                if (restHandler != null)
                    restHandler.Stop(this, Skills.Get(SkillId.Rest));
            }

            // Cancel any active skills
            if (Skills.ActiveSkill != null)
                Skills.CancelActiveSkill();

            Quests.Dispose();

            if (Temp.ActiveTrade != null)
                Temp.ActiveTrade.Cancel();

            if (Temp.ActiveEntrustment != null)
                Temp.ActiveEntrustment.Cancel();

            if (Temp.ActivePersonalShop != null)
                Temp.ActivePersonalShop.TakeDown();
        }

        // Events
        // ------------------------------------------------------------------

        /// <summary>
        ///     Raised when creature died, regardless of whether it's already
        ///     finished as well.
        /// </summary>
        public event Action<Creature, Creature> Death;

        /// <summary>
        ///     Raised when creature is finished. It's called if no finishing
        ///     happens as well, when going straight to being completely dead.
        /// </summary>
        public event Action<Creature, Creature> Finish;

        /// <summary>
        ///     Raised when creature levels up.
        /// </summary>
        /// <remarks>
        ///     Raised only once, even if there are multiple level ups.
        ///     Parameters:
        ///     - The creature leveling up.
        ///     - The level before the level up process.
        /// </remarks>
        public event Action<Creature, int> LeveledUp;

        /// <summary>
        ///     Loads race and handles some basic stuff, like adding regens.
        /// </summary>
        /// <remarks>
        ///     Has to be called before anything is actually done with the creature,
        ///     as it initializes the race data, the inventory, and other vital
        ///     parts.
        /// </remarks>
        public virtual void LoadDefault()
        {
            if (RaceId == 0)
                throw new Exception("Set race before calling LoadDefault.");

            RaceData = AuraData.RaceDb.Find(RaceId);
            if (RaceData == null)
            {
                // Try to default to Human
                RaceData = AuraData.RaceDb.Find(10000);
                if (RaceData == null)
                    throw new Exception("Unable to load race data, race '" + RaceId + "' not found.");

                Log.Warning("Creature.LoadDefault: Race '{0}' not found, using human instead.", RaceId);
            }

            if (InventoryWidth == 0)
                InventoryWidth = RaceData.InventoryWidth;

            if (InventoryHeight == 0)
                InventoryHeight = RaceData.InventoryHeight;

            // Add inventory
            Inventory.AddMainInventory();
            Inventory.StartLiveUpdate();

            // Add regens
            // The wiki says it's 0.125 life, but the packets have 0.12.
            Regens.Add(Stat.Life, 0.12f * RaceData.LifeRecoveryRate, LifeMax);
            Regens.Add(Stat.Mana, 0.05f, ManaMax);
            Regens.Add(Stat.Stamina, 0.4f, StaminaMax);
            if (ChannelServer.Instance.Conf.World.EnableHunger)
                Regens.Add(Stat.Hunger, 0.01f, StaminaMax);

            // Subscribe to time events
            ChannelServer.Instance.Events.SecondsTimeTick += OnSecondsTimeTick;
            ChannelServer.Instance.Events.MabiTick += OnMabiTick;

            if (IsPlayer)
                ChannelServer.Instance.Events.PlayTimeTick += OnPlayTimeTick;
        }

        public void Activate(CreatureStates state)
        {
            State |= state;
        }

        public void Activate(CreatureStatesEx state)
        {
            StateEx |= state;
        }

        public void Deactivate(CreatureStates state)
        {
            State &= ~state;
        }

        public void Deactivate(CreatureStatesEx state)
        {
            StateEx &= ~state;
        }

        public bool Has(CreatureStates state)
        {
            return (State & state) != 0;
        }

        public bool Is(RaceStands stand)
        {
            return (RaceData.Stand & stand) != 0;
        }

        /// <summary>
        ///     Returns current position.
        /// </summary>
        /// <returns></returns>
        public override Position GetPosition()
        {
            if (!IsMoving)
                return _position;

            var passed = (DateTime.Now - _moveStartTime).TotalSeconds;
            if (passed >= MoveDuration)
                return SetPosition(_destination.X, _destination.Y);

            var xt = _position.X + _movementX * passed;
            var yt = _position.Y + _movementY * passed;

            return new Position((int) xt, (int) yt);
        }

        /// <summary>
        ///     Sets region, x, and y, to be near entity.
        ///     Also randomizes direction.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="range"></param>
        public void SetLocationNear(Entity entity, int range)
        {
            var rnd = RandomProvider.Get();
            var pos = entity.GetPosition();
            var target = pos.GetRandomInRange(range, rnd);
            var dir = (byte) rnd.Next(256);

            SetLocation(entity.RegionId, target.X, target.Y);
            Direction = dir;
        }

        /// <summary>
        ///     Returns current destination.
        /// </summary>
        /// <returns></returns>
        public Position GetDestination()
        {
            return _destination;
        }

        /// <summary>
        ///     Sets position and destination.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Position SetPosition(int x, int y)
        {
            return _position = _destination = new Position(x, y);
        }

        /// <summary>
        ///     Sets RegionId and position.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetLocation(int region, int x, int y)
        {
            RegionId = region;
            SetPosition(x, y);
        }

        /// <summary>
        ///     Sets RegionId and position.
        /// </summary>
        /// <param name="loc"></param>
        public void SetLocation(Location loc)
        {
            SetLocation(loc.RegionId, loc.X, loc.Y);
        }

        /// <summary>
        ///     Starts movement from current position to destination.
        ///     Sends Running|Walking.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="walking"></param>
        public void Move(Position destination, bool walking)
        {
            _position = GetPosition();
            _destination = destination;
            _moveStartTime = DateTime.Now;
            IsWalking = walking;

            var diffX = _destination.X - _position.X;
            var diffY = _destination.Y - _position.Y;
            MoveDuration = Math.Sqrt(diffX * diffX + diffY * diffY) / GetSpeed();
            _movementX = diffX / MoveDuration;
            _movementY = diffY / MoveDuration;

            Direction = MabiMath.DirectionToByte(_movementX, _movementY);

            Send.Move(this, _position, _destination, walking);
        }

        /// <summary>
        ///     Returns current movement speed (x/s).
        /// </summary>
        /// <returns></returns>
        public float GetSpeed()
        {
            var speed = !IsWalking ? RaceData.RunningSpeed : RaceData.WalkingSpeed;

            // RaceSpeedFactor
            if (!IsWalking)
                speed *= RaceData.RunSpeedFactor;

            // The Zombie condition reduces speed to that of a Zombie.
            // We could query it from the speed db, but hardcoding is
            // more efficient, and it shouldn't be changing anyway.
            if (Conditions.Has(ConditionsC.Zombie))
                speed = ZombieSpeed;

            // Hurry condition
            if (!IsWalking && Conditions.Has(ConditionsC.Hurry))
            {
                var hurry = Conditions.GetExtraVal(169);
                speed *= 1 + hurry / 100f;
            }

            return speed;
        }

        /// <summary>
        ///     Stops movement, returning new current position.
        ///     Sends Force(Walk|Run)To.
        /// </summary>
        /// <returns></returns>
        public Position StopMove()
        {
            if (!IsMoving)
                return _position;

            var pos = GetPosition();
            SetPosition(pos.X, pos.Y);

            if (IsWalking)
                Send.ForceWalkTo(this, pos);
            else
                Send.ForceRunTo(this, pos);

            return pos;
        }

        /// <summary>
        ///     Stops movement and resets creature to the given position.
        /// </summary>
        /// <param name="pos"></param>
        public void ResetPosition(Position pos)
        {
            SetPosition(pos.X, pos.Y);
            Send.SetLocation(this, pos.X, pos.Y);
        }

        /// <summary>
        ///     Warps creature to target location,
        ///     returns false if warp is unsuccessful.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract bool Warp(int regionId, int x, int y);

        /// <summary>
        ///     Warps creature to target location,
        ///     returns false if warp is unsuccessful.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool Warp(Location loc)
        {
            return Warp(loc.RegionId, loc.X, loc.Y);
        }

        /// <summary>
        ///     Warps creature to target location,
        ///     returns false if warp is unsuccessful.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Warp(int regionId, Position position)
        {
            return Warp(regionId, position.X, position.Y);
        }

        /// <summary>
        ///     Warps creature to target location path,
        ///     returns false if warp is unsuccessful.
        /// </summary>
        /// <remarks>
        ///     Parses location paths like Uladh_main/SomeArea/SomeEvent
        ///     and calls Warp with the resulting values.
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
                return Warp(loc.RegionId, loc.X, loc.Y);
            }
            catch (Exception ex)
            {
                Send.ServerMessage(this, "Warp error: {0}", ex.Message);
                Log.Exception(ex, "Creature.Warp: Location parse error for '{0}'.", locationPath);
                return false;
            }
        }

        /// <summary>
        ///     Warps creature to given coordinates in its current region.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Jump(int x, int y)
        {
            SetPosition(x, y);
            Send.SetLocation(this, x, y);

            // TODO: Warp followers?
        }

        /// <summary>
        ///     Warps creature to given coordinates in its current region.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Jump(Position position)
        {
            Jump(position.X, position.Y);
        }

        /// <summary>
        ///     Called every 5 minutes, checks changes through food.
        /// </summary>
        /// <param name="time"></param>
        public void OnMabiTick(ErinnTime time)
        {
            UpdateBody();
            EquipmentDecay();
        }

        /// <summary>
        ///     Called every 9 minutes, increases play points.
        /// </summary>
        /// <param name="now"></param>
        private void OnPlayTimeTick(ErinnTime now)
        {
            PlayPoints++;
        }

        /// <summary>
        ///     Called once per second, running updates on different
        ///     creature components.
        /// </summary>
        /// <param name="obj"></param>
        private void OnSecondsTimeTick(ErinnTime time)
        {
            // TODO: General creature components in a list, with Update interface?
            Regens.OnSecondsTimeTick(time);
            StatMods.OnSecondsTimeTick(time);
            Conditions.OnSecondsTimeTick(time);
            Skills.OnSecondsTimeTick(time);

            PlayTime++;
        }

        /// <summary>
        ///     Called regularly to reduce equipments durability and give
        ///     proficiency to armors.
        /// </summary>
        /// <remarks>
        ///     http://wiki.mabinogiworld.com/view/Durability#Per_Tick
        ///     http://wiki.mabinogiworld.com/view/Proficiency
        ///     The dura loss actually doesn't seem to be fixed, I've logged
        ///     varying values on NA. However, *most* of the time the
        ///     values below are used.
        /// </remarks>
        private void EquipmentDecay()
        {
            var equipment = Inventory.GetMainEquipment(a => a.Durability > 0);
            var update = new List<Item>();

            // Go through all equipment to let armors gain prof, regardless
            // of any decay settings.
            foreach (var item in equipment)
            {
                // Dura loss
                // Going by the name "no_abrasion", I assume items with this
                // tag don't lose durability regularly.
                if (!ChannelServer.Instance.Conf.World.NoDurabilityLoss && !ChannelServer.Instance.Conf.World.NoDecay &&
                    !item.HasTag("/no_abrasion/"))
                {
                    var loss = 0;

                    switch (item.Info.Pocket)
                    {
                        case Pocket.Head:
                            loss = 3;
                            break;
                        case Pocket.Armor:
                            loss = 16;
                            break; // 6
                        case Pocket.Shoe:
                            loss = 14;
                            break; // 13
                        case Pocket.Glove:
                            loss = 10;
                            break; // 9
                        case Pocket.Robe:
                            loss = 10;
                            break;

                        case Pocket.RightHand1:
                        case Pocket.RightHand2:
                            loss = 3;
                            break;

                        case Pocket.LeftHand1:
                        case Pocket.LeftHand2:
                            loss = 6;
                            break;
                    }

                    if (loss != 0)
                    {
                        // Half dura loss if blessed
                        if (item.IsBlessed)
                            loss = Math.Max(1, loss / 2);

                        item.Durability -= loss;
                        update.Add(item);
                    }
                }

                // Armor prof
                if (item.Durability != 0 && item.Info.Pocket.IsMainArmor())
                {
                    var amount = Item.GetProficiencyGain(Age, ProficiencyGainType.Time);
                    Inventory.AddProficiency(item, amount);
                }
            }

            if (update.Count != 0)
                Send.ItemDurabilityUpdate(this, update);
        }

        /// <summary>
        ///     Called regularly to update body, based on what the creature ate.
        /// </summary>
        private void UpdateBody()
        {
            var weight = Temp.WeightFoodChange;
            var upper = Temp.UpperFoodChange;
            var lower = Temp.LowerFoodChange;
            var life = Temp.LifeFoodChange;
            var mana = Temp.ManaFoodChange;
            var stm = Temp.StaminaFoodChange;
            var str = Temp.StrFoodChange;
            var int_ = Temp.IntFoodChange;
            var dex = Temp.DexFoodChange;
            var will = Temp.WillFoodChange;
            var luck = Temp.LuckFoodChange;
            var changes = false;

            var sb = new StringBuilder();

            if (ChannelServer.Instance.Conf.World.YouAreWhatYouEat)
            {
                if (weight != 0)
                {
                    changes = true;
                    Weight += weight;
                    sb.Append(weight > 0
                        ? Localization.Get("You gained some weight.")
                        : Localization.Get("You lost some weight.") + "\r\n");
                }

                if (upper != 0)
                {
                    changes = true;
                    Upper += upper;
                    sb.Append(upper > 0
                        ? Localization.Get("Your upper body got bigger.")
                        : Localization.Get("Your upper body got slimmer.") + "\r\n");
                }

                if (lower != 0)
                {
                    changes = true;
                    Lower += lower;
                    sb.Append(lower > 0
                        ? Localization.Get("Your legs got bigger.")
                        : Localization.Get("Your legs got slimmer.") + "\r\n");
                }
            }

            if (life != 0)
            {
                changes = true;
                LifeFoodMod += life;
            }

            if (mana != 0)
            {
                changes = true;
                ManaFoodMod += mana;
            }

            if (stm != 0)
            {
                changes = true;
                StaminaFoodMod += stm;
            }

            if (str != 0)
            {
                changes = true;
                StrFoodMod += str;
            }

            if (int_ != 0)
            {
                changes = true;
                IntFoodMod += int_;
            }

            if (dex != 0)
            {
                changes = true;
                DexFoodMod += dex;
            }

            if (will != 0)
            {
                changes = true;
                WillFoodMod += will;
            }

            if (luck != 0)
            {
                changes = true;
                LuckFoodMod += luck;
            }

            if (!changes)
                return;

            Temp.WeightFoodChange = 0;
            Temp.UpperFoodChange = 0;
            Temp.LowerFoodChange = 0;
            Temp.LifeFoodChange = 0;
            Temp.ManaFoodChange = 0;
            Temp.StaminaFoodChange = 0;
            Temp.StrFoodChange = 0;
            Temp.IntFoodChange = 0;
            Temp.DexFoodChange = 0;
            Temp.WillFoodChange = 0;
            Temp.LuckFoodChange = 0;

            Send.StatUpdate(this, StatUpdateType.Private, Stat.LifeMaxFoodMod, Stat.ManaMaxFoodMod,
                Stat.StaminaMaxFoodMod, Stat.StrFoodMod, Stat.IntFoodMod, Stat.DexFoodMod, Stat.WillFoodMod,
                Stat.LuckFoodMod);
            Send.StatUpdate(this, StatUpdateType.Public, Stat.LifeMaxFoodMod);
            Send.CreatureBodyUpdate(this);

            if (sb.Length > 0)
                Send.Notice(this, sb.ToString());
        }

        /// <summary>
        ///     Returns true if creature is able to attack this creature.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public virtual bool CanTarget(Creature creature)
        {
            var attackerIsDead = IsDead;
            var targetIsDead = creature.IsDead;
            var attackerCanFinish = CanFinish(creature);
            var attackerIsTarget = creature == this;

            if (attackerIsDead || targetIsDead && !attackerCanFinish || attackerIsTarget)
                return false;

            return true;
        }

        /// <summary>
        ///     Returns whether this creature is eligible to finish the given
        ///     target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CanFinish(Creature target)
        {
            var finisherId = target.FinisherId;
            var isFinished = target.IsFinished;
            var isFinisher = finisherId == 0 || finisherId == Party.Id || Client.Creatures.ContainsKey(finisherId);

            return !isFinished && isFinisher;
        }

        /// <summary>
        ///     Returns the max distance the creature can have to attack.
        /// </summary>
        /// <remarks>
        ///     This might not be the 100% accurate formula, but it should be
        ///     good enough to work with for now.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        public int AttackRangeFor(Creature target)
        {
            var attackerRange = RaceData.AttackRange * BodyScale;
            var targetRange = target.RaceData.AttackRange * target.BodyScale;

            var result = 156f; // Default found in the client (for reference)

            if (attackerRange < 300 && targetRange < 300 || attackerRange >= 300 && attackerRange > targetRange)
                result = (attackerRange + targetRange) / 2;
            else
                result = targetRange;

            // A little something extra
            result += 25;

            return (int) result;
        }

        /// <summary>
        ///     Calculates random damage for the right hand (default).
        /// </summary>
        /// <remarks>
        ///     Method is used for bare hand attacks as well, if right hand is
        ///     empty, use bare hand attack values.
        ///     Uses bare hand damage if creature has 0 stamina.
        /// </remarks>
        /// <returns></returns>
        public virtual float GetRndRightHandDamage()
        {
            var min = AttackMinBase + AttackMinBaseMod;
            var max = AttackMaxBase + AttackMaxBaseMod;
            var balance = BalanceBase + BalanceBaseMod + RightBalanceMod;

            // Add potential weapon dmg only if creature has enough stamina
            if (RightHand != null && Stamina >= RightHand.Data.StaminaUsage)
            {
                min += RightAttackMinMod;
                max += RightAttackMaxMod;
            }

            return GetRndDamage(min, max, balance);
        }

        /// <summary>
        ///     Calculates random damage for the left hand (off-hand).
        /// </summary>
        /// <remarks>
        ///     Uses bare hand damage if creature has 0 stamina.
        /// </remarks>
        /// <returns></returns>
        public virtual float GetRndLeftHandDamage()
        {
            var min = AttackMinBase + AttackMinBaseMod;
            var max = AttackMaxBase + AttackMaxBaseMod;
            var balance = BalanceBase + BalanceBaseMod + LeftBalanceMod;

            // Add potential weapon dmg only if creature has enough stamina
            if (LeftHand != null && Stamina >= LeftHand.Data.StaminaUsage)
            {
                min += LeftAttackMinMod;
                max += LeftAttackMaxMod;
            }

            return GetRndDamage(min, max, balance);
        }

        /// <summary>
        ///     Calculates random damage with the given min/max and balance values.
        ///     Adds attack mods and stat bonuses automatically and randomizes
        ///     the balance.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="balance"></param>
        /// <returns></returns>
        protected virtual float GetRndDamage(float min, float max, int balance)
        {
            var balanceMultiplicator = GetRndBalance(balance) / 100f;

            min += AttackMinMod;
            min += Math.Max(0, Str - 10) / 3.0f;

            max += AttackMaxMod;
            max += Math.Max(0, Str - 10) / 2.5f;

            if (min > max)
                min = max;

            return min + (max - min) * balanceMultiplicator;
        }

        /// <summary>
        ///     Calculates the damage of left-and-right slots together
        /// </summary>
        /// <returns></returns>
        public float GetRndTotalDamage()
        {
            var balance = 0;
            if (RightHand == null)
            {
                balance = BalanceBase + BalanceBaseMod;
            }
            else
            {
                balance = RightBalanceMod;
                if (IsDualWielding)
                    balance = (balance + LeftBalanceMod) / 2;
            }

            var min = AttackMinBase + AttackMinBaseMod + RightAttackMinMod;
            if (IsDualWielding)
                min = (min + LeftAttackMinMod) / 2;

            var max = AttackMaxBase + AttackMaxBaseMod + RightAttackMaxMod;
            if (IsDualWielding)
                max = (max + LeftAttackMaxMod) / 2;

            return GetRndDamage(min, max, balance);
        }

        /// <summary>
        ///     Calculates random balance with given base balance, adding the
        ///     dex bonus along the way.
        /// </summary>
        /// <param name="baseBalance"></param>
        /// <returns></returns>
        public int GetRndBalance(int baseBalance)
        {
            var rnd = RandomProvider.Get();
            var balance = baseBalance;

            // Dex
            balance = (int) Math2.Clamp(0, 80, balance + (Dex - 10) / 4f);

            // Randomization
            var diff = 100 - balance;
            var min = balance - diff;
            var max = balance + diff;

            balance = rnd.Next(min, max + 1);

            return Math2.Clamp(0, 80, balance);
        }

        /// <summary>
        ///     Calculates random magic balance (0.0~1.0).
        /// </summary>
        /// <param name="baseBalance"></param>
        /// <returns></returns>
        public int GetRndMagicBalance(int baseBalance = BaseMagicBalance)
        {
            var rnd = RandomProvider.Get();
            var balance = baseBalance;

            // Int
            balance = (int) Math2.Clamp(0, 100, balance + (Int - 10) / 4f);

            // Randomization
            var diff = 100 - balance;
            var min = balance - diff;
            var max = balance + diff;

            balance = rnd.Next(min, max + 1);

            return Math2.Clamp(0, 100, balance);
        }

        /// <summary>
        ///     Calculates random base Magic damage for skill, using the given values.
        /// </summary>
        /// <remarks>
        ///     http://wiki.mabinogiworld.com/view/Stats#Magic_Damage
        /// </remarks>
        /// <param name="skill"></param>
        /// <param name="baseMin"></param>
        /// <param name="baseMax"></param>
        /// <returns></returns>
        public float GetRndMagicDamage(Skill skill, float baseMin, float baseMax)
        {
            var rnd = RandomProvider.Get();

            // Base damage
            var min = baseMin;
            var max = baseMax;

            // Bonus
            var factor = rnd.Between(skill.RankData.FactorMin, skill.RankData.FactorMax);
            var totalMagicAttack = MagicAttack + MagicAttackMod;

            var wandBonus = 0f;
            var chargeMultiplier = 0f;

            if (skill.Info.Id == SkillId.Icebolt && RightHand != null && RightHand.HasTag("/ice_wand/"))
                wandBonus = 5;
            else if (skill.Info.Id == SkillId.Firebolt && RightHand != null && RightHand.HasTag("/fire_wand/"))
                wandBonus = 5;
            else if (skill.Info.Id == SkillId.Lightningbolt && RightHand != null &&
                     RightHand.HasTag("/lightning_wand/"))
                wandBonus = 3.5f;

            if (skill.Info.Id == SkillId.Firebolt || skill.Info.Id == SkillId.IceSpear ||
                skill.Info.Id == SkillId.HailStorm)
                chargeMultiplier = skill.Stacks;

            var bonusDamage = (float) Math.Floor(wandBonus * (1 + chargeMultiplier)) + factor * totalMagicAttack;
            min += bonusDamage;
            max += bonusDamage;

            // Random balance multiplier
            var multiplier = GetRndMagicBalance() / 100f;

            if (min > max)
                min = max;

            return min + (max - min) * multiplier;
        }

        /// <summary>
        ///     Returns random base damage for a ranged attack,
        ///     e.g. Ranged Attack or Magnum Shot, based on race, weapon, etc.
        /// </summary>
        /// <returns></returns>
        public float GetRndRangedDamage()
        {
            // Base damage
            float min = AttackMinBase + AttackMinMod;
            float max = AttackMaxBase + AttackMaxMod;

            // Weapon
            min += RightHand == null ? 0 : RightHand.OptionInfo.AttackMin;
            max += RightHand == null ? 0 : RightHand.OptionInfo.AttackMax;

            // Dex bonus
            min += (Dex - 10) / 3.5f;
            max += (Dex - 10) / 2.5f;

            // Base balance
            var balance = RightHand == null ? BalanceBase + BalanceBaseMod : RightHand.OptionInfo.Balance;

            // Ranged balance bonus
            var skill = Skills.Get(SkillId.RangedAttack);
            if (skill != null)
                balance += (int) skill.RankData.Var5;

            // Random balance multiplier
            var multiplier = GetRndBalance(balance) / 100f;

            if (min > max)
                min = max;

            return min + (max - min) * multiplier;
        }

        /// <summary>
        ///     Applies damage to Life, kills creature if necessary.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="from"></param>
        public void TakeDamage(float damage, Creature from)
        {
            var lifeBefore = Life;

            Life -= damage;

            // Track hit
            if (from != null)
            {
                HitTracker tracker;
                lock (_hitTrackers)
                {
                    // Create new tracker if there is none yet
                    if (!_hitTrackers.TryGetValue(from.EntityId, out tracker))
                    {
                        var newId = Interlocked.Increment(ref _hitTrackerIds);
                        _hitTrackers[from.EntityId] = tracker = new HitTracker(newId, this, from);
                    }
                }
                tracker.RegisterHit(damage);
                _totalHits = Interlocked.Increment(ref _totalHits);
            }

            // Update equip
            var mainArmors = Inventory.GetMainEquipment(a => a.Info.Pocket.IsMainArmor());
            if (mainArmors.Length != 0)
            {
                // Select a random armor item to gain proficiency and lose
                // durability, as the one that was "hit" by the damage.
                var item = mainArmors.Random();

                // Give proficiency
                var profAmount = Item.GetProficiencyGain(Age, ProficiencyGainType.Damage);
                Inventory.AddProficiency(item, profAmount);

                // Reduce durability
                var duraAmount = RandomProvider.Get().Next(1, 30);
                Inventory.ReduceDurability(item, duraAmount);
            }

            // Kill if life too low
            if (Life < 0 && !ShouldSurvive(damage, from, lifeBefore))
                Kill(from);
        }

        /// <summary>
        ///     Returns true if creature should go into deadly by the attack.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="from"></param>
        /// <param name="lifeBefore"></param>
        /// <returns></returns>
        protected abstract bool ShouldSurvive(float damage, Creature from, float lifeBefore);

        /// <summary>
        ///     Kills creature. Returns true if it was killed and false if it
        ///     entered "finish mode".
        /// </summary>
        /// <param name="killer"></param>
        public virtual bool Kill(Creature killer)
        {
            var rnd = RandomProvider.Get();
            var pos = GetPosition();

            // Conditions
            if (Conditions.Has(ConditionsA.Deadly))
                Conditions.Deactivate(ConditionsA.Deadly);

            var wasAlive = !Has(CreatureStates.Dead);
            Activate(CreatureStates.Dead);

            // Kill events, fire once when the creature dies.
            if (wasAlive)
            {
                ChannelServer.Instance.Events.OnCreatureKilled(this, killer);
                if (killer != null && killer.IsPlayer)
                    ChannelServer.Instance.Events.OnCreatureKilledByPlayer(this, killer);
                Death.Raise(this, killer);
            }

            // Drop keys in case the monster isn't being finished yet
            DropKeys(killer, rnd, pos);

            // When a creature is killed, and the attacker is in a party,
            // the party's finisher rules come into effect. Depending on its
            // settings a finisher might be set, who gets to actually kill the
            // monster and gets assigned the drops.
            // If a finisher is set, the method returns, so nothing is done
            // but setting the creature to be dead. The next time we come here,
            // after the finisher attacked the monster again, the finisher id
            // won't be 0, and as such it won't return again, but continue
            // to the actual kill behavior.
            if (killer.IsInParty && FinisherId == 0)
            {
                if (killer.Party.Finish == PartyFinishRule.Anyone)
                {
                    SetFinisher(killer.Party.Id);
                }
                else if (killer.Party.Finish == PartyFinishRule.BiggestContributer)
                {
                    // Get top damage dealer and set them to be finisher,
                    // if they're still around and they aren't the killer.
                    // If they are the killer, we don't need a finish.
                    var hitTracker = GetTopDamageDealer();
                    if (hitTracker != null)
                    {
                        var finisher = hitTracker.Attacker;
                        if (finisher.Region == Region && finisher != killer)
                            SetFinisher(finisher.EntityId);
                    }
                }
                else if (killer.Party.Finish == PartyFinishRule.Turn)
                {
                    var finisher = killer.Party.GetNextFinisher();
                    if (finisher.Region == Region && finisher != killer)
                        SetFinisher(finisher.EntityId);
                }

                // Stop here if we just set a finisher
                if (FinisherId != 0)
                {
                    Send.IsNowDead(this);
                    return false;
                }
            }

            SetFinisher(0);
            Send.IsNowDead(this);

            // Finish events, fire when creature is finished.
            ChannelServer.Instance.Events.OnCreatureFinished(this, killer);
            if (killer != null && killer.IsPlayer)
                ChannelServer.Instance.Events.OnCreatureFinishedByPlayer(this, killer);
            Finish.Raise(this, killer);

            // Cancel active skill
            if (Skills.ActiveSkill != null)
                Skills.CancelActiveSkill();

            // Drops
            DropGold(killer, rnd, pos);
            DropItems(killer, rnd, pos);

            // DeadMenu
            DeadMenu.Update();

            return true;
        }

        /// <summary>
        ///     Sets finisher and sends SetFinisher.
        /// </summary>
        /// <param name="id"></param>
        protected void SetFinisher(long id)
        {
            FinisherId = id;
            if (id == 0)
                IsFinished = true;

            Send.SetFinisher(this, id);
        }

        /// <summary>
        ///     Drops creature's gold.
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="rnd"></param>
        /// <param name="pos"></param>
        private void DropGold(Creature killer, Random rnd, Position pos)
        {
            var goldDropChance = ChannelServer.Instance.Conf.World.GoldDropChance;

            // Add global bonus
            float goldRateBonus;
            string bonuses;
            if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(GlobalBonusStat.GoldDropRate,
                out goldRateBonus, out bonuses))
                goldDropChance *= goldRateBonus;

            // Check if drop
            if (rnd.NextDouble() >= goldDropChance)
                return;

            // Random base amount
            var amount = rnd.Next(Drops.GoldMin, Drops.GoldMax + 1);

            // Add global bonus
            float goldDropBonus;
            if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(GlobalBonusStat.GoldDropAmount,
                out goldDropBonus, out bonuses))
                amount = (int) (amount * goldDropBonus);

            if (amount > 0)
            {
                var finish = LuckyFinish.None;

                // Lucky Finish
                var luckyChance = rnd.NextDouble();

                // Sunday: Increase in lucky finish.
                // +5%, bonus is unofficial.
                if (ErinnTime.Now.Month == ErinnMonth.Imbolic)
                    luckyChance += 0.05;

                var hugeLuckyFinishChance = ChannelServer.Instance.Conf.World.HugeLuckyFinishChance;
                var bigLuckyFinishChance = ChannelServer.Instance.Conf.World.BigLuckyFinishChance;
                var luckyFinishChance = ChannelServer.Instance.Conf.World.LuckyFinishChance;

                // Add global bonus
                float luckyDropBonus;
                if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(
                    GlobalBonusStat.LuckyFinishRate, out luckyDropBonus, out bonuses))
                {
                    hugeLuckyFinishChance *= luckyDropBonus;
                    bigLuckyFinishChance *= luckyDropBonus;
                    luckyFinishChance *= luckyDropBonus;
                }

                if (luckyChance < hugeLuckyFinishChance)
                {
                    amount *= 100;
                    finish = LuckyFinish.Lucky;

                    Send.CombatMessage(killer, Localization.Get("Huge Lucky Finish!!"));
                    Send.Notice(killer, Localization.Get("Huge Lucky Finish!!"));
                }
                else if (luckyChance < bigLuckyFinishChance)
                {
                    amount *= 5;
                    finish = LuckyFinish.BigLucky;

                    Send.CombatMessage(killer, Localization.Get("Big Lucky Finish!!"));
                    Send.Notice(killer, Localization.Get("Big Lucky Finish!!"));
                }
                else if (luckyChance < luckyFinishChance)
                {
                    amount *= 2;
                    finish = LuckyFinish.HugeLucky;

                    Send.CombatMessage(killer, Localization.Get("Lucky Finish!!"));
                    Send.Notice(killer, Localization.Get("Lucky Finish!!"));
                }

                // If lucky finish
                if (finish != LuckyFinish.None)
                {
                    // Event
                    ChannelServer.Instance.Events.OnCreatureGotLuckyFinish(killer, finish, amount);

                    // Sunday: Increase in lucky bonus.
                    // +5%, bonus is unofficial.
                    if (ErinnTime.Now.Month == ErinnMonth.Imbolic)
                        amount = (int) (amount * 1.05f);
                }

                // Drop rate muliplicator
                amount = Math.Min(21000, Math2.MultiplyChecked(amount, ChannelServer.Instance.Conf.World.GoldDropRate));

                // Drop stack for stack
                var i = 0;
                var pattern = amount == 21000;
                do
                {
                    Position dropPos;
                    if (!pattern)
                    {
                        dropPos = pos.GetRandomInRange(Item.DropRadius, rnd);
                    }
                    else
                    {
                        dropPos = new Position(pos.X + CreatureDrops.MaxGoldPattern[i, 0],
                            pos.Y + CreatureDrops.MaxGoldPattern[i, 1]);
                        i++;
                    }

                    var gold = Item.CreateGold(Math.Min(1000, amount));
                    gold.Drop(Region, dropPos, 0, killer, false);

                    amount -= gold.Info.Amount;
                } while (amount > 0);
            }
        }

        /// <summary>
        ///     Drops creature's drop items.
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="rnd"></param>
        /// <param name="pos"></param>
        private void DropItems(Creature killer, Random rnd, Position pos)
        {
            // Normal
            DropItems(killer, rnd, pos, Drops.Drops);

            // Event
            var eventDrops = ChannelServer.Instance.GameEventManager.GlobalBonuses.GetDrops(this, killer);
            if (eventDrops.Count != 0)
                DropItems(killer, rnd, pos, eventDrops);

            // Static
            foreach (var item in Drops.StaticDrops)
                item.Drop(Region, pos, Item.DropRadius, killer, false);

            Drops.ClearStaticDrops();
        }

        /// <summary>
        ///     Drops only keys from creature's static drops.
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="rnd"></param>
        /// <param name="pos"></param>
        private void DropKeys(Creature killer, Random rnd, Position pos)
        {
            var keys = Drops.StaticDrops.Where(a => a.IsDungeonKey);

            foreach (var item in keys)
                item.Drop(Region, pos, Item.DropRadius, killer, false);

            Drops.RemoveFromStaticDrops(a => a.IsDungeonKey);
        }

        /// <summary>
        ///     Handles dropping of items in given collection.
        /// </summary>
        /// <param name="dataCollection"></param>
        private void DropItems(Creature killer, Random rnd, Position pos, IEnumerable<DropData> dataCollection)
        {
            var dropped = new HashSet<int>();
            foreach (var dropData in dataCollection)
            {
                if (dropData == null || !AuraData.ItemDb.Exists(dropData.ItemId))
                {
                    Log.Warning("Creature.DropItems: Invalid drop '{0}' from '{1}'.",
                        dropData == null ? "null" : dropData.ItemId.ToString(), RaceId);
                    continue;
                }

                // Check feature
                if (!string.IsNullOrWhiteSpace(dropData.Feature) && !AuraData.FeaturesDb.IsEnabled(dropData.Feature))
                    continue;

                // Get chance
                var dropRate = dropData.Chance;
                var dropChance = rnd.NextDouble() * 100;
                var month = ErinnTime.Now.Month;

                // Add global bonus
                float itemDropBonus;
                string bonuses;
                if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(
                    GlobalBonusStat.ItemDropRate, out itemDropBonus, out bonuses))
                    dropRate *= itemDropBonus;

                // Tuesday: Increase in dungeon item drop rate.
                // Wednesday: Increase in item drop rate from animals and nature.
                // +50%, bonus is unofficial.
                if (month == ErinnMonth.Baltane && Region.IsDungeon ||
                    month == ErinnMonth.AlbanHeruin && !Region.IsDungeon)
                    dropRate *= 1.5f;

                // Add conf
                dropRate *= ChannelServer.Instance.Conf.World.DropRate;

                if (dropChance < dropRate)
                {
                    // Only drop any item once
                    if (dropped.Contains(dropData.ItemId))
                        continue;

                    var item = new Item(dropData);
                    item.ModifyEquipStats(rnd);
                    item.Drop(Region, pos, Item.DropRadius, killer, false);

                    dropped.Add(dropData.ItemId);
                }
            }
        }

        /// <summary>
        ///     Increases exp and levels up creature if appropriate.
        /// </summary>
        /// <param name="val"></param>
        public void GiveExp(long val)
        {
            Exp += val;

            var levelStats = AuraData.StatsLevelUpDb.Find(RaceId, Age);
            if (levelStats == null)
                if ((levelStats = AuraData.StatsLevelUpDb.Find(10000, 17)) == null)
                    Log.Error("Creature.GiveExp: No valid level up stats found for race {0}, age {1}.", RaceId, Age);

            var prevLevel = Level;
            float ap = AbilityPoints;
            var life = LifeMaxBase;
            var mana = ManaMaxBase;
            var stamina = StaminaMaxBase;
            var str = StrBase;
            var int_ = IntBase;
            var dex = DexBase;
            var will = WillBase;
            var luck = LuckBase;

            while (Level < AuraData.ExpDb.MaxLevel && Exp >= AuraData.ExpDb.GetTotalForNextLevel(Level))
            {
                Level++;
                TotalLevel++;

                if (levelStats == null)
                    continue;

                var addAp = levelStats.AP;

                // Add global bonus
                float bonusMultiplier;
                string bonuses;
                if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(GlobalBonusStat.LevelUpAp,
                    out bonusMultiplier, out bonuses))
                    addAp = (int) (addAp * bonusMultiplier);

                // Add conf
                addAp = (int) (addAp * ChannelServer.Instance.Conf.World.LevelApRate);

                AbilityPoints += (short) addAp;
                LifeMaxBase += levelStats.Life;
                ManaMaxBase += levelStats.Mana;
                StaminaMaxBase += levelStats.Stamina;
                StrBase += levelStats.Str;
                IntBase += levelStats.Int;
                DexBase += levelStats.Dex;
                WillBase += levelStats.Will;
                LuckBase += levelStats.Luck;

                PlayPoints += 5;
            }

            // Only notify on level up
            if (prevLevel < Level)
            {
                FullHeal();

                Send.StatUpdateDefault(this);
                Send.LevelUp(this);

                // Only send aquire if stat crosses the X.0 border.
                // Eg, 50.9 -> 51.1
                float diff = 0;
                if ((diff = AbilityPoints - (int) ap) >= 1) Send.SimpleAcquireInfo(this, "ap", diff);
                if ((diff = LifeMaxBase - (int) life) >= 1) Send.SimpleAcquireInfo(this, "life", diff);
                if ((diff = ManaMaxBase - (int) mana) >= 1) Send.SimpleAcquireInfo(this, "mana", diff);
                if ((diff = StaminaMaxBase - (int) stamina) >= 1) Send.SimpleAcquireInfo(this, "stamina", diff);
                if ((diff = StrBase - (int) str) >= 1) Send.SimpleAcquireInfo(this, "str", diff);
                if ((diff = IntBase - (int) int_) >= 1) Send.SimpleAcquireInfo(this, "int", diff);
                if ((diff = DexBase - (int) dex) >= 1) Send.SimpleAcquireInfo(this, "dex", diff);
                if ((diff = WillBase - (int) will) >= 1) Send.SimpleAcquireInfo(this, "will", diff);
                if ((diff = LuckBase - (int) luck) >= 1) Send.SimpleAcquireInfo(this, "luck", diff);

                ChannelServer.Instance.Events.OnCreatureLevelUp(this);
                LeveledUp.Raise(this, prevLevel);
            }
            else
            {
                Send.StatUpdate(this, StatUpdateType.Private, Stat.Experience);
            }
        }

        /// <summary>
        ///     Increases age by years and sends update packets.
        /// </summary>
        /// <param name="years"></param>
        public void AgeUp(short years)
        {
            if (years < 0 || Age + years > short.MaxValue)
                return;

            float life = 0, mana = 0, stamina = 0, str = 0, dex = 0, int_ = 0, will = 0, luck = 0;
            var ap = 0;

            var oldAge = Age;
            var newAge = Age + years;
            while (Age < newAge)
            {
                // Increase age before requestin statUp, we want the stats
                // for the next age.
                Age++;

                var statUp = AuraData.StatsAgeUpDb.Find(RaceId, Age);
                if (statUp == null)
                    if ((statUp = AuraData.StatsAgeUpDb.Find(10000, 17)) == null)
                    {
                        Log.Error("Creature.AgeUp: No valid age up stats found for race {0}, age {1}.", RaceId, Age);
                    }
                    else
                    {
                        // Only warn when creature was a player, we'll let NPCs fall
                        // back to Human 17 silently.
                        if (IsPlayer)
                            Log.Warning(
                                "Creature.AgeUp: Age up stats missing for race {0}, age {1}. Falling back to Human 17.",
                                RaceId, Age);
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
            LifeMaxBase += life;
            Life += life;
            ManaMaxBase += mana;
            Mana += mana;
            StaminaMaxBase += stamina;
            Stamina += stamina;
            StrBase += str;
            DexBase += dex;
            IntBase += int_;
            WillBase += will;
            LuckBase += luck;
            AbilityPoints += (short) Math2.Clamp(0, short.MaxValue, ap * ChannelServer.Instance.Conf.World.AgeApRate);

            LastAging = DateTime.Now;

            if (this is Character)
                Height = Math.Min(1.0f, 1.0f / 7.0f * (Age - 10.0f)); // 0 ~ 1.0

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
            Send.AgeUpEffect(this, Age);

            ChannelServer.Instance.Events.OnCreatureAged(this, oldAge);
        }

        /// <summary>
        ///     Heals all life, mana, stamina, hunger, and wounds and updates client.
        /// </summary>
        public void FullHeal()
        {
            Injuries = 0;
            Hunger = 0;
            Life = LifeMax;
            Mana = ManaMax;
            Stamina = StaminaMax;

            Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.Stamina, Stat.Hunger,
                Stat.Mana);
            Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
        }

        /// <summary>
        ///     Fully heals life and updates client.
        /// </summary>
        public void FullLifeHeal()
        {
            Injuries = 0;
            Life = LifeMax;

            Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured);
            Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
        }

        /// <summary>
        ///     Changes life and sends stat update.
        /// </summary>
        /// <param name="amount"></param>
        public void ModifyLife(float amount)
        {
            Life += amount;
            Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured);
            Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured);
        }

        /// <summary>
        ///     Increases AP and updates client.
        /// </summary>
        /// <param name="amount"></param>
        public void GiveAp(int amount)
        {
            AbilityPoints += (short) Math2.Clamp(short.MinValue, short.MaxValue, amount);
            Send.StatUpdate(this, StatUpdateType.Private, Stat.AbilityPoints);
        }

        /// <summary>
        ///     Revives creature.
        /// </summary>
        /// <param name="option">Determines the penalty and stat recovery.</param>
        public void Revive(ReviveOptions option)
        {
            if (!IsDead)
                return;

            // Get and check exp penalty
            // "Here" wil be disabled by the client if not enough exp are
            // available, nothing else though, so we send an error message
            // if creature doesn't have enough exp, instead of issuing a
            // warning.
            var expPenalty = DeadMenu.GetExpPenalty(Level, option);
            var minExp = AuraData.ExpDb.GetTotalForNextLevel(Level - 2);

            // Friday: Decrease in penalties if knocked unconscious.
            // -5%, bonus is unofficial.
            // TODO: Does the client subtract the bonus on its side?
            //   Check on Friday.
            if (ErinnTime.Now.Month == ErinnMonth.AlbanElved)
                expPenalty = (int) (expPenalty * 0.95f);

            if (Exp - expPenalty < minExp)
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
                case ReviveOptions.TirNaNog:
                    // 100% life and 50% injury recovery
                    Injuries -= Injuries * 0.50f;
                    Life = LifeMax;
                    break;

                case ReviveOptions.Here:
                    // 5 life recovery and 50% additional injuries
                    Injuries += LifeInjured * 0.50f;
                    Life = 5;
                    break;

                case ReviveOptions.HereNoPenalty:
                    // 100% life and 100% injury recovery
                    Injuries = 0;
                    Life = LifeMax;
                    break;

                case ReviveOptions.ArenaLobby:
                case ReviveOptions.ArenaWaitingRoom:
                    // 100% life, 100% injury, and 100% stamina recovery
                    Injuries = 0;
                    Life = LifeMax;
                    Stamina = StaminaMax;
                    break;

                case ReviveOptions.ArenaSide:
                    // 50% life, 20% injury, and 50% stamina recovery
                    Injuries -= Injuries * 0.20f;
                    Life = LifeMax * 0.50f;
                    Stamina = StaminaMax * 0.50f;
                    break;

                case ReviveOptions.InCamp:
                case ReviveOptions.StatueOfGoddess:
                    // 25% life recovery and 10% additional injuries
                    Injuries = Math2.Clamp(0, LifeMax * 0.75f, Injuries + LifeMax * 0.10f);
                    Life = LifeMax * 0.25f;
                    break;

                case ReviveOptions.PhoenixFeather:
                    // Only set life if life is not at max, since creatures
                    // will keep their life if they leveled up while dead.
                    if (Life < LifeMax)
                    {
                        // 10% additional injuries
                        Injuries += LifeInjured * 0.10f;
                        Life = 1;
                    }
                    break;

                case ReviveOptions.WaitForRescue:
                    DeadMenu.Options ^= ReviveOptions.PhoenixFeather;
                    Send.DeadFeather(this);
                    Send.Revived(this);
                    return;

                case ReviveOptions.NaoStone:
                    DeadMenu.Options = ReviveOptions.NaoStoneRevive;
                    Send.DeadFeather(this);
                    Send.NaoRevivalEntrance(this);
                    Send.Revived(this);
                    return;

                case ReviveOptions.NaoStoneRevive:
                    // First try beginner stones, then normals
                    var item = Inventory.GetItem(a => a.HasTag("/notTransServer/nao_coupon/"), StartAt.BottomRight);
                    if (item == null)
                    {
                        item = Inventory.GetItem(a => a.HasTag("/nao_coupon/"), StartAt.BottomRight);
                        if (item == null)
                        {
                            Log.Error("Creature.Revive: Unable to remove Nao Soul Stone, none found.");
                            return;
                        }
                    }

                    // 100% life and 100% injury recovery
                    Injuries = 0;
                    Life = LifeMax;

                    // Blessing of all items
                    BlessAll();

                    // Remove Soul Stone
                    Inventory.Decrement(item);

                    Send.NaoRevivalExit(this);
                    break;

                default:
                    Log.Warning("Creature.Revive: Unknown revive option: {0}", option);

                    // Fallback, set Life to something positive.
                    if (Life <= 1)
                        Life = 1;
                    break;
            }

            Deactivate(CreatureStates.Dead);
            DeadMenu.Clear();

            if (expPenalty != 0)
            {
                Exp = Math.Max(minExp, Exp - expPenalty);
                Send.StatUpdate(this, StatUpdateType.Private, Stat.Experience);
            }

            Send.RemoveDeathScreen(this);
            Send.StatUpdate(this, StatUpdateType.Private, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod,
                Stat.Stamina, Stat.Hunger);
            Send.StatUpdate(this, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod);
            Send.RiseFromTheDead(this);
            Send.DeadFeather(this);
            Send.Revived(this);
        }

        /// <summary>
        ///     Returns the power rating (Weak, Boss, etc) of
        ///     compareCreature towards creature.
        /// </summary>
        /// <param name="compareCreature">Creature to compare to</param>
        /// <returns></returns>
        public PowerRating GetPowerRating(Creature compareCreature)
        {
            var cp = CombatPower;
            var otherCp = compareCreature.CombatPower;

            var result = PowerRating.Boss;

            if (otherCp < cp * 0.8f) result = PowerRating.Weakest;
            else if (otherCp < cp * 1.0f) result = PowerRating.Weak;
            else if (otherCp < cp * 1.4f) result = PowerRating.Normal;
            else if (otherCp < cp * 2.0f) result = PowerRating.Strong;
            else if (otherCp < cp * 3.0f) result = PowerRating.Awful;

            // Weaken condition
            if (Conditions.Has(ConditionsA.Weaken))
            {
                var levels = 1;
                var wkn_lv = Conditions.GetExtraField(31, "WKN_LV");
                if (wkn_lv != null)
                    levels = (byte) wkn_lv;

                result += levels;
            }

            if (result > PowerRating.Boss)
                result = PowerRating.Boss;

            return result;
        }

        /// <summary>
        ///     Calculates right (or bare) hand crit chance, taking stat bonuses
        ///     and given protection into consideration. Capped at 0~30.
        /// </summary>
        /// <param name="protection"></param>
        /// <returns></returns>
        public float GetRightCritChance(float protection)
        {
            var crit = CriticalBase + CriticalBaseMod + RightCriticalMod;
            return GetCritChance(crit, protection);
        }

        /// <summary>
        ///     Calculates left hand crit chance, taking stat bonuses
        ///     and given protection into consideration. Capped at 0~30.
        /// </summary>
        /// <param name="protection"></param>
        /// <returns></returns>
        public float GetLeftCritChance(float protection)
        {
            if (LeftHand == null)
                return 0;

            return GetCritChance(LeftCriticalMod, protection);
        }

        /// <summary>
        ///     Calculates total crit chance, taking stat bonuses
        ///     and given protection and bonus into consideration.
        ///     Capped at 0~30.
        /// </summary>
        /// <param name="protection">Protection to subtract from crit.</param>
        /// <returns></returns>
        public float GetTotalCritChance(float protection)
        {
            return GetTotalCritChance(protection, false);
        }

        /// <summary>
        ///     Calculates total crit chance, taking stat bonuses
        ///     and given protection and bonus into consideration.
        ///     Capped at 0~30.
        /// </summary>
        /// <param name="protection">Protection to subtract from crit.</param>
        /// <param name="magic">If true, weapon crit bonuses only apply if weapon is a wand.</param>
        /// <returns></returns>
        public float GetTotalCritChance(float protection, bool magic)
        {
            var crit = 0f;

            if (RightHand == null || magic && !RightHand.HasTag("/weapon/wand/"))
            {
                // Use base crit if no weapon or no staff for magic is equipped.
                crit = CriticalBase + CriticalBaseMod;
            }
            else
            {
                // Get base crit from weapon.
                crit = RightCriticalMod;

                // Get average crit from both weapons if a second one
                // is equipped.
                if (LeftHand != null)
                    crit = (crit + LeftCriticalMod) / 2;
            }

            return GetCritChance(crit, protection);
        }

        /// <summary>
        ///     Adds stat bonuses to base and calculates crit chance,
        ///     taking protection into consideration. Capped at 0~30.
        /// </summary>
        /// <param name="baseCritical"></param>
        /// <param name="protection"></param>
        /// <returns></returns>
        private float GetCritChance(float baseCritical, float protection)
        {
            baseCritical += (Will - 10) / 10f;
            baseCritical += (Luck - 10) / 5f;

            // Sunday: Increase in critical hit rate.
            // +5%, bonus is unofficial.
            if (ErinnTime.Now.Month == ErinnMonth.Imbolic)
                baseCritical += 5;

            baseCritical -= protection;

            return Math2.Clamp(0, 30, baseCritical);
        }

        /// <summary>
        ///     Returns Rest pose based on skill's rank.
        /// </summary>
        /// <returns></returns>
        public byte GetRestPose()
        {
            byte pose = 0;

            var skill = Skills.Get(SkillId.Rest);
            if (skill != null)
                if (skill.Info.Rank >= SkillRank.R9)
                    pose = 4;

            return pose;
        }

        /// <summary>
        ///     Sets new position for target, based on attacker's position
        ///     and the distance, takes collision into account.
        /// </summary>
        /// <param name="target">Entity to be knocked back</param>
        /// <param name="distance">Distance to knock back the target</param>
        /// <returns>New position</returns>
        public Position Shove(Creature target, int distance)
        {
            var attackerPosition = GetPosition();
            var targetPosition = target.GetPosition();

            var newPos = attackerPosition.GetRelative(targetPosition, distance);

            Position intersection;
            if (target.Region.Collisions.Find(targetPosition, newPos, out intersection))
                newPos = targetPosition.GetRelative(intersection, -50);

            target.SetPosition(newPos.X, newPos.Y);

            return newPos;
        }

        /// <summary>
        ///     Sets new position for creature, based on entity's position
        ///     and the distance, takes collision into account.
        /// </summary>
        /// <param name="entity">Source of the force (attacker, prop)</param>
        /// <param name="distance">Distance to knock back the target</param>
        /// <returns>New position</returns>
        public Position GetShoved(Entity entity, int distance)
        {
            var attackerPosition = entity.GetPosition();
            var targetPosition = GetPosition();

            var newPos = attackerPosition.GetRelative(targetPosition, distance);

            Position intersection;
            if (Region.Collisions.Find(targetPosition, newPos, out intersection))
                newPos = targetPosition.GetRelative(intersection, -50);

            SetPosition(newPos.X, newPos.Y);

            return newPos;
        }

        /// <summary>
        ///     Returns true if creature's race data has the tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public override bool HasTag(string tag)
        {
            if (RaceData == null)
                return false;

            return RaceData.HasTag(tag);
        }

        /// <summary>
        ///     Returns targetable creatures in given range around creature.
        /// </summary>
        /// <param name="range">Radius around position.</param>
        /// <param name="options">Options to change the result.</param>
        /// <returns></returns>
        public ICollection<Creature> GetTargetableCreaturesInRange(int range,
            TargetableOptions options = TargetableOptions.None)
        {
            return GetTargetableCreaturesAround(GetPosition(), range, options);
        }

        /// <summary>
        ///     Returns targetable creatures in given range around position.
        ///     Optionally factors in attack range.
        /// </summary>
        /// <param name="position">Reference position.</param>
        /// <param name="range">Radius around position.</param>
        /// <param name="options">Options to change the result.</param>
        /// <returns></returns>
        public ICollection<Creature> GetTargetableCreaturesAround(Position position, int range,
            TargetableOptions options = TargetableOptions.None)
        {
            var targetable = Region.GetCreatures(target =>
            {
                var targetPos = target.GetPosition();
                var radius = range;
                if ((options & TargetableOptions.AddAttackRange) != 0)
                    radius += AttackRangeFor(target) / 2;

                return target != this // Exclude creature
                       && CanTarget(target) // Check targetability
                       && (!Has(CreatureStates.Npc) || !target.Has(CreatureStates.Npc) ||
                           Target == target) // Allow NPC on NPC only if it's the creature's target
                       && targetPos.InRange(position, radius) // Check range
                       && ((options & TargetableOptions.IgnoreWalls) != 0 ||
                           !Region.Collisions.Any(position, targetPos)) // Check collisions between positions
                       && !target.Conditions.Has(ConditionsA.Invisible); // Check visiblility (GM)
            });

            return targetable;
        }

        /// <summary>
        ///     Returns targetable creatures in cone, based on the
        ///     given parameters, with direction of cone being based on the
        ///     creature's and the target's position.
        /// </summary>
        /// <param name="position">Pointy end of the cone.</param>
        /// <param name="targetPosition">Target's position, used as reference for the direction of the cone.</param>
        /// <param name="radius">Cone's radius.</param>
        /// <param name="angle">Cone's angle in degree.</param>
        /// <param name="options">Options to change the result.</param>
        /// <returns></returns>
        public ICollection<Creature> GetTargetableCreaturesInCone(Position targetPosition, float radius, float angle,
            TargetableOptions options = TargetableOptions.None)
        {
            var position = GetPosition();
            var direction = position.GetDirection(targetPosition);
            return GetTargetableCreaturesInCone(position, direction, radius, angle, options);
        }

        /// <summary>
        ///     Returns targetable creatures in cone, based on the
        ///     given parameters, with direction of cone being based on the
        ///     given positions.
        /// </summary>
        /// <param name="position">Pointy end of the cone.</param>
        /// <param name="targetPosition">Target's position, used as reference for the direction of the cone.</param>
        /// <param name="radius">Cone's radius.</param>
        /// <param name="angle">Cone's angle in degree.</param>
        /// <param name="options">Options to change the result.</param>
        /// <returns></returns>
        public ICollection<Creature> GetTargetableCreaturesInCone(Position position, Position targetPosition,
            float radius, float angle, TargetableOptions options = TargetableOptions.None)
        {
            var direction = position.GetDirection(targetPosition);
            return GetTargetableCreaturesInCone(position, direction, radius, angle, options);
        }

        /// <summary>
        ///     Returns targetable creatures in cone, based on the
        ///     given parameters.
        /// </summary>
        /// <param name="position">Pointy end of the cone.</param>
        /// <param name="direction">Cone's direction as radian.</param>
        /// <param name="radius">Cone's radius.</param>
        /// <param name="angle">Cone's angle in degree.</param>
        /// <param name="options">Options to change the result.</param>
        /// <returns></returns>
        public ICollection<Creature> GetTargetableCreaturesInCone(Position position, float direction, float radius,
            float angle, TargetableOptions options = TargetableOptions.None)
        {
            if (radius == 0 || angle == 0)
                return new Creature[0];

            angle = MabiMath.DegreeToRadian((int) angle);

            var targetable = Region.GetCreatures(target =>
            {
                var targetPos = target.GetPosition();
                if ((options & TargetableOptions.AddAttackRange) != 0)
                    radius += AttackRangeFor(target) / 2;

                return target != this // Exclude creature
                       && CanTarget(target) // Check targetability
                       && (!Has(CreatureStates.Npc) || !target.Has(CreatureStates.Npc) ||
                           Target == target) // Allow NPC on NPC only if it's the creature's target
                       && targetPos.InCone(position, direction, (int) radius, angle) // Check position
                       && ((options & TargetableOptions.IgnoreWalls) != 0 ||
                           !Region.Collisions.Any(position, targetPos)) // Check collisions between positions
                       && !target.Conditions.Has(ConditionsA.Invisible); // Check visiblility (GM)
            });

            return targetable;
        }

        /// <summary>
        ///     Aggroes target, setting target and putting creature in battle stance.
        /// </summary>
        /// <param name="creature"></param>
        public abstract void Aggro(Creature target);

        /// <summary>
        ///     Disposes creature and removes it from its current region.
        /// </summary>
        public override void Disappear()
        {
            Dispose();
            RemoveFromRegion();

            base.Disappear();
        }

        /// <summary>
        ///     Removes creature from its current region.
        /// </summary>
        public void RemoveFromRegion()
        {
            if (Region != Region.Limbo)
                Region.RemoveCreature(this);
        }

        /// <summary>
        ///     Turns creature in direction of position.
        /// </summary>
        /// <param name="pos"></param>
        public void TurnTo(Position pos)
        {
            var creaturePos = GetPosition();
            var x = pos.X - creaturePos.X;
            var y = pos.Y - creaturePos.Y;

            TurnTo(x, y);
        }

        /// <summary>
        ///     Turns creature in given direction.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TurnTo(float x, float y)
        {
            Direction = MabiMath.DirectionToByte(x, y);
            Send.TurnTo(this, x, y);
        }

        /// <summary>
        ///     Starts dialog with NPC, returns false if NPC couldn't be found.
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
            Client.NpcSession.StartTalk(target, this);

            return true;
        }

        /// <summary>
        ///     Adds item to creature's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool GiveItem(int itemId, int amount = 1)
        {
            return Inventory.InsertStacks(itemId, amount);
        }

        /// <summary>
        ///     Adds item to creature's inventory.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool GiveItem(Item item)
        {
            return Inventory.Insert(item, true);
        }

        /// <summary>
        ///     Adds item to creature's inventory and shows it above head.
        /// </summary>
        /// <param name="item"></param>
        public void GiveItemWithEffect(Item item)
        {
            GiveItem(item);
            Send.Effect(this, Effect.PickUpItem, (byte) 1, item.Info.Id, item.Info.Color1, item.Info.Color2,
                item.Info.Color3);
        }

        /// <summary>
        ///     Adds item to creature's inventory and shows an acquire window.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        public void AcquireItem(int itemId, int amount = 1)
        {
            GiveItem(itemId, amount);
            Send.AcquireItemInfo(this, itemId, amount);
        }

        /// <summary>
        ///     Adds item to creature's inventory and shows an acquire window.
        /// </summary>
        /// <param name="item"></param>
        public void AcquireItem(Item item)
        {
            GiveItem(item);
            Send.AcquireItemInfo(this, item.EntityId);
        }

        /// <summary>
        ///     Adds warp scroll to creature's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="portal"></param>
        /// <returns></returns>
        public bool GiveWarpScroll(int itemId, string portal)
        {
            return GiveItem(Item.CreateWarpScroll(itemId, portal));
        }

        /// <summary>
        ///     Adds production pattern to creature's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="formId"></param>
        /// <param name="useCount"></param>
        /// <returns></returns>
        public bool GivePattern(int itemId, int formId, int useCount)
        {
            return GiveItem(Item.CreatePattern(itemId, formId, useCount));
        }

        /// <summary>
        ///     Adds given amount of gold to the creature's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool GiveGold(int amount)
        {
            return Inventory.AddGold(amount);
        }

        /// <summary>
        ///     Removes given amount of gold from the creature's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool RemoveGold(int amount)
        {
            return Inventory.RemoveGold(amount);
        }

        /// <summary>
        ///     Returns true if creature has at least the given amount of gold
        ///     in its inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool HasGold(int amount)
        {
            return Inventory.HasGold(amount);
        }

        /// <summary>
        ///     Checks if player has at least the given amount of the item.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool HasItem(int itemId, int amount = 1)
        {
            return Inventory.Has(itemId, amount);
        }

        /// <summary>
        ///     Returns the amount of the given item the creature has in its
        ///     inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public int CountItems(int itemId)
        {
            return Inventory.Count(itemId);
        }

        /// <summary>
        ///     Removes items with the given id from the creature's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool RemoveItem(int itemId, int amount = 1)
        {
            return Inventory.Remove(itemId, amount);
        }

        /// <summary>
        ///     Activates given Locks for creature.
        /// </summary>
        /// <remarks>
        ///     Some locks are lifted automatically on Warp, SkillComplete,
        ///     and SkillCancel.
        ///     Only sending the locks when they actually changed can cause problems,
        ///     e.g. if a lock is removed during a cutscene (skill running out)
        ///     the unlock after the cutscene isn't sent.
        ///     The client actually has counted locks, unlike us atm.
        ///     Implementing those will fix the problem. TODO.
        /// </remarks>
        /// <param name="locks">Locks to activate.</param>
        /// <param name="updateClient">Sends CharacterLock to client if true.</param>
        /// <returns>Creature's current lock value after activating given locks.</returns>
        public Locks Lock(Locks locks, bool updateClient = false)
        {
            var prev = Locks;
            Locks |= locks;

            if (updateClient /*&& prev != this.Locks*/)
                Send.CharacterLock(this, locks);

            return Locks;
        }

        /// <summary>
        ///     Deactivates given Locks for creature.
        /// </summary>
        /// <remarks>
        ///     Unlocking movement on the client apparently resets skill stuns.
        /// </remarks>
        /// <param name="locks">Locks to deactivate.</param>
        /// <param name="updateClient">Sends CharacterUnlock to client if true.</param>
        /// <returns>Creature's current lock value after deactivating given locks.</returns>
        public Locks Unlock(Locks locks, bool updateClient = false)
        {
            var prev = Locks;
            Locks &= ~locks;

            if (updateClient /*&& prev != this.Locks*/)
                Send.CharacterUnlock(this, locks);

            return Locks;
        }

        /// <summary>
        ///     Returns true if given lock isn't activated.
        /// </summary>
        /// <param name="locks"></param>
        /// <returns></returns>
        public bool Can(Locks locks)
        {
            return (Locks & locks) == 0;
        }

        /// <summary>
        ///     Sends a msg box to creature's client, asking a question.
        ///     The callback is executed if the box is answered with OK.
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
        ///     Calls inquiry callback for id if there is one.
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
        ///     Calculates and returns general production skill success chance.
        /// </summary>
        /// <remarks>
        ///     Unofficial, but seems to work fine in most cases.
        ///     Dex bonus: http://mabination.com/threads/57123-Chaos-Life-Skill-Guide-Refining
        /// </remarks>
        /// <returns></returns>
        public float GetProductionSuccessChance(Skill skill, ProductionCategory category, int baseChance, int rainBonus)
        {
            // Base
            float result = baseChance;
            if (skill.Info.Id != SkillId.PotionMaking && skill.Info.Id != SkillId.Milling)
                result += (Dex - 60) * (baseChance / 300f);

            // Production Mastery bonus
            var pm = Skills.Get(SkillId.ProductionMastery);
            if (pm != null)
                result += (byte) pm.Info.Rank;

            // Weather bonus
            if (ChannelServer.Instance.Weather.GetWeatherType(RegionId) == WeatherType.Rain)
                if (category == ProductionCategory.Weaving)
                    result += rainBonus * 2;
                else
                    result *= 1 + rainBonus / 100f;

            // Party bonus
            result += GetProductionPartyBonus(skill);

            // Monday: Increase in success rate for production skills.
            // +10%, bonus is unofficial.
            if (ErinnTime.Now.Month == ErinnMonth.AlbanEiler)
                result += 5;

            return Math2.Clamp(0, 99, result);
        }

        /// <summary>
        ///     Returns party production bonus for the given skill if creature
        ///     is in a party.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="skill"></param>
        /// <returns></returns>
        public float GetProductionPartyBonus(Skill skill)
        {
            // No bonus when not in party
            if (!IsInParty)
                return 0;

            var result = 0f;

            var members = Party.GetMembers();
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
        ///     Returns the tracker for the creature that did the most hits.
        /// </summary>
        /// <returns></returns>
        public HitTracker GetTopHitter()
        {
            HitTracker result = null;
            var top = 0;

            lock (_hitTrackers)
            {
                foreach (var tracker in _hitTrackers.Values)
                    if (tracker.Hits > top)
                    {
                        result = tracker;
                        top = tracker.Hits;
                    }
            }

            return result;
        }

        /// <summary>
        ///     Returns the tracker for the creature with the given id, or null
        ///     if it doesn't exist.
        /// </summary>
        /// <returns></returns>
        public HitTracker GetHitTracker(long entityId)
        {
            HitTracker result = null;

            lock (_hitTrackers)
            {
                _hitTrackers.TryGetValue(entityId, out result);
            }

            return result;
        }

        /// <summary>
        ///     Returns the tracker for the creature that did the most damage.
        /// </summary>
        /// <returns></returns>
        public HitTracker GetTopDamageDealer()
        {
            HitTracker result = null;
            var top = 0f;

            lock (_hitTrackers)
            {
                foreach (var tracker in _hitTrackers.Values)
                    if (tracker.Damage > top)
                    {
                        result = tracker;
                        top = tracker.Damage;
                    }
            }

            return result;
        }

        /// <summary>
        ///     Returns all creatures that have hit this creature and are still
        ///     in the same region.
        /// </summary>
        /// <returns></returns>
        public List<Creature> GetAllHitters()
        {
            var result = new List<Creature>();

            lock (_hitTrackers)
            {
                foreach (var tracker in _hitTrackers.Values)
                    if (tracker.Attacker.Region == Region)
                        result.Add(tracker.Attacker);
            }

            return result;
        }

        /// <summary>
        ///     Returns the total number of hits the creature took.
        /// </summary>
        /// <returns></returns>
        public int GetTotalHits()
        {
            return _totalHits;
        }

        /// <summary>
        ///     Returns whether creature is allowed to pick up the given item
        ///     from the ground.
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

            // Return whether the item's owner is controlled by the
            // creature's client, this way masters can pick up their
            // follower's (e.g. pet's) items.
            return Client.Creatures.ContainsKey(item.OwnerId);
        }

        /// <summary>
        ///     Calculates the creature's elemental damage modifier, against the
        ///     target's elements.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public float CalculateElementalDamageMultiplier(Creature target)
        {
            return CalculateElementalDamageMultiplier(ElementLightning, ElementFire, ElementIce,
                target.ElementLightning, target.ElementFire, target.ElementIce);
        }

        /// <summary>
        ///     Calculates the elemental damage modifier, based on the given
        ///     affinities, against target.
        /// </summary>
        /// <param name="myLightning"></param>
        /// <param name="myFire"></param>
        /// <param name="myIce"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public float CalculateElementalDamageMultiplier(int myLightning, int myFire, int myIce, Creature target)
        {
            return CalculateElementalDamageMultiplier(myLightning, myFire, myIce, target.ElementLightning,
                target.ElementFire, target.ElementIce);
        }

        /// <summary>
        ///     Calculates the elemental damage modifier, between the "my" and
        ///     the "target" affinities.
        /// </summary>
        /// <remarks>
        ///     Since nobody seems to know the exact way elementals work,
        ///     this function is mostly based on guess.
        ///     First, all elementals are stacked against each other.
        ///     If you have 1 affinity for an element that the enemy has as well,
        ///     you lose 11.1% damage. Afterwards, you gain 11.1% for each very
        ///     effective combination, and 3.7% for each slightly effective
        ///     combination.
        ///     The basic idea is that the same elements cancel each other out,
        ///     while Fire and Ice are very, and Ice and Lightning are slightly
        ///     effective against each other, as is hinted at in the in-game
        ///     book "Understanding Elementals". The book also mentions that
        ///     Fire and Lightning don't affect each other.
        ///     The acual values, 11.1 and 3.7 (11.1 / 3) are based on the max
        ///     affinity number 9, 11.1 * 9 being 99.9, and findings of the
        ///     community, stating the need for at least 3 affinity for a
        ///     noticible effect.
        /// </remarks>
        /// <param name="myLightning"></param>
        /// <param name="myFire"></param>
        /// <param name="mytIce"></param>
        /// <param name="targetLightning"></param>
        /// <param name="targetFire"></param>
        /// <param name="targetIce"></param>
        /// <returns></returns>
        public float CalculateElementalDamageMultiplier(int myLightning, int myFire, int myIce, int targetLightning,
            int targetFire, int targetIce)
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

            return 1f + result / 9f;
        }

        /// <summary>
        ///     Blesses all main equip items and updates client.
        /// </summary>
        public void BlessAll()
        {
            var items = Inventory.GetMainEquipment(a => a.IsBlessable);
            Bless(items);
        }

        /// <summary>
        ///     Blesses given items and updates client.
        /// </summary>
        public void Bless(params Item[] items)
        {
            foreach (var item in items)
                item.OptionInfo.Flags |= ItemFlags.Blessed;

            Send.ItemBlessed(this, items);
        }

        /// <summary>
        ///     Removes the given percentage of the creature's mana
        ///     and updates client.
        /// </summary>
        /// <param name="amount"></param>
        public void BurnMana(float amount = 100)
        {
            var toBurn = Mana * (amount / 100f);

            // Mana preservation stones
            // http://wiki.mabinogiworld.com/view/Category:Mana_Preservation_Stones
            if (!AuraData.FeaturesDb.IsEnabled("ManaBurnRemove"))
            {
                var stones = Inventory.GetItems(a => a.Data.ManaPreservation != 0, StartAt.BottomRight);
                if (stones.Count != 0)
                {
                    var stone = stones[0];
                    var preserve = stone.Data.ManaPreservation;
                    toBurn = Math.Max(0, toBurn - preserve);

                    Inventory.Decrement(stone);
                }
            }

            if (toBurn == 0)
                return;

            Mana -= toBurn;

            Send.StatUpdate(this, StatUpdateType.Private, Stat.Mana);
        }

        /// <summary>
        ///     Returns the chain cast level the creature can use for the
        ///     given skill.
        /// </summary>
        /// <remarks>
        ///     Checks passive monster skill and upgrades of equipped weapons.
        /// </remarks>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public int GetChainCastLevel(SkillId skillId)
        {
            if (Skills.Has(SkillId.ChainCasting))
                return 5;

            if (RightHand == null)
                return 0;

            return Inventory.GetChainCastLevel(skillId);
        }

        /// <summary>
        ///     Returns given Mana cost adjusted for this creature, factoring in
        ///     bonuses and modifications.
        /// </summary>
        /// <param name="baseVal"></param>
        /// <returns></returns>
        public float GetAdjustedManaCost(float baseVal)
        {
            var cost = baseVal;
            var mod = Inventory.GetManaUseModificator();

            // Positive values mean you use less Mana.
            if (mod != 0)
                cost *= (100 - mod) / 100f;

            return cost;
        }

        /// <summary>
        ///     Returns creature's splash radius, based on equipment.
        /// </summary>
        public float GetTotalSplashRadius()
        {
            var result = 0f;

            if (RightHand != null)
            {
                result = RightHand.Data.SplashRadius;

                if (LeftHand != null && !LeftHand.IsShieldLike)
                {
                    result += LeftHand.Data.SplashRadius;
                    result /= 2;
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns creature's splash angle, based on equipment.
        /// </summary>
        public float GetTotalSplashAngle()
        {
            var result = 0f;

            if (RightHand != null)
            {
                result = RightHand.Data.SplashAngle;

                if (LeftHand != null && !LeftHand.IsShieldLike)
                {
                    result += LeftHand.Data.SplashAngle;
                    result /= 2;
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns creature's splash damage, based on equipment.
        /// </summary>
        public float GetTotalSplashDamage()
        {
            var result = 0f;

            if (RightHand != null)
            {
                result = RightHand.Data.SplashDamage;

                if (LeftHand != null && !LeftHand.IsShieldLike)
                {
                    result += LeftHand.Data.SplashDamage;
                    result /= 2;
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns creature's splash radius, based on given weapon.
        /// </summary>
        public float GetSplashRadius(Item item)
        {
            var result = 0f;

            if (item != null)
                result = item.Data.SplashRadius;

            return result;
        }

        /// <summary>
        ///     Returns creature's splash angle, based on given weapon.
        /// </summary>
        public float GetSplashAngle(Item item)
        {
            var result = 0f;

            if (item != null)
                result = item.Data.SplashAngle;

            return result;
        }

        /// <summary>
        ///     Returns creature's splash damage, based on given weapon.
        /// </summary>
        public float GetSplashDamage(Item item)
        {
            var result = 0f;

            if (item != null)
                result = item.Data.SplashDamage;

            return result;
        }

        /// <summary>
        ///     If this creature is an RP character, it returns the player's
        ///     character behind the RP character, if not it just returns itself.
        /// </summary>
        /// <remarks>
        ///     Use in cases where you want to execute an action on the actual
        ///     player character, but you don't know if you're working with an
        ///     RP character or not.
        ///     For example, maybe you want to give a player a quest item at the
        ///     end of the dungeon, but the dungeon can be played as RP or
        ///     non-RP. Using just the creature would give it to the RP
        ///     character, with the player never getting it.
        /// </remarks>
        /// <returns></returns>
        public Creature GetActualCreature()
        {
            if (!IsRpCharacter)
                return this;

            var rpCharacter = this as RpCharacter;
            return rpCharacter.Actor;
        }

        /// <summary>
        ///     Checks if player has the skill.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public bool HasSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
        {
            return Skills.Has(skillId, rank);
        }

        /// <summary>
        ///     Checks if player has the skill on the specified rank.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public bool IsSkill(SkillId skillId, SkillRank rank)
        {
            return Skills.Is(skillId, rank);
        }

        /// <summary>
        ///     Gives skill to player if he doesn't have it on that rank yet.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="rank"></param>
        public void GiveSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
        {
            if (HasSkill(skillId, rank))
                return;

            Skills.Give(skillId, rank);
        }

        /// <summary>
        ///     Trains the specified condition for skill by one.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="condition"></param>
        public void TrainSkill(SkillId skillId, int condition)
        {
            var skill = Skills.Get(skillId);
            if (skill == null)
                return;

            skill.Train(condition);
        }

        /// <summary>
        ///     Returns true if quest is in progress and not all objectives
        ///     have been finished yet.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="objective"></param>
        /// <returns></returns>
        public bool QuestActive(int questId, string objective = null)
        {
            return Quests.IsActive(questId, objective);
        }

        /// <summary>
        ///     Returns true if player has quest, completed or not.
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        public bool HasQuest(int questId)
        {
            return Quests.Has(questId);
        }

        /// <summary>
        ///     Returns true if quest was completed.
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        public bool QuestCompleted(int questId)
        {
            return Quests.IsComplete(questId);
        }

        /// <summary>
        ///     Returns true if player has quest, but it wasn't finished or
        ///     completed yet.
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        public bool QuestInProgress(int questId)
        {
            return HasQuest(questId) && !QuestCompleted(questId);
        }

        /// <summary>
        ///     Finishes objective in quest.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="objective"></param>
        /// <returns></returns>
        public bool FinishQuestObjective(int questId, string objective)
        {
            return Quests.Finish(questId, objective);
        }

        /// <summary>
        ///     Returns current quest objective.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="objective"></param>
        /// <returns></returns>
        public string GetCurrentQuestObjective(int questId)
        {
            var quest = Quests.GetFirstIncomplete(questId);
            if (quest == null)
                throw new Exception("Player doesn't have quest '" + questId + "'.");

            var current = quest.CurrentObjective;
            if (current == null)
                return null;

            return current.Ident;
        }

        /// <summary>
        ///     Starts quest.
        /// </summary>
        /// <param name="questId"></param>
        public void StartQuest(int questId)
        {
            try
            {
                Quests.Start(questId);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Creature.StartQuest: Quest '{0}'", questId);
            }
        }

        /// <summary>
        ///     Sends quest to player via owl.
        /// </summary>
        /// <param name="questId"></param>
        public void SendOwl(int questId)
        {
            SendOwl(questId, 0);
        }

        /// <summary>
        ///     Sends quest to player via owl after the delay.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="delay">Arrival delay in seconds.</param>
        /// <returns></returns>
        public bool SendOwl(int questId, int delay)
        {
            try
            {
                Quests.SendOwl(questId, delay);
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Creature.SendOwl: Quest '{0}'", questId);
                return false;
            }
        }

        /// <summary>
        ///     Completes quest (incl rewards).
        /// </summary>
        /// <param name="questId"></param>
        public void CompleteQuest(int questId)
        {
            Quests.Complete(questId, false);
        }

        /// <summary>
        ///     Starts PTJ quest.
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        public bool StartPtj(int questId, string npcName)
        {
            try
            {
                var scroll = Item.CreateQuestScroll(questId);
                var quest = scroll.Quest;

                quest.MetaData.SetByte("QMRTCT", (byte) quest.Data.RewardGroups.Count);
                quest.MetaData.SetInt("QMRTBF",
                    0x4321); // (specifies which groups to display at which position, 1 group per hex char)
                quest.MetaData.SetString("QRQSTR", npcName);
                quest.MetaData.SetBool("QMMABF", false);

                // Calculate deadline, based on current time and quest data
                var now = ErinnTime.Now;
                var diffHours = Math.Max(0, quest.Data.DeadlineHour - now.Hour - 1);
                var diffMins = Math.Max(0, 60 - now.Minute);
                var deadline =
                    DateTime.Now.AddTicks(diffHours * ErinnTime.TicksPerHour + diffMins * ErinnTime.TicksPerMinute);
                quest.Deadline = deadline;

                // Do quests given out by NPCs *always* go into the
                // quest pocket?
                Inventory.Add(scroll, Pocket.Quests);

                ChannelServer.Instance.Events.OnCreatureStartedPtj(this, quest.Data.PtjType);

                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Creature.StartPtj: Quest '{0}'", questId);
                return false;
            }
        }

        /// <summary>
        ///     Completes PTJ quest, if one is active. Rewards the selected rewards.
        /// </summary>
        /// <param name="rewardReply">Example: @reward:0</param>
        public void CompletePtj(string rewardReply)
        {
            var quest = Quests.GetPtjQuest();
            if (quest == null)
                return;

            // Get reward group index
            var rewardGroupIdx = 0;
            if (!int.TryParse(rewardReply.Substring("@reward:".Length), out rewardGroupIdx))
            {
                Log.Warning("Creature.CompletePtj: Invalid reply '{0}'.", rewardReply);
                return;
            }

            // Get reward group id
            // The client displays a list of all available rewards,
            // ordered by group id, with unobtainable ones disabled.
            // What it sends is the index of the element in that list,
            // not the actual group id, because that would be too easy.
            var rewardGroup = -1;
            var group = quest.Data.RewardGroups.Values.OrderBy(a => a.Id).ElementAt(rewardGroupIdx);
            if (group == null)
                Log.Warning("Creature.CompletePtj: Invalid group index '{0}' for quest '{1}'.", rewardGroupIdx,
                    quest.Id);
            else if (!group.HasRewardsFor(quest.GetResult()))
                throw new Exception("Invalid reward group, doesn't have rewards for result.");
            else
                rewardGroup = group.Id;

            // Complete
            Quests.Complete(quest, rewardGroup, false);

            ChannelServer.Instance.Events.OnCreatureCompletedPtj(this, quest.Data.PtjType);
        }

        /// <summary>
        ///     Gives up Ptj (fail without rewards).
        /// </summary>
        public void GiveUpPtj()
        {
            var quest = Quests.GetPtjQuest();
            if (quest == null)
                return;

            Quests.GiveUp(quest);
        }

        /// <summary>
        ///     Returns true if a PTJ quest is active and its type matches
        ///     the given one.
        /// </summary>
        /// <returns></returns>
        public bool IsDoingPtj(PtjType type)
        {
            var quest = Quests.GetPtjQuest();
            return quest != null && quest.Data.PtjType == type;
        }

        /// <summary>
        ///     Returns true if a PTJ quest is active.
        /// </summary>
        /// <returns></returns>
        public bool IsDoingPtj()
        {
            var quest = Quests.GetPtjQuest();
            return quest != null;
        }

        /// <summary>
        ///     Returns true if a PTJ quest from the given NPC is in progress.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public bool IsDoingPtjFor(Creature npc)
        {
            return IsDoingPtjFor(npc.Name);
        }

        /// <summary>
        ///     Returns true if a PTJ quest from the given NPC is in progress.
        /// </summary>
        /// <param name="npcName"></param>
        /// <returns></returns>
        public bool IsDoingPtjFor(string npcName)
        {
            var quest = Quests.GetPtjQuest();
            return quest != null && quest.MetaData.GetString("QRQSTR") == npcName;
        }

        /// <summary>
        ///     Returns true if a PTJ quest from an NPC other than the given one
        ///     is in progress.
        /// </summary>
        /// <param name="npcName"></param>
        /// <returns></returns>
        public bool IsDoingPtjNotFor(Creature npc)
        {
            return IsDoingPtjNotFor(npc.Name);
        }

        /// <summary>
        ///     Returns true if a PTJ quest from an NPC other than the given one
        ///     is in progress.
        /// </summary>
        /// <param name="npcName"></param>
        /// <returns></returns>
        public bool IsDoingPtjNotFor(string npcName)
        {
            var quest = Quests.GetPtjQuest();
            return quest != null && quest.MetaData.GetString("QRQSTR") != npcName;
        }

        /// <summary>
        ///     Returns true if the player can do a PTJ of type, because he hasn't
        ///     done one of the same type today.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="remaining"></param>
        /// <returns></returns>
        public bool CanDoPtj(PtjType type, int remaining = 99)
        {
            // Always allow devCATs
            //if (this.Title == TitleId.devCAT)
            //	return true;

            // Check remaining
            if (remaining <= 0)
                return false;

            // Check if PTJ has already been done this Erinn day
            var ptj = Quests.GetPtjTrackRecord(type);
            var change = new ErinnTime(ptj.LastChange);
            var now = ErinnTime.Now;

            return now.Day != change.Day || now.Month != change.Month || now.Year != change.Year;
        }

        /// <summary>
        ///     Returns the player's level (basic, int, adv) for the given PTJ type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public QuestLevel GetPtjQuestLevel(PtjType type)
        {
            var record = Quests.GetPtjTrackRecord(type);
            return record.GetQuestLevel();
        }

        /// <summary>
        ///     Returns number of times the player has done the given PTJ type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetPtjDoneCount(PtjType type)
        {
            return Quests.GetPtjTrackRecord(type).Done;
        }

        /// <summary>
        ///     Returns number of times the player has successfully done the given PTJ type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetPtjSuccessCount(PtjType type)
        {
            return Quests.GetPtjTrackRecord(type).Success;
        }

        /// <summary>
        ///     Returns how well the current PTJ has been done (so far).
        /// </summary>
        /// <returns></returns>
        public QuestResult GetPtjResult()
        {
            var quest = Quests.GetPtjQuest();
            if (quest != null)
                return quest.GetResult();

            return QuestResult.None;
        }

        /// <summary>
        ///     Displays notice.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Notice(string format, params object[] args)
        {
            Send.Notice(this, format, args);
        }

        /// <summary>
        ///     Displays notice.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Notice(NoticeType type, string format, params object[] args)
        {
            Send.Notice(this, type, format, args);
        }

        /// <summary>
        ///     Displays as notice and system message.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void SystemNotice(string format, params object[] args)
        {
            Notice(format, args);
            SystemMsg(format, args);
        }

        /// <summary>
        ///     Displays system message in player's chat log.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void SystemMsg(string format, params object[] args)
        {
            Send.SystemMessage(this, format, args);
        }

        /// <summary>
        ///     Returns true if player has the keyword.
        /// </summary>
        /// <param name="keyword"></param>
        public bool HasKeyword(string keyword)
        {
            return Keywords.Has(keyword);
        }

        /// <summary>
        ///     Returns true if player has the keyword.
        /// </summary>
        /// <param name="keyword"></param>
        public void GiveKeyword(string keyword)
        {
            if (HasKeyword(keyword))
                return;

            Keywords.Give(keyword);
        }

        /// <summary>
        ///     Returns true if player has the keyword.
        /// </summary>
        /// <param name="keyword"></param>
        public void RemoveKeyword(string keyword)
        {
            if (HasKeyword(keyword))
                Keywords.Remove(keyword);
        }

        /// <summary>
        ///     Adds points (Pon) to creature's account.
        /// </summary>
        /// <param name="amount"></param>
        public void GivePoints(int amount)
        {
            Points += amount;
        }

        /// <summary>
        ///     Removes points (Pon) from creature's account.
        /// </summary>
        /// <param name="amount"></param>
        public void RemovePoints(int amount)
        {
            Points -= amount;
        }

        /// <summary>
        ///     Returns true if creature's account has at least the given amount
        ///     of points (Pon).
        /// </summary>
        /// <param name="amount"></param>
        public bool HasPoints(int amount)
        {
            return Points >= amount;
        }

        /// <summary>
        ///     Returns true if creature knows about the title, even if it
        ///     doesn't have it.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        public bool KnowsTitle(int titleId)
        {
            return Titles.Knows((ushort) titleId);
        }

        /// <summary>
        ///     Returns true if creature has and is able to use the given title.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        public bool CanUseTitle(int titleId)
        {
            return Titles.IsUsable((ushort) titleId);
        }

        /// <summary>
        ///     Let's creature know about given title, but doesn't enable it.
        /// </summary>
        /// <param name="titleId"></param>
        public void ShowTitle(int titleId)
        {
            Titles.Show((ushort) titleId);
        }

        /// <summary>
        ///     Enables creature to use given title.
        /// </summary>
        /// <param name="titleId"></param>
        public void EnableTitle(int titleId)
        {
            Titles.Enable((ushort) titleId);
        }

        /// <summary>
        ///     Returns true if creature is using the given title as either main
        ///     or option title.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        public bool IsUsingTitle(int titleId)
        {
            return Titles.SelectedTitle == titleId || Titles.SelectedOptionTitle == titleId;
        }

        /// <summary>
        ///     Returns true if the creature has equipped an item with the given
        ///     id in one of its main equip slots.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool HasEquipped(int itemId)
        {
            var items = Inventory.GetMainEquipment(a => a.Info.Id == itemId);
            return items.Any();
        }

        /// <summary>
        ///     Returns true if the creature has equipped an item that matches
        ///     the given tag in one of its main equip slots.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool HasEquipped(string tag)
        {
            var items = Inventory.GetMainEquipment(a => a.HasTag(tag));
            return items.Any();
        }

        /// <summary>
        ///     Plays sound for creature.
        /// </summary>
        /// <param name="fileName"></param>
        public void PlaySound(string fileName)
        {
            Send.PlaySound(this, fileName);
        }

        /// <summary>
        ///     Swaps the given equip sets' slots.
        /// </summary>
        /// <param name="set1"></param>
        /// <param name="set2"></param>
        public void SwapEquipmentSets(EquipmentSet set1, EquipmentSet set2, ExtraSlots slots)
        {
            if (set1 == set2)
                throw new ArgumentException("The given sets can't be the same.");

            var adds = new Dictionary<Item, Pocket>();

            foreach (var slot in _swapSlots)
            {
                // Check if slot is among the ones to swap
                if ((slots & slot) == 0)
                    continue;

                var pocket1 = TranslateSlotToPocket(slot, set1);
                var pocket2 = TranslateSlotToPocket(slot, set2);

                // If only one of the pockets is None, something went wrong.
                if (pocket1 == Pocket.None || pocket2 == Pocket.None)
                    throw new InvalidOperationException("Pocket1 or pocket2 is none.");

                // Get items
                var item1 = Inventory.GetItemAt(pocket1, 0, 0);
                var item2 = Inventory.GetItemAt(pocket2, 0, 0);

                // Correct pockets/items for magazines
                if (item1 != null && item1.IsMagazine && pocket2 == Inventory.LeftHandPocket)
                {
                    pocket2 = Inventory.MagazinePocket;
                    if (item2 != null && item2.IsMagazine)
                        item2 = Inventory.GetItemAt(pocket2, 0, 0);
                }
                else if (item1 == null && pocket1 == Inventory.LeftHandPocket)
                {
                    var magazine = Inventory.GetItemAt(Inventory.MagazinePocket, 0, 0);
                    if (magazine != null)
                    {
                        item1 = magazine;
                        if (item2 == null || item2.IsMagazine)
                            pocket1 = Inventory.MagazinePocket;
                    }
                }

                if (item2 != null && item2.IsMagazine && pocket1 == Inventory.LeftHandPocket)
                {
                    pocket1 = Inventory.MagazinePocket;
                    if (item1 != null && item1.IsMagazine)
                        item1 = Inventory.GetItemAt(pocket1, 0, 0);
                }
                else if (item2 == null && pocket2 == Inventory.LeftHandPocket)
                {
                    var magazine = Inventory.GetItemAt(Inventory.MagazinePocket, 0, 0);
                    if (magazine != null)
                    {
                        item2 = magazine;
                        if (item1 == null || item1.IsMagazine)
                            pocket2 = Inventory.MagazinePocket;
                    }
                }

                // Remove items
                if (item1 != null) Inventory.Remove(item1);
                if (item2 != null) Inventory.Remove(item2);

                // Add items
                if (item1 != null) adds.Add(item1, pocket2);
                if (item2 != null) adds.Add(item2, pocket1);
            }

            // Add items after all were remove, since the auto-equip code
            // might otherwise interfere with the adding and removing
            // of the weapons.
            foreach (var add in adds)
                Inventory.Add(add.Key, add.Value);
        }

        /// <summary>
        ///     Returns the Pocket for the given slot and set combination.
        ///     Magazines are not handled by this function.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        /// <example>
        ///     TranslateSlotToPocket(ExtraSlots.Glove, ExtraSet.Original) // Pocket.Glove
        ///     TranslateSlotToPocket(ExtraSlots.Shoe, ExtraSet.Set2) // Pocket.ShoeExtra2
        /// </example>
        private Pocket TranslateSlotToPocket(ExtraSlots slot, EquipmentSet set)
        {
            if (set == EquipmentSet.Original)
            {
                switch (slot)
                {
                    case ExtraSlots.Armor: return Pocket.Armor;
                    case ExtraSlots.Glove: return Pocket.Glove;
                    case ExtraSlots.Shoe: return Pocket.Shoe;
                    case ExtraSlots.Head: return Pocket.Head;
                    case ExtraSlots.Robe: return Pocket.Robe;
                    case ExtraSlots.RightHand: return Inventory.RightHandPocket;
                    case ExtraSlots.LeftHand: return Inventory.LeftHandPocket;
                    case ExtraSlots.Accessory1: return Pocket.Accessory1;
                    case ExtraSlots.Accessory2: return Pocket.Accessory2;
                }

                return Pocket.None;
            }
            var result = Pocket.None;

            switch (slot)
            {
                case ExtraSlots.Armor:
                    result = Pocket.ArmorExtra1;
                    break;
                case ExtraSlots.Glove:
                    result = Pocket.GloveExtra1;
                    break;
                case ExtraSlots.Shoe:
                    result = Pocket.ShoeExtra1;
                    break;
                case ExtraSlots.Head:
                    result = Pocket.HeadExtra1;
                    break;
                case ExtraSlots.Robe:
                    result = Pocket.RobeExtra1;
                    break;
                case ExtraSlots.RightHand:
                    result = Pocket.RightHandExtra1;
                    break;
                case ExtraSlots.LeftHand:
                    result = Pocket.LeftHandExtra1;
                    break;
                case ExtraSlots.Accessory1:
                    result = Pocket.Accessory1HandExtra1;
                    break;
                case ExtraSlots.Accessory2:
                    result = Pocket.Accessory2HandExtra1;
                    break;
            }

            return result + 9 * (int) set;
        }

        /// <summary>
        ///     Returns true if creature can currently use the given pocket.
        /// </summary>
        /// <param name="pocket"></param>
        /// <returns></returns>
        public bool CanUseExtraEquipmentPocket(Pocket pocket)
        {
            if (!pocket.IsExtraSlot())
                throw new ArgumentException("Pocket is not an extra slot pocket.");

            var pocketId = (int) pocket;
            var set = Math.Floor((pocketId - 2000) / 9.0);

            return ExtraEquipmentSetsCount > set;
        }

        /// <summary>
        ///     Extends time in which the extra sets may be used by the given
        ///     time span.
        /// </summary>
        /// <param name="timeSpan"></param>
        public void ExtendExtraEquipmentSetsTime(TimeSpan timeSpan)
        {
            if (ExtraEquipmentSetsEnd == DateTime.MinValue)
                ExtraEquipmentSetsEnd = DateTime.Now.Add(timeSpan);
            else
                ExtraEquipmentSetsEnd += timeSpan;

            Send.UpdateExtraEquipmentEnd(this);
        }

        /// <summary>
        ///     Adds an extra equipment set, returns true on success or false
        ///     if the max number of sets was reached already.
        /// </summary>
        /// <returns></returns>
        public bool AddExtraEquipmentSet()
        {
            // The client only supports 3 hotkeys for the extra slots,
            // but it supports having way more. We'll set a hard limit of
            // 16 for now, the absolut max our current implementation could
            // support are 111, but who knows if devCAT won't add more
            // stuff in between.
            var max = Math.Min(16, ChannelServer.Instance.Conf.World.MaxExtraSets);

            if (ExtraEquipmentSetsCount >= max)
                return false;

            // The linked pocket refers to the first pocket in a nine pocket
            // range that this kit's equip set uses. Technically it might be
            // possible to use any number here, but we'll still to the officials
            // convention for now, where they start at 2000 for the first set,
            // 2009 for the second, and so on.
            // 3000 (ExtraEquipSlotKits) is the pocket for the kits though,
            // so we should be careful not to add too many sets.

            var item = new Item(PrefabEquipmentSwapKit);
            item.OptionInfo.LinkedPocketId = Pocket.ArmorExtra1 + 9 * ExtraEquipmentSetsCount;

            Inventory.Add(item, Pocket.ExtraEquipSlotKits);

            return true;
        }
    }

    public enum TargetableOptions
    {
        None,

        /// <summary>
        ///     Adds attack range of creature to the given range.
        /// </summary>
        AddAttackRange,

        /// <summary>
        ///     Ignores collision lines between creature and potential targets.
        /// </summary>
        IgnoreWalls
    }
}