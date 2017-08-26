* [Enter dungeon warp effect](#enter-dungeon-warp-effect)
* [Random random weather](#random-random-weather)
* [Id obsession](#id-obsession)

## Enter dungeon warp effect

When entering a dungeon, by dropping an item onto an altar, you get a special warp effect. This effect seems to not be hard-coded for entering dungeons though, it depends on the regions you come from and go to. If you warp from Alby altar to Ciar altar you get the same warp effect, not because you entered a dungeon, but because the regions are visually similar. The same works between Rabbie and Math.

## Random random weather

Due to a bug in one of Mabinogi's Random Number Generators, the locally displayed weather is "truly" random every few hours. At that time you can't predict the weather, it actually changes with every re-log, because the RNG reads a value from dynamically allocated memory that's not its own.

The server, which assumedly has the same problem, because it uses the same libraries, will calculate its own "random random" weather, which will differ from what the client displays.

Aura identifies this as weather type "Unknown", which doesn't grant any bonuses.

## Id obsession

The client, or rather Mabinogi as a whole, heavily depends on ids being within certain ranges. For example, a character's id *has to be* between 0x0010000000000001 and 0x0010010000000000. If it's not, *you can't even log in*.

The client also differentiates between characters, pets, and partners by id. They all have their own id range, which the client uses to decide what you can do with which creature, such as preventing you from logging in with one, which's id is in the partner id range. If you give a partner an id in the pet range you can log in just fine.

Another example are the ids of props (trees, bushes, etc.), which are pieced together short ints, 4 in total: an identifier, the region, the area inside the region, and an actual id. If any part of that whole id is incorrect, you can't interact with the prop.