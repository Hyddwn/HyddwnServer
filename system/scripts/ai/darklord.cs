//--- Aura Script -----------------------------------------------------------
// Dark Lord AI
//--- Description -----------------------------------------------------------
// AI for all Dark Lords. Everything seems to be the same between them.
//---------------------------------------------------------------------------

[AiScript("darklord")]
public class DarkLordAi : AiScript
{
	public DarkLordAi()
	{
		SetVisualField(1200, 800);
		SetAggroRadius(400);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(60))
		{
			Do(Wander(400, 500, Random(100) < 50));
		}
		else if (Case(20))
		{
			Do(Wait(4000, 6000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
		}

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(Wait(1000, 2000));
		}
		else if (Case(25))
		{
			Do(Wait(1000, 4000));
			Do(Circle(600, 2000, 2000, true));
		}
		else if (Case(5))
		{
			Do(Wait(1000, 4000));
			Do(Attack(Rnd(1, 2, 3), 4000));
		}

		Do(PrepareSkill(SkillId.Lightningbolt, Rnd(1, 2)));
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(60))
		{
			if (Random(100) < 75)
			{
				Do(PrepareSkill(SkillId.DarkLord));
				Do(Say(Rnd(L("You should be afraid..."), L("You're pathetic!"), L("I'm here!"), "", "", "", "")));
			}

			Do(Attack(3, 4000));

			SwitchRandom();
			if (Case(60))
			{
				Do(StackAttack(SkillId.Lightningbolt, 1));
			}
			else if (Case(20))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(10))
		{
			Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 1, 1, 1, 1, 2, 3, 4, 5)));

			if (Random(100) < 80)
				Do(Attack(3, 4000));

			Do(Wait(500, 2000));
		}
		else if (Case(10))
		{
			Do(Say(L("I will teach you a lesson."), "", ""));

			SwitchRandom();
			if (Case(60))
			{
				Do(Circle(400, 2000, 2000, true));
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
			Do(Say(L("What are you waiting for?"), "", ""));
			Do(Wait(1000, 10000));
			Do(Say(L("Are you afraid?"), "", ""));
			Do(CancelSkill());
		}
		else if (Case(5))
		{
			SwitchRandom();
			if (Case(70))
			{
				Do(PrepareSkill(SkillId.Smash));

				if (Random(100) < 40)
					Do(CancelSkill());

				Do(Attack(1, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 7000));
			}

			Do(Wait(1000, 2000));
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Defense));

			SwitchRandom();
			if (Case(60))
			{
				Do(Circle(400, 2000, 2000, true));
			}
			else if (Case(40))
			{
				Do(Follow(400, true, 5000));
			}

			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
		//Do(Say(Rnd(L(""), "", "")));

		SwitchRandom();
		if (Case(50))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(20))
		{
			Do(KeepDistance(1000, false, 2000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say(Rnd(L("Is this all you can do?"), L("Ridiculous..."), "", "", "")));

		Do(Attack(3, 4000));

		if (Random(100) < 40)
		{
			Do(StackAttack(SkillId.Lightningbolt, 1));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say(Rnd(L("Argh..."), L("Humph!"), "", "", "")));

		SwitchRandom();
		if (Case(35))
		{
			Do(PrepareSkill(SkillId.Defense));

			SwitchRandom();
			if (Case(60))
			{
				Do(Circle(400, 2000, 2000, true));
			}
			else if (Case(40))
			{
				Do(Follow(400, true, 5000));
			}

			Do(CancelSkill());
		}
		else if (Case(25))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(40))
		{
			if (Random(100) < 40)
				Do(PrepareSkill(SkillId.DarkLord));

			Do(Attack(3, 4000));

			if (Random(100) < 40)
			{
				Do(StackAttack(SkillId.Lightningbolt, 1));
				Do(Wait(1000, 2000));
			}
		}
	}
}
