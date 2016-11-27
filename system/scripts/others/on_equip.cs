//--- Aura Script -----------------------------------------------------------
// On Equip
//--- Description -----------------------------------------------------------
// Handles general things that should happen when equipping certain kinds
// of equipment, like learning skills.
//---------------------------------------------------------------------------

public class OnEquipSkillLearnScript : GeneralScript
{
	[On("PlayerEquipsItem")]
	public void PlayerEquipsItem(Creature creature, Item item)
	{
		// Give Ranged Attack when equipping a (cross)bow
		if ((item.HasTag("/bow/|/bow01/|/crossbow/")) && !creature.Skills.Has(SkillId.RangedAttack))
			creature.Skills.Give(SkillId.RangedAttack, SkillRank.Novice);

		// Give Dice Tossing When equiping Dice
		if ((item.HasTag("/dice/")) && !creature.Skills.Has(SkillId.DiceTossing))
			creature.Skills.Give(SkillId.DiceTossing, SkillRank.Novice);

		// Give Playing Instrument when equipping an instrument
		if ((item.HasTag("/instrument/")) && !creature.Skills.Has(SkillId.PlayingInstrument))
			creature.Skills.Give(SkillId.PlayingInstrument, SkillRank.Novice);

		// Give Potion Making when equipping a Potion Concoction Kit
		if ((item.HasTag("/potion_making/kit/")) && !creature.Skills.Has(SkillId.PotionMaking))
			creature.Skills.Give(SkillId.PotionMaking, SkillRank.Novice);

		// Give Handicraft when equipping a Handicraft Kit
		if ((item.HasTag("/handicraft_kit/")) && !creature.Skills.Has(SkillId.Handicraft))
			creature.Skills.Give(SkillId.Handicraft, SkillRank.RF);

		// Give Tailoring when equipping a Tailoring Kit
		if ((item.HasTag("/tailor/kit/")) && !creature.Skills.Has(SkillId.Tailoring))
			creature.Skills.Give(SkillId.Tailoring, SkillRank.Novice);

		// Give Blacksmithing when equipping a Blacksmith Hammer
		if ((item.HasTag("/Blacksmith_Hammer/")) && !creature.Skills.Has(SkillId.Blacksmithing))
			creature.Skills.Give(SkillId.Blacksmithing, SkillRank.Novice);

		// Give Fishing when equipping a Fishing Rod
		if ((item.HasTag("/fishingrod/")) && !creature.Skills.Has(SkillId.Fishing))
			creature.Skills.Give(SkillId.Fishing, SkillRank.Novice);

		// Give Enchant when equipping Magic Powder
		if ((item.HasTag("/enchant/powder/")) && !creature.Skills.Has(SkillId.Enchant))
		{
			creature.Skills.Give(SkillId.Enchant, SkillRank.Novice);
			creature.Skills.Train(SkillId.Enchant, 1);
		}

		// Cancel active bolt skill if weapon changes
		if (ChannelServer.Instance.Conf.World.SwitchCancelBolts)
		{
			if (item.Info.Pocket == creature.Inventory.RightHandPocket || item.Info.Pocket == creature.Inventory.LeftHandPocket || item.Info.Pocket == creature.Inventory.MagazinePocket)
			{
				var skill = creature.Skills.ActiveSkill;
				if (skill != null && skill.Is(SkillId.Icebolt, SkillId.Firebolt, SkillId.Lightningbolt))
					creature.Skills.CancelActiveSkill();
			}
		}
	}

	[On("PlayerUnequipsItem")]
	public void PlayerUnequipsItem(Creature creature, Item item)
	{
		// Remove Mana on unequipping wand
		// http://mabinogiworld.com/view/Mana_Evaporation
		if (!IsEnabled("ManaBurnRemove"))
		{
			if (item.HasTag("/wand/|/staff/"))
			{
				var rate = Math2.Clamp(0, 100, 100 - creature.Inventory.GetManaBurnBonus());
				creature.BurnMana(rate);

				if (rate == 100)
					Send.Notice(creature, L("The Mana connected to the Wand has disappeared!"));
				else
					// Unofficial, but makes more sense.
					Send.Notice(creature, L("Some Mana connected to the Wand has disappeared!"));
			}
		}
	}
}
