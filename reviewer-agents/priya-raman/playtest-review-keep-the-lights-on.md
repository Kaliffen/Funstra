# Keep the Lights On (Demo 03) — reviewed by Priya Raman

**Verdict: 8/10.** I came in ready to write the same review I wrote last time with new
nouns. I can't. The person I said wasn't in the build yet has arrived, and I can quote
the sentence where I noticed.

---

### What I actually looked at

I ran the harness myself — `Tools\Test-Demo.ps1 -Mode Full -Visible` then `-Mode Visual
-Visible` against `Build\KeepTheLightsOn\Funstra.exe` — and both came back clean, same as
Dag and Marcus report. I'm not re-litigating that; the machine works, I believe them and
I watched it pass myself. What I actually spent my time on was the twenty-four B/C
screenshots, read as stills, plus the full text of `Assets\Scripts\RefugeState.cs` — the
file that turned out to hold every line of Neri's and Ivo's dialogue as a stacked
if/return chain, `NeriWords` and `IvoWords` — and `FunstraDistrictUI.cs` for how those
strings actually surface on screen. That's the same method I used on Demo 02, where I
went into `DistrictState.cs` and found the `Record(...)` calls sitting next to each other.
This time I went into the equivalent file expecting to find the same kind of gap. I found
the opposite.

### The line I was owed, and got

Demo 02 gave me one sentence of Neri's interiority — "I keep a spare bed for people the
port won't keep" — and then nothing else that wasn't a stat block. I said I wanted three
more sentences like it. This build gives me the actual history behind that bed, and it's
specific in a way that costs the writer something to write:

> "She repaired pumps. Never trained as a doctor. When the water came, people brought her
> everything broken. Engines. Hands. Children." Neri folds the same clean towel twice. "I
> inherited the room and the debt. I kept the door open. I don't always know which part of
> that was a choice."

