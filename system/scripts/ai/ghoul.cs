//--- Aura Script -----------------------------------------------------------
// Ghoul AI
//--- Description -----------------------------------------------------------
// AI for Ghoul.
//---------------------------------------------------------------------------

[AiScript("ghoul")]
public class GhoulAi : AiScript
{
	public GhoulAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(400);
		SetAggroLimit(AggroLimit.None);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(30))
		{
			if (Random(100) < 30)
				Do(KeepDistance(500, false, 2000));
			Do(Circle(300, 2000, 2000));
		}
		else if (Case(20))
		{
			Do(CancelSkill());
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
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Wait(100, 2000));
			Do(CancelSkill());
			Do(Attack(Rnd(2, 3), 4000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 2000));
			if (Random(100) < 30)
			{
				Do(PrepareSkill(SkillId.Windmill));
				Do(UseSkill());
			}
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true, 4000));
			Do(CancelSkill());
			if (Random(100) < 80)
			{
				Do(Attack(Rnd(1, 1, 2, 2, 3, 3), 4000));
			}
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(5000, 5000));
			Do(CancelSkill());
			if (Random(100) < 30)
			{
				Do(Attack(Rnd(1, 1, 2, 2, 3, 3), 4000));
			}
		}
	}

	private IEnumerable OnKnockDown()
	{
		if (Creature.Life < Creature.LifeMax * 0.20f)
		{

			if (Random() < 50)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
		}
		else
		{
			SwitchRandom();
			if (Case(40))
			{
				Do(PrepareSkill(SkillId.Windmill));
				//Do(Wait(3000, 3000)); // Official; Fix when AI Windmill can auto-counter
				Do(Wait(1000, 1000)); // Unofficial
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
				Do(Wait(2000, 4000));
				Do(CancelSkill());
			}
		}
	}
}
