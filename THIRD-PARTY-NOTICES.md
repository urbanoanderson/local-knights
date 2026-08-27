# Third-Party Notices

This project, Local Knights, is licensed under the Mozilla Public License 2.0. The full text is available in [LICENSE](LICENSE).

This file documents third-party libraries, packages, and bundled components used by the application. It is intended as a practical consolidated notice file for distribution and repository transparency.

## How To Use This File

- Use this document as the central index for third-party attributions and license references.
- Keep the original license texts for redistributed dependencies when their licenses require it.
- Update this file whenever a package or bundled binary is added, removed, or replaced.

## Bundled Or Directly Integrated Components

### chesspairing Go library

- Purpose: used for Swiss and Round-Robin pairing generation through the custom Go wrapper in `Scripts/ChessPairingWrapper`
- Upstream: https://github.com/gnutterts/chesspairing
- Version referenced in this repository: `v0.2.2`
- Integration model: built as `chesspairing.dll` and loaded by the .NET application through P/Invoke
- Local wrapper source: `Scripts/ChessPairingWrapper/chesspairing_wrapper.go`
- License: Apache License 2.0
- License text included at: `Docs/ThirdPartyLicenses/chesspairing-APACHE-2.0.txt`
- Distribution note: when distributing builds that bundle `chesspairing.dll`, preserve the required Apache 2.0 copyright and license notices and include the upstream license text

### SQLite3 Multiple Ciphers

- Purpose: provides the SQLite native engine and SQLCipher-compatible database encryption
- Upstream: https://github.com/utelle/SQLite3MultipleCiphers
- Version built in this repository: `2.5.0`
- Source commit: `4bbdff307dd48cf439978659c54c05878022c2cb`
- Integration model: built as `sqlite3mc.dll` by `Scripts/SQLite3MC/build.ps1` and loaded through `SQLitePCLRaw.provider.dynamic_cdecl`
- License: MIT
- License text included at: `Docs/ThirdPartyLicenses/SQLite3MC-MIT.txt`

## NuGet Packages

The application also depends on third-party NuGet packages. These packages are restored through the project files and may carry their own license terms.

### Application packages

- `Microsoft.EntityFrameworkCore.Sqlite.Core` `10.0.0` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt`
- `Microsoft.EntityFrameworkCore.Tools` `10.0.0` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt` - development-time tool package marked `PrivateAssets=all`
- `SQLitePCLRaw.core` `2.1.11` - Apache-2.0 - see `Docs/ThirdPartyLicenses/chesspairing-APACHE-2.0.txt`
- `SQLitePCLRaw.provider.dynamic_cdecl` `2.1.11` - Apache-2.0 - see `Docs/ThirdPartyLicenses/chesspairing-APACHE-2.0.txt`
- `Microsoft.Extensions.Logging` `10.0.0` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt`
- `FreeDataExportsv2` `1.0.200` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt`
- `CsvHelper` `30.0.1` - dual licensed under MS-PL or Apache-2.0 - see `Docs/ThirdPartyLicenses/MS-PL.txt` and `Docs/ThirdPartyLicenses/chesspairing-APACHE-2.0.txt`
- `System.Security.Cryptography.Xml` `10.0.10` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt`
- `Microsoft.Extensions.Localization` `10.0.0` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt`
- `SharpVectors.Wpf` `1.8.0` - BSD-3-Clause - see `Docs/ThirdPartyLicenses/BSD-3-Clause.txt` - upstream third-party notices copied to `Docs/ThirdPartyLicenses/SharpVectors-third-party-notices.txt`
- `Microsoft.Extensions.DependencyInjection` `10.0.0` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt`
- `Microsoft.Extensions.Logging.Console` `10.0.0` - MIT - see `Docs/ThirdPartyLicenses/MIT.txt`

## Recommended Distribution Practice

For released builds, the simplest maintainable approach is:

1. Keep your own license in [LICENSE](LICENSE).
2. Keep this consolidated notice file in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
3. Include the original third-party license texts for bundled or copied components when required by their licenses.
4. Periodically review NuGet package license metadata before each public release.
5. Re-check upstream third-party notice files and native-binary provenance for SQLite3MC before public releases.

## Important Note

This file is a practical notice registry, not legal advice.

The repository currently includes the canonical license texts for the direct license families used by the listed packages in `Docs/ThirdPartyLicenses/`.