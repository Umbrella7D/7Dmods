# AirDropBomb mod

## Desc
The AirDrop crate explodes on impact. It still contains loot.


## Impl
- The base EntitySupplyCrate "sc_General" is changed to use subclass EntitySupplyCrateBomb.
- On contact (with non Air Block below), it explodes.
- Explosion throws molotov and custom frag bombs around
- They are tied to the closest player (XP is gained)

## Todo / Fixme
- Check damages
- NB: Only tested through game command in a single player game.

## Future
- Example in /Harmony to trigger a molotov above player location when the crate is spawned (currently the molotov explodes before contact !)
- It is possible to add multiple crate types (list Crates) to have multiple (they are randomly selected)
- But the debug command only uses "sc_General" !
- The flight path could also be modified to go closer to player, instead of dropping a second projectile near player and far from the plane
