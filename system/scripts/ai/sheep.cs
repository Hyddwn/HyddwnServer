//--- Aura Script -----------------------------------------------------------
// Sheep AI
//--- Description -----------------------------------------------------------
// AI for sheeps.
//---------------------------------------------------------------------------

[AiScript("sheep")]
public class SheepAi : AiScript
{
	protected override IEnumerable Idle()
	{
		Do(Wander());
		Do(Wait(10000, 14000));
	}
	
	protected override IEnumerable Aggro()
	{
		Do(Attack(3));
		Do(Wait(3000, 8000));
	}
}
