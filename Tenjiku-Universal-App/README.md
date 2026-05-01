# Tenjiku Universal App (Esports Foundations)

Cross-platform esports client for desktop/web, Android, and iOS with a server-authoritative multiplayer backend.

## Quick Start

```bash
npm install
npm run start
```

## Core Commands

- `npm run web` - launch desktop/web runtime.
- `npm run android` - launch Android runtime.
- `npm run ios` - launch iOS runtime.
- `npm run server` - start authoritative multiplayer server.
- `npm run verify:esports` - run lint + typecheck quality gate.
- `npm run loadtest:server` - run synthetic multiplayer load simulation.

## Esports Architecture

- Shared platform and integrity policies in `src/esports/config.ts`.
- Queue and MMR helper logic in `src/esports/integrity.ts`.
- Spectator/replay structures in `src/esports/spectator.ts`.
- Realtime backend in `server/server.js` with:
  - rate-limited authoritative actions,
  - tournament lobbies,
  - spectator support,
  - replay clip streaming.

## Live Ops and Release Readiness

- Operational guide: `docs/esports-ops-playbook.md`.
- Phase completion record: `docs/phase-completion-report.md`.
- CI checks: `.github/workflows/cross-platform-ci.yml`.
- Mobile/web release preview: `.github/workflows/mobile-release-preview.yml`.
