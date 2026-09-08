## Local Demo 05 ready — 8 September 2026

The requested next-demo boundary is complete. `Play.cmd` now opens `Build/PriceOfAGun/Funstra.exe` (v0.5.0). Source changes remain local; this is not a source push, panel review or publication.

Implemented Sella in Market Court; first-gun purchase, clinic-paper favor, theft and recovery from downed armed actors; explicit legacy-save migration and unarmed fresh starts; finite priced ammunition; a distinct shared-projectile SMG; observed open possession and actual arrest confiscation; three bounded physical courier consignments serving dealer and clinic. Blocking, recipient incapacity and diversion affect real custody. Diverted doses stay in the bag until given to Neri or sold to Mara. The main-menu session debug toggle is included in the normal build. Ivo, garage and dealer map labels are separated.

Validation: 2,294 editor checks, zero build errors/warnings; 376 core runtime checks (Arms 108, Streets 143, Police 35, Legacy 90), 33 captures. All four routes identify assembly `B1F9C4DDEC4004268248415C6EB1BE4FACA6562C6A3D744A78452131FEA5C8D0`. Every game run muted and serialized. Guided evidence distinguishes real controller travel from staged transactions/combat/possession and accelerated courier steps; this is not human play acceptance or low-end hardware validation.

Portable ZIP: `Releases/Funstra-the-price-of-a-gun-0.5.0-windows.zip`, 35,290,361 bytes, SHA256 `F4A0EE457D845828342A9ECACD3BD3A4C8B3BBAF05A9298A6C5A1D1F40086C5A`. All 145 player files compared byte for byte; extracted ZIP independently passed the full 108-check Arms route and 15 captures. No test game process remains running.

Local guide: `THE-PRICE-OF-A-GUN.md`; evidence: `Evidence/Demo05/validation-summary.json`, `package-verification.json`, `final/` and `portable/`. README, DESIGN and Evidence/VALIDATION reflect this local candidate while preserving the published v0.4.2 baseline. Previous builds, saves and unrelated field-review HTML preserved.

Reproduced and fixed during preparation: legacy inline-null serialization could mistake an old save for empty owned inventory (now explicit arms schema); courier could fail to step away from Neri after delivery; pending favor could prevent helping downed Neri; existing-pistol favor reward and dropped-courier goods needed proper custody paths. Earlier failing evidence is retained separately from final results.

Next action: product owner plays the new demo. Four independent reviewer passes/dossier, CD integration and publication remain subsequent stages. Rifle response and 30-plus army pressure remain the later roadmap slices; this demo retains nine pistol officers/two police trucks.
