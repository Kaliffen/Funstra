# Demo 04 — Streets Worth Fighting For

Development candidate. Build validation, panel review and owner acceptance are tracked in [release #27](https://github.com/Kaliffen/Funstra/issues/27). The expanded environment and clinic pass is tracked in [#37](https://github.com/Kaliffen/Funstra/issues/37). The owner is holding this demo until the slice is complete; it is not yet approved for publication. This guide describes the candidate under development, not a certified build.

## Play and test

The candidate executable is `Build/StreetsWorthFightingFor/Funstra.exe`. The title menu offers Old Port and four prepared foundation levels. Each test uses the same movement, weapons, projectiles and enemy logic as the campaign, with disposable equipment and state. Resetting a test never changes your story save.

| Control | Action |
|---|---|
| WASD / arrows | Move relative to the camera |
| Shift / Ctrl | Sprint / sneak |
| Mouse / left button | Aim / fire; fists strike nearby people in the aimed direction |
| 1 / 2 / 3 | Fists / pistol / shotgun; switching interrupts reload |
| R | Reload; ammunition remains finite |
| Space / Escape | Tactical pause / menu |
| E beside a gate | Open or close it; occupied openings cannot close |
| B | Use a bandage |
| Scroll / F11 / M | Camera zoom / fullscreen / sound |
| F1 / F6 in test levels | Toggle checklist / reset the whole test |
| F2 / Tab in Old Port | Toggle compact/details HUD / district map |
| G / H / Shift+R / T in Old Port | Neri follow / hold / retreat / aid |

## Four focused tests

1. **Movement & obstacles.** Walk and sprint around corners, through the passage, beside the bin and through the gate. Try to close the gate while standing in its opening. Check stopping, turning, camera visibility and collision. No enemies attack here.
2. **Weapon handling.** Weapons and ammunition are prepared alongside moving targets. Compare pistol and shotgun at several distances, shoot beside close cover, empty a magazine, and interrupt a reload by switching. Look for readable impacts and a meaningful difference in handling.
3. **Enemy coordination.** Face a three-person group. Expose yourself to one guard, break sight and change position. Observe communication, holding, repositioning and retreat. Pause to inspect; reset to compare another approach.
4. **The yard encounter.** Use a public route or gated approach, work between cover, engage at your preferred distance and withdraw. Repeat with the other weapon. Judge the complete movement-and-combat experience.

The checklist is a prompt for judgment, not a prescribed verdict. Note the test number, weapon, location and what felt wrong. Automated checks and guided reviewers cannot approve human control feel.

## Old Port route

Old Port now extends beyond the previous district boundary. Imported architectural meshes give the streets rowhouse terraces, retail blocks, lofts, a market hall and a church landmark, with single-storey workshop fronts. The church forecourt is accessible; the church and these other exterior buildings are not enterable interiors.

Home is now the frontcourt of a residential rowhouse, opening onto the southern street instead of an isolated marker in an open square. Follow the offset street blocks, courts and back passages toward Mara, then approach the clinic from the public court on its south side. Walk through the front doorway into the treatment room and speak to Neri there. Continue into the connected medicine store, then leave through the east doorway onto the receiving lane. This is a continuous room in the district: no loading transition or teleport. The roof and camera-facing walls cut away to keep the player visible; hidden walls still block movement and shots. Beds, shelving and other furniture have collision.

From the clinic's back lanes, head north to the church forecourt, then continue around the market toward the public quay. Cross east to the workshop court and walk south through the residential street before returning home. Use Tab to inspect the enlarged district map. The northeast bonded yard still offers a gated service passage and a public approach around the warehouse. Compare a cautious approach, an armed incursion and a retreat; existing clinic, cargo and Mara opportunities remain available.

Dusk lighting separates warmer sunlit masonry from cooler shade. Existing lamps, occasional warm windows and subtle pavement/material wear add depth without a post-processing overhaul. Building signboards follow their facade cutaway.

The compact HUD is the default street view, showing health, cash, heat and the current objective without filling the screen. F2 switches to the detailed HUD and back. Compare both while moving, inside the clinic and near a combat encounter; check that the information you need remains readable.

## Guided environment demonstration

The Environment route demonstrates the expanded district and the clinic through actual controller travel in an isolated campaign fixture. It enters through the south doorway, reaches Neri's clinic service and the connected store, exits through the east doorway, visits the church forecourt, market/quay, workshop court and residential street, then returns home. Its screenshots are `E01` through `E09` in the route's evidence directory.

```powershell
pwsh Tools/Test-Streets.ps1 -Mode Environment -Visible
```

Default evidence: `Evidence/Demo04/environment/`, including `environment-runtime-result.txt`, `build-identity.json`, `player.log` and rendered captures. The route freezes district AI and removes the yard squad for the walkthrough; it verifies travel and service access, not combat pressure or autonomous district behavior. Inspect the captures as well as the result. Streets, Legacy, Visual and all four foundation runs remain separate release checks against the same gameplay assembly. Packaging now also requires a fresh passing Environment result and matching assembly identity.

## Sound

Fourteen recorded CC0 effects replace the placeholder tones: firearm reports, reload actions, four concrete footsteps, impacts and quiet interaction cues. Reload start and finish are separate sounds. The selected packs contain one-shot effects; there is no harbor ambience pack, ambient drone or music loop, so the background stays quiet between events. M toggles sound. Source credits, original license notices and exact processing/hash provenance are in [Audio/CREDITS.md](Assets/Resources/Audio/CREDITS.md). Asset measurements do not establish human approval of the mix; the final build still needs listening and review.

## Shared rules and limits

When stamina is exhausted, holding sprint keeps you walking until the stamina bar fully recovers; it then resumes sprinting. This avoids rapid run/walk oscillation. The exported controller test verifies one transition to walking and one restart after full recovery; your judgment of movement feel remains separate.

Pistol rounds and individual shotgun pellets travel through space over simulation time. A shot can miss a moving target or hit intervening cover. Weapons have finite magazines, distinct firing intervals and interruptible reloads. The HUD separates loaded rounds from reserve. Enemies use the same projectile and magazine rules, with their own reaction and firing cadence.

The group shares delayed observations over limited range; it cannot fire at an unseen player's exact current position. Roles and last-contact labels in the test levels expose behavior for inspection. These labels are diagnostic aids, not evidence that every tactical decision is good.

The campaign stores magazine state, reload progress and in-flight projectiles. The new save is `streets-progress.json` in the existing Funstra save folder. Older `lights-progress.json`, `district-progress.json` and `progress.json` can be imported without overwriting their originals. Test levels do not save campaign progress.

This release establishes two firearm families and one bounded group encounter. The clinic is the first enterable interior; other buildings remain exterior landmarks. Dealers, broader weapon acquisition and investigations remain later work. Testing on this machine cannot establish low-end hardware compatibility. Your foundation approval requires both normal gameplay and these four special tests.
