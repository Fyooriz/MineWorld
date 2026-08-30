# MineWorld Environment Context

## Status
PARTIAL — engine contract only. Runtime integration is pending the renderer/world runtime.

## Purpose

Provide one read-only context snapshot for rendering, audio, particles, and simulation systems. Consumers must not mutate world state through this interface.

## Context

- dimension_id
- biome_id
- time_of_day
- weather_state
- precipitation_strength
- ambient_light
- sky_visibility
- is_underground
- is_submerged
- local_temperature
- wind_strength

## Rules

1. World simulation owns authoritative state.
2. Rendering/audio consume snapshots.
3. Presentation randomness must not alter simulation state.
4. Missing optional data falls back to a deterministic default profile.
5. Context snapshots are cheap to copy and safe to cache for a frame/tick.

## Integration targets

- `Engine/Audio/AmbientSelector.md`
- World/biome runtime
- Weather runtime
- Lighting runtime
- Particle runtime
