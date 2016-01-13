//--- Aura Script -----------------------------------------------------------
// Flying Sword AI
//--- Description -----------------------------------------------------------
// AI for Flying Swords.
//---------------------------------------------------------------------------

[AiScript("flyingsword")]
public class FlyingSwordAi : AiScript
{
	public FlyingSwordAi()
	{
		SetVisualField(1200, 120);
		SetAggroRadius(800);

		Doubts("/pc/", "/pet/");
		Doubts("/ahchemy_golem/");
		HatesNearby(3000);
		HatesBattleStance(1000);

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.MagicHit, OnMagicHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(10))
		{
			Do(Wander(250, 500));
		}
		else if (Case(30))
		{
			Do(Wander(250, 500, false));
		}
		else if (Case(20))
		{
			Do(Wait(4000, 6000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
		}

		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(CancelSkill());

		SwitchRandom();
		if (Case(20))
		{
			Do(Wait(1000, 2000));
		}
		else if (Case(30))
		{
			Do(Wait(1000, 4000));

			if (Random() < 90)
			{
				Do(Circle(600, 2000, 2000));
			}
			else
			{
				Do(Attack(Rnd(1, 2, 3), 4000));
			}
		}

		Do(PrepareSkill(SkillId.Lightningbolt, Rnd(1, 2)));
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		Do(StackAttack(SkillId.Lightningbolt, 1));
		Do(CancelSkill());

		SwitchRandom();
		if (Case(10))
		{
			if (Random() < 50)
				Do(Wander(200, 200, false));

			Do(Attack(3, 4000));
			if (Random() < 70)
			{
				Do(StackAttack(SkillId.Lightningbolt, 1));
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 1, 1, 1, 1, 2, 3, 4, 5)));
			if (Random() < 50)
				Do(Attack(3, 4000));

			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(CancelSkill());
				Do(Attack(3, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(1000, 2000));
			}

			Do(Wait(1000, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));

			if (Random() < 60)
			{
				Do(Circle(400, 2000, 2000));
			}
			else
			{
				Do(Follow(400, true, 5000));
			}

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
			Do(Wait(1000, 10000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
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
		Do(Attack(3, 4000));

		if (Random() < 40)
		{
			Do(StackAttack(SkillId.Lightningbolt));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnMagicHit()
	{
		Do(Say("Tachy granide inchatora mana prow!"));
		Do(SetHeight(3.0));
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(50))
		{
			Do(PrepareSkill(SkillId.Defense));

			if (Random() < 60)
			{
				Do(Circle(400, 2000, 2000));
			}
			else
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
		else if (Case(25))
		{
			Do(Attack(3, 4000));
			Do(StackAttack(SkillId.Lightningbolt));
			Do(Wait(1000, 2000));
		}
	}
}
