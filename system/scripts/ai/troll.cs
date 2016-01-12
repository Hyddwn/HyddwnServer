//--- Aura Script -----------------------------------------------------------
// Troll AI
//--- Description -----------------------------------------------------------
// AI for Trolls.
//---------------------------------------------------------------------------

[AiScript("troll")]
public class TrollAi : AiScript
{
	public TrollAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.None);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(60))
		{
			Do(Say("...", "", ""));
			Do(Wander(300, 500, Random() < 50));
			Do(Wait(2000, 7000));
		}
		else if (Case(40))
		{
			Do(Say("Tukarolto-Housikka", "", ""));
			Do(StartSkill(SkillId.Rest));
			Do(Wait(14000, 17000));
			Do(StopSkill(SkillId.Rest));
		}
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(Say("Dal-ku Iikijako", "", ""));
			Do(Attack(3));
		}
		else if (Case(30))
		{
			Do(Say("...", "", ""));
			if (Random() < 60)
			{
				Do(Circle(400, 2000, 2000, false));
			}
			else
			{
				Do(Wander(300, 400, false));
				Do(Wait(1000));
			}
			Do(Attack(3));
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Say("...", "", ""));

			if (Random() < 50)
			{
				Do(Attack(1, 6000));
			}
			else
			{
				Do(Attack(1, 50));
				Do(CancelSkill());
				Do(Attack(3, 6000));
			}
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say("Dokagoto-bokajak!", "", ""));

			SwitchRandom();
			if (Case(60))
			{
				Do(Circle(400, 5000, 5000));
			}
			else if (Case(20))
			{
				Do(Wander(300, 400));
				Do(Wait(4000));
			}
			else if (Case(20))
			{
				Do(Follow(600, true, 5000));
			}
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Say("...", "", ""));
			Do(Wait(5000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(KeepDistance(1000, false, 1500));
			Do(Say("...", "", ""));
			Do(StartSkill(SkillId.Rest));
			Do(Wait(3000, 30000));
			Do(StopSkill(SkillId.Rest));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say("...", "", ""));
			Do(KeepDistance(1000, true, 4000));
			Do(Wait(500, 1000));
			Do(CancelSkill());

			SwitchRandom();
			if (Case(50))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say("...", "", ""));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say("...", "", ""));
				Do(Attack(1, 50));
				Do(CancelSkill());
			}
			else if (Case(20))
			{
				Do(Say("Tu-Karoltogotonun-kada-ta", "", ""));
			}

			Do(Attack(3, 4000));
			Do(Wait(500, 1000));
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say("Gah!", "Ta!", "Kta!", "", ""));
		Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say("Gata-Soau ka-Chintadata!", "Dita-pen tasto!", "", ""));

		SwitchRandom();
		if (Case(50))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(40))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 3000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say("...", "", ""));
		Do(Attack(3));
		Do(Wait(1000, 3000));
		Do(Say("...", "", ""));
	}
}
