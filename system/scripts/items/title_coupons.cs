[ItemScript(75688)]
public class SAOKiritoTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10684))
		{
			cr.EnableTitle(10684);
			Send.Notice(cr, L("The title \"SAO Kirito\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75688);
		}
	}
}

[ItemScript(75689)]
public class SAOAsunaTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10685))
		{
			cr.EnableTitle(10685);
			Send.Notice(cr, L("The title \"SAO Asuna\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75689);
		}
	}
}

[ItemScript(75690)]
public class SAOHeathcliffTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10686))
		{
			cr.EnableTitle(10686);
			Send.Notice(cr, L("The title \"SAO Heathcliff\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75690);
		}
	}
}

[ItemScript(75691)]
public class SAOLisbethTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10687))
		{
			cr.EnableTitle(10687);
			Send.Notice(cr, L("The title \"SAO Lisbeth\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75691);
		}
	}
}

[ItemScript(75692)]
public class SAONishidaTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10688))
		{
			cr.EnableTitle(10688);
			Send.Notice(cr, L("The title \"SAO Nishida\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75692);
		}
	}
}

[ItemScript(75693)]
public class SAOPinaTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10689))
		{
			cr.EnableTitle(10689);
			Send.Notice(cr, L("The title \"SAO Pina\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75693);
		}
	}
}

[ItemScript(75694)]
public class ALOKiritoTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10690))
		{
			cr.EnableTitle(10690);
			Send.Notice(cr, L("The title \"ALO Kirito\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75694);
		}
	}
}

[ItemScript(75695)]
public class ALOAsunaTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10691))
		{
			cr.EnableTitle(10691);
			Send.Notice(cr, L("The title \"ALO Asuna\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75695);
		}
	}
}

[ItemScript(75696)]
public class SAOSilicaTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10693))
		{
			cr.EnableTitle(10693);
			Send.Notice(cr, L("The title \"SAO Silica\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75696);
		}
	}
}

[ItemScript(75697)]
public class SAOKleinTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10692))
		{
			cr.EnableTitle(10692);
			Send.Notice(cr, L("The title \"SAO Klein\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75697);
		}
	}
}

[ItemScript(75698)]
public class HalloweenPartyTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10694))
		{
			cr.EnableTitle(10694);
			Send.Notice(cr, L("The title \"Halloween Party\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75698);
		}
	}
}

[ItemScript(75707)]
public class SantaClausTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10696))
		{
			cr.EnableTitle(10696);
			Send.Notice(cr, L("The title \"Santa Claus\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75707);
		}
	}
}

[ItemScript(75708)]
public class GiftSackTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10696))
		{
			cr.EnableTitle(10696);
			Send.Notice(cr, L("The title \"Gift Sack\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75708);
		}
	}
}

[ItemScript(75709)]
public class ChristmasTreeTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10697))
		{
			cr.EnableTitle(10697);
			Send.Notice(cr, L("The title \"Christmas Tree\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75709);
		}
	}
}

[ItemScript(75710)]
public class RedNosedTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(10698))
		{
			cr.EnableTitle(10698);
			Send.Notice(cr, L("The title \"Red-Nosed\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75710);
		}
	}
}

[ItemScript(75712)]
public class WinterAngelTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(15000))
		{
			cr.EnableTitle(15000);
			Send.Notice(cr, L("The title \"Winter Angel\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75712);
		}
	}
}

[ItemScript(75713)]
public class IfItSnowsOnChristmasTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(15001))
		{
			cr.EnableTitle(15001);
			Send.Notice(cr, L("The title \"If It Snows On Christmas\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75713);
		}
	}
}

[ItemScript(75714)]
public class IHopeItRainsOnChristmasTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(15002))
		{
			cr.EnableTitle(15002);
			Send.Notice(cr, L("The title \"I Hope It Rains On Christmas\" has been added!"));
		}
		else
		{
			Send.Notice(cr, L("You already have this title."));
			cr.GiveItem(75714);
		}
	}
}

[ItemScript(75715)]
public class BeCarefulOfIcyRoadsTitleItemScript : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		if (!cr.KnowsTitle(15003))
		{
			cr.EnableTitle(15003);
			Send.Notice(cr, "The title \"Be Careful Of Icy Roads\" has been added!");
		}
		else
		{
			Send.Notice(cr, "You already have this title.");
			cr.GiveItem(75715);
		}
	}
}