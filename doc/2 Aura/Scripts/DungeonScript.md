Dungeon scripts are part of the dungeon system and are used to describe what's supposed to happen on certain events inside a dungeon, while the dungeon db holds information about the structure of the dungeon, and [PuzzleScripts](PuzzleScript) control what happens inside the dungeon's rooms.

## Events

- [Route](#route)
- [OnCreation](#oncreation)
- [OnBoss](#onboss)
- [OnBossDeath](#onbossdeath)
- [OnCleared](#oncleared)
- [OnLeftEarly](#onleftearly)
- [OnPlayerEntered](#onplayerentered)
- [OnPartyEntered](#onpartyentered)
- [OnSectionCleared](#onsectioncleared)

Each of those methods can be overridden and allow a certain amount of control over what is supposed to happen in a dungeon, particularly when the boss door is opened (`OnBoss`) and when the boss room is cleared (`OnCleared`).

It's recommended to study existing dungeons to see how the events are generally used. Also take a look at the [source](https://github.com/aura-project/aura/blob/master/src/ChannelServer/Scripting/Scripts/DungeonScript.cs), to see which parameters the events get.

### Route

Route is a method used when dropping an item on a dungeon's altar. Certain scripts have the absolute control over which type of dungeon you will enter. For example, Alby Normal if you drop a normal item, or Alby Basic if you drop a Basic pass. Which script has control over the route depends on the dungeon, but generally it's the script that also contains the normal version of it. In the case of Alby it's "tircho_alby_dungeon.cs".

```csharp
public override bool Route(Creature creature, Item item, ref string dungeonName)
{
	// Alby Basic
	if (item.Info.Id == 63101) // Alby Basic Fomor Pass
	{
		dungeonName = "tircho_alby_low_dungeon";
		return true;
	}

	// Alby Int 1
	if (item.Info.Id == 63116) // Alby Intermediate Fomor Pass for One
	{
		if (creature.Party.MemberCount == 1)
		{
			dungeonName = "tircho_alby_middle_1_dungeon";
			return true;
		}
		else
		{
			Send.Notice(creature, L("You can only enter this dungeon alone."));
			return false;
		}
	}
```

The method receives a reference to the creature that dropped the item, to the item that was dropped, and a string variable that the method has to set to specify which dungeon to finally enter. As you can see in the example, the method is also responsible for making sure that you are actually allowed to enter the dungeon. For example, Alby Int 1 can only be entered alone, if you're not alone you will get a notice and the method will return false, telling the dungeon manager not to drop the creature into the dungeon.

The value of `dungeonName` must be equal to an existing dungeon in the db, and to the respective dungeon script file.

### OnCreation

`OnCreation` is currently not used by any dungeons, but it's called right after the dungeon has been fully created and it would allow for things to be added to the dungeon before the first player arrives. For example, monsters could be spawned in the corridors.

### OnBoss

This event is called when the boss door is opened and should be used to tell the dungeon which bosses to spawn.

```csharp
public override void OnBoss(Dungeon dungeon)
{
	dungeon.AddBoss(130001, 1); // Golem
	dungeon.AddBoss(11003, 6); // Metal Skeleton

	dungeon.PlayCutscene("bossroom_Metalskeleton_Golem");
}
```

This event is used in a special way by the Rabbie dungeon, spawning a specific boss based on the amount of players in the dungeon.

### OnBossDeath

While not used by most dungeons, `OnBossDeath` is called whenever a boss creature dies, allowing for something special to happen, like getting a quest item (Malcolm's Ring) or spawning another wave of bosses (Rabbie).

### OnCleared

Clearing a dungeon means that all bosses have died. At this point all players should generally receive a key to a treasure chest, which also have to be added to the dungeon by this method. For an [example](https://github.com/aura-project/aura/blob/master/system/scripts/dungeons/tircho_alby_dungeon.cs#L115), look at pretty much [any dungeon script](https://github.com/aura-project/aura/tree/master/system/scripts/dungeons).

### OnLeftEarly

Another event that is currently rarely used is `OnLeftEarly`, which is called when you leave a dungeon without clearing it, by logging out, leaving through the entrance statue, etc. This event is generally used to give back special quest passes that were dropped to enter the dungeon. This way you don't lose them and you can get right back in if you didn't clear the dungeon for whatever reason.

### OnPlayerEntered

Called whenever a player enters the dungeon's lobby (the room with the entrance statue). Internally this event is used to send the scroll messages, telling you who created the dungeon.

### OnPartyEntered

Called once `OnPlayerEntered` was called for all party members that stood on the altar when the party entered the dungeon. This is called even if you're not actually in a party.

### OnSectionCleared

Dungeon floors are separated into sections internally, as defined by the dungeon db. Each section usually has a random amount of "puzzles" (rooms or events) and once all puzzles of one section have been solved, this method is called, with the floor and the section number, to handle special events that are supposed to happen during the run.

For example, a cutscene might be supposed to play after clearing a certain section on  a certain floor.