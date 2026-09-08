# Police Response 0.4.1 — Priya Raman

**7/10. A credible first consequence, with the person who was hurt still waiting for a voice.**

Date: 8 September 2026. Method: **guided scripted build review**, independently run and judged. This is a review of the nine-police increment, not the future army response or the whole promised Demo05.

## Build and route

- Executable: `Build/PoliceResponse/Funstra.exe`, assigned version 0.4.1. EXE SHA-256: `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`.
- Gameplay DLL SHA-256: `3CBCA4699C37181628C9167712F394FBAB2F125E1E1845AE069A51C2EC0A8BBC`, checked before and after the run. No package was reviewed.
- Source HEAD: `38ad01c218ecc93a20722fbfe99bd3c9a67dcbd2`, with substantial modified and untracked implementation. Source inspection is supporting evidence, not a claim that HEAD alone reproduces this binary. No inspected gameplay source was newer than the assembly; that timestamp check is not a source provenance proof.
- Command: `pwsh -NoProfile -File Tools/Test-Streets.ps1 -Mode Police -Visible -ThirtyFPS -EvidenceRoot Evidence/PoliceResponse/review-priya`.
- Runner started `2026-09-08T01:23:36.1913073Z`; final player-log timestamp `2026-09-08T01:23:44.0667150Z`; completed before the post-run inspection at `01:23:56Z`. Exit 0, 35 PASS checks, four fresh rendered PNGs. The runner released its shared mutex and exited before another reviewer could launch.
- Read the complete [player log](../../Evidence/PoliceResponse/review-priya/player.log), [result](../../Evidence/PoliceResponse/review-priya/police-runtime-result.txt), and [identity](../../Evidence/PoliceResponse/review-priya/build-identity.json). No exception/error appeared in this log, including shutdown. Opened all four PNGs with an image-capable tool.
- The route used its isolated smoke campaign. Audio was forced muted and the runtime check passed. No sound judgment. Rendering was 1280×720, D3D11 on the logged RTX 4090, capped at 30 FPS; this is not a minimum-spec or frame-time assessment.

I read my Demo03 review and both earlier meeting notes before this pass, plus [the guide](../../POLICE-RESPONSE.md), the relevant [design gates](../../DESIGN.md), the runner, `FunstraPoliceTests.cs`, `FunstraPolice.cs`, `TownAgent.cs`, and relevant combat, UI and recovery paths.

## What this adds

I wanted violence to change the place around the player. In this pass it does. The first truck is a blue shape occupying an ordinary street, beneath the same shop signs and warm apartment fronts that make Old Port feel inhabited. The second frame brings two trucks and clustered officers into that street. The response has arrived somewhere. It is more convincing than a wanted number alone.

The distinction between a heard shot and an identified gunman matters to me as much as the delivery. The controlled runtime checks show a search at the reported place, without reading the hidden player's new position, and all three patrols receiving an identified report. That gives the institution a point of view. It knows something because somebody saw something. The route does not establish an entire human escape sequence, but the distinction it exercises is the right one for a city built around witnesses and debts.

Then the live portion produces a blunt, readable consequence: the player has 72 health and is bleeding. The officer spent ammunition and the projectile actually hurt the player in the Update loop. The toast offers bandaging, pause and breaking sight. I can believe that standing in this street with a gun is dangerous. I cannot infer from one short exchange that nine officers are well balanced, or that sustained pressure feels overwhelming.

The recovery card remains humane about failure. “STILL BREATHING” is rendered above the emergency bed and $40 debt. The game permits a bad choice and leaves a route back into the story. I value that more than a punishing reload screen.

## Where the meaning stops short

My Demo03 score of 8 came from specific people answering specific choices. This increment mostly speaks through enforcement. The resident struck in the live fixture is recorded in source as a numbered resident, with an assault incident and a flee order. That is a consequence, but it is not yet the kind of memory I praised when Ivo spoke about Rook. I did not return to the victim, inspect their incident journal on screen, or observe a new conversation after recovery. I will not pretend I did.

There is also a concrete copy mismatch. In this fresh fixture the player shoots a resident and never acquires or loses the medical shipment. The recovery card nevertheless says, “The lost medical shipment remains with its owner.” The same card talks about Ivo's memory. The wording comes from the unconditional recovery text in `FunstraDistrictUI.cs:103`. It is inherited presentation, not evidence that shipment state was corrupted. Still, the game has just demonstrated a new kind of consequence and then explains an older one. Make that line conditional, and use the space to acknowledge the incident that actually led here when the game knows it.

The pause presentation also competes with itself: the central tactical banner covers the weapon information, while a large unused crew panel occupies the right side. This is visible in P01–P03. The response count at upper right and the health display remain readable. I would tidy those layers before multiplying the actors, because the small visible figures already compete with roofs, labels and exclamation marks. That is a readability concern; these stills do not prove moving officers are impossible to follow.

## Evidence ledger and action

| Finding | Basis and evidence | Disposition requested |
|---|---|---|
| Response physically enters the street | Controlled runtime calls production movement/deployment functions; [P01](../../Evidence/PoliceResponse/review-priya/P01-trucks-arriving.png) shows one truck; [P02](../../Evidence/PoliceResponse/review-priya/P02-response-deployed.png) shows two and deployed officers. The camera/player are teleported for capture. | Strength. Keep actual arrivals when scaling. |
| Hearing differs from identification; all patrols respond | Runtime result checks use explicit report fixtures and actor steps. Hidden position remains separate from reported position. | Strength within fixture coverage. Replay a continuous witnessed attack–escape–return route later. |
| Finite roster, persistence and search expiry work in the fixture | Result records six deployed personnel, exact reconstruction, magazine/casualty persistence and search expiry. One casualty and ammo values were directly assigned; P02 correctly displays eight living officers after that setup. | Strength. No claim of a naturally fought casualty or organic disappearance timer. |
| Exposed player can be shot | Actual trigger creates the resident projectile; explicit combat steps apply that first hit. The subsequent police exchange uses live Update. [P03](../../Evidence/PoliceResponse/review-priya/P03-live-return-fire.png) shows 72 health/bleeding. | Strength. No full-pressure difficulty verdict. |
| Recovery wording refers to a shipment not lost in this scenario | Rendered [P04](../../Evidence/PoliceResponse/review-priya/P04-recovery.png), fresh fixture in `FunstraPoliceTests.cs:73`, unconditional UI at `FunstraDistrictUI.cs:103`. Defeat itself was directly invoked after setting health to zero. | Confirmed copy issue, nonblocking for this increment. Condition text on actual events. |
| Resident aftermath is not demonstrated | Source `FunstraCombat.cs` records assault/incapacitation; runtime confirms incidents remain after recovery, without opening their text or revisiting the victim. | Coverage gap and creative request. Show a surviving resident or their contact remembering the incident after escape/reload. |
| Pause panels obscure weapon information | Rendered P01–P03. | Confirmed presentation overlap, already disclosed in guide. Consolidate HUD/pause layers before scaling. |

This is a 7 for the focused increment: a visible, intelligible response with an honest escape/recovery boundary, held back by generic aftermath and crowded presentation. It is not a revision of my Demo03 8, and it does not grade the absent rifles, grenades or 32 soldiers. Those are explicitly later work. No new blocking simulation failure was observed. My next replay question is simple: once the trucks stop looking for me, who still remembers what I did?
