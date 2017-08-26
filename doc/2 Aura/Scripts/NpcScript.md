_Under construction_

Traditional NPCs, with or without dialog, typically inherit from `NpcScript`, which provides methods to control dialog and the setup of the NPC. `NpcScript` in turn inherits from [`GeneralScript`](GeneralScript), giving it the same helper methods and useful extras, like auto event subscription.

## Setup

The NPC can be customized in the `Load` method, with a race id, a name, equipment, etc. The only important rule here is to set the race id first, as the NPC object will be created once you do so, which is needed to set the other values. Take a look at the [source code](https://github.com/aura-project/aura/blob/master/src/ChannelServer/Scripting/Scripts/NpcScript.cs#L581) to see all Setup methods, and the [existing NPCs](https://github.com/aura-project/aura/tree/master/system/scripts/npcs) for examples.

```csharp
public override void Load()
{
	SetRace(10002); // Set race and initiate NPC
	SetName("_duncan"); // Set name (underscore names are localized by the client)
	SetLocation(1, 15409, 38310, 122); // Set location and direction

	EquipItem(Pocket.Face, 4950, 0x93005C); // Equip face item
	EquipItem(Pocket.Hair, 4083, 0xBAAD9A); // Equip hair item
	EquipItem(Pocket.Armor, 15004, 0x5E3E48, 0xD4975C, 0x3D3645); // Equip clothes
	EquipItem(Pocket.Shoe, 17021, 0xCBBBAD); // Equip shoes
}
```

## Dialog Flow

NPC dialogues are basically controlled by 3 methods, `Msg`, `Select`, and `Close`. It's important to understand how they work, to accurately control the flow of dialogues.

A `Msg` call does not stop the script, multiple `Msg` are basically sent together, until a block occurs, typically in the form of a `Select`. If you send a single message to the client, it will show the message and an "End Conversation" button, because there's nothing else to do. Clicking that button will send a simple End packet.

```csharp
Msg("test1"); // Shows "test1" and "End Conversation" button.
```

If you send multiple messages after one another, the client will show the messages and continue buttons, until you reach the last message, but all messages are sent at the same time, so if you put other code in between, that code runs without delay. While the player still sees "test1", the server is already at the end of the block in the example below.

```csharp
Msg("test1"); // Shows "test1" and "Continue" button.
Msg("test2"); // Shows "test2" and "Continue" button.
Msg("test3"); // Shows "test3" and "End Conversation" button.
```

Select makes the script wait for a response, which is returned as a string. However, if there's nothing to select, i.e. no input requests, like buttons, were sent in the last message, it will show the "End Conversation" button. Unlike the one in `Msg` though, the dialog will not just end, because no End packet is sent. Instead, `Select` returns "@end", this "End Conversation" button being one that was automatically created to have *something* to select.

```csharp
Msg("test1<button title='Button 1' keyword='@foo' /><button title=''Button 2' keyword='@bar' />"); // Shows "test1" and two buttons
await Select(); // Select returns "@foo" or "@bar", depending on which button is clicked.

Msg("test1"); // Shows "test1" and "End Conversation" button.
await Select(); // Nothing to select, Select returns "@end".
```

Close will immediately close the dialog if no message is specified as first parameter. If a message is set, it will be displayed with an "End Conversation" button. Just like with multiple messages, `Close` is called right away, so if you don't use a closing message, and there's no `Select`, the code will run till the end, closing the dialog window before the player has a chance to read any messages.

```csharp
Msg("test1"); // Shows "test1" and "End Conversation" button.
Close(); // Closes the dialog instantly, with no chance for the player to read the previous message.

Msg("test1"); // Shows "test1" and "Continue" button.
Close("(You've ended the conversation.)"); // Shows message and "End Conversation" button, closing the dialog afterwards.
```

## Select Implementation

For its stateless NPCs Aura makes use of C#'s [asynchronous features](http://msdn.microsoft.com/en-us/library/hh191443.aspx). This allows you to 'await' the response to something from a client, like clicking buttons putting in text, selecting something from a list, etc. The script resumes as soon as that response came in, with `Select` returning the response from the client.

```csharp
Msg("How are you?<button title='Good' keyword='@good' /><button title='Bad' keyword='@bad' />");
var response = await Select();
if(response == "@good")
    Msg("=D");
else
    Msg("=(");
```

In this example we send a message and two buttons to the client, prompting the player to click one. On the next line we write the result of `Select` into a variable that we can check for a specific response afterwards. Note the `await`, which instructs the server to pause the script and wait for something. This keyword is vital, because without it the code would just continue.

If you leave out the keyword in the above example, the script doesn't pause on that line. It simply runs from top to bottom. The result would be that you get 3 buttons in your first message, "Good", "Bad", and "Continue". Because you sent multiple messages to the client at once, it assumes the next message is supposed to be displayed after a click on Continue, making it display the button. Furthermore you would always get the sad smiley, because the result of Select, without waiting for the actual response, is always an empty string.

The `await` keyword also has to be used for other methods, that *might* use `Select`. Methods that are supposed to act asynchronously like that have/need a specific return type, `async Task`. Built in methods that make use of this are `Intro`, `StartConversation`, and `Conversation`, which are using `Select` internally.

```csharp
protected override async Task Talk()
{
    Msg("How are you?<button title='Good' keyword='@good' /><button title='Bad' keyword='@bad' />");
    await ReactToResponse(await Select());
}
    
private async Task ReactToResponse(string response)
{
    if(response == "@good")
        Msg("=D");
    else
        Msg("=(");

    Msg("Press this button, maybe it will make you feel better!<button title='Some button' keyword='@foobar' />");
    await Select();
    Msg("=)");
}
```

## XML

Mabinogi uses XML code for its dialog, to display buttons, lists, images, etc. For a list of available codes, go ~~[here](XML Dialog)~~.
