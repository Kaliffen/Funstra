# Funstra — Demo 03: Keep the Lights On

An isometric sandbox RPG prototype in Old Port. **Version 0.3.0**, Windows x64, Unity 6000.4.0f1. Original procedural geometry and sound; no external services required to play.

## Play

Run **Play.cmd** or **Build/KeepTheLightsOn/Funstra.exe**. The portable package is **Releases/Funstra-demo-03-windows.zip**: extract it, then run Funstra.exe alongside its data folder and DLLs.

New players begin with an origin introduction, a pistol, twelve rounds and two bandages. Find **Neri at the cyan REPAIR clinic**. A collector holds six medical doses behind Vico's garage. You can pay, steal, fight, sell the goods elsewhere or ignore the dispute. The clinic, supplier and collector keep transacting without a quest acceptance.

Helping the clinic earns Neri's trust and a recruitable companion who can follow, hold, retreat and stabilize you. After earning the partnership, invest $120 at the clinic to repair a shared refuge: $60 buys repairs and $60 goes into the clinic fund. Recover there without new debt, contribute funds, buy remaining supplier stock, and choose whether to reserve the last two doses for the crew. Public care and Neri's response follow that choice. Mara's original three jobs and repeatable purple cargo remain available to fund your decisions. Press **L** to switch guidance between the clinic and Mara.

[Demo details and review guide](KEEP-THE-LIGHTS-ON.md) · [Vision](VISION.md) · [World origin](WORLD.md) · [Validation](Evidence/VALIDATION.md)

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
| F | Supplies at home; shared refuge at the clinic |
| P | Pet Tally beside the clinic |
| Tab / J / L | Map / history / switch tracked story |
| F5 | Save current world |
| Escape | Pause / leave a menu |
| Mouse wheel / F11 / M | Zoom / fullscreen / sound |

## Saves and previous builds

This demo writes `%USERPROFILE%\AppData\LocalLow\Funstra\Funstra\lights-progress.json`. F5, ten-second autosaves, transactions and normal quitting preserve world state, carried goods, wounds, actor positions, companion orders and knowledge. There is no offline simulation. An abrupt process kill can lose changes since the last checkpoint.

If no Demo 03 save exists, Continue imports `district-progress.json` from Demo 02, or `progress.json` from Hot Cargo. Progress, carried goods and existing relationships are preserved. Characters saved inside newly solid objects are placed beside them. **The original save remains untouched.** Starting a new night replaces only the new demo's save after confirmation. Isolated automated tests use `bandage-smoke.json` and `smoke-save.json`.

Demo 02 remains at `Build/BedAndBandage/Funstra.exe` and in `Releases/Funstra-demo-02-windows.zip`. Hot Cargo remains at `Build/Funstra.exe`, through `Play-Hot-Cargo.cmd`, and in `Releases/Funstra-hot-cargo-windows.zip`.

## Build and test

Use the installed Unity CLI:

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/build.log --no-tail
```

`Tools/Build.ps1` and the Unity Editor's Funstra build menu also work. The output is `Build/KeepTheLightsOn/Funstra.exe`. Building requires an active Unity Editor license.

```powershell
./Tools/Test-Demo.ps1 -Mode Full -ThirtyFPS
./Tools/Test-Demo.ps1 -Mode Legacy
./Tools/Test-Demo.ps1 -Mode Visual -Width 1280 -Height 720 -ThirtyFPS
```

Full and Legacy use real headless players by default; add `-Visible` to watch. Visual mode opens a visible player and captures staged screens. Run Full and Visual sequentially because they share an isolated test save. See the validation report for exactly what each pass covers.

Four cars follow street routes, reserve junction access, yield to people and stop during menus. Physics and navigation share solid bins, counters, beds, crates and moving vehicle footprints.

## Release and website

Funstra is MIT licensed and developed in the open. Every finished demo is published as a GitHub Release, and the download page rebuilds itself from that release list:

```powershell
pwsh Tools/Release.ps1 -Name "Keep the Lights On" -NotesFile KEEP-THE-LIGHTS-ON.md
```

That one command builds the player, verifies it is complete, zips the portable package, publishes a tagged release and prunes to the newest five. Publishing fires two workflows: `release-check.yml` rejects a release with no playable zip or no notes, and `site.yml` regenerates **https://kaliffen.github.io/Funstra/** from the live releases and deploys it to GitHub Pages. Nobody edits the download page by hand.

The site source is `site/` — prose in `site/content/site.json`, styling in `site/assets/styles.css`, generated by `node site/build.mjs`. Full description of the cycle: [Docs/PIPELINE.md](Docs/PIPELINE.md).

The Unity build stays local because a batch-mode build needs a licensed editor; CI takes over as soon as the artifact exists.

## Source map

- `RefugeState.cs` / `FunstraRefuge.cs`: shared refuge, clinic funding/policy, reactive dialogue and Tally.
- `CityTraffic.cs`: moving traffic, junction access and pedestrian yielding.
- `FunstraRefugeTests.cs` / `RefugeRules.cs`: new exported-player and rule coverage.
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
