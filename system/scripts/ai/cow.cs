//--- Aura Script -----------------------------------------------------------
// Cow AI
//--- Description -----------------------------------------------------------
// AI for cows.
//---------------------------------------------------------------------------

[AiScript("cow")]
public class CowAi : AiScript
{
	public CowAi()
	{
		SetVisualField(2000, 90);
		SetAggroRadius(2000);

		//HatesAttacking("/cow/");
	}

	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(7000, 20000));
	}

	protected override IEnumerable Aggro()
	{
		Do(Attack(3));
		Do(Wait(3000));
	}
}
