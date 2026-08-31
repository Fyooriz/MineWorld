# ADR-0004 — Client Input Is Not Authority

- Status: ACCEPTED
- Date: 2026-08-31
- Scope: P0/P1 and future multiplayer

## Decision

Playable collects input into frame-local state. Consequential gameplay actions must eventually cross an application command boundary. The authoritative server, when present, validates the same command contracts against the Core simulation.

## P0 implementation rule

The P0 client may invoke Core-backed gameplay through the local application layer. It must not introduce a second world implementation.

## Future multiplayer rule

Client messages represent explicit commands or requests. They do not carry trusted world mutations, inventory outcomes, permissions, economy changes, or damage results.

## Rationale

This preserves one rules implementation across local play, server play, tests, and future replay/debug tooling.

## Verification condition

A future server can reuse Core simulation rules without changing the meaning of a player action.
