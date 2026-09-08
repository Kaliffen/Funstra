# Pressure and Escape 0.4.2 — Priya Raman

**8/10. The street can hurt you, and getting home now has a readable shape. The person hurt at the beginning still needs an aftermath.**

8 September 2026. Method: **guided scripted build review**, independently launched and judged. This score covers the focused Pressure and Escape increment. It does not replace my Police Response 7/10 or Demo03 8/10, and it does not assess future rifle squads or the army tier.

## The identified pass

- Assigned executable: `~\repos\Funstra\Build\PressureEscape\Funstra.exe`, version 0.4.2. EXE SHA-256 `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`.
- Gameplay DLL SHA-256 `BD9CA29F2787A9F2546E10CA88650142DF7E80D0E8D22743C7B88AB9F99BB776`, checked before and after the run alongside the executable. No archive was reviewed.
- Source HEAD `18415af855cf79c38743b71ab778fefd125eb86a`, with substantial dirty implementation. Supporting source inspection describes the current checkout; it is not proof that clean HEAD reproduces the player.
- Command: `pwsh -NoProfile -File Tools/Test-Streets.ps1 -Mode Pressure -Visible -ThirtyFPS -PlayerPath ~\repos\Funstra\Build\PressureEscape\Funstra.exe -EvidenceRoot Evidence/PressureEscape/review-priya`. No ReplaySnapshot.
- Runner start `2026-09-08T07:50:25.8867782Z`; final player-log timestamp `2026-09-08T07:52:45.2536033Z`; runner completion observed by `07:52:49Z`. Exit 0, **41 PASS checks**, nine fresh rendered captures. Shared run slot released before the next reviewer.
- Read the full [runtime result](../../Evidence/PressureEscape/review-priya/pressure-runtime-result.txt), [player log including shutdown](../../Evidence/PressureEscape/review-priya/player.log), [build identity](../../Evidence/PressureEscape/review-priya/build-identity.json), [stress profile](../../Evidence/PressureEscape/review-priya/pressure-profile.json) and [escape profile](../../Evidence/PressureEscape/review-priya/escape-profile.json). Opened all nine PNGs with an image-capable tool. No exception or error appeared in the inspected log.

Before judgment I read my profile, previous Police Response review/meeting notes, Demo03 review/continuity, earlier meeting notes, the reviewer skill, [guide](../../PRESSURE-AND-ESCAPE.md), runner and relevant [design gates](../../DESIGN.md). Supporting source: `FunstraPressureTests.cs`, `FunstraPressureCrowdingTests.cs`, `FunstraSmoke.cs`, `FunstraPolice.cs`, and the recovery/save/pause paths in `DistrictState.cs`, `FunstraDistrictUI.cs` and `FunstraGame.cs`. I did not read other reviewers' current verdicts.

## What changed for me

The useful image here is the clinic after the search ends. Two blue trucks remain down the street. The player's marker is inside a room with beds and partitions. Home has a location; the response has occupied the roads around it. I can read the journey between those things in this pass, even though a script made the turns.

Last time I had a brief exchange of fire and a recovery card that explained somebody else's trouble. This time the separate ordinary-health encounters supply a clearer pair of outcomes. Remaining exposed leads to actual defeat. Leaving promptly, turning through streets, saving and reloading, and staying unseen ends pursuit with 100 health. That does not tell me whether I would find the turns under pressure. It does tell me that withdrawal is an available action with a different consequence.

The displayed distinction between CONTACT and SEARCH helps that consequence feel intelligible. A city that loses sight of you should have to look for you. The logged hidden-position checks and escape samples support that limit in the exercised route: the player waits around x=-45 while the remembered report remains around x=35. The end message says, “Immediate pursuit has ended. Recorded incidents remain.” That is a useful promise about the difference between safety and history.

The recovery card now tells the truth about the scenario. In the fresh defeat it says, “You lost no cash or carried goods.” The emergency bed adds $40 debt and restores 45 health, with no invented shipment or Ivo offense. The separate laden fixture lists $17 cash, $60 unbanked cargo, Mara's item and medicine, then correctly includes Ivo's memory. The longer version fits visibly on its card. A modest change, but I care about it: the game is paying attention to which story it is telling.

My old ammunition-overlap complaint is also resolved in the inspected pause/map states. Pistol, magazine and reserve sit above the tactical instructions; the unused companion orders grid is gone. I can read the situation without disentangling the interface first.

