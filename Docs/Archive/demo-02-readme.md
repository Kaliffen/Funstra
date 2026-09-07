# Funstra — Demo 02: A Bed & a Bandage

An isometric sandbox RPG prototype in Old Port. **Version 0.2.0**, Windows x64, Unity 6000.4.0f1. Original procedural geometry and sound; no external services required to play.

## Play

Run **Play.cmd** or **Build/BedAndBandage/Funstra.exe**. The portable package is **Releases/Funstra-demo-02-windows.zip**: extract it, then run Funstra.exe alongside its data folder and DLLs.

New players begin with an origin introduction, a pistol, twelve rounds and two bandages. Find **Neri at the cyan REPAIR clinic**. A collector holds six medical doses behind Vico's garage. You can pay, steal, fight, sell the goods elsewhere or ignore the dispute. The clinic, supplier and collector keep transacting without a quest acceptance.

Helping the clinic earns Neri's trust and a recruitable companion who can follow, hold, retreat and stabilize you. That partnership is this demo's first step in the power arc. Mara's original three jobs and repeatable purple cargo remain available to fund your decisions. Press **L** to switch guidance between the clinic and Mara.

[Demo details and review guide](../../BED-AND-BANDAGE.md) · [Vision](../../VISION.md) · [World origin](../../WORLD.md) · [Validation](../../Evidence/VALIDATION.md)

## Controls

| Control | Action |
|---|---|
| WASD / arrows | Move relative to the camera |
| Shift | Sprint |
| Ctrl / C | Sneak; use green-bin cover when unseen |
| E | Talk / stabilize a downed person; hold to take goods or bank cargo at home |
| Right mouse | Select a person for combat |
| Left mouse | Attack selected person, with cursor over the central world view |
| 1 / 2 | Fists / pistol |
| B | Use a bandage on yourself |
| Space | Tactical pause; select a target or issue companion orders |
| G / H / R / T | Neri: follow / hold / retreat / aid |
| Y | Surrender near a hostile Rook |
| F | Open supplies and recovery at home |
| Tab / J / L | Map / history / switch tracked story |
| F5 | Save current world |
| Escape | Pause / leave a menu |
| Mouse wheel / F11 / M | Zoom / fullscreen / sound |

## Saves and previous builds

This demo writes `%USERPROFILE%\AppData\LocalLow\Funstra\Funstra\district-progress.json`. F5, ten-second autosaves, transactions and normal quitting preserve world state, carried goods, wounds, actor positions, companion orders and knowledge. There is no offline simulation. An abrupt process kill can lose changes since the last checkpoint.

If no new-demo save exists, Continue can import `progress.json` from Hot Cargo, preserving its jobs, cash, perks and satchel. **The original save remains untouched.** Starting a new night replaces only the new demo's save after confirmation. Isolated automated tests use `bandage-smoke.json` and `smoke-save.json`.

The previous playable build remains at `Build/Funstra.exe`, through `Play-Hot-Cargo.cmd`, and in `Releases/Funstra-hot-cargo-windows.zip`.

## Build and test

Use the installed Unity CLI:

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/build.log --no-tail
```

`Tools/Build.ps1` and the Unity Editor's Funstra build menu also work. The output is `Build/BedAndBandage/Funstra.exe`. Building requires an active Unity Editor license.

```powershell
./Tools/Test-Demo.ps1 -Mode Full -ThirtyFPS
./Tools/Test-Demo.ps1 -Mode Legacy
./Tools/Test-Demo.ps1 -Mode Visual -Width 1280 -Height 720 -ThirtyFPS
```

Full and Legacy use real headless players by default; add `-Visible` to watch. Visual mode opens a visible player and captures staged screens. Run Full and Visual sequentially because they share an isolated test save. See the validation report for exactly what each pass covers.

## Source map

- `DistrictState.cs`: persistent medical economy, actors, incidents, trust, wounds, recovery and ownership.
- `FunstraDistrict.cs`: combat, sight, NPC navigation, companion orders, interactions and new world art.
- `FunstraDistrictUI.cs`: introduction, people, tactical pause, journal and preparation screens.
- `RunState.cs`: campaign/cargo progression, versioned persistence and migration.
- `FunstraGame.cs` / `FunstraUI.cs`: shared controls, simulation, camera, sound and interface.
- `CargoRun.cs` / `FunstraCargo.cs`: legacy optional cargo and home banking.
- `CityArt.cs` / `CityNavigation.cs` / `TownAgent.cs`: city, paths and residents/police.
- `FunstraSmoke.cs` / `FunstraDistrictTests.cs`: opt-in exported-player validation.
- `Assets/Editor/FunstraBuild.cs` / `DistrictRules.cs`: build and rule assertions.

## Scope

One district, one finite medical dispute, one recruitable support companion, fists and one pistol, incapacitation and bleeding, small named-party consequences, and the original campaign/cargo. The medicine economy is finite; general shops and the rest of the city do not have a complete supply simulation. No interiors, driving, character death, custody system, faction conquest, skill-training system or large organization management yet. Those longer-term ambitions are separated from current functionality in DESIGN.md.
