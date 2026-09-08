# Demo 03 validation — Keep the Lights On

Review candidate: **0.3.0**, Windows x64, Unity **6000.4.0f1**. Build: `Build/KeepTheLightsOn/Funstra.exe`. Portable archive: `Releases/Funstra-demo-03-windows.zip`.

## Verification

| Check | Result |
|---|---|
| Unity CLI build | Succeeded, zero errors, zero warnings |
| Existing editor rules | 75 passed |
| Medical simulation editor rules | 32 passed |
| New refuge, dialogue, routes and vehicle editor rules | 33 passed |
| Exported-player medical/refuge/street suite, 30 FPS cap | 78 passed |
| Exported-player legacy campaign/cargo suite | 90 passed |
| Rendered capture pass | 24 non-black screens at 1280×720, 30 FPS cap |

Totals: **140 editor assertions and 168 exported-player assertions**. Two visual completion checks are not counted as gameplay assertions. Packaging refuses runtime/visual results older than the final managed assembly; `demo-03-build-info.txt` identifies that assembly by SHA256.

## Publication integration — 8 September 2026

Rebuilt with the installed Unity CLI after the four Demo 03 reviews. Gameplay rules are unchanged by this integration pass. Seven new editor checks prove public treatment above the reserve threshold, exact refusal at two doses without a charge, and resumed treatment after reopening. Two new legacy assertions and captures show RUNNER after actual first-job delivery and CONNECTED after the completed campaign and reload. These are in `13-mara-standing-runner.png` and `14-mara-standing-connected.png`.

The initial hidden-window legacy capture failed with a black screenshot. The visible replay passed; `legacy-hidden-attempt.log` retains the failed attempt locally. Final Full, Legacy and Visual logs describe the passing artifact. The full original reviewer reports and four 8/10 scores are preserved; `reviewer-agents/demo03-integration-replay.md` is a targeted inspection of this CD-run evidence, not four new playthroughs.

The website and dossier passed generator, HTML structure and local link checks. A browser preview was unavailable in this session, so no new browser-render inspection is claimed. Only an RTX 4090 test machine is available. Low-end performance remains a target; another physical machine is not a release requirement.

## Coverage

New editor checks cover the refuge relationship gate, split repair/fund payment, duplicate-purchase prevention, finite supply purchases, medicine and money conservation, debt-free recovery, bleeding and betrayal restrictions, public care versus crew reserve, refusal-dependent dialogue and clinic status, paid/violent/unidentified-theft responses, scheduled event times, small-prop route edges, safe placement, persistence, and vehicle sight obstruction.

Two concrete regressions were added after failures: a lamppost corner missed by sampled clearance, and a legal endpoint beside a crate rejected by boundary precision. Movement now checks exact expanded bounds with consistent floating-point tolerance.

The exported-player suite drives the actual controller, hold interactions, live companion following/aid and a live fight with Rook. It retains medicine, wounds, extraction interruption/retry, witness knowledge, resident damage, recovery and save/load checks. New coverage tests all six bins, controller resistance to entering/climbing a bin, the refuge's door/light, benefits and reload, a live companion route around a bin, and Tally familiarity.

Traffic is advanced through 3,600 steps at 1/30 second without pedestrians: all four cars travel more than 120 metres, and no vehicle footprints intersect. Separate checks verify pedestrian stopping clearance, resumption, moving navigation/collider footprints and menu pause. This is bounded junction/yielding coverage, not proof against every possible congestion pattern.

The legacy suite completes three jobs and perks, free roam, cargo banking, satchel purchase, alarms, pursuit/arrest, persistence and citizen movement against the updated streets. Its isolated mode disables the medical extension; the new suite covers combined medical/cargo interactions.

## Rendered inspection

Twelve B-series captures revisit existing screens. Twelve C-series captures show the new conversations, paid donation response, refuge offer/management, care-policy consequence, changed clinic street, Ivo after violence, timed history and traffic movement. The C-series and both new Mara standing captures are included in the zip.

Direct inspection prompted a clinic cutaway so Tally and the bed are visible, a background for the shipment appointment, a corrected journal care status and preloaded font sizes to prevent missing glyphs during atlas refresh. Captures wait for settled frames. The new panels, clinic, journal and traffic screens were inspected directly.

These are staged screens. Tests invoke transaction methods and interaction helpers programmatically; they do not click every GUI button or replace a human playthrough. Non-black checks alone do not establish visual correctness.

## Reproduce

Use the installed Unity CLI and active Editor license from the project directory:

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/build.log --no-tail --non-interactive
./Tools/Test-Demo.ps1 -Mode Full -ThirtyFPS
./Tools/Test-Demo.ps1 -Mode Legacy
./Tools/Test-Demo.ps1 -Mode Visual -Width 1280 -Height 720 -ThirtyFPS
./Tools/Package-Demo03.ps1
```

Full and Legacy default to headless exported players; add `-Visible` to watch. Visual needs a visible window. Full and Visual share `bandage-smoke.json` and must run sequentially. Legacy uses `smoke-save.json`. Normal play uses `lights-progress.json`; importing an earlier save does not alter its original file.

## Limits and acceptance

- Rendering was checked on **NVIDIA GeForce RTX 4090 / Direct3D 11**. No lower-tier physical GPU was available. A 30 FPS cap checks timing behavior, not minimum hardware requirements.
- Traffic has four local routes and conservative junction access. People can intentionally block lanes. No vehicle damage, player driving or police reinforcement system is implemented.
- Public patients remain scheduled transactions. Edda is characterized in dialogue, not a new actor. Medicine remains a finite twelve-dose economy; broader city production and shop/cargo replenishment remain prototype systems.
- Navigation and presentation failures were fixed before delivery. Final result files describe the passing candidate. Raw build/player logs remain in the project and are not distributed.
- The supplied dossier records harness, source and screen assessments. It informed this slice; its scores do not approve Demo 03.
- **User acceptance, human control feel, difficulty and the value of the refuge remain pending.**

Demo 02 evidence is preserved under `Evidence/Demo02-archive`; its original build and release remain available.
