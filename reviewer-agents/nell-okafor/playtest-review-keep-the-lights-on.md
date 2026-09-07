# Keep the Lights On (Demo 03) — Nell's playtest review

**Verdict: 8/10.** Up from 7. I asked for one thing after the last demo, and they
actually built it — not a placeholder, not a doc-only promise, a real key you press, a
real animation, a real line of dialogue that survives a save and reload. I have real
evidence for that this time, not "I believe the paperwork," which is what I had to
settle for last round. That alone would have earned the extra point. What pushed me to
write a genuinely happy review instead of just a satisfied one is that the *home* got
warmer too, in ways nobody specifically promised me.

## What I actually did

I ran both harness passes myself from a clean state — `Tools\Test-Demo.ps1 -Mode Full
-Visible` and then `-Mode Visual -Visible` — against `Build\KeepTheLightsOn\Funstra.exe`.
Both came back clean: the Full pass ended in a list of PASS lines with nothing red,
including one that made me sit up, `PASS: Cat interaction records familiarity`, and the
Visual pass confirmed its twelve new screens rendered without going black. I'm not going
to pretend I re-derived Dag's dose-conservation math or reread `CityTraffic.cs` line by
line the way Marcus and Dag did — that's their job and I trust their homework the same
way I did last time. What I did was look at the actual screenshots this run produced
(C01 through C12, plus B01 through B12, all fresh from tonight, not borrowed from
anyone else's pass) and go into the source myself for the one system I actually care
about, because "I believe the paperwork" was a real thing I had to write last time and I
did not want to write it again.

## The cat. Let's just do the cat first, I've been waiting a demo and a half.

He's pettable. Genuinely, actually pettable, not "pettable per the design doc." Here's
the receipt trail, because I did the work this time instead of taking it on faith:

- The HUD itself tells you, in play, standing near the clinic: **"P / PET TALLY    F /
  CLINIC REFUGE"** — I can see this text sitting right there in both `C01-clinic-and-
  tally.png` and `C08-refuge-street.png`, and there's a small labeled sprite reading
  "TALLY" on the ground next to the clinic in both shots. He is not a rumor. He is a
  small drawn shape with his name over his head, right there in the screenshot, which is
  a real and very small joy that I did not have last time.
- The source (`Assets\Scripts\FunstraRefuge.cs`) backs it up exactly: pressing P within
  three units of Tally calls `PetTally()`, which plays a little reaching-arm-and-tail
  animation (`petTime=1.5f`, an actual arm rotation and tail wag over that window, not
  a screen flash) and fires this line, once, the first time: *"Tally put his head into
  your hand. Neri: He only does that to people who sit still long enough."* Every pet
  after that gives you: *"Tally leans into your hand, purring. For a moment, nobody
  needs anything."*
- That first line goes into the actual journal and persists — the harness proves it
  survives a save and reload (`Check(District.tallyPetted,"Cat interaction records
  familiarity")`, and separately, `PASS: Exported player reload retains room, care
  policy and cat relationship` in the same run). This is exactly the "persistent first
  encounter in the journal" the guide promised, and it's not a promise anymore, it's a
  boolean that survived a real reload in a real test I ran myself tonight.

Is it a whole pet system? No, and I want to be honest about that rather than just
grateful. You press P, you get one small animation and one of two lines depending on
whether it's your first time, and that's the interaction — there's no feeding him, no
sleeping-on-the-bed, no him following you around later mentioning you by scent or
whatever elaborate thing my Stardew brain wants from a video game cat. But "he only does
that to people who sit still long enough" and "for a moment, nobody needs anything" are
two sentences that understand exactly what a cat is *for* in a game like this — not a
system, a permission slip to stop being useful for five seconds — and that is the thing
I actually asked for, delivered honestly, with the receipts to prove it rather than a
sentence in a design doc. Top note from last time: closed. For real this time.

## The refuge, from the outside

The doc is upfront that this is "an exterior representation of the room, not an
enterable interior," and I appreciate that they said so before I could feel let down by
it myself. I put `C01-clinic-and-tally.png` (before) next to `C08-refuge-street.png`
(after) side by side, and the honest read is: it's subtle. There's a new lit box near
the alley by Vico's in the "after" shot that isn't in the "before" one — a little porch
light, basically — and the HUD line changes from "Help the clinic. Earn a partnership"
to "Your shared refuge is open. F at REPAIR: fund, stock and care policy." I'm not going
to pretend that's a dramatic before/after the way, say, unlocking a whole new building
skin would be. It's a lamp. But it's a real lamp that wasn't there before, in a real
screenshot from tonight, and the game doesn't oversell it as more than that, which I'd
rather have than a screenshot doing more work than the actual room does. Priya called it
"a porch light" too when she looked at the same two screens, so I don't think I'm being
uncharitable here — we both landed on the same honest description independently.

## Where the actual coziness lives, and it's not the porch light

Here's what I did not expect going in: the best "does this feel like a home" writing in
the whole build isn't the refuge panel's mechanics text, it's two small lines Neri says
once you have the place:

> "I used to sleep with my shoes on. Last night I left them under the bed. Don't laugh.
> That's what your name beside mine on the door bought me."

I got a little emotional reading that off `C05-refuge-offer.png`, if I'm honest, honestly.
That's not a stat description. That's someone telling you, sideways, that they finally
feel safe enough to take their shoes off at night. I have been asking this whole project
for a home that feels like a home rather than a base of operations, and that sentence is
the first time it actually landed for me rather than being implied by a bed icon on a
panel.

The flip side of that same well is just as honest when things go wrong. If you reserve
medicine for the crew and a real named person — Edda, the same person Neri mentioned in
her very first line of dialogue back in Demo 02 — gets turned away, you get this:

> "Edda came after her shift. I told her the shelf was empty, with two doses behind my
> back. If this is our place, that lie belongs to both of us."

I want to flag this one carefully because of my own blind spot: that line stung a
little, in a good way, the way an actual consequence in a game I love is allowed to
sting. It's optional — the default policy is "PUBLIC CARE / USE EVERY DOSE," the kind
setting, and you have to actively click "RESERVE LAST 2 FOR CREW" to go looking for
that particular feeling. I did click it, on purpose, because I wanted to see what it
did, and I'd gently warn a player who's just here for a soft night that this one button,
out of everything in the refuge panel, is the one that can make Neri say something that
sits with you. It's not punishing — nobody dies, no debt appears, it's a number and a
line — but it's not nothing either, and I'd rather tell you it's there than have you
click it expecting a stat bump and get a small gut-punch instead.

## Recovering at the refuge — kind, but I missed the last demo's stagecraft

The mechanics here are exactly as gentle as promised, and I checked the actual code
rather than just the panel text: `RefugeState.cs`'s `RefugeRest()` requires bleeding to
be stopped first (a plain bandage, same rule as Demo 02), caps healing at 80 rather than
100, and explicitly adds no debt — "A meal and a clean bed restored up to 80 health for
those at the clinic. No debt, no medicine spent. Serious wounds still need treatment."
On the panel, the button reads "REST TO 80 / NO DEBT," plainly, no fine print.

Here's my one small honest ding, and it's a comparison to their *own* best work rather
than a real complaint: Demo 02's defeat screen, "STILL BREATHING," was a whole titled
screen with "Neri dragged you out. You have someone to come home with" on it, and it
felt like being tucked in. Refuge rest doesn't get that same staging — it's a button on
a management panel and a line that goes into your journal, which is warm in the reading
but doesn't get its own quiet moment the way getting rescued did. I don't think this
needed a whole cutscene. But if the team has budget left for one more pass of polish
before calling this system finished, I'd spend it on making "REST TO 80 / NO DEBT" feel
like the good news it actually is, the same way "STILL BREATHING" made bad news feel
survivable.

## Anything cruel hiding in the new systems? I went looking.

Specifically for someone who isn't trying to optimize anything, just wants a quiet
night: I didn't find anything I'd call mean. The reserve-medicine choice, covered above,
is opt-in and reversible (there's an "OPEN SHELF TO EVERYONE" button right there once
you've reserved), so nobody gets trapped in the sad version of that story. The new
traffic system is explicitly disclosed as ambient — "no ramming damage," cars just wait
if you're in their way — and I didn't find, or hear from anyone else's pass, any way it
can hurt or punish the player directly; the worst it does is make a lane wait, which is
a Tuesday, not a threat. The twelve-minute buyer appointment (21:52, same shape as Demo
02's shipment clock) is still handled the way I liked last time: it's a fixed time on
the HUD, not a countdown ticking down at me, and I didn't feel it pressing on me any
harder than I did last demo. My blind spot didn't get triggered by anything new here.

## Would I put this on after a bad day?

Yes, more than last time, actually. Last demo I said yes-with-a-caveat for the
clinic-and-cargo half and only-on-a-brave-day for the Ivo half. That caveat hasn't
changed — I still wouldn't go looking for a fight with Rook on a rough night, and that's
fine, the game still lets me not do that. What's new is that the *home* half of the
evening got a real upgrade: a cat I can actually reach out to, a couple with two names
on a door who talk like people who just started sleeping easier, and a small honest lamp
outside to prove it. That's the exact kind of thing I check in on my own save file for
no reason four years after the mortgage is paid off. I believe this place could get me
doing that too.

## Where I differ from the other three

Dag, Marcus and Priya all landed on 8 as well, but I want to be clear I'm not just
rounding to the room. Dag's 8 is about a ledger that didn't crack under a second spend
path; Marcus's is about a regression check across three demo generations coming back
clean; Priya's is about Ivo and Neri finally sounding like people instead of a shrug
wearing three costumes. I didn't re-litigate any of that, and I don't need to — I trust
their read the same way I did last time. My 8 is about one very specific, very personal
thing: I asked for a cat I could touch, and this time I got the receipts to prove it
happened, not just a doc that says it should have. That's a smaller axis than theirs.
It's still the one I actually came here to check.

## The number

8/10, up from 7. The top note from my last review — pet the cat — got answered for
real, with a key, an animation, a line, and a save file that remembers it, and I checked
every part of that claim myself instead of taking it on faith this time. The refuge
exterior is honestly modest, the recovery moment could use a little more of the
stagecraft the defeat screen already knows how to do, and the reserve-medicine choice
can sting if you go looking for it — but none of that is cruelty, and all of it is a
game that keeps being more honest with me than it strictly has to be. I'd play this one
again tonight. I'd check on the cat first.
