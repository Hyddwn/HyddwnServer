// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
    public class OptionSetData
    {
        public OptionSetData()
        {
            Effects = new List<OptionSetEffectData>();
        }

        public int Id { get; set; }
        public OptionSetCategory Category { get; set; }
        public UpgradeType Type { get; set; }
        public SkillRank Rank { get; set; }
        public string Allow { get; set; }
        public string Disallow { get; set; }
        public int ItemId { get; set; }
        public bool AlwaysSuccess { get; set; }
        public bool IgnoreRank { get; set; }
        public float RepairMultiplier { get; set; }
        public bool Personalize { get; set; }

        public List<OptionSetEffectData> Effects { get; set; }
    }

    public class OptionSetEffectData
    {
        private static readonly Regex _valueRegex =
            new Regex(@"^(?<sign>[+-])(?<min>[0-9]+)(?:~(?<max>[0-9]+))?%?$", RegexOptions.Compiled);

        public UpgradeType Type { get; set; }
        public UpgradeStat? Stat { get; set; }
        public string Value { get; set; }
        public string If { get; set; }
        public string Check { get; set; }
        public string CheckValue { get; set; }

        public UpgradeEffect GetUpgradeEffect(Random rnd)
        {
            var effect = new UpgradeEffect(Type);

            // Stat effect
            if (Stat != null)
            {
                // Stat
                var stat = (UpgradeStat) Stat;

                // Value type
                UpgradeValueType valueType;
                if (Value.EndsWith("%"))
                    valueType = UpgradeValueType.Percent;
                else
                    valueType = UpgradeValueType.Value;

                // Value
                if (Value == null)
                    throw new NullReferenceException("Value can't be empty if Stat is set.");

                var match = _valueRegex.Match(Value);
                if (!match.Success)
                    throw new FormatException("Invalid value format: " + Value);

                var min = Convert.ToInt32(match.Groups["min"].Value);
                var maxs = match.Groups["max"].Value;
                var max = maxs == "" ? min : Math.Max(min, Convert.ToInt32(maxs));
                if (match.Groups["sign"].Value == "-")
                {
                    // For negative numbers min and max must be reversed,
                    // e.g. -1~2 becomes "from -2 to -1".
                    var temp = min;
                    min = -max;
                    max = -temp;
                }
                var value = (short) rnd.Next(min, max + 1);

                // Set
                effect.SetStatEffect(stat, value, valueType);
            }

            // Stat check
            if (Check == "gte" || Check == "lte" || Check == "gt" || Check == "lt" || Check == "equal")
            {
                // Check type
                UpgradeCheckType checkType;
                switch (Check)
                {
                    case "gte":
                        checkType = UpgradeCheckType.GreaterEqualThan;
                        break;
                    case "lte":
                        checkType = UpgradeCheckType.LowerEqualThan;
                        break;
                    case "gt":
                        checkType = UpgradeCheckType.GreaterThan;
                        break;
                    case "lt":
                        checkType = UpgradeCheckType.LowerThan;
                        break;
                    case "equal":
                        checkType = UpgradeCheckType.Equal;
                        break;
                    default: throw new NotSupportedException("Unknown check type: " + Check);
                }

                // Stat
                var stat = (UpgradeStat) Enum.Parse(typeof(UpgradeStat), If);

                // Value type
                var valueType = UpgradeValueType.Value;
                if (CheckValue.EndsWith("%"))
                    valueType = UpgradeValueType.Percent;

                // Value
                var value = short.Parse(CheckValue.TrimEnd('%'));

                // Set
                effect.SetStatCheck(stat, checkType, value, valueType);
            }
            // Skill check
            else if (Check == "rank_gte" || Check == "rank_lte" || Check == "rank_equal")
            {
                // Check type
                UpgradeCheckType checkType;
                switch (Check)
                {
                    case "rank_gte":
                        checkType = UpgradeCheckType.SkillRankGreaterThan;
                        break;
                    case "rank_lte":
                        checkType = UpgradeCheckType.SkillRankLowerThan;
                        break;
                    case "rank_equal":
                        checkType = UpgradeCheckType.SkillRankEqual;
                        break;
                    default: throw new NotSupportedException("Unknown check type: " + Check);
                }

                // Skill id
                var skillId = (SkillId) Enum.Parse(typeof(SkillId), If);

                // Rank
                var rank = CheckValue == "N" ? SkillRank.Novice : (SkillRank) (16 - Convert.ToInt32(CheckValue, 16));

                // Set
                effect.SetSkillCheck(skillId, checkType, rank);
            }
            // Ptj check
            else if (Check == "ptj_gte")
            {
                // Ptj type
                var ptjType = (PtjType) Enum.Parse(typeof(PtjType), If);

                // Count
                var count = int.Parse(CheckValue);

                // Set
                effect.SetPtjCheck(ptjType, count);
            }
            // Broken check
            else if (If == "intact" || If == "broken")
            {
                var broken = If == "intact" ? false : true;
                effect.SetBrokenCheck(broken);
            }
            // Title check
            else if (If == "title")
            {
                var titleId = int.Parse(CheckValue);
                effect.SetTitleCheck(titleId);
            }
            // Condition check
            else if (If == "condition")
            {
                var condition = int.Parse(CheckValue);
                effect.SetConditionCheck(condition);
            }
            // Month check
            else if (If == "month")
            {
                var month = (Month) Enum.Parse(typeof(Month), CheckValue);
                effect.SetMonthCheck(month);
            }
            // Summon check
            else if (If == "summoned")
            {
                var summonStat = (UpgradeStat) Enum.Parse(typeof(UpgradeStat), CheckValue);
                effect.SetSummonCheck(summonStat);
            }
            // Support check
            else if (If == "supporting")
            {
                var race = (SupportRace) Enum.Parse(typeof(SupportRace), CheckValue);
                effect.SetSupportCheck(race);
            }

            return effect;
        }
    }

    public class OptionSetDb : DatabaseJsonIndexed<int, OptionSetData>
    {
        protected override void ReadEntry(JObject entry)
        {
            entry.AssertNotMissing("id", "category", "rank", "allow", "disallow");

            var data = new OptionSetData();

            data.Id = entry.ReadInt("id");
            data.Category = (OptionSetCategory) entry.ReadInt("category");
            var rank = entry.ReadString("rank");
            data.Rank = rank == "N" ? SkillRank.Novice : (SkillRank) (16 - Convert.ToInt32(rank, 16));
            data.Allow = entry.ReadString("allow");
            data.Disallow = entry.ReadString("disallow");
            data.ItemId = entry.ReadInt("itemId", 62005); // Enchant Scroll
            data.AlwaysSuccess = entry.ReadBool("alwaysSuccess");
            data.IgnoreRank = entry.ReadBool("ignoreRank");
            data.RepairMultiplier = entry.ReadFloat("repairMultiplier", 1);
            data.Personalize = entry.ReadBool("personalize");

            UpgradeType type;
            switch (data.Category)
            {
                case OptionSetCategory.Prefix:
                    type = UpgradeType.Prefix;
                    break;
                case OptionSetCategory.Suffix:
                    type = UpgradeType.Suffix;
                    break;
                case OptionSetCategory.Unknown:
                    type = UpgradeType.ItemAttribute;
                    break; // ?
                case OptionSetCategory.Elemental:
                    type = UpgradeType.Elemental;
                    break;
                case OptionSetCategory.Artisan:
                    type = UpgradeType.Artisan;
                    break;
                case OptionSetCategory.Alchemy:
                    type = UpgradeType.ItemAttribute;
                    break; // ?
                case OptionSetCategory.HolyFlame:
                    type = UpgradeType.ItemAttribute;
                    break;
                default: throw new Exception("Unknown category: " + data.Category);
            }
            data.Type = type;

            if (!Enum.IsDefined(typeof(OptionSetCategory), data.Category))
                throw new DatabaseErrorException("Unknown category: '" + data.Category + "'");

            if (entry.ContainsKey("effects"))
                foreach (JObject effectEntry in entry["effects"])
                {
                    effectEntry.AssertNotMissing("stat", "value");

                    var effectData = new OptionSetEffectData();
                    effectData.Type = data.Type;

                    var stat = effectEntry.ReadString("stat");
                    effectData.Stat = (UpgradeStat) Enum.Parse(typeof(UpgradeStat), stat);
                    effectData.Value = effectEntry.ReadString("value");
                    effectData.If = effectEntry.ReadString("if", null);
                    effectData.Check = effectEntry.ReadString("check", null);
                    effectData.CheckValue = effectEntry.ReadString("cvalue", null);

                    data.Effects.Add(effectData);
                }

            Entries[data.Id] = data;
        }
    }

    public enum OptionSetCategory
    {
        Prefix = 0,
        Suffix = 1,
        Unknown = 2, // Not seen before NA232, Fate Stay Night update
        Elemental = 3,
        Alchemy = 4,
        HolyFlame = 7,
        Artisan = 8
    }
}