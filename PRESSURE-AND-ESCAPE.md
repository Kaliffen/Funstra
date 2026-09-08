# Pressure and Escape — playable increment 0.4.2

This iteration follows published Police Response 0.4.1. The owner approved [#40](https://github.com/Kaliffen/Funstra/issues/40): make the nine-officer pursuit dangerous, readable and escapable before adding rifle squads. Contextual recovery text addresses the narrow defeat-copy concern from [#22](https://github.com/Kaliffen/Funstra/issues/22); broader aftermath remains future work.

## Play

Launch `Build/PressureEscape/Funstra.exe`, or extract the portable candidate and launch `Funstra.exe`. Campaign saves remain in `streets-progress.json`. The guided routes use an isolated smoke save; they do not replace the normal campaign. **Start a new night** resets the campaign, so retain a backup if you want to preserve an existing story.

WASD moves, Shift sprints, Ctrl sneaks. Mouse aims, left mouse fires, 1 selects fists, 2 pistol, 3 shotgun, R reloads, B bandages. Space pauses tactics; Tab opens the map; F2 switches compact/detailed HUD; F5 saves. M toggles sound during normal play. Automated tests force mute throughout their process.

## Review route

1. Enter Old Port and approach the eastern arcade street. Shoot a resident where a patrol or another witness can see the gunman. Residents remain attackable. A heard shot alone creates an area search; witnessed violence identifies the attacker.
2. Repeated witnessed injuries bring two police trucks, carrying three additional officers each. Watch their physical arrival and exit clearance while the starting patrols respond. Nine living officers is the maximum for this tier.
3. Pause during the response. Inspect the selected weapon, magazine/reserve and reload progress above the tactical instructions. Open the map while a notification is visible. The ammunition readout should remain readable; an unrecruited companion should not occupy a disabled orders grid.
4. Compare exposure with withdrawal. Officers use traveling pistol projectiles, finite magazines and real reloads. They move around blocked firing lanes and pull back when an attacker gets too close to use the pistol. Staying in their firing lanes should lead to defeat. Withdraw early from the edge of the firing lane, move behind solid buildings and keep changing streets to break contact; a bandage stops bleeding, but does not fully heal wounds.
5. Watch **CONTACT** change to **SEARCH** when the officers lose sight. The responding police share reported positions. They must not follow a concealed player's exact position through walls. Remain unseen for roughly 45 simulation seconds to end the search; new contact restarts it. Save/reload during the pursuit to inspect continuity.
6. Return to the clinic or recover after defeat. Recovery clears the immediate response and reports actual cash, cargo, job-item and medicine losses. A fresh attack on a resident must not invent a lost medical shipment or an offense remembered by Ivo. Existing equipment and recorded consequences remain.

## Guided build validation

Run the candidate through the sustained route:

```powershell
pwsh -NoProfile -File Tools/Test-Streets.ps1 -Mode Pressure -Visible -ThirtyFPS -PlayerPath Build/PressureEscape/Funstra.exe -EvidenceRoot Evidence/PressureEscape/pressure-final
```

Run the Police, Streets and Legacy regressions against that same `-PlayerPath`, using `Evidence/PressureEscape/police-final`, `streets-final` and `legacy-final` respectively. Serialize player runs. Reviewers use separate `review-dag`, `review-marcus`, `review-priya` and `review-nell` evidence folders and the same identified executable. Each runner records the tested assembly hash.

The Pressure route deliberately separates two kinds of evidence. A 75-second extended-durability fixture starts with actual witnessed player projectiles against a resident and allows all nine officers to arrive, fire, reload and spend their ammunition while normal Update, AI, projectiles and traffic run. That fixture increases player health and disables arrest; it measures collective activity and load, not normal survivability. Separate prepared encounters use ordinary health and arrest rules to compare exposed defeat with controller-driven withdrawal, search, save/reload and return. Resident movement is frozen only during the triggering exchange, and the unrelated yard squad is suspended throughout this police-focused route. Initial positions and encounter setup are scripted; this is guided build review, not human play or an entirely organic campaign outing. Captures pause the world for readable inspection.

`pressure-runtime-result.txt` describes the checks and setup actually executed. `pressure-profile.json` records hardware, resolution, frame cap, frame-time distribution, allocated memory and time samples of the responding roster, ammunition and positions. A frame cap does not establish minimum hardware support. Interpret results with the recorded fixture method rather than treating the stress fixture as a normal player encounter.

## Candidate and review status

The identified review candidate is v0.4.2, gameplay assembly SHA256 `BD9CA29F2787A9F2546E10CA88650142DF7E80D0E8D22743C7B88AB9F99BB776`. Core validation passes on this build: 2,213 editor checks and 309 runtime checks (Pressure 41, Police 35, Streets 143, Legacy 90). All four independent guided reviews are complete, each 8/10. Results and CD dispositions are in [validation-summary.json](Evidence/PressureEscape/validation-summary.json) and the [review dossier](Docs/funstra-review-dossier-pressure-escape.html). The exact reviewed assembly is the release target; human acceptance remains separate.

The packaging target is `Releases/Funstra-pressure-escape-0.4.2-windows.zip`. `Tools/Package-PressureEscape.ps1 -ExpectedAssemblySha256 <tested-hash> -ValidateOnly` checks the existing player, four core routes, sustained profile, four reviewer records and dossier before packaging. Packaging does not rebuild or publish the player.

## Boundaries

This remains a pistol response with two trucks and at most nine officers. Trucks remain parked after a search, and their finite crew/ammunition is reset by recovery. Rifle-equipped squads remain Demo06; the army maximum of 32 soldiers with rifles and physical grenades remains Demo08. Human judgments of difficulty, input feel and enjoyment remain separate from the scripted evidence. No new audio assets are required by this iteration.
