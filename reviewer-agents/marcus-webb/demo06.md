# Marcus Webb — Nobody Gets Home Alone, Demo06

**Game now 5.7/10 · Slice delivery 8.7–9.8/10 provisional · Evidence 89%**

**8 September 2026 · Evaluation protocol v2 · Guided scripted build review.**

You have a functioning local crime-and-recovery game here. You do not yet have much sustained progression after its small set of material problems is solved. Recruiting somebody, giving them a real gun and carrying a casualty home now connect in the same city. That is useful progress. It does not turn the existing jobs, one contested pump and finite supply run into a substantially realised organization game.

The executable ran cleanly in my pass. Its presentation still needs a sweep: the title calls itself Demo06 at the top and Demo04 at the bottom, and idle police markers render as broken characters. Neither is a crash. Both are on the screen you actually receive.

## Identity and method

- Assigned player: `Build/NobodyGetsHomeAlone/Funstra.exe`, v0.6.0, Unity 6000.4.0f1. EXE SHA-256 `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`; gameplay assembly SHA-256 `3A3C13ED20B5E957CE62F53D8611702FB8D0080CD575C5FF76FAA0F2625985C3`. Independently hashed before launch; runner verified neither changed afterward.
- [Frozen source identity](../../Evidence/Demo06/candidate-source.json): fourteenth export, built 12:24:39.796 UTC, source base `bd522de8cfcc4bfbc86584ca28e3df1bf671ce3a`, dirty implementation. The commit alone does not identify the binary. I checked current Crew, CrewUI, Game, CrewState, DockOperation and ResidentPersona source hashes against this manifest; all six matched. This is a recorded export mapping, not an independent reproducible-build proof. No game changes, rebuild, archive verification or publication performed by me.
- Exact command from repository root: `./Tools/Test-Crew.ps1 -PlayerPath Build/NobodyGetsHomeAlone/Funstra.exe -Visible -ThirtyFPS -EvidenceRoot Evidence/Demo06/review-marcus`.
- Own fresh pass: **12:47:37.476–12:49:26.140 UTC**, **108.664 seconds**, player/runner exit **0**, **141 PASS assertions**, **17 PNGs**. No retries. [Identity](../../Evidence/Demo06/review-marcus/build-identity.json), [complete result](../../Evidence/Demo06/review-marcus/crew-runtime-result.txt), [complete player log](../../Evidence/Demo06/review-marcus/player.log), [method](../../Evidence/Demo06/review-marcus/crew-method.txt). Normal engine shutdown; no error or exception in this player log. Released the sole player slot immediately after exit.

This was a visible, forced-muted exported player using isolated `crew-smoke.json`, not the normal campaign save. I opened every one of my 17 frames. The controller actually travels for paid collection/return, clinic carrying and workshop repair. Money, recruitment, starting wounds and several positions are prepared. Timed aid includes accelerated production steps; self-aid also runs through normal Update. The total wipe is a directly prepared failure state. UI guards, arrest selection and surrender predicates are direct production calls, not desktop keyboard/mouse acceptance. The congestion fixture translates the earlier blocked pair four metres west because the failed scene's traffic positions were not recorded; it is not an exact replay of that old scene. The separate traffic step checks Rell's physical footprint.

I read the current guide, VISION/DESIGN/WORLD, amended brief, shared outcome ledger, runner, relevant crew/dock/UI/resident source, my prior reviews and meeting records. No other current reviewer verdict was read. Zero health means incapacitation here; irreversible death is not implemented. The owner's recorded amendment permits casualties and mission failure. I did not demand a clean assault victory or easier combat.

## Shared evidence inspected

All eight final result files, method statements, identities and logs through shutdown were inspected. Duplicate `CHECK` log lines were read through the corresponding complete result files. All eight identify the same final assembly. The [batch record](../../Evidence/Demo06/final-batch.json) records serialized successful completion; some older runner identities omit exit/end fields, so their batch receipts supply the completion bounds. These are shared CD runs, not eight additional runs of my own.

