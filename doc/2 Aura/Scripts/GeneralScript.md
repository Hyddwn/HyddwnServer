`GeneralScript` is a base class with various general use methods, like `Random` and `Spawn`. As these methods are still in constant flux, it's best to take a look at the [source code](https://github.com/aura-project/aura/blob/master/src/ChannelServer/Scripting/Scripts/GeneralScript.cs#L132), to learn what methods there are, and what they do. Alternatively, you can look at the [source docs](http://aura-project.org/doc/class_aura_1_1_channel_1_1_scripting_1_1_scripts_1_1_general_script.html), those aren't guaranteed to be up-to-date though.

`GeneralScript` is a good base class for scripts that don't have a special purpose, providing helpful methods, that can be useful in a variety of ways. However, all of this is convenience only, as you can do anything `GeneralScript` does, yourself as well.

## Events

A special feature of `GeneralScript` is its auto event subscription. Aura has many events for various situations, that anybody can subscribe to, to be informed when certain things happen. While you can do this from any script, all of them being the same old C#, `GeneralScript` makes it especially easy, as this example from Aura's Tarlach NPC script shows:

```csharp
// Subscribe to the "ErinnDaytimeTick" event, which is raised
// at 6:00 and 18:00 Erinn time.
[On("ErinnDaytimeTick")]
public void OnErinnDaytimeTick(ErinnTime time)
{
	// Warp Tarlach to Sidhe if it's night and to an
	// inaccessible region during the day.
	if (time.IsNight)
		NPC.WarpFlash(48, 11100, 30400);
	else
		NPC.WarpFlash(22, 5800, 7100);
}

```

The special `On` [attribute](https://msdn.microsoft.com/en-us/library/z0w1kczw.aspx) is used to look for subscriber methods, with the string argument being the name of the event. The above `On` attribute is equivalent to this code:

```csharp
ChannelServer.Instance.EventManager.ErinnDaytimeTick += OnErinnDaytimeTick;
```

Subscriptions that are set up with the `On` attribute are automatically removed when the script is reloaded though.

Please refer to the [source](https://github.com/aura-project/aura/blob/master/src/ChannelServer/World/EventManager.cs) or the [docs](http://aura-project.org/doc/class_aura_1_1_channel_1_1_world_1_1_event_manager.html) for more information about what kind of events there are.