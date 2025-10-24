# Contributing

Thanks for your interest in improving WorkTimer.

## Development setup
- Install .NET SDK8.0+
- Open `WorkTimer.App/WorkTimer.App.csproj` in Visual Studio2022 or newer
- Or use `dotnet build` from the repo root

## Branching & commits
- Create feature branches from `master`
- Keep commits small and focused
- Use meaningful messages (English or Czech)

## Code style
- C#12, nullable enabled, implicit usings enabled
- Prefer explicit `IDisposable` cleanup for UI components
- Keep UI text localized via `Services/Localization.cs`

## Testing the app
- Run the app and verify:
 - Start/stop tasks, break handling, shift end
 - Tray menu actions
 - Settings persistence and language switch
 - JSON files created next to the EXE

## Pull requests
- Include a short description and screenshots if UI changes
- Update `README.md` if behavior or setup changed
- Reference related issues if applicable

## Reporting issues
- Provide steps to reproduce, expected/actual behavior, and logs (if any)
