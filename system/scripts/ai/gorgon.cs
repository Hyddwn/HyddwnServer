//--- Aura Script -----------------------------------------------------------
// Gorgon AI
//--- Description -----------------------------------------------------------
// AI used for Gorgons.
//--- Notes -----------------------------------------------------------------
// The dialog might be different between normal and boss Arguses.
//---------------------------------------------------------------------------

[AiScript("gorgon")]
public class GorgonAi : AiScript
{
	public GorgonAi()
	{
		SetVisualField(1500, 90);
		SetAggroRadius(1200);
		SetAggroLimit(AggroLimit.None);

		Hates("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		if (Random() < 20)
			Do(Wander(400, 500));
		Do(Wait(2000, 4000));
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(60))
		{
			Do(Attack(3, Rnd(2000, 4000)));
			Do(Wait(0, 4000));
		}
		else if (Case(20))
		{
			Do(Circle(600, 2000, 2000));
		}
		else if (Case(10))
		{
			Do(KeepDistance(600, false, 2000));
		}
		else if (Case(10))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Follow(600, true, 5000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnHit()
	{
		SwitchRandom();
		if (Case(70))
		{
			Do(Attack(3, 4000));
		}
		else if (Case(15))
		{
			Do(KeepDistance(600, false, 2000));
		}
		else if (Case(15))
		{
			Do(Wander(500, 500, false));
			Do(Wait(1000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		SwitchRandom();
		if (Case(60))
		{
			Do(PrepareSkill(SkillId.Defense));
			if (Random() < 50)
				Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (Case(40))
		{
			Do(Attack(3, 7000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
