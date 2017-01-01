//--- Aura Script -----------------------------------------------------------
// Rebirth Event
//--- Description -----------------------------------------------------------
// Players get rebirth potions for spending a certain amount of time in-game.
// The potion can be used to reset ones rebirth time, so they're able to
// rebirth again ahead of time.
// 
// Reference: http://mabinogi.nexon.net/News/Announcements/1/00J1F
//---------------------------------------------------------------------------

public class NewRebirthEventScript : GameEventScript
{
	private readonly TimeSpan PlayTimeNeeded = TimeSpan.FromHours(36);

	private const string StartPlayTimeVar = "NewRebirthEvent_StartPlayTime";
	public const int RebirthPotion = 86040;

	public override void Load()
	{
		SetId("new_rebirth_event");
		SetName(L("Rebirth"));

		AddPacketHandler(Op.RebirthEventInfoRequest, RebirthEventInfoRequest);
		AddPacketHandler(Op.RebirthEventReceivePotion, RebirthEventReceivePotion);
	}

	public override void AfterLoad()
	{
		//ScheduleEvent(DateTime.Parse("2016-12-01 00:00"), DateTime.Parse("2016-12-29 00:00"));
	}

	[On("CreatureConnecting")]
	private void OnCreatureConnecting(Creature creature)
	{
		if (!IsEventActive("new_rebirth_event") || !creature.IsCharacter)
		{
			// Remove any event Rebirth Potions the creature might have
			if (creature.Inventory.Has(RebirthPotion))
				creature.Inventory.Remove(RebirthPotion, 1000);
			return;
		}

		var start = creature.Vars.Perm[StartPlayTimeVar];
		if (start == null)
			ResetStart(creature);
	}

	private void ResetStart(Creature creature)
	{
		creature.Vars.Perm[StartPlayTimeVar] = creature.PlayTime;
	}

	private void RebirthEventInfoRequest(ChannelClient client, Packet packet)
	{
		var entityId = packet.GetLong();
		var creature = client.GetCreatureSafe(packet.Id);

		if (!IsEventActive("new_rebirth_event"))
		{
			Send.MsgBox(creature, L("The event is over."));
			return;
		}

		if (creature.Vars.Perm[StartPlayTimeVar] == null)
			return;

		SendRebirthEventInfo(creature);
	}

	private void SendRebirthEventInfo(Creature creature)
	{
		var duration = PlayTimeNeeded;
		var startPlayTime = (long)creature.Vars.Perm[StartPlayTimeVar];
		var pastPlayTime = TimeSpan.FromSeconds(creature.PlayTime - startPlayTime);

		var packet = new Packet(Op.RebirthEventInfo, creature.EntityId);
		packet.PutByte(true);
		packet.PutLong(duration);
		packet.PutLong(pastPlayTime);
		packet.PutString(L("(Receive a Rebirth Potion at 0.00%.)"));
		packet.PutString(L("Receive Potion"));

		creature.Client.Send(packet);
	}

	private void RebirthEventReceivePotion(ChannelClient client, Packet packet)
	{
		var entityId = packet.GetLong();
		var creature = client.GetCreatureSafe(packet.Id);

		if (!IsEventActive("new_rebirth_event"))
		{
			Send.MsgBox(creature, L("The event is over."));
			return;
		}

		var duration = PlayTimeNeeded;
		var startPlayTime = (long)creature.Vars.Perm[StartPlayTimeVar];
		var pastPlayTime = TimeSpan.FromSeconds(creature.PlayTime - startPlayTime);
		if (pastPlayTime < duration)
		{
			Send.MsgBox(creature, L("The time isn't over yet."));
			return;
		}

		creature.AcquireItem(Item.Create(RebirthPotion));

		ResetStart(creature);
		SendRebirthEventInfo(creature);
	}
}
