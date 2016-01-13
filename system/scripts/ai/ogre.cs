//--- Aura Script -----------------------------------------------------------
// Ogre AI
//--- Description -----------------------------------------------------------
// AI for free range Ogres.
//---------------------------------------------------------------------------

[AiScript("ogre")]
public class OgreAi : AiScript
{
	public OgreAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Alert()
	{
		Do(Say("I can smell humans!", "Human flesh...", "", "", ""));

		if (Random() < 60)
			Do(Wander(300, 500, Random() < 60));

		Do(Wait(2000, 7000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(Say("I will feed on you.", "You've made me angry!", "Come here!", "Give me your flesh!", "", "", ""));
			Do(PrepareSkill(SkillId.Stomp));
			Do(Wait(500, 2000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(30))
		{
			Do(Say("I'm starving... Sniff!", "Sniff...", "", "", ""));

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
			Do(Say("Wait!", "Ha ha ha", "Where should I eat first...", "", "", ""));
			Do(Attack(3));
		}
		else if (Case(10))
		{
			Do(Say("Grrr... Grrr...", "", "", ""));
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
		Do(Say("Huk!", "Ugh!", "Grrr...", "Argh!!", "", "", ""));
		Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say("Here I come!", "", "", ""));

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
