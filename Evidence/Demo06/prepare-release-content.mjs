import {readFileSync,writeFileSync} from 'node:fs';
const file='site/content/site.json';
const s=JSON.parse(readFileSync(file,'utf8'));
delete s.candidate;
s.tagline='An isometric sandbox crime RPG about surviving a damaged port city. Nobody Gets Home Alone gives you three people to equip and direct, with real losses when an operation goes wrong.';
s.honesty.lede='Funstra is a free playable prototype. Demo06, Nobody Gets Home Alone, adds a crew, one contested dock operation and physical rescue. Four guided reviewer agents scored the game as it exists today at 5.3–5.7/10: the systems work, but sustained life, relationships and breadth remain thin.';
s.honesty.is=s.honesty.is.map(x=>x.replaceAll('Candidate controls:','Controls:').replaceAll('Candidate equipment controls:','Equipment controls:'));
s.honesty.isNot=s.honesty.isNot.map(x=>x.replace("Demo06's final evidence and reviewer verdicts are still pending.",'The original full eight-route candidate and four independent guided reviews are preserved; the release corrects three display expressions, with fresh Crew, Police and Legacy checks plus rendered reviewer addenda.'));
s.honesty.isNot.splice(2,0,'A guarantee that everyone comes home. Operations can fail and people can be abandoned. This build has <strong>incapacitation and persistent abandonment, with existing total-wipe recovery</strong>; irreversible death is not implemented yet.');
s.panel=[
 {who:'Marcus Webb',lens:'Technical state, value',gameNow:5.7,sliceDelivery:'8.7–9.8 provisional',evidence:89,accent:'marcus'},
 {who:'Dag Møller',lens:'Systems rigor',gameNow:5.4,sliceDelivery:'8.5–9.8 provisional',evidence:89,accent:'dag'},
 {who:'Priya Raman',lens:'Emotional, thematic',gameNow:5.4,sliceDelivery:'8.7–10.0 provisional',evidence:89,accent:'priya'},
 {who:'Nell Okafor',lens:'Is this a worthwhile hour',gameNow:5.3,sliceDelivery:'8.3–9.6 provisional',evidence:89,accent:'nell'}
];
s.panelNote='Original Demo06 protocol-v2 verdicts, unchanged after integration. Each reviewer ran a fresh guided Crew route and inspected all 17 captures and logs; the eight shared routes provide separate supporting evidence. Game now penalizes missing depth, while Slice delivery measures the promised increment. Provisional slice bounds include delivery work that was pending at review and a remaining ordinary crew livelihood coverage gap. Later display fixes and publication do not silently raise original scores. This is a new baseline, not comparable to earlier 8/10 ratings; human acceptance remains separate.';
s.panelBuild='Nobody Gets Home Alone (v0.6.0)';
s.panelSummary='Four independent guided reviews · Game now 5.3–5.7/10';
s.shots=[
 {file:'Demo06/integration-crew/C00-title.png',caption:'Release build: Nobody Gets Home Alone. A bounded crew prototype in a damaged port city.'},
 {file:'Demo06/integration-crew/C05b-physical-carry.png',caption:'Guided rescue fixture: a survivor physically carries a downed partner through the clinic doorway.'},
 {file:'Demo06/integration-crew/C07-remembered-abandonment.png',caption:'Guided abandonment fixture: the body remains where it was left, and the person remembers.'},
 {file:'Demo06/approach-final/AP-full-crew-combat-outcome.png',caption:'Original reviewed build: an actual full-crew yard attempt returns the finite component with wounds and spent ammunition. This attempt succeeded; success and survival are not guaranteed.'}
];
if(!s.previousDossiers.some(x=>x.file==='funstra-review-dossier-pressure-escape.html'))s.previousDossiers.unshift({file:'funstra-review-dossier-pressure-escape.html',output:'dossier-pressure-escape.html',label:'Pressure and Escape dossier — v0.4.2'});
s.dossierFile='funstra-review-dossier-demo06.html';
s.releaseVersion='v0.6.0';
s.links=s.links.map(x=>x.label==='Demo06 candidate manual'?{...x,label:'Play manual'}:x.label==='Published v0.4.2 guide'?{...x,label:'Earlier v0.4.2 guide'}:x);
writeFileSync(file,JSON.stringify(s,null,2)+'\n');
