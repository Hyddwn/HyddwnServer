//--- Aura Script -----------------------------------------------------------
// Small Golem AI
//--- Description -----------------------------------------------------------
// AI for Ciar Beginner Golem (ID 130014)
//---------------------------------------------------------------------------

[AiScript("smallgolem")]
public class SmallGolemAi : AiScript
{
	public SmallGolemAi()
	{
		SetVisualField(1500, 90);
		SetAggroRadius(1200);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(StartSkill(SkillId.Rest));
		Do(Wait(1000000000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(50))
		{
			if (Random() < 20)
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 4000));
			}
			else
			{
				Do(Attack(3, 4000));
			}
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Stomp));
			Do(Wait(500, 2000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		Do(Wait(2000, 3000));
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Wait(500));

		if (Creature.Life < Creature.LifeMax * 0.50f)
		{
			Do(Say("Tachy granide inchatora mana prow!"));
			Do(SetHeight(1.2));
			Do(SetStat(Stat.Str, 100));
			Do(PlaySound("data/sound/Glasgavelen_MagicCasting.wav"));
		}

		SwitchRandom();
		if (Case(20))
		{
			if (Creature.Life < Creature.LifeMax * 0.30f)
				Do(PrepareSkill(SkillId.Stomp));
			else
				Do(PrepareSkill(SkillId.Windmill));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(5))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
	}
}