## What I still want

“Recorded incidents remain” is an assurance from the interface. I still want a person to answer it. The clinic return opens no conversation about the injured resident; the route does not revisit that resident or display their incident record. This is a creative request and a coverage gap, not a newly discovered broken feature or a demand to complete the future investigation system in this increment.

The route also resets between the witnessed attack/stress and the ordinary-health encounters. It does **not** carry that original resident's complete story through an ordinary-health escape and reunion. Next time, keep one particular assault, its saved history and its aftermath together, then show the injured person or a known contact responding. That would test the promise I actually care about.

The map still has a small presentation blemish: in E04 the collector label partly covers the garage label. It does not obscure the restored ammunition or SEARCH status. I would separate those destination labels when revisiting the map. The large pause and notification bands also conceal some street actors in E01/E02; that is a taste/readability concern in paused inspection, not evidence that active combat is unreadable.

## Evidence ledger

| Finding | Basis, evidence and limits | Action |
|---|---|---|
| Sustained collective response exists | Runtime: actual witnessed projectiles initiate dispatch, followed by 75 seconds of normal Update/AI/projectiles/traffic. [E01](../../Evidence/PressureEscape/review-priya/E01-nine-officer-stress.png) is paused, explicitly labeled extended durability. Result records nine shooters, seven reloaders and finite ammo. | Strength. Do not interpret its 10,000 starting health or disabled arrest as normal survival. |
| Exposure and withdrawal differ | Runtime ordinary-health prepared encounters; [E02](../../Evidence/PressureEscape/review-priya/E02-paused-contact-ammunition.png), [E03](../../Evidence/PressureEscape/review-priya/E03-actual-defeat-recovery.png), [E04](../../Evidence/PressureEscape/review-priya/E04-search-map-ammunition.png), [E05](../../Evidence/PressureEscape/review-priya/E05-escape-complete.png). Initial actors/roster/ammo are reset; travel thereafter uses the production controller, with save/reload and live search. | Strength within the prepared route. Human difficulty and discovery of this route remain unmeasured. |
| Return reaches a physical clinic | Runtime controller arrival and valid save; rendered [E06](../../Evidence/PressureEscape/review-priya/E06-homeward-return.png). | Strength. No observed new conversation or resident aftermath. |
| Recovery wording matches losses | Runtime live ordinary-health defeat plus rendered E03; separate direct inventory transactions and direct defeat invocation produce [E07](../../Evidence/PressureEscape/review-priya/E07-laden-recovery.png). `DistrictState.Defeat` explains conditional details. | Previous copy issue resolved in exercised empty/laden cases. Laden defeat was not organically played. |
| People can obstruct deployment and be passed | Controlled placement and explicit production steps on real geometry; [alternate exit](../../Evidence/PressureEscape/review-priya/P42-crowded-truck-alternate-exit.png), [officer passing](../../Evidence/PressureEscape/review-priya/P42-officer-passing.png). | Strength within isolated fixtures; stills alone do not prove the movement sequence or arbitrary crowd behavior. |
| Map destination labels overlap | Rendered E04: collector label covers part of garage label. | Confirmed minor presentation issue. Separate labels; no blocking effect observed. |
| Resident memory is not demonstrated | No incident-journal inspection, surviving-victim return or aftermath dialogue on this route. | Coverage gap/creative request: replay one assault through escape/reload to a specific human response. |

Stress profiling recorded RTX 4090, Intel i9-14900KF, D3D11, 1280×720, 30 FPS cap, one Funstra process, 2,250 sampled frames: p50 33.334 ms, p95 33.338 ms, maximum 41.401 ms, Unity allocated memory 111,599,919 bytes. All nine arrived; the final stress sample has eight living officers and 33 total rounds. This is not a claim that nine survived the entire fixture or a minimum-spec result. Audio was forced muted; no sound judgment. The escape took no damage, so its conditional bandage branch was not exercised. Stills and scripted checks establish neither human input feel nor enjoyment or long-term balance.

**8/10 for this increment.** The extra point over Police Response comes from a legible escape/recovery arc and the removal of the two concrete presentation failures I raised. I can now understand the immediate consequence and the way back. A richer answer from the people of Old Port remains the reason to return for another review. No new blocking runtime failure was observed; publication and human acceptance remain with the CD and owner.
