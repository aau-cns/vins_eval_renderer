# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] Public release
### Added
- AAU-CNS License file.
- License to README.md
- Citation code to README.md

### Changed
- License header in each source file.

## [0.2.1] Illumination Hotfix
### Changed
- Removed hardcoded seed. Unity should use the system time per default to calculate seed.

### Fixed
- Internal state update was not called, hence illumination not updated (#17).

## [0.2.0] Centralization Update
### Added
- Interpretation of feature noise levels.
- Various new rock:
  - [Rock Package](https://assetstore.unity.com/packages/3d/props/exterior/rock-package-118182)
  - [Rocks](https://assetstore.unity.com/packages/3d/props/exterior/rocks-604)
  - [HQ Rocks](https://assetstore.unity.com/packages/3d/props/exterior/hq-rocks-pack-668959)
  - [Rocks Free](https://assetstore.unity.com/packages/3d/props/exterior/rocks-free-pack-98219)
  - [Stones Free](https://assetstore.unity.com/packages/3d/environments/fantasy/pbr-exterior-pack-5-stones-117126)
  - [Free Rocks](https://assetstore.unity.com/packages/3d/environments/landscapes/free-rocks-19288)
- Cylindrical takeoff region where no objects are places (around the origin).
- Added prefabs to git repo.
- Added new prefabs, such as trees and stones.
- Generic interpretation for noise.
- Generic interpretation for illumination.
- Illumination changer class.
- GitLab issue templates.
- Minimap camera which can be displaced in test mode.
- Added textures from [https://www.the3rdsequence.com/texturedb/?p=26] (all resolutions).

### Changed
- Changed to Unity 2019.4.12f1.
- Changed ground size to 60x60m^2. (Hardcoded see #7).
- Updated the illumination object to be from (0,0,100) and super soft shadows.
- Updated used objects to only include convex ones.
- Changed GitLab Templates.
- Naming conventions of variables.
- Updated scene parameter object which will be used to generate object distribution.
  They can now be set from the ROS Bridge (#5).
- Minimal area is set to 0.1m^2 for object placement (change this for huge scenes, hardcoded for now).
- Restructured folder structure.
- Changed ground size to 20x20m^2. (Hardcoded see #7).
- New object placing scheme, to use all sizes of unused placed size.
- Added scene parameter object which will be used to generate distirbution.
  These are hardcoded for now, in furture release this can be set from the ROS Bridge (#5).
- Removed `Rocks and Bolders 2` from loading, due to render issue (#14).

### Fixed
- Prefab placement was calculated wrongly for height.
- Objects are now placed above ground, not at object origin=0.
- Object placement was not centered in origin but topleft (#4).
- Removed the hardcoded noise/distribution to the scene parameters.
- Fixed issue with illumination using system time instead of sim time (#13).

### Removed
- Removed unneeded texture generation of perlin noise.
- Removed old prefab placement (generation).

## [0.1.0] Generic Scene
### Added
- Added and fixed the flightgoggles scripts to this project.
- Added CNS internal scripts developed by @hardtstremal.
- Added generic scene manager.

### Changed
- Changed flightgoggles state messages to meet CNS requirements.

### Removed
- Removed IR Markers from Unity Camera Scripts.

[1.0.0]: https://github.com:aau-cns/vins_eval
