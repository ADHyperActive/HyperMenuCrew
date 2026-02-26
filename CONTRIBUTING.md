# Contributing to ModMenuCrew

Thank you for your interest in contributing to ModMenuCrew! This document provides guidelines and information to help you get started.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Project Architecture](#project-architecture)
- [Coding Standards](#coding-standards)
- [Submitting Changes](#submitting-changes)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment. Be kind, constructive, and professional in all interactions.

## How Can I Contribute?

### Reporting Bugs

- Use the [Bug Report](https://github.com/MRLuke956/ModMenuCrew/issues/new?template=bug_report.md) issue template
- Include your game version, mod version, and BepInEx version
- Describe the steps to reproduce the bug
- Include any relevant log output from BepInEx console

### Suggesting Features

- Use the [Feature Request](https://github.com/MRLuke956/ModMenuCrew/issues/new?template=feature_request.md) issue template
- Describe the feature and why it would be useful
- Include mockups or examples if possible

### Code Contributions

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-new-feature`)
3. Make your changes following our [coding standards](#coding-standards)
4. Test your changes in-game
5. Submit a Pull Request

## Development Setup

### Prerequisites

| Dependency | Version | Download |
|:-----------|:--------|:---------|
| **.NET SDK** | 6.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/6.0) |
| **BepInEx** | IL2CPP 6 (be.754+) | [builds.bepinex.dev](https://builds.bepinex.dev/projects/bepinex_be) |
| **Among Us** | v17.1.0+ | Steam or Epic Games |

### Building

```bash
# Clone the repository
git clone https://github.com/MRLuke956/ModMenuCrew.git
cd ModMenuCrew

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build --configuration Release
```

The compiled DLL will be in `ModMenuCrew/bin/Release/`.

### Installing for Testing

1. Copy the built DLL to `Among Us/BepInEx/plugins/ModMenuCrew/`
2. Launch Among Us
3. Press **F1** to open the mod menu

## Project Architecture

```
ModMenuCrew/
├── ModMenuCrewPlugin.cs      # Plugin entry point (BepInEx loader)
├── DragWindow.cs             # Draggable IMGUI window system
├── TabControl.cs             # Tab bar for switching content panels
├── MenuSection.cs            # Collapsible sections within tabs
├── MenuSystem.cs             # Alternative section-based menu layout
├── CheatManager.cs           # Cheats tab — toggles, sliders, actions
├── GameCheats.cs             # Static cheat implementations (tasks, vision, teleport)
├── TeleportManager.cs        # Teleportation logic with validation & sync
├── GameEndManager.cs         # Force game end (host-only)
├── SystemManager.cs          # Door control utilities
├── CustomMessage.cs          # Custom RPC messaging system
├── GuiStyles.cs              # Complete IMGUI style/theme system
├── skinunlocked.cs           # Cosmetics unlock via Harmony patches
├── LobbyHarmonyPatches.cs    # Lobby info display & streamer mode
├── VersionShowerPatch.cs     # Version text effects (horror theme)
└── ModMenuCrew.csproj        # Project file
```

### Key Concepts

- **IMGUI**: Unity's Immediate Mode GUI — all UI is drawn every frame via `OnGUI()`
- **Harmony**: Runtime method patching library — used to modify game behavior
- **BepInEx**: Plugin framework for Unity IL2CPP games
- **RPC**: Remote Procedure Calls — used to sync actions between players

### How to Add Features

Every source file contains XML documentation with tutorial comments. Start with:

- **New Tab**: See `ModMenuCrewPlugin.cs` → `InitializeTabsForGameIMGUI()`
- **New Button**: See `CheatManager.cs` class docs
- **New Toggle**: See `ModMenuCrewPlugin.cs` → `DebuggerComponent` class docs
- **New Cheat**: See `GameCheats.cs` class docs
- **New Teleport Location**: See `TeleportManager.cs` class docs
- **UI Styling**: See `GuiStyles.cs` → Public Utility Functions region

## Coding Standards

- **Language**: All code, comments, and documentation must be in **English**
- **XML Docs**: Every public class and method must have `<summary>` documentation
- **Naming**: PascalCase for public members, _camelCase for private fields
- **Style**: Follow the `.editorconfig` in the repository root
- **Error Handling**: Always wrap game API calls in try/catch — the IL2CPP runtime can throw unexpectedly
- **Null Checks**: Always null-check game singletons (`PlayerControl.LocalPlayer`, `HudManager.Instance`, etc.)
- **No Portuguese**: This is an English-only codebase for the showcase

## Submitting Changes

1. Ensure your code compiles without errors
2. Test in-game (both in lobby and during a match)
3. Write clear commit messages: `feat: add radar overlay` or `fix: noclip not resetting on game end`
4. Submit a PR with a clear description of what changed and why
5. Reference any related issues (`Fixes #123`)

## Questions?

- **Discord**: [discord.gg/crewcore](https://discord.gg/crewcore)
- **Website**: [crewcore.online](https://crewcore.online)

---

Thank you for helping make ModMenuCrew better!
