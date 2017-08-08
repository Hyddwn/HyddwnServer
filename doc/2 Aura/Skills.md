The skill system is one of the most complicated parts of Aura, and it's always been kind of in flux, as we adjust it to new requirements and automate tasks.

##Introduction

Almost every little action you take in Mabinogi is implemented as a skill, be it attacking, chopping wood, or using Continent Warp. Each of those skills has multiple phases, called "Prepare", "Ready", "Use", "Complete", and "Cancel" in Aura. (The names are slightly different in the client files.) Additionally, there are toggleable skills, like Rest and Mana Shield, that have only 2 phases, "Start" and "Stop".

The phases reflect the packet sequence. When implementing a skill you have to pay great attention to the packet logs and implement the skills accordingly.

The first phase you enter when using a normal skill like, say, Windmill, is "Prepare". This notifies the server that you'd like to load this skill, and the server's answer usually contains the cast (or load) time. After that amount of time, the client will send the request for the next phase, "Ready". Being in the ready phase means that the skill is full loaded and ready to be used, and is portrayed by the skill bubble's animation stopping.

Once you do whatever the skill wants you to do to use it, maybe clicking the ground or an enemy, the server will receive the next request, "Use". That packet is quickly followed by "Complete", which is usually sent as soon as the client is notified about the skill having been used.

Alternatively, you can cancel the skill at any point before "Use". Doing so will get you into the Cancel phase, where you can clean up what the skill did. For example, removing effects from the character, removing temporary props, etc.

Toggleable skills are considerably easier, Start is sent when activating them, Stop on deactivation.

##Phase skipping

A special case are skills that skip phases. This is usually done for simple skills, or ones that don't have any kind of target that needs selection from the player. For example, WebSpinning, the skill used by spiders to drop cobwebs, goes straight from Prepare to Used to Complete. There's no loading involved, they use the skill, the web is dropped instantly, and that's it.

Similarly, Continent Warp skips the loading part and goes straight to being Ready. In the Ready phase you select the destination, followed by Use and then Complete, where you are finally warped.

You have to study the packet logs to determine whether a skill does this or not. Pay special attention to the response sent to SkillPrepare and act accordingly, using the mentioned skills as examples.

##Combat skills

There are two ways a skill can be used, the "SkillUse" and the "CombatAttack" packet. Skills that use CombatAttack we commonly call combat skills, while others are "normal" skills. The difference is that combat skills go through the combat attack packet handler.

For example, Smash is loaded like any other skill, Prepare and Ready, but to use it, you don't do anything special, you just run into the enemy head on, like you would when attacking it normally. The skill isn't used explicitly, you're just attacking the enemy. The server will then pick up that you have Smash loaded and will act accordingly, calling the Smash handler, instead of the CombatMastery handler (which is used for normal attacks). Most skills that target a single enemy are combat skills.

In Aura, combat skills differ from normal skills by the interfaces their handler classes implement.

##Implementation

The class definition of a typical combat skill looks something like this:

```
public class Smash : IPreparable, IReadyable, ICompletable, ICancelable, ICombatSkill
```

whereas normal skills switch `ICombatSkill` for `IUseable`.

```
public class Windmill : IPreparable, IReadyable, IUseable, ICompletable, ICancelable
```

You have to use the correct interfaces to implement the skills, or you will get an error, saying that the handler or the interface has not been implemented.

As I mentioned in the introduction, model the skill handler after the packet log. You see SkillPrepare being sent, add the IPreparable handler, you see SkillReady, add IReadyable. If Ready is followed by CombatAttack, you add ICombatSkill, is it followed by SkillUse, you add IUseable.

If you don't see SkillReady, being sent to the server, but by the server, as response to SkillPrepare, it means loading the skill was skipped, and it went straight to the Ready phase. If the response is SkillUse, it skipped the entire Ready and Use phase and went straight to Complete. Don't implement phases you don't need, and send the correct responses.

The two usage interfaces differ in the initial information they get and in the return type. SkillUse doesn't give you anything, but doesn't need anything from you either. You get the packet that was sent to the server, in case there are additional information you need for the skill (e.g. the target area id in Windmill), and the method's return type is void, so you don't return anything.

With CombatAttack, since you usually target an enemy to use those skills, you get the target's entity id instead of the packet, because the packet is always the same. The combat attack packet handler automatically handles error and out of range responses for you, to use those you return the appropriate value. For example, in Icebolt, when you determine that the given target is out of range, after you factored in the weapon used by the attacker, you `return CombatSkillResult.OutOfRange`. This makes the combat attack packet handler send an out of range response, which in turn will make the client move the character closer to the target.

Prepare and Ready are a mix of giving you nothing, but offering you limited automation. You get the packet sent to the server, in case of additional, skill specific parameters, and you can return a bool. If you return true, the skill was prepared/readied successfully. In that case a few information are saved for internal usage, like the new phase the skill is now in, and the time it's done loading. If you return false, a cancellation packet is sent to the client, informing it that something went wrong and the skill could not be prepared/readied. However, there are no other automated responses, unlike using combat skills.

Skills require you to send the appropriate response packets, since many skills have specific packet parameters, that can't be generalized. Unless you return `false` from Ready or Prepare, or something other than Okay from combat skill's Use, you have to tell the client what's going on yourself. Usually this means sending the same Packet that came in as response. SkillPrepare will usually get a packet with the SkillPrepare op as response, SkillReady will get SkillReady, etc.

The only phase that doesn't need this response packet sent by you is Cancel. Since cancellation is always followed by the same cancel packet, this is done automatically for you. All you have to do in cancel is clean up what the skill might have done, effects, props, motions, etc.

##Combat Actions

Combat Actions are used to communicate to the clients what happened during an attack. How you hit the target, what skills you used, whether it was a crit, or if the target used Defense and blocked your attack, all this is put into combat action objects, that are bundled in a "CombatActionPack" (cap).

These actions, again, reflect the packet logs, although with these it's harder to see, since combat actions use nested packets. The "Packet Analyzer" in MabiPale can help you with that. You should also study existing skill handlers and their packets, to learn more about how they work.

In theory it's simple, for a normal attack from an attacker on a target, you create an AttackerAction and a TargetAction, which are then both added to a CombatActionPack. On those actions you set options and properties, depending on what is happening. If you calculate that a crit should happen, you set the crit option, did the target die during the attack, you set the finished option, etc. For initial options and action types you have to look at the values used in the official packets.

Once you are done preparing the combat action pack with the necessary information, you call `Handle` on it, to do everything that is necessary to execute these actions. It sets the creature's stuns to the values you provided, sends stat updates for Life, cancels active skills on the target, etc. Everything that can be automated in terms of combat action handling will happen in that method, which is why it can be helpful to study it.

##Last words

As said in the beginning, the skill handling is still in flux, and you will have to dig into the existing code to learn more about what you have to send where and when. We have implemented many skills already though, and studying them, side-by-side with their packet logs, should tell you a great deal about their implementation in Aura, and how you can implement similar skills.