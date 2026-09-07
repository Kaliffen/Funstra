# A Bed & a Bandage (Demo 02) — playtest review

**Verdict: competent, legible where it matters, and the doses really do add up. 7/10.**

## What I actually ran

Working directory `~\repos\Funstra`, build under test `Build\BedAndBandage\Funstra.exe`
(version 0.2.0, Assembly-CSharp.dll SHA256 `C240CFE2…6DF4EEB` per `Evidence\demo-02-build-info.txt`).

- `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Full -Visible` — the scripted
  scenario runner drove a real character controller through the clinic/collector/companion/combat
  content. 52 lines of console output, one bare `PASS` header plus 51 named assertions, zero FAILs.
  Fresh evidence: `Evidence\bandage-runtime-result.txt`, `Evidence\bandage-full-player.log`.
- `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Visual -Visible` — regenerated
  the twelve B01–B12 capture screens at 1280×720, `Evidence\bandage-visual-result.txt`.
- Read `Evidence\VALIDATION.md`, `Evidence\demo-02-build-info.txt`, and, because I don't take a
  design document's arithmetic on faith, the actual conservation checks in
  `Assets\Editor\DistrictRules.cs`.
- Viewed the screenshots directly: B03 (Neri's dialogue), B04 (street map), B05/B06 (combat and
  tactical pause), B07/B08 (partnership and history log), B10 (defeat/recovery), B12 (extraction).

Both harness passes completed on the first attempt with no manual intervention, no hung process, no
retry. That alone is worth a sentence, because the previous demo's own documentation admits its
hidden-window test mode used to time out; this runner switched to an actual headless player
(`-batchmode -nographics`) to dodge that, and on this pass it did. I'd rather report a boring, clean
run than manufacture drama, so: boring, clean run.

## The medicine economy — the part I actually came for

The design prose claims the initial twelve doses (six impounded, two clinic stock, four supplier
reserve) are "conserved across stockpiles, deliveries and consumption." That is exactly the kind of
line that gets written by someone who trusts their own spreadsheet, so I went looking for where it's
actually checked rather than asserted. It is checked, and better than I expected from a project this
size. `DistrictRules.cs` runs a `TotalMedicine==12` assertion after an idle 721-second autonomous
tick (the buyer sale executing on schedule, the clinic self-treating and self-restocking with no
quest ever accepted), again after stepping the same 721 seconds in 7,210 increments of 0.1s to prove
the total doesn't drift depending on how coarsely you sample time, again after a paid release and a
clinic treatment, and again after a violent seizure and confiscation. Four independent checkpoints
across four different play paths, all landing on the same closed number. That is a real ledger, not
a narrative gesture toward one — it's the same discipline you'd want from a hunger clock in the
immersive-sim tradition, checked at the arithmetic level rather than trusted at the UI level, and
it survives the one stress test that actually matters for a scheduled-tick economy: decoupling the
result from your timestep.

Where it gets thinner is visibility to the player. The rule-level result file
(`Evidence\district-rules-result.txt`) collapses all 32 of those checks into a single summary line —
"PASS: 32 district simulation, conservation, recovery and migration assertions" — with no per-check
itemization, unlike the runtime file, which names all 51 of its assertions individually. I had to go
into the C# to find out what was actually being tested; a player never will, and neither will most
reviewers. The runtime pass backs the same claim with exactly one composite assertion, "Complete
runtime outcome sequence conserves all medicine," which is fine as a smoke test but tells you nothing
about which of the twelve individual doses moved where. In play, the number I could actually see was
"CLINIC 2 DOSES / 0 PATIENTS TREATED / TRUST 0 — Supplier reserve: 4 doses. Clinic funds: $20" (B03).
That's honest and it's more legible than most economy HUDs manage at this scale, but the twelve-minute
shipment clock itself is nowhere on screen. I looked across all twelve B-series captures for a
countdown, a progress bar, anything — the top-left clock reads a fixed in-fiction time and never
shows the twelve-minute window closing. A system this well-audited under the hood deserves a
surface the player can actually read without opening the history log and hoping.

