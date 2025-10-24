# Release guide

This app is a single WinForms executable. You can distribute it as a zipped folder or MSIX installer.

##1) Versioning
- Update `AssemblyVersion`/`FileVersion` via `Directory.Build.props` or project properties (optional).

##2) Build Release
- Visual Studio: Set Configuration to Release, build `WorkTimer.App`.
- CLI: `dotnet publish WorkTimer.App -c Release -r win-x64 -p:PublishSingleFile=false -p:PublishReadyToRun=true`

Artifacts will be under `WorkTimer.App/bin/Release/net8.0-windows/win-x64/publish/`.

##3) Portable zip
- Include the `WorkTimer.App.exe` and its runtime files.
- Exclude `shift.json`, `settings.json`, `history.json` (created on first run).

##4) Optional: Single-file
- `dotnet publish WorkTimer.App -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=false`

##5) Optional: MSIX
- Create a Packaging project or use Windows App SDK packaging to generate an MSIX.

##6) GitHub release
- Tag: `vX.Y.Z`
- Upload zip with binaries
- Add changelog notes
