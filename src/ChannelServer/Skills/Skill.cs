// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using Aura.Shared.Util;
using Aura.Channel.Util.Configuration.Files;
using System.Globalization;
using Aura.Mabi;
using Aura.Channel.Scripting.Scripts;

namespace Aura.Channel.Skills
{
	public class Skill
	{
		private Creature _creature;
		private int _race;

		/// <summary>
		/// Information about the skill, serialized to packets.
		/// </summary>
		public SkillInfo Info;

		/// <summary>
		/// Data about the skill, loaded from the db.
		/// </summary>
		public SkillData Data { get; protected set; }

		/// <summary>
		/// Data about the skill's current rank, loaded from the db.
		/// </summary>
		public SkillRankData RankData { get; protected set; }

		/// <summary>
		/// The skills current state.
		/// </summary>
		public SkillState State { get; set; }

		/// <summary>
		/// Holds time at which the skill is fully loaded.
		/// </summary>
		public DateTime CastEnd { get; set; }

		private int _stack = 0;
		/// <summary>
		/// Gets or sets loaded stack count, capped at 0~max.
		/// Updates client automatically.
		/// </summary>
		public int Stacks
		{
			get { return _stack; }
			set
			{
				_stack = Math2.Clamp(0, sbyte.MaxValue, value);
				Send.SkillStackSet(_creature, this.Info.Id, _stack);
			}
		}

		/// <summary>
		/// Returns true if skill has enough experience and is below max rank.
		/// </summary>
		public bool IsRankable { get { return (this.Info.Experience >= 100000 && this.Info.Rank < this.Info.MaxRank); } }

		/// <summary>
		/// Returns true if all training conditions were cleared.
		/// </summary>
		public bool IsFullyTrained
		{
			get
			{
				return (
					this.Info.ConditionCount1 + this.Info.ConditionCount2 + this.Info.ConditionCount3 +
					this.Info.ConditionCount4 + this.Info.ConditionCount5 + this.Info.ConditionCount6 +
					this.Info.ConditionCount7 + this.Info.ConditionCount8 + this.Info.ConditionCount9 == 0);
			}
		}

		/// <summary>
		/// New Skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="id"></param>
		/// <param name="rank"></param>
		/// <param name="race"></param>
		public Skill(Creature creature, SkillId id, SkillRank rank, int race)
		{
			_creature = creature;
			_race = race;

			this.Info.Id = id;
			this.Info.Rank = rank;
			this.Info.MaxRank = rank;

			this.Info.Flag = SkillFlags.Shown;

			// The conditions are set to the max and are reduced afterwards,
			// making them "Complete" once they reach 0. Initializing to 1
			// in case of problems.
			this.Info.ConditionCount1 = 1;
			this.Info.ConditionCount2 = 1;
			this.Info.ConditionCount3 = 1;
			this.Info.ConditionCount4 = 1;
			this.Info.ConditionCount5 = 1;
			this.Info.ConditionCount6 = 1;
			this.Info.ConditionCount7 = 1;
			this.Info.ConditionCount8 = 1;
			this.Info.ConditionCount9 = 1;

			this.LoadRankData();
		}

