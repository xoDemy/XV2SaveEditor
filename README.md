# XV2 Save Editor

A Windows save editor for *Dragon Ball Xenoverse 2*, made with love by Demyliciouss with help from Gliscors.

© 2026 Demyliciouss. All rights reserved. Created by Demyliciouss with help from Gliscors.

This repository is source-available, not open-source software. The source is
provided for viewing and community review only. See [LICENSE.md](LICENSE.md)
before copying, modifying, redistributing, rebranding, or republishing any
part of this project.

The editor supports verified editing tools for CaCs, presets, inventory and QQ Bangs, progression and unlocks, quests, mentor customization, Tokipedia, Play Data, save diagnostics, backups, and platform-aware save handling. Unknown save structures are intentionally left unsupported rather than guessed.

> This is an independent community project. *Dragon Ball Xenoverse 2* and related names and assets belong to their respective owners.

## Download

For normal use, download the latest portable ZIP from the repository's **Releases** page. Extract the complete ZIP, then run `START XV2 SAVE EDITOR.cmd`. Do not launch it from inside the ZIP.

The portable package includes its own private .NET runtime, so users do not need to install .NET separately.

## Supported saves

- PC / Steam: `DBXV2.sav`
- Xbox: supported decrypted `.bin` containers
- PlayStation: verified decrypted and encrypted SDATA/`.DAT` variants
- Switch: not currently supported

Steam saves moved between accounts must be linked to the destination Steam ID before saving. Some console outputs still require platform-specific re-encryption or profile resigning before a console will accept them.

Always keep an untouched copy of the original save until the edited version has been tested in game. The editor also creates organized safety backups under `Documents\XV2 Save Editor Backups` before writes.

## Building from source

Requirements:

- Windows x64
- Visual Studio with the .NET desktop development workload, or the .NET 10 SDK
- The required `AesCtrLibrary.dll` dependency in the project root

Build the x64 configuration:

```powershell
dotnet build .\XV2SaveEditor.csproj -c Debug -p:Platform=x64
```

The executable will be written under `bin\x64\Debug\net10.0-windows`.

## Repository safety

Do not commit real saves, exported CaCs, Steam account details, backups, scan results, or diagnostic dumps. The `.gitignore` excludes the common forms of these files, but contributors should still review every commit before publishing it.

## Third-party components

The project currently depends on `AesCtrLibrary.dll` and optional platform-conversion helpers under `Tools\PlatformConverters`. Their upstream projects and redistribution terms must be confirmed before including those binaries in a public repository or GitHub Release. See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

This project uses a source-available, all-rights-reserved notice rather than an open-source license. The source may be viewed for community review, but copying, modification, redistribution, rebranding, and republishing require prior written permission.

## Contact

- Discord: `demyliciouss`
- Discord: `gliscors`
- [Desurui Discord](https://discord.com/invite/desurui)
- [Community Discord](https://discord.gg/rrpvUequwX)

See [CHANGELOG.txt](CHANGELOG.txt) for release changes and [RELEASE-README.txt](RELEASE-README.txt) for end-user instructions.
