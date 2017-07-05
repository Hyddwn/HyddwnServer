//--- Aura Script -----------------------------------------------------------
// Wight AI
//--- Description -----------------------------------------------------------
// AI for Wight.
//---------------------------------------------------------------------------

[AiScript("wight")]
public class WightAi : AiScript
{
	public WightAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.None);

		Doubts("/pc/", "/pet/");
		HatesNearby(3000);

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Say("......"));
		SwitchRandom();
		if (Case(30))
		{
			Do(Wander(100, 500, true));
		}
		else if (Case(10))
		{
			Do(Wander(100, 500, false));
		}
		else if (Case(30))
		{
			Do(Wait(4000, 6000));
		}
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		Do(Say(""));
		SwitchRandom();
		if (Case(30))
		{
			Do(Attack(3, 4000));
			SwitchRandom();
			if (Case(33))
			{
				Do(Say(""));
				Do(StackAttack(SkillId.Icebolt, Rnd(1, 2, 5, 5, 5), 10000));
			}
			else if (Case(33))
			{
				Do(Say(""));
				Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 2, 5, 5, 5), 10000));
			}
			else if (Case(33))
			{
				Do(Say(""));
				Do(StackAttack(SkillId.Firebolt, Rnd(1, 2, 5, 5, 5), 10000));
			}
		}
		else if (Case(25))
		{
			Do(KeepDistance(1000, false, 5000));
			SwitchRandom();
			if (Case(33))
			{
				Do(StackAttack(SkillId.Icebolt, Rnd(1, 2, 5, 5, 5), 10000));
			}
			else if (Case(33))
			{
				Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 2, 5, 5, 5), 10000));
			}
			else if (Case(33))
			{
				Do(StackAttack(SkillId.Firebolt, Rnd(1, 2, 5, 5, 5), 10000));
			}
		}
		else if (Case(30))
		{
			SwitchRandom();
			if (Case(30))
			{
				Do(Say(""));
				if (Random(100) < 25)
					Do(Timeout(1000, Wander(100, 200, true)));
				Do(Attack(Rnd(1, 2, 2, 3, 3), 4000));
				if (Random(100) < 60)
				{
					Do(Wait(500, 2000));
					Do(Attack(Rnd(1, 1, 2, 2, 3, 3), 4000));
				}
				if (Random(100) < 30)
				{
					Do(PrepareSkill(SkillId.Windmill));
					Do(UseSkill());
				}
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 3000));
				Do(CancelSkill());
			}
			else if (Case(10))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Wait(500, 2000));
				Do(CancelSkill());
			}
			else if (Case(10))
			{
				Do(Say(""));
				Do(StackAttack(SkillId.Icebolt, Rnd(1, 2, 5, 5, 5), 10000));
				if (Random(100) < 50)
					Do(Attack(3, 4000));
			}
		}
		else if (Case(5))
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
				Do(Say(""));
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 6000));
			}
			Do(Wait(1000, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random(100) < 50)
			{
				Do(Say(""));
				Do(Circle(400, 1000, 3000));
			}
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(Say(""));
			SwitchRandom();
			if (Case(30))
			{
				Do(Circle(400, 1000, 2000));
			}
			else if (Case(30))
			{
				Do(Circle(400, 1000, 2000, false));
			}
			else if (Case(10))
			{
				Do(Follow(400, true, 5000));
			}
			else if (Case(10))
			{
				Do(Follow(400, false, 5000));
			}
			else if (Case(10))
			{
				Do(KeepDistance(1000, true, 5000));
			}
			else if (Case(10))
			{
				Do(KeepDistance(1000, false, 5000));
			}
		}
		else if (Case(5))
		{
			Do(Say(""));
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(1000, 10000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			SwitchRandom();
			if (Case(50))
			{
				Do(Say(""));
				Do(PrepareSkill(SkillId.Defense));
				Do(KeepDistance(1000, false, 3000));
				SwitchRandom();
				if (Case(30))
				{
					Do(CancelSkill());
					Do(StackAttack(SkillId.Lightningbolt, Rnd(1, 5), 6000));
					Do(Say(""));
					if (Random(100) < 20)
					{
						Do(StackAttack(SkillId.Icebolt, Rnd(1, 5), 6000));
						Do(Wait(1000, 2000));
					}
					else
						Do(Attack(3, 4000));
				}
				else if (Case(30))
				{
					Do(CancelSkill());
					Do(StackAttack(SkillId.Icebolt, 1, 6000));
					Do(Wait(1000, 2000));
				}
				else if (Case(30))
				{
					Do(CancelSkill());
					Do(StackAttack(SkillId.Icebolt, 1, 6000));
					if (Random(100) < 20)
					{
						Do(StackAttack(SkillId.Lightningbolt, 1, 6000));
						Do(Wait(1000, 2000));
					}
					else
						Do(Attack(3, 4000));
				}
				else if (Case(50))
				{
					Do(Wait(3000, 4000));
					Do(Wait(1000, 2000));
					Do(CancelSkill());
				}
			}
			else if (Case(50))
			{
				if (Random(100) < 50)
				{
					Do(CancelSkill());
					Do(StackAttack(SkillId.Icebolt, 1, 6000));
				}
				else
				{
					Do(Wait(3000, 4000));
					Do(Wait(1000, 2000));
					Do(CancelSkill());
				}
			}
			else if (Case(30))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(3000, 7000));
				Do(CancelSkill());
				Do(Wait(1000, 2000));
			}
		}
	}

	private IEnumerable OnHit()
	{
		Do(Say(""));
		if (Random(100) < 20)
			Do(KeepDistance(600, false, 2000));
		else
			Do(Attack(3, 4000));
	}

	private IEnumerable OnDefenseHit()
	{
		if (Random(100) < 40)
		{
			Do(Attack(3, 4000));
			if (Random(100) < 50)
				Do(Wait(1000, 2000));
			else
			{
				Do(StackAttack(SkillId.Icebolt, 5, 8000));
				Do(Attack(3, 4000));
			}
		}
		else
		{
			Do(Attack(1, 4000));
			Do(Wait(2000, 2000));
			Do(Attack(1, 4000));
			Do(Wait(2000, 2000));
			Do(Attack(1, 4000));
			Do(Wait(2000, 2000));
			Do(Attack(1, 4000));
			Do(Wait(2000, 2000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(PrepareSkill(SkillId.Windmill));
			//Do(Wait(3000, 4000)); // Official; Fix when AI Windmill can auto-counter
			Do(Wait(1000, 1500)); // Unofficial
			Do(UseSkill());
		}
		else if (Case(30))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(30))
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random(100) < 60)
				Do(Circle(400, 1000, 2000));
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
	}
}
