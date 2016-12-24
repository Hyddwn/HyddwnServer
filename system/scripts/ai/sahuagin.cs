//--- Aura Script -----------------------------------------------------------
// Sahuagin AI
//--- Description -----------------------------------------------------------
// AI for Sahuagin.
//---------------------------------------------------------------------------

[AiScript("sahuagin")]
public class SahuaginAi : AiScript
{
	public SahuaginAi()
	{
		SetVisualField(850, 120);
		SetAggroRadius(200);

		Hates("/pc/", "/pet/");
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(Wander(300, 500, Random(100) < 20));
		}
		else if (Case(20))
		{
			Do(Wait(3000, 6000));
		}
		else if (Case(20))
		{
			Do(CancelSkill());
			Do(Attack(3));
		}
		else if (Case(10))
		{
			Do(Say(L("Kaoo"), "...", "", "", ""));
		}

		Do(Wait(1000, 3000));
	}

	protected override IEnumerable Alert()
	{
		if (Random(100) < 30)
		{
			Do(Say(L("Kaoo"), "...", "", "", ""));
			Do(Wait(1000, 4000));
			Do(Say(L("Kaoo"), "...", "", "", ""));
			Do(Circle(600, 2000, 2000));
		}

		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		if (Random(100) < 50)
			Do(Circle(300, 1000, 1000, false));

		SwitchRandom();
		if (Case(30))
		{
			Do(Say(L("Kaoo"), L("Woooh"), "", "", ""));
			Do(Attack(Rnd(2, 2, 2, 3, 3)));
		}
		else if (Case(30))
		{
			Do(Attack(1, 700));
			Do(Wait(1000));
			Do(Attack(1, 700));
			Do(Wait(1000));

			if (HasSkill(SkillId.Windmill) && Random(100) < 30)
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(UseSkill());
				Do(Say(L("Pfff... Pfff"), L("I... hate... humans..."), "", "", ""));
				Do(Wait(1000, 3000));
				Do(CancelSkill());
			}
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(200, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Follow(200, true, 5000));
			Do(Say("...", "", "", ""));
			Do(Attack(1));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
	}
}