		/// <summary>
		/// Loads rank data, based on current rank.
		/// </summary>
		public void LoadRankData()
		{
			this.Data = AuraData.SkillDb.Find((int)this.Info.Id);
			if (this.Data == null)
				throw new Exception("Skill.LoadRankData: Skill data not found for '" + this.Info.Id.ToString() + "'.");

			if ((this.RankData = this.Data.GetRankData(this.Info.Rank, _race)) == null)
				throw new Exception("Skill.LoadRankData: No rank data found for '" + this.Info.Id.ToString() + "@" + this.Info.Rank.ToString() + "'.");

			this.Info.MaxRank = this.Data.MaxRank;

			this.Info.ConditionCount1 = (short)this.RankData.Conditions[0].Count;
			this.Info.ConditionCount2 = (short)this.RankData.Conditions[1].Count;
			this.Info.ConditionCount3 = (short)this.RankData.Conditions[2].Count;
			this.Info.ConditionCount4 = (short)this.RankData.Conditions[3].Count;
			this.Info.ConditionCount5 = (short)this.RankData.Conditions[4].Count;
			this.Info.ConditionCount6 = (short)this.RankData.Conditions[5].Count;
			this.Info.ConditionCount7 = (short)this.RankData.Conditions[6].Count;
			this.Info.ConditionCount8 = (short)this.RankData.Conditions[7].Count;
			this.Info.ConditionCount9 = (short)this.RankData.Conditions[8].Count;

			if (this.RankData.Conditions[0].Visible) this.Info.Flag |= SkillFlags.ShowCondition1;
			if (this.RankData.Conditions[1].Visible) this.Info.Flag |= SkillFlags.ShowCondition2;
			if (this.RankData.Conditions[2].Visible) this.Info.Flag |= SkillFlags.ShowCondition3;
			if (this.RankData.Conditions[3].Visible) this.Info.Flag |= SkillFlags.ShowCondition4;
			if (this.RankData.Conditions[4].Visible) this.Info.Flag |= SkillFlags.ShowCondition5;
			if (this.RankData.Conditions[5].Visible) this.Info.Flag |= SkillFlags.ShowCondition6;
			if (this.RankData.Conditions[6].Visible) this.Info.Flag |= SkillFlags.ShowCondition7;
			if (this.RankData.Conditions[7].Visible) this.Info.Flag |= SkillFlags.ShowCondition8;
			if (this.RankData.Conditions[8].Visible) this.Info.Flag |= SkillFlags.ShowCondition9;
		}

		/// <summary>
		/// Changes rank, resets experience, loads rank data.
		/// </summary>
		/// <param name="rank"></param>
		public void ChangeRank(SkillRank rank)
		{
			this.Info.Rank = rank;
			this.Info.Experience = 0;
			this.Info.Flag &= ~SkillFlags.Rankable;
			this.LoadRankData();

			ChannelServer.Instance.Events.OnSkillRankChanged(_creature, this);
		}

		/// <summary>
		/// Increases training condition count.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="amount"></param>
		public void Train(int condition, int amount = 1)
		{
			// Only characters can train skills.
			if (!_creature.IsCharacter)
				return;

			var bonus = "";

			// Apply skill exp multiplier
			if (ChannelServer.Instance.Conf.World.SkillExpRate != 1)
			{
				amount = (int)(amount * ChannelServer.Instance.Conf.World.SkillExpRate);
				bonus = string.Format(Localization.Get(" (Skill Exp Rate Bonus: x{0})"), ChannelServer.Instance.Conf.World.SkillExpRate);
			}

			// Change count and reveal the condition
			if (amount > 0)
			{
				switch (condition)
				{
					case 1: if (this.Info.ConditionCount1 == 0) return; this.Info.ConditionCount1 = (short)Math.Max(0, this.Info.ConditionCount1 - amount); this.Info.Flag |= SkillFlags.ShowCondition1; break;
					case 2: if (this.Info.ConditionCount2 == 0) return; this.Info.ConditionCount2 = (short)Math.Max(0, this.Info.ConditionCount2 - amount); this.Info.Flag |= SkillFlags.ShowCondition2; break;
					case 3: if (this.Info.ConditionCount3 == 0) return; this.Info.ConditionCount3 = (short)Math.Max(0, this.Info.ConditionCount3 - amount); this.Info.Flag |= SkillFlags.ShowCondition3; break;
					case 4: if (this.Info.ConditionCount4 == 0) return; this.Info.ConditionCount4 = (short)Math.Max(0, this.Info.ConditionCount4 - amount); this.Info.Flag |= SkillFlags.ShowCondition4; break;
					case 5: if (this.Info.ConditionCount5 == 0) return; this.Info.ConditionCount5 = (short)Math.Max(0, this.Info.ConditionCount5 - amount); this.Info.Flag |= SkillFlags.ShowCondition5; break;
					case 6: if (this.Info.ConditionCount6 == 0) return; this.Info.ConditionCount6 = (short)Math.Max(0, this.Info.ConditionCount6 - amount); this.Info.Flag |= SkillFlags.ShowCondition6; break;
					case 7: if (this.Info.ConditionCount7 == 0) return; this.Info.ConditionCount7 = (short)Math.Max(0, this.Info.ConditionCount7 - amount); this.Info.Flag |= SkillFlags.ShowCondition7; break;
					case 8: if (this.Info.ConditionCount8 == 0) return; this.Info.ConditionCount8 = (short)Math.Max(0, this.Info.ConditionCount8 - amount); this.Info.Flag |= SkillFlags.ShowCondition8; break;
					case 9: if (this.Info.ConditionCount9 == 0) return; this.Info.ConditionCount9 = (short)Math.Max(0, this.Info.ConditionCount9 - amount); this.Info.Flag |= SkillFlags.ShowCondition9; break;
					default:
						Log.Error("Skill.Train: Unknown training condition ({0})", condition);
						break;
				}
			}

			var exp = this.UpdateExperience();
			if (exp > 0)
				Send.SkillTrainingUp(_creature, this, exp, bonus);

			this.CheckMaster();
		}

