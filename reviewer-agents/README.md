# Reviewer agents

Four independent perspectives on the same game. Different priorities can produce different
judgments; neither agreement nor disagreement is a target. A passing harness does not earn an 8.

| Profile | Archetype | Cares about | Score scale |
|---|---|---|---|
| [Dag Møller](dag-moller.md) | The Systems Archivist | Depth, systems, simulation honesty | 0–10, weighted |
| [Priya Raman](priya-raman.md) | The Feelings-First Critic | Theme, character, emotional payoff | 0–10, weighted |
| [Marcus Webb](marcus-webb.md) | The Consumer Advocate | Performance, bugs, value for money | 0–10, weighted |
| [Nell Okafor](nell-okafor.md) | The Cosy Hours Player | Comfort, routine, a nice evening | 0–10, weighted |

## Evaluation protocol v2 — 8 September 2026

Applies to future full reviews. Historical reviews, scores and dossiers retain their original
meaning; do not recalculate them or claim an old slice score was a whole-game score. The owner
requested this change because repeated high scores for narrow increments concealed how much
game was still missing.

Lead every review with **Game now G/10 · Slice delivery S/10 · Evidence E%**, then the most
important missing or weak experience. G is the primary rating of the playable game as it exists.
S measures delivery of this release's committed outcomes. E describes how much of that outcome
ledger the evidence actually resolves. Never average these three measures into one number.
Use one decimal for G and S, and whole percentages for E. There is no starting score, floor,
preferred range, automatic improvement bonus or required spread between reviewers.

### Game now: quality and completeness together

Score five categories from 0 to 10 in half-point increments. Use the fixed weights below;
weights sum to 100 for each reviewer. Do not move weights after seeing the results.

| Category | Dag | Priya | Marcus | Nell |
|---|---:|---:|---:|---:|
| Systems and agency | 35 | 15 | 15 | 15 |
| People and consequences | 15 | 35 | 10 | 20 |
| Control, clarity and comfort | 15 | 15 | 20 | 30 |
| Craft and reliability | 15 | 20 | 35 | 15 |
| Breadth and sustained play | 20 | 15 | 20 | 20 |

`G_raw = sum(weight × category_score) / 100`. Choose anchored category scores from evidence,
then expose the arithmetic: category, weight, score, weighted contribution, weighted deficit
from 10, reason and evidence. A category's deficit is `weight × (10 - score) / 100`.
List the concrete missing experiences responsible for the largest deficits. Do not begin at
10 and invent a deduction for every small bug, or deduct the same shortcoming again afterward.

| Category | 2: rudimentary | 5: functional but limited | 8: strong, substantially realised |
|---|---|---|---|
| Systems and agency | Rules/counters exist; few decisions connect. | A coherent outing offers interacting choices, but dominant solutions or shallow consequences restrict it. | Several viable approaches and resource pressures support repeated, meaningfully different decisions. |
| People and consequences | Labels, generic reactions or incident text stand in for people. | Some persistent individual motives and reactions change play; many relationships remain thin. | Recognisable people, commitments and remembered actions repeatedly change access, behavior and relationships. |
| Control, clarity and comfort | Basic operation requires workarounds or guesswork. | Ordinary play and stopping/resuming work; avoidable friction or inaccessible states remain. | Controls, information, recovery and session boundaries stay understandable in difficult situations. |
| Craft and reliability | Rough presentation or serious failures obscure play. | Generally usable build with uneven visual/audio craft, bugs or constrained technical evidence. | Coherent presentation and reliable operation across relevant conditions, with few material defects. |
| Breadth and sustained play | Isolated demonstrations or one thin activity exhaust the options. | A complete local loop offers some varied reasons to return, but repetition and limited world change are apparent. | Several connected activities, changed return visits and durable progression sustain self-directed sessions. |

0 means absent or unusable; 10 means exceptional and convincingly sustained in that category,
not merely all tests passing. Interpolate with an explicit reason. An absent category scores 0;
disconnected proofs cap its score at 2. A narrow working example without the connected depth
described above caps the affected systems, people or breadth category at 4. Good visual polish
can earn its own credit but cannot fill those missing categories.

Judge the current game against the durable pillars in VISION and DESIGN, not against every
speculative feature or an arbitrary commercial game's length. A small, varied, complete game
can score well. Missing crew agency, meaningful relationships or reasons for another outing
are missing game when they materially limit those pillars, even if their implementation is
scheduled later. Name that deficit; “out of scope,” “prototype” and “future work” explain a
development decision, not why the present experience deserves full credit. Do not subtract
one point for each unimplemented roadmap bullet, cosmetic variant or uncommitted idea.

