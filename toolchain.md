# Toolchain — VSCode + .NET SDK + NuGet

Dev environment for Valheim mod development in VSCode. Build via `dotnet build`, BepInEx and AssemblyPublicizer via NuGet, game DLLs from local Valheim installation.

## Tools

### .NET SDK 10.x
Compiler and build system (MSBuild). Required for `dotnet build` and `dotnet restore`.
→ [dot.net](https://dot.net) — install **before** VSCode extensions

### .NET Framework 4.8 Developer Pack
Reference assemblies for the `net48` target. MSBuild needs them to compile — Runtime alone is not enough. Developer Pack includes Runtime, so only one installer needed.
→ [dotnet.microsoft.com/.../net48](https://dotnet.microsoft.com/download/dotnet-framework/net48)
> If VS Build Tools or Visual Studio are installed — Developer Pack is not needed separately.

### VSCode
IDE. C# Dev Kit provides IntelliSense, code navigation, and `dotnet build` integration.
→ [code.visualstudio.com](https://code.visualstudio.com)

**Extensions** (install via `Ctrl+Shift+X` or links below):
- `ms-dotnettools.csharp` [vscode](vscode:extension/ms-dotnettools.csharp) [browser](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) — Roslyn language server (IntelliSense, diagnostics)
- `ms-dotnettools.csdevkit` [vscode](vscode:extension/ms-dotnettools.csdevkit) [browser](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) — C# Dev Kit (build, NuGet GUI; requires free MS account)
- `ms-dotnettools.vscode-dotnet-runtime` [vscode](vscode:extension/ms-dotnettools.vscode-dotnet-runtime) [browser](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.vscode-dotnet-runtime) — C# Dev Kit dependency
- `patcx.vscode-nuget-gallery` [vscode](vscode:extension/patcx.vscode-nuget-gallery) [browser](https://marketplace.visualstudio.com/items?itemName=patcx.vscode-nuget-gallery) — GUI for browsing and installing NuGet packages

### Git / Git Bash
Version control. Git Bash provides a POSIX terminal on Windows.
→ [git-scm.com/download/win](https://git-scm.com/download/win)

### Valheim + BepInExPack_Valheim
Game DLLs (`assembly_valheim.dll`, `UnityEngine.dll`, etc.) are compile-time references — the project resolves them via `VALHEIM_INSTALL`. BepInEx is the modding framework, required by the game at runtime.
→ Valheim via Steam; BepInExPack via [Vortex / Thunderstore](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)

## NuGet — Project Packages

Declared in `.csproj`, downloaded by `dotnet restore`. Not copied to output — players have them from BepInExPack.

| Package | Version | Role |
|---|---|---|
| `BepInEx.Core` | `5.*` | `BepInEx.dll`, `0Harmony.dll` as compile-time refs |
| `BepInEx.Analyzers` | `1.*` | Roslyn analyzers — highlights BepInEx-specific errors in IDE |
| `BepInEx.AssemblyPublicizer.MSBuild` | `0.4.*` | Automatically publicizes `assembly_valheim.dll` at build time |

BepInEx feed (`https://nuget.bepinex.dev/v3/index.json`) declared in `BeehiveUtilities/NuGet.Config` — BepInEx packages are not on nuget.org.

## Valheim Path — `VALHEIM_INSTALL`

`.csproj` resolves game DLLs via this variable. Fallback: `D:\Steam\steamapps\common\Valheim`.

```bash
# Git Bash — per session:
export VALHEIM_INSTALL="/e/OtherPath/Valheim"

# one-time override at build:
dotnet build -c Release /p:VALHEIM_INSTALL="E:\OtherPath\Valheim"
```

## Build

```bash
# Build + auto-deploy to Valheim plugins:
dotnet build -c Release

# Build without deploying:
dotnet build -c Release /p:ValheimCore=""

# Debug build (generates .pdb):
dotnet build -c Debug
```

`Ctrl+Shift+B` in VSCode runs the build via `.vscode/tasks.json`.

- Output: `BeehiveUtilities\bin\Release\net48\BeehiveUtilities.dll`
- Auto-deploy: `$(VALHEIM_INSTALL)\BepInEx\plugins\BeehiveUtilities.dll`

## In-game Diagnostics

Valheim (Mono Unity) does not support the VSCode debugger. Practical options:
- `Plugin.Log.LogInfo(...)` / `LogWarning(...)` — logs to `BepInEx\LogOutput.log`
- Debug build generates `.pdb` — attach via dnSpy is possible but complex
