# Meeting memory — Marcus Webb

**Date:** 2026-09-07

## The table-read

The organizer pulled all four of us into one thread to prep for the Hot Cargo release: me,
Dag Møller (systems), Priya Raman (feelings-first), Nell Okafor (cosy hours). Everyone
introduced themselves and said what they'd be looking for before anyone touched the build.

What I said going in: I expected it to run clean, because the validation doc was already
sitting there claiming zero build errors and no runtime exceptions — that's a low bar and
most devs clear it. What I actually wanted to hunt was the gap between "assertion passed"
and "feels fine in a human's hands." Four specific things I flagged before playing:

1. The extraction-interrupt path — what actually happens the instant you let go of E or
   get spotted mid-hold.
2. The 20%-slower-at-5-load penalty — whether it's a real tax on the loop or a rounding
   error nobody feels.
3. Getting arrested mid-run — whether it reads as a fair cost of doing business or a
   progress-eraser.
4. The $180 satchel — whether it actually changes how you play, or it's a number on a
   screen that never gets tested by a real decision.

## What was different about today

This was the first time one of the four personas got run through an actual playtest
pipeline instead of just talking about the game from the sell sheet. Trial run, and I drew
it. I ran `Tools\Test-Player.ps1 -Visible` against the handed-off build myself, read the
raw runtime-result.txt and player.log, and looked at the screenshots — not vibes, not the
marketing copy in HOT-CARGO.md. The other three haven't been put through this yet; if this
pipeline holds up, that's presumably next.

I'll revisit this after a patch, same as always, and say so in print if the numbers move.
