# Keep the Lights On (Demo 03) — Marcus Webb playtest review

**Verdict: 8/10.** First time I've moved off 7. Clean technical state, a real regression
check that came back clean across all three demo generations, and a HUD stat I almost wrote
up as the same dead-readout bug I caught last time — until I read the source and found out
it isn't one. Read to the bottom before you assume that's me going soft.

## What I actually ran

- Working directory `~\repos\Funstra`, build under test
  `Build\KeepTheLightsOn\Funstra.exe` (0.3.0, per `Evidence\demo-03-build-info.txt`).
- `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Full -Visible` — a real
  visible window, real character controller. Came back **78/78 pass, 0 fail**, output in
  `Evidence\bandage-runtime-result.txt` and `Evidence\bandage-full-player.log`, both fresh
  from this run. Matches Dag's count from his own read exactly — good, that's what
  cross-checking is supposed to look like.
- `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Visual -Visible` —
  `Evidence\bandage-visual-result.txt`: "Twelve new demo screens captured without black
  rendering" and "Twelve Demo 03 review screens rendered." Confirmed 24 PNGs on disk (B01–B12,
  C01–C12).
- `Evidence\refuge-rules-result.txt`: "PASS: 26 refuge, dialogue, schedule and prop route
  assertions" — matches what Dag read.
- Then, because neither of the above two passes exercises the *original* Hot Cargo campaign
  (per `VALIDATION.md`: "Its isolated mode disables the medical extension"), I ran
  `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Legacy -Visible` myself.
  This is the one nobody asked me to run. It replays the three-job cargo campaign, satchel
  purchase, arrest/pursuit, and citizen movement in isolation, against the same 0.3.0 exe that
  now also has a refuge, a companion, guard AI, and four traffic cars layered on top of it.
  Came back clean: every assertion from "Actual hold interaction acquired job 1" through
  "Three jobs reach ending with $740" through "Town agent walks its route" — same shape as the
  original Hot Cargo 88-assertion pass I ran on the very first build I ever tested.
- Read both player logs to the tail (`bandage-full-player.log`, 117 lines;
  `bandage-visual-player.log`, 41 lines) and grepped both plus the run output for
  warn/error/exception/abort — nothing in either. Confirmed `[D3D12 Device Filter]` and
  `Direct3D:` blocks in the log both report the same **NVIDIA GeForce RTX 4090**, same as
  every build before this one.
- Viewed nine screenshots directly: B01 (title), B05 (combat), B12 (extraction), C01
  (clinic/Tally), C06 and C07 (refuge management, two states), C09 (Ivo post-settlement),
  C10 (history log), C11/C12 (traffic before/after).
- Read `Assets\Scripts\RunState.cs`, `Assets\Scripts\FunstraUI.cs` and
  `Assets\Scripts\FunstraSmoke.cs` directly to chase down the HUD question below — not
  because I doubted the pass count, but because a screenshot that looks frozen and a stat
  that's actually frozen are two different claims, and I got burned assuming they were the
  same thing once already.

Three harness passes, all clean on the first try, no retries, no hangs. I'm with Dag on not
making a production of that by itself, but three-for-three across Full, Visual, and a Legacy
pass nobody explicitly told me to run is worth the sentence.

## The regression check — this is the one I actually came for

My standing complaint on this project has been "more infrastructure, same ceiling," and my
standing worry has been "does the old stuff survive the new stuff getting bolted on." This
build is the first real test of that worry, because it's the third generation of features
sitting on the same city: Hot Cargo's job/cargo/pursuit loop, Demo 02's companion/medical/
combat layer, and now a refuge economy, a care-policy branch, and four physically simulated
traffic cars, all coexisting in one exe.

I ran the original Hot Cargo campaign — job 1 through job 3, satchel purchase, alarm/pursuit,
arrest, citizen movement — in isolation against this 0.3.0 build, the same way I ran it
against the very first build I ever tested. It came back exactly as clean as it did that
first time: `Three jobs reach ending with $740`, `Actual arrest confiscates loose cargo`,
`Officer sees and pursues wanted player`, `Unseen heat expires`, all present, all passing.
That's the oldest, least-exercised layer of this codebase, and it hasn't rotted under two
demos' worth of new systems getting stacked on top of it. I don't say this lightly: this is
exactly the kind of thing that quietly breaks in real projects when nobody's watching the
old path, and here, someone was.

