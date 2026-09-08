# Recorded sound assets

All selected third-party sounds are distributed under [CC0 1.0 Universal](https://creativecommons.org/publicdomain/zero/1.0/).

| Use | Source and creators |
|---|---|
| Pistol and shotgun reports | [The Free Firearm Sound Library](https://opengameart.org/node/21826), Ben Jaszczak, Brian Nelson, Kevin Heras and Matthew Nanney. Selected near-distance 1911 and Mossberg recordings. |
| Reload and shotgun action | [Gun reload sounds](https://opengameart.org/content/gun-reload-sounds), SpringySpringo. These mechanics were recorded with airsoft guns. |
| Concrete footsteps, cover and body impacts | [Impact Sounds 1.0](https://kenney.nl/assets/impact-sounds), Kenney. Original license notice: `Kenney-Impact-License.txt`. |
| Quiet pickup, cash and alert feedback | [Interface Sounds 1.0](https://kenney.nl/assets/interface-sounds), Kenney. Original license notice: `Kenney-Interface-License.txt`. |

[provenance.json](provenance.json) contains exact download URLs, archive/source/output SHA-256 hashes, selected filenames, crop windows, sample durations, peak levels and decoding commands. `Tools/Prepare-Audio.py` reproduces the selected 44.1 kHz mono PCM16 assets from the ignored `Temp/audio-source` downloads. Full vendor archives are not shipped or committed.

Preparation preserves one firearm discharge per clip, removes the long lead-in and second shots, applies a 35 Hz high-pass filter, sets explicit peak headroom and adds short boundary fades. Reload start and completion are separate short recorded actions; interrupted reloads do not play a complete prerecorded reload sequence. Four footstep variants avoid repeating one tone. Pellet impact accents are rate-limited without changing collision or damage events. Distant shots/impacts are attenuated relative to the player.

These packs provide one-shot effects, not environmental wind or harbor ambience. There is no ambient drone or added music loop. Measured sample validation establishes file format, durations, peaks and identity; it does not establish human approval of the mix or claim the preparer heard the audio.
