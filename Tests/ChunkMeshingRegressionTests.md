# Chunk Meshing Regression Tests

## Scope

These tests define the P0 contract for CPU greedy meshing, asynchronous scheduling, stale-result rejection, and render-thread GPU ownership.

## Cases

| ID | Scenario | Expected |
|---|---|---|
| MESH-001 | Empty chunk | No vertices/indices; no GPU mesh resident |
| MESH-002 | Single solid block | Six exposed faces |
| MESH-003 | Two adjacent equal blocks | Shared face omitted; coplanar faces may merge |
| MESH-004 | Different block IDs | Faces are not merged across IDs |
| MESH-005 | Chunk boundary neighbor is solid | Boundary face is omitted |
| MESH-006 | Chunk boundary neighbor is air | Boundary face is emitted |
| MESH-007 | Chunk remeshed twice before first job completes | Only newest version may become visible |
| MESH-008 | Worker throws | Error is reported without crashing the render thread |
| MESH-009 | CPU worker calls renderer/GPU API | Architecture violation; GPU work must remain on render thread |
| MESH-010 | Empty replacement mesh | Existing GPU mesh is released |
| MESH-011 | Mesh exceeds 16-bit index capacity | Upload is rejected explicitly; no silent truncation |
| MESH-012 | Scheduler shutdown | Workers stop and GPU resources are released by owner |

## Verification rule

Do not mark a case PASS until it is executed by the CI/build environment. This document is the regression contract; it is not evidence that the current implementation has passed every case.
