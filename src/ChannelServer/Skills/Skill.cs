// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.GameEvents;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using Aura.Shared.Util;

namespace Aura.Channel.Skills
{
    public class Skill
    {
        private readonly Creature _creature;
        private bool _enabled;
        private readonly int _race;

        private int _stack;

        /// <summary>
        ///     Information about the skill, serialized to packets.
        /// </summary>
        public SkillInfo Info;

        /// <summary>
        ///     New Skill.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="id"></param>
        /// <param name="rank"></param>
        /// <param name="race"></param>
        public Skill(Creature creature, SkillId id, SkillRank rank, int race)
        {
            _creature = creature;
            _race = race;

            Info.Id = id;
            Info.Rank = rank;
            Info.MaxRank = rank;

            Info.Flag = SkillFlags.Shown;

            // The conditions are set to the max and are reduced afterwards,
            // making them "Complete" once they reach 0. Initializing to 1
            // in case of problems.
            Info.ConditionCount1 = 1;
            Info.ConditionCount2 = 1;
            Info.ConditionCount3 = 1;
            Info.ConditionCount4 = 1;
            Info.ConditionCount5 = 1;
            Info.ConditionCount6 = 1;
            Info.ConditionCount7 = 1;
            Info.ConditionCount8 = 1;
            Info.ConditionCount9 = 1;

            LoadRankData();
        }

        /// <summary>
        ///     Data about the skill, loaded from the db.
        /// </summary>
        public SkillData Data { get; protected set; }

        /// <summary>
        ///     Data about the skill's current rank, loaded from the db.
        /// </summary>
        public SkillRankData RankData { get; protected set; }

        /// <summary>
        ///     The skills current state.
        /// </summary>
        public SkillState State { get; set; }

        /// <summary>
        ///     Holds time at which the skill is fully loaded.
        /// </summary>
        public DateTime CastEnd { get; set; }

        /// <summary>
        ///     Returns true if skill is currently on cool down.
        /// </summary>
        public bool IsCoolingDown => _creature.CoolDowns.IsCoolingDown("SkillUse:" + Info.Id);

        /// <summary>
        ///     Gets or sets loaded stack count, capped at 0~max.
        ///     Updates client automatically.
        /// </summary>
        public int Stacks
        {
            get => _stack;
            set
            {
                _stack = Math2.Clamp(0, sbyte.MaxValue, value);
                Send.SkillStackSet(_creature, Info.Id, _stack);
            }
        }

        /// <summary>
        ///     Returns true if skill has enough experience and is below max rank.
        /// </summary>
        public bool IsRankable => Info.Experience >= 100000 && Info.Rank < Info.MaxRank;

        /// <summary>
        ///     Returns true if all training conditions were cleared.
        /// </summary>
        public bool IsFullyTrained => Info.ConditionCount1 + Info.ConditionCount2 + Info.ConditionCount3 +
                                      Info.ConditionCount4 + Info.ConditionCount5 + Info.ConditionCount6 +
                                      Info.ConditionCount7 + Info.ConditionCount8 + Info.ConditionCount9 == 0;

