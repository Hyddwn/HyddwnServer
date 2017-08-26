##Handshake

Upon connection to a Mabinogi server, the "Handshake" begins. This is a series of packets that are sent back and forth between client and server, that verify to the server that you're allowed to connect. The handshake can be skipped if you have control over the server, which is considerably easier than the alternative, which would involve working with HackShield, which is used in the process.

In recent updates HackShield has been removed and the handshake has been deactivated as a result.

##Encryption

Once you're past the Handshake process you can send and receive packets as you see fit. Packets are encrypted using a customized AES algorithm, which is why you have to decrypt them on the server to be able to read what the client told you. It's possible to send packets unencrypted from the server to the client, by setting a flag in the packet header. While the client accepts unencrypted packets, it's unknown if the official server does so as well.

##Checksum

Packets from the client to the server include a 4 byte checksum at the end of the packet. The algorithm for those is currently not public, but generally the checksum can be ignored.

##Packet Structure

While the servers use two different ways to specify how long the packets are, the actual packets are the same for all servers (Login, Channel, Messenger).

```
[00 00 00 1E] [00 00 00 00 00 00 00 00] [20] [02] [00] [01 01] [06 00 1B 55 53 41 5F 52 65 67 75 6C 61 72 2D 30 33 31 32 44 46 2D 41 39 45 2D 33 43 37 00]
```

* `00 00 00 1E` - The packet's op code, this identifies the packet and lets us know how to read it.
* `00 00 00 00 00 00 00 00` - The target or source id, usually the id of the sending character, e.g. the person sending the chat message.
* `20` - The length of the body of the packet, the actual variables, in bytes. ([variable length](https://en.wikipedia.org/wiki/Variable-length_quantity))
* `02` - The amount of elements (variables). ([variable length](https://en.wikipedia.org/wiki/Variable-length_quantity))
* `00` - Purpose unknown, seemingly always 0x00.
* `01 01` - A variable of type byte (0x01), with the value 0x01.
* `06 00 1B 55 53 41 5F 52 ...` - A variable of type string (0x06) with the value "USA_Regular-0312DF-A9E-3C7", incl. null byte, of length 27 (0x1B).

**Element Types**

| Type   | Id | Length                            |
|:-------|:---|:----------------------------------|
| Byte   | 1  | 1+1 bytes, type and value         |
| Short  | 2  | 1+2 bytes, type and value         |
| Int    | 3  | 1+4 bytes, type and value         |
| Long   | 4  | 1+8 bytes, type and value         |
| Float  | 5  | 1+4 bytes, type and value         |
| String | 6  | 3+X bytes, type, length and value |
| Bin    | 7  | 3+X bytes, type, length and value |

##Packet headers

The packet header for the channel and login servers has the following format:

```
[00] [00 00 00 00] [00] [Packet]
```

The first byte's purpose is known (using 0x88 from the server works fine), the following int is the length of the entire packet, incl. header, and the last byte is the type flag. Use `0` for encrypted packets and `3` for unencrypted. Type 1 might be some kind of ping packet.

The messenger server's packets have a slightly different header.

```
[00] [00] [00] [00] [Packet]
```

The first byte is a marker that apparently is always 0x55, the second byte's purpose is unknown, maybe a checksum of sorts, the third byte is a flag, 2 for unencrypted, and the last byte is a [variable length](https://en.wikipedia.org/wiki/Variable-length_quantity) value for the packet size.