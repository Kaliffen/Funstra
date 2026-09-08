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
            public float seconds, health;
            public int officers, shots, coverHits, totalAmmo;
            public Vector3 player, lastKnown;
            public string status;
            public List<PressureOfficer> roster=new List<PressureOfficer>();
        }
        [Serializable] sealed class PressureOfficer
        {
            public string id; public Vector3 position; public int ammo, magazine, pathPoints; public float reload, health;
        }
        [Serializable] sealed class PressureProfile
        {
            public string method="Rendered normal Update/AI/projectiles/traffic. A witnessed projectile attack triggers dispatch; resident movement is frozen only during that setup exchange. Extended load fixture uses 10000 player HP and disables arrest only; no enemy refills. Separate escape/defeat use 100 HP and normal arrest. Initial positions are fixtures, not human play.";
            public string gpu, cpu; public int width,height,frameCap,frames,processes;
            public float p50,p95,max; public long allocatedBytes;
            public List<PressureSample> samples=new List<PressureSample>();
        }
        PressureSample PressureSnapshot(float elapsed)
        {
            var s=new PressureSample{seconds=elapsed,health=District.health,officers=PoliceOfficerCount,shots=CombatShotCount,coverHits=CombatCoverHitCount,player=Player.position,lastKnown=Police.lastKnown,status=PoliceStatus};
            foreach(var a in Agents)if(a.Police&&a.Record!=null)
            {s.totalAmmo+=a.Record.ammo;s.roster.Add(new PressureOfficer{id=a.Record.id,health=a.Record.health,position=a.Position,ammo=a.Record.ammo,magazine=a.Record.combat.magazine,reload=a.Record.combat.reloadRemaining,pathPoints=a.Path.Count});}
            return s;
        }
        void PressureFresh(Vector3 position)
        {
            pressureDurabilityFixture=false;StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=false;freezeDistrictAI=false;yardSquad=null;autoMove=null;showMap=false;
            Teleport(position);InitializeCombat();SelectCombatWeapon(2);
        }
        void PressureEncounter(string rosterJson,Vector3 player)
        {
            PressureFresh(player);
            District.police=JsonUtility.FromJson<PoliceResponse>(rosterJson);
            foreach(var a in District.police.officers){a.health=100;a.ammo=18;a.combat=new WeaponState();a.combat.Initialize(18);}
            InitializePoliceResponse();
            // A common firing-line setup isolates the consequences of exposure versus withdrawal.
            int slot=0;
            foreach(var a in Agents)if(a.Police)
            {
                Vector3 position=new Vector3(9+(slot%3)*2.1f,0,-16+(slot/3)*2.2f);
                a.Body.position=City.Nav.SafePoint(position);a.Record.position=a.Position;a.Body.LookAt(Player.position);
                a.Record.ammo=18;a.Record.combat=new WeaponState();a.Record.combat.Initialize(18);a.FireDelay=.9f;a.Repath=0;a.Path.Clear();slot++;
            }
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
            File.WriteAllText(Path.Combine(evidencePath,"pressure-profile.json"),JsonUtility.ToJson(profile,true));
            smokeResults.Add("MEASURED: shooters="+shooters.Count+" reloaders="+reloaders.Count+" frames="+frames.Count+" p95ms="+profile.p95+" finalAmmo="+PressureSnapshot(75).totalAmmo);
            Check(rosterJson!=null&&PoliceReinforcementCount==6,"Normal live dispatch delivers all six reinforcements while combat and traffic run");
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
            screen=ScreenMode.Play;yield return new WaitForSeconds(.25f);
            yield return Travel(new Vector3(45,0,-13));
            yield return Travel(new Vector3(45,0,-39));
            yield return Travel(new Vector3(8,0,-39));
            Check(District.health>0&&screen==ScreenMode.Play,"Ordinary-health player withdraws around two corners using the production controller");
            if(District.bleeding)Check(District.BandagePlayer(),"Escape uses a carried bandage to stop actual bleeding");
            screen=ScreenMode.Tactics;showMap=true;Notify("Break sight, change streets, then wait for the search to end.");
            yield return Capture("E04-search-map-ammunition");showMap=false;
            Save();StartRun(true);yardSquad=null;screen=ScreenMode.Play;
            Check(PoliceOfficerCount==9,"Escape save/reload preserves the full responding roster");
            yield return Travel(new Vector3(-45,0,-39));
            yield return Travel(new Vector3(-45,0,-13));
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
            File.WriteAllText(Path.Combine(evidencePath,"escape-profile.json"),JsonUtility.ToJson(escapeProfile,true));
            smokeResults.Add("ESCAPE: health="+District.health+" heat="+Heat+" screen="+screen+" status="+PoliceStatus);
            Check(observedSearch&&keptHiddenKnowledge,"Lost-contact search is readable and does not refresh coordinates without an officer sighting");
            Check(Heat==0&&!Police.Searching&&District.health>0&&screen==ScreenMode.Play,"Unseen controller escape ends pursuit under normal health and arrest rules");
            screen=ScreenMode.Tactics;yield return Capture("E05-escape-complete");
            screen=ScreenMode.Play;yield return Travel(DistrictState.Clinic);
            Save();Check(District.Valid(),"Return to clinic leaves a valid persisted campaign");
            screen=ScreenMode.Tactics;yield return Capture("E06-homeward-return");
            yield return PressureCrowdingSteps();
            // Longest recovery presentation uses actual loss transactions in an explicit fixture.
            State.cash=17;State.Accept();State.carrying=true;Cargo.Take(0,State);District.TakeShipment(true);
            DistrictDefeat();
            Check(District.RecoveryDetails.Contains("Lost $17 cash")&&District.RecoveryDetails.Contains("cargo worth $60")&&District.RecoveryDetails.Contains("Lost medicine")&&District.RecoveryDetails.Contains("Mara's job item"),"Laden defeat reports each actual carried loss and preserves its recorded context");
            yield return Capture("E07-laden-recovery");
            Check(District.Valid(),"Laden defeat still conserves a valid campaign");
        }
    }
}
