# Nobody Gets Home Alone 0.6.0 — Priya Raman

**Game now 5.4/10 · Slice delivery 8.7–10.0/10 provisional · Evidence 89%.** The crew can bring someone home. What happens between those people afterward is still much smaller than the journey.

8 September 2026. **Guided scripted build review**, independently launched and judged. Evaluation protocol v2; **new baseline, historical scores not comparable**. The slice range reflects specific unresolved integration evidence, not an assumed failure or an award of its upper bound. No other current review was read.

## The person at the other end of the journey

The strongest image is Neri inside the clinic, with the protagonist standing beside her again. In [C05b](../../Evidence/Demo06/review-priya/C05b-physical-carry.png), the fallen body is physically with her outside the building. In [C06b](../../Evidence/Demo06/review-priya/C06b-clinic-admission.png), both are inside its little partitioned room. The intervening controller route goes around the wall and through the doorway. One existing dose is consumed. This is a substantial change from the old recovery sentence saying that Neri dragged you out: there is now a body to carry and a place to carry it.

The panel says, “NERI carried me to the clinic. I know who came back.” That sentence is earned by the exercised action, although the injury and starting positions were prepared. I do not mistake it for a naturally unfolding rescue under gunfire. I also do not need a clean victory to give this mechanic credit. The explicit abandonment fixture leaves Rell down at his recorded position after reload; it does not quietly return him to the party. Incapacitation is what this build implements. Irreversible death is a different, unimplemented claim.

Rell has a reason to be here. [His offer](../../Evidence/Demo06/review-priya/C08-rell-workshop-offer.png) says Vale impounded his pump for a debt already paid. Working on the auxiliary pump uses his last gasket, drains the visible sump and earns his partnership while the main component remains elsewhere. I like that helping someone work can recruit him before helping him fight. The material problem makes the relationship more credible.

But the relationship largely stops developing once it has admitted the player. Rescue and abandonment produce a stored sentence and a condition; I found no subsequent crew objection, negotiation or changed willingness to accompany you driven by those new memories. [CrewState](../../Assets/Scripts/CrewState.cs), [the rescue/abandonment paths](../../Assets/Scripts/FunstraCrew.cs) and [their display](../../Assets/Scripts/FunstraCrewUI.cs) support that distinction. This is a present-game limitation, not a broken promise to implement the next release. The record remembers the event more fully than the relationship answers it.

The residents are a welcome partial answer to my previous question about who remembers violence. Ruth spends her own dressing on Elias after danger passes; Bruno leaves him behind. The [saved aftermath](../../Evidence/Demo06/residents-final/R04-after-aid-state.json) identifies both choices and the consumed resource. Elias visibly comes over to thank the player in [R05](../../Evidence/Demo06/residents-final/R05-recognition.png). That final player-help flag is a separate explicit hook, however: Ruth's actual aid must not be retold as a continuous player rescue and reunion. The people have different conduct now. Their language and subsequent relationships remain thin and frequently templated.

The darker saltstone streets and small warm clinic doorway give care a physical setting. Yet bodies are still tiny, generic figures, with much of their identity carried by labels and panels. Rell's pump comes home; his neighbors' changed lives are mostly a sentence about what the pump can do. That is where I want the next piece of the game to begin.

## What I actually reviewed

- Assigned player: `Build/NobodyGetsHomeAlone/Funstra.exe`, v0.6.0. EXE SHA-256 `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`; gameplay DLL SHA-256 `3A3C13ED20B5E957CE62F53D8611702FB8D0080CD575C5FF76FAA0F2625985C3`. No archive reviewed.
- [Source freeze](../../Evidence/Demo06/candidate-source.json): fourteenth export, base commit `bd522de8cfcc4bfbc86584ca28e3df1bf671ce3a`, dirty source. I checked the inspected crew, dock, resident and refuge source hashes against this manifest; they matched. Clean HEAD alone is not the reviewed source.
- Command: `./Tools/Test-Crew.ps1 -PlayerPath Build/NobodyGetsHomeAlone/Funstra.exe -EvidenceRoot Evidence/Demo06/review-priya -Visible -ThirtyFPS`.
- [Run identity](../../Evidence/Demo06/review-priya/build-identity.json): `2026-09-08T12:44:38.0539336Z` to `12:46:26.5749618Z`, 108.521 seconds, exit 0, **141 PASS assertions, 17 fresh PNGs**. Runner checked both binary hashes before/after; I also inspected their current hashes. I notified the CD immediately on exit to release the sole player slot.
- Read the complete [result](../../Evidence/Demo06/review-priya/crew-runtime-result.txt), [player log through shutdown](../../Evidence/Demo06/review-priya/player.log), [method](../../Evidence/Demo06/review-priya/crew-method.txt) and relevant state/receipt evidence. Opened **all 17 own captures**. No exception/error appeared in the own log.
- The runner uses `crew-smoke.json`, separate from the player's `crew-progress.json`, and a shared validation mutex. The pass was muted at 1280×720, 30 FPS. No desktop input, free exploration, audio experience or minimum-spec claim.

