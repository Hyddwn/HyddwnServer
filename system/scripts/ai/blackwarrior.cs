//--- Aura Script -----------------------------------------------------------
// Black Warrior AI
//--- Description -----------------------------------------------------------
// Used for Black Warrior and Black Soldier.
//---------------------------------------------------------------------------

[AiScript("blackwarrior")]
public class BlackWarriorAi : AiScript
{
	public BlackWarriorAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(600);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(Wander(400, 500, Random() < 50));
		}
		else if (Case(20))
		{
			Do(Wait(4000, 6000));
		}
		else if (Case(10))
		{
			Do(Say("!!!"));
		}

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(30))
		{
			Do(Say("?"));
			Do(Wait(1000, 4000));

			Do(Say("..."));
			Do(Circle(600, 2000, 2000));
		}
		else if (Case(20))
		{
			Do(Say("?"));
			Do(Wait(1000, 2000));
		}

		Do(Say("!!!"));
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(Say("!!!"));
			Do(Wait(2000));

			if (Random() < 80)
			{
				Do(Say("!"));
				Do(Attack(3, 5000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say("!!!!"));
				Do(Attack(1, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say("!"));
				Do(CancelSkill());
				Do(Attack(3, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Say("!!"));
				Do(Wait(2000, 7000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(10))
		{
			if (Random() < 50)
				Do(Wander(200, 200, false));

			Do(Say("!"));
			Do(Attack(3, 4000));

			if (Random() < 20)
			{
				Do(Say("!!"));
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say("!!"));

			if (Random() < 50)
				Do(Circle(400, 2000, 2000));
			else
				Do(Follow(400, true, 5000));

			Do(CancelSkill());
		}
		else if (Case(10))
		{
			SwitchRandom();
			if (Case(60))
			{
				Do(Circle(400, 2000, 2000));
			}
			else if (Case(20))
			{
				Do(Follow(400, true, 5000));
			}
			else if (Case(20))
			{
				Do(KeepDistance(1000, false, 5000));
			}
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Say("!!!!"));
			Do(Wait(1000, 10000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
		if (Random() < 50)
			Do(KeepDistance(1000, false, 2000));
		else
			Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(50))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Say("!!!!"));
			Do(Attack(1, 4000));
		}
		else if (Case(25))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say("!!"));

			if (Random() < 50)
				Do(Circle(400, 2000, 2000));
			else
				Do(Follow(400, true, 5000));

			Do(CancelSkill());
		}
		else if (Case(25))
		{
			Do(Say("!"));
			Do(Attack(3, 8000));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say("?!?!"));
		Do(Attack(3, 4000));
		Do(Wait(1000, 2000));
	}
}
