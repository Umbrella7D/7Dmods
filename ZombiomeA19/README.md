Zombiome is a mod for 7DTD

# Zombiome

Zombiome - it is not dead ! 

 Behold survivors !
Mother Hearth has been soiled long enough by these nasty Zombies ... it can stand no more.
Its revolt has started, and biomes won't make a distinction between the livings and the undeads.

Apocalypse is upon use ... let's hope it is fun


A big thank to the forums' community - to all of you that help, answer questions and provide great tutorials !

## Mod Features
The world has permanent random activity, structured by biomes and geographic areas. Most are
apocalyptic natural disasters. A few of them are more "supernatural".

Biome Activity includes:
- Earthquakes, ground motions, flood, geyser
- Bushfires, wissfires, electric phenomena, meteore strikes, starfall
- Moving and flying biome elements, slime blocks 
- Toxic emanations with various effects (poison, freeze, enlighten, blinded ... )
- Altered gravity, sizes 


** Gameplay hints **

Globally, this mod only makes it harder for the player.

Depending on your location, biome activity may be fun, or it may be a pain.
If you don't like what is happening around you, move and change area ! You'll find worse :)

Some activites may be even be used to your advantage. Observe your surroundings, explore, adapt or uninstal !

## Current status

experimental - I need feedback
SP: playable
MP: probably too buggy

Important Warnings to players
- It can mess up your saves. Duplicate your savefile or start a new one !
- It could break at any release (tested through A18.2/4)
- It is not yet balanced, and performances are not optimized.
- Errors are still too frequent in multiplayer. I wasn't able to test MP much. 
- Expect that any container (and its content !) could be destroyed in a second. The biome ground-motions are not friendly to containers (yet), but
there are 2 things you can do for your most precious safes:
	- There is a landclaim/bedroll protection for all biome activity. In particular, the game should not destroy containers in these area
	- Activity depends on the map position you are at. If you see no ground motion for a long enough time period, there might not be any in the area.


Emergency manual: In case of error / crash, feel free to share the log file
.../Mods/zb_err.txt. Please add these info: worldseed, worldsize, MP/SP, short description

## Quick Code Guide
*under construction*

1) Activity start
- buffZombiomeManager starts local coroutines


2) Zone, areas, effect generation
- 


3) Effects implementation


**Commands**

zb pause : pause ZB activity (already running effects still need to finish)
zb start : restart ZB activity (or start if automatic start failed)
zb start X : select the activity to run
zb nz 1 / zb nz 4 : Activity uses 1 or 4 (default) adjacent areas. 1 useful for testings
zb f : togle random activity time period (Vs. always active)

## Known issues, todo list

DEV
- Check if nitrogen creates more biome names
- Track and use player speed for next effect loc, anticipate player direction (eg vehicles)
- Meteorite: new block stops projectiles : + distance
- rate near (not at) player for proj
- Adjust effect intensity (not only frequency)
- more blocks / cluster in block rain
- More ghost trajectory / behaviour / powers
- some message / sign when zone activates
- surface above buildings for entity (not for groundmotion)
- spark particle on dwarf. keep the smoke ghost for sth else (volcanic)
- Release GC pressure (in-place ops on Vector3, strings)
- Reimplement FloatingDeco with Searcher. Use a transition on airFull ?
- Some effect reversion

FIXME
- Projectiles: EntityItem fell off the world (too fast or not high enough)
- Multiplayer desync (giant, ghost)
- remove ghost on minimap (helps debug)
- unkown particle effect impact_stone_on_ : (str cat with material.impact sound)
- tree burnt maple flying disappear below ground
- Err removeChunk Transforms !zone (occlusionmanager::RemoveChunkTransforms)
- Fix vertical mbs (airtemp)
- Ground motion: Multiblock poorly managed (if horizontal, or if vertically supporting something)
- Fix TileEntity re-created (container content vanishing) : drop the content to ground (DropContentOfLootContainerServer) / track and re-assign
- Hard to have precise Entity motion (implementation depends on frame rate and callback delay, but monitoring improves)
- Vehicles gravity is not applied automatically after motion
- Removed the "broken glass" sound of the base molotov. Use fire loops ?
- House decoration moving weirdly
