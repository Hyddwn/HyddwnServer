//--- Aura Script -----------------------------------------------------------
// Imp AI
//--- Description -----------------------------------------------------------
// AI for Imps.
//--- Missing ---------------------------------------------------------------
// Magic Charges and Magic Attack
// Do(Wait(1000, 2000)); Do(Attack(1, 4000)); are not official.
// Without the wait the AI would be WAY to fast.
//---------------------------------------------------------------------------

[AiScript("imp")]
public class ImpAi : AiScript
{
	readonly string[] ImpIdle = new[]
	{
		L("Booyah!"),
		L("Ha ha"),
		L("Hahaha"),
		L("Growl.."),
		L("What?"),
	};

	readonly string[] ImpAlert = new[]
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

	readonly string[] ImpAttack = new[]
	{
		L("Attack!"),
		L("Here I come!"),
		L("Fool."),
		L("Ha ha"),
		L("Hahaha"),
		"",
		"",
		"",
	};

	readonly string[] ImpDefense = new[]
	{
		L("Do you know how to use the Smash skill?"),
		L("Do you know how to use magic?"),
		L("Let's see what you've got!"),
		"",
		"",
	};

	readonly string[] ImpCounter = new[]
	{
		L("Do you know how to use the Smash skill?"),
		L("Do you know how to attack?"),
		L("Let's see what you've got!"),
		"",
		"",
	};

	readonly string[] ImpChargeLB1 = new[]
	{
		L("There is something under the keyboard!"),
		L("You like that?"),
		L("Please, I need guidance."),
		L("Do you know how to use this?"),
		L("Please wait."),
		L("Hey, wait."),
		L("Ran out of cash..."),
		"",
		"",
		"",
		"",
	};

	readonly string[] ImpChargeLB2 = new[] 
	{
		L("What is your IP?"),
		"",
		"",
	};

	readonly string[] ImpSmash = new[] 
	{
		L("Imp Smash!"),
		L("Here comes a Smash!"),
		L("Why don't you start over again."),
		L("Can I really use the Smash skill?"),
		"",
		"",
	};

	readonly string[] ImpOnHit = new[] 
	{
		L("Umph..."),
		L("Mmph!"),
		L("Gah!"),
		"",
		"",
	};

	readonly string[] ImpOnKnockDown = new[] 
	{
		L("Ouch!"),
		L("Are you a noob?"),
		L("It hurts."),
		"",
		"",
	};

	public ImpAi()
	{
		SetVisualField(950, 120);
		SetAggroRadius(600);
		SetAggroLimit(AggroLimit.Two);

		Hates("/pc/", "/pet/");
		HatesNearby(2000);

		On(AiState.Aggro, AiEvent.DefenseHit, OnDefenseHit);
		On(AiState.Aggro, AiEvent.Hit, OnHit);
		On(AiState.Aggro, AiEvent.KnockDown, OnKnockDown);
	}

	protected override IEnumerable Idle()
	{
		yield break;
	}

	protected override IEnumerable Alert()
	{
		yield break;
	}

	protected override IEnumerable Aggro()
	{
		yield break;
	}

	private IEnumerable OnDefenseHit()
	{
		yield break;
	}

	private IEnumerable OnHit()
	{
		yield break;
	}

	private IEnumerable OnKnockDown()
	{
		yield break;
	}
}