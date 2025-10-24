# WorkTimer

Simple Windows desktop timer for tracking daily shift and task time. Built with .NET8 and Windows Forms.

- Track worked shift time (excludes breaks)
- Start/pause tasks, mark finished, rename or delete
- Explicit break tracking independent from tasks
- Target shift length with progress and remaining time
- Tray icon with quick actions (Show, Pause/Resume, Settings)
- Balloon notifications (half, end, next15?minute leave window)
- History of finished shifts (JSON)
- Optional CSV export of segments
- Localization: Czech and English
- Portable data (JSON) saved next to the executable

## Getting started

Prerequisites:
- Windows10/11
- .NET SDK8.0+ (for building)
- Visual Studio2022 or `dotnet` CLI

Build and run:
- Visual Studio: open `WorkTimer.App/WorkTimer.App.csproj` and Run.
- CLI: `dotnet build -c Release` then run the produced exe in `WorkTimer.App/bin/Release/net8.0-windows/`.

Portable data files (created next to the executable):
- `settings.json` – app preferences (language, target shift, notifications)
- `shift.json` – current/last shift state
- `history.json` – list of finished shifts

Nothing is synced or sent anywhere.

## Usage

- Set arrival/start time via `Set start`.
- Start tasks: type a name and press Enter or click `Start task`.
- Pause/Resume the current task or start a different task (auto?pauses current).
- Take a break with `Break`/`End break` – breaks extend the planned end time but don’t count as worked time.
- End shift with `End shift` (moves the shift to history).
- Tray icon: double?click to show, or use the context menu.
- Settings: language, target shift, notification bubbles.

CSV export (optional UI button is hidden by default in code) writes: `Start, End, Duration, Task, Note`.

## Localization

- Supported: `Èeština`, `English`
- Change in `Settings` dialog. Strings are in `WorkTimer.App/Services/Localization.cs`.

## Project structure

- Target: .NET8, Windows Forms
- Project: `WorkTimer.App`
- Entry form: `FormMain`
- Tray icon service: `TrayManager`
- Persistence: `PersistenceService` (JSON next to EXE)
- Core model: `Shift`, `WorkSegment`, `BreakSegment`, `AppSettings`

## Releases

See `docs/RELEASE.md` for publishing steps and recommended settings.

## Screenshots

Add screenshots under `docs/screenshots/` and reference them here.

## Roadmap and contributing

- Planned ideas: see `ROADMAP.md`
- How to contribute: see `CONTRIBUTING.md`

## License

Licensed under the MIT License. See `LICENSE`.