The Full pass separately re-confirms companion following (`Companion navigates streets to
follow player`), combat (`Live gunfight wounds player and incapacitates guard without
scripted damage`, `Actual pistol attack consumes ammo, wounds target and records witnessed
offense`), and extraction (`Interrupted extraction can be retried and pays exactly once`,
`Becoming wanted cancels an in-progress extraction`) all still working with the refuge and
traffic systems live in the same session. Nothing here regressed. That's the headline, and
it's a better headline than either of my first two reviews got to write.

## The dose ledger and the reserve threshold — I'm not re-deriving Dag's work

Dag already opened `DistrictState.cs` and traced the reserve-policy comparison operator line
by line. I have no reason to redo that homework, and I'm not going to manufacture a
disagreement with a correct read just to look independent. What I'll add from my own pass:
C06 and C07 show the fund arithmetic progressing exactly the way the source and the panel
both claim — $80 after the initial repair split, $128 after a further $30 contribution — and
`refuge-rules-result.txt`'s 26-assertion pass includes the same fund-conservation checks Dag
cited. Source, panel, and screenshot all agree with each other on this pass too. Good sign,
same as last time I checked something this closely.

## The HUD stat I almost got wrong

Here's the part I want to walk through carefully, because it's exactly the seam I exist to
check and I nearly filed the wrong verdict on it.

Every single screenshot in this build — B01 at the start with $0 cash, B12 after banking $170
of freeform cargo, C07 and C09 and C10 after combat, a settled Ivo offense, and a repaired
refuge with $80–$128 in the fund — shows **MARA / STANDING: NEW FACE** in the top-right HUD,
never moving. That is the exact symptom shape I flagged in Demo 02: a stat sitting frozen
next to a screen full of other things that plainly changed. My first instinct was to write
this up as the same bug, recurring under a new label — the design doc even says outright
"the old campaign rank is labeled MARA / STANDING," which reads like confirmation that this
is the same stat I already caught not updating.

I went to the source instead of trusting that instinct, which is the discipline I said last
time I'd hold myself to. `FunstraUI.cs` line 110:

```
Text("MARA / STANDING",...); Text(State.completed==0?"NEW FACE":State.completed==1?"RUNNER":
State.completed==2?"TRUSTED":"CONNECTED",...);
```

`State.completed` (`RunState.cs`) is the count of numbered Hot Cargo campaign jobs delivered
to Mara — the *original* three-job structure, not the freeform cargo-running loop that
produces the "$170 / 5 load" readout on the HUD's left panel. Those are two different
systems: one increments `completed` (and therefore MARA/STANDING) on delivery to Mara through
`Jobs.All[]`; the other is the standalone cash-and-weight loop this Demo 03 review scenario
actually exercises via `FunstraGame.cs`. The Full and Visual harness passes never call the
job-delivery path — they run the refuge, medical, combat, and traffic content, all of which
sits on top of the freeform loop, not the numbered one. `State.completed` legitimately never
leaves zero in this scenario, so MARA/STANDING legitimately never leaves NEW FACE. It isn't
dead wiring. It's an untested path in this particular review scenario, and I confirmed that
by running `Test-Demo.ps1 -Mode Legacy` myself and watching `Delivery advances job 1`,
`Delivery advances job 2`, and `Delivery advances job 3` all pass on this same 0.3.0 build —
proof the stat does move, correctly, when the actual mechanic it's tied to gets exercised.

Compare that against what I actually did find working correctly: C09's dialogue shows Ivo's
live grievance state (`IVO KNOWS YOUR FACE / $60 restitution`, in red) before settlement, and
C06/C07/C10's left panel — captured after the settlement in the same playthrough — reads
`NO IDENTIFIED OFFENSE WITH IVO`. That's the exact stat-class I called out as broken in Demo
02, and this time, on a genuinely comparable live event, it updates correctly and I watched it
happen across two screenshots taken at different points in the same session.

So: not a recurrence of the old bug. A different, better-explained situation that merely
looks identical from a screenshot pass alone, sitting next to a fix for the actual class of
bug I flagged last time. I'm crediting both things — the real fix, and my own choice to check
the code instead of pattern-matching the symptom — but I'm also flagging the gap that nearly
tripped me: nothing in the 24-screen evidence set demonstrates MARA/STANDING moving off NEW
FACE. A less careful pass than mine reads this build and reports a regression that isn't
there. That's a documentation/evidence gap worth a one-line fix (one screenshot with
`completed>0`), not a code bug.

## Traffic and hardware — same shape as Dag's read, my own angle on it

