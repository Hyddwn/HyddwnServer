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
		if ((item.HasTag("/tool/blacksmith/")) && !creature.Skills.Has(SkillId.Blacksmithing))
			creature.Skills.Give(SkillId.Blacksmithing, SkillRank.Novice);

		// Give Enchant when equipping Magic Powder
		if ((item.HasTag("/enchant/powder/")) && !creature.Skills.Has(SkillId.Enchant))
		{
			creature.Skills.Give(SkillId.Enchant, SkillRank.Novice);
			creature.Skills.Train(SkillId.Enchant, 1);
		}
	}
}
