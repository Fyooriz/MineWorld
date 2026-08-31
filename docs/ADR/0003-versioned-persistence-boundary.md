# ADR-0003 — Versioned Persistence Boundary

- Status: ACCEPTED
- Date: 2026-08-31
- Scope: P0/P1

## Decision

MineWorld save data must have an explicit versioned header containing save format, ruleset, generator ID, and generator version. Persistence DTOs are separate from runtime objects.

## Problem

The previous P0 save shape stored seed and state but had no explicit schema envelope or migration boundary.

## Rationale

World generation and saved state must remain interpretable as the project evolves. Versioned boundaries permit explicit migration and safe rejection of unknown formats.

## P0 policy

- Format version: `1`
- Ruleset version: `0.2.0`
- Generator ID: `mineworld:basic`
- Generator version: `1`

These values are protocol decisions for the P0 slice and are not claims about external games.

## Reliability

Writes use a temporary file and replacement rather than overwriting the target directly. A future server store may replace the file provider without changing the simulation contract.

## Verification condition

Invalid, missing, or unsupported headers must fail safely; valid P0 saves must round-trip in automated tests.
