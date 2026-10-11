# SQLite3MC Native Library

The application uses a custom x64 build of SQLite3 Multiple Ciphers instead of a precompiled SQLitePCLRaw native bundle.

## Build

From the repository root, run:

```powershell
.\Scripts\SQLite3MC\build.ps1
```

To build another SQLite3MC release:

```powershell
.\Scripts\SQLite3MC\build.ps1 -Version 2.5.0
```

The script requires Git and an x64 MinGW `gcc.exe` on `PATH`. Use `-GccPath` to provide another compiler path.

The generated `runtimes/win-x64/native/sqlite3mc.dll` uses SQLCipher with version 4 compatibility as its default codec. Project builds and publishes copy this DLL beside the application automatically.

## Current Build

- SQLite3MC version: 2.5.0
- Source commit: 4bbdff307dd48cf439978659c54c05878022c2cb
- SHA-256: D2285934FEFF02B3D02DC374499196E9EA843DB31ED88DC4EF36F1481351E3FB