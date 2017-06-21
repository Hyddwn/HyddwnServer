//--- Aura Script -----------------------------------------------------------
// Emain Macha Richman AI
//--- Description -----------------------------------------------------------
// Ai for Emain Macha Richman, wanders and drops gold randomly
//---------------------------------------------------------------------------

[AiScript("npc_richman")]
public class NpcRichManAI : AiScript
{
	public NpcRichManAI()
	{
		SetHeartbeat(500);
		SetMaxDistanceFromSpawn(500);
	}

	protected override IEnumerable Idle()
	{
		if(Random() < 10)
			Do(SayRandomPhrase());
		if (Random() < 50)
		{
			Do(Wander());
		}
		else
		{
			Do(Wait(2000, 6000));
		}

		if (Random() < 30)
		{
			SwitchRandom();
			if (Case(1))
			{
				Do(DropGold(7));
			}
			else if (Case(30))
			{
				Do(DropGold(1));
			}
			else if (Case(30))
			{
				Do(DropGold(2));
			}
			else if (Case(9))
			{
				Do(DropGold(9));
			}
			else if (Case(30))
			{
				Do(Wait(100, 1000));
			}
		}
		else
		{
			Do(Wait(1000, 3000));
		}
	}
}
