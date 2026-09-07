# Funstra Demo 02 validation

7 September 2026 — Windows x64, Unity 6000.4.0f1, Direct3D 11. Release version 0.2.0, A Bed & a Bandage.

## Results

| Check | Result | Evidence |
|---|---|---|
| Final Unity CLI Windows build | Succeeded; 0 errors, 0 warnings | build-result.txt, build.log |
| Existing rule/persistence/navigation checks | 75 assertions passed | rules-result.txt |
| New district simulation rules | 32 assertions passed | district-rules-result.txt |
| Exported-player new scenario | 51 assertions passed, 30 FPS cap | bandage-runtime-result.txt, bandage-full-player.log |
| Exported-player campaign/cargo regression | 88 assertions passed | runtime-result.txt, bandage-legacy-player.log |
| Final rendered capture suite | 12 non-black screens at 1280x720, 30 FPS cap | bandage-visual-result.txt, bandage-visual-player.log, B01 through B12 PNGs |

Total: 107 rule assertions and 139 runtime assertions. Player logs were checked for errors/exceptions/failures; none were found in the passing runs. Updated UI was visually inspected, including the origin, clinic, map, combat target, tactical pause, partnership, history, home, recovery, pause/save help and extraction. Earlier 1600x900 captures were also inspected during development; the final B-series evidence is 1280x720.

## New-rule coverage

Autonomous buyer sale, clinic consumption and paid replenishment without quest acceptance; finite medical stock and local money conservation; consistent scheduled outcomes across update steps; release payment and duplicate refusal; shared shipment ownership across pickup/donation/sale; earned recruitment; finite companion aid; downed-companion rescue; treatment consuming medicine; stock discovery without invented suspect knowledge; identified offenses persisting past immediate pursuit; restitution; solo and companion recovery; penniless fallback after sale/supply depletion; late acquisition from the buyer; world save/load; legacy campaign/cargo migration and original-file backup behavior; rejection of invalid stock.

## New-runtime coverage

Actual character-controller travel to the clinic, collector, case and home; interaction opening conversations; paid hold-to-collect; disappearance of acquired stock; clinic donation and recruitment; companion pathfinding and autonomous bandaging; hold order; tactical pause freezing time; restoration of stock, orders and position; loose-cargo persistence across resume; companion interaction priority at home; extraction interrupted by movement and becoming wanted; successful extraction retry with exactly one payment; actual pistol/melee attack rules, ammo and damage; guard return fire; building occlusion for player and guard; persistent identified grievance; bandages; defeat/recovery and confiscation; downed-person aid; actual unobserved theft and missing-stock audit; persistent resident wounds/reaction; live local gunfight with guard AI active, followed by physical medicine pickup and reload of the resulting state.

## Test boundaries

Tests are scripted player runs, not human keyboard/mouse playtests. UI transaction methods are invoked programmatically; hold interactions and traversal run through gameplay update code. Legacy patrols are frozen during the new scenario to make routes repeatable. New guard/companion AI is isolated where necessary; companion follow/aid and the final local gunfight run with their AI active. The legacy suite separately enables real pursuit, arrest and patrol movement. Its district extension is disabled so historical assertions remain meaningful.

The final 51-assertion gameplay pass includes the combat targeting corrections and current gameplay build. The final build reran all 107 rule assertions, followed by the twelve-screen capture suite. The isolated legacy pass predates the final district-only combat/UI refinements.

A 30 FPS cap is a timing check on this workstation, not evidence from a mid-range computer. No low-end hardware, ultrawide display, long-session balance, human input-pressure testing or full user acceptance has been performed. The earlier Hot Cargo hidden-window timeout is avoided in the new runner by using an actual headless player (`-batchmode -nographics`) rather than a hidden graphical window.

## Remaining design limits

One finite medical dispute and one medical-support companion. No direct companion piloting, companion attacks, detailed body parts, death, custody simulation, faction conquest or reinforcements. Generic equipment shops and repeatable cargo are not part of the finite medical economy. Long-term balance and the value of the partnership are primary questions for the next human review.

## Reproduce

From the project directory:

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/build.log --no-tail
./Tools/Test-Demo.ps1 -Mode Full -ThirtyFPS
./Tools/Test-Demo.ps1 -Mode Legacy
./Tools/Test-Demo.ps1 -Mode Visual -Width 1280 -Height 720 -ThirtyFPS
```

Full and Visual share an isolated test save and must run sequentially. New runtime uses bandage-smoke.json; legacy uses smoke-save.json. The user's district-progress.json and the older progress.json are not used by these tests. The portable package includes assertion result files and the B-series screenshots. Raw machine/build logs remain in the project Evidence directory. Human acceptance of Demo 02 remains pending.

