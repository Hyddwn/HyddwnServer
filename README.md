Aura
==============================

Aura is an open-source server emulator written in C#. It's solely being
developed for educational purposes, learning about programming, MMORPGs,
maintaining huge projects, working with other people, and improving knowledge.
It's not about playing a game or competing with any services provided by
Nexon or its partners, and we don't endorse such actions.

Aura is completely free and licensed under the GNU GPL.
As such, every user is free to use Aura and choose how to use it.

Compatibility
------------------------------
Compatibility to all major versions but NA was dropped on
[2013-09-13 (Aura Legacy)](https://github.com/Fuhhue/aura_legacy/commit/c6483faace4d79b8f772bee519531718084a243d). Since that time, Aura is kept compatible only to the *latest update* of NA.

Requirements
------------------------------
To *run* Aura, you need
* .NET 4.5 (Mono 3.0+)
* MySQL 5 compatible database

To *compile* Aura, you need
* C# 5 compiler, such as:
  * [Visual Studio](http://www.visualstudio.com/en-us/products/visual-studio-express-vs.aspx) (2012 or later)
  * [Monodevelop](http://monodevelop.com/) (With mono version 3 or greater)
  * [SharpDevelop](http://www.icsharpcode.net/OpenSource/SD/) (Version 4.4 or greater)

Installation
------------------------------
* Compile Aura
* Run `sql/main.sql` to setup the database
* Copy `system/conf/database.conf` to `user/conf/`,
  adjust the necessary values and remove the rest.

Afterwards, you should be able to start Aura via the provided scripts or
directly from the bin directories. If not, or if you need a more detailed guide,
head over to our [Gitter chat](https://gitter.im/aura-project/aura).

Contribution
------------------------------
There are 3 ways **you** can help us to improve Aura:

1. Research
2. Bug reports
3. Pull Requests

### 1. Research
Do research on NPCs, quests, skills, anything really that isn't implemented yet.
The information you post will help developers to implement the features. 

### 2. Bug reports
Report bugs on [GitHub](https://github.com/Whimsytech/aura/issues) so they can be fixed ASAP.
If you're not sure if you've found a bug or not, report it on [Gitter Chat](https://gitter.im/aura-project/aura),
and we'll clarify for you.


### 3. Code
The fastest way to get code contributions into the source is a pull request, which,
if well written, can be merged right in to *master*. To expediate this process, 
all pull requests must comply with our coding conventions below.

#### Coding conventions
* Base: [MS Naming Guidelines](http://msdn.microsoft.com/en-us/library/xzf533w0%28v=vs.71%29.aspx),
        [MS C# Coding Conventions](http://msdn.microsoft.com/en-us/library/ff926074.aspx)
* Exceptions:
  * Use `_private` for private fields and `this.Foobar` for properties, public fields, and methods.
  * Use tabs, not spaces.
* Comment lines shouldn't exceed ~80 characters, other lines' lengths are irrevelant.
* Excessive usage of the auto-formatting feature is encouraged. (Default VS settings)
* Avoid regions.

Common problems
------------------------------

### Errors after updates
Usually all errors are solveable by recompiling, running SQL updates from `sql/`, and deleting the cache folder.

### Korean message when trying to connect to channel
This message means that the client wasn't able to connect to the channel,
which is usually caused by configuration mistakes. Make sure you can reach
the IP/Port you've set the channel to run on (`channel.conf`) from the affected computer.

In some rare cases this can also be caused by routers, for example,
if you try to make the server publicly available and the router doesn't
allow you to connect to your own public IP.

Links
------------------------------
* GitHub: https://github.com/Whimsytech
* [![Gitter chat](https://badges.gitter.im/aura-project/aura.png)](https://gitter.im/aura-project/aura)
