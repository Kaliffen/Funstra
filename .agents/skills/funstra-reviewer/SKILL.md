---
name: funstra-reviewer
description: Review an actual Funstra demo through guided scripted gameplay, rendered screenshots, logs, manual and source inspection using the existing reviewer personas. Use for one reviewer, the four-reviewer panel, or a role-played review conference with durable reviews and meeting notes.
---

# Funstra reviewer

Review the game as close to playing it as the available tools support. **Gameplay on rails in the actual exported build is the intended method and is sufficient for a completed guided review.** Launch the demo's scripted routes, inspect what it renders and records, and use the manual and source to understand or challenge what happened. Exploratory keyboard/mouse play is not required. Describe this honestly as a guided build review, never as a human or freely explored playthrough.

## Recover the assignment

Read applicable repo instructions, the [persona index](../../../reviewer-agents/README.md), the assigned profile, and that reviewer's previous reviews and meeting notes. Resolve paths from the repo containing this skill; do not depend on a private transcript or a particular computer path.

| Persona | Profile | Review lens |
|---|---|---|
| Dag Møller | [dag-moller.md](../../../reviewer-agents/dag-moller.md) | Choices, economy, simulation depth and legibility |
| Priya Raman | [priya-raman.md](../../../reviewer-agents/priya-raman.md) | People, theme, atmosphere and the meaning of consequences |
| Marcus Webb | [marcus-webb.md](../../../reviewer-agents/marcus-webb.md) | Bugs, controls, recovery, performance and value |
| Nell Okafor | [nell-okafor.md](../../../reviewer-agents/nell-okafor.md) | Comfort, routine, clarity, session boundaries and attachment |

Use their existing voices and standards without prescribing scores or manufacturing disagreement. Their fictional biographies do not establish real hardware, elapsed play time or experiences in this run.

Respect the requested stage: an expectations conference does not start tests; a single-reviewer assignment does not launch the whole panel; a review request does not authorize game fixes or publication. Use the release/review ticket as the checkpoint, finding an existing issue before creating one. Record the assigned build, reviewer, phase, evidence locations and next action so another turn can resume.

## Identify and demonstrate the build