Which brings me to the history log (B08), which is the demo's one real UI failure. Three narrative
beats — Neri joining, the clinic donation, the shipment theft — are each timestamped "00:00." Either
the clock resets per node for cosmetic reasons or the log genuinely isn't tracking elapsed time, and
either way a player trying to reconstruct "how much of my twelve minutes did I burn deciding" gets
nothing from the one screen built to answer that question. This is not a fatal flaw, it's a rounding
error against an otherwise disciplined system, but it's the kind of rounding error I dock for on
principle, because a hidden clock you can't read is functionally the same as no clock at all for
planning purposes — and the entire point of a scheduled state change is that the player should be
able to feel the deadline pressing, not discover after the fact that the buyer already came and went.

## The shipment clock and witnessed theft — real pressure or cosmetic

The clock itself is real, in the sense that it changes outcomes, not just flavor text. `PayRelease`
costs $100 before the buyer's sale executes and the design doc states $140 after — a genuine
40-percent tax on dawdling, and the rule check confirms the buyer sale actually completes
autonomously at the 721-second mark with money changing hands (`buyerMoney==100&&collectorMoney==100`)
whether or not the player has done anything. That's the correct shape for a scheduled state change:
it doesn't wait for you, and the harness proves it doesn't wait for you. The runtime suite backs this
up further downstream — "Late acquisition from the buyer" is listed among covered scenarios, meaning
you can still get the case after the sale, just worse off, rather than the content simply vanishing.
No content lockout, a real cost for missing the window. Good.

