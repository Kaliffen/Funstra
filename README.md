# Funstra

An isometric sandbox crime RPG about surviving a damaged port city and becoming someone it depends on.

**Current release: Police Response, v0.4.1.** Windows x64, Unity 6000.4.0f1. This is the first enforcement tier after the owner-approved Demo04, before the remaining Demo05 acquisition and supply work.

## Play

[Download and previous versions](https://kaliffen.github.io/Funstra/) · [Release notes](https://github.com/Kaliffen/Funstra/releases/tag/v0.4.1) · [Review dossier](https://kaliffen.github.io/Funstra/dossier.html)

Locally run `Play.cmd` or `Build/PoliceResponse/Funstra.exe`. Extract all of `Releases/Funstra-police-response-0.4.1-windows.zip` before launching its executable alongside the data folder and DLLs.

A witnessed attack alerts the patrols to the reported gunman. Police return real projectile fire and arrive in two trucks, with six additional officers for a maximum of nine. Break sight and leave the search area; pause, wounds, ammunition, casualties and recoverable defeat remain part of the same campaign. The expanded Old Port, enterable clinic, pistol/shotgun combat and four prepared foundation levels are included. Rifle squads and 30-plus soldiers with grenades remain future slices.

WASD moves, Shift sprints, Ctrl sneaks, mouse aims, left mouse fires, 1/2/3 selects fists/pistol/shotgun, R reloads, B bandages, E interacts, Space pauses and Tab opens the map. See the [current guide](POLICE-RESPONSE.md), [four independent reviews](Docs/funstra-review-dossier-police-response.html) and [validation](Evidence/VALIDATION.md). Automated tests stay muted. Normal saves use `streets-progress.json` under `%USERPROFILE%/AppData/LocalLow/Funstra/Funstra`; earlier saves are preserved on import.

The original approved Demo04 remains locally in `Build/StreetsWorthFightingFor`. [Demo03's guide](KEEP-THE-LIGHTS-ON.md) and [dossier](Docs/funstra-review-dossier-demo03.html) describe the previous published version.

## Read and work

| Question | Maintained source |
|---|---|
| What should this game become? | [VISION.md](VISION.md) |
| What is this city and why does it exist? | [WORLD.md](WORLD.md) |
| What changes next, including maps and the six-release arc? | [DESIGN.md](DESIGN.md) |
| What is planned, underway or accepted? | [Epic #26](https://github.com/Kaliffen/Funstra/issues/26) and its release issues |
| How do we build, review and publish? | [Docs/PIPELINE.md](Docs/PIPELINE.md), with linked CD/reviewer skills |
| What did the current build prove? | [Evidence/VALIDATION.md](Evidence/VALIDATION.md) |
| What did reviewers conclude? | [Police Response dossier](Docs/funstra-review-dossier-police-response.html) and [reviewer records](reviewer-agents/README.md) |
| Where are older documents? | [Archive index](Docs/Archive/README.md) |

Update these owners instead of adding parallel roadmaps or status documents. Versioned guides and reviews are historical build records, not current planning instructions. Original reviews and scores remain intact.

## Build

Use the installed Unity CLI from the repository:

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/build.log --no-tail --non-interactive
```

An active Unity license is required. Read [validation commands and save-isolation rules](Evidence/VALIDATION.md) before tests; read [the pipeline](Docs/PIPELINE.md) before packaging. A build command alone does not complete review or publication.

The only available test GPU is an RTX 4090. Low-end compatibility is a target, not a verified minimum specification. No offline simulation, driving, large squads or citywide faction management is implemented in Demo 03.

Game code is MIT licensed. Asset and audio provenance: [CREDITS.md](CREDITS.md). No external services are required to play.
