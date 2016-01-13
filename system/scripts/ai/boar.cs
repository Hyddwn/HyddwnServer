//--- Aura Script -----------------------------------------------------------
// Boar AI
//--- Description -----------------------------------------------------------
// AI used by Wild Boars. While very similar to some wolves and jackals,
// this AI has some waits in place of skills, as boars only have Defense.
//---------------------------------------------------------------------------

[AiScript("boar")]
public class BoarAi : AiScript
{
	public BoarAi()
	{
		SetVisualField(1600, 180);
		SetAggroRadius(1600);

		Hates("/pc/", "/pet/");
		SetAggroLimit(AggroLimit.None);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(400, 1600));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		SwitchRandom();
		if (Case(55))
		{
			Do(Circle(1600, 1000, 4000));
		}
		else if (Case(25))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(1600, 5000, 5000));
			Do(CancelSkill());
		}
		else if (Case(15))
		{
			Do(Wait(5000));
		}
		else if (Case(5))
		{
			Do(Attack(3, 4000));
		}
	}

	protected override IEnumerable Aggro()
	{
		SwitchRandom();
		if (Case(50))
		{
			Do(Attack(3, 5000));
		}
		else if (Case(15))
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Circle(1600, 1000, 5000));
			Do(CancelSkill());
		}
		else if (Case(35))
		{
			Do(Wait(5000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
