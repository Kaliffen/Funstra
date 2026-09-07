# Funstra development direction

The [game vision](VISION.md) and [world origin](WORLD.md) were approved for implementation on 7 September 2026. The [approved implementation proposal](Docs/Archive/2026-09-07-approved-direction.md) and [original demo plan](Docs/Archive/2026-09-07-demo-design.md) remain as history.

## Current review candidate

**Demo 02: A Bed & a Bandage, version 0.2.0.** See [the playable scope and review guide](BED-AND-BANDAGE.md) and [validation evidence](Evidence/VALIDATION.md).

This demo implements the first transition in the intended power arc: you can gain a dependable partner by materially helping a person and their clinic. Payment, stealth, combat and sale affect one finite stock of medicine. An autonomous baseline changes supplies and ownership. Wounds, actor state, knowledge, relationships and goods persist. Existing campaign and cargo activities remain available.

The implementation uses plain serializable C# state with structured incidents. Rendering and scene objects follow that state. New saves import old campaign progression into a separate file. No framework migration or whole-project rewrite was needed.

## Deliberate limits of this proof

Companion orders control a medical support partner; direct character switching and companion attacks are deferred. Combat has one pistol and fists, bleeding, incapacitation and recovery; detailed body parts, death and custody are deferred. The finite simulation covers medical stock and relevant funds. Supplier trips and treatments are transactions, not fully animated labor. General equipment shops and cargo restocking still use prototype rules.

Residents and officers can be harmed and stabilized, with persistent wounds and positions. Their broader needs, relationships and faction plans are not simulated yet. There are no replenishing police forces, business ownership, political institutions or developed skill progression.

The small local system now has testable causes and consequences. It should not be described as the full city simulation in the vision.

## Acceptance gate

Implementation and automated validation are complete for this review candidate. Human acceptance of Demo 02 is pending. The next players should assess the first partnership, legibility of consequences, control feel, fairness under pressure and whether alternate approaches remain interesting.

The Hot Cargo reviewer-persona remarks were used as hypotheses, not as evidence of human testing. They prompted explicit extraction-interruption coverage, preservation of carried goods between sessions, a visible load penalty, clearer preparation costs and a review guide that separates real validation from remaining uncertainty.

## Next decisions after player feedback

Resolve control, pacing or recovery problems before expanding the content count. If the partnership succeeds, deepen the same people and location: useful skills, additional relationships, dependable work, a refuge and consequences for providing a service. The subsequent step toward a crew should add obligations as well as capacity.

Avoid expanding to a large map, a generic mission generator or a disconnected feature catalogue. New systems must explain their player decision, shared inputs and consequences, failure behavior, and how the player can understand them. Factional power should eventually arise from people, supply, services and coercion that already work at street level.
