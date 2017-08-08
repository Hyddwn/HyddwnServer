GM commands are a command system built into the normal in-game chat. Commands are prefixed by `>` by default, followed by the command name and usually a few parameters. Commands can also be used on others, by using the prefix twice and using the target's name after the command name. For example, while `>item 123` would spawn the item with the id 123 in *your* temp inventory, `>>item John 123` would spawn it in John's inventory, provided that he's online and on the same channel.

GM commands can technically be used by anyone from the in-game chat, as long as they have the required authority level. For example, to spawn items using the command `>item` you need an authority level of at least 50 by default. The required authority levels and the command prefix can be configured in the conf file `system/conf/commands.conf`, which serves as a kind of command list, that also includes short comments about what each command does.

The easiest way to change your authority level, to "turn you" into a GM, is to type the console command

```
auth accountname level
```

into the login server's window, replacing accoutname with your account name, level with your desired authority level, and pressing return to execute it. Make sure you're not logged into your account while executing the command. Level 99 will give you access to all commands Aura has to offer.

# User Commands
These are commands anyone can use. Auth level is set at 0. 
### where
* Description: Displays your character location.
* Arguments: None
* Example Usage: `>where` or `>>where username`
* Example Output: `Region: 14 @ 41999/36063, Area: 10, Dir: 208 (Radian: 5.125)`

### cp
* Description: Displays combat power.
* Arguments: None
* Example Usage: `>cp` or `>>cp username`
* Example Output: `Your combat power: 521.5`

### distance
* Description: Calculates distance between two positions.
* Instructions: Stand in one spot and use the command. Then, move to another location and use the command again.
* Arguments: None
* Example Usage: `>distance` or `>>distance username`
* Example Output: `Distance between '(Position: 39898, 37406)' and '(Position 39413, 37833)': 646`

### partysize
* Description: Changes party max size.
* Arguments: 1~8
* Example Usage `>partysize 5` or `>>partysize username 5`
* Example Output: `Changed party size to 5.`