		/// <summary>
		/// Enables master title if skill is on r1 and fully trained.
		/// </summary>
		private void CheckMaster()
		{
			// Skip if not R1 or already has the master title.
			if (this.Info.Rank != SkillRank.R1 || _creature.Titles.IsUsable(this.Data.MasterTitle))
				return;

			// Give master title if all conditions were met. (met == 0)
			if (this.IsFullyTrained)
				_creature.Titles.Enable(this.Data.MasterTitle);
		}

		/// <summary>
		/// Updates exp and returns gained amount.
		/// </summary>
		/// <returns></returns>
		public float UpdateExperience()
		{
			var result = this.Info.Experience / 1000f;
			var exp = 0f;
			exp += ((this.RankData.Conditions[0].Count - this.Info.ConditionCount1) * this.RankData.Conditions[0].Exp);
			exp += ((this.RankData.Conditions[1].Count - this.Info.ConditionCount2) * this.RankData.Conditions[1].Exp);
			exp += ((this.RankData.Conditions[2].Count - this.Info.ConditionCount3) * this.RankData.Conditions[2].Exp);
			exp += ((this.RankData.Conditions[3].Count - this.Info.ConditionCount4) * this.RankData.Conditions[3].Exp);
			exp += ((this.RankData.Conditions[4].Count - this.Info.ConditionCount5) * this.RankData.Conditions[4].Exp);
			exp += ((this.RankData.Conditions[5].Count - this.Info.ConditionCount6) * this.RankData.Conditions[5].Exp);
			exp += ((this.RankData.Conditions[6].Count - this.Info.ConditionCount7) * this.RankData.Conditions[6].Exp);
			exp += ((this.RankData.Conditions[7].Count - this.Info.ConditionCount8) * this.RankData.Conditions[7].Exp);
			exp += ((this.RankData.Conditions[8].Count - this.Info.ConditionCount9) * this.RankData.Conditions[8].Exp);
			this.Info.Experience = (int)(exp * 1000);

			if (this.IsRankable)
				this.Info.Flag |= SkillFlags.Rankable;

			return (exp - result);
		}

		/// <summary>
		/// Activates given flag(s).
		/// </summary>
		/// <param name="flags"></param>
		public void Activate(SkillFlags flags)
		{
			this.Info.Flag |= flags;
		}

		/// <summary>
		/// Deativates given flag(s).
		/// </summary>
		/// <param name="flags"></param>
		public void Deactivate(SkillFlags flags)
		{
			this.Info.Flag &= ~flags;
		}

		/// <summary>
		/// Returns true if skill has the given flags.
		/// </summary>
		/// <param name="flags"></param>
		public bool Has(SkillFlags flags)
		{
			return ((this.Info.Flag & flags) != 0);
		}

