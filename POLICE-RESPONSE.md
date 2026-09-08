# Police response — playable increment 0.4.1

This candidate follows the owner-approved Demo04. It implements the first enforcement tier in [#39](https://github.com/Kaliffen/Funstra/issues/39); the remaining Demo05 weapon acquisition/supply work and later rifle/army/grenade tiers are planned in DESIGN.md.

## Play

Launch `Build/PoliceResponse/Funstra.exe`, or extract the portable candidate and launch `Funstra.exe`. The approved Demo04 remains in `Build/StreetsWorthFightingFor`. Normal campaign saves continue using `streets-progress.json`; the guided route uses an isolated smoke save. Back up your campaign before choosing **Start a new night**, which resets that campaign.

WASD moves, Shift sprints, Ctrl sneaks. Mouse aims, left mouse fires, 1 selects fists, 2 pistol, 3 shotgun, R reloads, B bandages. Space pauses. Tab opens the map, F5 saves, M toggles sound during normal play. Automated tests force mute for their entire process.

## Five-minute review route

1. Enter Old Port and walk toward the arcade on the eastern street. Residents remain attackable. Fire at a resident in view of a patrol and observe the armed-response status.
2. Break sight around a building. All living patrols receive reported contact, but cannot track a hidden position through walls. A gunshot heard without identification produces an area search.
3. Continued witnessed injuries dispatch police trucks from the north and south. Each carries three officers, giving a maximum of nine police including the starting patrols. Trucks drive in, stop at their destination or a persistent roadblock, then deploy through clear side exits.
4. Staying in an open firing lane is dangerous. Officers spend finite ammunition on traveling projectiles and reload. Intervening actors and walls prevent a clear shot. Use cover, bandages and tactical pause.
5. Escape without fresh sightings for about 45 simulation seconds. Save and reload during a response to retain its severity, truck progress, roster, casualties and magazines. Defeat returns you to recovery and clears immediate reinforcements; residents' recorded consequences remain.

## Current boundaries

This is the first tier: pistols, two trucks and six additional officers. The trucks remain parked after search expires; their crew is finite until recovery resets the response. Rifle-equipped squads belong to Demo06. The bounded maximum of 32 soldiers with army trucks, rifles and physical grenades belongs to Demo08. This build does not claim that maximum-pressure experience.

Human judgment of pressure and enjoyment remains separate from scripted validation. The HUD has existing map and tactical-pause overlaps; the response count itself is visible at the upper right. No new sound assets were added; the approved recorded effects are reused.

## Reproduce validation

`pwsh -NoProfile -File Tools/Test-Streets.ps1 -Mode Police -Visible -ThirtyFPS -EvidenceRoot Evidence/PoliceResponse/police`

The focused route separates explicit report/dispatch/persistence fixtures from the live Update-driven firefight. It exercises noise versus identification, all-patrol response, pause, physical truck movement/deployment, finite roster, casualty/ammo reload, search expiry, actual civilian projectile damage, live police return fire and recovery. Captures freeze at tactical pause for inspection; they are not continuous video or human playtest evidence. Regression evidence and exact build identity live under `Evidence/PoliceResponse`.


The portable candidate is `Releases/Funstra-police-response-0.4.1-windows.zip`. The [four-reviewer dossier](Docs/funstra-review-dossier-police-response.html) records four independent 7/10 verdicts, retained evidence and CD decisions. Core validation passed 2,201 editor checks and 268 runtime checks on one identified build; the four reviewer passes each repeated the focused 35-check route. Follow-up #40 covers visible ammunition during pause, loss-of-sight feedback and sustained nine-officer combat; #22 covers contextual recovery text and resident aftermath. These limits remain disclosed for this playable increment.
