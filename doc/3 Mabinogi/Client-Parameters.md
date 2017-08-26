The client takes various parameters that affect how it operates. This page lists some of them that we know about.

- [code](#code)
- [ver](#ver)
- [logip](#logip)
- [logport](#logport)
- [chatip](#chatip)
- [chatport](#chatport)
- [setting](#setting)
- [resourcesvr](#resourcesvr)
- [Gamania](#gamina)

##code
Purpose unknown, required for starting the client.

**Example**
```
code:1622
```

##ver
Purpose unknown, not required for starting the client. It never made a difference whether this number was equal to the actual client version or not.

**Example**
```
ver:143
```

##logip
IP of the login server the client is supposed to connect to upon clicking "Login".

There aren't any channel connection parameters, since the login server gives those information to the client.

**Example**
```
logip:127.0.0.1
```

##logport
Port the login server is listening on.

**Example**
```
logport:11000
```

##chatip
IP of the messenger server (friend list, etc).

**Example**
```
chatip:127.0.0.1
```

##chatport
Port of the messenger server.

**Example**
```
chatport:8002
```

##setting
Setting to use from features.xml. Using a setting from a different region usually results in bugs and crashes.

**Example**
```
setting:"file://data/features.xml=Regular, USA"
```

##resourcesvr
This parameter gives the client a URL where it can look for files that are missing in the client. This seems to be mainly used for MP3 files, it's unknown whether the client would download other files from there as well.

Basically this enables custom BGM without client modifications, because you can simply tell the client where to download the music from that it doesn't have locally.

The MP3 files that are stored on the resource servers of KR are "encrypted". A tool to encrypt files in the same way can be found [here](https://github.com/exectails/rsvrconv). They need to be saved in a "mp3" sub-folder, without extension (e.g. "test", not "test.mp3"), and the files have to be "clean", i.e. no unnecessary metadata, like album arts.

**Example**
```
resourcesvr:"http://127.0.0.1/resources/"
```

##Gamania
Gamania is the publisher (?) of TW, on which client we've noticed the following parameters.

```
/N:username /V:password /T:gamania
```

These are used by their web launch to login automatically. They work on NA as well, so they could be used by launchers and the like.