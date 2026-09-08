using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        [Serializable] sealed class CrewOppositionActorSample
        { public string id,order;public Vector3 position;public int ammo,magazine,dressings,used;public float health,aid;public bool seesCrew,pursuing; }
        [Serializable] sealed class CrewOppositionSample
        {
            public string stage,screen,selected,recovery,aidPatient,crewOrder;public float time,health,stamina,heat,arrest,realtime,aidRemaining,aidOriginDistance;public bool bleeding,aidSight;public int bandages;public Vector3 player;
            public List<CrewOppositionActorSample> actors=new List<CrewOppositionActorSample>();
        }
        [Serializable] sealed class CrewOppositionReceiptBatch
        { public string stage;public List<ProjectileHitReceipt> hits=new List<ProjectileHitReceipt>(); }
        [Serializable] sealed class CrewGunHandlingSample
        { public string name;public int kind,initialAmmo,remainingAmmo,pellets,magazine;public float speed,cycle,reload; }
        [Serializable] sealed class CrewOppositionEvidence
        {
            public string method="Guided exported-player fixtures, not human input. Starting positions, finite ammunition and guard casualty/report setup are explicitly staged. Live Update advances rifle officers, projectiles, ordinary 100 HP damage, controller retreat, moving firing lanes and guard field aid. No durability, arrest or ammunition-refill override. Four-family trigger/reload checks are staged production API actions. This bounded route does not claim the nine-officer stress/deployment coverage of the separate Police/Pressure suites.";
            public List<CrewGunHandlingSample> handling=new List<CrewGunHandlingSample>();
            public List<CrewOppositionSample> samples=new List<CrewOppositionSample>();
            public List<CrewOppositionReceiptBatch> receipts=new List<CrewOppositionReceiptBatch>();
        }
        CrewOppositionEvidence crewOppositionEvidence;
        void CrewOppositionRecord(string stage,bool receipts=false)
        {
            var sample=new CrewOppositionSample{stage=stage,screen=screen.ToString(),selected=ControlledCrewId,time=Elapsed,health=District.health,player=Player.position,bleeding=District.bleeding,bandages=District.bandages,stamina=Stamina,heat=Heat,arrest=arrestProgress,recovery=District.RecoveryDetails};
            var aid=District.crew.For("player");sample.realtime=Time.realtimeSinceStartup;sample.aidRemaining=aid.aidRemaining;sample.aidPatient=aid.patient;sample.crewOrder=aid.order;sample.aidOriginDistance=Vector3.Distance(Player.position,aid.aidOrigin);sample.aidSight=City.Nav.Sight(Player.position,Player.position);
            foreach(var a in Agents)if(a.Police&&a.Record!=null)
                sample.actors.Add(new CrewOppositionActorSample{id=a.Record.id,order=a.Record.order,position=a.Position,health=a.Record.health,ammo=a.Record.ammo,magazine=a.Record.combat.magazine,seesCrew=a.SeesPlayer,pursuing=a.Pursuing});
            if(yardSquad!=null)foreach(var m in yardSquad.Members)
                sample.actors.Add(new CrewOppositionActorSample{id=m.Actor.id,order=m.Order,position=m.Actor.position,health=m.Actor.health,ammo=m.Actor.ammo,magazine=m.Actor.combat.magazine,dressings=m.Data.dressings,used=m.Data.dressingsUsed,aid=m.Data.aidProgress});
            crewOppositionEvidence.samples.Add(sample);
            if(receipts)crewOppositionEvidence.receipts.Add(new CrewOppositionReceiptBatch{stage=stage,hits=new List<ProjectileHitReceipt>(ProjectileHits)});
            File.WriteAllText(Path.Combine(evidencePath,"crew-opposition-evidence.json"),JsonUtility.ToJson(crewOppositionEvidence,true));
        }
        void CrewOppositionRequireLive(string stage)
        {
            if(screen==ScreenMode.Play&&District.health>0&&ControlledCrewId=="player")return;
            autoMove=null;CrewOppositionRecord(stage+" / interrupted by "+screen,true);
            throw new Exception("Crew opposition "+stage+" stopped immediately at "+Player.position+" / "+screen+" / HP "+District.health+". Recovery: "+District.RecoveryDetails+". See quarter-second timeline and projectile receipts in crew-opposition-evidence.json.");
        }
        IEnumerator CrewOppositionTravel(Vector3 destination,string stage)
        {
            var route=Travel(destination);float nextSample=Elapsed;
            while(true)
            {
                CrewOppositionRequireLive(stage);
                if(Elapsed>=nextSample){CrewOppositionRecord(stage,true);nextSample=Elapsed+.25f;}
                if(!route.MoveNext())break;
                yield return route.Current;
            }
            CrewOppositionRecord(stage+" / arrived",true);
        }
        Vector3 CrewOppositionNearestCover(Vector3 threat)
        {
            Vector3 origin=Player.position,best=origin;float bestCost=float.MaxValue;
            // Inspect the actual city's occluders and shared routes. A close usable
            // corner is enough to treat bleeding; walking across Old Port is unnecessary.
            for(int z=-28;z<=28;z+=2)for(int x=-28;x<=28;x+=2)
            {
                Vector3 point=new Vector3(origin.x+x,0,origin.z+z);float direct=Vector3.Distance(origin,point);
                if(direct<3||direct>28||direct>=bestCost||Vector3.Distance(threat,point)<Vector3.Distance(threat,origin)||!City.Nav.Walkable(point)||City.Nav.Sight(threat,point))continue;
                var path=City.Nav.Find(origin,point);if(path.Count==0||Vector3.Distance(path[path.Count-1],point)>.5f)continue;
                float length=0;Vector3 previous=origin;foreach(var corner in path){length+=Vector3.Distance(previous,corner);previous=corner;}
                if(length<bestCost){bestCost=length;best=point;}
            }
            Check(bestCost<float.MaxValue,"Delayed retreat finds nearby reachable real cover without approaching the shooter");
            smokeResults.Add("ROUTE: nearest occluded corner "+best+" / shared-navigation distance "+bestCost.ToString("F1")+"m from "+origin);
            return best;
        }
        void CrewOppositionFresh(Vector3 position)
        {
            pressureDurabilityFixture=false;StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=false;freezeDistrictAI=true;yardSquad=null;autoMove=null;showMap=false;
            debugFastRunning=false;ProjectileHits.Clear();Teleport(position);
            // Separate the other living patrols physically, without suppressing their AI,
            // removing their health or changing the response's finite dispatch capacity.
            int slot=0;foreach(var a in Agents)if(a.Police)
            {
                Vector3 remote=City.Nav.SafePoint(new Vector3(-60+slot*3,0,52));slot++;
                a.Body.position=a.Record.position=remote;a.Route=new[]{remote,remote+Vector3.forward};
                a.Path.Clear();a.Repath=0;a.SeesPlayer=a.Pursuing=false;
            }
            InitializeCombat();
        }
        void CrewOppositionPlace(TownAgent officer,Vector3 position,int rounds)
        {
            Check(City.Nav.Walkable(position),"Prepared rifle officer stands on shared walkable street");
            officer.Body.position=officer.Record.position=position;officer.Body.LookAt(Player.position);
            officer.Record.ammo=rounds;officer.Record.combat=new WeaponState{kind=5};officer.Record.combat.Initialize(rounds,5);
            officer.FireDelay=.8f;officer.Path.Clear();officer.Repath=0;officer.Pursuing=true;
            // This temporary visual belongs to the existing actor; the shared weapon and
            // projectile services still own all handling, spending and damage.
            City.Box("Prepared rifle / finite test kit",new Vector3(.4f,.95f,.3f),new Vector3(.12f,.15f,1.1f),CityArt.Hex("303848"),officer.Body);
        }
        IEnumerator CrewOppositionSteps()
        {
            Check(CrewEnabled&&Smoke&&muteTests&&AudioListener.volume==0,"Crew opposition runs in an isolated muted crew-enabled player");
            crewOppositionEvidence=new CrewOppositionEvidence();smokeResults.Add("METHOD: "+crewOppositionEvidence.method);

            CrewOppositionFresh(new Vector3(28,0,-13));smokeFreezeAgents=true;
            District.InitializeCrewArms();District.arms.ownedMask|=(1<<2)|(1<<3)|(1<<4)|(1<<5);
            foreach(int kind in new[]{2,3,4,5})
            {
                var spec=WeaponSpec.For(kind);int initial=spec.magazine+2;
                District.SetGunAmmo(kind,initial);District.SetGunState(kind,new WeaponState{kind=kind});District.InitializeWeapons();SelectCombatWeapon(kind);
                int before=ActiveProjectileCount;
                Check(FirePlayerAt(Player.position+Vector3.right*15),"Staged "+spec.name+" uses the production trigger");
                Check(CombatTotalAmmo==initial-1&&ActiveProjectileCount-before==spec.pellets,"Staged "+spec.name+" spends one cartridge for its real projectile count");
                Check(!FirePlayerAt(Player.position+Vector3.right*15)&&CombatTotalAmmo==initial-1,"Staged "+spec.name+" cannot double-spend during the same-frame cooldown");
                var shot=District.projectiles[District.projectiles.Count-1];
                Check(shot.owner=="player"&&shot.kind==kind&&Mathf.Abs(shot.velocity.magnitude-spec.speed)<.01f,"Staged "+spec.name+" preserves the real owner, kind and projectile speed");
                Check(ReloadPlayer()&&Mathf.Abs(CurrentWeapon.reloadRemaining-spec.reload)<.001f,"Staged "+spec.name+" starts its own committed reload");
                crewOppositionEvidence.handling.Add(new CrewGunHandlingSample{name=spec.name,kind=kind,initialAmmo=initial,remainingAmmo=CombatTotalAmmo,pellets=spec.pellets,magazine=CurrentWeapon.magazine,speed=shot.velocity.magnitude,cycle=CurrentWeapon.cooldown,reload=CurrentWeapon.reloadRemaining});
                screen=ScreenMode.Play;float started=Elapsed;
                while(Elapsed-started<spec.reload+.12f)yield return null;
                screen=ScreenMode.Pause;
                Check(CurrentWeapon.reloadRemaining==0&&CurrentWeapon.magazine==spec.magazine&&CombatTotalAmmo==initial-1,"Live Update completes "+spec.name+" reload without creating reserve ammunition");
            }
            CrewOppositionRecord("four-family staged handling",true);

            // The existing east-street pressure approach, with one explicitly issued
            // three-round rifle. The player starts at ordinary campaign health and waits
            // after an actual wound before using the production movement controller.
            CrewOppositionFresh(new Vector3(28,0,-13));var rifleOfficer=Agents.Find(a=>a.Police);
            CrewOppositionPlace(rifleOfficer,new Vector3(16,0,-13),3);
            Check(City.Nav.Sight(rifleOfficer.Position,Player.position)&&City.Nav.ClearWalk(Player.position,new Vector3(45,0,-13)),"Delayed retreat has a real visible firing approach and walkable escape street");
            ReportPoliceViolence(Player.position,0,"prepared three-round rifle contact");
            Check(District.health==100&&!pressureDurabilityFixture,"Delayed rifle retreat starts at 100 HP with ordinary arrest rules");
            CrewOppositionRecord("delayed retreat / starting three-round kit");
            screen=ScreenMode.Play;float deadline=Time.realtimeSinceStartup+6;
            int lastAmmo=3;bool finite=true;
            while(District.health==100&&screen==ScreenMode.Play&&Time.realtimeSinceStartup<deadline)
            {yield return null;finite&=rifleOfficer.Record.ammo<=lastAmmo;lastAmmo=rifleOfficer.Record.ammo;}
            Check(ProjectileHits.Exists(h=>h.owner==rifleOfficer.Record.id&&h.victim=="player"&&h.kind==5)&&District.health<100,"Live rifle officer spends a round and produces an immutable player-hit receipt");
            float delayStarted=Elapsed;while(Elapsed-delayStarted<.8f&&screen==ScreenMode.Play)yield return null;
            CrewOppositionRecord("delayed retreat / wounded after 0.8 second hesitation",true);
            screen=ScreenMode.Tactics;cameraSize=19;SnapCamera();Notify("GUIDED FIXTURE / rifle wound, three finite rounds / retreat begins after hesitation");
            yield return Capture("C20-rifle-contact-before-retreat");screen=ScreenMode.Play;
            Vector3 cover= CrewOppositionNearestCover(rifleOfficer.Position);
            yield return CrewOppositionTravel(cover,"delayed retreat / controller to nearest real cover");
            Check(District.health>0&&District.health<100&&screen==ScreenMode.Play,"Wounded ordinary-health protagonist survives delayed controller withdrawal around the corner");
            Check(finite&&rifleOfficer.Record.ammo<=lastAmmo&&rifleOfficer.Record.ammo>=0,"Rifle pressure preserves its finite three-round starting kit during retreat");
            Check(!rifleOfficer.SeesPlayer&&!City.Nav.Sight(rifleOfficer.Position,Player.position),"The retreat ends behind real occluding geometry");
            if(District.bleeding)
            {
                int bandages=District.bandages,aidPractice=District.crew.For("player").medicinePractice,firstAidReceipt=ProjectileHits.Count;
                float aidHealth=District.health,aidHitClock=District.clock;
                Check(BeginCrewAid("player","player"),"Player begins actual timed self-aid after reaching cover");
                float aidStarted=Elapsed,aidWall=Time.realtimeSinceStartup,nextAidSample=Elapsed;
                while(District.crew.For("player").aidRemaining>0&&Elapsed-aidStarted<4&&Time.realtimeSinceStartup-aidWall<20)
                {
                    CrewOppositionRequireLive("timed aid after rifle retreat");
                    if(Elapsed>=nextAidSample){CrewOppositionRecord("delayed retreat / timed self-aid",true);nextAidSample=Elapsed+.25f;}
                    yield return null;
                }
                CrewOppositionRequireLive("ended aid after rifle retreat");
                CrewOppositionRecord("delayed retreat / final self-aid state",true);
                smokeResults.Add("AID OUTCOME: simulation="+(Elapsed-aidStarted).ToString("F3")+"s wall="+(Time.realtimeSinceStartup-aidWall).ToString("F3")+"s remaining="+District.crew.For("player").aidRemaining+" patient="+District.crew.For("player").patient+" dressings="+District.bandages+" receipts="+ProjectileHits.Count);
                var aidState=District.crew.For("player");float aidDamage=0;bool newOfficerHit=false;
                for(int i=firstAidReceipt;i<ProjectileHits.Count;i++)
                {
                    var hit=ProjectileHits[i];
                    if(hit.victim=="player"&&hit.time>=aidHitClock&&hit.damage>0)
                    {aidDamage+=hit.damage;newOfficerHit|=hit.owner==rifleOfficer.Record.id&&hit.kind==5;}
                }
                bool aidIdle=aidState.aidRemaining==0&&aidState.patient=="";
                bool completed=aidIdle&&!District.bleeding&&District.bandages==bandages-1&&aidState.medicinePractice==aidPractice+1&&Elapsed-aidStarted>=2.999f;
                bool interrupted=aidIdle&&District.bleeding&&District.bandages==bandages&&aidState.medicinePractice==aidPractice&&newOfficerHit&&District.health<=Mathf.Max(0,aidHealth-aidDamage)+.001f;
                smokeResults.Add("AID OUTCOME: post-retreat "+(completed?"completed":interrupted?"direct-hit interruption":"unverified")+"; healthBefore="+aidHealth+" healthAfter="+District.health+" newHitDamage="+aidDamage+" newOfficerHit="+newOfficerHit);
                Check(completed||interrupted,"Post-retreat timed aid records completed finite treatment or verified direct-hit interruption without healing or dressing loss");
            }
            CrewOppositionRecord("delayed retreat / around the corner",true);

            // An explicitly depleted starting kit isolates movement after exhaustion.
            CrewOppositionFresh(new Vector3(28,0,-13));var empty=Agents.Find(a=>a.Police);
            CrewOppositionPlace(empty,new Vector3(20,0,-13),0);ReportPoliceViolence(Player.position,0,"prepared depleted rifle officer");
            Vector3 emptyStart=empty.Position,away=(empty.Position-Player.position).normalized;int emptyShots=CombatShotCount;
            screen=ScreenMode.Play;yield return Travel(new Vector3(24,0,-13));
            float observe=Elapsed;while(Elapsed-observe<1.2f)yield return null;
            Check(Vector3.Dot(empty.Position-emptyStart,away)>1&&City.Nav.Walkable(empty.Position),"Depleted officer withdraws on shared walkable ground while the protagonist approaches");
            Check(empty.Record.ammo==0&&empty.Record.combat.magazine==0&&CombatShotCount==emptyShots,"Depleted rifle officer neither refills ammunition nor fires a free shot");
            CrewOppositionRecord("depleted officer / player approach",true);

            // Two ordinary-health patrol actors, each issued one round. The rear actor
            // begins with its colleague in the actual swept firing lane. Both actors'
            // movement and fire are driven by Update rather than an explicit stepping loop.
            CrewOppositionFresh(new Vector3(32,0,-13));var officers=Agents.FindAll(a=>a.Police);
            var rear=officers[0];var front=officers[1];
            CrewOppositionPlace(rear,new Vector3(18,0,-13),1);CrewOppositionPlace(front,new Vector3(23,0,-13),1);
            ReportPoliceViolence(Player.position,0,"prepared moving rifle lane / one round each");
            Vector3 rearStart=rear.Position,frontStart=front.Position;float side=0,minGap=Vector3.Distance(rear.Position,front.Position);
            bool walked=true,finiteLane=true;int rearAmmo=1,frontAmmo=1;
            CrewOppositionRecord("moving lane / initial blocked formation");
            screen=ScreenMode.Play;deadline=Time.realtimeSinceStartup+8;
            while(Time.realtimeSinceStartup<deadline&&screen==ScreenMode.Play&&(rear.Record.ammo>0||front.Record.ammo>0||ActiveProjectileCount>0))
            {
                yield return null;side=Mathf.Max(side,Mathf.Abs(rear.Position.z-rearStart.z));
                minGap=Mathf.Min(minGap,Vector3.Distance(rear.Position,front.Position));walked&=City.Nav.Walkable(rear.Position)&&City.Nav.Walkable(front.Position);
                finiteLane&=rear.Record.ammo<=rearAmmo&&front.Record.ammo<=frontAmmo;rearAmmo=rear.Record.ammo;frontAmmo=front.Record.ammo;
            }
            CrewOppositionRecord("moving lane / live outcome",true);
            Check(side>.35f&&Vector3.Distance(front.Position,frontStart)>.1f&&walked&&minGap>=.77f,"Live rifle formation opens a blocked lane through officer movement while retaining walkability and separation");
            Check(finiteLane&&rear.Record.ammo==0&&front.Record.ammo==0,"Both moving-lane officers spend only their single issued round");
            Check(ProjectileHits.Exists(h=>h.owner==rear.Record.id&&h.victim=="player"&&h.kind==5)&&ProjectileHits.Exists(h=>h.owner==front.Record.id&&h.victim=="player"&&h.kind==5),"Moving firing lane preserves both distinct rifle owners and the actual struck crew identity");
            Check(!ProjectileHits.Exists(h=>h.victim==rear.Record.id||h.victim==front.Record.id)&&District.health>0&&District.health<100,"Officers clear the friendly firing lane without shooting one another or requiring extra player durability");
            screen=ScreenMode.Tactics;cameraSize=19;SnapCamera();Notify("GUIDED FIXTURE / moving rifle lanes / one real round per officer");yield return Capture("C21-moving-rifle-lane");

            // A staged casualty makes the recovery behavior observable without calling it
            // a player-won fight. Production squad Update cancels the pending report and
            // performs physical, interrupted-time-aware treatment with finite dressings.
            CrewOppositionFresh(new Vector3(36,0,-43));InitializeYard();
            var vale=yardSquad.Members.Find(m=>m.Data.role==CombatRole.Anchor);
            var aidGuard=yardSquad.Members.Find(m=>m.Data.role==CombatRole.Flank);
            var distant=yardSquad.Members.Find(m=>m.Data.role==CombatRole.Support);
            Vector3 aidPoint=City.Nav.SafePoint(new Vector3(28,0,-13));
            foreach(var m in yardSquad.Members){m.Data.observedAt=-100;m.Data.reportDue=-1;m.Data.withdrawing=false;m.Data.aidProgress=0;m.Data.aidTarget="";}
            vale.Actor.position=vale.Body.position=aidPoint;vale.Actor.health=0;
            aidGuard.Actor.position=aidGuard.Body.position=aidPoint+Vector3.right*1.5f;
            distant.Actor.position=distant.Body.position=City.Nav.SafePoint(aidPoint+Vector3.left*14);
            foreach(var m in yardSquad.Members){m.Data.home=m.Actor.position;m.Data.retreat=City.Nav.SafePoint(m.Actor.position+Vector3.back*7);m.Goal=m.Actor.position;m.Body.rotation=Quaternion.Euler(0,0,0);}
            // The protagonist begins outside the guards' sight range; no other crew
            // actor is recruited by this fresh casualty fixture.
            District.squad.leaderLost=false;
            distant.Data.reportDue=District.squad.clock+.5f;distant.Data.reportObservedAt=District.squad.clock;distant.Data.reportPosition=Player.position;
            int supplied=aidGuard.Data.dressings;CrewOppositionRecord("guard aid / staged leader casualty and queued report");
            screen=ScreenMode.Play;float aidStart=Elapsed;
            while(Elapsed-aidStart<.25f)yield return null;
            Check(District.squad.leaderLost&&distant.Data.reportDue<0&&aidGuard.Data.observedAt<0,"Live leader loss cancels a surviving guard's already pending report");
            Check(aidGuard.Data.aidProgress>0&&vale.Actor.health==0&&aidGuard.Data.dressings==supplied,"Live guard starts timed physical aid before consuming its finite dressing");
            deadline=Time.realtimeSinceStartup+5;while(vale.Actor.health==0&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(vale.Actor.health==25&&aidGuard.Data.dressings==supplied-1&&aidGuard.Data.dressingsUsed==1,"Live guard completes one dressing and restores only 25 health to the casualty");
            Check(vale.Data.withdrawing&&District.squad.leaderLost,"Recovered leader withdraws and cannot restore the lost report network");
            screen=ScreenMode.Tactics;Hidden=false;Teleport(aidPoint+new Vector3(7,0,-8));cameraSize=18;SnapCamera();Notify("GUIDED CASUALTY FIXTURE / one dressing used / Vale withdraws at 25 HP");
            CrewOppositionRecord("guard aid / finite completed recovery",true);yield return Capture("C22-guard-dressing-withdrawal");
            smokeResults.Add("COVERAGE: bounded live rifle pressure, delayed ordinary-health controller retreat, depleted movement, moving-lane owner/victim receipts, four staged gun families and live guard report/aid. Full nine-officer dispatch/stress remains separate Police/Pressure evidence.");
            screen=ScreenMode.Pause;
        }
    }
}
