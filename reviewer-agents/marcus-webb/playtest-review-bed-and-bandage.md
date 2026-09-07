# A Bed & a Bandage (Demo 02) — Marcus Webb playtest review

**Verdict: 7/10.** Clean technical state again, bigger surface area, and one HUD problem I
caught with my own eyes that nobody else's lens was built to flag. Same number as Hot
Cargo, for different reasons — read to the bottom before you assume that's a coincidence.

## What I actually ran

- Working directory: `~\repos\Funstra`.
- Harness: `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Full -Visible`
  against `Build\BedAndBandage\Funstra.exe` (version 0.2.0, per
  `Evidence\demo-02-build-info.txt`) — a real visible window, real character controller,
  not a batch-mode mock. Came back clean: **51/51 pass, 0 fail**, fresh output in
  `Evidence\bandage-runtime-result.txt` and `Evidence\bandage-full-player.log`, both
  timestamped from this run, not carried over from anyone else's pass.
- Then `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Visual -Visible`
  to regenerate the twelve B01–B12 capture screens myself, rather than eyeballing whoever's
  screenshots happened to already be sitting in `Evidence\`. That mattered: the Full pass
  does not touch the screenshots, only the Visual pass does — worth knowing if you're
  trying to reproduce this yourself and wondering why your captures didn't refresh.
  `Evidence\bandage-visual-result.txt`: **PASS, twelve new demo screens captured without
  black rendering.**
- Read `Evidence\VALIDATION.md` in full, pulled `Evidence\build-result.txt` (0 errors, 0
  warnings, 2.77s build) and `Evidence\build.log`, and personally viewed five of the twelve
  screenshots at full resolution: B03 (Neri's dialogue), B05 (live combat), B08 (history
  log), B10 (defeat/recovery), B12 (cargo extraction).
- Both engine logs (`bandage-full-player.log`, `bandage-visual-player.log`) grepped clean
  for error/exception/fail — nothing.
- I'm not re-deriving Dag's code-level audit of the dose ledger; he already opened
  `DistrictRules.cs` and I have no reason to redo that homework. What follows is what my
  own pass adds on top of it.

## The number that matters: 51 for 51, plus 107 rule assertions I didn't have to take on faith

Same shape as Hot Cargo's 88-for-88, and it holds up the same way when you actually read
the log instead of the headline. Both harness passes finished well inside the runner's own
420-second timeout, no hang, no retry, no manual babysitting. `bandage-full-player.log` and
`bandage-visual-player.log` are both silent on errors — clean Unity 6000.4.0f1 / D3D11 /
PhysX startup and shutdown, same as before.

One thing I'll credit up front because it's exactly the kind of claim I go looking to
falsify: `VALIDATION.md` says the earlier Hot Cargo hidden-window test used to time out, and
that this runner switched to an actual headless player (`-batchmode -nographics`) to fix it.
I can't verify the "used to time out" half historically, but I can verify the fix holds now
— my Full pass, run cold, first try, came back green. That's a real regression fix, stated
plainly instead of buried, and it's the kind of honesty in a validation doc I said last time
I'd credit when I see it.

## Same abort_threads noise, same footnote

`Evidence\build.log` has the identical pattern I flagged on Hot Cargo: four
`abort_threads: Failed aborting id: ...` lines during the editor build/teardown, not the
shipped player. Didn't stop the build, doesn't show up in either player log, doesn't cost
points — I'm noting it for the same reason as last time: "0 errors, 0 warnings" is a true
statement about the compiler, and "completely quiet log" is a slightly different claim, and
only one of them is fully true here. Consistent behavior release over release is at least
consistent — this isn't a new problem, it's the same old one, still cosmetic.

## Hardware honesty — worse than last time, not better

Both my passes ran on the same RTX 4090 as Hot Cargo. `VALIDATION.md` says it outright: "A
30 FPS cap is a timing check on this workstation, not evidence from a mid-range computer. No
low-end hardware... has been performed." Fine, they said it, I'll credit the honesty again.
But notice what didn't happen between builds: zero new hardware coverage. Same blind spot,
carried forward untouched, on a build that's now added a second AI-driven companion, a
guard with return-fire logic, and line-of-sight occlusion checks — all of which cost more
GPU and CPU than the original cargo loop did. If Hot Cargo's 4090-only testing was a known
gap, Demo 02 is the same gap with more expensive systems running through it. I'm not
docking further for it because they're still not hiding it, but I want it on the record
that "we told you already" isn't the same as "we fixed it," and this is the second build in
a row where it wasn't fixed.

## The thing I caught that the others' lenses weren't built to catch

Dag flagged, from the code and the screenshots, that cash and reputation read static across
all twelve B-series captures — "$0" and "REPUTATION: NEW FACE" — and said he couldn't tell
from a twelve-image pass whether that's a real gap or an artifact of how the scenario staged
its screenshots. I went and looked at the same five screens myself with that specific
question in mind, because this is exactly the seam I exist to check: not "does the system
work" (Dag settled that) but "does the number on screen match what the system just did."

Here's what I can add: B05 shows the player mid-gunfight with Rook, HUD reading in red
**"IVO KNOWS YOUR FACE / $60 restitution"** — a live, correct, specific readout of the
grievance state changing in real time. Two panels up, in the same screenshot, **REPUTATION**
still reads **"NEW FACE."** The grievance system is updating. The reputation stat sitting
one inch above it on the same HUD is not, even though the player has, in-fiction, just been
identified attacking a collector's guard. That's not a crash and it's not a missing feature
— it's a readout that isn't wired to an event that plainly should move it. B12's cash
staying at $0 I'll actually give a partial pass on, same as Dag's instinct: the same
screenshot shows "Cargo $170 / 5 load: hold E at HOME to bank," meaning the money is
legitimately unbanked at capture time, not vanished. That one's probably not a bug. The
reputation stat sitting frozen next to a live grievance flag is a different animal, and I'm
calling it what it looks like: a stat that isn't hooked up yet, dressed as a stat that is.
Doesn't cap the score — nothing here is broken in a way that stops you playing — but it's
exactly the gap between "the log says PASS" and "the screen tells you the truth" that a
scripted assertion suite will never catch, because the assertion never checked whether
REPUTATION moves. Somebody should write that check.

I'd also flag, more quietly, what `VALIDATION.md` says about its own combat tests: "New
guard/companion AI is isolated where necessary; companion follow/aid and the final local
gunfight run with their AI active." Translation: most of the 51-assertion pass exercises
this content with the AI on rails, and only the capstone gunfight lets the real guard AI
loose. That's a reasonable engineering call for repeatable tests, and it's disclosed rather
than hidden, which I'll credit — but it means the sight-line and ammo rules Dag verified
against "hostile" AI were mostly verified against AI that was told to be hostile on a
schedule, not AI making its own calls for the whole pass. One live gunfight assertion
passing is real evidence. It's one data point, not twenty.

## Content per dollar, now that there are two of these

Hot Cargo was forty minutes and three jobs. Demo 02 layers a finite medical economy, a
companion with its own resource budget, and a combat/occlusion system on top of the same
cargo loop, which — per the design doc — is still fully playable standalone if you never
touch the new content. That's two demos' worth of systems now sitting in one repo, and
neither one is a purchasable product yet, so "value for money" is premature as a literal
question. What isn't premature: content-per-hour-of-my-time. I spent roughly the same
harness runtime checking a build with meaningfully more mechanical surface than last time —
finite stock conservation, a witnessed/unwitnessed branch, a companion with its own
casualty state — for the same "still a slice, not a campaign" verdict at the end. The
infrastructure-to-content ratio hasn't improved, it's just gotten more infrastructure. If
you're the kind of buyer I write for, that's not a reason to avoid it, it's a reason to
expect a third demo before there's a full evening here.

## Reconciling with Hot Cargo — is this more or less shippable

Straight answer: technically about even, and that's actually a mild disappointment, because
I expected a second pass to close gaps rather than carry them forward at the same size.
Hot Cargo's flaw was a mislabeled pair of screenshots — cosmetic, evidence-folder
bookkeeping, zero in-game impact. Demo 02's equivalent flaw is a live HUD stat that doesn't
track a state change the game itself just told you happened — smaller in scope, but it's a
step closer to the actual player-facing surface than a filename swap was. Neither one caps
the score; both are the kind of thing that makes me re-check everything else in the folder
by hand instead of trusting the headline pass rate, which I did, again, this time.

Where Demo 02 is unambiguously ahead: the timeout bug is fixed, the assertion count roughly
doubled in scope of systems touched (companion AI, occlusion, finite-resource conservation)
without a single regression in the parts Hot Cargo already proved, and the validation doc is
more forthcoming about its own test limits (the AI-isolation caveat) than the Hot Cargo doc
was. Where it's flat or worse: identical hardware blind spot on a build with heavier AI and
physics load, and a new-but-minor UI truthfulness gap that the last build didn't have
because the last build had less UI surface to get wrong.

Should you spend time on this today over Hot Cargo? Yes, if you're going to spend time on
either — it's a strict superset in systems and it hasn't broken anything getting there. But
"more shippable" implies motion toward a finished product, and on the two things I actually
measure release-readiness by — hardware coverage and HUD-to-state fidelity — this build
moved sideways, not forward. Same score, different reasons: last time a 7 because a solid
machine had thin content around it; this time a 7 because a bigger machine has one new,
small, honest-to-catch crack in its dashboard.

## Score breakdown

- Technical state: clean. No crash, no exception across two full harness passes, build
  succeeds in under 3 seconds, 51/51 runtime and 107/107 rule assertions pass on the actual
  shipped exe, confirmed by me, this session, not carried over from someone else's evidence.
- New-systems honesty: companion AI, occlusion, finite-resource conservation, and the
  witnessed/unwitnessed branch are all individually tested against a real running exe, same
  discipline as Hot Cargo's interrupt/load/arrest/satchel coverage last time. Credit held.
- UI truthfulness: docked, mildly. REPUTATION not moving next to a HUD line that just moved
  for the same event is a real, reproducible gap I found myself, not inherited from anyone
  else's writeup — worth fixing before anyone scores this build on "does the game tell you
  what's happening," because right now it doesn't, consistently, on one stat.
- Hardware coverage: unchanged and now carrying more mechanical weight per frame than last
  time. Not newly penalized, but not improved either, and I said last time I'd watch for
  movement here. There wasn't any.
- Value: more systems than Hot Cargo, same "still a slice" ceiling. Fine for what it is.

**7/10.** Ship it as the next demo slice, same as last time — the machine runs, the new
systems are honestly tested, and the thing that would actually worry me if I were funding
this isn't a crash, it's a HUD line that stopped listening. Fix that, put a real GPU-tier
spread on the test matrix before the next one, and I'll come back and check again, same as
always.
