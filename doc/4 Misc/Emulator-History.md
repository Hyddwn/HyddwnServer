There have been surprisingly few attempts to create emulators for Mabinogi that we know about. There are 8 "commonly" known ones, including Aura.

- [Previous Emulators](#previous-emulators)
  - [Wabinogi](#wabinogi)
  - [NOTMABI](#notmabi)
  - [Unnamed Korean Emulator](#unnamed-korean-emulator)
  - [Osiris](#osiris)
  - [Neamhain](#neamhain)
  - [ISNOGI](#isnogi)
  - [Yamase](#yamase)
- [Aura](#aura)
- [Chart](#chart)

##Previous Emulators

###Wabinogi
Started in 2009, Wabinogi was one of the first server emulators and was considered to be the most advanced of its time, though extremely simple. It is assumed that players were able to move around, spawn items, and use very few skills[<sup>ref</sup>](http://www.elitepvpers.com/forum/emulator-development/388839-recruit-mabinogi-online-emulation.html). The emulator was powered by C++, used LUA for its scripting engine, and MySQL to manage its database. Wabinogi and its creators vanished shortly after being unveiled.

###NOTMABI
Written in C++, this emulator was supposedly started by a single developer. Not much is known about NOTMABI, it can be assumed that it had little to no functionality.

###Unnamed Korean Emulator
Though not much is known about the project, several posts on Ragezone indicate the existence of an unnamed, C# powered emulator, written for Mabinogi KR. Unfortunately, all links leading to the emulator's source code are now dead.

###Osiris
Osiris was one of the first emulators to originate in Europe and was developed by Aki in C++[<sup>ref</sup>](http://osiris.akinova.de). Screen shots suggest that functionality was limited to basic creature spawning as well as some skills. There has not been any public indication of continued development since the original posts.

###Neamhain
Neamhain was started in early 2012 by exec after Mabinogi EU hadn't been updated or maintained in over a year. It was developed in D, MySQL, and LUA. The project was abandoned rather early as exec joined the ISNOGI team shortly after beginning the project. Functionality was limited, and comparable to Wabinogi.

###ISNOGI
April 2012: *ISNOGI is the most advanced Mabinogi private server in development. Its current incarnation was started in August 2011. I've worked on it off and on as a side project since then. -- K*

ISNOGI was a highly publicized attempt at emulating Mabinogi and received quite a bit of attention from the community. ISNOGI was a play off NOTMABI's name, and was considered the most advanced emulator of the time. Functionality, however, was quite limited.

ISNOGI was originally programmed by a single developer, with Yiting and exec joining shortly after its original publication. Development continued only a few weeks before branching into two separate projects, Yamase and an early form of Aura. The original developer lost interest in emulation shortly after and he seems to have vanished from the "scene" by now. However, he and ISNOGI united the developers that would eventually create Aura, providing him an important place in Aura's history.

###Yamase
Yamase (**Y**et **A**nother **Ma**binogi **S**erver **E**mulator) was developed by Yiting to produce an emulator with a source of higher quality than ISNOGI. Yamase featured much improved multi-threading, world management, and additional features, compared to ISNOGI. Yiting, however, put the project on hold within a few weeks, presumably due to a lack of interest.

Yamase and ISNOGI/Aura were developed alongside one another, providing better updates and systems to both sources. Research was able to be compared as the teams working on each emulator had the same goals and many of the techniques used by one emulator could be translated to the other source.

##Aura

Aura started out as a rewrite of ISNOGI in 2012. During this rewrite it was decided to eventually make the project public, and due to all the changes a new name seemed appropriate: Aura (Titan of the breeze and the fresh, cool air of early morning[<sup>ref</sup>](http://en.wikipedia.org/wiki/List_of_Greek_mythological_figures)).

Unfortunately it wasn't long before the team lost interest/motivation. Mid 2012 the source code wasn't public yet, but the project did have a forum, and people were hoping to see the emulator in action soon. With 2 developers basically having left, only exec remained, who didn't want to do all the remaining work himself. As a result he finally decided to publicize their existing work in September 2012, without much interest in continuing. That changed however, during cleaning up the source code and fixing bugs that the community found. Aura has been in constant development ever since.

Between late 2013 and early 2014, Aura was rewritten from scratch. The reason for this was that the source code released in 2012 hadn't been ready for release. But it was still used, only extending it, without fixing underlying issues and bad structural decisions.

In early 2017, Nexon took down the Aura repository, its forum, its chat, and everything else remotely related to it with DMCAs. Following this, the Aura team stopped developing the server in public, but continued to keep it up-to-date for private use.

##Chart

![](http://aura-project.org/history.png)