# Demo06 independent panel assignment

Review **Nobody Gets Home Alone v0.6.0**, issue [#29](https://github.com/Kaliffen/Funstra/issues/29), using the Funstra reviewer skill and your existing persona/history. Apply `reviewer-agents/README.md` protocol v2. Shared outcome weights were frozen in `Evidence/Demo06/review-outcomes.json`; choose your own attainment, evidence and category judgments. This is the first v2 baseline; historical scores are not comparable.

Player: `Build/NobodyGetsHomeAlone/Funstra.exe`.

- Gameplay assembly SHA-256: `3A3C13ED20B5E957CE62F53D8611702FB8D0080CD575C5FF76FAA0F2625985C3`.
- Executable SHA-256: `97845574417B7FC4DB70BFBAA4D3F941EAB8628DF2BCC9424DBA328D3A6549C3`.
- `candidate-source.json` records the dirty checkout's exact source hashes at export/freeze and its base commit. Report source status separately from binary identity. Do not rebuild or edit the game.
- Guide: `NOBODY-GETS-HOME-ALONE.md`. Creative scope: VISION, WORLD, DESIGN and `Evidence/demo06-plan.md`.

Each reviewer runs one fresh muted guided Crew route, with root/CD allocating the sole player slot:

```powershell
./Tools/Test-Crew.ps1 -Visible -ThirtyFPS -EvidenceRoot Evidence/Demo06/review-<shortname>
```

Read the runner first. All players share a validation mutex and isolated harness saves; do not launch while another player is running. Notify CD when the player exits so the next reviewer can run while you inspect and write. Record the full command, timestamps, exit status, hashes, assertion and capture counts. Open rendered screenshots with an image tool and read the relevant complete logs. Do not call this free exploration or human play.

Inspect the same-build final evidence you rely on: `crew-final`, `approach-final`, `arms-final`, `residents-final`, `streets-final`, `police-final`, `pressure-final`, `legacy-final`. The validation summary indexes these once complete. The four fresh Crew runs repeat the same route; wider evidence is shared, not four independently rerun campaigns. A folder not yet complete is pending evidence, never an assumed pass.

The core Crew route includes paid acquisition, selection/custody, rescue and abandonment, actual carried return, repair, rifle wound/retreat/aid and group behavior. It explicitly stages wounds, money, recruitment and some transactions. The approach route separately moves through solo theft, Neri theft and a live full-crew assault and its actual outcome using finite purchased equipment and normal health. The full pressure route separates an extended-durability 75-second stress fixture from ordinary-health escape. Arms/Residents/Streets/Police/Legacy exercise their legacy mode contracts on this assembly; their smoke modes disable the new crew layer. Read the METHOD/COVERAGE text before crediting each claim. Post-retreat aid may finish or be interrupted by a verified direct hit; pressure also records the actual Hold cancellation on renewed contact. Interrupted aid must remain idle without healing, practice or dressing expenditure. These late encounters do not promise successful treatment; the separate core self-aid and rescue checks cover completed treatment.

File unedited originals at `reviewer-agents/<persona>/demo06.md` and `demo06-meeting.md`, including both v2 ledgers, caps, explicit deductions, Evidence coverage, provisional bounds where needed, three largest present-game shortcomings and observations that could change them. Do not read the other current reviewers' verdicts before your own is filed. Publication/panel integration are pending during independent review; distinguish unresolved from absent. Later delivery evidence belongs in a separate addendum, never a rewritten favorable original. Human acceptance remains separate.



Owner clarification, 8 September: casualties, failure and leaving people without rescue are legitimate. No clean full-crew return is required. The weighted ledger retains all weights and records the owner's exact amendment. A PASS validates shared mechanics and persistent consequences, not mission victory. Review the actual combat result and do not deduct merely because the scripted crew loses. Missing mechanics, broken navigation, lost custody and unsupported claims still merit findings. Current zero-health actors are incapacitated; separate irreversible death is not implemented and must not be claimed.




