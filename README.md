# Funstra

An isometric sandbox crime RPG about surviving a damaged port city and becoming someone it depends on.

**Release: Nobody Gets Home Alone, Demo06 v0.6.0.** Windows x64, Unity 6000.4.0f1. [Validation and delivery receipts](Evidence/VALIDATION.md) distinguish the reviewed build, display integration and publication.

Control the protagonist, Neri and Rell as separate people with their own wounds, equipment and remembered rescues or abandonment. One impounded pump component supports paid release, theft or a crew fight; repairing Rell's auxiliary pump offers a nonviolent partnership. Rifle combat, finite field aid, physical carrying and crew orders extend the existing district. The darker port art and named residents' conduct are included. [Manual and routes](NOBODY-GETS-HOME-ALONE.md). Operations can fail and casualties can be left behind; this build implements incapacitation and existing total-wipe recovery, not irreversible death.

## Play

[Download and previous versions](https://kaliffen.github.io/Funstra/) · [Release notes](https://github.com/Kaliffen/Funstra/releases/tag/v0.6.0) · [Review dossier](https://kaliffen.github.io/Funstra/dossier.html)

Locally `Play.cmd` opens `Build/NobodyGetsHomeAlone/Funstra.exe`; its portable package is `Releases/Funstra-nobody-gets-home-alone-0.6.0-windows.zip`. Extract a whole portable archive before launching its executable alongside the data folder and DLLs. Earlier local players are retained, including `Build/PressureEscape/Funstra.exe`, `Build/PriceOfAGun/Funstra.exe` and its [Demo05 guide](THE-PRICE-OF-A-GUN.md).

A witnessed attack alerts patrols to the reported gunman. Police return real projectile fire and arrive in the same two finite trucks, with six additional officers for a maximum of nine. Demo06 adds rifles to the yard watch and reinforcement response, with communicated sightings, finite guard aid and retreat. The expanded Old Port, enterable clinic, pistol/shotgun/SMG combat, Sella's finite shop, Tomas's three physical supply consignments, cargo and four prepared foundation levels remain included. Army scale, grenades and 30-plus soldiers remain future work.

WASD moves, Shift sprints, Ctrl/C sneaks, mouse aims and left mouse fires. F1/F3/F4 selects protagonist/Neri/Rell; K opens crew orders, transfers and rescue. 1 holsters; 2/3/4/5 selects pistol/shotgun/SMG/rifle for the protagonist or draws a companion's carried gun. R reloads, B begins timed self-aid, E interacts, Space pauses and Tab opens the map. See the [Demo06 manual](NOBODY-GETS-HOME-ALONE.md) for carry, clinic admission, abandonment and the session debug running toggle.

The game saves to `crew-progress.json` under `%USERPROFILE%/AppData/LocalLow/Funstra/Funstra`; Continue can import earlier arms/street saves while preserving the originals. Existing trade, cargo and clinic services use the protagonist; selected companions use the new dock and crew/rescue systems. Automated tests stay muted. The [Demo06 dossier](Docs/funstra-review-dossier-demo06.html) preserves four independent guided reviews: Game now **5.3–5.7/10**, with explicit deductions for missing depth. Original full eight-route evidence and later display-fix regressions are identified separately; human acceptance remains separate. The [earlier guide](PRESSURE-AND-ESCAPE.md) and [dossier](Docs/funstra-review-dossier-pressure-escape.html) remain v0.4.2 history.

The published v0.4.1 player remains locally in `Build/PoliceResponse`, with its [guide](POLICE-RESPONSE.md) and [dossier](Docs/funstra-review-dossier-police-response.html) preserved. The original approved Demo04 remains in `Build/StreetsWorthFightingFor`. [Demo03's guide](KEEP-THE-LIGHTS-ON.md) and [dossier](Docs/funstra-review-dossier-demo03.html) are earlier historical records.

## Read and work

| Question | Maintained source |
|---|---|
| What should this game become? | [VISION.md](VISION.md) |
| What is this city and why does it exist? | [WORLD.md](WORLD.md) |
| What changes next, including maps and the six-release arc? | [DESIGN.md](DESIGN.md) |
| What is planned, underway or accepted? | [Epic #26](https://github.com/Kaliffen/Funstra/issues/26) and its release issues |
| How do we build, review and publish? | [Docs/PIPELINE.md](Docs/PIPELINE.md), with linked CD/reviewer skills |
| What did the current build prove? | [Evidence/VALIDATION.md](Evidence/VALIDATION.md) |
| What did reviewers conclude? | [Demo06 dossier](Docs/funstra-review-dossier-demo06.html) and [original reviewer records](reviewer-agents/README.md) |
| Where are older documents? | [Archive index](Docs/Archive/README.md) |

Update these owners instead of adding parallel roadmaps or status documents. Versioned guides and reviews are historical build records, not current planning instructions. Original reviews and scores remain intact.

## Build

Use the installed Unity CLI from the repository:

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.BuildCrew --log-file Evidence/build-crew.log --no-tail --non-interactive
```

An active Unity license is required. Read [validation commands and save-isolation rules](Evidence/VALIDATION.md) before tests; read [the pipeline](Docs/PIPELINE.md) before packaging. A build command alone does not complete review or publication.

The only available test GPU is an RTX 4090. Low-end compatibility is a target, not a verified minimum specification. No offline simulation, player driving, large controllable squads or citywide faction management is implemented.

Game code is MIT licensed. Asset and audio provenance: [CREDITS.md](CREDITS.md). No external services are required to play.
