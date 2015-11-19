//--- Aura Script -----------------------------------------------------------
// Fiodh Router
//--- Description -----------------------------------------------------------
// Danu was renamed to Fiodh, but the name of the altar is still "Danu",
// which is why we route from here to to the Fiodh scripts, unlike other
// dungeons, where the script for the normal version is also the router.
//---------------------------------------------------------------------------

[DungeonScript("danu_dungeon")]
public class FiodhDungeonRouteScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Fiodh Int 1
		if (item.Info.Id == 63119) // Fiodh Intermediate Fomor Pass for One
		{
			if (creature.Party.MemberCount == 1)
			{
				dungeonName = "gairech_fiodh_middle_1_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("You can only enter this dungeon alone."));
				return false;
			}
		}

		// Fiodh Int 2
		if (item.Info.Id == 63120) // Fiodh Intermediate Fomor Pass for Two
		{
			if (creature.Party.MemberCount == 2)
			{
				dungeonName = "gairech_fiodh_middle_2_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 2 members."));
				return false;
			}
		}

		// Fiodh Int 4
		if (item.Info.Id == 63121) // Fiodh Intermediate Fomor Pass for Four
		{
			if (creature.Party.MemberCount == 4)
			{
				dungeonName = "gairech_fiodh_middle_4_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 4 members."));
				return false;
			}
		}

		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		dungeonName = "gairech_fiodh_dungeon";
		return true;
	}
}
