# Roadmap

Ideas and possible next steps. Vote or send PRs.

## Short term
- App installer (MSIX) and versioning
- Add `LICENSE` and choose an OSI-approved license (MIT recommended)
- Optional minimize-to-tray on close, start minimized
- Auto-start with Windows (toggle in Settings)
- Persist window position/size
- Daily auto-archive to History at midnight if running
- Export improvements: date range export, per-task summary
- UI: dark theme support, better icons

## Medium term
- Per-task notes editing UI
- Hotkeys: global start/pause, quick task switch
- Configurable leave window cadence (e.g.,10/15/20 minutes)
- System notifications via Windows toast instead of balloon tips
- Multi-day shift support (carry over)
- Backup/restore of JSON data

## Long term
- Pluggable storage (SQLite) with reports
- Synchronization options (OneDrive) – optional
- Multi-language support beyond CS/EN
- Telemetry-free update checker

## Nice to have
- Unit tests for `Shift` logic
- CI builds (GitHub Actions) with Release artifacts
- Code signing for releases
