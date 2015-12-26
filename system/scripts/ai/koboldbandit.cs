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
	// Kobold Phrases
	string[] koboldBanditIdle = new[] {
		"Filthy humans, I remember.",
		"Fish... I want some fish.",
		"Hahaha",
		"I hate light.",
		"I will take revenge on those humans.",
		"Kobold hate humans.",
		"Kobold is looking around.",
		"Kobold is looking for humans.",
		"Kobold will keep a look out",
		"Let's pay back those humans.",
		"Sniff...",
		"Stupid humans double-crossed me.",
		"Those horrible humans...",
		"Where are you?!",
		"You humans...",
		};

	string[] koboldBanditAlert = new[] {
		"Are you frightened?",
		"Why have you come here?",
		"You cruel humans...",
		"Planning to steal this land as well?",
		};

	string[] koboldBanditAttack = new[] {
		"Come here",
		"Give me everything you've got.",
		"Grr...",
		"Hahaha",
		"Whoopee",
		"Hahaha",
		"Ha ha",
		};

	string[] koboldBanditCounterDefense = new[] {
		"Coward",
		"What are you looking at, hah?",
		"You're quick to catch on.",
		"What's this?",
		"You want a piece of me!",
		"The underground used to be our land.",
		};

	string[] koboldBanditOnDefenseHit = new[] {
		"Now you're in for some trouble!",
		"Use your brain!",
		"Talk about primitive!",
		};

	string[] koboldBanditOnKnockDown = new[] {
		"Burp",
		"Hehe He",
		"Ah",
		"A ha ha",
		"Auh!",
		"Huh Huh",
		"Shoot",
		"Hahaha",
		"Ah hak!",
		};

	string[] koboldBanditOnHit = new[] {
		"Eukk",
		"Aah",
		"Snore",
		"Oop",
		"Ouch!",
		};


	// Real AI starts here
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
		{
			Do(Say(koboldBanditIdle[Random(koboldBanditIdle.Length)]));
		}
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
			if (Random() < 90) // for "blanks"
			{
				Do(Say(koboldBanditAttack[Random(koboldBanditAttack.Length)]));
			}
			Do(Attack(3, 4000));
		}
		else if (rndAlert < 45)
		{
			if (Random() < 70)
			{
				if (Random() < 50)
				{
					Do(PrepareSkill(SkillId.Defense));
					if (Random() < 80)
					{
						Do(Say(koboldBanditCounterDefense[Random(koboldBanditCounterDefense.Length)]));
					}
					Do(Circle(500, 1000, 5000, true));
					Do(CancelSkill());
				}
				else
				{
					Do(PrepareSkill(SkillId.Defense));
					if (Random() < 80)
					{
						Do(Say(koboldBanditCounterDefense[Random(koboldBanditCounterDefense.Length)]));
					}
					Do(Circle(500, 1000, 5000, false));
					Do(CancelSkill());
				}
			}
			else
			{
				Do(PrepareSkill(SkillId.Counterattack));
				if (Random() < 80)
				{
					Do(Say(koboldBanditCounterDefense[Random(koboldBanditCounterDefense.Length)]));
				}
				Do(Wait(5000));
				Do(CancelSkill());
			}
		}
		else if (rndAlert < 90)
		{
			if (Random() < 80)
			{
				Do(Say(koboldBanditAlert[Random(koboldBanditAlert.Length)]));
			}
			if (Random() < 55)
			{
				Do(Circle(500, 1000, 4000, true));
			}
			else
			{
				Do(Circle(500, 1000, 4000, false));
			}
		}
		else
		{
			if (Random() < 50)
			{
				Do(Say(koboldBanditAlert[Random(koboldBanditAlert.Length)]));
			}
			if (Random() < 50)
			{
				Do(Circle(500, 1000, 1000, true, false));
			}
			else
			{
				Do(Circle(500, 1000, 1000, false, false));
			}
		}
	}

	protected override IEnumerable Aggro()
	{
		Do(Timeout(2000, KeepDistance(400, true)));
		Do(Circle(300, 1000, 1000));
		var rndAggro = Random();
		if (rndAggro < 60)
		{
			Do(Attack(3, 5000));
			if (Random() < 90)
			{
				Do(Say(koboldBanditAttack[Random(koboldBanditAttack.Length)]));
			}
		}
		else if (rndAggro < 75)
		{
			Do(PrepareSkill(SkillId.Smash));
			Do(Follow(200, false, 5000));
			Do(Attack(1, 4000));
			if (Random() < 90)
			{
				Do(Say(koboldBanditAttack[Random(koboldBanditAttack.Length)]));
			}
		}
		else if (rndAggro < 90)
		{
			Do(PrepareSkill(SkillId.Defense));
			Do(CancelSkill());
			if (Random() < 90)
			{
				Do(Say(koboldBanditCounterDefense[Random(koboldBanditCounterDefense.Length)]));
			}
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
		Do(Say(koboldBanditOnDefenseHit[Random(koboldBanditOnDefenseHit.Length)]));
		Do(Attack());
		Do(Wait(3000));
	}
	private IEnumerable OnHit()
	{
		if (Random() < 90)
		{
			Do(Say(koboldBanditOnHit[Random(koboldBanditOnHit.Length)]));
		}
		if (Random() < 20)
		{
			Do(Timeout(2000, KeepDistance(10000, true)));
		}
		else
		{
			Do(Timeout(4000, Attack(3)));
		}
	}

	private IEnumerable OnKnockDown()
	{
		if (Random() < 90)
		{
			Do(Say(koboldBanditOnKnockDown[Random(koboldBanditOnKnockDown.Length)]));
		}
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
			{
				Do(Say(koboldBanditAttack[Random(koboldBanditAttack.Length)]));
			}
		}
		else
		{
			Do(Timeout(500, PrepareSkill(SkillId.Defense)));
			Do(CancelSkill());
			Do(Attack(3, 5000));
			if (Random() < 90)
			{
				Do(Say(koboldBanditAttack[Random(koboldBanditAttack.Length)]));
			}
		}
	}
}