That's `THE BED WAS MY MOTHER'S`, reachable by asking Neri about the bed directly
(`Evidence\C03-neri-story.png`). "Engines. Hands. Children." is a list that gets more
human as it goes, on purpose, and "I don't always know which part of that was a choice"
is a sentence a real person says about inherited obligation, not a sentence a quest giver
says about a fetch item. This is exactly the gap I flagged. It's closed.

### The three doors actually stopped converging

My hardest complaint last time was structural: pay, steal, or fight Ivo, and all three
routes ended on the same line, "Everything is accounted for." I went looking for whether
that was still true, and it isn't — I found five distinct titled dialogue states for Ivo
in `RefugeState.cs`, keyed off exactly the variables the design doc names (paid release,
unidentified missing stock, identified offense, an incapacitated guard, settlement):

- Untouched: **THE DEBT HAS A BUYER** — "My father kept this ledger through the flood.
  People thanked him for writing down what they owed. Nobody told their children the ink
  wouldn't wash out."
- Unwitnessed theft: **SIX EMPTY SPACES** — "Six spaces on the shelf. Someone will be
  asked to cover that loss. If you hear a name, bring me a name. I don't have yours in
  that column." (This is the exact "narrative absence" I complained about last time —
  and now it's not an absence, it's a line about not having your name, which is a
  different and better thing than silence.)
- Paid release: **YOUR NAME IS ON THE RECEIPT** — "Keep the receipt dry. Rook reads the
  stamp before he reads faces."
- Identified after violence, Rook down: **I REMEMBER YOU** — "Rook has a daughter who
  waits up for him. Spare me the speech about the clinic. Sixty settles my claim. It
  doesn't stand him back up." (`Evidence\C09-ivo-remembers.png`, confirmed against
  `RefugeState.cs` line 63 — `if(identified&&guard.health<=0)`.)
- Settled after violence, without killing the claim's meaning: **SETTLED ISN'T
  FORGOTTEN** — "The account is settled. That's what the money buys: an end to
  collection. It doesn't buy the conversation we had before."

That last pair is the thing I actually asked for by name in my Demo 02 review: "I need it
to notice, in prose, that I chose violence over three attempts to avoid it." "It doesn't
stand him back up" and "It doesn't buy the conversation we had before" are both, in two
different ways, the game refusing to let money fully absolve the verb you chose. This
isn't a shrug wearing three costumes anymore. It's five actual sentences, each keyed to a
distinct thing you did, each written by someone who understood what a debt collector who
sounds like a person, and not a customs form, would actually say about a guard's kid.

Neri's side does the same work with a longer chain — betrayal, refuge-with-reserve-and-
refusal, refuge-with-empty-shelf, refuge in general, rescue-after-defeat, sold-to-market,
guard-still-wounded, paid-release, general-illicit-trust, mid-carry, and the cold open —
eleven distinct branches in one property, each a full sentence with its own image, not a
template with a noun swapped in. Compare the betrayal line — "I can still put a needle
through a vein. I can't put trust back where it was. Stand on the other side of the
counter." — against the rescue line — "You were heavier than you look. I kept talking on
the way home so I'd know if you stopped answering. Next time, answer sooner." Those are
not the same sentence shape wearing different data. They're two different registers of
hurt and relief, and I believe both of them came from the same person.

### The care-policy choice has actual weight, not just a counter

The design doc asks whether reserving medicine for the crew "creates a choice you care
about, or merely a new toggle." I went to check whether that was honest framing or a
leading question. It's honest, and here's the evidence: reserving the last two doses
doesn't just move a number (`Evidence\C07-policy-consequence.png` shows "5 turned away by
policy" sitting right there, which is itself an improvement — Demo 02 buried consequence
counters where the player couldn't read them). It also produces this, once a real person
gets turned away:

> "Edda came after her shift. I told her the shelf was empty, with two doses behind my
> back. If this is our place, that lie belongs to both of us."

Edda is the same named person from the very first Neri line in the game — "Edda cleans
the ferry engines. Her hand won't close" — which means the reserve policy isn't an
abstract stranger getting turned away, it's a callback to a specific person the game
already made me care about in the first thirty seconds, coming back to be denied
treatment by the choice I made. That's the policy toggle actually costing something in
prose, not just in a number on a panel. I'll take Dag's word that the threshold triggers
at exactly stock==2; what I care about is that the moment it triggers, Neri lies to a
named woman about it and tells you the lie belongs to both of you. That's a choice with
weight. It wasn't guaranteed to be — plenty of games would have left this as "3 turned
away by policy" and nothing else — and this one didn't.

### What's still thin

I won't pretend everything is solved. The exterior "refuge" is still, honestly, a porch
light — I compared `Evidence\C01-clinic-and-tally.png` against `Evidence\C08-refuge-
street.png` and the visible difference is a single new light fixture near Vico's. The doc
is upfront that this is "an exterior representation of the room, not an enterable
interior," and I respect the honesty more than I'd respect an oversold screenshot, but I
still felt the same thing I felt at the title screen last time: the game is better at
telling me a room now has two names on the door than at showing me the room. All the
actual feeling lives in text panels, same as Demo 02 — nobody's face still ever changes,
there's still no scene where Neri and I are standing in the same physical space having
the conversation instead of it happening inside a UI card over a paused world. Tally gets
a pet prompt and "a short reaching/tail response" per the doc, which is a nice touch for
a side character, but I didn't go looking for Tally's inner life and wouldn't expect one.

I also want to flag, in fairness to my own axis: I'm scoring the writing, and the writing
earned this. I am consciously not folding in Dag's and Marcus's systems findings as if
they were mine — the reserve threshold being exactly where the doc claims, or the traffic
model being honestly disclosed, are both real and both irrelevant to whether Neri sounds
like a person. She does now. That's the whole review.

### The honest math on the score

Last time: 4/10, because the demo's marketing ("someone to trust") was writing a check
the actual content of the partnership screen couldn't cash. This time the check clears.
Five distinct, specific Ivo states replacing one convergent line; eleven distinct Neri
states instead of one good paragraph; a care-policy choice that produces a named
person's betrayal instead of a number. I don't hand out an easy 9 or 10 because the
delivery mechanism for all of this is still a paused text panel over an isometric tile
set, and because "a room with two names on the door" is still mostly told to me rather
than shown. But I asked, after Demo 02, whether the team had the plumbing to let
consequences talk. They did. This build is the proof they also had something worth
saying with it. 8/10 — the same number Dag and Marcus landed on, for a completely
different reason than either of theirs, and for once I'm glad to agree with the systems
guys without having to borrow their argument.
