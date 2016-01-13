//--- Aura Script -----------------------------------------------------------
// Cloaker AI
//--- Description -----------------------------------------------------------
// AI for Cloakers.
//---------------------------------------------------------------------------

[AiScript("cloaker")]
public class CloakerAi : AiScript
{
	public CloakerAi()
	{
		SetVisualField(1600, 120);
		SetAggroRadius(1600);
		SetAggroLimit(AggroLimit.Three);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(300, 500));
		Do(Wait(1000, 2000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(50))
		{
			Do(PrepareSkill(SkillId.Lightningbolt));

			if (Random() < 50)
				Do(KeepDistance(600, false, 1000));
			if (Random() < 50)
				Do(KeepDistance(1000, false, 2000));
			else
				Do(Circle(800, 2000, 2000, true, true));
		}
		else if (Case(30))
		{
			if (Random() < 50)
				Do(Wander(200, 200, false));

			Do(Attack(3, 4000));
			if (Random() < 80)
			{
				Do(StackAttack(SkillId.Lightningbolt, 1, 5000));
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(50, true, 1000));
			}
			Do(Wait(500, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 50)
				Do(Circle(400, 2000, 2000, Random() < 50));
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			SwitchRandom();
			if (Case(60))
				Do(Wander(400, 400, Random() < 50));
			else if (Case(20))
				Do(Follow(400, Random() < 50, 5000));
			else if (Case(20))
				Do(KeepDistance(1000, Random() < 50, 5000));
		}
	}

	private IEnumerable OnHit()
	{
		if (Random() < 90)
			Do(KeepDistance(1000, false, 2000));
		else
			Do(Attack(3, 4000));
	}

	private IEnumerable OnKnockDown()
	{
		if (Random() < 50)
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 50)
				Do(Circle(400, 2000, 2000, Random() < 50));
			else
				Do(Follow(400, true, 5000));
			Do(CancelSkill());
		}
		else
		{
			Do(Attack(3, 8000));

			if (Random() < 50)
			{
				Do(StackAttack(SkillId.Lightningbolt, 1, 10000));
				Do(Wait(1000, 2000));
			}
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3, 4000));

		if (Random() < 50)
		{
			Do(StackAttack(SkillId.Lightningbolt, 1, 10000));
			Do(KeepDistance(1000, false, 2000));
		}
	}
}
