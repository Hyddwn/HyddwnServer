* [When will Aura be done?](#when-will-aura-be-done)
* [How can I play on the Aura server?](#how-can-i-play-on-the-aura-server)
* [How can I contribute to Aura?](#how-can-i-contribute-to-aura)
* [I get errors after updating Aura!](#i-get-errors-after-updating-aura)
* [What's the purpose of branches in the source repository?](#whats-the-purpose-of-branches-in-the-source-repository)
* [Where does the name "Aura" come from?](#where-does-the-name-aura-come-from)
* [Will you implement feature XY?](#will-you-implement-feature-xy)
* [Who should I contact if I have questions about Aura?](#who-should-i-contact-if-i-have-questions-about-aura)
* [Why is XY not working?](#why-is-xy-not-working)
* [Why can't I move / am I naked/headless / do I crash after login?](#why-cant-i-move--am-i-nakedheadless--do-i-crash-after-login)
* [Why aren't my hotkeys being saved?](#why-arent-my-hotkeys-being-saved)
* [How can I enable the old/new combat system?](#how-can-i-enable-the-oldnew-combat-system)

##When will Aura be done?

"Done" is a very broad and usually subjective word. If by "done" you mean "in a state where its features are comparable to Generation X in the official game", we'd like to refer you to our [backlog](https://github.com/aura-project/aura/issues?utf8=%E2%9C%93&q=is%3Aissue+is%3Aopen+label%3Abacklog), for a list of missing features for Generation X, and [milestones](https://github.com/aura-project/aura/milestones), for a general overview of where the project stands.

Unfortunately we can't give any accurate estimates on when any given feature will be implemented. We are also implementing the generation updates in sequence, so don't expect too many G18 features, while we're maybe still working on G3.

##How can I play on the Aura server?

Aura itself is not a server, it's a [server emulator](http://en.wikipedia.org/wiki/Server_emulator), it's not a service you can use, but a software that imitates behavior of another server software. We aren't operating any private servers and advise you to not do so either.

##How can I contribute to Aura?

Please take a look at the [Contribution section](http://github.com/aura-project/aura#contribution) of Aura's README file.

##I get errors after updating Aura!

Most errors you can encounter after updating Aura can be solved by *recompiling* and *deleting the Cache folder*. You should do so after every update, unless you know for a fact that nothing related changed. If that didn't help, open an issue on [GitHub](https://github.com/aura-project/aura/issues).

##What's the purpose of branches in the source repository?

Branches contain mostly experimental and unfinished features, they are merged into master once they're ready to be used. Unless you plan to test or work on the features in the branches, we recommend normal users stick to master, where things are kept clean and compilable.

##Where does the name "Aura" come from?

In Greek mythology, Aura is the "Titan of the breeze and the fresh, cool air of early morning".

##Will you implement feature XY?

Our first goal is to make Aura work like the official servers as much as possible. On the way we're gonna add options to customize a few things (e.g. "bagception", an option to store bags inside bags, or classic combat vs dynamic combat), but for the most part we'll stick to official features, which we will implement in sequence. Though we're open for suggestions, you can post your ideas on the forum, and if we don't implement it, maybe another user will.

##Who should I contact if I have questions about Aura?

That depends on your question, but generally you should create a thread on our [forum](http://aura-project.org/forum/) if there isn't one for your question already. Not only will you often times get an answer sooner, because the core developers aren't available 24/7, but when others have the same question later on, they can find an answer. Another option is the the [chat](https://gitter.im/aura-project/aura).

##Why is XY not working?

Aura is work-in-progress, not everything will work just yet, in fact, you should expect most things *not* to work. We are (generally) implementing features in sequence, so you won't be able to use a G8 feature when we're still implementing G1. To get a better idea of where the project stands, check our [backlog](https://github.com/aura-project/aura/issues?utf8=%E2%9C%93&q=is%3Aissue+is%3Aopen+label%3Abacklog) and [milestones](https://github.com/aura-project/aura/milestones).

##Why can't I move / am I naked/headless / do I crash after login?

This usually happens when the client you're using is not compatible to Aura. Aura is currently only ever compatible to the latest version of NA, so you can't connect to an Aura server with any other clients, like KR or TW. If you're using the latest version of NA and Aura and still experience problems, we probably haven't updated Aura for a recent patch yet. Give us a few hours, usually we update Aura on the same day the patch came out.

##Why aren't my hotkeys being saved?

Mabinogi's UI settings are stored on the web server, but for this to work you have to modify your client, so it stores them on your Aura web server, instead of trying to store them on NA's. You can find more information on how to do this on the [Web Server](https://github.com/aura-project/aura/wiki/Web-server) page.

##How can I enable the old/new combat system?

Since this is something that changed over the course of Mabinogi's generation updates, you can find the option(s) in the [feature settings](https://github.com/aura-project/aura/wiki/Feature-Settings). Specifically, the features you want to change are "CombatSystemRenewal", which marked the introduction of the new combat system, and "TalentRenovationCloseCombat", "TalentRenovationMagic", and"TalentRenovationArchery", which changed a few more things in G19.