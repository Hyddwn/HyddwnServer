//--- Aura Script -----------------------------------------------------------
//  Kobold Bandit AI
//--- Description -----------------------------------------------------------
//  AI for kobold bandits.
//--- History ---------------------------------------------------------------
// 1.0 Added general AI behaviors
// Missing: weaponswap, aggro over time, visual angle...
//---------------------------------------------------------------------------


[AiScript("koboldbandit")]
public class KoboldBanditAi : AiScript
{
	readonly string[] koboldBanditIdle = new[] 
	{
		L("Filthy humans, I remember."),
		L("Fish... I want some fish."),
		L("Hahaha"),
		L("I hate light."),
		L("I will take revenge on those humans."),
		L("Kobold hate humans."),
		L("Kobold is looking around."),
		L("Kobold is looking for humans."),
		L("Kobold will keep a look out"),
		L("Let's pay back those humans."),
		L("Sniff..."),
		L("Stupid humans double-crossed me."),
		L("Those horrible humans..."),
		L("Where are you?!"),
		L("You humans..."),
	};

	readonly string[] koboldBanditAlert = new[]
	{
		L("Are you frightened?"),
		L("Why have you come here?"),
		L("You cruel humans..."),
		L("Planning to steal this land as well?"),
	};

	readonly string[] koboldBanditAttack = new[] 
	{
		L("Come here"),
		L("Give me everything you've got."),
		L("Grr..."),
		L("Hahaha"),
		L("Whoopee"),
		L("Hahaha"),
		L("Ha ha"),
	};

	readonly string[] koboldBanditCounterDefense = new[]
	{
		L("Coward"),
		L("What are you looking at, hah?"),
		L("You're quick to catch on."),
		L("What's this?"),
		L("You want a piece of me!"),
		L("The underground used to be our land."),
	};

	readonly string[] koboldBanditOnDefenseHit = new[]
	{
		L("Now you're in for some trouble!"),
		L("Use your brain!"),
		L("Talk about primitive!"),
	};

	readonly string[] koboldBanditOnKnockDown = new[]
	{
		L("Burp"),
		L("Hehe He"),
		L("Ah"),
		L("A ha ha"),
		L("Auh!"),
		L("Huh Huh"),
		L("Shoot"),
		L("Hahaha"),
		L("Ah hak!"),
	};

	readonly string[] koboldBanditOnHit = new[] 
	{
		L("Eukk"),
		L("Aah"),
		L("Snore"),
		L("Oop"),
		L("Ouch!"),
	};

	public KoboldBanditAi()
	{
		SetAggroRadius(850); // angle 120 audiorange 200
		Doubts("/pc/", "/pet/");
		//SetAggroDelay(7000);
		SetAggroLimit(AggroLimit.Two);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		if (Random() < 90)
			Do(Say(koboldBanditIdle));
		var rndIdle = Random();
		if (rndIdle < 20)
		{
			Do(Wander(100, 500));
			Do(Wait(2000, 5000));
		}
		else if (rndIdle < 60)
		{
			Do(Wander(300, 500, false));
			Do(Wait(4000, 7000));
		}
		else
		{
			Do(Wait(2000, 5000));
		}
	}

	protected override IEnumerable Alert()
	{
		var rndAlert = Random();
		if (rndAlert < 5)
		{
			if (Random() < 90)
				Do(Say(koboldBanditAttack));
			Do(Attack(3, 4000));
		}
		else if (rndAlert < 45)
		{
			if (Random() < 70)
			{
				Do(PrepareSkill(SkillId.Defense));
				if (Random() < 80)
					Do(Say(koboldBanditCounterDefense));
				Do(Circle(500, 1000, 5000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				if (Random() < 80)
					Do(Say(koboldBanditCounterDefense));
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (rndAlert < 90)
		{
			if (Random() < 80)
				Do(Say(koboldBanditAlert));
			Do(Circle(500, 1000, 4000));

		}
		else
		{
			if (Random() < 50)
				Do(Say(koboldBanditAlert));
			Do(Circle(500, 1000, 1000, false));
		}
	}

	protected override IEnumerable Aggro()
	{
		Do(KeepDistance(400, true, 2000));
		Do(Circle(300, 1000, 1000));
		var rndAggro = Random();
		if (rndAggro < 60)
		{
			Do(Attack(3, 5000));
			if (Random() < 90)
				Do(Say(koboldBanditAttack));
		}
		else if (rndAggro < 75)
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Follow(200, false, 5000));
			Do(Attack(1, 4000));
			if (Random() < 90)
				Do(Say(koboldBanditAttack));
		}
		else if (rndAggro < 90)
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(CancelSkill());
			if (Random() < 90)
				Do(Say(koboldBanditCounterDefense));
		}
		else
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say(koboldBanditOnDefenseHit));
		Do(Attack());
		Do(Wait(3000));
	}
	private IEnumerable OnHit()
	{
		if (Random() < 90)
			Do(Say(koboldBanditOnHit));
		if (Random() < 20)
		{
			Do(KeepDistance(10000, true, 2000));
		}
		else
		{
			Do(Attack(3, 4000));
		}
	}

	private IEnumerable OnKnockDown()
	{
		if (Random() < 90)
			Do(Say(koboldBanditOnKnockDown));
		var rndOKD = Random();
		if (rndOKD < 20)
		{
			if (Random() < 50)
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Follow(100, false, 4000));
				Do(CancelSkill());
			}
			else
			{
				Do(PrepareSkill(SkillId.Defense));
				Do(Timeout(4000, Wander(100, 500)));
				Do(CancelSkill());
			}
		}
		else if (rndOKD < 30)
		{
			Do(PrepareSkill(SkillId.Counterattack));
			Do(Wait(2000, 4000));
			Do(CancelSkill());
		}
		else if (rndOKD < 40)
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Attack(1, 5000));
			Do(CancelSkill());
		}
		else if (rndOKD < 70)
		{
			Do(Attack(3, 5000));
			if (Random() < 90)
				Do(Say(koboldBanditAttack));
		}
		else
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(Wait(500));
			Do(CancelSkill());
			Do(Attack(3, 5000));
			if (Random() < 90)
				Do(Say(koboldBanditAttack));
		}
	}
}