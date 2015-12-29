//--- Aura Script -----------------------------------------------------------
// Worm AI
//--- Description -----------------------------------------------------------
// AI for Worm type monster.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
// Missing: aggro over time, visual angle, original circle Idle
//---------------------------------------------------------------------------

[AiScript("worm")]
public class WormAi : AiScript
{
	public WormAi()
	{
		SetAggroRadius(600); // audio 400 visual Angle 45°
		Doubts("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
	}

	protected override IEnumerable Idle()
	{
		//if (Random() < 60) // 60%
		//Do(Circle(400, 1000, 3000));
		Do(Wait(2000, 10000));
		Do(CancelSkill());
	}

	protected override IEnumerable Alert()
	{
		if (Random() < 25) // 25%
			Do(PrepareSkill(SkillId.Defense));
		Do(Circle(500, 1000, 3000));
		Do(Wait(2000, 4000));
		Do(CancelSkill());

	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 75) // 75%
		{
			var rndAggro = Random();
			if (rndAggro < 40) // 40%
			{
				Do(Attack(2, 10000));
			}
			else // 60%
			{
				Do(Attack(3, 10000));
			}
		}
		else
		{
			Do(PrepareSkill(SkillId.Defense));
		}
		var rndnum = Random();
		if (rndnum < 40) // 40%
		{
			Do(KeepDistance(400, true, 3000));
		}
		else if (rndnum < 70) // 30%
		{
			Do(KeepDistance(700, false, 3000));
		}
		else // 30%
		{
			Do(Wait(3000));
		}
		Do(CancelSkill());
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnHit()
	{
		var rndOH = Random();
		if (rndOH < 15) // 15%
		{
			Do(KeepDistance(1000, false, 2000));
		}
		else if (rndOH < 30) // 15%
		{
			Do(Timeout(2000, Wander(100, 500, false)));
		}
		else // 70%
		{
			Do(Attack(3, 4000));
		}
	}
}
