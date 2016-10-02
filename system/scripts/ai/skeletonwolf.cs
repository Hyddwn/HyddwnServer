//--- Aura Script -----------------------------------------------------------
// Skeleton Wolf AI
//--- Description -----------------------------------------------------------
// AI for Skeleton and Beetle Wolves.
//---------------------------------------------------------------------------

[AiScript("skeletonwolf")]
public class SkeletonWolfAi : AiScript
{
	public SkeletonWolfAi()
	{
		SetVisualField(650, 120);
		SetAggroRadius(400);

		Doubts("/pc/", "/pet/");
		Hates("/sheep/");
		Hates("/dog/");
		HatesNearby(8000);
		HatesBattleStance(3000);
		SetAggroLimit(AggroLimit.One);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		if (Random() < 50)
		{
			if (Random() < 50)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else
		{
			Do(Circle(400, 1000, 5000));
			Do(Wait(1000, 5000));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 50)
		{
			SwitchRandom();
			if (Case(20))
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
				Do(Wait(3000, 8000));
			}
			else if (Case(40))
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else
		{
			Do(Attack(3, 5000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}
}
