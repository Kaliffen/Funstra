# Demo 04 development-candidate validation — Streets Worth Fighting For

Version **0.4.0**, Windows x64, Unity **6000.4.0f1**. Candidate: `Build/StreetsWorthFightingFor/Funstra.exe`. Work is tracked in release [#27](https://github.com/Kaliffen/Funstra/issues/27) and environment [#37](https://github.com/Kaliffen/Funstra/issues/37).

**The owner holds review and publication until the slice is complete. No Demo 04 panel/dossier or owner approval is recorded. This report is development evidence, not a release or acceptance claim.** Demo 03's original report is preserved in [Demo03-archive/VALIDATION.md](Demo03-archive/VALIDATION.md).

## Integrated candidate identity

`Funstra_Data/Managed/Assembly-CSharp.dll` SHA-256:

`0E487EF0DA5202C4F644BE0FF990543FA541101A6CFD71700CCD783B73E7C835`

Built 8 September 2026 at 00:34 UTC. This assembly includes the gate-post separation repair, full-bar stamina recovery, residential home/street changes and recorded audio replacement. On resumption, no source or resource assets were newer than the successful build. Foundation and Environment evidence already matched this assembly; those passing runs were preserved.

Each runtime row below requires its `build-identity.json` to match this DLL hash, a test start after the assembly write time, and a subsequent PASS result. Earlier files remain on disk; their mere presence does not qualify them for this table. Unity's executable hash alone cannot identify changed gameplay code.

## Integrated checks

This Unity build succeeded with **zero errors and zero warnings** (`build-result.txt`, `demo04-build.log`). Editor checks: 75 core, 32 district, 33 refuge, 1,910 spatial, 72 clinic, 39 squad and 25 combat; **2,186 total**. The spatial checks include separated overlapping ground top faces, gate-jamb/wall face separation and no collision ledges on visual ground overlays. Clinic checks cover reachable entrances, collision and cutaway behavior.

Runtime snapshot (serialized visible runs at 1280×720, 30 FPS cap):

| Route | Current candidate status | Reported PASS checks | Fresh PNGs | Result under Evidence |
|---|---|---:|---:|---|
| Foundation 1 | PASS | 23 | 26 | `Foundation/level-1/foundation-1-result.txt` |
| Foundation 2 | PASS | 28 | 15 | `Foundation/level-2/foundation-2-result.txt` |
| Foundation 3 | PASS | 14 | 27 | `Foundation/level-3/foundation-3-result.txt` |
| Foundation 4 | PASS | 18 | 14 | `Foundation/level-4/foundation-4-result.txt` |
| Environment | PASS | 40 | 33 | `Demo04/environment/environment-runtime-result.txt` |
| Streets | PASS | 143 | 4 | `Demo04/streets/streets-runtime-result.txt` |
| Legacy | PASS | 90 | 10 | `Demo04/legacy/runtime-result.txt` |
| Visual | PASS | 2 | 24 | `Demo04/visual/bandage-visual-result.txt` |

All eight routes have passing reports and captures newer than their test start, with matching assembly identity and no exception/error matches in the final player logs. The machine-readable audit is [Demo04/validation-summary.json](Demo04/validation-summary.json). Counts include route/capture completion checks; they are not all independent gameplay assertions. Fresh PNG counts establish recorded evidence, not visual quality or human control feel.

## What the routes exercise

Foundation 1–4 are isolated prepared levels using production controller movement, weapons, timed projectiles and squad rules. Fixture placement, disposable equipment and explicit actor injuries are identified in results; these are guided scenarios, not exploratory input. Foundation 1 measures real controller displacement while sprint remains held near exhaustion: `Foundation/level-1/F01-sprint-recovery.csv` records each step's stamina and horizontal speed. The current result reports exactly two speed transitions, a 197-frame recovery walk and a 15-frame resumed run segment at 30 Hz. The player walks until the full stamina bar recovers, then resumes sustained sprint. Human handling approval remains separate.

Environment travels through the expanded normal district, enters the south clinic doorway, reaches Neri and the connected medicine store, exits east, and visits the church forecourt, market/quay, workshop and residential streets. It freezes district AI and removes the yard squad for the architectural walkthrough. Nine staged views and a 24-frame `E-motion-00`–`23` sequence support inspection of movement, ground surfaces and cutaways; confirm their current identity before using them. Ground-face geometry assertions cannot alone rule out visible flicker.

Streets combines inherited medical/refuge checks with campaign traversal, gates, traffic obstruction/recovery and a live yard encounter. Its profiling route suspends yard AI during traversal; the separate encounter engages live guards. The obstruction test first exercises actual Update braking/pause, then explicitly advances three minutes of traffic and district transactions while the gameplay loop is paused, clears the blocker and verifies recovery without resetting the car. The two traffic screenshots retain the pause panel, which obscures the lane; their presence does not prove traffic readability. The live-yard capture shows the player wounded by an actual traveling enemy shot and the result verifies controller withdrawal. Explicit state setup and controller travel are distinguished in its result. Legacy exercises Mara/cargo with the medical extension disabled. Visual stages the B/C-series panels and streets; it does not click every GUI control. These routes run in exported players and do not replace four independent reviewer judgments.

## Presentation and performance limits

The candidate replaces flat exterior blocks with imported architectural meshes, expands the district and adds a continuous two-door clinic with collision and camera cutaways. Compact street HUD and F2 details are documented in the [guide](../STREETS-WORTH-FIGHTING-FOR.md). On resumption, direct image inspection covered the home entry, clinic treatment/store/receiving lane, church, market/quay, workshop and residential views; both close gate views; front-gate motion samples 00/03/05; environment motion samples 00/12/23; and foundation weapon/coordination/combined-approach views. The clinic rooms and player remain readable in these views, and the sampled gate/ground faces show no visible overlap pattern. This is sampled frame inspection, not continuous video playback or human movement/immersion acceptance. Large diagnostic signs and some character labels overlap in the prepared levels; those levels still need owner readability/feel judgment.

All 14 recorded audio files match `Assets/Resources/Audio/provenance.json`. Current exported foundation checks confirm distinct firearm clips, four footstep variants and no looping placeholder drone. This verifies asset loading and identity, not listening approval of report, timing or mix.

Final UI inspection covered the title's four test choices, expanded district map, refuge management and completed Mara standing. Title/management text is readable in the sampled captures. The map still has a presentation issue: the combat HUD and toast overlap its header, and nearby point labels collide. These remain visible limitations for the owner/review pass; two successful Visual capture checks do not settle UI quality. No Demo 04 panel verdict or owner acceptance is implied.

Available hardware is **NVIDIA GeForce RTX 4090 / Intel Core i9-14900KF / Direct3D 11**. The matching Streets profile has 1,643 capped rendered-frame samples: p50 33.334 ms, p95 33.340 ms, maximum 49.082 ms; endpoint Unity allocation 111,399,016 bytes, 18 actors and four cars. These are frame intervals and endpoint allocation, not isolated CPU/GPU timings or minimum specifications. The profile observed **two Funstra processes**: an older `Build/Funstra.exe` instance was subsequently identified and stopped after owner-reported audio. This sample is not an isolated benchmark; no lower-tier hardware or matched Demo 03 performance comparison is claimed.

## Reproduce and preserve state

Run from the project directory using the installed Unity CLI and licensed editor. Run suites sequentially; they share a validation mutex and fixture save paths. Use visible players for rendered checks: a hidden-window Streets attempt reached the traffic assertions but failed its black-screenshot guard, then passed when rerun visibly. Mute test players as requested by the owner and check for stray game instances before testing/profiling.

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/demo04-build.log --no-tail --non-interactive
pwsh Tools/Test-Foundation.ps1 -Level 0 -Visible -ThirtyFPS
pwsh Tools/Test-Streets.ps1 -Mode Environment -Visible -ThirtyFPS
pwsh Tools/Test-Streets.ps1 -Mode Streets -Visible -ThirtyFPS
pwsh Tools/Test-Streets.ps1 -Mode Legacy -Visible -ThirtyFPS
pwsh Tools/Test-Streets.ps1 -Mode Visual -Visible -ThirtyFPS
```

Normal play uses `streets-progress.json`; older save files import without overwriting the originals. Prepared foundation levels do not save campaign state. Guided runtime fixtures are separate from normal saves, and conflicting runs must stay serialized. Raw local player logs supplement the reports but are excluded from the portable package.

`Tools/Package-Demo04.ps1` requires the explicit tested DLL identity, all four foundations, Environment/Streets/Legacy/Visual, the guide, validation and complete panel records with one Demo 04 dossier. No packaging or publication is performed by this report. The integrated validation checkpoint is complete. The owner-directed review/publication hold remains recorded in #27/#37; environment, movement, weapon feedback and sound acceptance remain open, along with the presentation/performance limits above. All test players and the identified older stray instance were stopped at handoff.


## Police response increment � 8 September 2026

Owner approved the preceding Demo04 candidate and authorized this next playable increment (#39). Separate build: `Build/PoliceResponse/Funstra.exe`, version 0.4.1. Current candidate assembly SHA-256: `3CBCA4699C37181628C9167712F394FBAB2F125E1E1845AE069A51C2EC0A8BBC`. Approved Demo04 assembly remains `0E487EF0DA5202C4F644BE0FF990543FA541101A6CFD71700CCD783B73E7C835` in its original build folder.

The export passed 2,201 editor checks (the preceding 2,186 plus 15 police knowledge/dispatch/persistence checks), with zero build errors/warnings. `Evidence/PoliceResponse/police-final` passes 35 guided checks, including actual Update return fire, separate controlled logistics/persistence fixtures and four rendered captures. `streets-final` passes the integrated Streets regression on the same hash. The final Legacy route passes 90 checks; Streets passes 143. All three core routes total 268 PASS rows on the same hash. Four independent fresh Police routes each pass 35 checks and render four screenshots. Dag, Priya, Marcus and Nell each scored this increment 7/10, with no focused-route blocker. Original records and the rendered-inspected HTML dossier are linked from `Docs/funstra-review-dossier-police-response.html`; follow-up #40 owns live nine-officer pressure/readability before rifle scaling, and #22 owns contextual recovery/aftermath. The dossier was rendered with an isolated headless Chrome profile because the browser connector had no available browser; the saved full-page capture was inspected.

During integration, fixed empty-ammunition police holding forever, stale truck requests activated by later unidentified noise, recovery saving stale patrol positions, and the new traffic yield rule incorrectly stopping cars at old roadside props. The earlier `police` and `streets` folders retain pre-fix evidence; their results do not identify the final candidate. The final tests remain process-muted with `--mute-tests` and use isolated saves.

The first tier has three patrols plus two trucks with three officers each. Logistics are explicitly stepped at 30Hz with combat omitted for dispatch inspection; the subsequent live fixture observes one patrol shooting and wounding the player. This does not prove collective nine-officer difficulty or the future 32-soldier maximum. Captures show actual truck entry, deployed crews, live-fire injury and recovery, paused for inspection. Trucks remain parked after the search. Existing tactical-pause/map overlays remain imperfect. No sound audition or minimum-spec hardware claim.
