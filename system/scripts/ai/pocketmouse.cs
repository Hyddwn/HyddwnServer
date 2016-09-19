//--- Aura Script -----------------------------------------------------------
// Pocket Mouse AI
//--- Description -----------------------------------------------------------
// AI for Pocket Mice. On Magic Hit level up.
//---------------------------------------------------------------------------

[AiScript("pocketmouse")]
public class PocketMouseAi : AiScript
{
	public PocketMouseAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);

		Doubts("/pc/", "/pet/");
		HatesNearby(3000);
		HatesBattleStance(1000);
		SetAggroLimit(AggroLimit.One);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.MagicHit, OnMagicHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		SwitchRandom();
		if (Case(10))
		{
			Do(Wander(100, 500));
		}
		else if (Case(30))
		{
			Do(Wander(100, 500, false));
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
			Do(Circle(600, 1000, 2000));
			if (Random() < 10)
			{
				Do(Attack(Rnd(1, 2, 3), 4000));
			}
		}

		Do(PrepareSkill(SkillId.Lightningbolt, Rnd(1, 2)));
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		Do(StackAttack(SkillId.Lightningbolt));
		Do(CancelSkill());

		SwitchRandom();
		if (Case(50))
		{
			if (Random() < 60)
				Do(Wander(100, 500, false));
			Do(Attack(1, 4000));
			if (Random() < 50)
				Do(StackAttack(SkillId.Lightningbolt));
			Do(Wait(500, 2000));
		}
		else if (Case(30))
		{
			Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 1, 1, 1, 1, 2, 3, 4, 5)));

			if (Random() < 80)
			{
				Do(Attack(3, 4000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			SwitchRandom();
			if (Case(60))
			{
				Do(Circle(400, 1000, 2000, false));
			}
			else if (Case(20))
			{
				Do(KeepDistance(500, false, 5000));
			}
			else if (Case(20))
			{
				Do(KeepDistance(1000, false, 5000));
			}
		}
	}

	private IEnumerable OnHit()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(KeepDistance(1000, false, 2000));
		}
		else if (Case(50))
		{
			Do(Attack(3, 4000));
		}
	}

	private IEnumerable OnMagicHit()
	{
		Do(Say(L("Tachy granide inchatora mana prow!")));
		Do(SetHeight(3.0));
		Creature.GiveExp(10000);
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

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(25))
		{
			Do(Attack(1, 4000));
		}
		else if (Case(50))
		{
			if (Random() < 60)
				Do(Circle(400, 1000, 2000, true));
			else
				Do(Follow(400, true, 5000));
		}
		else if (Case(25))
		{
			Do(Attack(3, 4000));
			if (Random() < 40)
			{
				Do(StackAttack(SkillId.Lightningbolt));
				Do(Wait(1000, 2000));
			}
		}
	}
}
