//--- Aura Script -----------------------------------------------------------
// Golem AI
//--- Description -----------------------------------------------------------
// AI for golems.
//---------------------------------------------------------------------------

[AiScript("golem")]
public class GolemAi : AiScript
{
	public GolemAi()
	{
		SetVisualField(1500, 90);
		SetAggroRadius(1200);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(StartSkill(SkillId.Rest));
		Do(Wait(1000000000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000));
			Do(Wait(3000, 8000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Stomp));
			Do(Wait(2000));
			Do(UseSkill());
			Do(Wait(2000));
		}
		else if (Case(30))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(Wait(2000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(30))
		{
			Do(Attack());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}
}
