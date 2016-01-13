//--- Aura Script -----------------------------------------------------------
// Werewolf AI
//--- Description -----------------------------------------------------------
// AI for normal werewolves.
//---------------------------------------------------------------------------

[AiScript("werewolf")]
public class WerewolfAi : AiScript
{
	public WerewolfAi()
	{
		SetVisualField(1500, 120);
		SetAggroRadius(1200);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(300, 500));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(20))
		{
			Do(Circle(300, 2000, 2000, false, Random() < 50));
		}
		else if (Case(20))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(500, true, 5000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(5000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 4000));
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(Wait(4000));
			Do(UseSkill());
		}

		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 6000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(4000, 8000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
