//--- Aura Script -----------------------------------------------------------
// Bookcases
//--- Description -----------------------------------------------------------
// Located in the Dunbarton Library.
//---------------------------------------------------------------------------

// [Self Improvement] section
public class BookCase1Script : NpcScript
{
	public override void Load()
	{
		SetRace(990006);
		SetName("_bookcase01");
		SetBody(weight: 0f, upper: 0f, lower: 0f);
		SetLocation(72, 8320, 7965, 3);
	}

	protected override async Task Talk()
	{
		await Hook("after_intro");

		End(L("A huge bookshelf stretches all the way from the floor to the Library's high ceiling. Old books and volumes line every shelf.<br/>A soft ray of light can be seen through spots where the bookcases sag, casting a warm sepia light into the room.<br/>A musty scent of old paper permeates the area."));
	}
}

// [Art/Literature] section
public class BookCase2Script : BookCase1Script
{
	public override void Load()
	{
		SetRace(990006);
		SetName("_bookcase02");
		SetBody(weight: 0f, upper: 0f, lower: 0f);
		SetLocation(72, 8320, 8892, 0);
	}
}

// [Literature/Philosophy] section
public class BookCase3Script : BookCase1Script
{
	public override void Load()
	{
		SetRace(990006);
		SetName("_bookcase03");
		SetBody(weight: 0f, upper: 0f, lower: 0f);
		SetLocation(72, 8320, 9818, 0);
	}
}

// [History] section
public class BookCase4Script : BookCase1Script
{
	public override void Load()
	{
		SetRace(990006);
		SetName("_bookcase04");
		SetBody(weight: 0f, upper: 0f, lower: 0f);
		SetLocation(72, 12030, 9807, 0);
	}
}

// [Specialties] section
public class BookCase5Script : BookCase1Script
{
	public override void Load()
	{
		SetRace(990006);
		SetName("_bookcase05");
		SetBody(weight: 0f, upper: 0f, lower: 0f);
		SetLocation(72, 12030, 8895, 0);
	}
}

// [Magazine] section
public class BookCase6Script : BookCase1Script
{
	public override void Load()
	{
		SetRace(990006);
		SetName("_bookcase06");
		SetBody(weight: 0f, upper: 0f, lower: 0f);
		SetLocation(72, 12030, 7971, 0);
	}
}
