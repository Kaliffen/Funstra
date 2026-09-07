# A Bed & a Bandage (Demo 02) — reviewed by Priya Raman

**Verdict: 4/10.** A demo that knows exactly what feeling it's reaching for and hasn't
yet built anyone who can give it to you.

---

### What I actually looked at

I ran the project's own harness — `Tools\Test-Demo.ps1 -Mode Full -Visible` — start to
finish against `Build\BedAndBandage\Funstra.exe`. It drove a real controller through the
clinic, the collector, a paid release, an unwitnessed theft, a violent route, companion
orders, a defeat-and-recovery cycle, and reload persistence, and it came back clean: 107
rule assertions, 139 runtime assertions, all PASS, twelve fresh B01–B12 captures timestamped
to this run (22:26, same evening as the result log at 22:33). I'm not here to re-litigate
that — Dag already put his hand on every gear and confirmed they turn. I looked at the
twelve screenshots as stills, read every line of UI copy and journal text I could pull
from `FunstraDistrictUI.cs` and `DistrictState.cs` (the `Record(...)` calls are the closest
thing this build has to a script), and skimmed the player log, which is almost entirely
engine plumbing — no dialogue lives there, all the writing lives in the UI panels
themselves.

---

### The person-shaped hole where Neri should be

Here's the thing nobody can code around: I never saw a face. Not Neri's, not Ivo's, not
Rook's, not Mara's. Twelve screenshots, and every one of them is the same isometric tile
set with a colored circle standing in for a human being — pink for Neri, a name-tag
floating over a HP number. The title screen promises "someone to trust," and I believe
the team means it. But when I actually walked up to REPAIR, what I got was a panel
titled PEOPLE NEED THIS PLACE, and inside it, one paragraph of very functional dialogue:

> "I keep a spare bed for people the port won't keep. Ivo holds six doses behind Vico's.
> Pay him $100 or find another way. Bring them here and I'll offer more than thanks."

That's a mission briefing wearing a first-person voice. It's not nothing — "more than
thanks" is doing real work, it's reaching for a person who's tired of being thanked — but
it's one paragraph, and it never gets a second one that isn't a stat block. After I
recruited Neri, the screen retitled itself NOT ALONE ANYMORE, which is a good line, an
actually good line, sitting directly above a readout that says "100 HP / 3 dressings."
The game hands you the emotional beat and the inventory screen in the same breath and
expects you not to notice they're the same screen. I noticed. When I ran the recovery
scenario, Neri "dragged you out," and the panel is called STILL BREATHING with the line
"Neri dragged you out. You have someone to come home with" — that one landed on me a
little, I'll admit it, the way a single strong sentence in an otherwise dry document can.
But it's an event log entry, not a scene. Nobody knelt over me. Nobody's face changed.

So: gaining Neri is gaining a mechanic. G-follow, H-hold, R-retreat, T-aid, a dressing
count that ticks down, a companion that "cannot yet be directly piloted." I don't say
that as a technical complaint — Dag will tell you those verbs are honest and consistent,
and I believe him. I say it because the demo's own marketing language ("someone to
trust," "keep each other alive") is writing a check that the actual content of the
partnership screen doesn't cash yet. There is real intent here — the framing knows what
it wants to be — but intent isn't the same as having written it.

### Three doors, one warehouse

Pay, steal, or fight — Ivo's case sits behind all three, unmoved by which one you pick.
I went looking for the version of this choice that costs you something in how the world
sees you afterward, and mechanically it's there: theft that's witnessed gets you
identified, "Rook remembers your face after the chase ends," and that memory survives a
reload — a genuinely good piece of design, Dag is right to like it. But narratively, all
three routes converge on the same terminal screen: IVO / HARBOR COMBINE, "Everything is
accounted for," a line that reads as a shrug regardless of what you did to get there. Pay
him and the line is procedural. Steal from him unseen and the line doesn't change at all
— he never finds out, so there's no moral texture, just an absence. Fight him and the
line becomes about restitution money, not about what it means that you shot someone over
medicine. The verb changes; Ivo's opinion of you, as written, does not. I don't need the
game to moralize at me. I need it to notice, in prose, that I chose violence over three
attempts to avoid it, and right now the only place that's noticed is a red UI line that
says IVO KNOWS YOUR FACE / $60 RESTITUTION. That's bookkeeping wearing the costume of a
reckoning.

### Is anyone in this a person

Ivo and Rook are functionally the same character split across two nameplates — a debt
collector's name-brand ("Harbor Combine") and a guard who "searches a last-seen location"
instead of omnisciently punishing you, which is more mercy than most guards get, and I
noticed and appreciated it. But neither of them says a sentence that couldn't be spoken by
a customs form. Mara, going by the copy I found, exists entirely as a transaction: "Mara
paid $160. The clinic received none." Neri is the only one who gets anything like an
interior life, and it arrives in fragments — "I keep a spare bed for people the port
won't keep" is a real character detail, the kind of line that implies a whole history of
who's slept in that bed before you. I wanted three more sentences like it. I got one.

The world itself does more of the emotional work than the people do. The title screen's
tagline — "One person. A city of debts. Someone to trust." — sits over a genuinely
handsome dusk shot of Old Port, warm windows, a red car idling under a streetlamp, and for
about four seconds I believed the pitch. Then I walked the district and it was the same
tile set, competently doing its job, buildings labeled ARCADIA and HOTEL LUNA like a
board game, no dusk-specific mood carried into play. The art direction knows how to open
a film. It hasn't yet learned to keep shooting once the characters start talking.

### Where the systems and the story pull against each other

Dag's favorite fact about this build — the identified-grievance-survives-reload rule — is
also the moment I felt the split most clearly. As a system, it's honest: the world
remembers you, save-scumming doesn't erase your sins, that's real integrity. As a story,
it's inert, because the "memory" is a flag and a restitution price, not a changed
sentence anywhere in the game. Good systems design and good storytelling aren't fighting
here so much as one of them showed up to work and the other one is still in the outline
stage. The bones for both exist in the same file — `Record("identified","rook",...)` sits
two lines from `Record("donation","neri",...)` in the source, they're literally built on
the same mechanism — which tells me the team already has the plumbing to let consequences
talk. They just haven't asked it to say anything with texture yet.

### The honest math on the score

I don't hand out low scores for a small demo being small — punishing scope would be a
different kind of bad faith than the kind I actually watch for. This isn't cynical
writing. Nobody here is being manipulative or hollow on purpose; if anything the intent
is unusually sincere for something this early — "someone to trust" is the right ambition.
But I judge on what actually lands, and right now what lands is one good line at the
title screen, one good line at the recovery screen, and a companion system that calls
itself a partnership while reading, on every screen I could find, as an inventory slot
with a heartbeat. That's a 4. Not a condemnation — a note that says: the person is not in
the build yet, but I can see exactly where the team is leaving room for her to arrive.
