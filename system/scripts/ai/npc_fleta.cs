//--- Aura Script -----------------------------------------------------------
// Fleta NPC AI
//--- Description -----------------------------------------------------------
// AI for the Fleta NPC. Wanders around frequently.
//---------------------------------------------------------------------------

[AiScript("npc_fleta")]
public class FletaNpcAi : AiScript
{
	public FletaNpcAi()
	{
		SetHeartbeat(500);
		SetMaxDistanceFromSpawn(1000);
	}

	protected override IEnumerable Idle()
	{
		if (Random() < 5)
			Do(SayRandomPhrase());
		if (Random() < 50)
		{
			Do(Timeout(2000, Wander(100, 100)));
			Do(Wait(1000, 5000));
		}
		else
		{
			Do(Timeout(2000, Wander(100, 1000)));
			Do(Wait(1000, 5000));
		}
	}
}