The fresh route covers paid controller collection/return, independent identity and in-flight projectile ownership, finite transfers, interrupted/completed aid, carried clinic arrival, remembered abandonment/reload, total-defeat cleanup, held repair and rifle/guard behavior. Money, recruitment, wounds and selected placements are expressly prepared. Some aid/movement checks use stepped production functions; the carry journey, repair hold and opposition exchanges use actual controller/Update movement. The connector regression is translated from an earlier failure's geometry; it is not an exact recreation of that old traffic scene.

My own late rifle encounter interrupted treatment after **2.740 simulation seconds**: health 68.94521 became 37.16434, including a new 30-damage projectile and bleeding. Both dressings remained, with no healing or practice credit. Earlier normal-Update self-aid completed successfully. Neither outcome substitutes for the other.

I also inspected matching identities, complete relevant results/methods and logs for all eight shared final routes. Repeated log CHECK lines were cross-read with the result; startup, other messages and shutdown were inspected separately. These are shared evidence, not eight additional runs of my own:

| Shared route | Checks / captures | Credit and principal limit |
|---|---:|---|
| [Crew](../../Evidence/Demo06/crew-final/crew-runtime-result.txt) | 141 / 17 | Same core contract; my independent repeat supplies fresh rendered evidence. |
| [Approach](../../Evidence/Demo06/approach-final/crew-runtime-result.txt) | 113 / 8 | Live solo and Neri unseen collection/return; prepared recruitment/money, finite purchases and live full-crew attack/return. |
| [Arms](../../Evidence/Demo06/arms-final/arms-runtime-result.txt) | 108 / 15 | Controller favor plus staged shopping/combat and accelerated physical supply. Crew disabled. |
| [Residents](../../Evidence/Demo06/residents-final/residents-runtime-result.txt) | 28 / 7 | Real routine movement; staged assault, stepped danger/aid, separately injected player-help memory. Crew disabled. |
| [Streets](../../Evidence/Demo06/streets-final/streets-runtime-result.txt) | 143 / 4 | Medicine, refuge, cargo, geometry and service-gate regressions; mixed live/fixture checks, crew disabled. |
| [Police](../../Evidence/Demo06/police-final/police-runtime-result.txt) | 35 / 4 | Dispatch/knowledge fixtures and live return fire. Crew disabled. |
| [Pressure](../../Evidence/Demo06/pressure-final/pressure-runtime-result.txt) | 43 / 7 | Crew/rifle enabled; 75-second extended-durability load and separate ordinary-health defeat/escape. |
| [Legacy](../../Evidence/Demo06/legacy-final/runtime-result.txt) | 90 / 10 | Mara jobs, upgrades, cargo, standing and free roam, under legacy smoke rules with crew disabled. |

Shared images opened: Residents R01/R04/R05; Arms A03/A13; Approach solo-returned, full-crew-combat-outcome, full-crew-live-fight-result and rifle-dealer; Pressure E03/E04/E06; Streets D04-normal-quay-service-route; Police P02; Legacy 07-ending and 14-mara-standing-connected. I did not visually inspect every shared capture.

The [actual assault outcome](../../Evidence/Demo06/approach-final/crew-combat-outcome.json) returned the component with protagonist 41.53655 HP, Neri/Rell 100, no incapacitations, and 38 SMG / 11 pistol / 15 rifle rounds remaining. It preserves reload state, allowing the documented 0.12m uncarried-protagonist vertical clearance. Successful return is an observation, not a required survival score. The owner amendment explicitly permits loss and unrescued casualties.

