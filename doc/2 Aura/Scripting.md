_Under construction_

Our language of choice for scripts in Aura is C#, which gives us a lot of freedom in what we can do with scripts, it being the same language we're using for the core. There's almost nothing you can't do from a script, as long as it doesn't involve modifying existing core classes. You'll also learn to understand and write core code as you're writing scripts, and vice versa.

When a server starts, all C#-files in its script list are compiled and initialized. You can picture every C# script as a small DLL, that is compiled and loaded when the server is started. While you can put anything in those files, only classes implementing `IScript` are initialized, by calling their `Init` method. Since Aura's script manager initializes every class separately, multiple script classes can be put into one file.

Aura provides several base script classes that implement `IScript`, for different purposes, with most of them inheriting from `GeneralScript`, a base class that implements `IScript` and provides various general use methods.

## Base classes

- [IScript](IScript)
- [GeneralScript](GeneralScript)
- [NpcScript](NpcScript)
- ~~[QuestScript](QuestScript)~~
- ~~[NpcShopScript](NpcShopScript)~~
- ~~[RegionScript](RegionScript)~~
- ~~[ItemScript](ItemScript)~~
- ~~[AiScript](AiScript)~~
- [DungeonScript](DungeonScript)
- ~~[PuzzleScript](PuzzleScript)~~

## Reloading

While not recommended on a production server, all scripts can be reloaded at once with the GM command `reloadscripts`. This will remove all NPCs and monsters from the server, dispose all script classes that implement `IDispose`, remove all creature spawners, and finally load every file in the server's script list again.

## Script list

The script lists are files in the scripts folders, that specify which files should be loaded when the servers start. For the channel server, it's "scripts.txt", for login it's "scripts_login.txt", and for the web server it's "scripts_web.txt", although login and web currently don't have any scripts.

For more information on how the script list works, and how to override files in the system folder from the user folder, see the [User folder page](https://github.com/aura-project/aura/wiki/User-folder), and more specifically, the section about [scripts](https://github.com/aura-project/aura/wiki/User-folder#scripts).

## Scripting variables

While not limited to scripting, the "scripting variables"-system was specifically added to be able to save information across script and play-sessions. Every creature and account has a list of temporary and permanent variables attached to it, which can be accessed via their `Vars` property.

```csharp
creature.Vars.Temp["variable_name"] = 5;
account.Vars.Perm["variable_name"] = "foobar";
```

As the variable names are simple strings, there are no restrictions on them. However, you should stick to certain naming conventions, to reduce the risk of conflicts. A name like "skill" or "player" might be used by many scripts. If you were writing a warper for example, you could name a variable something like "MyWarper_LastDestination", differentiating it from other variables in other scripts that might be named "LastDestination", making sure that the two scripts don't mess with each other.

There are global variables as well, that aren't attached to a creature or account, which can be accessed via `ChannelServer.Instance.ScriptManager.GlobalVars` in the same manner. Inside scripts inheriting from `GeneralScript` there's a shortcut to it, a property called `GlobalVars`.

```csharp
// Get the name of the player that broke the Dugald Seal Stone.
var breakerName = GlobalVars.Perm["SealStoneName_sealstone_dugald"];
if (breakerName == null)
{
    // Seal Stone hasn't been broken yet
}
```

Should a variable not exist, the value returned will be `null`, no matter the type of the actual value.

While values saved in `Temp` are discarded on logout/server shutdown, `Perm` values will be saved to the database. You can technically save almost anything in these variables, but you should only use them if you have to. If you can solve an issue without them, do so, especially if the value is very long, or performance is important.

The values returned from `Vars` are `dynamic` ([MSDN](https://msdn.microsoft.com/en-us/library/dd264741.aspx)), which simplifies their usage a little, as you don't have to worry about types as much.