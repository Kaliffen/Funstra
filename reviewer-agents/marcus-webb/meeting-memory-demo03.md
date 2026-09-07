# Meeting memory — Marcus Webb (Demo 03 / Keep the Lights On)

**Date:** 2026-09-07

Same drill as the last two rounds, and I'm not going to pretend it's novel anymore: Dag
played Demo 03 first and scored it 8/10 off a code-and-screenshot read. My job isn't to
countersign that number, it's to go get my own hands on the same build and see if it holds
up from a different angle — technical state, regression risk, hardware coverage, and
whether the new stuff actually survives contact instead of just existing for a screenshot.

Went in with four specific things to check, same shape as always:

1. Does the refuge/clinic-fund economy survive on top of the twelve-dose conservation law
   I've now watched hold across two prior builds, or does adding a second spend path crack
   it.
2. Does the traffic system have a hidden escape hatch, or does it actually livelock/behave
   exactly as conservatively as the design doc admits.
3. Regression: does everything that used to work — companion following, combat, extraction,
   and specifically the *original* three-job Hot Cargo campaign — still work with two more
   demos' worth of simulation stacked on top of it. Nobody else's lens is built to ask this
   question the way mine is.
4. The "REPUTATION: NEW FACE"-shaped bug again. I found a dead HUD stat in Demo 02 that
   didn't move when a live state change said it should. I fully expected to either find the
   same bug again, find it fixed, or find a new one just like it. What actually happened was
   none of those three — see the review for the honest answer, because it surprised me and
   I don't want to undersell that in a two-line summary.

Ran `Tools\Test-Demo.ps1 -Mode Full -Visible` and `-Mode Visual -Visible` myself, fresh, this
session, against `Build\KeepTheLightsOn\Funstra.exe` (0.3.0). Then, because "does the old
stuff still work" isn't something the Full/Visual harness alone proves — it's scoped to the
new medical/refuge/traffic content — I also ran `Tools\Test-Demo.ps1 -Mode Legacy -Visible`,
which replays the original Hot Cargo three-job campaign in isolation on this same 0.3.0
build. That's the regression check that actually matters to me, and nobody asked me to run
it; I ran it because it's the only way to answer question 3 with evidence instead of
inference.

Full writeup in `playtest-review-keep-the-lights-on.md`. Short version: I almost flagged a
HUD bug that wasn't one, caught myself by reading the source instead of stopping at the
screenshot, and the actual regression check came back clean. Score's an 8 — first time I've
moved off 7, and it's for real reasons, not fatigue with grading these things down.