The witnessed/unwitnessed split is likewise a genuine branch, not a coat of paint. The rules layer
distinguishes `TakeShipment(false)` (unwitnessed) from `TakeShipment(true)` (witnessed) with different
downstream state: an unwitnessed theft produces `discovered&&!identified` — the collector notices
stock is missing without ever pinning it on you — while the witnessed path produces a persistent
`identified` flag that survives ending the immediate chase (`"Ending pursuit does not erase
identity"`) and, per the runtime suite, survives a reload as well ("Reload retains stolen medicine
and distinct witness knowledge"). The runtime pass independently exercised the live version of this:
"Actual unobserved hold creates missing stock without identifying thief" and "Reload retains stolen
medicine and distinct witness knowledge" both passed against the real guard AI, not just the rule
harness. Restitution ($60, confirmed both in rules and in the design prose) buys the grievance down
without erasing the police/pursuit system, which the doc is careful to note is a separate track — and
the harness has a dedicated assertion for exactly that separation ("Clearing immediate heat preserves
collector memory"). That's the correct relationship between a short-term alert state and a long-term
reputation ledger, and it's rarer to see done right than the genre likes to admit — I've watched more
than one immersive sim conflate "the guards stopped looking for you" with "the guards forgot you
exist." This one keeps them apart on purpose and tests that they stay apart.

## Combat — does the sight-line and ammo talk survive contact

Fists and one pistol, twelve rounds, eighteen-meter unobstructed range, a guard with a finite
magazine who searches a last-seen location instead of tracking through geometry. I went at this
adversarially, because "the guard doesn't shoot through walls" is a sentence design documents love to
write and codebases frequently fail to enforce once the player starts standing in doorways. The
runtime suite has four assertions aimed directly at the seams: "Hostile guard fires through clear
sight and causes bleeding," "Building blocks attack without consuming ammo," "Guard cannot shoot
player through a building," and "Empty-magazine guard strikes nearby visible companion rather than
distant player." That last one is the interesting one — it's not just "the guard can't cheat," it's
"the guard degrades sensibly when its ranged option is spent," falling back to melee against whichever
valid target is actually reachable rather than either standing idle or teleporting a hit through
cover. That's a better AI-failure state than most demos this size bother to model, and it's the kind
of emergent-adjacent behavior I actually go looking for. "Ammo not consumed on a blocked shot" is a
small, correct detail that a lot of amateur combat systems get backwards, charging the player (or the
NPC) for a shot the geometry itself refused to deliver.

"Three actual pistol shots spend three rounds" is a plain, honest assertion and I have no complaints
about it beyond noting that it's the kind of thing that should never need saying in a functioning
system and yet regularly isn't true in ones that aren't. The live-fire capstone — "Live gunfight
wounds player and incapacitates guard without scripted damage" — is the assertion that actually
matters here, because it says the numbers in the final combat outcome came from the real damage model
running against real AI decisions in that test pass, not from a scripted cutscene dressed as combat.
B05 and B06 back this up visually: B05 shows a live selection of "ROOK / 80 HP" with the player at
58/100 and bleeding, mid-encounter, and B06 shows the tactical pause overlay explicitly stating
"Buildings block shots" as a rule the HUD itself teaches you, not a hidden mechanic you have to
discover by dying to it first. I did not, and could not from a scripted harness alone, verify whether
a sufficiently patient player can bait the guard into a search pattern predictable enough to route
around entirely — the harness proves the guard searches a last-seen location rather than omniscient
tracking, which is the right primitive, but it doesn't tell me how forgiving that search radius is
under sustained abuse, and that's exactly the kind of long-session cheese question no scripted pass
can answer. Flagging it as unresolved rather than pretending the harness settled it.

## Neri — capability or icon

The design guide poses its own question here almost verbatim as I would have asked it: did you feel
a new capability, or receive another companion icon. On the runtime evidence, it's a capability,
modestly scoped but real. Neri pathfinds the streets to follow you (not a teleporting leash), holds
position on command, retreats to the clinic, and — the part that actually matters mechanically —
autonomously spends a finite dressing to stabilize the player's bleeding without being ordered to,
confirmed both at the rule level (`NeriAid()` reducing `neri.bandages` from a starting stock) and at
the runtime level ("Nearby companion autonomously stabilizes bleeding with finite supplies"). A
companion that consumes its own limited resource to help you is a companion with a cost structure,
which is the difference between an ally and a UI ornament. Neri can also go down and needs the
player's own aid in return ("Downed-companion rescue," confirmed live as "Actual E interaction
stabilizes downed companion"), which closes the loop symmetrically rather than making the companion
either invincible or a pure liability.

The honest caveats, which the design doc states more plainly than most marketing copy would: Neri is
not directly pilotable and does not attack. This is medical support, full stop, and calling it a
"partnership" is accurate rather than inflated. Whether that's enough to carry the promised "power
arc" over a longer campaign is a question this eight-minute scripted slice cannot answer and I won't
pretend it can — my genuine reservation is that a support-only companion is one idea, well
implemented, and the demo doesn't yet show whether it's the first of several or the whole of the
design. For what's here: better than an icon, not yet a system.

## Where I'd dock further on a longer look

The screenshot HUD across all twelve captures shows "REPUTATION: NEW FACE" and "$0" cash sitting
static regardless of what combat or economic state the rest of the screen displays — I can't tell
from a twelve-image capture pass whether that's a genuine gap in reputation feedback or an artifact
of how the scripted scenario staged its screenshots, and I'd want a longer human session before
either crediting or blaming it. Similarly, the visible UI never surfaces the shipment clock as a
countdown, which I've already flagged above as a real legibility miss rather than a nitpick — a
scheduled state change the player can't see coming isn't pressure, it's a surprise, and those are
different design tools with different effects on decision-making. I'd also want more than one
scripted run's worth of evidence on the guard search-radius question before calling the "witnessed"
branch airtight against a determined cheeser, because a rule that holds under one adversarial pass
and a rule that holds under twenty are not the same claim.

## The number

Seven. The finite-resource ledger is genuinely closed and independently checked against timestep
variance, which is more rigor than this scope usually gets and more than I expected walking in; the
witnessed/unwitnessed and clock-tax systems create real, tested consequences rather than flavor text;
and the combat sight/ammo rules hold up against the specific adversarial questions I know to ask of
this genre. It loses ground for shipping that discipline behind a HUD that won't show the player the
clock they're supposedly racing and a history log that timestamps three different events identically
at zero. Competent engineering, and a legibility problem sitting directly on top of it — which is, for
this beat, the single most common way a good system fails to read as one.
