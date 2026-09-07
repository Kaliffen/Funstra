# Demo 03 integration replay: Dag and Marcus coverage addendum

Date: 2026-09-08 local / 2026-09-07 UTC. Scope: accepted evidence work [#20](https://github.com/Kaliffen/Funstra/issues/20), following the [existing Demo 03 dossier](../Docs/funstra-review-dossier-demo03.html).

This is a targeted inspection of **shared CD-run evidence**, using Dag's systems lens and Marcus's verification lens. The inspecting agent added the targeted assertions, then read the completed results and opened both rendered captures. It did not launch an independent game run. This is neither another four-person review nor a new dossier. The four original Demo 03 reviews and their 8/10 scores remain unchanged.

## Candidate and evidence

- Executable: `Build/KeepTheLightsOn/Funstra.exe`; SHA-256 `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`.
- Gameplay assembly: `Build/KeepTheLightsOn/Funstra_Data/Managed/Assembly-CSharp.dll`; SHA-256 `D2FA2AF7EA929422C702AD82866D641893063ACB55BEBEB6C77D2D356AEC47EA`.
- Source checkout at inspection: `b22e311170c1fcb6d8fcd0e3f8e2d81a49b789d1` plus uncommitted integration changes. The commit alone does not identify this candidate; the assembly hash does.
- [Refuge rules](../Evidence/refuge-rules-result.txt): fresh PASS, 33 assertions, written 22:14:09 UTC. Seven new assertions exercise the reserve boundary.
- [Legacy runtime result](../Evidence/runtime-result.txt): fresh final PASS, 90 assertions, written 22:20:19 UTC. The CD reports process exit 0. The [complete player log](../Evidence/bandage-legacy-player.log) includes those checks and normal shutdown, with no runtime exception in the successful run.
- [RUNNER capture](../Evidence/13-mara-standing-runner.png): written 22:17:18 UTC; [CONNECTED capture](../Evidence/14-mara-standing-connected.png): written 22:19:59 UTC. Both were opened and inspected directly at 1280 × 720.

Successful invocation, reported by the CD (repository working directory, `Start-Process -WindowStyle Normal`, started after the 22:16:16 UTC failed attempt):

```text
Build/KeepTheLightsOn/Funstra.exe --smoke-test --capture-screens --evidence "~\repos\Funstra\Evidence" -screen-width 1280 -screen-height 720 -screen-fullscreen 0 --30fps -logFile "~\repos\Funstra\Evidence\bandage-legacy-player.log"
```

## Dag: reserve boundary gap closed

The earlier review correctly distinguished reaching the reserve floor from proving that public treatment still works above it. The new fixture in [RefugeRules.cs](../Assets/Editor/RefugeRules.cs) addresses that distinction:

1. Begin with three clinic doses, three supplier doses and six shipment doses: twelve total, with reserve-last-two enabled.
2. At time 89, nobody has been treated or refused and the shelf still holds three doses.
3. At time 90, the public patient receives one dose: the shelf falls to two, consumed medicine rises by one, and refusal count stays zero. Exactly $8 moves from patients to clinic; total money and medicine remain conserved.
4. At time 180, the next patient is refused at the two-dose boundary. Treatment, consumed medicine and both cash balances remain unchanged.
5. Reopen the shelf; at time 270 another patient is treated, leaving one dose. Refusals do not increase; another $8 transfers and conservation still holds.

**Basis:** executed Unity editor rules assertions plus source inspection. This uses a directly initialized, valid boundary fixture and scheduled `Tick` calls; it is not rendered patient gameplay. It closes the specific missing negative/boundary test without claiming to establish long-term economic depth. No treatment rules were changed.

## Marcus: moving standing gap closed

The [Legacy smoke route](../Assets/Scripts/FunstraSmoke.cs) now captures standing after its own progression mechanic fires. The successful runtime log records controller travel, held theft interaction, return travel and delivery for all three jobs. Acceptance, perk choice and some subsequent setup remain scripted; this is guided gameplay, not exploratory control.

The first new image shows **MARA / STANDING — RUNNER**, with $120 after the first actual delivery and perk selection. The second shows **CONNECTED**, the **DEBT SETTLED** objective, $950 secured cash and a nine-slot satchel after all three deliveries, cargo banking and reload. Both labels are legible and within their HUD panel. The runtime assertions connect these frames to completed counts one and three; the latter also checks that the completed state remains in free roam after reload.

**Basis:** executed exported-player route, complete successful runtime log, and direct image inspection. The former evidence gap is closed. These observations do not establish input feel or mid-range performance: the log identifies an RTX 4090, not Marcus's fictional reference hardware.

One nonblocking polish observation remains: the CONNECTED frame briefly displays the generic reload hint, "Meet Mara at the mint circle. Press E to talk," while the objective correctly describes free roam. This does not undo standing progression; a future context-aware reload hint would be clearer.

## Limits and disposition

An earlier capture attempt produced a black-screenshot failure at 22:16:16 UTC. It is not counted as passing evidence; the later completed rendered run replaced the result and supplied the inspected frames. The successful visible capture establishes these screens, not hidden-window screenshot reliability.

Both accepted coverage gaps are closed for the hashed candidate above. Full-suite and broader release readiness remain the CD's integration responsibility. No original score is revised and no user acceptance is implied.
