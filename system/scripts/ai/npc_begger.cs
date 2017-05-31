//--- Aura Script -----------------------------------------------------------
// Emain Macha Beggar resting AI
//--- Description -----------------------------------------------------------
// Ai for the Beggar, to make him use rest
//---------------------------------------------------------------------------

[AiScript("npc_begger")]
public class NpcBeggerAI : AiScript
{
	public NpcBeggerAI()
	{
		SetHeartbeat(500);
	}

	protected override IEnumerable Idle()
	{
		Do(SayRandomPhrase());
		if (!HasSkill(SkillId.Rest))
			Do(AddSkill(SkillId.Rest, SkillRank.RF));
		Do(StartSkill(SkillId.Rest));
		Do(Wait(36000, 60000));
	}
}