- Use the executable handed over by the user/CD. For “latest,” resolve the actual candidate once and record it; do not switch because another build appears during the panel. Record executable path, version if available, SHA-256 of the executable and gameplay assembly (Unity's executable alone may not identify changed game code), and archive hash when reviewing a package. Record source commit and dirty state separately: the checkout may differ from the binary.
- Read its review guide/manual and the current test runner before launching. Inspect paths, supported modes, flags, save handling, output locations and timeout behavior. Existing entry points include [Test-Demo.ps1](../../../Tools/Test-Demo.ps1) and [Test-Player.ps1](../../../Tools/Test-Player.ps1); their hardcoded target paths can differ. Do not assume a runner accepts an executable override or automatically targets the requested build.
- Use existing scripted runtime and visual routes that cover the demo. A rendered run is required for a full guided review. Keep the process responsive to user updates while waiting. If the runner targets another build, use only verified supported flags against the assigned executable or ask the CD for a matching runner; do not quietly rebuild or replace the review candidate.
- Isolate saves and outputs using supported mechanisms. Read the harness's save behavior before running it. Where runners share files, serialize runs and preserve each completed run's evidence in a unique reviewer/build/run directory before the next run. Do not overwrite another reviewer's evidence or a real player save.
- Record commands, start/end times, exit status, result counts and raw log paths. Require fresh outputs from that run; stale PASS files and old PNGs do not count. Inspect the full relevant logs, including their tails, and open rendered screenshots with an image-capable tool. File counts and non-black checks alone do not establish readable or correct visuals.

Default panel operation is one agent per persona, scheduled sequentially when using shared game/evidence state. Give each the same build identity and brief, its own profile and history, and the runner constraints. Each launches a guided pass and reaches its initial verdict independently of the other current reviews. Repeated identical routes are independent critiques, not four different gameplay paths. If the user asks to share a recorded run, identify it as shared evidence rather than four fresh runs. If delegation is unavailable, provide explicitly sequential persona assessments without claiming independent agents.

## Judge what was actually shown

Keep the review about the player experience. Technical inspection supports the verdict rather than replacing it with an assertion-count report. Relate observations to decisions, consequences, readability, people, pacing and the persona's priorities. Compare previous concerns only when the old and new evidence supports the comparison.

Maintain a small evidence ledger, with each material finding linked to a run/log/frame or source location and one of these bases:

- **Runtime observation:** the scripted build actually exercised the behavior. State whether the hook moved through gameplay or directly set state/called a transaction.
- **Rendered observation:** a frame shows the UI/world/dialogue. Disclose staged setup or teleports; a still does not prove motion, timing, input feel or an entire causal sequence.
- **Source inspection:** a code path suggests or explains behavior. If source-to-build correspondence is unverified, say so; reading a conditional is not a runtime test of both branches.
- **Manual claim:** documented intent or controls that were not independently observed.
- **Judgment or hypothesis:** the persona's interpretation, preference or suspected issue, with the observation that prompted it.

Passing automation does not prove human difficulty, long-term economy, enjoyable input feel or minimum-spec performance. Record measured hardware and timings only when actually observed. A frame cap is not a different hardware test. Quote only dialogue actually present in inspected evidence and distinguish source-only dialogue from dialogue rendered on the route.

For an important uncertainty, replay the smallest existing route that can resolve it. If no route covers it, record a coverage gap or request a narrowly scoped demo hook through the CD/ticket. Do not alter the game's rules to make a review pass. A failed run can support a technical failure finding; a screenshot-only or source-only assessment remains partial if no valid guided run is available. Stop repeated identical failures without new evidence and report the concrete blocker. Missing exploratory control alone is never a blocker for this workflow.

## File the verdict and continuity

Save local records next to the existing persona, under `reviewer-agents/<persona-slug>/`, following current naming conventions. Use a new build/revision suffix when a filename already holds a completed review. Preserve the original full review and score; corrections and changed verdicts belong in a dated addendum or separate replay review.

A full review contains:

- Persona, demo/build identity, date, and method: **guided scripted build review**.
- The routes actually run, inspected evidence, supporting source/manual checks, and material coverage limits.
- A readable review in the persona's voice, concrete strengths and concerns, and a justified score using the profile's convention. Scope the score to this demo; do not force a target range or invent long-term experience to justify it.
- Actionable findings with evidence, reproduction conditions where known, player impact, and a distinction between confirmed bugs, suspected issues, coverage gaps and taste.
- A concise verdict, what changed since the previous review, and what would merit replay. Nell's comfort judgment and Marcus's actual hardware limits belong here when relevant.

Write a build-specific meeting-memory record with expectations, what was actually learned, unresolved concerns, score changes and the next review's questions. These are versioned project notes, not hidden personal memory. Link the full review and evidence from the review/release ticket. Completion means the guided review and records are delivered, not that the game passed or the user accepted the demo.

## Run the conference

The user is the organizer. Use the established conversational format: named speakers, distinct voices, light stage directions and room for their questions. Do not fabricate organizer decisions or speak approval on the user's behalf.

**Before the demo:** personas know the announced concept and their recorded previous experiences. Let them express hopes, skepticism and questions in player language. Do not leak new source findings or technical opinions about code they have not reviewed into this scene. Expectations are not verdicts.

**After the guided passes:** each presents its actual verdict and evidence-backed impressions. Let them respond and disagree naturally. Translate implementation details into what they mean to a player; keep commands and raw evidence in the linked reviews. A request for an off-script explanation gets a factual explanation of the harness method, then the conference resumes at its existing stage.

Before concluding, surface whether an unresolved claim warrants another targeted run. Keep revisions attached to the correct build. Consolidate the panel's agreements, disagreements, bugs, coverage limits and suggested improvements for the CD, linking the unedited reviews. Use local repo artifacts for dossiers unless publication is requested. The CD chooses which creative feedback to integrate; reviewers do not rewrite their scores to match that decision. Publishing and human acceptance belong to the wider release loop.

## Deliver the review dossier

**Produce exactly one consolidated HTML review dossier per demo, after all four reviewers have completed their reviews**, alongside their individual reviews and meeting notes. A conference transcript alone does not complete the handoff. Single-reviewer passes and expectations-only conferences produce no separate dossiers. While reviews are pending, record progress in the review/release ticket; never fill missing panel seats with invented verdicts. Later conference conclusions, CD decisions, fixes and replay addenda update this same demo dossier rather than creating additional dossiers.

Use the existing case-file format in [Demo 03's dossier](../../../Docs/funstra-review-dossier-demo03.html). Save one versioned file per demo under `Docs/` (for example `funstra-review-dossier-demo03.html`); preserve Demo 02's original `funstra-review-dossier.html` and all previous demo files. Label each dossier with its demo/build identity and review date. Follow the existing editorial design and keep it readable when opened locally.

Include the four attributed scores and verdicts, faithful summaries linked to full unedited reviews, concrete strengths and concerns, conference agreements and disagreements, evidence links and coverage limits. Keep confirmed bugs, suspected issues and creative suggestions distinct. Include a CD disposition section with accepted/deferred/declined feedback and ticket links; leave decisions pending until the CD actually makes them. Preserve review-build identity when later fixes or replays target a different candidate.

Open the finished HTML in a browser and inspect its rendered layout, navigation and evidence/review links before handing it over. Provide a clickable local file link and attach it to the release/review ticket. The site configuration selects the current demo dossier for publication as `dossier.html`; the CD's authorized publication stage must verify the published dossier matches the intended review cycle. Creating a local dossier does not itself publish it.


## Release history contract

The owner-approved policy keeps the latest five published demos on GitHub and the website, or all while fewer than five exist. The CD/publication workflow removes older release records and assets; source tags and per-demo review dossiers remain. When reviewing the release handoff, check that Previous versions links to the retained downloads and release notes. Preserve historical reviews even after their binary expires from public retention; identify unavailable builds honestly rather than silently substituting the latest. A review-only invocation does not itself run cleanup or publish.
