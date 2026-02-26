# Changelog

All notable changes to ModMenuCrew are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [6.0.8] — 2026-02-25

### Added
- **Anti-OVERLOAD**: MMC users are now immune to connection drops caused by Premium users spamming the OVERLOAD feature. Everyone else disconnects — MMC users stay in the game.
- **Reveal Votes**: New toggle in the Settings tab to see who voted for whom during meetings.

### Fixed
- Fixed Discord Login button failing to open on some outdated Windows versions.
- Fixed critical SSL errors across all Linux and macOS builds (Wine/Proton).
- Fixed Satellite View not resetting properly after a match ends.
- Fixed lobby chat bug where setting Satellite View above `3.1f` prevented the user from typing.

### Changed
- Updated BepInEx to build `754` for improved game compatibility and performance.
- Deployed general stability and security improvements for all MMC users.
- Rolled out backend refinements to the website and free API for better stability.

---

## [6.0.6a] — 2026-01-15

### Added
- **Replay System**: Record, replay & analyze every game with full playback controls (Exclusive).
- **New Spoofing**: Enhanced spoofing options + Detective role override.
- **Door Control**: Close all doors permanently, open them individually.
- **Unlock All**: Unlock all Skins, Pets, Hats, Visors & Cosmicubes (Local).
- **Level Spoof**: Set your level to any number (0–999).
- **Platform Spoof**: Appear as playing on Mobile, Xbox, PlayStation, or Switch.
- **Game End Manager**: Force Win/Loss — instantly end game with any reason (Host).
- **God Mode**: Invincible Host — automatically reapplies protection.
- **Hide MMC Star**: Option to remove the ★ prefix from names.
- **Redesigned UI**: Completely modern visual overhaul for 2026.
- **Radar + Skeld**: Real-time Mini-Map Radar with player tracking.
- **MMC User Finder**: Identify other ModMenuCrew users in lobby.
- **Phantom Mode**: Kill while invisible & vanish (Premium).
- **Event Logger**: Live log of kills, tasks, vents, and sabotages.
- **Custom Hotkey**: Change menu key in settings (default: F1).

### Fixed
- Fixed free key sessions dying mid-game (token mismatch & anti-replay false positives).
- Fixed menu randomly closing during a session.
- Fixed kill cooldown not resetting properly.
- Fixed satellite view not resetting after game ends.
- Fixed satellite view scroll zoom not updating the UI value.
- Fixed menu window getting stuck off-screen on resolution change.
- Fixed silent session denial on lobby entry.
- Fixed radar crash on enable.
- Fixed GMT time zone error preventing menu from opening.

### Changed
- Improved overall security and anti-bypass protections.
- Improved window position clamping — fully customizable, error-proof recovery.
- Full support for all screen resolutions (1080p, 2.5K, 4K+).
- Significant performance optimizations across the board.

---

## [5.4.0] — 2025

### Added
- Initial showcase release with core features.
- IMGUI-based mod menu with tabs, toggles, and buttons.
- Speed hack, noclip, teleportation, infinite vision.
- Task completer, meeting controls, sabotage controls.
- Cosmetics unlock (all skins, hats, pets, visors).
- Custom RPC messaging system.
- Draggable window with minimize/close.
- Lobby info patches with mod detection and streamer mode.
- Horror-themed version shower effects.

---

[6.0.8]: https://github.com/MRLuke956/ModMenuCrew/releases/tag/6.0.8
[6.0.6a]: https://github.com/MRLuke956/ModMenuCrew/releases/tag/6.0.6a
[5.4.0]: https://github.com/MRLuke956/ModMenuCrew/releases/tag/5.4.0
