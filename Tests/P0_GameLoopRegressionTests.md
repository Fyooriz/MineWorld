# P0 Game Loop Regression Tests

## Scope

Regression coverage for the fixed-step playable loop and player movement.

## Cases

| ID | Case | Expected |
|---|---|---|
| GL-001 | Render frame takes longer than 50 ms | Simulation delta is clamped; no oversized physics step |
| GL-002 | Normal 60 FPS frame | Simulation advances by one 1/60 s step |
| GL-003 | Temporary frame hitch | Accumulator catches up with bounded fixed steps |
| GL-004 | Severe frame hitch | Fixed-step cap prevents a spiral of death |
| GL-005 | Mouse look while moving | Camera/player orientation updates independently of render rate |
| GL-006 | Forward + strafe | Horizontal movement is normalized; diagonal speed does not exceed configured speed |
| GL-007 | Jump from ground | Vertical velocity is initialized once and gravity is applied consistently |
| GL-008 | Falling onto terrain | Player is clamped to the terrain surface and downward velocity resets |
| GL-009 | World streaming | Streaming is driven from the simulation/player state rather than render frequency |
| GL-010 | Save input | World persistence is triggered without stopping the render loop |
| GL-011 | Window close | Final world save occurs before loop exit |

## Verification status

STATUS: PARTIAL

The cases are specified as regression requirements. Runtime execution still requires a build/test-capable environment; no PASS result is claimed from repository inspection alone.
