//--- Aura Script -----------------------------------------------------------
// Repairing an Item
//--- Description -----------------------------------------------------------
// Ferghus suggests to visit him, to give repairing a try.
//---------------------------------------------------------------------------

public class RepairingAnItemQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202042);
		SetName("Repairing an Item");
		SetDescription("This is Ferghus of the Blacksmith Shop in Tir Chonaill. I have just bought a new Blacksmith Hammer, so to celebrate this new buy, why don't you bring an item that you'll want to repair? The new hammer needs to be broken in, but I don't have a lot of takers for this repair offer. I'll give you some EXP, too, so drop by if you can. - Ferghus -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202034)); // Gather Cobweb

		AddObjective("keyword", "Request a repair to Ferghus", 1, 18075, 29960, GetKeyword("ExperienceRepair"));

		AddReward(Exp(2000));
		AddReward(Item(1607)); // Repair Guidebook

		// Is there any dialog?
	}
}
