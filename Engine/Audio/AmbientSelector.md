# Ambient Selector

## Status
PARTIAL — selection rules defined; runtime implementation follows after the engine backend is established.

## Selection priority

`dimension > interior/cave > weather > biome > time-of-day > rare event`

Higher-priority contexts modify or replace lower-priority layers rather than creating unlimited concurrent loops.

## Example profiles

| Context | Primary ambience | Secondary layer |
|---|---|---|
| Forest / day | wind + foliage | distant wildlife |
| Forest / night | low wind + insects | sparse wildlife |
| Plains / day | open wind | distant wildlife |
| Ocean / shore | waves | wind |
| River / lake | flowing water | wind/foliage |
| Desert | dry wind | sparse environmental accents |
| Snow | cold wind | sparse environmental accents |
| Cave | enclosed low-frequency ambience | distant water/drips |
| Rain | rain bed | biome ambience reduced |
| Storm | heavy rain | thunder events |

These are design profiles, not copied game assets.

## Crossfade

Ambient profile changes use short crossfades. Abrupt context changes are allowed for exceptional events such as explosions or major world events.

## Naturalism

Avoid constant repetitive loops. Use layered recordings, randomized phase offsets, bounded event intervals, and distance-aware one-shot sounds. Randomness affects presentation only and must not affect gameplay state.