		/// <summary>
		/// Returns cast time of skill, specific for its creature.
		/// </summary>
		/// <returns></returns>
		public int GetCastTime()
		{
			var result = 0;

			// Characters/Dynamic
			if (_creature.IsCharacter && AuraData.FeaturesDb.IsEnabled("CombatSystemRenewal"))
				result = this.RankData.NewLoadTime;
			// Monsters/Pets
			else
				result = this.RankData.LoadTime;

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
						(this.Is(SkillId.Firebolt, SkillId.Fireball) && rh.HasTag("/fire_wand/")) ||
						(this.Is(SkillId.Lightningbolt, SkillId.Thunder) && rh.HasTag("/lightning_wand/")) ||
						(this.Is(SkillId.Icebolt, SkillId.IceSpear) && rh.HasTag("/ice_wand/"));

					// Modify if valid
					if (valid)
						result = (int)(result * Math.Max(0, 1f - mod / 100f));
				}
			}

			return result;
		}

		/// <summary>
		/// Returns true if the skill has one of the given ids.
		/// </summary>
		/// <param name="skillId"></param>
		/// <returns></returns>
		public bool Is(params SkillId[] skillId)
		{
			return skillId.Contains(this.Info.Id);
		}

		/// <summary>
		/// Returns exp that the creature would get for a rank up of this
		/// skill in its current state.
		/// </summary>
		/// <remarks>
		/// The formula is entirely custom and is based on a very small
		/// amout of test values, which it doesn't match 100% either.
		/// However, the results seem reasonable, they appear to be close
		/// to officials, and going by the lack of research, nobody ever
		/// bothered to take a closer look at this feature anyway.
		/// </remarks>
		/// <returns></returns>
		public int GetExpBonus()
		{
			var result = 0f;
			var month = ErinnTime.Now.Month;

			// Use current training experience as base.
			result += ((this.RankData.Conditions[0].Count - this.Info.ConditionCount1) * this.RankData.Conditions[0].Exp);
			result += ((this.RankData.Conditions[1].Count - this.Info.ConditionCount2) * this.RankData.Conditions[1].Exp);
			result += ((this.RankData.Conditions[2].Count - this.Info.ConditionCount3) * this.RankData.Conditions[2].Exp);
			result += ((this.RankData.Conditions[3].Count - this.Info.ConditionCount4) * this.RankData.Conditions[3].Exp);
			result += ((this.RankData.Conditions[4].Count - this.Info.ConditionCount5) * this.RankData.Conditions[4].Exp);
			result += ((this.RankData.Conditions[5].Count - this.Info.ConditionCount6) * this.RankData.Conditions[5].Exp);
			result += ((this.RankData.Conditions[6].Count - this.Info.ConditionCount7) * this.RankData.Conditions[6].Exp);
			result += ((this.RankData.Conditions[7].Count - this.Info.ConditionCount8) * this.RankData.Conditions[7].Exp);
			result += ((this.RankData.Conditions[8].Count - this.Info.ConditionCount9) * this.RankData.Conditions[8].Exp);

			// Multiply for more exp on higher ranks.
			result *= (int)this.Info.Rank * 0.6f;

			// Perfect bonus
			if (this.IsFullyTrained)
			{
				var bonus = 1.5f;

				// Wednesday: Increase in rank-up bonus for complete mastery of a skill.
				if (month == ErinnMonth.AlbanHeruin)
					bonus = 2f;

				result *= bonus;
			}

			// Monday: Increase in rank up bonus for life skills (110%).
			if (month == ErinnMonth.AlbanEiler && this.Data.Category == SkillCategory.Life)
				result *= 1.10f;
			// Tuesday: Increase in rank-up bonus for Combat skills.
			else if (month == ErinnMonth.Baltane && this.Data.Category == SkillCategory.Combat)
				result *= 1.10f;
			// Thursday: Increase in rank-up bonus for Magic skills.
			else if (month == ErinnMonth.Lughnasadh && this.Data.Category == SkillCategory.Magic)
				result *= 1.10f;

			return (int)result;
		}
	}

	/// <summary>
	/// Current state of a skill.
	/// </summary>
	public enum SkillState
	{
		None,
		Prepared,
		Ready,
		Used,
		Completed,
		Canceled,
	}
}