Shared pressure recorded RTX 4090 / i9-14900KF, 2,203 frames, p50 33.334ms, p95 33.602ms, maximum 138.108ms. Nine officers fired/reloaded and exhausted finite ammunition. Its 10,000-HP stress fixture says nothing about ordinary survival. Separate escape reached the clinic at 100 HP without needing aid. Human difficulty, sustained economic balance, animation quality in continuous viewing, input feel and long-term attachment remain unmeasured.

## Game now ledger

Scores use fixed Priya weights and half-point category increments. Contributions and deficits are points out of ten; no later duplicate deductions.

| Category | Weight | Score | Contribution | Deficit | Reason and evidence |
|---|---:|---:|---:|---:|---|
| Systems and agency | 15 | 5.5 | 0.825 | 0.675 | Connected payment/theft/fight, finite supply and rescue choices work. Planning remains concentrated in a small set of authored transactions and one new operation; practice counters do not yet change capabilities. Own core, shared Approach/Arms, inspected practice paths. |
| People and consequences | 35 | 5.0 | 1.750 | 1.750 | Neri's clinic relationship, Rell's material stake, resident aid/avoidance and remembered physical rescue clear the functional anchor. New crew memories rarely produce a later relationship decision; residents share short response templates. Own C06/C07/C08, Residents state/R05, current crew/resident/refuge source. |
| Control, clarity and comfort | 15 | 6.5 | 0.975 | 0.525 | Selection, explicit paused orders, physical return and recovery remain understandable in exercised states. Crew panel conceals much of the immediate scene; selected companions cannot perform older services, requiring F1. Own 17 frames, pointer/arrest/resume checks, guide and `FunstraGame` interaction path. No human control-feel inference. |
| Craft and reliability | 20 | 6.0 | 1.200 | 0.800 | Coherent streets and clinic, clean exercised runtime and truthful recovery. Generic figures, dense labels, stale title footer and broken glyphs weaken authored presence. Own C00/C01a/C06b/C22; shared Police P02 and Legacy07. Muted, high-end hardware evidence limits assessment. |
| Breadth and sustained play | 15 | 4.0 | 0.600 | 0.900 | Medicine, deliveries, cargo, refuge and pump give a connected local loop. Finite consignments and one resolved pump exhaust much of its new direction; free roam has few demonstrated evolving social purposes. Arms final depletion, Legacy ending/free roam, returned-workshop states. Narrow sustained-play depth keeps this category at the v2 ceiling of 4. |

`G_raw = 5.350`; rounded **G = 5.4**. No progress-loss or launch cap applies. Breadth's narrow-example limit is already reflected in its category score; it is not a second subtraction.

Three largest shortcomings by weighted deficit:

1. **People do too little with what they remember (1.750).** Rescue changes a body and records a sentence, but offers little subsequent negotiation or changed loyalty. Replay one saved rescue/abandonment followed by a distinct, consequential decision from that person.
2. **Few evolving reasons for another outing (0.900).** The workshop is repaired, the component returned and finite supply exhausted without a comparably developed next dependency. Show an ordinary continued campaign where those outcomes create a materially different purpose, rather than resetting the operation.
3. **People and aftermath are carried heavily by UI text (0.800).** The streets have atmosphere; the figures and congested labels carry little individual expression. Replay closer, readable in-world aftermath and corrected title/attribution/status text. This is a craft judgment plus the confirmed small defects below, not a request for easy combat.

## Slice delivery and evidence ledger

The [frozen ten-outcome ledger](../../Evidence/Demo06/review-outcomes.json) totals 40. Its owner-authorized casualty amendment changes no weights. Attainment uses 0/0.25/0.50/0.75/1; evidence uses 0/0.5/1. Bounds express unresolved conditions, not confirmed missing features.

