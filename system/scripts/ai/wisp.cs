//--- Aura Script -----------------------------------------------------------
// Wisp AI
//--- Description -----------------------------------------------------------
// AI for wisps.
//--- History ---------------------------------------------------------------
// 1.0 Official AI behavior
// Missing: Stacking charges
//---------------------------------------------------------------------------

[AiScript("wisp")]
public class WispAi : AiScript
{
	readonly string[] RandomChat = new[] { "?", "!?!?", "???" };
	const string AttackChat = "!";
	const string DefenseChat = "!!";
	const string LightningChat = "!!!";
	const string SmashChat = "!!!!";
	const string CounterChat = "!!!!!";

	public WispAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(600);
		SetAggroLimit(AggroLimit.One);

		Doubts("/pc/", "/pet/");
		HatesNearby(7000);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
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
			Do(Say(LightningChat));
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
			Do(Say(RandomChat));
			Do(Wait(1000, 2000));
		}
		else if (Case(30))
		{
			Do(Say(RandomChat));
			Do(Wait(1000, 4000));
			Do(Say(RandomChat));
			Do(Circle(600, 1000, 2000));
		}

		Do(Say(LightningChat));
		Do(PrepareSkill(SkillId.Lightningbolt, Rnd(1, 2)));
		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(10))
		{
			if (Random() < 50)
				Do(Wander(100, 200, false));

			Do(Say(AttackChat));
			Do(Attack(3, 4000));

			SwitchRandom();
			if (Case(60))
			{
				Do(Say(LightningChat));
				Do(StackAttack(SkillId.Lightningbolt));
			}
			else if (Case(20))
			{
				Do(Say(DefenseChat));
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}
			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			Do(Say(LightningChat));
			Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 1, 1, 1, 1, 2, 3, 4, 5)));

			if (Random() < 80)
			{
				Do(Say(AttackChat));
				Do(Attack(3, 4000));
			}

			Do(Wait(500, 2000));
		}
		else if (Case(20))
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(SmashChat));
				Do(Attack(1, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Say(AttackChat));
				Do(CancelSkill());
				Do(Attack(3, 4000));
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Say(DefenseChat));
				Do(Wait(2000, 7000));
			}
			Do(Wait(1000, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say(DefenseChat));

			if (Random() < 60)
			{
				Do(Circle(400, 1000, 2000, false));
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
				Do(Circle(400, 1000, 2000, false));
			}
			else if (Case(20))
			{
				Do(Follow(400, false, 5000));
			}
			else if (Case(20))
			{
				Do(KeepDistance(1000, false, 5000));
			}
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Say(CounterChat));
			Do(Wait(1000, 10000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say(RandomChat));
		Do(Attack(3, 4000));
		if (Random() < 40)
		{
			Do(Say(LightningChat));
			Do(StackAttack(SkillId.Lightningbolt));
			Do(Wait(1000, 2000));
		}
	}

	private IEnumerable OnHit()
	{
		if (Random() < 50)
		{
			Do(KeepDistance(1000, false, 2000));
		}
		else
		{
			Do(Attack(3, 4000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(25))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Say(SmashChat));
			Do(Attack(1, 4000));
		}
		else if (Case(50))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Say(DefenseChat));

			if (Random() < 60)
				Do(Circle(400, 1000, 2000, true));
			else
				Do(Follow(400, true, 5000));

			Do(CancelSkill());
		}
		else if (Case(25))
		{
			Do(Say(AttackChat));
			Do(Attack(3, 4000));
			if (Random() < 40)
			{
				Do(Say(LightningChat));
				Do(StackAttack(SkillId.Lightningbolt));
				Do(Wait(1000, 2000));
			}
		}
	}
}