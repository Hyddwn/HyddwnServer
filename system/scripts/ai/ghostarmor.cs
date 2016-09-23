//--- Aura Script -----------------------------------------------------------
// Ghost Armor AI
//--- Description -----------------------------------------------------------
// AI for Ghost Armors.
//---------------------------------------------------------------------------

[AiScript("ghostarmor")]
public class GhostArmorAi : AiScript
{
	public GhostArmorAi()
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
		if (Case(25))
		{
			Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 1, 1, 1, 1, 2, 3, 4, 5)));

			if (Random(100) < 80)
				Do(Attack(3, 4000));

			Do(Wait(500, 2000));
		}
		else if (Case(25))
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
		else if (Case(20))
		{
			if (Random(100) < 50)
				Do(Wander(200, 200, false));

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
		else if (Case(10))
		{
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

		if (Random(100) < 40)
		{
			Do(StackAttack(SkillId.Lightningbolt, 1));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(50))
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
		else if (Case(25))
		{
			Do(Attack(3, 4000));

			if (Random(100) < 40)
			{
				Do(StackAttack(SkillId.Lightningbolt, 1));
				Do(Wait(1000, 2000));
			}
		}
	}
}
