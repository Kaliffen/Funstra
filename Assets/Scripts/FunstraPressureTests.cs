using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        // Only the extended load fixture changes durability; escape/defeat use ordinary rules.
        bool pressureDurabilityFixture;
        [Serializable] sealed class PressureSample
        {
            public float seconds, health, stamina, arrest, realtime,aidRemaining,aidOriginDistance;
            public int officers, shots, coverHits, totalAmmo;
            public int bandages;
            public bool bleeding,aidSight;
            public Vector3 player, lastKnown;
            public string status,screen,recovery,aidPatient,crewOrder;
            public List<PressureOfficer> roster=new List<PressureOfficer>();
        }
        [Serializable] sealed class PressureOfficer
        {
            public string id; public Vector3 position; public int kind,ammo, magazine, pathPoints; public float reload, health;
        }
        [Serializable] sealed class PressureProfile
        {
            public string method="Rendered normal Update/AI/projectiles/traffic. A witnessed projectile attack triggers dispatch; resident movement is frozen only during that setup exchange. Extended load fixture uses 10000 player HP and disables arrest only; no enemy refills. Separate escape/defeat use 100 HP and normal arrest. Initial positions are fixtures, not human play.";
            public string gpu, cpu; public int width,height,frameCap,frames,processes;
            public float p50,p95,max; public long allocatedBytes;
            public List<PressureSample> samples=new List<PressureSample>();
            public List<ProjectileHitReceipt> projectileHits=new List<ProjectileHitReceipt>();
        }
        PressureSample PressureSnapshot(float elapsed)
        {
            var s=new PressureSample{seconds=elapsed,health=District.health,officers=PoliceOfficerCount,shots=CombatShotCount,coverHits=CombatCoverHitCount,player=Player.position,lastKnown=Police.lastKnown,status=PoliceStatus,stamina=Stamina,arrest=arrestProgress,bleeding=District.bleeding,screen=screen.ToString(),recovery=District.RecoveryDetails};
            s.realtime=Time.realtimeSinceStartup;s.bandages=District.bandages;
            if(CrewEnabled&&District.crew!=null)
            {var m=District.crew.For("player");s.aidRemaining=m.aidRemaining;s.aidPatient=m.patient;s.crewOrder=m.order;s.aidOriginDistance=Vector3.Distance(Player.position,m.aidOrigin);s.aidSight=City.Nav.Sight(Player.position,Player.position);}
            foreach(var a in Agents)if(a.Police&&a.Record!=null)
            {s.totalAmmo+=a.Record.ammo;s.roster.Add(new PressureOfficer{id=a.Record.id,health=a.Record.health,position=a.Position,kind=a.Record.combat.kind,ammo=a.Record.ammo,magazine=a.Record.combat.magazine,reload=a.Record.combat.reloadRemaining,pathPoints=a.Path.Count});}
            return s;
        }
        PressureProfile pressureEscapeTrace;
        void RecordPressureEscape()
        {
            pressureEscapeTrace.samples.Add(PressureSnapshot(Elapsed));
            pressureEscapeTrace.projectileHits=new List<ProjectileHitReceipt>(ProjectileHits);
            File.WriteAllText(Path.Combine(evidencePath,"escape-movement-profile.json"),JsonUtility.ToJson(pressureEscapeTrace,true));
        }
        void RequirePressureEscapeLive()
        {
            if(screen==ScreenMode.Play&&District.health>0)return;
            autoMove=null;RecordPressureEscape();
            throw new Exception("Ordinary-health pressure escape interrupted immediately at "+Player.position+" / "+screen+" / "+District.RecoveryDetails+". See escape-movement-profile.json for quarter-second health/arrest/ammunition and projectile evidence.");
        }
        IEnumerator PressureTravel(Vector3 target)
        {
            var route=Travel(target);float next=Elapsed;
            while(true)
            {
                RequirePressureEscapeLive();
                if(Elapsed>=next){RecordPressureEscape();next=Elapsed+.25f;}
                if(!route.MoveNext())break;
                yield return route.Current;
            }
            RecordPressureEscape();
        }
        void PressureFresh(Vector3 position)
        {
            pressureDurabilityFixture=false;StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=false;freezeDistrictAI=false;yardSquad=null;autoMove=null;showMap=false;
            ProjectileHits.Clear();debugFastRunning=false;Teleport(position);
            // The crew campaign may start unarmed. This disclosed trigger fixture
            // supplies one pistol and twelve finite rounds before the encounter only.
            if(CrewEnabled&&District.arms!=null){District.arms.ownedMask|=1<<2;District.ammo=12;District.pistolWeapon=new WeaponState();}
            InitializeCombat();SelectCombatWeapon(2);
        }
        void PressureEncounter(string rosterJson,Vector3 player)
        {
            PressureFresh(player);
            District.police=JsonUtility.FromJson<PoliceResponse>(rosterJson);
            if(CrewEnabled)District.police.InitializeCrewResponse();
            foreach(var a in District.police.officers)
            {int kind=a.combat==null?2:a.combat.kind;a.health=100;a.ammo=18;a.combat=new WeaponState{kind=kind};a.combat.Initialize(18,kind);}
            InitializePoliceResponse();
            // A common firing-line setup isolates the consequences of exposure versus withdrawal.
            int slot=0;
            foreach(var a in Agents)if(a.Police)
            {
                Vector3 position=new Vector3(9+(slot%3)*2.1f,0,-16+(slot/3)*2.2f);
                a.Body.position=City.Nav.SafePoint(position);a.Record.position=a.Position;a.Body.LookAt(Player.position);
                int kind=a.Record.combat==null?2:a.Record.combat.kind;
                a.Record.ammo=18;a.Record.combat=new WeaponState{kind=kind};a.Record.combat.Initialize(18,kind);a.FireDelay=.9f;a.Repath=0;a.Path.Clear();slot++;
            }
            if(CrewEnabled)Check(Police.officers.Count==6&&Police.officers.TrueForAll(a=>a.combat.kind==5),"Prepared crew encounter retains all six real reinforcement rifles and supplies eighteen finite rounds per officer");
            District.health=100;District.bleeding=false;ReportPoliceViolence(Player.position,0,"prepared encounter / all nine deployed");
        }
        IEnumerator PressureSteps()
        {
            yield return new WaitForSeconds(1);
            string[] arguments=Environment.GetCommandLineArgs();int replay=Array.IndexOf(arguments,"--pressure-replay");
            if(replay>=0&&replay+1<arguments.Length)
            {
                smokeResults.Add("METHOD: focused encounter replay from an earlier real deployment; this is not a full stress pass.");
                yield return PressureEncounterSteps(File.ReadAllText(arguments[replay+1]));yield break;
            }
            Check(Smoke&&DistrictEnabled&&muteTests&&AudioListener.volume==0,"Pressure route uses isolated normal campaign and forced mute");
            smokeResults.Add("METHOD: extended durability stress, then separate ordinary-health exposure and controller escape; no human input/feel claim. Production Update advances all combat and arrivals.");
            if(CrewEnabled)smokeResults.Add("METHOD: crew-pressure uses the production rifle response: three base patrols plus six rifle reinforcements delivered by the existing two trucks. The triggering player kit is one pistol/twelve rounds. Exposure/escape reset each delivered officer to ordinary health and eighteen finite rounds while retaining weapon kind. No ammunition is supplied during an encounter.");
            PressureFresh(new Vector3(20,0,-13));
            pressureDurabilityFixture=true;District.health=10000;
            // Place a resident on the clear street only for the triggering exchange.
            var resident=Agents[3];resident.Body.position=new Vector3(24,0,-13);resident.Record.position=resident.Position;
            resident.Body.LookAt(Player.position);smokeFreezeAgents=true;screen=ScreenMode.Play;
            float triggerDeadline=Time.realtimeSinceStartup+6;
            while(resident.Record.health>0&&Time.realtimeSinceStartup<triggerDeadline)
            {FirePlayerAt(resident.Position);yield return null;}
            Check(resident.Record.health==0&&Police.harm>=4&&Police.trucks[1].requested,"Actual witnessed player projectiles injure a resident and request both reinforcement trucks");
            smokeFreezeAgents=false;
            var profile=new PressureProfile{gpu=SystemInfo.graphicsDeviceName,cpu=SystemInfo.processorType,width=Screen.width,height=Screen.height,frameCap=Application.targetFrameRate};
            var frames=new List<float>();var shooters=new HashSet<string>();var reloaders=new HashSet<string>();
            var previousAmmo=new Dictionary<string,int>();
            float started=Elapsed,nextSample=0;string rosterJson=null;bool arrivedCapture=false,finiteAmmo=true;
            screen=ScreenMode.Play;
            while(Elapsed-started<75)
            {
                yield return null;
                frames.Add(Time.unscaledDeltaTime*1000);
                foreach(var a in Agents)if(a.Police)
                {
                    int before=previousAmmo.TryGetValue(a.Record.id,out int old)?old:18;
                    finiteAmmo&=a.Record.ammo<=before;
                    if(a.Record.ammo<before)shooters.Add(a.Record.id);
                    if(a.Record.combat.reloadRemaining>0)reloaders.Add(a.Record.id);
                    previousAmmo[a.Record.id]=a.Record.ammo;
                }
                if(Elapsed-started>=nextSample){profile.samples.Add(PressureSnapshot(Elapsed-started));nextSample+=1;}
                if(PoliceOfficerCount==9&&rosterJson==null){rosterJson=JsonUtility.ToJson(Police);File.WriteAllText(Path.Combine(evidencePath,"deployment-snapshot.json"),rosterJson);}
                if(rosterJson!=null&&!arrivedCapture)
                {
                    arrivedCapture=true;screen=ScreenMode.Tactics;cameraSize=24;Notify("STRESS FIXTURE / extended durability for full response measurement");
                    yield return Capture("E01-nine-officer-stress");screen=ScreenMode.Play;
                }
            }
            screen=ScreenMode.Pause;pressureDurabilityFixture=false;
            frames.Sort();profile.frames=frames.Count;profile.p50=frames[frames.Count/2];profile.p95=frames[Mathf.CeilToInt(frames.Count*.95f)-1];profile.max=frames[frames.Count-1];
            profile.allocatedBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            profile.processes=System.Diagnostics.Process.GetProcessesByName("Funstra").Length;
            profile.projectileHits=new List<ProjectileHitReceipt>(ProjectileHits);
            File.WriteAllText(Path.Combine(evidencePath,"pressure-profile.json"),JsonUtility.ToJson(profile,true));
            smokeResults.Add("MEASURED: shooters="+shooters.Count+" reloaders="+reloaders.Count+" frames="+frames.Count+" p95ms="+profile.p95+" finalAmmo="+PressureSnapshot(75).totalAmmo);
            Check(rosterJson!=null&&PoliceReinforcementCount==6,"Normal live dispatch delivers all six reinforcements while combat and traffic run");
            if(CrewEnabled)
            {
                Check(Police.trucks.Length==2&&Police.trucks[0].deployed==3&&Police.trucks[1].deployed==3&&Police.officers.TrueForAll(a=>a.combat.kind==5),"The existing two trucks account for exactly six rifle reinforcements under the nine-officer ceiling");
                var rifleOwners=new HashSet<string>();foreach(var hit in ProjectileHits)if(hit.kind==5&&hit.owner.StartsWith("response-")&&hit.victim=="player")rifleOwners.Add(hit.owner);
                Check(rifleOwners.Count>0,"Live dispatched rifle reinforcement produces immutable owner/player-hit evidence");
                smokeResults.Add("MEASURED: distinct rifle reinforcement owners with actual player-hit receipts="+rifleOwners.Count+"; firing and reloading by all officers are separately counted from finite ammunition.");
            }
            Check(shooters.Count==9,"Every one of the nine officers fires through the live combat loop");
            Check(reloaders.Count>=6,"Sustained collective fire includes real finite-magazine reloads from at least six officers");
            Check(District.health<9900,"Collective projectile fire deals more than a normal player's full health in stress fixture");
            Check(finiteAmmo&&Agents.FindAll(a=>a.Police&&a.Record.ammo==0).Count>=3,"At least three officers exhaust ammunition; all nine conserve their finite supply without replenishment");
            Check(profile.processes==1,"Only this muted Funstra player runs during pressure measurement");

            yield return PressureEncounterSteps(rosterJson);
        }
        IEnumerator PressureEncounterSteps(string rosterJson)
        {
            PressureEncounter(rosterJson,new Vector3(28,0,-13));
            screen=ScreenMode.Tactics;Notify("Prepared encounter / normal health. SPACE resumes the street.");
            yield return Capture("E02-paused-contact-ammunition");
            int shotsBefore=CombatShotCount;float hp=District.health,remaining=Police.searchRemaining;
            yield return new WaitForSeconds(.5f);
            Check(District.health==hp&&CombatShotCount==shotsBefore&&Police.searchRemaining==remaining,"Tactical pause freezes full roster, damage and search while ammunition stays inspectable");
            screen=ScreenMode.Play;float deadline=Time.realtimeSinceStartup+15;
            while(screen==ScreenMode.Play&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(screen==ScreenMode.Recovery,"Exposed normal-health player is defeated by live nine-officer fire");
            Check(District.health>0&&Heat==0&&PoliceReinforcementCount==0,"Actual defeat provides recovery and clears immediate reinforcements");
            Check(District.RecoveryDetails.Contains("no cash or carried goods")&&!District.RecoveryDetails.Contains("medicine"),"Fresh street defeat describes actual losses without inventing a medical shipment");
            yield return Capture("E03-actual-defeat-recovery");

            PressureEncounter(rosterJson,new Vector3(28,0,-13));
            pressureEscapeTrace=new PressureProfile{method="Normal 100 HP, normal arrest, finite eighteen rounds per officer. Quarter-second production controller escape trace. Crew mode keeps all six reinforcement rifles, follows the fixed32,-17 /28,-22 /28,-36 /8,-39 Arcadia alley route and uses actual timed self-aid; no durability or ammunition refill after the starting fixture."};
            screen=ScreenMode.Play;yield return new WaitForSeconds(.25f);
            if(!CrewEnabled)yield return PressureTravel(new Vector3(45,0,-13));
            if(CrewEnabled)
            {
                // Short bin corner, then the real alley between Arcadia and the east
                // shop. Turn west behind Arcadia before attempting stationary aid.
                smokeResults.Add("ROUTE: fixed crew escape32,-17 ->28,-22 ->28,-36 ->8,-39; real controller follows the bin corner and Arcadia rear alley.");
                foreach(var corner in new[]{new Vector3(32,0,-17),new Vector3(28,0,-22),new Vector3(28,0,-36),new Vector3(8,0,-39)})
                {
                    Check(City.Nav.Walkable(corner),"Fixed crew escape waypoint is on shared walkable ground: "+corner);
                    yield return PressureTravel(corner);
                }
                RequirePressureEscapeLive();
                bool sheltered=!Agents.Exists(a=>a.Police&&a.Record.health>0&&City.Nav.Sight(a.Position,Player.position));
                Check(sheltered,"Fixed rear-alley escape reaches real shelter behind Arcadia");
                if(District.bleeding)
                {
                    int before=District.bandages,aidPractice=District.crew.For("player").medicinePractice,firstAidReceipt=ProjectileHits.Count;
                    float aidHealth=District.health,aidHitClock=District.clock;bool renewedContact=false;
                    Check(BeginCrewAid("player","player"),"Crew pressure escape begins real timed self-aid behind Arcadia");
                    float aidStarted=Elapsed,aidWall=Time.realtimeSinceStartup,next=Elapsed;
                    while(District.crew.For("player").aidRemaining>0&&Elapsed-aidStarted<4&&Time.realtimeSinceStartup-aidWall<20)
                    {
                        RequirePressureEscapeLive();if(Elapsed>=next){RecordPressureEscape();next=Elapsed+.25f;}
                        if(Agents.Exists(a=>a.Police&&a.Record.health>0&&a.SeesPlayer))
                        {
                            Check(OrderCrew("player","Hold"),"Renewed live contact lets the controller cancel interrupted aid");
                            renewedContact=true;break;
                        }
                        yield return null;
                    }
                    RequirePressureEscapeLive();RecordPressureEscape();
                    smokeResults.Add("AID OUTCOME: simulation="+(Elapsed-aidStarted).ToString("F3")+"s wall="+(Time.realtimeSinceStartup-aidWall).ToString("F3")+"s remaining="+District.crew.For("player").aidRemaining+" patient="+District.crew.For("player").patient+" dressings="+District.bandages+" receipts="+ProjectileHits.Count);
                    var aidState=District.crew.For("player");float aidDamage=0;bool newDirectHit=false;
                    for(int i=firstAidReceipt;i<ProjectileHits.Count;i++)
                    {
                        var hit=ProjectileHits[i];
                        if(hit.victim=="player"&&hit.owner!="player"&&hit.time>=aidHitClock&&hit.damage>0){aidDamage+=hit.damage;newDirectHit=true;}
                    }
                    bool aidIdle=aidState.aidRemaining==0&&aidState.patient=="";
                    bool completed=aidIdle&&!District.bleeding&&District.bandages==before-1&&aidState.medicinePractice==aidPractice+1&&Elapsed-aidStarted>=2.999f;
                    bool interrupted=aidIdle&&District.bleeding&&District.bandages==before&&aidState.medicinePractice==aidPractice&&(renewedContact||newDirectHit)&&District.health<=Mathf.Max(0,aidHealth-aidDamage)+.001f;
                    smokeResults.Add("AID OUTCOME: pressure "+(completed?"completed":interrupted?"interrupted":"unverified")+"; renewedContact="+renewedContact+" newDirectHit="+newDirectHit+" healthBefore="+aidHealth+" healthAfter="+District.health+" newHitDamage="+aidDamage);
                    Check(completed||interrupted,"Pressure aid ends through completed finite treatment or verified contact interruption without free healing");
                }
                Check(sheltered&&District.crew.For("player").aidRemaining==0&&District.crew.For("player").patient=="","Ordinary-health rifle escape reaches real Arcadia shelter and ends any treatment through production behavior");
            }
            else
            {
                yield return PressureTravel(new Vector3(45,0,-39));yield return PressureTravel(new Vector3(8,0,-39));
                if(District.bleeding)Check(District.BandagePlayer(),"Escape uses a carried bandage to stop actual bleeding");
            }
            Check(District.health>0&&screen==ScreenMode.Play,"Ordinary-health player withdraws through real streets and cover using the production controller");
            screen=ScreenMode.Tactics;showMap=true;Notify("Break sight, change streets, then wait for the search to end.");
            yield return Capture("E04-search-map-ammunition");showMap=false;
            Save();StartRun(true);yardSquad=null;screen=ScreenMode.Play;
            Check(PoliceOfficerCount==9,"Escape save/reload preserves the full responding roster");
            yield return PressureTravel(new Vector3(-45,0,-39));
            yield return PressureTravel(new Vector3(-45,0,-13));
            deadline=Time.realtimeSinceStartup+65;
            bool observedSearch=false,keptHiddenKnowledge=true;Vector3 remembered=Police.lastKnown;
            var escapeProfile=new PressureProfile{method="Normal-health controller escape and live search after reload."};float nextEscapeSample=0;
            while(Heat>0&&screen==ScreenMode.Play&&Time.realtimeSinceStartup<deadline)
            {
                if(Time.realtimeSinceStartup>=nextEscapeSample){escapeProfile.samples.Add(PressureSnapshot(Elapsed));nextEscapeSample=Time.realtimeSinceStartup+1;}
                bool anySight=Agents.Exists(a=>a.Police&&a.Record.health>0&&a.SeesPlayer);
                if(!Police.HasContact){observedSearch=true;if(!anySight)keptHiddenKnowledge&=Vector3.Distance(remembered,Police.lastKnown)<.01f;}
                remembered=Police.lastKnown;yield return null;
            }
            escapeProfile.projectileHits=new List<ProjectileHitReceipt>(ProjectileHits);File.WriteAllText(Path.Combine(evidencePath,"escape-profile.json"),JsonUtility.ToJson(escapeProfile,true));
            smokeResults.Add("ESCAPE: health="+District.health+" heat="+Heat+" screen="+screen+" status="+PoliceStatus);
            Check(observedSearch&&keptHiddenKnowledge,"Lost-contact search is readable and does not refresh coordinates without an officer sighting");
            Check(Heat==0&&!Police.Searching&&District.health>0&&screen==ScreenMode.Play,"Unseen controller escape ends pursuit under normal health and arrest rules");
            screen=ScreenMode.Tactics;yield return Capture("E05-escape-complete");
            screen=ScreenMode.Play;yield return PressureTravel(DistrictState.Clinic);
            Save();Check(District.Valid(),"Return to clinic leaves a valid persisted campaign");
            screen=ScreenMode.Tactics;yield return Capture("E06-homeward-return");
            if(!CrewEnabled)yield return PressureCrowdingSteps();
            else
            {
                smokeResults.Add("COVERAGE: legacy crowded-door/empty-officer-advance appendix remains in the non-crew Pressure route. Crew exhaustion intentionally withdraws; its separate guided opposition route measures depleted withdrawal and moving rifle lanes.");
                PressureFresh(Jobs.Home);
            }
            // Longest recovery presentation uses actual loss transactions in an explicit fixture.
            State.cash=17;State.Accept();State.carrying=true;Cargo.Take(0,State);District.TakeShipment(true);
            if(CrewEnabled){District.health=0;smokeResults.Add("METHOD: laden recovery presentation starts from an explicitly prepared casualty and real carried loss transactions; this is not a second combat defeat.");}
            DistrictDefeat();
            Check(District.RecoveryDetails.Contains("Lost $17 cash")&&District.RecoveryDetails.Contains("cargo worth $60")&&District.RecoveryDetails.Contains("Lost medicine")&&District.RecoveryDetails.Contains("Mara's job item"),"Laden defeat reports each actual carried loss and preserves its recorded context");
            yield return Capture("E07-laden-recovery");
            Check(District.Valid(),"Laden defeat still conserves a valid campaign");
        }
    }
}
