<#
.SYNOPSIS
Downloads and builds the x64 SQLite3 Multiple Ciphers native DLL used by Local Knights.

.DESCRIPTION
This script downloads a tagged SQLite3MC release, compiles it with MinGW GCC, and writes
the resulting sqlite3mc.dll to:

    Scripts\SQLite3MC\runtimes\win-x64\native\sqlite3mc.dll

The DLL is built with SQLCipher as the default codec and SQLCipher version 4 compatibility.
Local Knights loads this DLL through SQLitePCLRaw.provider.dynamic_cdecl. Directory.Build.targets
automatically copies it to application and test output directories and includes it when publishing.

The script replaces an existing sqlite3mc.dll in the output directory. Downloaded source code is
stored in a uniquely named temporary directory and removed when the build finishes or fails.

.PARAMETER Version
SQLite3MC release number to download. The script clones the matching v<Version> Git tag.

.PARAMETER GccPath
Command name or full path of the x64 MinGW gcc.exe compiler. Defaults to gcc.exe on PATH.

.NOTES
Prerequisites:

1. Windows PowerShell 5.1 or PowerShell 7.

2. Git for Windows, available from:
   https://git-scm.com/download/win

   The git.exe command must be available on PATH.

3. A 64-bit MinGW-w64 GCC toolchain. Either of these distributions can be used:
   - WinLibs: https://winlibs.com/
   - MSYS2: https://www.msys2.org/

   For MSYS2, install the UCRT64 GCC package from an MSYS2 UCRT64 terminal:

       pacman -S --needed mingw-w64-ucrt-x86_64-gcc

   Add the compiler's bin directory to PATH, or pass the full gcc.exe path through -GccPath.
   The script verifies that GCC targets x86_64 before compiling.

4. Internet access to clone the requested release from:
   https://github.com/utelle/SQLite3MultipleCiphers

When upgrading SQLite3MC:

1. Review the upstream release notes and confirm that the release contains a SQLite version that
   fixes all advisories relevant to the application.
2. Run this script with the new release number, for example:

       .\Scripts\SQLite3MC\build.ps1 -Version 2.6.0

3. Record the version, source commit, and printed SHA-256 in Scripts\SQLite3MC\README.md.
4. Update THIRD-PARTY-NOTICES.md if the version, license, or upstream attribution changed.
5. Run the complete test suite. The encryption tests verify encrypted file creation, correct and
   incorrect password handling, and password changes through PRAGMA rekey.
6. Publish the Windows application and confirm sqlite3mc.dll is beside the executable.
7. Run the NuGet vulnerability audit and confirm no SQLitePCLRaw native bundle packages were added.
8. Commit the regenerated DLL together with the updated provenance documentation.

Do not remove CODEC_TYPE_SQLCIPHER, SQLCIPHER_VERSION_DEFAULT=4, or the x64 architecture check
without also updating and retesting the application's database connection behavior.

If script execution is disabled for the current PowerShell process, run:

    Set-ExecutionPolicy -Scope Process Bypass

Then invoke the script normally. Administrator privileges are not required.

.EXAMPLE
Run from the repository root using the default pinned version:

    .\Scripts\SQLite3MC\build.ps1

.EXAMPLE
Build a specific SQLite3MC release:

    .\Scripts\SQLite3MC\build.ps1 -Version 2.5.0

.EXAMPLE
Use a MinGW compiler that is not on PATH:

    .\Scripts\SQLite3MC\build.ps1 `
        -GccPath "C:\ProgramData\mingw64\mingw64\bin\gcc.exe"
#>

param(
    [string]$Version = "2.5.0",
    [string]$GccPath = "gcc.exe"
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$outputDirectory = Join-Path $PSScriptRoot "runtimes\win-x64\native"
$outputPath = Join-Path $outputDirectory "sqlite3mc.dll"
$sourceDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "SQLite3MultipleCiphers-$Version-$([Guid]::NewGuid().ToString('N'))"

try {
    $compiler = Get-Command $GccPath -ErrorAction Stop
    $targetArchitecture = & $compiler.Source -dumpmachine
    if ($LASTEXITCODE -ne 0 -or $targetArchitecture -notmatch "x86_64") {
        throw "An x64 MinGW GCC compiler is required. Detected target: '$targetArchitecture'."
    }

    Write-Host "Downloading SQLite3MC v$Version..."
    & git clone --quiet --depth 1 --branch "v$Version" "https://github.com/utelle/SQLite3MultipleCiphers.git" $sourceDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to download SQLite3MC v$Version."
    }

    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

    $compilerArguments = @(
        "-shared"
        "-O2"
        "-s"
        "-I$sourceDirectory\src"
        "-I$sourceDirectory\src\aegis\include"
        "-I$sourceDirectory\src\argon2\include"
        "-DSQLITE_API=__declspec(dllexport)"
        "-DCODEC_TYPE=CODEC_TYPE_SQLCIPHER"
        "-DSQLITE3MC_USE_SQLCIPHER_LEGACY=1"
        "-DSQLCIPHER_VERSION_DEFAULT=4"
        "-DSQLITE_THREADSAFE=1"
        "-DSQLITE_DEFAULT_FOREIGN_KEYS=1"
        "-DSQLITE_DQS=0"
        "-DSQLITE_MAX_ATTACHED=10"
        "-DSQLITE_SECURE_DELETE=1"
        "-DSQLITE_ENABLE_COLUMN_METADATA=1"
        "-DSQLITE_ENABLE_FTS3=1"
        "-DSQLITE_ENABLE_FTS3_PARENTHESIS=1"
        "-DSQLITE_ENABLE_FTS4=1"
        "-DSQLITE_ENABLE_FTS5=1"
        "-DSQLITE_ENABLE_RTREE=1"
        "-DSQLITE_ENABLE_GEOPOLY=1"
        "-DSQLITE_ENABLE_MATH_FUNCTIONS=1"
        "-DSQLITE_USE_URI=1"
        "-DSQLITE_TEMP_STORE=2"
        "-DSQLITE3MC_SECURE_MEMORY=1"
        (Join-Path $sourceDirectory "src\sqlite3mc.c")
        "-o"
        $outputPath
    )

    Write-Host "Building x64 SQLite3MC with SQLCipher v4 defaults..."
    & $compiler.Source @compilerArguments
    if ($LASTEXITCODE -ne 0) {
        throw "SQLite3MC compilation failed with exit code $LASTEXITCODE."
    }

    $commit = & git -C $sourceDirectory rev-parse HEAD
    $hash = (Get-FileHash $outputPath -Algorithm SHA256).Hash
    Write-Host "Built: $outputPath"
    Write-Host "Source: SQLite3MC v$Version ($commit)"
    Write-Host "SHA-256: $hash"
}
finally {
    if (Test-Path $sourceDirectory) {
        Remove-Item $sourceDirectory -Recurse -Force
    }
}