# Funstra

An isometric sandbox crime RPG about surviving a damaged port city and becoming someone it depends on.

**Published game: Demo 03 — Keep the Lights On, v0.3.0.** Windows x64, Unity 6000.4.0f1. The six-release plan is future work, not functionality in this download.

## Play

[Download and previous versions](https://kaliffen.github.io/Funstra/) · [GitHub release](https://github.com/Kaliffen/Funstra/releases/tag/v0.3.0)

Locally run `Play.cmd` or `Build/KeepTheLightsOn/Funstra.exe`. Extract all of `Releases/Funstra-demo-03-windows.zip` before running its executable alongside the data folder and DLLs.

Start at Neri's cyan REPAIR clinic. Acquire medicine by payment, theft or force; earn a medical partner, repair a shared refuge and decide whom its limited supplies protect. Mara's jobs and cargo remain available. The clinic is represented outside; enterable rooms and a working supply network are planned.

WASD moves, E interacts, Space pauses tactically, Tab opens the map, J the journal, F the nearby refuge and Escape pauses. See the [Demo 03 guide and review route](KEEP-THE-LIGHTS-ON.md) and its linked [Demo 02 controls/system reference](BED-AND-BANDAGE.md). Normal saves use `lights-progress.json` under `%USERPROFILE%/AppData/LocalLow/Funstra/Funstra`; earlier demo saves are preserved on import.

## Read and work

| Question | Maintained source |
|---|---|
| What should this game become? | [VISION.md](VISION.md) |
| What is this city and why does it exist? | [WORLD.md](WORLD.md) |
| What changes next, including maps and the six-release arc? | [DESIGN.md](DESIGN.md) |
| What is planned, underway or accepted? | [Epic #26](https://github.com/Kaliffen/Funstra/issues/26) and its release issues |
| How do we build, review and publish? | [Docs/PIPELINE.md](Docs/PIPELINE.md), with linked CD/reviewer skills |
| What did the current build prove? | [Evidence/VALIDATION.md](Evidence/VALIDATION.md) |
| What did reviewers conclude? | [Demo 03 dossier](Docs/funstra-review-dossier-demo03.html) and [reviewer records](reviewer-agents/README.md) |
| Where are older documents? | [Archive index](Docs/Archive/README.md) |

Update these owners instead of adding parallel roadmaps or status documents. Versioned guides and reviews are historical build records, not current planning instructions. Original reviews and scores remain intact.

## Build

Use the installed Unity CLI from the repository:

```powershell
unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/build.log --no-tail --non-interactive
```

An active Unity license is required. Read [validation commands and save-isolation rules](Evidence/VALIDATION.md) before tests; read [the pipeline](Docs/PIPELINE.md) before packaging. A build command alone does not complete review or publication.

The only available test GPU is an RTX 4090. Low-end compatibility is a target, not a verified minimum specification. No offline simulation, driving, large squads or citywide faction management is implemented in Demo 03.

MIT licensed. Original procedural geometry and sound; no external services required to play.