### Slice delivery: explicit commitments and numerical deductions

Before the run, transcribe the assigned release ticket's observable outcomes into a short ledger,
including accepted review fixes and integration requirements. Link the frozen brief/build; mark
any later scope amendment and who authorised it. Weight each outcome **5 core**, **3 supporting**
or **1 polish**. Core means necessary to the stated player outcome. Group implementation details
under their player outcome; dozens of easy assertions must not outweigh one missing core loop.
The panel shares these outcome weights, not verdicts or prescribed attainment scores. If the
brief lacks a ledger, propose it before testing and retain it with the review checkpoint.

For each outcome, show status, attainment `a`, evidence, and lost points:

- **1.00:** delivered in the relevant player conditions, without a material shortfall.
- **0.75:** substantially delivered; a specific limitation materially weakens it.
- **0.50:** a useful part works, but a major promised branch or integration is missing/broken.
- **0.25:** a fragment or staged substitute exists; the promised player experience is largely missing.
- **0.00:** absent, unusable or contradicted by the observed build.

`S_raw = 10 × sum(w × a) / sum(w)`; each deduction is `10 × w × (1 - a) / sum(w)`.
Confirmed missing/broken core outcomes cap S at **5.0**. If the central end-to-end outcome is
unavailable, cap S at **3.0**. Show raw score, every applicable cap, binding cap and final score;
caps do not stack as extra subtractions. A confirmed unrecoverable progress-loss defect caps
both G and S at **3.0**, with the reproduction and affected save scope stated. A launch failure
leaves gameplay G **not assessable**, craft/reliability 0, and the failed launch/delivery outcomes
0; it does not justify inventing assessments of unseen gameplay.

These are commitments, not the entire roadmap. A deferred future feature reduces S only if it
was committed for this release and no authorised scope change removed it before review. It may
still limit G through a durable category. Describe that as two different questions, not two
deductions within one score. Example arithmetic, not a Funstra verdict: weights 5/3/1 with
attainments 0/1/1 yield S_raw 4.4; if the missing core is the central outcome, final S is 3.0.

### Evidence and uncertainty

For every slice outcome use `e=1` when the evidence adequately resolves the claim, `0.5` when it
resolves only part, and `0` when it does not resolve it. `E = 100 × sum(w × e) / sum(w)`.
Evidence of a failure can earn e=1: E measures coverage, not success. Direct transactions can
fully verify custody arithmetic; they cannot fully verify a promised controller journey.
Normal runtime, controlled fixtures, source, manual claims and judgment must remain labelled.
More assertion lines do not increase E. Uninspected team results are claims, not observed credit.

Distinguish **unverified** from **absent**. If an unresolved claim materially changes a score,
give its justified lower/upper attainment bounds and publish an S range marked provisional.
For an unsupported game category use NR and a bounded G range without redistributing its weight.
Do not assume it works, silently score uncertainty as a defect, or invent human enjoyment,
input feel, listening or minimum-spec performance from guided stills and capped runs. A valid
guided run is sufficient for a completed guided review; uncertainty limits particular claims,
not permission to file an honest review. Review current whole-game coverage too: identify which
G categories were revisited, which use inspected same-build evidence, and what remains unknown.

### Progress and required handoff

Include two ledgers (G categories and S outcomes), E, caps, the three largest present-game
shortcomings, and the next observation that could change each conclusion. Report G delta against
the last review under this rubric with the same weights. Compare S only on equivalent outcomes;
otherwise show added/delivered/missing/regressed outcomes rather than a misleading numeric delta.
For the first v2 review say **new baseline; historical scores not comparable**. Progress can be
real while G stays low or falls; fixes earn only the points their evidence changes.

The dossier and conference must present G and S side by side, coverage and material deductions,
and each reviewer's priorities. Preserve full reviews, earlier scores and later addenda separately.
Do not present four identical numbers as consensus about readiness, and do not force different
numbers for dramatic effect. Product-owner acceptance remains separate from every score.

## Axes of disagreement

- **Systems vs. meaning** — Dag and Priya split here on almost every narrative game.
- **Craft vs. state** — Marcus can sink a game the other three loved, on launch bugs alone.
- **Demanding vs. mean** — Nell reads as "unfriendly" what Dag reads as "respects the player."
- **Depth vs. first ten hours** — Nell reviews the opening; Dag reviews the endgame.

Each file follows the same sections: background, what they care about, blind spots, voice,
scoring behaviour, sample line. Keep that shape when adding a fifth.
