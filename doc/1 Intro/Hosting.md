## Introduction

So, you've decided to host your own Aura server, but don't know how to get started?  
Well, you've come to the right place. (Because Wiki pages *are* usually informative.)

## Direct connection

If you're planning to have people directly connect to your computer, with no VPN (Hamachi is an example of a VPN), then this is the part of the guide for you. Otherwise, you're going to want to [look here](https://github.com/aura-project/aura/wiki/Hosting#vpn-connection)

### Requirements
* An internet connection
* At least *one* computer
* At least *one* installation of Aura

### Step 1: Identifying your computer

This is a pretty tricky step, but, for the most part, you can assume that you're probably reading this on it.
Crisis averted!

### Step 2: Identify which computer will host your Aura server

I'll assume that you followed [Exec's guide to installing the server](https://github.com/aura-project/aura/wiki/Getting-started), which is 97% of this step.
The following are each a percentile:
* Remembering which machine it was
* Finding it
* Turning it on  

Pretty tricky stuff, but I'm confident that you can handle it. I believe in you.

### Step 3: Gathering information
 
This is where things get real. Really real. Maximum amount of realness.
Press the Windows key + R in combination, enter `cmd` into that dialogue, and press OK. Now enter `ipconfig` in the window that opened and press Return.

Now, depending on your situation, you're going to want at least the default gateway:  

```
Default Gateway . . . . . . . . . : fe80::1:1%3
                                    192.168.1.254
```

If you're on the machine that you want to host your Aura server on, also note down the ***local*** IP address of your machine:  

```
IPv4 Address. . . . . . . . . . . : 192.168.1.1
```

If you want to host it on a different machine, repeat this step there, to get *that* computer's ***local*** IP.

Local IP addresses usually start with `192.`.

### Step 4: Opening the gates

To allow others to connect to your Aura server, you have to configure your router, to let connections to certain ports through, and forward them to the machine that Aura is running on.

Since we now have all the information needed to actually open some ports, take the default gateway address from the previous step, enter it into your browser's address bar, and press enter. (Did you really need me to tell you that last bit?)

Here is where things get horribly divergent, and you'll be mostly on your own, since every router is different.  
But don't worry! If you get stuck you can come by the [chat](https://gitter.im/aura-project/aura) or the [forums](aura-project.org/forum/), and we'll help you out.

Warning: A failure to provide information coupled with attitude filled replies in response to requests for more information will lead to you being *sassed into an early grave*. You have been warned.

You'll initially need to login to your router, which usually either has a default username and password, or a sticker on your device that tells you what your default login details are. If you don't know your login details, Google or the link below can assist you.

[You can find detailed information on setting up ports, along with pictures here.](http://portforward.com/english/routers/port_forwarding/routerindex.htm)

The default ports, and the reasons to forward them:

|   Port   |   What it's for     | Why you might want it |
|----------|---------------------|-----------------------|
| 11000    | Default login server port  | You might want people to join you, on your server |
| 11020    | Default channel server port| You might want people to be able to join a channel |
| 80       | Default web server port | If you're running the web server, and want people to be able to connect to it|
| 8002     | Default messenger server port | If you have a messenger server running, opening this port will allow connections to it.|

Bear in mind that you want to allow an external connection to these ports, and you want to forward them to the ***local*** IP address of the machine Aura is running on.

[Once that's done, you can check your work!](http://portchecker.co/)  
Don't forget to note down your ***external*** IP address, which you can find on that page! You'll need it for the next step.

### Step 5: Configuring your .confs

So, with the absolute hardest part out of the way, all you have to do now is open up `channel.conf` in the `system\conf\` folder of your Aura install, and where it says `channel_host : 127.0.0.1`, highlight the `127.0.0.1`, and replace it with the ***external*** IP address you got at the very end of the previous step.

And that's it! You should be completely ready to give out that ***external*** IP address to all your friends, and have a fun time!

### Optional Step 6: But my IP isn't static!

That's too bad. Very sad.  

But don't fret! You can get a free static hostname from [these nice people](http://www.noip.com/remote-access), and then put that in your client's command line parameters!

For example: `"C:\Mabinogi\Client.exe" code:1622 ver:171 logip:first.convenient.hostname logport:11000 chatip:another.convenient.hostname chatport:8002 setting:"file://data/features.xml=Regular, USA"`

## VPN Connection

TODO