| Outcome | Weight | Attainment | Lost points | e | Status / evidence |
|---|---:|---:|---:|---:|---|
| Three stable selectable people | 5 | 1.00 | 0 | 1 | Delivered in core identity, transfer, in-flight reload and selected controller routes. |
| One component and three approaches | 5 | 1.00 | 0 | 1 | Paid own route; same-build solo/Neri theft and live full-crew fight/return. Shared Streets resolves physical alternate gate access under its stated mode; no separate live stealth success through every entrance claimed. |
| Rescue or remembered abandonment | 5 | 1.00 | 0 | 1 | Actual carry/admission plus explicit casualty, abandonment and total-defeat fixtures; condition/location/cost persist. |
| Dangerous yard through people/position | 5 | 1.00 | 0 | 1 | Ordinary guards, finite firing, cancelled report after leader loss, live finite aid and withdrawal. Core/opposition and shared Approach receipts. |
| Finite rifle response/shared pursuit | 5 | 1.00 | 0 | 1 | Own rifle damage, interrupted aid, depleted withdrawal and moving lanes; shared Police/crew-Pressure dispatch, stress and escape. |
| Partnership/practice from completed work | 3 | 1.00 | 0 | 1 | Real repair hold/gasket and finite medicine recruitment; completed aid increments practice, interruption does not. Practice is rudimentary game growth, but the committed awards/results exist. |
| Existing livelihood/supply with crew | 3 | 0.75–1.00 | 0–0.1875 unresolved | 0.5 | Dealer/medicine/equipment coexist in crew tests; extensive Arms/Streets/Legacy regressions use the same assembly with crew disabled. Entire legacy livelihood in a crew-enabled campaign remains only partially resolved. No confirmed broken livelihood. |
| Resident/darker presentation bridge | 3 | 1.00 | 0 | 1 | Named conduct, finite NPC aid, saved memories and visible recognition; actual darker street/clinic/yard frames inspected. Player-help recognition setup disclosed. |
| Reviewed corrected release reaches users | 3 | 0.25–1.00 | 0–0.5625 unresolved | 0.5 | Exact runtime and rendered evidence exists. Full panel, dossier, integration, package/source/release/site/retention handoff pending at this review. |
| Evidence-based v2 panel delivered | 3 | 0.25–1.00 | 0–0.5625 unresolved | 0.5 | Frozen rubric and this independent full ledger exist. Other independent originals and completed dossier are not inspected or presumed. |

`S_raw = 10 × [34.75, 40] / 40 = [8.6875, 10]`; **final provisional S = 8.7–10.0**. No confirmed missing/broken core, unavailable central outcome, launch failure or unrecoverable progress-loss cap. Binding overall cap: none. `E = 100 × 35.5 / 40 = 88.75%`, rounded **89%**. The 1.3125-point width is entirely unresolved credit, not a confirmed deduction. Publication pending is not publication absent. Later evidence belongs in a dated addendum; it must not rewrite this original.

## Findings and next review

Confirmed minor presentation defects: own [C00](../../Evidence/Demo06/review-priya/C00-title.png) has the correct Demo06 heading but a **DEMO 04 / FOUNDATION CANDIDATE** footer; shared [Legacy07](../../Evidence/Demo06/legacy-final/07-ending.png) has garbled attribution punctuation before Mara; blue stacked malformed glyphs appear above officers in own C00a/C01a and shared [Police P02](../../Evidence/Demo06/police-final/P02-response-deployed.png). Correct the strings/encoding and replay those exact screens. These impair confidence and presentation, not observed custody or recovery. Own C22 also piles guard status labels under the notification band; separate the labels when revisiting combat inspection.

Coverage gaps: a fully crew-enabled livelihood-to-operation campaign; organic resident injury through the player's actual assistance to saved recognition; later crew decisions driven by abandonment/rescue; ordinary human route discovery and input. The first two can change delivery confidence. The latter relationship question chiefly concerns Game now.

Compared with my [Pressure and Escape review](pressure-escape-review.md), return now includes physical carrying, another controllable person and finite clinic admission. Named residents have a demonstrated distinction between helping and leaving. The old demand for a specific person to answer an incident is partly met, with the player-recognition fixture limitation preserved. Existing Neri/Ivo authored branches remain in the inspected source; this route did not render every branch again. No numerical delta from historical 8/10 is valid: that was a focused increment under different semantics.

**My verdict:** a convincingly delivered local crew capability inside a still limited game about people. I can now follow the body home. I want the next outing to be shaped by the person who carried it.
