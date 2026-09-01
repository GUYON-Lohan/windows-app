# Pull Request: Modernize solution to .NET 8 SDK with native ARM64 & x64 support

## Summary of Changes
This pull request modernizes the entire Windows geteduroam solution from legacy .NET Framework 4.7.2 to **modern .NET 8 SDK style (`net8.0-windows10.0.19041.0`)**, introducing first-class native **Windows on ARM64 (`win-arm64`)** support alongside existing **64-bit Intel/AMD (`win-x64`)** architectures.

---

## Verified Hardware & Test Environment

This implementation was compiled, published, and validated natively on real Copilot+ Windows on ARM hardware:

| Property | Value |
| :--- | :--- |
| **Device Model** | Microsoft Surface Pro (11th Edition) |
| **Processor (SoC)** | Qualcomm Snapdragon® X Plus 10-core (X1P64100 @ 3.40 GHz) |
| **Architecture** | Native 64-bit ARM (`ARM64` / `win-arm64`) |
| **Operating System** | Windows 11 Pro Insider Preview (Build 26220.x, 64-bit ARM) |
| **.NET SDK** | .NET 8.0 / 10.0.301 SDK |
| **.NET Desktop Runtime** | `Microsoft.WindowsDesktop.App` 9.0.12 / 10.0.9 (`arm64`) |
| **Execution Verified** | `geteduroam.exe` (WPF GUI), `getgovroam.exe` (WPF GUI), `eduroam-cli.exe` (CLI), `geteduroam.msi` (WiX 4) |

---

## Key Improvements & Technical Details

### 1. Modern SDK-Style Project Conversion
- Replaced legacy verbose MSBuild XML project files with concise modern SDK-style `.csproj` configurations across all projects.
- Added `Directory.Build.props` configuring `<RollForward>LatestMajor</RollForward>` so binaries run seamlessly across .NET 8, 9, and 10 desktop runtimes.

### 2. Elimination of Costura.Fody in Favor of Native Single-File
- Removed `Costura.Fody` IL-weaving dependency.
- Enabled .NET native single-file deployment (`PublishSingleFile=true`, `IncludeNativeLibrariesForSelfExtract=true`) producing lightweight standalone executables.

### 3. Pure C# COM Interop
- Authored `App.Library/Utility/ComInterop.cs` defining `IWshRuntimeLibrary` (`WshShell`, `IWshShortcut`) and `NETWORKLIST` (`INetworkListManager`, `NetworkListManager`).
- Completely removed legacy `<COMReference>` elements, unlocking cross-platform MSBuild and `dotnet` CLI compilation.

### 4. WiX v4 & Native ARM64 MSI Packaging
- Upgraded `App.MsiCreator` to `WixSharp_wix4.bin` (WiX 4.0.5).
- Implemented automatic PE machine header detection (`0xAA64` for ARM64, `0x8664` for x64).
- Configured `%ProgramFiles64Folder%`, `Platform.arm64`, and `InstallerVersion = 500` for native ARM64 MSI packages.

### 5. Encoding & Diacritic Normalization Fix
- Added `System.Text.Encoding.CodePages` (8.0.0) with startup registration of `CodePagesEncodingProvider.Instance`.
- Updated `IdentityProviderParser.cs` with Unicode `NormalizationForm.FormD` fallback to ensure robust diacritic stripping when searching institutions across all .NET runtime versions.

### 6. GitHub Actions CI/CD Pipeline
- Added `.github/workflows/build.yml` automating multi-architecture restoration, building, single-file publishing, and `.msi` artifact generation on Windows runners.

---

## Verification & Testing Matrix

| Component / Target | Architecture | Result |
| :--- | :--- | :--- |
| **EduroamApp.sln Build** | AnyCPU / Multi | Pass (0 Errors) |
| **geteduroam.exe (WPF)** | `win-arm64` (Native ARM64 PE `0xAA64`) | Pass (Tested on Surface Pro 11 Snapdragon X) |
| **getgovroam.exe (WPF)** | `win-arm64` (Native ARM64 PE `0xAA64`) | Pass (Tested on Surface Pro 11 Snapdragon X) |
| **eduroam-cli.exe (CLI)** | `win-arm64` (Native ARM64 PE `0xAA64`) | Pass (Help, Status, Search verified) |
| **geteduroam.exe (WPF)** | `win-x64` (Cross-compilation) | Pass |
| **geteduroam.msi** | ARM64 (`%ProgramFiles64Folder%`) | Pass (WiX v4 output verified) |
| **getgovroam.msi** | ARM64 (`%ProgramFiles64Folder%`) | Pass (WiX v4 output verified) |
| **geteduroam-cli.msi** | ARM64 (`%ProgramFiles64Folder%`) | Pass (WiX v4 output verified) |
