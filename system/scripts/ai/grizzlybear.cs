//--- Aura Script -----------------------------------------------------------
//  Grizzlybear AI
//--- Description -----------------------------------------------------------
//  AI for bears.
//---------------------------------------------------------------------------

[AiScript("grizzlybear")]
public class GrizzlybearAi : AiScript
{
	public GrizzlybearAi()
	{
		SetAggroRadius(700); //90 angle Audio 400
		Doubts("/pc/", "/pet/");
		//SetAggroDelay(4000); 
		//HatesAttacking("/grizzlybearkid/"); duration="500"
		SetAggroLimit(AggroLimit.One);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		Do(Wander(100, 400));
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		var rndalert = Random();
		if (rndalert < 5) // 5%
		{
			Do(Attack(3, 4000));
		}
		else if (rndalert < 45) // 40%
		{
			if (Random() < 70) // 70%
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(400, 1000, 5000));
				Do(CancelSkill());
			}
			else // 30%
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (rndalert < 90) // 45%
		{
			Do(Circle(400, 1000, 4000));
		}
		else // 10%
		{
			Do(Circle(500, 1000, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		if (Random() < 50) // 50%
		{
			var rndagr = Random();
			if (rndagr < 40) // 40%
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Circle(500, 1000, 6000));
				Do(CancelSkill());
			}
			else if (rndagr < 70) // 30%
			{
				Do(PrepareSkill(SkillId.Smash));
				Do(Attack(1, 5000));
				Do(Wait(3000, 8000));
			}
			else // 30%
			{
				Do(PrepareSkill(SkillId.Counterattack));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else // 50%
		{
			Do(Attack(3, 5000));
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Attack());
		Do(Wait(3000));
	}

	private IEnumerable OnHit()
	{
		var onhitr = Random();
		if (onhitr < 15) // 15%
		{
			Do(KeepDistance(100, true, 2000));
		}
		else if (onhitr < 30) // 15%
		{
			Do(Timeout(2000, Wander()));
		}
		else // 70%
		{
			Do(Attack(3, 4000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		var knockd = Random();
		if (knockd < 20) // 20%
		{
			if (Random() < 50) // 50%
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(100, true, 4000));
				Do(CancelSkill());
			}
			else // 50%
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Timeout(4000, Wander()));
				Do(CancelSkill());
			}
		}
		else if (knockd < 30) // 10%
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (knockd < 40) // 10%
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000));
			Do(CancelSkill());
		}
		else if (knockd < 70) // 30%
		{
			Do(Attack(3, 5000));
		}
		else // 30%
		{
			Do(Timeout(500, PrepareSkill(SkillId.Defense)));
			Do(CancelSkill());
			Do(Attack(3, 5000));
		}
	}
}