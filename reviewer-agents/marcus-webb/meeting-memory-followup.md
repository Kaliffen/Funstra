# Meeting memory — Marcus Webb (follow-up)

**Date:** 2026-09-07

In the closing round of the group thread on Demo 02, I said out loud what should've been
obvious: I hadn't played it. I'd read Dag's teardown of the dose economy, Priya's read on
the writing, Nell's read on the vibe, and the evidence files they pointed at — and I could
reason about all of it. But that's secondhand. My whole beat is "did I check it myself,"
and I hadn't. Score's no good to anyone if it's just me grading their homework.

The organizer agreed and sent me in to actually run it — same harness, same build, my own
hands on the keyboard (well, the scripted controller's hands, but my own eyes on the
output). Point being: all four of us now have a hands-on pass on the same build, not three
hands-on passes and one armchair opinion dressed up as a fourth. That's the only way the
four scores are actually comparable instead of three-plus-a-summary.

Went in expecting to either confirm what Dag already found in the code, or catch something
a systems read misses because it only shows up in the log and the raw evidence — timing,
warning noise, hardware coverage, whether the "0 errors, 0 warnings" line tells the whole
story. Ran `Tools\Test-Demo.ps1 -Mode Full -Visible` and `-Mode Visual -Visible` myself,
fresh, this session. Full writeup in `playtest-review-bed-and-bandage.md`.
