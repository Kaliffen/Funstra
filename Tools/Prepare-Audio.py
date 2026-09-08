"""Rebuild selected CC0 WAV assets from downloads in ignored Temp/audio-source.

Exact download URLs and source archive hashes are in Resources/Audio/provenance.json.
Requires ffmpeg on PATH; no package installation. Does not download or modify vendors.
"""
from pathlib import Path
import array, hashlib, json, math, subprocess, wave

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'Temp/audio-source'
DEST = ROOT / 'Assets/Resources/Audio'
DEST.mkdir(parents=True, exist_ok=True)
FIRE = 'firearms/Prepared SFX Library/'
SPECS = [
    ('pistol', FIRE+'1911/A_42P.wav', .931, 1.35, .52, 'firearms'),
    ('shotgun', FIRE+'Mossberg/N_30P.wav', 1.686, 1.65, .62, 'firearms'),
    ('reload-start', 'gunreload.wav', 0, .34, .38, 'mechanics'),
    ('reload-finish', 'gunreload.wav', 1.18, .399, .42, 'mechanics'),
    ('shotgun-action', 'shotguncock.wav', 0, .474, .40, 'mechanics'),
    *[(f'step-{i+1:02}',f'impact/Audio/footstep_concrete_{i:03}.ogg',0,1,.48,'impact') for i in range(4)],
    ('impact-cover','impact/Audio/impactMetal_light_001.ogg',0,.6,.42,'impact'),
    ('impact-body','impact/Audio/impactPunch_medium_002.ogg',0,.6,.40,'impact'),
    ('pickup','interface/Audio/click_001.ogg',0,.5,.22,'interface'),
    ('cash','interface/Audio/confirmation_001.ogg',0,.7,.25,'interface'),
    ('alert','interface/Audio/question_002.ogg',0,.7,.27,'interface'),
]
SOURCES = {
 'firearms': {'title':'The Free Firearm Sound Library','authors':'Ben Jaszczak, Brian Nelson, Kevin Heras, Matthew Nanney','page':'https://opengameart.org/node/21826','download':'https://opengameart.org/sites/default/files/Prepared%20SFX%20Library.7z','archive':'firearms.7z'},
 'mechanics': {'title':'Gun reload sounds (recorded airsoft mechanics)','authors':'SpringySpringo','page':'https://opengameart.org/content/gun-reload-sounds','downloads':['https://opengameart.org/sites/default/files/gunreload1.wav','https://opengameart.org/sites/default/files/shotguncock_0.wav']},
 'impact': {'title':'Impact Sounds 1.0','authors':'Kenney','page':'https://kenney.nl/assets/impact-sounds','download':'https://kenney.nl/media/pages/assets/impact-sounds/87b4ddecda-1677589768/kenney_impact-sounds.zip','archive':'impact.zip'},
 'interface': {'title':'Interface Sounds 1.0','authors':'Kenney','page':'https://kenney.nl/assets/interface-sounds','download':'https://kenney.nl/media/pages/assets/interface-sounds/fa43c1dd4d-1677589452/kenney_interface-sounds.zip','archive':'interface.zip'}
}
RATE=44100
records=[]
for name, relative, start, duration, target, collection in SPECS:
    path=SOURCE/relative
    # Decode to float so stereo downmix peaks cannot clip before peak normalization.
    cutoff=5200 if collection=='firearms' else 3800 if collection=='interface' else 4200
    filters=f'highpass=f=35,lowpass=f={cutoff}:p=2,acompressor=threshold=0.18:ratio=2.5:attack=6:release=90:makeup=1'
    command=['ffmpeg','-v','error','-i',str(path),'-ac','1','-ar',str(RATE),'-af',filters,'-f','f32le','-']
    decoded=array.array('f'); decoded.frombytes(subprocess.check_output(command))
    data=decoded[int(start*RATE):int((start+duration)*RATE)]
    peak=max(abs(x) for x in data)
    assert peak>0 and all(math.isfinite(x) for x in data)
    if collection!='firearms':target*=.78
    gain=target/peak
    # A gentle attack and tail join the mix without the earlier brittle edge.
    fadein=max(1,int(.008*RATE));fadeout=max(1,int(.06*RATE))
    pcm=array.array('h',(round(x*gain*min(1,i/fadein,(len(data)-1-i)/fadeout)*32767) for i,x in enumerate(data)))
    dest=DEST/(name+'.wav')
    with wave.open(str(dest),'wb') as output:
        output.setnchannels(1);output.setsampwidth(2);output.setframerate(RATE);output.writeframes(pcm.tobytes())
    records.append({'asset':dest.name,'collection':collection,'source_file':relative,'source_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'output_sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'crop_start_seconds':start,'crop_max_seconds':duration,'seconds':len(pcm)/RATE,'target_peak':target,'measured_peak':max(abs(v) for v in pcm)/32768,'samples':len(pcm),'decode_command':['<source-file>' if value==str(path) else value for value in command]})
for collection in SOURCES.values():
    collection['license']='CC0 1.0 Universal'
    if 'archive' in collection:collection['archive_sha256']=hashlib.sha256((SOURCE/collection['archive']).read_bytes()).hexdigest()
(DEST/'provenance.json').write_text(json.dumps({'sources':SOURCES,'processing':'Tools/Prepare-Audio.py soft mix: float mono 44.1kHz, 35Hz highpass, 3.8-5.2kHz two-pole lowpass, 2.5:1 gentle compression, explicit crop, lower peak targets, 8ms attack and60ms tail fade, PCM16 output','verification':'Decoded sample structure, finite amplitudes, duration and peak measurements. No claim of audition or human mix approval.','clips':records},indent=2)+'\n',encoding='utf-8')
print('Prepared',len(records),'mono clips,',sum((DEST/x['asset']).stat().st_size for x in records),'bytes; peaks and hashes recorded.')
