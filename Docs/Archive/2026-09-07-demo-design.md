# Funstra — a small-time crime story

Playable Windows demo. Unity 6.4, built-in renderer. Original procedural low-poly art.

You arrive in the port town of Funstra owing a favor to Mara, a fence at the night market. Work three jobs, lose the police, and earn your way out.

## Demo boundary
- Six blocks: market, apartments, garage, arcade, depot, waterfront.
- Isometric 3D, camera-relative WASD, sprint, sneak, hold E interaction.
- Mara offers three sequential jobs: lift a courier bag, open a garage lockbox, steal the depot ledger.
- Pedestrians walk circuits. Police patrol, witness theft, pursue via a walkable grid, search the last seen position, and give up after losing sight.
- Break sight behind buildings; hide in marked cover while unseen. Being caught loses carried loot and a small cash fine; the current job remains retryable.
- Deliver each item to Mara only when police heat is clear. Cash, reputation, and one permanent perk choice after each of the first two jobs.
- Third delivery ends the story. Restart available. Save at deliveries and perk choices.
- Title, tutorial, HUD, minimap, pause/settings, credits, win screen, audio cues.

## Build plan
1. Create project, city geometry, movement, camera, and readable visual language.
2. Add mission state, interaction, NPC simulation, pursuit and escape, progression and persistence.
3. Build Windows player; verify mission transitions, recovery, navigation and runtime rendering; capture evidence.

## Deliberate limits
No driving, combat, interiors, multiplayer, generated dialogue, or large economy. Stylized human figures and authored blocks keep the prototype self-contained and editable. The small simulation exists to make the theft/escape loop react to the player.

## Next slice: Hot Cargo
Implemented optional cargo extraction, bag weight, a satchel purchase and post-campaign free roam. See HOT-CARGO.md for design, tradeoffs and a review route. This extends the original demo boundary above.
