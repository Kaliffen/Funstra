# Keep the Lights On (Demo 03) — playtest review

**Verdict: the ledger still closes, the reserve threshold is exactly where the doc says it is, and the traffic disclosure is honest about its own limits. 8/10.**

## What I actually ran

Working directory `~\repos\Funstra`, build under test `Build\KeepTheLightsOn\Funstra.exe`
(version 0.3.0, Assembly-CSharp.dll SHA256 `4A219371340B2FBB1631C5C0F73D1843B3204C67967E38C10728D4FEEC8A4449`
per `Evidence\demo-03-build-info.txt`, packaged 2026-09-07T21:37:28Z).

- `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Full -Visible` — 78 named
  assertions, one bare `PASS` header, zero FAILs. Fresh evidence at `Evidence\bandage-runtime-result.txt`
  (78 lines, I counted them) and `Evidence\bandage-full-player.log`, which I read to the tail: clean
  shutdown, no exceptions, no warnings grep-able anywhere in the file.
- `powershell -ExecutionPolicy Bypass -File Tools\Test-Demo.ps1 -Mode Visual -Visible` —
  `Evidence\bandage-visual-result.txt`: "Twelve new demo screens captured without black rendering" and
  "Twelve Demo 03 review screens rendered," which produced the fresh B01–B12 and C01–C12 sets.
- Read `Evidence\VALIDATION.md` in full. It claims 133 editor assertions (75 existing + 32 medical +
  26 refuge/dialogue/route/vehicle) and 166 exported-player assertions across the medical/refuge suite
  and the legacy campaign suite, plus 24 non-black rendered screens. I did not re-run the editor rule
  suites myself — that requires the Unity Editor CLI, which I don't have wired up here — so the 133
  editor-side number is VALIDATION.md's claim, not something I re-derived. What I could and did verify
  independently is the 78-assertion exported-player number (matches exactly) and the 24 screens
  (I counted 12 B-series plus 12 C-series PNGs on disk, matches exactly), plus the actual C# source of
  the two new editor rule classes, below.
- Read `Assets\Editor\RefugeRules.cs` (49 lines, 26 checks per its own counter) and
  `Assets\Editor\DistrictRules.cs` (66 lines, 32 checks) in full — not because I doubt the pass/fail
  count, but because a passing assertion only tells you the code agrees with itself, not that it's
  testing the right thing. Also read `Assets\Scripts\DistrictState.cs` (the reserve-policy branch) and
  `Assets\Scripts\CityTraffic.cs` (the traffic yield/junction logic) directly, the way I went into
  `DistrictRules.cs` last time for the dose count.
- `Evidence\refuge-rules-result.txt` reads "PASS: 26 refuge, dialogue, schedule and prop route
  assertions" — 26, not the 22 I'd been told to expect going in; I'm reporting the number the file
  actually contains. `Evidence\district-rules-result.txt` reads "PASS: 32 district simulation,
  conservation, recovery and migration assertions," unchanged from Demo 02's own count, which is
  consistent with the twelve-dose economy being extended rather than replaced.
