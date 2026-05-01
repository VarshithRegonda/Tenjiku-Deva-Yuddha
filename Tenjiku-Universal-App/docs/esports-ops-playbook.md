# Cross-Platform Esports Ops Playbook

## Engine and Shared Architecture
- Primary runtime: Expo React Native client + Node authoritative realtime server.
- Shared gameplay and policy logic: `src/esports/*`.
- Platform targets: desktop (web), Android, iOS from one codebase.

## Performance Tiers
- Desktop: high ceiling profile, up to 128Hz server expectation.
- Android: thermal-aware 30/60/90 FPS profile bands.
- iOS: 30/60/120 FPS profile bands with battery-aware defaults.

## Competitive Integrity
- Queue policy and placement configuration live in `src/esports/config.ts`.
- Basic input fairness checks and MMR evolution helpers in `src/esports/integrity.ts`.
- Backend validates authoritative actions and rate-limits clients per second.

## Spectator and Tournament Features
- Spectator defaults and replay bookmark model in `src/esports/spectator.ts`.
- Backend supports:
  - Tournament lobby creation and joins.
  - Spectator role joins.
  - Replay clip requests from ring buffer.

## Live Ops Cadence
- Balance patch cadence: every 14 days.
- Season cadence: every 84 days.
- Anti-cheat review window target: 24 hours.

## Release Gates
- `npm run verify:esports` must pass.
- Validate queue + replay events against multiplayer server.
- Validate desktop/web + Android + iOS launch paths each release candidate.
