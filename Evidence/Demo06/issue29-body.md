Part of #26. Owner approved revised direction on 8 September 2026. Demo06 v0.6.0 is published and technically verified; product-owner gameplay and visual acceptance remain separate. Depends on Demo05.

Canonical roadmap, spatial and combat contracts: [DESIGN.md](https://github.com/Kaliffen/Funstra/blob/main/DESIGN.md). This revision supersedes the earlier service-first scope; existing completion history remains intact.

## Player outcome
A capable crew can take on a dangerous site and bring its people home.

## Scope
Player plus two persistent recruits, including Neri and one dockworker/mechanic. Selected-character control, tactical pause orders, aid/carry/rescue and limited practical skill growth serve one dock operation. Introduce a named boss, a helpful local contact with their own stake, and guards with group roles. Add a rifle distinguished by sightline use, handling, recoil and report. Guards cover movement, investigate reports, aid an ally or retreat; leader loss changes coordination without magical knowledge.

## Observable acceptance
Approach the same boss-controlled yard solo, with Neri and with the full crew; demonstrate stealth, bargaining and a coordinated fight with different costs. The boss uses the same damage/ammunition rules, with danger from people and position rather than a giant health pool. Interrupt enemy communication or remove their leader and observe a specific coordination change. Rescue or abandon an ally; their condition and remembered event survive reload. Compare rifle, SMG, shotgun and pistol on the same site.

## Validation
Control switching/order cancellation, companion and enemy pathing, resource costs, carrying an incapacitated actor, group retreat under obstruction, loss of leader/contact and failure recovery. Test ammo-cost and handling differences across four gun families in the exported player.

## Limits
Three controllable people, one authored dangerous site and boss, four gun families total at this stage. No large squads, extensive skill tree or forced boss kill.

## Stop or replan
If character switching/rescue is unstable, reduce encounter complexity until player plus Neri works. Prove one integrated operation before deepening every subsystem.

## Release checkpoint
- [x] Implementation issues scoped before work
- [x] Identified exported candidate; relevant regressions and rendered checks
- [x] Four actual-build guided reviews and one consolidated dossier
- [x] CD disposition and affected-route replays
- [x] Tested build published; GH/site latest-five retention and links verified
- [ ] Product owner acceptance recorded separately


Direction approval is recorded; the playable acceptance checkbox above remains open. No calendar promise. Next action: reconcile detailed scope against the preceding reviewed release. All firearm damage must use simulated projectiles, no hitscan; ray/sphere sweeps of actual traveled segments are collision detection, not instant full-range damage.

## Owner-directed escalation — 8 September 2026
Add rifle-equipped police response squads arriving with finite personnel in trucks. Covering movement and flanking use communicated sightings; player crew remains three. This supersedes the blanket no-large-squads limit only for bounded enemy reinforcements. The maximum 30+ army force remains Demo08.

## Review integration gate
#40 now owns full-nine live combat/escape and finite-ammunition replay, crowded truck exits, visible paused ammunition and lost-sight feedback before scaling to rifle squads. These come from the four v0.4.1 guided reviews, each7/10; #39 is locally packaged and verified. Do not treat its nine-person logistics fixture plus separate short patrol shot as proof of collective pressure.

## Pressure and Escape integration — 8 September 2026
#40 now proves nine distinct shooters in a 75-second normal-Update combat/load route; core run observes nine reloaders, four independent reviewer runs seven to eight. Explicit extended durability/no-arrest setup is separate from ordinary-health exposed defeat and early controller escape. All core regressions pass on the reviewed v0.4.2 assembly. This advances the previous gate without claiming ordinary maximum-pressure difficulty.

Before rifle escalation, cover delayed retreat under ordinary health/arrest rules, depleted officers approaching the player, and moving firing lanes. Record projectile owner/hit attribution to distinguish shared friendly damage from other stress casualties; do not remove ally collision or refill ammo merely to force nine survivors. A same-build CD focused replay uses a carried bandage and escapes at 81.68149 health (Evidence/PressureEscape/encounter-third); Marcus inspected it in a dated review addendum. The four independent full-route escapes were early and uninjured. Their records remain unchanged. Wider human difficulty and the timing window remain unproven. Dossier: Docs/funstra-review-dossier-pressure-escape.html.


## Resumed implementation checkpoint — 8 September 2026

Implementation is now present in the working tree: separate protagonist/Neri/Rell identities, timed aid/carry/clinic rescue and abandonment, finite equipment handoff, Rell's pump/payment/repair, rifle guards and existing finite response, resident/art bridge #41, and review rubric v2. No final release or owner acceptance is claimed.

Current diagnostic export: Build/NobodyGetsHomeAlone/Funstra.exe, 0.6.0, assembly SHA256 9E797175C8E9996C72A108EF1CE8F2CB85B2B0E27B735DE16E02B2C251B0EC1E. Build succeeds. Paid/controller recruitment, identity/in-flight ownership, timed rescue and repair pass. Focused rifle opposition passes independently; full sequence revealed intermittent treatment interruption at its shallow retreat corner. Full75s pressure proves two truck deliveries, six rifles, nine shooters/reloaders and finite ammunition; its first treatment corner also needs a deeper live retreat. Live theft probe was observed crossing the yard boundary; revised approach uses the west spine and northern loading edge. Failing evidence is retained, not counted as acceptance.

Evidence folders: Evidence/Demo06/{crew-fourth,opposition-fourth,approach-fourth,pressure-fourth}. Frozen panel ledger: Evidence/Demo06/review-outcomes.json (10 outcomes, weight40). Earlier engineering evidence predates the review ledger. Selected-control audit also fixed firing through roster buttons, stale pause overlay, arrest targeting and casualty/hiding state leaks; current source includes follow-up rendered UI cleanup after this diagnostic export.

Next action: build revised routes, resolve observed failures, validate the exact final candidate, then four independent guided reviewers and one dossier, integrate/replay defects, package/extract/test matching files, push source and publish/verify release/site/latest-five retention. Current public release remains v0.4.2. Stop after this authorized release; owner acceptance remains separate.

## Frozen candidate checkpoint — 8 September 2026

Eighth export: Nobody Gets Home Alone v0.6.0, gameplay SHA256 EB140E9C032D6A882B329C575FA4DD7E01EF90ED4819AEE88AB71BC584C910BA. Exact source/export mapping: Evidence/Demo06/candidate-source.json. Full Crew route now PASS (crew-final): separate equipment/in-flight ownership, actual carry around clinic walls, finite rescue and remembered abandonment/reload, total-wipe cleanup, held repair, delayed rifle wound/retreat/aid, moving firing lanes and guard report/aid. Focused live crew fight, pickup and east return also PASS (fight-eighth), all three alive with finite equipment and live responding police.

Diagnostics isolated real auto-Cover omissions: it now engages visible pursuing police and rejects firing corridors occupied by teammates. The western fight withdrawal entered arriving rifle reinforcements; actual east withdrawal to the workshop passes. No health boost, enemy deletion or ammunition refill was used for the fight. The ninth-officer prepared escape uses physical Arcadia alley corners and passed a seventh-build focused replay; the full final stress/escape gate remains to run. Earlier fourth aid failure has no definitive causal receipt; do not infer a geometry defect from it. The final delayed-wound route completed three simulated seconds of aid and consumed one carried dressing.

Final alternate approaches and regressions are running on the frozen eighth build. Panel brief: Evidence/Demo06/panel-brief.md; same frozen v2 outcome weights, no prescribed scores. The first reviewer is preparing while awaiting the sole player slot. Final summary, four fresh guided reviews, dossier, integration, extracted archive/source/release/site verification remain pending. Public download is still v0.4.2; no acceptance is claimed.

## Combat acceptance decision pending — 8 September 2026

Current eleventh candidate assembly: 6D8580F4691CDD147BEEE4C04C9060881818B176FF7F498DF31C2A47FE938C03. Source snapshot is candidate-source.json. Focused movement-eleventh PASS: crew skips a redundant visible grid connector while retaining obstacles and body separation; traffic now yields to Rell. The ninth full route exposed a two-person connector stall; a first exact-position fixture was invalid because fresh traffic covered those positions. The corrected fixture preserves pair offsets/grid alignment four metres west, asserts walkable origins, and completes both orders with minimum separation0.8819m. This is a controlled regression, not an exact replay of unrecorded ninth traffic positions.

Latest approach-final: both solo and Neri stealth pickups/returns/reloads PASS; full crew assembly/movement and actual shared-weapon yard victory PASS. The script then aborts when the protagonist goes down to patrol fire before collection. It still forces a stationary covering fight on renewed pre-pickup contact; post-pickup movement fix was not reached. Earlier eighth focused fight did return everyone alive, but repeated full-sequence failures are retained and cannot be promoted to final PASS. No ammunition refill, health boost, enemy removal or weakened collision was used.

CD stopped further assault-route tuning and asked the owner whether this acceptance should allow incapacitation followed by survivor-led rescue/return, or require nobody to be downed. This addresses the harness's unconditional casualty abort versus the slice's intended recoverable defeat. No answer or scope change is assumed. Core regression on the current candidate is running; independent reviewers are prepared but have not launched or filed scores. Full exact-candidate regressions, four reviews, dossier/integration/package/source/release/site verification remain unfinished. Publication remains v0.4.2; owner acceptance is separate.
Core eleventh regression completed PASS: 141 assertions and 17 rendered captures, including new traffic/congestion checks and the full rescue/repair/opposition route. No player remains running. Assault acceptance question above is still pending; no review or release completed.

Owner clarification, 8 September: casualties, failure and leaving people without rescue are legitimate. No clean full-crew return is required. The weighted ledger retains all weights and records the owner's exact amendment. A PASS validates shared mechanics and persistent consequences, not mission victory. Review the actual combat result and do not deduct merely because the scripted crew loses. Missing mechanics, broken navigation, lost custody and unsupported claims still merit findings. Current zero-health actors are incapacitated; separate irreversible death is not implemented and must not be claimed.

## Final candidate checkpoint - 8 September, thirteenth export

Assembly `442A8FD11DD93105429BF8ADFCEE5225F79369D89159D071807433AAA4CAA73D`; zero build errors/warnings and 2,377 editor assertions. The complete approach route passed 113 assertions with eight rendered captures in 222.24 seconds. Solo and Neri unseen theft both returned the same component and survived reload. The one finite-equipment crew assault returned it with the protagonist at 41.51 health; no casualty-free outcome is required. Both raw reload snapshots remain in `crew-combat-outcome.json`. Their only allowed difference is the existing uncarried protagonist upward spawn clearance of at most 0.1201 units; all other recorded state and coordinates match exactly. Twelfth failed evidence is preserved separately: its only mismatch was that spawn clearance.

Seven remaining same-candidate runtime routes are running serially. Four independent guided reviews, one inspected dossier, CD integration, portable archive/extracted test and source/release/site verification remain pending. No release completion or human acceptance claimed.

## Independent review checkpoint - 8 September

Assembly A `3A3C13ED20B5E957CE62F53D8611702FB8D0080CD575C5FF76FAA0F2625985C3`: all eight final routes PASS,701 assertions/72 captures;2,377 editor assertions. Dag, Priya and Marcus originals filed; Nell fresh Crew run passed and verdict is being completed. Each original has141 checks/17 inspected captures, independent v2 judgment and the same build. Product-owner acceptance remains separate.

Confirmed presentation corrections are limited to three expressions: idle police bullet, Mara attribution dash and Demo06 title footer. Preserve A's original full evidence and scores. A later B export must prove all other226 source files unchanged and the exact three approved UI replacements, then pass full Crew/Police/Legacy and Marcus/Priya rendered addenda. Five unaffected routes remain labelled A evidence, never B reruns. The strict package option records this scope and checks exact player bytes.

Future depth/clarity work is tracked in #42 and #43; relationship aftermath and subsequent operations belong with #30. No new depth systems are being slipped into the current UI correction. One consolidated dossier, package/extracted check, pushed source, published archive/site and latest-five verification remain pending.

## Published release — 8 September 2026

Demo06 v0.6.0 is published from source commit `13389848e3d9da55ba8398e0dbe591f611386777`. The public 177,810,627-byte archive matches the tested local archive by SHA-256: `462F4D42B90242ED735A95FBCA21597E1976D15F5A8C116BD7F30362B7C29A48`. Release check and Pages deployment passed. The live homepage, Demo06 dossier, download and exact retained list `v0.6.0, v0.4.2, v0.4.1, v0.3.0, v0.2.0` were verified. There was no sixth release to delete.

Four original reviewer scores remain unchanged: Game now 5.3–5.7/10; Slice delivery 8.3–10.0 provisional; Evidence 89% each. B presentation corrections and portable/public delivery are later receipts, not silent score upgrades. #42, #43 and #30 retain the deeper progression, crew-livelihood and relationship work. Product-owner gameplay and visual acceptance remain separate and open.
