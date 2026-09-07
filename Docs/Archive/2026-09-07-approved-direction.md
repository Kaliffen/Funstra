# Funstra development direction

7 September 2026. [VISION.md](../../VISION.md) establishes the proposed game; [WORLD.md](../../WORLD.md) establishes its setting. The [original demo plan](2026-09-07-demo-design.md) is retained as history. It no longer defines the game's long-term limits.

## Review and decision

The product owner enjoyed the missions and Hot Cargo, but identified an absent power arc and insufficient sandbox, RPG, combat and simulation identity. Positive prototype feedback does not establish approval of the long-term direction. This vision is ready for review; implementation of another feature slice has not begun.

Creative decision: keep Old Port and its successful movement/theft/escape foundation. Establish persistent people and consequences before expanding content. The immediate question is whether one small operation can change both a person's future and the neighborhood's material state.

## What exists and what changes

| Current prototype | Intended evolution |
|---|---|
| Three sequential favors | An optional introduction to people and competing interests |
| Resetting cargo pickups | Owned stock transported, consumed and replenished through explicit actions |
| Cash and three perks | Skills, wounds, relationships, equipment and organizational capacity |
| Patrol loops and witnesses | People with duties, knowledge, affiliations and responses to specific events |
| One decaying heat meter | Immediate pursuit plus lasting consequences based on evidence |
| Arrest resets the excursion | Recoverable bodily, social and material consequences |
| Safehouse as bank point | Shelter, treatment, storage and eventually a staffed base |

Do not replace every prototype system at once. Existing missions and cargo remain playable while one contained situation proves the new model. Clearly distinguish legacy restocking cargo from the finite shipment during that transition.

## Next proof: A Bed and a Bandage

One Old Port situation establishes the transition from isolated survivor to a dependable partnership. This is the proposed next implementation slice, subject to vision review.

Neri, a clinic orderly, cannot obtain a small medical shipment held at Vico's garage. A collector has impounded it against clinic debt. Neri wants treatment to reopen; the collector wants payment; the supplier wants its stock accounted for. The goods and these interests exist before the player talks to anyone.

You can pay the posted release price, steal the shipment, or confront its guard and take it. Keeping or selling the contents is allowed. Extensive negotiation checks and branching cinematic dialogue are deferred. Three functioning approaches are more useful than six superficial buttons.

Without intervention, the collector sells the stock to another buyer on a disclosed schedule. The clinic consumes its remaining supplies treating patients and then suspends treatment when they run out. Those are inventory transactions and work decisions, not a quest-failure trigger. The actors and conflict continue afterward. Conversation and the journal expose the schedule so experimentation is not punished by a hidden deadline.

Helping Neri establish treatment creates a relationship and access to a refuge bed. Neri can agree to accompany you and provide basic stabilization. Harming or robbing Neri creates a different outcome. This is the first power reward: someone who helps you survive and has a concrete reason to do so.

### Smallest useful simulation

- One tracked medical stockpile, one shipment and a scheduled replenishment paid from an actual supplier reserve. The same shipment cannot reward multiple recipients. No instant restock on banking or arrest.
- Three persistent named participants: Neri, a collector and a guard. Each has an affiliation, inventory, current task and relationship facts. Existing passersby need not all become simulated citizens yet.
- One controllable companion with follow, hold, retreat and stabilize behavior. Recruitment follows the relevant relationship event, not a generic hiring menu.
- Minimal combat: a melee attack, one firearm, ammunition, sight, incapacity and bleeding. No body-part inventory or grafts in this slice. A bandage stabilizes; a stocked clinic and rest recover an injury.
- A short defeat path: robbery and injury if the guard incapacitates you; rescue if Neri can help. Clear surrender/retreat options and no permanent helpless waiting. Full custody simulation is deferred.
- A durable incident record: theft discovered, attacker witnessed, medicine delivered, person stabilized. Witness knowledge determines identification. Ending a chase cannot erase an identified attack. One local faction response is enough.
- A world clock and saves for these entities. Menus/tactical pause stop time. Loading restores stock, wounds, knowledge and relationships without rerolling outcomes.

This crosses several systems and is larger than Hot Cargo. Work in the dependency order below; reduce content count before removing persistence or consequences. No calendar deadline before the technical spikes are measured.

Recovery cannot depend exclusively on recruiting Neri or keeping this clinic open. Provide a basic fallback for a penniless, injured solo character, such as stabilization followed by a debt-financed bed elsewhere. It must carry a cost and a new decision without softlocking the campaign. Test that fallback with the shipment already sold.

### Implementation order and stop conditions

1. **Persistent state.** Stable IDs, owned inventories, named actors, clock, save migration and an incident log. Demonstrate a shipment changing owners and surviving reload. If it cannot coexist cleanly with the controller, isolate a scenario rather than rewrite the game.
2. **Bodies and response.** Prototype combat, sight, incapacity, stabilization and guard intent in a contained test scene. Test direct control plus tactical pause before adding weapons. Stop for control feedback if it does not feel good.
3. **The situation.** Connect payment, theft and force to the same stock and participants. Add recruitment and one lasting local response. Minimal authored conversation exposes the conflict and known consequences.
4. **Play and inspect.** Integrate into Old Port, verify alternate outcomes, communicate consequences and collect player approval. Then consider broader simulation.

No citywide economy, base construction system, generated districts, diplomacy tree, elections or additional campaign arc belongs in this proof.

## Simulation contract

Store gameplay state independently of scene objects. Rendering follows state; despawning a mesh cannot delete ownership or history. Begin with plain C# state and a modest scheduled update loop, not a framework migration or ECS rewrite.

Actions produce structured incidents with time, participants, location and observations. Systems react using their own knowledge and resources. Keep an inspectable cause chain for development and a concise journal of consequences the player knows. Do not infer a suspect from the global player reference.

Use affordable, explicit priorities. A guard can protect, investigate, pursue, retreat or aid; it does not need a general planner. Distant stockpiles update on scheduled ticks while nearby combat updates continuously. Prevent reactions from manufacturing resources, knowledge or reinforcements.

Version the new save format and import existing cash, jobs, perks and satchel. Verify migration against a fixture. Keep the current release runnable; never silently reset a save to simplify development.

## Acceptance evidence

Run a fixed initial world through separate outcomes: no intervention, payment, unnoticed theft, witnessed force and loss/retreat. Verify medical-stock and money conservation, appropriately different knowledge, and persistent consequences after reloading.

An autonomous baseline must produce an understandable result without accepting a quest. Both success and failure must leave the player able to act. A companion must demonstrably improve an outcome and be capable of needing help in return. A witnessed offense and an unseen shortage must provoke different responses.

The player should be able to answer: Who needs what? What did I change? Why did someone react? What can I do now that I could not do before? If those answers are unclear, more content is not the remedy.

## After the proof

Deepen this neighborhood: occupations and mutual dependencies, crew equipment and wages, a functioning refuge, rival plans and multiple uses for the same supplies. Later pursue business ownership, institutional conflict and stranger bodies. These are directions, not an approved backlog. Each addition must strengthen the power arc and interact with what already exists.