| Shared route | Assertions / PNGs available | Rendered frames I opened | Coverage used |
|---|---:|---|---|
| [Crew](../../Evidence/Demo06/crew-final/crew-runtime-result.txt) | 141 / 17 | C05b physical carry; all corresponding own frames separately | Core custody/rescue/repair and bounded opposition |
| [Approach](../../Evidence/Demo06/approach-final/crew-runtime-result.txt) | 113 / 8 | All eight AP frames | Actual solo/Neri theft, live three-person assault and return |
| [Arms](../../Evidence/Demo06/arms-final/arms-runtime-result.txt) | 108 / 15 | A04b map, A06 reload, A13 shortage | Controller favor; staged purchases/theft; accelerated physical consignments |
| [Residents](../../Evidence/Demo06/residents-final/residents-runtime-result.txt) | 28 / 7 | R01–R06 | Seven seconds of routine Update; staged assault, accelerated finite aid; separate recognition hook |
| [Streets](../../Evidence/Demo06/streets-final/streets-runtime-result.txt) | 143 / 4 | Public/service route, live yard contact, sustained traffic blockage | Existing medicine/refuge/cargo/traffic; controlled gate and projectile persistence |
| [Police](../../Evidence/Demo06/police-final/police-runtime-result.txt) | 35 / 4 | P02 deployment, P03 return fire, P04 recovery | Staged dispatch/persistence plus live return fire |
| [Pressure](../../Evidence/Demo06/pressure-final/pressure-runtime-result.txt) | 43 / 7 | E01 stress, E02 ammunition, E04 map, E07 losses | 75-second extended-durability stress; separate normal-health defeat and escape |
| [Legacy](../../Evidence/Demo06/legacy-final/runtime-result.txt) | 90 / 10 | 13 RUNNER, 14 CONNECTED | Original three jobs, cargo, pursuit and standing |

Arms/Residents/Streets/Police/Legacy smoke modes disable the new crew layer. They establish preservation of those mode contracts on this binary, not complete co-presence coverage for every old service with every selected companion. Core/Approach/Pressure supply actual crew integration evidence. Editor assertion totals in the validation summary are team claims; I did not rerun or use them as extra gameplay evidence.

## What actually works

**People retain their property when you switch control.** The staged identity sequence buys and hands Rell a real finite pistol, switches away while his projectile is airborne, reloads the save, and records that same projectile hitting Vale once. Selection preserves separate health, position and equipment. That answers a basic squad-game question with actual runtime evidence instead of three identical portraits.

**Rescue is a trip with a cost.** In my [C05b](../../Evidence/Demo06/review-marcus/C05b-physical-carry.png), Neri physically holds the downed protagonist. Controller travel goes around the clinic wall; admission spends one existing dose, restores 45 health, clears the carrying link and survives reload. [C06](../../Evidence/Demo06/review-marcus/C06-clinic-rescue-memory.png) identifies who returned. [C07](../../Evidence/Demo06/review-marcus/C07-remembered-abandonment.png) records Neri leaving Rell behind; the body remains down at its saved location. The wipe fixture clears stale carry/aid links and leaves a playable emergency recovery. These are meaningful persistence checks. They do not prove that abandonment drives later loyalty disputes.

**The alternatives use the same component.** My paid route moves $90 to Vale and gets home without a shot. Shared solo and Neri theft remain unseen through pickup and return. Shared combat uses staged recruitment and $800, finite purchases and live opposition. It returned the component with protagonist **41.53655 HP**, Neri/Rell **100 HP**, no incapacitated crew, and no protagonist friendly-crew hit receipt. The protagonist still owns **38 SMG rounds including an eight-round magazine**: the general actor row says zero because fists are selected, so I checked the dedicated ammunition arrays in [combat outcome](../../Evidence/Demo06/approach-final/crew-combat-outcome.json). Neri retains 11 pistol rounds; Rell 15 rifle rounds. Reload preserves this. This is one successful prepared attempt, not proof of fair human difficulty or an organically financed loadout.

**A corner buys time; it does not freeze the pursuer.** My [C20](../../Evidence/Demo06/review-marcus/C20-rifle-contact-before-retreat.png) begins retreat after a genuine rifle wound. The controller reaches real cover. Treatment then gets interrupted at about 2.741 simulation seconds by another attributed 30-damage hit: health **68.94476 → 37.16299**, two dressings retained, no treatment reward. Completed normal-Update self-aid is covered earlier. The separate full-pressure escape finishes at **100 HP**, so it is not a second wounded retreat. Both facts belong in the review.

**Earlier concerns have concrete new evidence.** The shared map labels separate Ivo/dealer/cargo; A06 shows an actual reload countdown during pause. Legacy frames explicitly show MARA: RUNNER and CONNECTED. The moving rifle-lane fixture retains distinct projectile owners, ammunition and separation without friendly officer hits. The earlier ninth-build movement failure is historical diagnostic evidence; my fresh connector fixture and the final live approach pass. It would be wrong to keep reporting that old failure as a reproduced final-build defect.

## Hardware and reliability limits

