# Meeting memory — Dag Møller (Demo 03)

## The "next demo" conference

The organizer pulled the four of us back together after Demo 02 to talk about where the slice was
headed next, before any of us had touched the new build. The pitch, as relayed: a porch light, a
shared refuge, and Tally finally pettable. I said what I always say to a feature list before I've
seen the state machine behind it — a porch light is art direction, a refuge is a claim about a
resource, and "pettable cat" is either a genuine interaction system or a cursor-change wearing a
tail. I wasn't hostile about it. Nell was, predictably, delighted by all three in the abstract. My
interest was narrower: whether "shared refuge" meant a second sink on the same twelve-dose ledger I
spent the last review auditing, or a cosmetic room that happens to sit next to the clinic. Those are
very different features wearing the same screenshot.

## Demo 02 went public

Worth recording for the file: Demo 02 has since shipped with all four of our scores published
unedited, mine included. My 7/10 stands as written — I have not revised it, and nothing in this
Demo 03 pass changes my accounting of that build, because Demo 03 doesn't touch the systems I
scored it on, it extends them. The organizer was clear that our dossier informed the release but our
scores are not the same thing as acceptance of the build; I take that at face value and it matches
how I'd want a review used regardless.

## What I wanted verified going in

Three things, all carried over from unfinished business in the Demo 02 write-up:

1. **The medicine ledger still closes at twelve, now with a refuge and a reserve policy pulling on
   it.** A second consumption path (public treatment) and a second spend path (refuge repair, clinic
   fund) are exactly the kind of addition that silently cracks a conservation law if nobody re-checks
   it after the fact. I wanted `TotalMedicine==12` re-proven with the new transactions in the mix, at
   the state layer, not read off a HUD number.
2. **The reserve-policy threshold.** The design guide states public patients get turned away only
   once the shelf reaches two doses. That is a boundary condition, and boundary conditions are where
   off-by-one errors live. I wanted to see the actual comparison operator, not just an end-state
   screenshot showing "2" on it after the fact.
3. **Whether a car can be held up indefinitely by a stationary pedestrian**, and whether that's an
   admitted design boundary or a bug nobody wrote down. The doc says "a person can hold up a lane
   until they leave" in its own boundaries section, which is either an honest disclosure or a
   euphemism for a stuck simulation, and I wanted to see which by reading the traffic code rather than
   trusting the sentence.

I also carried the two specific Demo 02 debts forward without prompting: no visible countdown for the
scheduled shipment, and a history log that timestamped three different beats all "00:00." The release
note claims both are fixed for this build ("deadline is legible," "history is honest"). I went in
expecting to have to prove that, not take the changelog's word for it.
