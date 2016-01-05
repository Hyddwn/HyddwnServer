//--- Aura Script -----------------------------------------------------------
// Banner Imp AI
//--- Description -----------------------------------------------------------
// AI for Banner Imp. Very similiar to the Imp AI, but Banner Imps
// usually don't attack.
//---------------------------------------------------------------------------

[AiScript("bannerimp")]
public class BannerImpAi : AiScript
{
	protected readonly string[] ChatIdle = new[]
	{
		L("Booyah!"),
		L("Ha ha"),
		L("Hahaha"),
		L("Growl.."),
		L("What?"),
	};

	protected readonly string[] ChatAlert = new[]
	{
		L("Why have you come?"),
		L("What's your business..."),
		L("Are you human?"),
		L("What are you?"),
		L("Set your items."),
		L("Go get lost."),
		L("My mouse got dirty."),
		L("Rubbish..."),
		"",
		"",
		"",
		"",
	};

	protected readonly string[] ChatAggro = new[]
	{
		L("..."),
		"",
		"",
		"",
	};

	protected readonly string[] ChatOnHit = new[] 
	{
		L("Umph..."),
		L("Mmph!"),
		L("Gah!"),
		"",
		"",
	};

	protected readonly string[] ChatOnKnockDown = new[] 
	{
		L("Ouch!"),
		L("Are you a noob?"),
		L("It hurts."),
		"",
		"",
	};

	public BannerImpAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(600);
		SetAggroLimit(AggroLimit.Two);

		Doubts("/pc/", "/pet/");

		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
	}

	protected override IEnumerable Idle()
	{
		var num = Random();
		if (num < 30) // 30%
		{
			Do(Say(ChatIdle));
		}
		else if (num < 40) // 10%
		{
			Do(Wander(100, 500));
		}
		else if (num < 60) // 20%
		{
			Do(Wander(100, 500, false));
		}
		else if (num < 80) // 20%
		{
			Do(Wait(4000, 6000));
		}
		else if (num < 90) // 10%
		{
			Do(PrepareSkill(SkillId.Lightningbolt));
		}
		Do(Wait(2000, 5000));
	}

	protected override IEnumerable Alert()
	{
		Do(CancelSkill());

		var num = Random();
		if (num < 20) // 20%
		{
			Do(Say(ChatAlert));
			Do(Wait(1000, 2000));
		}
		else // 80%
		{
			Do(Wait(1000, 4000));
			Do(Say(ChatAlert));
			Do(Circle(600, 1000, 2000));
		}

		Do(Wait(2000, 10000));
	}

	protected override IEnumerable Aggro()
	{
		Do(Say(ChatAggro));
		Do(KeepDistance(Rnd(500, 700, 1000), true, Rnd(2000, 3000, 5000)));
	}

	private IEnumerable OnHit()
	{
		Do(Say(ChatOnHit));
		Do(KeepDistance(Rnd(1000, 7000), true, Rnd(2000, 3000)));
	}

	private IEnumerable OnKnockDown()
	{
		Do(Say(ChatOnKnockDown));
		if (Random() < 50)
		{
			Do(PrepareSkill(SkillId.Defense));

			var num = Random();
			if (num < 60) // 60%
			{
				Do(Say(ChatOnKnockDown));
				Do(Circle(500, 2000, 2000));
			}
			else // 40%
			{
				Do(KeepDistance(1000, true, 4000));
			}

			Do(CancelSkill());
		}
	}

	private IEnumerable OnDefenseHit()
	{
		Do(Say(ChatAlert));
		Do(Attack(3, 4000));

		if (Random() < 50) // 50%
		{
			Do(Circle(500, 2000, 2000, false));
		}
		else // 50%
		{
			Do(KeepDistance(1000, false, 4000));
			Do(Wait(1000, 2000));
		}
	}
}