Actual host from my method/log and the same-build [pressure profile](../../Evidence/Demo06/pressure-final/pressure-profile.json): **Intel i9-14900KF / NVIDIA RTX 4090**, D3D11, driver **32.0.15.9597**, 1280×720, 30 FPS cap, one Funstra process during stress. No smaller GPU was tested. My fictional biography supplies no midrange machine.

Shared stress: **2,203 sampled frames; p50 33.334 ms, p95 33.602 ms, maximum 138.108 ms; endpoint Unity allocated memory 113,754,082 bytes (108.5 MiB)**. All nine officers fired and reloaded; aggregate ammunition reached zero. These are frame intervals on a capped workstation, not separate CPU/GPU costs, uncapped throughput or peak process memory. The maximum is a real recorded long interval; attribution is unresolved, and paused capture is part of the harness. I cannot call it a demonstrated recurring gameplay hitch. The earlier Pressure review's maximum was lower, but this changed rifle workload is not a controlled performance comparison.

No human aiming, input feel, listening, long-session repetition, lower-tier hardware or portable installation was tested by me. These bound the verdict; they are not invented failures.

## Game now ledger

Fixed Marcus weights; half-point category scores. Contributions and deficits are points out of ten. Scores assess the existing game against its durable pillars, not every future ticket.

| Category | Weight | Score | Contribution | Deficit | Reason and evidence |
|---|---:|---:|---:|---:|---|
| Systems and agency | 15 | 5.5 | 0.825 | 0.675 | Connected acquisition, finite arms, crew positions and rescue offer real choices (Crew/Approach/Arms). Practice has no demonstrated learned capability; organization-building and repeated strategic consequences remain shallow. |
| People and consequences | 10 | 5.0 | 0.500 | 0.500 | Rell/Neri partnerships, resident help/avoidance and persistent custody change play (C06–C09, R03–R05). Much personality remains short occupation/reaction text; crew memory mostly records an event rather than changing later commitments. |
| Control, clarity and comfort | 20 | 6.0 | 1.200 | 0.800 | Pause, ammunition, selection and recovery are usable (C05–C07, A06, E07). Large order overlays conceal the encounter; unavailable actions lack useful explanations, and older services require a protagonist switch explained better in the manual than in the contextual prompt. |
| Craft and reliability | 35 | 6.5 | 2.275 | 1.225 | Clean own runtime, inspected regressions and real obstacle handling. Repeated façade/actor forms, heavy floating labels, garbled markers and stale title identity make the city feel unfinished (C00/C01a/C22, R01–R06). Technical evidence remains constrained; no unsupported claim of low-end failure. |
| Breadth and sustained play | 20 | 4.5 | 0.900 | 1.100 | Several connected local loops and changed clinic/workshop state exceed isolated demonstrations (Arms/Streets/Legacy/Approach). One pump, short authored job arc, finite consignments and limited relationship development give insufficient varied reasons for another self-directed outing. |
| **Total** | **100** | | **5.700** | **4.300** | **G raw/final 5.7.** |

No absent/unsupported category is silently filled, and no category weight is redistributed. No core-unavailable or progress-loss cap applies. Each category was revisited through my crew run and the inspected same-build supporting routes. Human-feel and sustained-enjoyment judgments remain explicitly outside the evidence.

## Slice delivery ledger

Frozen [ten-outcome ledger](../../Evidence/Demo06/review-outcomes.json), total weight **40**, including the owner's casualty/failure amendment. `a` is attainment; `e` is resolved evidence. Lost points = `10 × weight × (1 − a) / 40`. Ranges are uncertainty, not missing-feature convictions. Shared integration and panel delivery are pending, not absent.

