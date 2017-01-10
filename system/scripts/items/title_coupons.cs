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