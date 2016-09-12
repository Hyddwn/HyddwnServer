//--- Aura Script -----------------------------------------------------------
// Glas Ghaibhleann AI
//--- Description -----------------------------------------------------------
// AI for G1's final boss.
//---------------------------------------------------------------------------

[AiScript("glasghaibhleann")]
public class GlasGhaibhleannAi : AiScript
{
	public GlasGhaibhleannAi()
	{
		SetVisualField(1200, 1200);
		SetAggroRadius(400);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
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
		if (Case(20))
		{
			Do(Wait(2000, 5000));
		}
		else if (Case(2))
		{
			Do(KeepDistance(1000, true, 5000));
		}
		else if (Case(18))
		{
			Do(Wander(500, 500, true));
		}
		else if (Case(20))
		{
			Do(Attack(1, 4000));
			Do(Wait(4000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(Wait(4000, 6000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(4000, 12000));
			Do(CancelSkill());
		}
		else if (Case(20))
		{
			Do(PrepareSkill(SkillId.GlasGhaibhleannSkill));
			Do(UseSkill());
			Do(Wait(2500));
		}

		Do(Wait(3000, 3000));
		Do(Summon(160003, 4, 400, 1000)); // Firewood Gargoyle
		Do(Summon(160004, 1, 400, 1000)); // Seal Scroll Gargoyle
		Do(Wait(3000, 3000));
	}

	private IEnumerable OnHit()
	{
		SwitchRandom();
		if (Case(70))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(20))
		{
			Do(Wait(2000, 5000));
		}
		else if (Case(5))
		{
			Do(KeepDistance(1000, true, 5000));
		}
		else if (Case(5))
		{
			Do(Wander(500, 500, true));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(1, 4000));
		Do(Wait(4000));
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(40))
		{
			Do(Attack(1, 4000));
			Do(Wait(4000, 4000));
		}
		else if (Case(40))
		{
			Do(PrepareSkill(SkillId.Windmill));
			Do(Wait(4000, 4000));
			Do(UseSkill());
			Do(Wait(1500));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(2000, 8000));
			Do(CancelSkill());
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.GlasGhaibhleannSkill));
			Do(UseSkill());
			Do(Wait(5500));
		}
	}
}