| Outcome | Weight | Attainment a / status | e | Lost points | Evidence / limitation |
|---|---:|---|---:|---:|---|
| Three stable selectable people | 5 | 1.00 delivered | 1 | 0 | Own identity/transfer/in-flight save and reload sequence |
| Finite component / three approaches | 5 | 1.00 delivered | 1 | 0 | Own paid route; shared solo/Neri theft and actual assault return. Service gate traversal is separately shown; no claim of a second independent theft through it. |
| Rescue / abandonment | 5 | 1.00 delivered | 1 | 0 | Own C05–C07b; interrupted/completed aid, physical clinic journey, saved abandonment and wipe cleanup |
| Yard opposition | 5 | 1.00 delivered | 1 | 0 | Live shared assault; own finite guard aid/report-loss and rifle-lane receipts. Controlled casualty setup is disclosed. |
| Rifle / shared pursuit | 5 | 1.00 delivered | 1 | 0 | Own wounded retreat/interruption and depleted movement; inspected Police/Pressure dispatch, finite stress and escape |
| Partnership / practice | 3 | 0.75 substantially delivered | 1 | 0.1875 | Real held repair consumes final gasket and recruits Rell; aid awards saved practice. No visible practice progress or earned capability found in inspected UI/source, materially weakening the learning result. |
| Livelihood / supply integration | 3 | 0.75–1.00 provisional | 0.5 | 0–0.1875 | Existing routes and crew purchases/custody work. Legacy suites disable crew; full old-service/job/courier co-presence with recruited crew has not been established. No confirmed regression. |
| Resident/art bridge #41 | 3 | 1.00 delivered | 1 | 0 | Named differentiated reaction/finite aid and remembered-help behavior; readable darker market/clinic/yard frames. Artistic limitations remain in G. |
| Reviewed release reaches users | 3 | 0.50–1.00 provisional | 0.5 | 0–0.3750 | Exact-build routes and rendered review exist; full panel/dossier/package/source/release/site retention remain unresolved at original review. |
| Protocol v2 review delivery | 3 | 0.25–1.00 provisional | 0.5 | 0–0.5625 | This independent original supplies the required ledgers; other originals and consolidated dossier are intentionally uninspected/pending. |

`S_raw = 10 × [34.75, 39.25] / 40 = [8.6875, 9.8125]`, hence **S final 8.7–9.8 provisional**. Confirmed attainment loss is **0.1875** for learning; up to **1.1250** additional points remain unresolved across custody/integration/rubric. Missing/broken-core cap 5.0: not applicable. Central-outcome cap 3.0: not applicable. Unrecoverable-progress-loss cap 3.0: not applicable. No binding cap and no extra cap subtraction.

`E = 100 × 35.5 / 40 = 88.75%`, displayed **89%**. This is outcome coverage, not success percentage or image count. It includes adequate controlled evidence where that resolves arithmetic, and partial evidence where it cannot establish the promised integration. Later delivery evidence belongs in an addendum, not a rewritten favorable original.

## Largest shortcomings and actionable findings

1. **Craft/reliability deficit 1.225:** the world is readable but relies heavily on repeated masses and floating text; small visible defects survive into its title and threat markers. Fix the malformed idle marker in `FunstraUI.cs:261` and v0.6.0 footer at line 88. Replay C00/C01a plus a moving contact route. A profiled replay attributing the 138 ms interval and a measured smaller machine would resolve the technical uncertainty; neither is a request to guess a minimum spec.
2. **Breadth deficit 1.100:** once the local errands and pump are resolved, the game provides limited durable change to organize another outing around. A return visit where prior supplies, people and actions create a different practical problem would change this assessment. More counters or another façade would not.
3. **Control/clarity deficit 0.800:** addressing a partner is explained, but the order panel occupies most of the central scene and offers actions without their availability conditions. With a companion selected, old services fall back to the generic crew prompt (`FunstraGame.cs:226`), while the manual says to use F1. Add contextual refusal/requirement text and demonstrate a companion arriving at an older service. This is a source-supported clarity finding, not a claimed fresh runtime failure of that branch.

**Confirmed minor visual defects:** wrong title footer and malformed idle police glyph; own C00/C01a reproduce both. **Confirmed capability shortfall:** practice is stored but not visibly developed into a skill result; six relevant frozen source files matched, and the script search found practice only in state/increments/validation/tests. **Coverage gaps:** cross-system normal-campaign co-presence, input/audio/hardware breadth, package/publication and full panel completion. **Taste/judgment:** repeated architecture, dense labels, thin post-operation life. No confirmed final-build functional blocker or unrecoverable progress-loss defect was observed.

## Verdict and continuity

**5.7 for the game now; 8.7–9.8 provisional for this delivery.** It is worth inspecting as a working small crew slice. There is no price-based purchase recommendation: no price was assigned, and I have not measured an evening of sustained enjoyment. The real improvement is connected control, finite equipment and recoverable consequences, alongside explicit evidence for prior HUD questions. The remaining missing game still costs points.

**New baseline; historical scores not comparable.** My earlier 8/10 for Pressure and Escape rated a bounded increment under the old rubric. It is not a G delta and has not been retroactively changed. Comparable outcomes gained: physical crew rescue/custody, distinct rifle pressure and actual delayed wounded retreat. Preserved: readable paused ammo, finite response and playable recovery. The next useful review should check normal-campaign integration and reasons for returning, then the documented presentation fixes. Product-owner acceptance remains separate.
