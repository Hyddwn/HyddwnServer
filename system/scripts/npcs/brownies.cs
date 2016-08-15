//--- Aura Script -----------------------------------------------------------
// Personal Brownies
//--- Description -----------------------------------------------------------
// Watch over player's personal shops.
//---------------------------------------------------------------------------

public class PersonalBrownie1 : NpcScript
{
	public override void Load()
	{
		SetRace(201);
		SetName("_ps_steward_brownie");
		SetBody(height: -0.5f, lower: 1.1f);
	}

	protected override async Task Talk()
	{
		RndMsg(
			L("There's a lot of great items for sale here!<br/>Take a look!"),
			L("Please take a look at all these items that my owner has put out for you! Come and browse!"),
			L("Please use my owner's shop!")
		);
	}
}

public class PersonalBrownie2 : NpcScript
{
	public override void Load()
	{
		SetRace(202);
		SetName("_ps_weaving_brownie");
		SetBody(height: -0.5f, lower: 1.1f);
	}

	protected override async Task Talk()
	{
		RndMsg(
			L("There's a lot of great items for sale here!<br/>Take a look!"),
			L("Please take a look at all these items that my owner has put out for you! Come and browse!"),
			L("Please use my owner's shop!")
		);
	}
}

public class PersonalBrownie3 : NpcScript
{
	public override void Load()
	{
		SetRace(203);
		SetName("_ps_musician_brownie");
		SetBody(height: -0.5f, lower: 1.1f);
	}

	protected override async Task Talk()
	{
		RndMsg(
			L("There's a lot of great items for sale here!<br/>Take a look!"),
			L("Please take a look at all these items that my owner has put out for you! Come and browse!"),
			L("Please use my owner's shop!")
		);
	}
}
