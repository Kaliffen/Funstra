# Marcus Webb — Demo06 presentation replay addendum

**8 September 2026 · Shared fresh guided rendered replay · Original scores unchanged.**

The corrected title, police marker and Mara attribution are now visible in candidate B. These are useful presentation repairs. They add no new agency, relationships or reasons for another outing, so I am not converting three text fixes into a higher whole-game score.

The [original independent review](demo06.md) and [meeting record](demo06-meeting.md) remain unchanged: **Game now 5.7/10 · Slice delivery 8.7–9.8/10 provisional · Evidence 89%**. Those are the original A assessment and coverage figures, not a newly calculated B full-game rating. This addendum closes the identified display defects on B; it does not resolve every provisional integration or publication outcome.

## Build boundary and verification

- **A, original reviewed assembly:** `3A3C13ED20B5E957CE62F53D8611702FB8D0080CD575C5FF76FAA0F2625985C3`.
- **B, integrated assembly:** `0F599165F989EFB88B610219CF5E31CE9DF617A486FC7DE26FE800A4A521AE29`.
- Both EXEs: `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`, v0.6.0. I independently rehashed the current B assembly. Source base remains `bd522de8cfcc4bfbc86584ca28e3df1bf671ce3a`, with dirty source recorded at export.

Read [integration source](../../Evidence/Demo06/integration-source.json), [validation manifest](../../Evidence/Demo06/integration-validation.json), [three replacements](../../Evidence/Demo06/ui-replacements.json), [A player-file manifest](../../Evidence/Demo06/reviewed-player-files.json) and [B player-file manifest](../../Evidence/Demo06/integrated-player-files.json). Applying the three specified literal replacements to the preserved A `FunstraUI.cs` reproduced the current B UI source exactly. Its SHA-256 is `83FC3AF7E7EC4DCDE0F620C961D01722E6B3B01CBDE688C28B505BBAD2F7BC01`.

My manifest comparison found **145 files in each player, 143 identical**, with only `Assembly-CSharp.dll` and `boot.config` different. Comparing the preserved boot files showed only the build GUID changed. Compiled scenes and runtime assets have matching hashes in those manifests. The source manifests differ at `FunstraUI.cs` and generated `OldPort.unity`; the recorded generated scene identifiers do not imply a changed compiled scene. This is inspected source/file evidence, not an independent reproducible-build audit or eight freshly rerun B routes.

## Fresh shared replay evidence

I did not launch another player. The CD ran serialized visible, muted 1280×720 routes with the existing production test runners, explicit B player path and isolated evidence folders. The [integration runner](../../Evidence/Demo06/run-integration-routes.ps1) adds `-Visible -ThirtyFPS`; Crew uses `Test-Crew.ps1`, Police and Legacy use their respective `Test-Streets.ps1 -Mode` values. [Batch receipts](../../Evidence/Demo06/integration-batch.json) pin each route to B. The method remains scripted controller journeys mixed with disclosed fixtures and direct production calls, not free exploration or another independent campaign.

| Affected route | Fresh observed evidence | Result |
|---|---|---|
| [Crew](../../Evidence/Demo06/integration-crew/crew-runtime-result.txt) | [Identity](../../Evidence/Demo06/integration-crew/build-identity.json), [log](../../Evidence/Demo06/integration-crew/player.log) | 141 assertions, 17 captures; exit 0; 13:07:14.995–13:09:03.696 UTC, 108.701 seconds |
| [Police](../../Evidence/Demo06/integration-police/police-runtime-result.txt) | [Identity](../../Evidence/Demo06/integration-police/build-identity.json), [log](../../Evidence/Demo06/integration-police/player.log) | 35 assertions, 4 captures; successful batch completion at 13:09:11.371 UTC |
| [Legacy](../../Evidence/Demo06/integration-legacy/runtime-result.txt) | [Identity](../../Evidence/Demo06/integration-legacy/build-identity.json), [log](../../Evidence/Demo06/integration-legacy/player.log) | 90 assertions, 10 captures; successful batch completion at 13:12:43.971 UTC, about 212.6 seconds after launch |

All three complete results and logs through normal shutdown were inspected, reading duplicated `CHECK` lines through the complete result text. The final Legacy identity matches B, and its completed batch receipt was inspected at 13:13 UTC. No player error or exception appeared. Police/Legacy's older identity format omits exit status, but the inspected runner only writes those identities after exit 0 and a fresh PASS; the outer batch also records PASS. B Crew's late aid happened to complete this time: **3.020 simulation seconds**, **68.92524 → 78.96214 HP**, one dressing consumed, no new hit. A's interrupted outcome remains correctly recorded in the original. Both are permitted production outcomes; the different encounter timing is not evidence of an undisclosed difficulty change.

## Rendered fixes confirmed

1. **Title identity:** opened [B C00](../../Evidence/Demo06/integration-crew/C00-title.png). The footer now reads `0.6.0 / NOBODY GETS HOME ALONE`, consistent with the Demo06 heading. The old Demo04 footer finding is closed on B.
2. **Idle police marker:** opened [B C05b](../../Evidence/Demo06/integration-crew/C05b-physical-carry.png). The officer at lower right has a clean blue dot instead of the stacked broken characters in A. Also opened [B P02](../../Evidence/Demo06/integration-police/P02-response-deployed.png): its pursuing exclamation marks remain readable. P02 is not the evidence for the idle-dot branch; C05b is.
3. **Mara attribution:** opened [B Legacy07](../../Evidence/Demo06/integration-legacy/07-ending.png). The ending renders a normal em dash before Mara, with three jobs complete and $740 cash. The screenshot appeared while the longer Legacy route continued, so the frame alone was not treated as a completed-run receipt.

Actual replay logs still identify RTX 4090, D3D11 and driver 32.0.15.9597. There is no new smaller-machine, uncapped, human-input or sound evidence here. I inspected the four affected PNGs named above, not all 31 captures from these replay routes.

## Disposition

The two visible defects in my original review are fixed; the additional Mara attribution correction is also visibly verified. The category anchors remain unchanged: craft still has broader presentation limitations, and the largest missing experiences are sustained progression, richer ongoing consequences and clearer contextual crew/service actions. No numerical uplift is justified by this bounded replay. No new functional blocker was observed in the inspected evidence.

The full eight-route validation and four independent originals remain **A** evidence. B has the explicitly identified affected-route replays and source/file comparison. Package, source publication, release/site retention and owner acceptance must still be recorded by the release workflow; this addendum does not assert their completion.
