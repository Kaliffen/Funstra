# Hot Cargo — Marcus Webb playtest review

**Verdict: 7/10.** Clean technical state, an actual working loop, and I couldn't break it in
the time I had. Score's held down by scope, not stability — this is still forty minutes of
game with a lot of infrastructure under it.

## What I actually ran

- Working directory: `~\repos\Funstra`
- Harness: `powershell -ExecutionPolicy Bypass -File Tools\Test-Player.ps1 -Visible`, launched
  against the handed-off `Build\Funstra.exe` — a real visible player window driving a real
  character controller, not a headless mock.
- The run finished clean: **88/88 pass, 0 fail**, output captured verbatim in
  `Evidence\runtime-result.txt`.
- Engine/session log at `Evidence\player.log` — no exceptions, no errors, just clean
  Unity/D3D11/PhysX startup and shutdown on an RTX 4090 rig. That's a strong GPU, worth
  flagging: this is not evidence for a mid-range card, and the dev team's own
  `Evidence\Hot-Cargo-VALIDATION.md` says as much ("Human difficulty balancing... and other
  hardware remain unverified").
- Also pulled `Evidence\build-result.txt` (0 errors, 0 warnings, 2.77s build) and eyeballed
  eleven of the twelve fresh screenshots (`Evidence\01-title.png` through
  `Evidence\12-extraction.png`).
- Cross-checked all of it against `Evidence\Hot-Cargo-VALIDATION.md`, the dev team's own
  claims sheet for this build, not against the pitch in `HOT-CARGO.md`.

## The number that matters: 88 for 88

I went in expecting to find the seam between "assertion passed" and "feels right in a
human's hands." I did not find a crash, and I did not find a lie in the validation doc.
Everything it claimed, the log backs up:

- Three-job campaign completes and lands you at exactly **$740 cash, 0 arrests** —
  confirmed on screen in `07-ending.png`, matching the assertion `Three jobs reach ending
  with $740`.
- The heavy-load penalty is real, not decorative. Screenshot `06-alarm.png` shows the bag
  at **5/6 load** tagged `HEAVY / MOVE SLOWER` in the HUD, and the log has a distinct pass
  line for it: `Loaded cargo reduces movement speed`. That's the 20%-at-5-load claim from
  the design doc actually gated behind its own test, not just asserted in prose.
- The extraction-interrupt path I specifically wanted hunted has two separate assertions
  covering it: `Extraction requires a sustained hold` and `Releasing interaction resets
  extraction without banking`. So letting go of E doesn't silently half-bank your cargo or
  desync the counter — it just resets, which is the boring, correct behavior. I'd have
  respected the game less if this one had been vague.
- Arrest-mid-run: `Arrest fines cash, removes goods and preserves retryable job` plus
  `Actual arrest confiscates loose cargo` and `Arrest returns player to safehouse`. That's
  the cost-of-doing-business model I wanted to see — you lose the unbanked bag and pay a
  fine, but the job itself isn't torched. Fair, not punishing.
- Police AI isn't a rubber stamp either: `Officer sees and pursues wanted player`,
  `Building interrupts officer line of sight`, `Unseen player does not update search
  location`, `Unseen heat expires`. That's real occlusion and search-state logic getting
  exercised, not a binary "seen/not seen" flag.
- The $180 satchel: `Satchel purchase costs $180 and expands bag to nine` and `Purchased
  satchel cannot charge player again`. Screenshot `11-safehouse.png` shows it live —
  `COURIER SATCHEL / $180`, "Raise bag capacity from 6 to 9. Carry more; risk more." It
  does gate a real decision: with the base 6-slot bag you cannot combine bonded cargo
  ($220, 5 load) with anything else, so the satchel is the only way the highest-value
  pickup stops being a standalone run. That's a purchase with a mechanical consequence, not
  a cosmetic unlock — worth the $180 if you're going to keep playing.

## What I'd actually flag

- **Screenshot mislabeling.** `Evidence\11-safehouse.png` is the street-side "HOLD E / BANK
  $220" extraction prompt, and `Evidence\12-extraction.png` is actually the safehouse
  upgrade menu — the two are swapped relative to their filenames. Doesn't affect the game,
  but it's the kind of sloppy bookkeeping that makes me double-check everything else in the
  evidence folder by hand instead of trusting the labels. I did double-check. The content
  itself is fine.
- **Build.log has noise the "0 errors, 0 warnings" line doesn't mention**: four
  `abort_threads: Failed aborting id: ...` lines during the editor build/teardown phase.
  These are Mono thread-abort warnings during Unity's own build tooling, not the shipped
  player, and they didn't stop the build or show up in player.log — so I'm not marking the
  score down for it. I am noting it, because "0 warnings" in a headline number and "clean"
  in a raw log are two different claims, and only one of them is fully true here.
- **Hardware honesty.** This all ran on an RTX 4090. Zero information here about a 3060 or
  integrated graphics. The dev's own validation doc says so upfront, which I'll credit
  them for — but until someone runs this on a mid-range box, "runs clean" only means
  "runs clean on the best case."
- **This is the previous build, not the newest one.** There's already a Demo 02 ("A Bed &
  a Bandage") sitting in this repo per `Evidence\demo-02-build-info.txt`, which is a
  superset with a new companion/medical system layered on top. Hot Cargo is what I was
  handed and what I tested; if the org wants a score for the current build, that's a
  separate pass.
- **Content per dollar is still the open question, and it's not a testing question.**
  Three authored jobs plus a loop that's provably solid mechanically. 88 assertions tell
  you the machine works. They don't tell you if running the same three cargo tiers for the
  fourth time in an evening stays interesting — that's the "human feedback on pursuit
  difficulty and repeated routes remains the acceptance gate" line the validation doc
  itself flags as still open. I agree with that. It's the actual gap between "assertion
  passed" and "feels fine in a human's hands," and no amount of scripted playtesting closes
  it — you need a person mashing keys for forty minutes, bored or not.

## Score breakdown

- Technical state: clean. No crash, no exception, build succeeds in under 3 seconds, 88/88
  scripted assertions pass on the actual shipped exe. This is what "not broken" looks like
  when you check instead of assume.
- Systems honesty: the interrupt, load-penalty, arrest, and satchel mechanics I asked about
  going in are all individually tested, not just described. That's rare and it's worth
  crediting.
- Value: three story beats and one repeatable loop. Fine for a demo slice, not enough on
  its own to push past a 7 — and I have zero data on whether the loop holds up on the
  fifth or tenth repeat, because that's not something a scripted test can tell you.
- Hardware coverage: unverified below a 4090. Docked a point in spirit, not in the number,
  because the dev doc admits it rather than hiding it.

**7/10.** Ship it as a demo slice with your eyes open about what's untested — a mid-range
card and a bored human doing the same run twice. Nothing here caps me at 4. Nothing here
earns a 9 either.
