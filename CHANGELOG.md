# Changelog

All notable changes to this project will be documented in this file.
## [1.2.3] - 2024-12-8

## Changed 
- Updated BepInEx.dll to v5.4.22
- Updated JotunnLib and dependencies to v2.22.0

## [1.2.2] - 2024-10-16

### Added
- small fx when fish level gets boosted

## [1.2.1] - 2024-10-15
### Changed

- Fixxed issues with stacked fish in inventory losing their attributes by removing the ability to boost fish after they've been picked up
- Fixxed a bug where fish would lose their attributes due to saving before assigning them

### Removed
- No longer able to boost fish that were already picked up

### Notes
- Some of these issues could be fixxed by making each fish unstuckable but I believe the annoyance with inventory slots would not be worth it for most players.

## [1.2.0] - 2024-10-14

### Added

- Deboosting fish level (if it has been boosted) when failing to catch

### Changed
- Some code cleanups

## [1.1.0] - 2024-10-14

### Added

- Starting using Jotunn Library (now it is a dependency)
- Synced configuration with server
- Configuration updates live with or without config watcher

### Changed

- Logging using bepinex instead of Debug.Logging
- Publicized dll

## [1.0.0] - 2024-10-13

### Added

- Probability to boost the level of a fish while hooking it (once)
- Extra experience gained for catching a fish, depending on bait and level
- Configuration for all  features
- Control over exp gained while reeling a hooked fish

### Changed

- Logging using bepinex instead of Debug.Logging
- Publicized dll
