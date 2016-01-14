//--- Aura Script -----------------------------------------------------------
// Argus AI
//--- Description -----------------------------------------------------------
// AI used for all Arguses.
//--- Notes -----------------------------------------------------------------
// The dialog might be different between normal and boss Arguses.
//---------------------------------------------------------------------------

[AiScript("argus")]
public class ArgusAi : AiScript
{
	public ArgusAi()
	{
		SetVisualField(2000, 120);
		SetAggroRadius(800);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Say("...", "", "", ""));

		Do(Wander(400, 500, Random() < 40));
		Do(Wait(2000, 7000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(Say("...", "", "", ""));

			Do(PrepareSkill(SkillId.Stomp));
			Do(Wait(500, 2000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(30))
		{
			Do(Say("...", "", "", ""));

			if (Random() < 60)
				Do(Circle(400, 2000, 2000, false));
			else
				Do(Wander(400, 400, false));

			Do(Attack(3));
		}
		else if (Case(20))
		{
			Do(Say("...", "", "", ""));
			Do(Attack(3));
		}
		else if (Case(10))
		{
			Do(Say("...", "", "", ""));
			Do(PrepareSkill(SkillId.Defense));

			if (Random() < 60)
			{
				Do(Circle(400, 5000, 5000, false));
			}
			else
			{
				Do(Wander(400, 400, false));
				Do(Wait(2000));
			}

			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say("...", "", "", ""));
		Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say("...", "", "", ""));

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
		Do(Attack(3));
		Do(Wait(1000, 3000));
	}
}
