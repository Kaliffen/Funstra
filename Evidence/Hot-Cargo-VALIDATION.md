# Hot Cargo validation

2026-09-07 — Windows x64, Unity 6000.4.0f1, Direct3D 11.

- Final Windows build: succeeded, 0 errors, 0 warnings. Built with the installed `unity build . --target StandaloneWindows64 --execute-method FunstraBuild.Build --log-file Evidence/build.log --no-tail --non-interactive` CLI command. See build-result.txt and build.log.
- 75 editor rule/persistence/navigation assertions passed, including 26 new cargo checks. See rules-result.txt.
- 88 exported-player assertions passed in the visible full playthrough. See runtime-result.txt and player.log.
- Twelve non-black screenshots captured. The new/changed title, cargo map, loaded HUD, safehouse, extraction, ending and pause layouts were visually inspected. Final captures include the shorter HOME sign and extraction prompt. See visual-result.txt and 01-title.png through 12-extraction.png.
- Player and visual-player logs contain no runtime exceptions or errors.

Coverage includes all three original jobs, character-controller traversal to every cargo site, actual hold-to-pickup and hold-to-bank interactions, capacity rejection, burden rules, guaranteed bonded-cargo alarm, heat-blocked banking/management, interruption on releasing E, extraction rewards, duplicate purchase refusal, upgrade persistence, finished-campaign free roam, reload clearing loose cargo, police sight/pursuit, building occlusion, search cooldown, actual arrest/confiscation, pause and NPC patrol movement. Editor tests also cover legacy save loading and invalid cargo requests.

The full runtime pass preceded the final presentation-only refinements (shorter sign/prompt, HOME map label, nearest-cargo guidance in free roam and a staged screenshot prop refresh). The final build reran all 75 editor checks and the twelve-screen capture suite.

Test boundaries: NPCs are frozen during deterministic traversal; police pursuit/arrest and patrol movement are exercised separately with simulation enabled. Screenshots stage situations for visual review. Movement-based extraction cancellation is implemented but was not separately exercised by the automated test. Human difficulty balancing, long-session economy balance and other hardware remain unverified. User acceptance is pending.

Environment findings: the first sandboxed Editor build could not access its license; an approved build outside the sandbox succeeded. A hidden-window runtime run timed out on an unobstructed initial route; visible execution passed the whole route and suite. Use `Tools/Test-Player.ps1 -Visible` for this machine. The original pre-slice demo remains in Releases/Funstra-demo-windows.zip.
