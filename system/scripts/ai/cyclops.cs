//--- Aura Script -----------------------------------------------------------
// Cyclops AI
//--- Description -----------------------------------------------------------
// AI for Cyclops and Giant Headless. Essentially a copy of the Ogre AI.
//---------------------------------------------------------------------------

[AiScript("cyclops")]
public class CyclopsAi : AiScript
{
	public CyclopsAi()
	{
		SetVisualField(1600, 120);
		SetAggroRadius(1000);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Say("Ah... arrrghh...", "", "", ""));

		if (Random() < 60)
			Do(Wander(300, 500, Random() < 60));

		Do(Wait(2000, 7000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(Say("Wooohhh", "", "", ""));
			Do(PrepareSkill(SkillId.Stomp));
			Do(Wait(500, 2000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(30))
		{
			Do(Say("Hum...", "", "", ""));

			if (Random() < 80)
			{
				Do(Circle(400, 2000, 2000, false));
			}
			else
			{
				Do(Wander(400, 400, false));
				Do(Wait(1000));
			}

			Do(Attack(3));
		}
		else if (Case(20))
		{
			Do(Say("Growl", "", "", ""));
			Do(Attack(3));
		}
		else if (Case(10))
		{
			Do(Say("Huk", "", "", ""));
			Do(PrepareSkill(SkillId.Defense));

			SwitchRandom();
			if (Case(80))
			{
				Do(Circle(400, 5000, 5000));
			}
			else if (Case(10))
			{
				Do(Wander(400, 400));
				Do(Wait(1000));
			}
			else if (Case(10))
			{
				Do(Follow(600, true, 5000));
			}

			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say("Grrrrr...", "Hmph", "Gr...", "", "", ""));
		Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say("Wooohhh", "", "", ""));

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