- Viewed nine of the twenty-four screens directly: C01 (clinic-and-Tally establishing shot), C05
  (refuge offer dialogue), C06 (refuge management panel, first visit), C07 (refuge panel after reserve
  policy has turned people away), C08 (repaired street exterior), C09 (Ivo's post-violence dialogue),
  C10 (history log after the reserve consequence), C11 and C12 (traffic before/after a short interval).
  Also B08 (history log, the screen I specifically flagged last time).

Both harness passes completed cleanly on the first attempt, same as Demo 02. Nothing here required a
retry, a longer timeout, or manual intervention. I'll keep not making a production of a clean run, but
two-for-two on a rebuilt harness against a build with new systems in it is worth the sentence.

## The medicine ledger, extended, not replaced

My first question going in was whether adding a refuge fund and a public/reserve care split would
survive contact with the twelve-dose conservation law I spent most of my last review verifying.
`DistrictRules.cs` still runs its four independent conservation checkpoints unchanged (`TotalMedicine
==12` after an idle 721-second autonomous tick, after the same span in 7,210 increments of 0.1s, after
a paid release and treatment, and after a violent confiscation), which tells me the base economy the
refuge sits on top of wasn't touched. `RefugeRules.cs` then re-proves the same invariant with the new
transactions folded in: `d.FundClinic(run)&&d.BuyClinicReserve()&&d.supplierStock==2&&d.TotalMedicine
==12&&money(run)==total` after both a clinic-fund contribution and a $30 supplier purchase, and again
after switching the care policy off and letting more patients get treated
(`d.treatments>treated&&d.TotalMedicine==12&&money(run)==total`). That is the correct instinct — don't
just add a new sink and assume the old proof still applies, re-run the invariant with the new sink
active — and it's exactly what I asked for after Demo 02: proven at the state layer, not read off a
panel. The panel agrees anyway: C06 shows "SHELF 8 / SUPPLIER 4" cleanly split from "FUND $80," and the
arithmetic checks out on screen too — clinic fund starts at $20 (matching Demo 02's B03 reading), the
refuge repair moves $60 of the $120 into it, landing at $80, which is exactly what `RefugeRules.cs`
asserts (`d.clinicMoney==80`) after the same sequence. Source and screenshot agree with each other and
with the stated $60/$60 split. No transaction here manufactures a thirteenth dose; the doc says as much
and the code and the screen both back it.

## The reserve-policy threshold — the part I actually came for

This is the check I most wanted to do myself rather than trust. The design guide states "reserve
policy only binds when the shelf reaches two doses." I went to `DistrictState.cs` rather than the test
harness for this one, because a harness assertion only proves the endpoint was reached, not that the
boundary is exact. Line 72 of `DistrictState.cs`:

```
if(clinicStock>(refuge&&reserveMedicine?2:0)&&patientMoney>=8) { clinicStock--;consumed++;patientMoney-=8;clinicMoney+=8;treatments++; }
else if(refuge&&reserveMedicine&&clinicStock>0) { refusedPatients++; ... }
```

That is the actual comparison, and it is exactly the threshold the doc claims: with the reserve policy
active, treatment continues normally at stock 3 and above (`clinicStock>2`), refusal only begins once
stock is down to 2 or 1 (`else if ... clinicStock>0`), and at stock 0 a separate branch fires
("shortage," not "refused") rather than double-counting the same event two ways. It doesn't fire early
— a clinic sitting at 3+ doses with the reserve flag on still treats the public, which
`RefugeRules.cs`'s own end-state check doesn't actually exercise (it only samples the state after
Tick(900), by which point stock has already fallen to 2 — it never asserts refusedPatients==0 while
stock is still 3+). That's a real gap in the automated coverage, not a bug in the game: I confirmed the
correct behavior by reading the comparison operator directly, but the test suite itself doesn't prove
the "not too early" half of the claim, only the "eventually happens" half. I'm crediting the game and
docking the test suite for that distinction, because they're different claims and only one of them is
actually machine-checked here.

The player-facing side of this is a genuine improvement over Demo 02's opacity. C07, captured after the
reserve policy has been active a while, shows "SHELF 2 / RESERVE LAST 2 FOR CREW / 5 turned away by
policy" directly on the refuge panel, alongside Neri's line about lying to Edda that the shelf was
empty — "If this is our place, that lie belongs to both of us." That refusal counter is a number I
complained last time never made it past the C# into anything a player could read; here it's on the
same screen as the toggle that causes it. B08's and C10's history log entries independently confirm
the same event chain in the journal: "#6 21:40:00 / You reserved the last two clinic doses for the
crew," "#7 21:50:30 / A dockworker left untreated. Two doses remain, but you reserved them for the
crew." Three independent surfaces — source comparison, refuge panel counter, and history log — all
agree on the same mechanic. That's the kind of legibility I was asking for and didn't get last time.

## The history log and the clock — both debts from Demo 02 actually paid

Demo 02's history log timestamped three unrelated beats all "00:00," which I called the demo's one
real UI failure. B08 here (an early-game capture, same review beat as the old one) shows entries "#1
21:40:00 / Took the impounded medicine," "#2 21:40:00 / You supplied the clinic," "#3 21:40:00 / Neri
joined you" — still identical timestamps, but this time because those three things plausibly did
happen in the same tick of simulated time at the very start of a run, not because the clock is broken.
C10, captured later in the same session, proves the distinction: "#6 21:40:00," "#7 21:50:30," "#8
21:56:41" — three genuinely different times as real time passed. That's the fix the release note
claims ("simultaneous actions remain simultaneous rather than acquiring fabricated delays"), and I can
see the difference between "the clock is stuck" and "these three things were actually simultaneous"
by comparing an early-game screen against a later one, which Demo 02 never gave me the means to do.
Credit where it's due — this was a real complaint and it got a real fix, not a re-skin.

The countdown complaint is handled differently but also honestly. C01 shows "BUYER ARRIVES 21:52 /
DETAILS IN J" sitting on the HUD as a fixed appointment against the port clock, not a ticking countdown
timer. That's a legitimate design choice, not a dodge — a dominating countdown and a fixed appointment
time are different pressure mechanisms, and the doc says explicitly it chose the latter on purpose so
the deadline doesn't eat the whole session. I'd have accepted either; what I wanted was for the
information to exist on screen at all, and it does. I have not personally timed a session to $100 versus
$140 psychological difference this way versus a bare countdown, but the underlying tax itself
(`PayRelease` before/after the sale) is unchanged from Demo 02 and I already verified that one hits.

## Traffic — real physics or ambient wallpaper

I read `Assets\Scripts\CityTraffic.cs` end to end rather than trust the harness assertion names alone,
because "four cars move" and "four cars simulate real intersection conflict" are very different claims.
What's actually there: each car checks its own projected bounding box plus a 1.2-unit braking corridor
against every pedestrian position, every other car's current footprint, and — for the two cars running
the cross-street route — a conservative junction-occupancy check that looks for any car within 10 units
of the intersection center before allowing a crossing car to commit (`CityTraffic.cs` lines 55–60). A
car that finds a conflict simply sets `blocked=true` and does not move that frame; there is no timeout,
no reroute, no honk-and-force-through. That is a genuinely simple model, and it is also exactly what
the doc's own boundaries section discloses: "a person can hold up a lane until they leave," "junction
checks are deliberately conservative." I went looking for a hidden escape hatch that would make that
statement false in the game's favor — a patience timer, an alternate-route fallback — and there isn't
one in the code. The harness backs the honest half of this: "Car brakes before pedestrian and preserves
physical clearance" and "Car resumes after pedestrian clears lane" both passed, and I watched the
transition on C11→C12, where the same two cars have advanced along their routes across the interval
between captures — real movement, not a static diorama. What the harness does not and, by its own
scope, cannot demonstrate is the failure mode the doc admits to: a pedestrian who never leaves does
in fact stall a lane forever, because the blocking check has no escape clause. That's not a bug I'm
reporting — it's a disclosed simplification I confirmed at the source level rather than took on the
document's word, which is the distinction I actually care about.

The four traffic-car assertions each report a distance traveled over the run ("455.415m," "451.6643m,"
"447.2116m," "449.212m" — precise enough to be read from an actual accumulator, not a rounded flavor
number) with "without intersection deadlock" in the assertion name, which is the right thing to check
given the junction logic above — four cars sharing two routes through one junction is exactly the
configuration where a naive conservative check could livelock all four cars against each other, and it
apparently doesn't.

## Refuge as a capability, not a room skin

`RefugeRules.cs` proves the gating I'd have wanted to see fail if it were faked: `OpenRefuge` refuses
before recruitment ("A refuge cannot buy a relationship"), refuses a second purchase at the same state
("No duplicate purchase or lost money"), and — the check I find most telling — `d.trust=-3;check(!d.
RefugeRest()&&!d.FundClinic(run)&&!d.SetClinicPolicy(true),"Betrayal removes shared refuge
privileges")`. A betrayed partnership doesn't just stop being nice about the room, it locks out rest,
fund contributions, and policy changes simultaneously — three independent gates keyed to the same trust
flag, which is a more coherent consequence model than most demo-scale "relationship" systems bother
with. The health-recovery rule is also correctly ordered: bleeding must be stopped by an actual bandage
before `RefugeRest()` will succeed at all (`d.health=35;d.bleeding=true;check(!d.RefugeRest()...);d.
BandagePlayer();check(d.RefugeRest()&&d.health==80&&d.debt==0...)`), matching the doc's "stop bleeding
first, full recovery still needs medical treatment" line precisely, and it caps at 80 rather than 100,
which is the promised "practical security," not a free full heal. C08 shows the exterior change — a lit
doorway and repaired storefront replacing whatever was there before — which is honestly labeled in the
guide as "an exterior representation of the room, not an enterable interior," and the screenshot doesn't
oversell it: there's no interior to walk into, and the panel is where the actual mechanics live, which
matches what was promised rather than dressing up a facade as more than it is.

## Where I'd still dock

The 26-assertion refuge suite proves the reserve threshold is reached correctly but never asserts the
negative case — that treatment continues normally above the threshold — leaving that half of the claim
verified by me reading the comparison operator rather than by the automated suite itself. That's a
coverage gap worth naming even though the underlying code is correct. Second, I have not independently
reproduced the 133-editor-assertion figure or the "75 existing checks" baseline in VALIDATION.md; I
verified the two new rule files and their counts by reading the source, and I verified the
exported-player and screenshot counts by running the harness myself, but the older editor-rule
regression suite is outside what I re-ran this pass, and I'm not crediting a number I didn't check.
Third, the same long-session cheese question I flagged after Demo 02 for the guard's search radius has
a direct analogue here for traffic: the harness proves cars don't deadlock over a bounded run and that
they yield correctly to a pedestrian who eventually moves, but nothing here or in the source stress-tests
what happens to the wider simulation if a player deliberately parks in a lane indefinitely across a much
longer session than 3,600 steps — the doc admits the car simply waits forever, but "forever" in a
90-second harness step count and "forever" across a real play session carrying other scheduled events
are not proven to be the same thing, and I'd want a longer run before calling that settled either way.

## The number

Eight. The core ledger absorbed a second spend path without cracking, and I confirmed that at the same
level of rigor I demanded last time rather than taking the panel's word for it. The reserve-policy
threshold is exactly where the document says it is, confirmed by reading the actual comparison rather
than an end-state screenshot, and it's now visible to the player on the same screen that causes it — a
direct, specific fix to what I docked hardest for last time. The history-log timestamp bug is
genuinely fixed, not re-skinned, and the traffic system's stated limitation turned out to be exactly as
disclosed when I went looking for a hidden exception to it. It doesn't get a nine because a chunk of
the claimed editor coverage (the older 75-check baseline) is outside what I personally re-verified this
pass, and because the new refuge test suite proves its headline threshold reaches the right number
without proving it doesn't fire early — a gap in the tests, not the game, but I dock the same regardless
of which side of that line the fault sits on, because "the game is probably fine, trust me" is the
exact sentence I refuse to accept from anyone else's spreadsheet.