I'm not re-litigating the junction-logic source read; Dag already did that and I agree with
what he found reading `CityTraffic.cs` — a genuinely simple conservative model with no hidden
escape hatch, exactly as the doc discloses. What I'll add: C11→C12 shows the same two cars at
visibly different points along their routes between captures, and the `bandage-full-player.log`
line for all four cars reports distinct, precise distance-traveled figures (455.4789m,
451.6013m, 446.5782m, 448.4521m) consistent with a real accumulator rather than a static
prop. Real movement, not a diorama, confirmed independently.

Hardware: unchanged, still worse in spirit than the last time I said this. Same RTX 4090,
same `[D3D12 Device Filter] Device Name: NVIDIA GeForce RTX 4090` in the log, same disclosure
in `VALIDATION.md` up front ("No lower-tier physical GPU was available"). This is now the
third build in a row with zero new hardware coverage, and it's the build that adds the most
new simulated load yet — four cars each running a per-frame junction/pedestrian/other-car
bounding-box check, on top of the companion and guard AI from Demo 02. The team keeps
disclosing this rather than hiding it, which I'll keep crediting in the same breath as I keep
noting that disclosure isn't the same as coverage. Three builds of "we said so" is starting to
look less like honesty and more like a gap nobody's prioritized closing.

## Build noise, for the record

`Evidence\build.log` still carries the same four `abort_threads: Failed aborting id: ...`
lines during editor build/teardown that I've now seen on every build I've tested. Doesn't
touch the shipped player, doesn't show up in either player log I read this pass, doesn't cost
points. Consistent is at least consistent.

## Where I'd still dock

- **The MARA/STANDING evidence gap** above — not a bug, but the kind of thing that would read
  as one to anyone who didn't go read the source, and this project has enough moving parts now
  that "read the source to be sure" shouldn't be the only way to tell a benign gap from a
  regression.
- **Hardware coverage, unmoved for the third consecutive build**, on the build with the most
  new simulated load yet. Not a new penalty — I said last time I'd watch for movement, and
  there wasn't any, again.
- **I'm not re-verifying the 133 editor-side assertions** (75 existing + 32 medical + 26
  refuge) — that needs the Unity Editor CLI, which I don't have wired up here. I read the two
  new rule-file sources directly for my own confidence, same discipline as before, but the
  editor-suite number itself is `VALIDATION.md`'s claim, not mine.

## Reconciling with Hot Cargo (7/10) and Demo 02 (7/10)

Both of those got a 7 for different reasons — Hot Cargo because a clean machine had thin
content around it, Demo 02 because a bigger machine had one new, small, honest-to-catch crack
in its dashboard (the REPUTATION stat). This build is the first one where I can point at a
concrete, previously-flagged problem and say it's actually fixed on the exact stat I flagged
it on, not just "generally improved" — Ivo's grievance line clears correctly now. It's also
the first build where I've run a real regression check spanning all three demo generations and
gotten a clean answer instead of an inference. Those are two separate, real reasons to move
the number, not one review being generous because I liked the vibe.

It doesn't get higher than 8 because the hardware gap is now three-for-three unaddressed on
the build that needs it checked the most, and because the evidence set has a hole that
produced a false alarm in my own process — if it fooled me for five minutes with the source
open, it'll fool a reader who never opens the source at all.

## Score breakdown

- Technical state: clean. Three harness passes (Full, Visual, and the Legacy regression check
  I ran on my own initiative), zero fails, zero crashes, zero exceptions in either log I read
  to the tail.
- Regression discipline: the best result yet. The original three-job campaign, companion
  following, combat, and extraction all verified intact on the newest build carrying the most
  new simulation surface to date. This is exactly what I've been asking the org to prove since
  Hot Cargo, and this time I checked it myself rather than assuming it.
- UI truthfulness: net positive, first time. The specific stat I flagged as broken in Demo 02
  is fixed. A superficially similar frozen stat turned out to be a scenario-coverage gap, not
  a wiring bug, confirmed by source and by an independent Legacy-mode run — credit for the
  fix, a smaller ding for the evidence gap that made it look worse than it is.
- Hardware coverage: unchanged for a third straight build, now carrying the heaviest
  simulated load yet (four traffic cars plus everything from Demo 02). Disclosed, not fixed,
  same as always.
- Value: more systems again, same "still a slice" ceiling as the last two. Not what's moving
  the score this time — the regression story and the UI-truthfulness story are.

**8/10.** First build to earn more than a 7 from me, and it's for the two things I actually
grade release-readiness on: does the old stuff still work, and does the screen tell you the
truth. Both came back better than I expected going in. Fix the hardware gap before the next
one — that's three requests now — and put one screenshot in the evidence set where
MARA/STANDING isn't stuck on NEW FACE, so the next reviewer doesn't have to open
`FunstraUI.cs` to find out it isn't a bug.
