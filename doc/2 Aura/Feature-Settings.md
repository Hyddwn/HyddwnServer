Aura has two ways of modifying your game play. One are the configuration settings that can be found in `system/conf/`, which includes options like experience rates, drop rates, etc, and the other are the feature settings, found in `system/db/features.txt`. The feature settings include everything that was changed in Mabinogi over the "generations", and has only boolean options, that can be enabled or disabled. These settings allow you to easily enable/disable features of entire generation updates, with the flip of a switch.

For this, the features db supports a children/parent design, wherein all children become active if a parent gets enabled. As you can see in the following example that was taken from the features db, "G1" is enabled (true = enabled, false = disabled), which means that all children that also have `enabled: true` are enabled as well.

```js
// Generation 1
{ name: "G1", enabled: true, children: [
	{ name: "MainStreamG1", enabled: true },
	{ name: "Rebirth", enabled: true },
	{ name: "SystemGuild", enabled: true },
	{ name: "PersonalShop", enabled: true },
	{ name: "EnchantEntrust", enabled: true },
	{ name: "DynamicMonsterAllocation", enabled: true },
	{ name: "FoodStatLimit", enabled: true },
	{ name: "G1EasyOverseas", enabled: false, children: [
		// Kill only ten zombies to revive in TNN.
		{ name: "EasyBinding1", enabled: true },
	]},
	
	// Season 1
	{ name: "G1S1", enabled: true, children: [
		{ name: "CiarAdvanced", enabled: true },
	]},
	
	// Season 2
	{ name: "G1S2", enabled: true, children: [
		{ name: "ItemShop", enabled: false },
	]},
]},
```

If you wanted to disable everything that was added with G1, including the mainstream quest, you would simply set G1's enabled to false, and you'd be "back in beta". Similarly you could go to G2 and set enabled to true, to activate everything in it.

To enable only a single feature of G2, without G2, like "Fireball", so it can be learned during G1, you take the Fireball line and copy it to the end of the file.

```js
// Generation 19
{ name: "G19", enabled: false, children: [
	// Season 1
	{ name: "G19S1", enabled: true, children: []},
	
	// Season 2
	{ name: "G19S2", enabled: true, children: [
		{ name: "TalentRenovationCloseCombat", enabled: true },
		{ name: "TalentRenovationMagic", enabled: true },
		{ name: "TalentRenovationArchery", enabled: true },
	]},
]},

{ name: "Fireball", enabled: true },
]
```

Unfortunately there's no way to enable children this way. If you enable, say, "G2S1", without copying its children, you will only enable "G2S1", but not "PeacaDungeon", "Fireball", etc. If you don't want to enable a whole generation, you have enable every single feature you want separately.

Now keep in mind that Aura is still in development, and just because we already have all generations in the file, doesn't mean all features are working. If G2 hasn't been implemented yet, enabling it won't necessarily activate all features. Enabling Fireball can't do anything if Fireball doesn't work yet. Keep an eye on our [milestones](https://github.com/aura-project/aura/milestones), to see where Aura stands in terms of features. If you see that the G1 milestone is done, you can safely assume that all G1 features are working.