        /// <summary>
        ///     Enables/Disables skill, changing whether it can/will be used,
        ///     and updates the client.  Visualized on the client side by
        ///     changing the icon's transparency.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                Send.SetSkillEnabled(_creature, Info.Id, _enabled);
            }
        }

        /// <summary>
        ///     Loads rank data, based on current rank.
        /// </summary>
        public void LoadRankData()
        {
            Data = AuraData.SkillDb.Find((int) Info.Id);
            if (Data == null)
                throw new Exception("Skill.LoadRankData: Skill data not found for '" + Info.Id + "'.");

            if ((RankData = Data.GetRankData(Info.Rank, _race)) == null)
                throw new Exception("Skill.LoadRankData: No rank data found for '" + Info.Id + "@" + Info.Rank + "'.");

            Info.MaxRank = Data.MaxRank;

            Info.ConditionCount1 = (short) RankData.Conditions[0].Count;
            Info.ConditionCount2 = (short) RankData.Conditions[1].Count;
            Info.ConditionCount3 = (short) RankData.Conditions[2].Count;
            Info.ConditionCount4 = (short) RankData.Conditions[3].Count;
            Info.ConditionCount5 = (short) RankData.Conditions[4].Count;
            Info.ConditionCount6 = (short) RankData.Conditions[5].Count;
            Info.ConditionCount7 = (short) RankData.Conditions[6].Count;
            Info.ConditionCount8 = (short) RankData.Conditions[7].Count;
            Info.ConditionCount9 = (short) RankData.Conditions[8].Count;

            if (RankData.Conditions[0].Visible) Info.Flag |= SkillFlags.ShowCondition1;
            if (RankData.Conditions[1].Visible) Info.Flag |= SkillFlags.ShowCondition2;
            if (RankData.Conditions[2].Visible) Info.Flag |= SkillFlags.ShowCondition3;
            if (RankData.Conditions[3].Visible) Info.Flag |= SkillFlags.ShowCondition4;
            if (RankData.Conditions[4].Visible) Info.Flag |= SkillFlags.ShowCondition5;
            if (RankData.Conditions[5].Visible) Info.Flag |= SkillFlags.ShowCondition6;
            if (RankData.Conditions[6].Visible) Info.Flag |= SkillFlags.ShowCondition7;
            if (RankData.Conditions[7].Visible) Info.Flag |= SkillFlags.ShowCondition8;
            if (RankData.Conditions[8].Visible) Info.Flag |= SkillFlags.ShowCondition9;
        }

        /// <summary>
        ///     Changes rank, resets experience, loads rank data.
        /// </summary>
        /// <param name="rank"></param>
        public void ChangeRank(SkillRank rank)
        {
            Info.Rank = rank;
            Info.Experience = 0;
            Info.Flag &= ~SkillFlags.Rankable;
            LoadRankData();

            ChannelServer.Instance.Events.OnSkillRankChanged(_creature, this);
        }

        /// <summary>
        ///     Increases training condition count.
        /// </summary>
        /// <param name="condition">Condition to train (1~9).</param>
        /// <param name="amount">Amount of EXP to give.</param>
        /// <param name="applyBonuses">
        ///     Whether to apply bonuses to the EXP, from events or rate settings.
        /// </param>
        public void Train(int condition, int amount = 1, bool applyBonuses = true)
        {
            // Only characters can train skills.
            if (_creature.IsPet)
                return;

            var bonusMessage = "";

            // Add bonuses
            if (applyBonuses)
                HandleSkillExpRateBonuses(ref amount, ref bonusMessage);

            // Change count and reveal the condition
            switch (condition)
            {
                case 1:
                    if (Info.ConditionCount1 == 0) return;
                    Info.ConditionCount1 = (short) Math.Max(0, Info.ConditionCount1 - amount);
                    Info.Flag |= SkillFlags.ShowCondition1;
                    break;
                case 2:
                    if (Info.ConditionCount2 == 0) return;
                    Info.ConditionCount2 = (short) Math.Max(0, Info.ConditionCount2 - amount);
                    Info.Flag |= SkillFlags.ShowCondition2;
                    break;
                case 3:
                    if (Info.ConditionCount3 == 0) return;
                    Info.ConditionCount3 = (short) Math.Max(0, Info.ConditionCount3 - amount);
                    Info.Flag |= SkillFlags.ShowCondition3;
                    break;
                case 4:
                    if (Info.ConditionCount4 == 0) return;
                    Info.ConditionCount4 = (short) Math.Max(0, Info.ConditionCount4 - amount);
                    Info.Flag |= SkillFlags.ShowCondition4;
                    break;
                case 5:
                    if (Info.ConditionCount5 == 0) return;
                    Info.ConditionCount5 = (short) Math.Max(0, Info.ConditionCount5 - amount);
                    Info.Flag |= SkillFlags.ShowCondition5;
                    break;
                case 6:
                    if (Info.ConditionCount6 == 0) return;
                    Info.ConditionCount6 = (short) Math.Max(0, Info.ConditionCount6 - amount);
                    Info.Flag |= SkillFlags.ShowCondition6;
                    break;
                case 7:
                    if (Info.ConditionCount7 == 0) return;
                    Info.ConditionCount7 = (short) Math.Max(0, Info.ConditionCount7 - amount);
                    Info.Flag |= SkillFlags.ShowCondition7;
                    break;
                case 8:
                    if (Info.ConditionCount8 == 0) return;
                    Info.ConditionCount8 = (short) Math.Max(0, Info.ConditionCount8 - amount);
                    Info.Flag |= SkillFlags.ShowCondition8;
                    break;
                case 9:
                    if (Info.ConditionCount9 == 0) return;
                    Info.ConditionCount9 = (short) Math.Max(0, Info.ConditionCount9 - amount);
                    Info.Flag |= SkillFlags.ShowCondition9;
                    break;
                default:
                    Log.Error("Skill.Train: Unknown training condition ({0})", condition);
                    break;
            }

            var exp = UpdateExperience();
            if (exp != 0)
                Send.SkillTrainingUp(_creature, this, exp, bonusMessage);

            CheckMaster();
        }

        /// <summary>
        ///     Modifies amount and bonus message based on active skill exp
        ///     rate bonuses, like rate settings and events.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="bonusMessage"></param>
        private void HandleSkillExpRateBonuses(ref int amount, ref string bonusMessage)
        {
            // Add global bonus
            float bonusMultiplier;
            string bonuses;
            if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(GlobalBonusStat.SkillTraining,
                out bonusMultiplier, out bonuses))
                ApplySkillExpRateBonus(ref amount, bonusMultiplier, bonuses, ref bonusMessage);

            // Apply skill exp multiplier
            var skillExpRate = ChannelServer.Instance.Conf.World.SkillExpRate;
            ApplySkillExpRateBonus(ref amount, skillExpRate, Localization.Get("Skill Exp Rate"), ref bonusMessage);

            // Apply separate skill exp multipliers
            // The code be cleaner if we combined skill exp rates into one
            // amount, but we might want separate bonus names.
            var worldConf = ChannelServer.Instance.Conf.World;
            switch (Data.Category)
            {
                case SkillCategory.Life:
                    ApplySkillExpRateBonus(ref amount, worldConf.LifeSkillExpRate,
                        Localization.Get("Life Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Combat:
                    ApplySkillExpRateBonus(ref amount, worldConf.CombatSkillExpRate,
                        Localization.Get("Combat Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Magic:
                    ApplySkillExpRateBonus(ref amount, worldConf.MagicSkillExpRate,
                        Localization.Get("Magic Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Alchemy:
                    ApplySkillExpRateBonus(ref amount, worldConf.AlchemySkillExpRate,
                        Localization.Get("Alchemy Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Fighter:
                    ApplySkillExpRateBonus(ref amount, worldConf.FighterSkillExpRate,
                        Localization.Get("Fighter Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Music:
                    ApplySkillExpRateBonus(ref amount, worldConf.MusicSkillExpRate,
                        Localization.Get("Music Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Puppet:
                    ApplySkillExpRateBonus(ref amount, worldConf.PuppetSkillExpRate,
                        Localization.Get("Puppetry Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Guns:
                    ApplySkillExpRateBonus(ref amount, worldConf.GunsSkillExpRate,
                        Localization.Get("Dual Gun Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Ninja:
                    ApplySkillExpRateBonus(ref amount, worldConf.NinjaSkillExpRate,
                        Localization.Get("Ninja Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Transformation:
                    ApplySkillExpRateBonus(ref amount, worldConf.TransformationSkillExpRate,
                        Localization.Get("Transformations Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.Demi:
                    ApplySkillExpRateBonus(ref amount, worldConf.DemiSkillExpRate,
                        Localization.Get("Demigod Skill Exp Rate"), ref bonusMessage);
                    break;
                case SkillCategory.DivineKnights:
                    ApplySkillExpRateBonus(ref amount, worldConf.DivineKnightsSkillExpRate,
                        Localization.Get("Crusader Skill Exp Rate"), ref bonusMessage);
                    break;
            }
        }

        /// <summary>
        ///     Multiplies amount with multiplier if multiplier doesn't equal 1
        ///     and appends bonus name to message if it's not empty.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="multiplier"></param>
        /// <param name="bonusName"></param>
        /// <param name="bonusMessage"></param>
        private static void ApplySkillExpRateBonus(ref int amount, float multiplier, string bonusName,
            ref string bonusMessage)
        {
            if (multiplier == 1)
                return;

            amount = (int) (amount * multiplier);
            if (!string.IsNullOrWhiteSpace(bonusName))
                bonusMessage += string.Format(Localization.Get(" ({0} Bonus: x{1})"), bonusName, multiplier);
        }

        /// <summary>
        ///     Enables master title if skill is on r1 and fully trained.
        /// </summary>
        private void CheckMaster()
        {
            // Skip if not R1 or already has the master title.
            if (Info.Rank != SkillRank.R1 || _creature.Titles.IsUsable(Data.MasterTitle))
                return;

            // Give master title if all conditions were met. (met == 0)
            if (IsFullyTrained)
                _creature.Titles.Enable(Data.MasterTitle);
        }

        /// <summary>
        ///     Updates exp and returns gained amount.
        /// </summary>
        /// <returns></returns>
        public float UpdateExperience()
        {
            var result = Info.Experience / 1000f;
            var exp = 0f;
            exp += (RankData.Conditions[0].Count - Info.ConditionCount1) * RankData.Conditions[0].Exp;
            exp += (RankData.Conditions[1].Count - Info.ConditionCount2) * RankData.Conditions[1].Exp;
            exp += (RankData.Conditions[2].Count - Info.ConditionCount3) * RankData.Conditions[2].Exp;
            exp += (RankData.Conditions[3].Count - Info.ConditionCount4) * RankData.Conditions[3].Exp;
            exp += (RankData.Conditions[4].Count - Info.ConditionCount5) * RankData.Conditions[4].Exp;
            exp += (RankData.Conditions[5].Count - Info.ConditionCount6) * RankData.Conditions[5].Exp;
            exp += (RankData.Conditions[6].Count - Info.ConditionCount7) * RankData.Conditions[6].Exp;
            exp += (RankData.Conditions[7].Count - Info.ConditionCount8) * RankData.Conditions[7].Exp;
            exp += (RankData.Conditions[8].Count - Info.ConditionCount9) * RankData.Conditions[8].Exp;
            Info.Experience = (int) (exp * 1000);

            if (IsRankable)
                Info.Flag |= SkillFlags.Rankable;

            return exp - result;
        }

        /// <summary>
        ///     Activates given flag(s).
        /// </summary>
        /// <param name="flags"></param>
        public void Activate(SkillFlags flags)
        {
            Info.Flag |= flags;
        }

        /// <summary>
        ///     Deativates given flag(s).
        /// </summary>
        /// <param name="flags"></param>
        public void Deactivate(SkillFlags flags)
        {
            Info.Flag &= ~flags;
        }

        /// <summary>
        ///     Returns true if skill has the given flags.
        /// </summary>
        /// <param name="flags"></param>
        public bool Has(SkillFlags flags)
        {
            return (Info.Flag & flags) != 0;
        }

        /// <summary>
        ///     Returns cast time of skill, specific for its creature.
        /// </summary>
        /// <returns></returns>
        public int GetCastTime()
        {
            var result = 0;

            // Characters/Dynamic
            if (_creature.IsCharacter && AuraData.FeaturesDb.IsEnabled("CombatSystemRenewal"))
                result = RankData.NewLoadTime;
            // Monsters/Pets
            else
                result = RankData.LoadTime;

            // CastingSpeed upgrade
            var rh = _creature.RightHand;
            if (rh != null)
            {
                // Check if there is a casting mod on the weapon
                var mod = _creature.Inventory.GetCastingSpeedMod(rh.EntityId);
                if (mod != 0)
                {
                    // Check if the skill <> weapon combination is a valid
                    // candidate for the casting speed upgrade.
                    var valid =
                        Is(SkillId.Firebolt, SkillId.Fireball) && rh.HasTag("/fire_wand/") ||
                        Is(SkillId.Lightningbolt, SkillId.Thunder) && rh.HasTag("/lightning_wand/") ||
                        Is(SkillId.Icebolt, SkillId.IceSpear) && rh.HasTag("/ice_wand/");

                    // Modify if valid
                    if (valid)
                        result = (int) (result * Math.Max(0, 1f - mod / 100f));
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns true if the skill has one of the given ids.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public bool Is(params SkillId[] skillId)
        {
            return skillId.Contains(Info.Id);
        }

        /// <summary>
        ///     Returns exp that the creature would get for a rank up of this
        ///     skill in its current state.
        /// </summary>
        /// <remarks>
        ///     The formula is entirely custom and is based on a very small
        ///     amout of test values, which it doesn't match 100% either.
        ///     However, the results seem reasonable, they appear to be close
        ///     to officials, and going by the lack of research, nobody ever
        ///     bothered to take a closer look at this feature anyway.
        /// </remarks>
        /// <returns></returns>
        public int GetExpBonus()
        {
            var result = 0f;
            var month = ErinnTime.Now.Month;

            // Use current training experience as base.
            result += (RankData.Conditions[0].Count - Info.ConditionCount1) * RankData.Conditions[0].Exp;
            result += (RankData.Conditions[1].Count - Info.ConditionCount2) * RankData.Conditions[1].Exp;
            result += (RankData.Conditions[2].Count - Info.ConditionCount3) * RankData.Conditions[2].Exp;
            result += (RankData.Conditions[3].Count - Info.ConditionCount4) * RankData.Conditions[3].Exp;
            result += (RankData.Conditions[4].Count - Info.ConditionCount5) * RankData.Conditions[4].Exp;
            result += (RankData.Conditions[5].Count - Info.ConditionCount6) * RankData.Conditions[5].Exp;
            result += (RankData.Conditions[6].Count - Info.ConditionCount7) * RankData.Conditions[6].Exp;
            result += (RankData.Conditions[7].Count - Info.ConditionCount8) * RankData.Conditions[7].Exp;
            result += (RankData.Conditions[8].Count - Info.ConditionCount9) * RankData.Conditions[8].Exp;

            // Multiply for more exp on higher ranks.
            result *= (int) Info.Rank * 0.6f;

            // Perfect bonus
            if (IsFullyTrained)
            {
                var bonus = 1.5f;

                // Wednesday: Increase in rank-up bonus for complete mastery of a skill.
                if (month == ErinnMonth.AlbanHeruin)
                    bonus = 2f;

                result *= bonus;
            }

            // Monday: Increase in rank up bonus for life skills (110%).
            if (month == ErinnMonth.AlbanEiler && Data.Category == SkillCategory.Life)
                result *= 1.10f;
            // Tuesday: Increase in rank-up bonus for Combat skills.
            else if (month == ErinnMonth.Baltane && Data.Category == SkillCategory.Combat)
                result *= 1.10f;
            // Thursday: Increase in rank-up bonus for Magic skills.
            else if (month == ErinnMonth.Lughnasadh && Data.Category == SkillCategory.Magic)
                result *= 1.10f;

            return (int) result;
        }

        /// <summary>
        ///     Sets time at which the skill can be used again.
        /// </summary>
        /// <param name="end"></param>
        public void SetCoolDownEnd(DateTime end)
        {
            _creature.CoolDowns.Add("SkillUse:" + Info.Id, end);
        }
    }

    /// <summary>
    ///     Current state of a skill.
    /// </summary>
    public enum SkillState
    {
        None,
        Prepared,
        Ready,
        Used,
        Completed,
        Canceled
    }
}