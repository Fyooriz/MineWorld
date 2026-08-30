# MineWorld Audio System

## Status
PARTIAL — architecture and deterministic runtime contract. No production audio files are bundled yet.

## Goals
- 3D positional audio with listener-relative attenuation.
- Biome-aware ambient layers.
- Day/night and weather-aware transitions.
- Cave/interior ambience based on sampled world context.
- Deterministic gameplay-triggered sound events for multiplayer.
- Data-driven event definitions so assets can be replaced without engine rewrites.

## Runtime model

`WorldContext -> AudioContext -> LayerSelector -> Mixer -> SpatialOutput`

The audio system must never become a gameplay dependency. If audio initialization fails, gameplay continues with a silent/null backend.

## Audio contexts

Each frame/tick may provide:

- biome id
- time-of-day phase
- weather state/intensity
- local elevation
- cave/interior factor
- nearby water factor
- nearby foliage factor
- player movement state
- dimension id
- master/audio accessibility settings

## Layers

1. `music`
2. `ambient`
3. `weather`
4. `environment`
5. `blocks`
6. `entities`
7. `ui`

Each layer has independent volume, mute, and accessibility controls.

## Determinism

Gameplay events such as block break, entity attack, projectile impact, and weather transitions are emitted from authoritative simulation events. Clients resolve the event to local audio assets and spatialize them. Random variation uses a seeded event variant selector so repeated network events do not create divergent gameplay state.

## Asset policy

Production audio must be:

- MineWorld-original, or
- sourced from a license compatible with redistribution and the repository's distribution model.

Do not import proprietary game audio, extracted assets, or copyrighted recordings from third-party games.

Natural-world recordings are preferred for environmental ambience when their provenance and license are documented.

## Performance

- Limit simultaneous voices by category.
- Use looped ambience for persistent environmental beds.
- Use distance culling and priority scoring.
- Stream long music/ambience assets instead of loading all tracks into memory.
- Avoid per-frame allocation in the mixer path.

## Failure handling

If an asset is missing or invalid:

1. log a diagnostic event,
2. fall back to the event's silent/default variant,
3. continue gameplay,
4. expose the failure to development diagnostics.
