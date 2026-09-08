# Marcus Webb — Police response 0.4.1

**8 September 2026 · Guided scripted build review · 7/10**

You can provoke a response, save in the middle of it, and load without receiving a fresh army from the save file. The police have ammunition. Their bullets actually hit you. Losing the fight leaves a recoverable campaign. Those are useful foundations, and this build's focused route passed cleanly. It is still a nine-officer increment. Nobody has demonstrated the proposed 32-soldier siege here, and the guide correctly says so.

My previous Demo03 score was 8. This is a 7 for this narrower increment, not a retroactive downgrade of Demo03 or a claim that its campaign regressed. The response works in the tested cases, but the evidence of actual fighting is much narrower than the evidence of dispatch accounting, and the screen hides combat information during the pause you need to inspect it.

## What I actually ran

- Executable: `Build/PoliceResponse/Funstra.exe`, candidate version 0.4.1 in the guide; Unity player metadata is engine version 6000.4.0f1.
- EXE SHA-256: `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`.
- Gameplay DLL SHA-256: `3CBCA4699C37181628C9167712F394FBAB2F125E1E1845AE069A51C2EC0A8BBC`, verified before the run and recorded by the runner afterward.
- Source HEAD: `38ad01c218ecc93a20722fbfe99bd3c9a67dcbd2`; checkout dirty with substantial Demo04 and response work. The commit alone does not identify this candidate. Source checks support interpretation; I did not independently rebuild it or verify a package.
- Command: `pwsh -NoProfile -File Tools/Test-Streets.ps1 -Mode Police -Visible -ThirtyFPS -EvidenceRoot Evidence/PoliceResponse/review-marcus`.
- Start: **2026-09-08 01:23:08.029 UTC**. Player log closed **01:23:16.235 UTC**. Shell elapsed about 8.8 seconds, exit **0**, **35 PASS rows**, four fresh PNGs. This is accelerated fixture work, not eight seconds of representative campaign play or a measured five-minute walkthrough.
- Read the complete [player log](../../Evidence/PoliceResponse/review-marcus/player.log), [result](../../Evidence/PoliceResponse/review-marcus/police-runtime-result.txt), and [identity](../../Evidence/PoliceResponse/review-marcus/build-identity.json). No error, exception, failure or crash entries; normal shutdown reaches the log tail.
- Opened all four PNGs with an image-capable tool. They are paused, staged camera captures. The command forces mute and the runtime checks zero listener volume. No subjective audio assessment.
- The runner uses the isolated `smoke-save.json` path for this route and a shared validation mutex. My run was serialized and stored under its own evidence directory. I did not modify a normal campaign save.

I read the persona history, current guide, DESIGN's enforcement stages, runner, `FunstraPoliceTests.cs`, `FunstraPolice.cs`, `TownAgent.cs`, and the relevant game/combat integration. I did not run the older campaign routes in this review. The CD reports current-build Legacy and Streets regressions passed; those are separate team checks, not my independently replayed evidence.

## What earns credit

**Saving is not a refill button.** The route requests both trucks, reloads before arrival, runs their real dispatch/movement functions, then writes a casualty with a partial magazine and reloads again. Six reinforcement records remain six, living officer count drops to eight, and the casualty and magazine survive. The setup directly assigns that casualty; it does not demonstrate a player killing an officer. It does demonstrate that the save path preserves the tested state.

**Hearing and identification are different.** Direct report fixtures distinguish an unidentified shot from witnessed harm. All three initial patrols respond to the reported attacker, while a relocated unseen player does not overwrite the shared location. Search expires without new reports, and an unidentified noise cannot awaken an old pending violent dispatch. That avoids a particularly annoying class of ghost pursuit.

**Return fire has a cost and a result.** The final encounter stages the player, a resident and an officer. The actual player fire function emits a projectile, and stepping combat wounds the resident. The route then resumes the normal Update loop: the officer spends ammunition and the player is hit. [P03](../../Evidence/PoliceResponse/review-marcus/P03-live-return-fire.png) shows **72 health / bleeding** with a response active. Pause freezes ammunition and damage. An explicitly emptied officer moves again rather than permanently posing with an empty gun. These are concrete runtime checks, although the test does not manually aim or empty an entire magazine by shooting.

**Recovery has a readable price.** [P04](../../Evidence/PoliceResponse/review-marcus/P04-recovery.png) clearly shows 45 health, $40 debt and a large return button. The route directly invokes defeat after setting health to zero; it does not play out a fatal police barrage. The assertions verify response reset and retained recorded incidents. That is enough to check this recovery transaction, not a full post-defeat session.

## Findings and evidence ledger

| Finding | Basis and reproduction | Player impact / action |
|---|---|---|
| Response information is legible | **Rendered:** [P01](../../Evidence/PoliceResponse/review-marcus/P01-trucks-arriving.png) shows three officers, a visible truck and an arrival notice; [P02](../../Evidence/PoliceResponse/review-marcus/P02-response-deployed.png) shows two trucks and eight living officers after the staged casualty. | Count follows the fixture's living roster, rather than advertising the original nine after a casualty. Keep that honesty. |
| Tactical pause obscures weapon information | **Confirmed rendered UI problem:** P01–P03 place the tactical-pause panel over the faint underlying pistol/magazine line. The notification bar consumes additional central space. | You pause to assess the fight and lose easy access to ammunition information. Give weapon/ammo a reserved visible location in tactical pause. Existing issue acknowledged by the guide; still present in this candidate. |
| Squad pressure remains a coverage gap | **Runtime/source:** logistics explicitly omits combat; the live encounter waits for one staged officer's successful return fire. P02 is paused and cannot prove coordinated pressure. | Before the rifle tier, provide a normal-Update encounter with the full nine, cover changes, actual reload cycles and a sustained escape. No claim that this build's maximum force is either trivial or overwhelming follows from the current route. |
| Truck obstruction has a bounded rule, but this pass is not an exhaustive collision test | **Source:** truck checks include structures, props, cars, player and actors; a four-second blockage permits unloading through checked exits. Runtime proves both arrivals and walkable deployments in this fixture. | Sensible rule. A resident blocking each door and a crowded mixed-traffic arrival deserve targeted coverage before raising crew counts. No collision defect observed here. |
| Hardware coverage remains limited | **Runtime log:** RTX 4090, D3D11, 1280×720 launch arguments, 30 fps cap. No frame-time trace or CPU/GPU profile was produced by this route. | Successful completion is not a minimum-spec or smoothness claim. Test simultaneous combat load and collect timings before multiplying personnel. I did not test a midrange machine; the persona biography does not supply one. |
| Human control and difficulty are untested | **Method limit:** scripted calls, teleports and controlled state setup; four stills. | No verdict on mouse feel, aiming comfort, sound, minute-to-minute escape difficulty or long-session value. A paid-game purchase recommendation would be premature. |

## Verdict and next replay

**7/10 for the response increment.** Clean launch, clean focused checks, finite reinforcement accounting, functioning return fire and a clear recovery screen. Good enough to hand over as this bounded playable step. It earns no speculative credit for rifles, grenades, coordinated army squads or the full remaining Demo05 acquisition economy.

The strongest next improvement is visible ammunition during tactical pause. The strongest next test is nine officers fighting through a sustained cover-and-escape sequence, with frame timings and without disabling combat to finish the logistics. A smaller machine remains a separate coverage request. My prior concern about truthful state displays is partly answered here by the living officer count; prior Mara-standing and medical-economy findings were not re-adjudicated by this focused run.
