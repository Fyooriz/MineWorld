# MineWorld — Audio, Music & Natural Asset Guidelines

## Goal

MineWorld uses an original visual and audio identity. Natural environments should feel grounded in the real world without copying proprietary game assets, sounds, textures, models, UI, or branding.

## Audio Direction

### Ambient layers
- Wind: biome-dependent intensity and variation.
- Water: shoreline, stream, river, lake and ocean layers.
- Forest: leaves, branches, insects and distant wildlife.
- Plains: wind, insects and sparse wildlife.
- Mountains: wind gusts, rock ambience and distant weather.
- Caves: low environmental ambience, dripping water and spatial reverb.
- Rain/thunder: dynamic intensity controlled by weather state.
- Night: reduced activity with biome-specific nocturnal ambience.

### Music

Music is adaptive and original rather than continuously looping.

Recommended states:
- exploration_day
- exploration_night
- forest
- plains
- mountains
- ocean
- caves
- settlement
- danger
- boss
- discovery
- calm_building

Music should crossfade rather than abruptly restart. Gameplay remains fully playable if music is disabled or unavailable.

## Natural Asset Direction

Use stylized-realistic representations inspired by real-world nature:
- rock formations
- soil layers
- trees
- grasses
- flowers
- fungi
- clouds
- water surfaces
- snow/ice
- natural particles

Assets should be authored from scratch or sourced under a license compatible with MineWorld distribution. Do not import Minecraft assets or derivative copies.

## Technical Requirements

- Prefer data-driven asset manifests.
- Keep audio event IDs independent from filenames.
- Support variation pools to avoid repetition.
- Use 3D spatial audio for world sounds.
- Use distance attenuation and biome/state filtering.
- Avoid loading every audio asset at startup.
- Stream long music tracks where appropriate.
- Keep placeholders explicit and replaceable.

## Suggested Asset Layout

```text
assets/
  audio/
    ambient/
    music/
    creatures/
    blocks/
    weather/
    ui/
  textures/
    blocks/
    items/
    environment/
  models/
    blocks/
    entities/
    environment/
  particles/
  manifests/
```

## Licensing Rule

Every external asset must have recorded provenance and license information before being included in a release build.

Required metadata:
- asset ID
- creator/source
- license
- attribution requirement
- modification permission
- redistribution permission
- local filename/path

## Status

STATUS: PARTIAL — architecture and production guidelines established. Actual final music, sounds, textures and models require authored or properly licensed asset files.
