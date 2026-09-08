using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        [Serializable] sealed class CrewApproachActor
        { public string id,order;public Vector3 position;public float health;public int ammo,magazine;public bool sight,withdrawing; }
        [Serializable] sealed class CrewApproachSample
        {
            public string stage,selected,componentOwner,acquisition;public float clock;public bool witnessed,hostile;
            public List<CrewApproachActor> actors=new List<CrewApproachActor>();
            public List<ProjectileHitReceipt> hits=new List<ProjectileHitReceipt>();
        }
        [Serializable] sealed class CrewApproachEvidence
        { public string method;public List<CrewApproachSample> samples=new List<CrewApproachSample>();public List<CrewApproachSightFrame> sightTrace=new List<CrewApproachSightFrame>(); }
        [Serializable] sealed class CrewApproachSightActor
        {
            public string id,order;public Vector3 position,forward,previousForward,visibleTarget,lastContact;
            public float observedAt,contactAge,distanceToSelected,closestDistanceToSelected,coneDot;
            public bool directSight,clearSightToSelected;
        }
        [Serializable] sealed class CrewApproachSightFrame
        {
            public string reason,selected;public float clock;public Vector3 position,destination,waypoint;
            public bool hasWaypoint,engaged;public List<CrewApproachSightActor> guards=new List<CrewApproachSightActor>();
        }
        CrewApproachEvidence crewApproachEvidence;
        readonly Dictionary<string,Vector3> approachPriorForward=new Dictionary<string,Vector3>();
        readonly Dictionary<string,float> approachClosestDistance=new Dictionary<string,float>();
        readonly HashSet<string> approachFirstSight=new HashSet<string>(),approachFirstContact=new HashSet<string>();
        bool approachPriorEngaged;
        float approachNextTrace;
        void TraceApproachSight(Vector3 destination,string reason="")
        {
            if(yardSquad==null)return;
            bool engaged=false,near=false;foreach(var p in LivingCrewPositions)if(DockYardEngaged(p))engaged=true;
            if(engaged&&!approachPriorEngaged)reason+=" FIRST TRESPASS ENGAGEMENT;";
            approachPriorEngaged=engaged;
            foreach(var m in yardSquad.Members)
            {
                float d=Vector3.Distance(ControlledPosition,m.Actor.position);near|=d<35;
                if(!approachClosestDistance.ContainsKey(m.Actor.id)||d<approachClosestDistance[m.Actor.id])approachClosestDistance[m.Actor.id]=d;
                if(m.DirectSight&&approachFirstSight.Add(m.Actor.id))reason+=" FIRST DIRECT SIGHT "+m.Actor.id+";";
                if(m.ContactAge<CombatSquad.ContactLifetime&&m.Data.observedAt>=0&&approachFirstContact.Add(m.Actor.id))reason+=" FIRST CONTACT "+m.Actor.id+";";
            }
            bool significant=reason!="";
            if(significant||near&&District.clock>=approachNextTrace)
            {
                approachNextTrace=District.clock+.25f;
                var frame=new CrewApproachSightFrame{reason=significant?reason:"near-watch sample",selected=ControlledCrewId,clock=District.clock,position=ControlledPosition,destination=destination,hasWaypoint=autoMove.HasValue,waypoint=autoMove??Vector3.zero,engaged=engaged};
                foreach(var m in yardSquad.Members)
                {
                    Vector3 forward=m.Body?m.Body.forward:Vector3.zero,delta=ControlledPosition-m.Actor.position;delta.y=0;
                    frame.guards.Add(new CrewApproachSightActor{id=m.Actor.id,order=m.Order,position=m.Actor.position,forward=forward,previousForward=approachPriorForward.ContainsKey(m.Actor.id)?approachPriorForward[m.Actor.id]:forward,visibleTarget=m.VisibleTarget,lastContact=m.Data.lastContact,observedAt=m.Data.observedAt,contactAge=m.ContactAge,distanceToSelected=delta.magnitude,closestDistanceToSelected=approachClosestDistance[m.Actor.id],coneDot=Vector3.Dot(forward,delta.normalized),directSight=m.DirectSight,clearSightToSelected=City.Nav.Sight(m.Actor.position,ControlledPosition)});
                }
                crewApproachEvidence.sightTrace.Add(frame);
                if(significant)File.WriteAllText(Path.Combine(evidencePath,"crew-approach-evidence.json"),JsonUtility.ToJson(crewApproachEvidence,true));
            }
            foreach(var m in yardSquad.Members)if(m.Body)approachPriorForward[m.Actor.id]=m.Body.forward;
        }
        void RecordCrewApproach(string stage)
        {
            var s=new CrewApproachSample{stage=stage,selected=ControlledCrewId,componentOwner=District.dock.componentOwner,acquisition=District.dock.acquisition,clock=District.clock,witnessed=District.dock.theftWitnessed,hostile=District.dock.yardHostile};
            foreach(string id in new[]{"player","neri","rell"})
                s.actors.Add(new CrewApproachActor{id=id,order=District.crew.For(id).order,position=CrewPosition(id),health=CrewHealth(id),ammo=id=="player"?CombatTotalAmmo:CrewActor(id).ammo,magazine=id=="player"?CurrentWeapon.magazine:CrewActor(id).combat.magazine});
            if(yardSquad!=null)foreach(var m in yardSquad.Members)
                s.actors.Add(new CrewApproachActor{id=m.Actor.id,order=m.Order,position=m.Actor.position,health=m.Actor.health,ammo=m.Actor.ammo,magazine=m.Actor.combat.magazine,sight=m.DirectSight,withdrawing=m.Data.withdrawing});
            foreach(var a in Agents)if(a.Police&&a.Record!=null)
                s.actors.Add(new CrewApproachActor{id=a.Record.id,order=a.Pursuing?"Pursuing":a.Record.order,position=a.Position,health=a.Record.health,ammo=a.Record.ammo,magazine=a.Record.combat.magazine,sight=a.SeesPlayer,withdrawing=a.Record.ammo==0||a.Record.health<30});
            s.hits=new List<ProjectileHitReceipt>(ProjectileHits);crewApproachEvidence.samples.Add(s);
            File.WriteAllText(Path.Combine(evidencePath,"crew-approach-evidence.json"),JsonUtility.ToJson(crewApproachEvidence,true));
            Save();File.WriteAllText(Path.Combine(evidencePath,"crew-approach-latest-state.json"),JsonUtility.ToJson(State,true));
            string navigation=stage;
            foreach(string id in new[]{"player","neri","rell"})
            {
                var p=CrewPosition(id);var path=crewPaths[id];navigation+="\n"+id+" at="+p+" walkable="+City.Nav.Walkable(p)+" path="+string.Join(", ",path);
                if(path.Count>0){var next=p+(path[0]-p).normalized*.15f;navigation+=" next="+next+" ground="+City.Nav.ClearWalk(p,next)+" clear="+CrewMoveClear(id,next);}
            }
            foreach(var b in City.Nav.Traffic)navigation+="\ntraffic center="+b.center+" size="+b.size;
            File.WriteAllText(Path.Combine(evidencePath,"crew-navigation-diagnostic.txt"),navigation);
        }
        void ApproachCheck(bool ok,string text)
        {
            if(!ok)RecordCrewApproach("FAILED / "+text);
            Check(ok,text);
        }
        void FreshCrewApproach()
        {
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;
            smokeFreezeAgents=false;freezeDistrictAI=false;pressureDurabilityFixture=false;debugFastRunning=false;
            autoMove=null;autoInteract=false;showMap=false;crewPanel=false;CloseDockDialog();ProjectileHits.Clear();
            approachPriorForward.Clear();approachClosestDistance.Clear();approachFirstSight.Clear();approachFirstContact.Clear();approachPriorEngaged=false;approachNextTrace=0;
            ApproachCheck(yardSquad!=null&&yardSquad.Members.Count==3&&District.health==100,"Fresh live yard has three ordinary guards and ordinary protagonist health");
            TraceApproachSight(ControlledPosition,"fresh approach / initial facing");
        }
        IEnumerator ApproachTravel(Vector3 destination)
        {
            var walk=CrewTravel(destination);
            while(true)
            {
                TraceApproachSight(destination);
                bool next;
                try{next=walk.MoveNext();}
                catch(Exception){RecordCrewApproach("FAILED / controller route to "+destination);throw;}
                if(!next)break;
                yield return walk.Current;
            }
        }
        IEnumerator ApproachCrewAt(string id,Vector3 point,float tolerance=1)
        {
            float deadline=Time.realtimeSinceStartup+55;
            while(Vector3.Distance(CrewPosition(id),point)>tolerance)
            {
                TraceApproachSight(point);
                if(!CrewAlive(id))ApproachCheck(false,id+" fell while following actual movement orders");
                if(Time.realtimeSinceStartup>deadline){ApproachCheck(false,id+" order blocked at "+CrewPosition(id)+" toward "+point);yield break;}
                yield return null;
            }
        }
        IEnumerator ApproachHoldComponent(string collector,bool requireUnseen)
        {
            float deadline=Time.realtimeSinceStartup+9;autoInteract=true;
            while(District.dock.componentOwner=="yard")
            {
                TraceApproachSight(DockOperationState.ComponentPost);
                if(ControlledCrewId!=collector||!CrewAlive(collector))ApproachCheck(false,"Component collector no longer remains the selected living actor");
                if(requireUnseen&&(DockWitness()||District.dock.theftWitnessed))ApproachCheck(false,"Actual live watch detected the theft interaction");
                if(!requireUnseen)ApproachFireResponse();
                if(Time.realtimeSinceStartup>deadline){autoInteract=false;ApproachCheck(false,"Finite pump hold did not complete: "+prompt);yield break;}
                yield return null;
            }
            autoInteract=false;
            ApproachCheck(District.dock.componentOwner==collector,"Actual held pickup gives sole component to "+collector);
        }
        IEnumerator ApproachNorthExit()
        {
            yield return ApproachTravel(new Vector3(30,0,48));yield return ApproachTravel(new Vector3(-8,0,48));
            yield return ApproachTravel(new Vector3(-8,0,12));yield return ApproachTravel(new Vector3(52,0,12));
            yield return ApproachTravel(DockOperationState.Workshop+Vector3.back*1.5f);
        }
        IEnumerator CrewApproachSteps()
        {
            crewApproachEvidence=new CrewApproachEvidence{method="Muted exported normal campaign. Live yard, citizens, police, traffic and crew movement. No hidden flags, relocated guards, health boosts, ammunition refill or deleted opponents. Solo and Neri approaches use actual selected controller behind initial south-facing watch. Full crew uses explicitly staged recruitment and $800, finite store purchases/transfers, movement orders, Engage and guided protagonist trigger calls; equipment remains finite. Menu pauses are ordinary tactical pause. No human play claim."};
            smokeResults.Add("METHOD: "+crewApproachEvidence.method);
            yield return new WaitForSeconds(1);
            Check(crewTest&&CrewEnabled&&muteTests&&AudioListener.volume==0,"Independent crew-approach runner is enabled and muted");
            bool fightOnly=Array.IndexOf(Environment.GetCommandLineArgs(),"--crew-fight")>=0;
            if(fightOnly)smokeResults.Add("METHOD: --crew-fight is a focused replay of the full-crew branch. Solo and Neri theft branches are deliberately not executed or credited by this run.");
            else {yield return CrewUnseenApproach(false);yield return CrewUnseenApproach(true);}
            yield return CrewLiveFightApproach();
            File.WriteAllText(Path.Combine(evidencePath,"crew-approach-method.txt"),crewApproachEvidence.method);
        }
        IEnumerator CrewUnseenApproach(bool withNeri)
        {
            FreshCrewApproach();string route=withNeri?"neri-pair":"solo",collector=withNeri?"neri":"player";
            smokeResults.Add("METHOD: unseen approach follows x=-8 west of Vale's 28m rifle sight radius, then crosses behind the watch at z=48. z=46 arrival tolerance could enter the yard with remembered central-street contact; z=50 intersects the north garage. No contacts or sight flags are reset. Every-frame first sight/contact transitions and nearby quarter-second samples record facing and actual controller waypoint.");
            if(withNeri)
            {
                smokeResults.Add("METHOD: Neri pair recruitment is explicitly staged through finite medicine TakeShipment/Donate/Recruit APIs; Neri's actual controller collects while protagonist holds on the outer street. No medicine retrieval journey is claimed.");
                ApproachCheck(District.TakeShipment(false)&&District.Donate()&&District.Recruit(),"Neri pair fixture earns existing partnership through finite donation");
                ApproachCheck(OrderCrew("neri","Move",new Vector3(-10,0,12)),"Neri receives actual west-spine movement order outside rifle range");
            }
            yield return ApproachTravel(new Vector3(-8,0,12));
            if(withNeri)
            {
                yield return ApproachCrewAt("neri",new Vector3(-10,0,12));
                OrderCrew("neri","Move",new Vector3(-10,0,48));
            }
            yield return ApproachTravel(new Vector3(-8,0,48));
            if(withNeri)
            {
                yield return ApproachCrewAt("neri",new Vector3(-10,0,48));
                // Keep the protagonist off both Neri's eastbound lane and her later west-spine exit.
                yield return ApproachTravel(new Vector3(-8,0,45));yield return ApproachTravel(new Vector3(-12,0,45));
                OrderCrew("neri","Move",new Vector3(5,0,48));yield return ApproachCrewAt("neri",new Vector3(5,0,48));
                ApproachCheck(SelectCrew("neri")&&OrderCrew("player","Hold"),"Neri takes actual control while protagonist holds north street");
            }
            int yardAmmo=0;foreach(var m in yardSquad.Members)yardAmmo+=m.Actor.ammo;
            float health=CrewHealth(collector);Vector3 partner=withNeri?Player.position:Vector3.zero;
            yield return ApproachTravel(new Vector3(30,0,48));yield return ApproachTravel(DockOperationState.ComponentPost);
            RecordCrewApproach(route+"-behind-watch");
            ApproachCheck(!DockWitness()&&!District.dock.theftWitnessed,"North loading bay provides a real unseen pickup position for "+route);
            screen=ScreenMode.Tactics;yield return Capture("AP-"+route+"-behind-watch");screen=ScreenMode.Play;
            yield return ApproachHoldComponent(collector,true);
            ApproachCheck(!District.dock.released&&!District.dock.theftWitnessed&&!District.dock.yardHostile&&District.dock.acquisition=="theft","Unpaid "+route+" pickup remains unobserved through actual hold duration");
            yield return ApproachNorthExit();
            int after=0;foreach(var m in yardSquad.Members)after+=m.Actor.ammo;
            ApproachCheck(after==yardAmmo&&CrewHealth(collector)==health,"Unobserved "+route+" extraction provokes no yard gunfire or collector injury");
            if(withNeri)ApproachCheck(Vector3.Distance(Player.position,partner)<.2f,"Neri's completed controller journey did not move the held protagonist");
            ApproachCheck(UpdateDockInteraction(.05f,true,false)&&District.dock.stakeResolved&&District.dock.componentOwner=="rell-workshop","Unseen "+route+" controller returns the same component to live Rell");
            OrderCrew("rell","Hold");RecordCrewApproach(route+"-returned");
            screen=ScreenMode.Tactics;yield return Capture("AP-"+route+"-returned");
            Save();StartRun(true);screen=ScreenMode.Tactics;
            ApproachCheck(District.dock.stakeResolved&&!District.dock.theftWitnessed&&District.dock.componentOwner=="rell-workshop"&&District.Valid(),"Unseen "+route+" outcome persists without component respawn");
        }
        bool ApproachClearCrewFireLane(Vector3 target)
        {
            Vector3 origin=Player.position+Vector3.up*1.1f;target.y=origin.y;
            Vector3 direction=target-origin;float length=direction.magnitude;if(length<.1f)return false;direction/=length;
            // Preserve actual friendly collision. Reject the entire possible SMG spread/recoil
            // corridor before issuing the guided trigger, not only the center aiming ray.
            float cone=Mathf.Tan((WeaponSpec.SMG.spread+recoil)*Mathf.Deg2Rad);
            foreach(string id in new[]{"neri","rell"})
            {
                if(!CrewAlive(id))continue;
                Vector3 offset=CrewPosition(id)+Vector3.up*1.1f-origin;float along=Vector3.Dot(offset,direction);
                if(along<=0||along>=length)continue;
                float clearance=(offset-direction*along).magnitude;
                if(clearance<.44f+ProjectileMath.Radius+.15f+cone*along)return false;
            }
            return true;
        }
        bool ApproachArmedResponse()
        {
            foreach(var a in Agents)if(a.Police&&a.Pursuing&&a.Record!=null&&a.Record.health>0&&a.Record.ammo>0)
                foreach(var p in LivingCrewPositions)if(Vector3.Distance(p,a.Position)<WeaponSpec.For(a.Record.combat.kind).range&&City.Nav.Sight(p,a.Position))return true;
            if(yardSquad!=null)foreach(var m in yardSquad.Members)if(m.Actor.health>0&&m.Actor.ammo>0&&!m.Data.withdrawing)
                foreach(var p in LivingCrewPositions)if(Vector3.Distance(p,m.Actor.position)<WeaponSpec.For(m.Data.gun).range&&City.Nav.Sight(p,m.Actor.position))return true;
            return District.projectiles.Exists(p=>!IsCrewId(p.owner));
        }
        void ApproachFireResponse()
        {
            if(ControlledCrewId!="player"||!CrewAlive("player"))return;
            SelectCombatWeapon(4);DistrictActor target=null;float nearest=WeaponSpec.SMG.range;
            foreach(var a in Agents)if(a.Police&&a.Pursuing&&a.Record!=null&&a.Record.health>0&&a.Record.ammo>0&&City.Nav.Sight(Player.position,a.Position)&&ApproachClearCrewFireLane(a.Position))
            {float distance=Vector3.Distance(Player.position,a.Position);if(distance<nearest){nearest=distance;target=a.Record;}}
            if(CurrentWeapon.magazine==0&&District.smgAmmo>0&&CurrentWeapon.reloadRemaining==0)ReloadPlayer();
            if(target!=null&&CurrentWeapon.cooldown==0&&CurrentWeapon.reloadRemaining==0&&CurrentWeapon.magazine>0)FirePlayerAt(target.position);
        }
        bool approachTotalDefeat;
        readonly HashSet<string> approachFallen=new HashSet<string>();
        bool ApproachSurvivor()
        {
            foreach(string id in new[]{"player","neri","rell"})if(!CrewAlive(id)&&approachFallen.Add(id))
                RecordCrewApproach("full-crew-incapacitated / "+id+" left at "+CrewPosition(id));
            if(screen==ScreenMode.Recovery||!CrewAlive("player")&&!CrewAlive("neri")&&!CrewAlive("rell"))
            {approachTotalDefeat=true;autoMove=null;autoInteract=false;return false;}
            if(!CrewAlive(ControlledCrewId))foreach(string id in new[]{"player","neri","rell"})if(CrewAlive(id)&&SelectCrew(id))break;
            return CrewAlive(ControlledCrewId);
        }
        void ApproachFollowSurvivor(bool cover=false)
        {
            foreach(string id in new[]{"player","neri","rell"})if(id!=ControlledCrewId&&CrewAlive(id))OrderCrew(id,cover?"Engage":"Follow");
            if(ControlledCrewId=="player"&&!cover)SelectCombatWeapon(1);
        }
        IEnumerator ApproachEscortedTravel(Vector3 destination,bool partnersCover=false)
        {
            if(!ApproachSurvivor())yield break;
            string selected=ControlledCrewId;ApproachFollowSurvivor(partnersCover);
            var travel=ApproachTravel(destination);
            while(ApproachSurvivor())
            {
                // Selection is a real actor change. Replan the controller journey from
                // that survivor's actual body; retain every fallen body and its custody.
                if(selected!=ControlledCrewId)
                {selected=ControlledCrewId;ApproachFollowSurvivor(partnersCover);travel=ApproachTravel(destination);RecordCrewApproach("full-crew-continued-as / "+selected);}
                if(!travel.MoveNext())break;
                yield return travel.Current;
            }
            autoMove=null;
        }
        IEnumerator ApproachFightPickup()
        {
            if(!ApproachSurvivor())yield break;
            string collector=ControlledCrewId;
            if(District.dock.componentOwner=="yard"&&DockNear(DockOperationState.ComponentPost,2.4f))
            {
                float until=District.clock+5,wall=Time.realtimeSinceStartup+25;autoInteract=true;
                while(District.dock.componentOwner=="yard"&&CrewAlive(collector)&&ControlledCrewId==collector&&ApproachSurvivor()&&District.clock<until)
                {
                    if(Time.realtimeSinceStartup>wall){autoInteract=false;ApproachCheck(false,"Live pickup simulation failed to advance");yield break;}
                    yield return null;
                }
                autoInteract=false;
            }
            if(!ApproachSurvivor())yield break;
            string owner=District.dock.componentOwner;
            if(IsCrewId(owner)&&owner!=ControlledCrewId&&DockNear(CrewPosition(owner),2.5f))
            {
                UpdateDockInteraction(.05f,true,false);
                RecordCrewApproach("full-crew-actual-nearby-component-handoff / "+owner+" to "+District.dock.componentOwner);
            }
            RecordCrewApproach("full-crew-pickup-attempt / "+District.dock.componentOwner);
        }
        bool ApproachSafeForAid(string id)
        {
            if(District.projectiles.Exists(p=>!IsCrewId(p.owner)))return false;
            foreach(var a in Agents)if(a.Police&&a.Pursuing&&a.Record!=null&&a.Record.health>0&&City.Nav.Sight(CrewPosition(id),a.Position))return false;
            if(yardSquad!=null)foreach(var m in yardSquad.Members)if(m.Actor.health>0&&City.Nav.Sight(CrewPosition(id),m.Actor.position))return false;
            return true;
        }
        IEnumerator ApproachTreatInCover()
        {
            bool started=false;
            foreach(string id in new[]{"player","neri","rell"})if(CrewAlive(id)&&CrewBleeding(id)&&CrewBandages(id)>0&&ApproachSafeForAid(id))started|=BeginCrewAid(id,id);
            if(!started)yield break;
            float until=District.clock+3.2f,wall=Time.realtimeSinceStartup+20;
            while(District.clock<until)
            {
                if(!ApproachSurvivor()||ApproachArmedResponse())yield break;
                if(Time.realtimeSinceStartup>wall){ApproachCheck(false,"Sheltered field aid simulation failed to advance");yield break;}
                yield return null;
            }
        }
        IEnumerator CrewLiveFightApproach()
        {
            FreshCrewApproach();approachTotalDefeat=false;approachFallen.Clear();screen=ScreenMode.Pause;
            smokeResults.Add("METHOD: full crew fixture grants recruitment only and $800 purchasing money. Neri's existing pistol18 remains; Rell receives a purchased rifle20 through physical handoff; protagonist buys SMG48. Initial magazines use paused production reload steps as equipment preparation. All actors retain ordinary fresh health. Travel, assembly, firing, losses, field aid and final component custody use live production behavior; no opponent is removed or its AI disabled.");
            ApproachCheck(District.TakeShipment(false)&&District.Donate()&&District.Recruit(),"Full-crew fixture recruits Neri via finite clinic donation");
            District.crew.rellRecruited=true;District.crew.For("rell").memory="Prepared full-crew approach fixture; recruitment staged.";OrderCrew("rell","Hold");OrderCrew("neri","Follow");
            State.cash=800;
            smokeResults.Add("METHOD: AP-rifle-dealer opens the real four-row dealer UI as an explicitly staged screen fixture before purchases; it does not claim controller travel to Sella.");
            screen=ScreenMode.Dealer;yield return Capture("AP-rifle-dealer");screen=ScreenMode.Pause;
            ApproachCheck(District.BuyGun(State,5),"Finite dealer sells one rifle for Rell");
            for(int i=0;i<4;i++)ApproachCheck(District.BuyAmmo(State,5),"Finite dealer sells rifle packet "+(i+1));
            SelectCombatWeapon(5);ApproachCheck(ReloadPlayer(),"Rifle begins production magazine load");StepCombat(WeaponSpec.Rifle.reload);SelectCombatWeapon(1);
            ApproachCheck(District.BuyGun(State,4),"Finite dealer sells one SMG for protagonist");
            for(int i=0;i<4;i++)ApproachCheck(District.BuyAmmo(State,4),"Finite dealer sells SMG packet "+(i+1));
            SelectCombatWeapon(4);ApproachCheck(ReloadPlayer(),"SMG begins production magazine load");StepCombat(WeaponSpec.SMG.reload);SelectCombatWeapon(1);screen=ScreenMode.Play;
            yield return ApproachTravel(DockOperationState.Workshop+Vector3.back*1.5f);
            SelectCombatWeapon(5);ApproachCheck(TransferCrewGun("player","rell")&&District.crew.rell.ammo==20&&District.rifleAmmo==0,"Reached handoff transfers purchased rifle and twenty rounds to Rell");SelectCombatWeapon(1);
            OrderCrew("rell","Follow");
            yield return ApproachTravel(new Vector3(52,0,12));yield return ApproachTravel(new Vector3(-8,0,12));
            OrderCrew("neri","Move",new Vector3(-10,0,48));OrderCrew("rell","Move",new Vector3(-12,0,48));
            yield return ApproachTravel(new Vector3(-8,0,48));
            yield return ApproachCrewAt("neri",new Vector3(-10,0,48));yield return ApproachCrewAt("rell",new Vector3(-12,0,48));
            smokeResults.Add("METHOD: full crew regroups outside the yard, waits eight simulation seconds in normal Update for observed contact to age naturally, then crosses behind the watch. Orders finish at z47 so arrival tolerance does not trigger trespass. Rell passes north of Neri's held position. Protagonist physically takes separate left firing lane(18,49); guided trigger rejects friendly bodies throughout the possible SMG spread/recoil corridor.");
            for(float until=District.clock+8;District.clock<until;){TraceApproachSight(new Vector3(-8,0,48));yield return null;}
            ApproachCheck(yardSquad.Members.TrueForAll(m=>!m.DirectSight&&m.ContactAge>=CombatSquad.ContactLifetime),"Outer regroup allows actual yard sighting memory to expire without clearing it");
            yield return ApproachTravel(new Vector3(30,0,48));
            ApproachCheck(OrderCrew("neri","Move",new Vector3(26,0,47))&&OrderCrew("rell","Move",new Vector3(34,0,49.2f)),"Crew receives separated north approach lanes through real Move orders");
            yield return ApproachCrewAt("neri",new Vector3(26,0,47));yield return ApproachCrewAt("rell",new Vector3(34,0,49.2f));
            OrderCrew("rell","Move",new Vector3(34,0,47));yield return ApproachCrewAt("rell",new Vector3(34,0,47));
            yield return ApproachTravel(new Vector3(30,0,49));yield return ApproachTravel(new Vector3(18,0,49));
            ApproachCheck(City.Nav.Sight(Player.position,DockVale.position)&&ApproachClearCrewFireLane(DockVale.position),"Protagonist reached independent left firing lane with clear geometry past both partners");
            int playerAmmo=District.smgAmmo,neriAmmo=District.neri.ammo,rellAmmo=District.crew.rell.ammo;
            RecordCrewApproach("full-crew-assembled");screen=ScreenMode.Tactics;crewPanel=true;yield return Capture("AP-full-crew-orders");crewPanel=false;screen=ScreenMode.Play;
            SelectCombatWeapon(4);ApproachCheck(OrderCrew("neri","Engage")&&OrderCrew("rell","Engage"),"Both armed partners receive actual coordinated Engage orders");
            float deadline=Time.realtimeSinceStartup+35;
            while(yardSquad.Members.Exists(m=>m.Actor.health>0&&!m.Data.withdrawing))
            {
                TraceApproachSight(DockOperationState.ComponentPost);
                if(!ApproachSurvivor())break;
                if(ControlledCrewId!="player")
                {
                    if(District.crew.For(ControlledCrewId).order!="Engage")OrderCrew(ControlledCrewId,"Engage");CrewEngage(ControlledCrewId);
                    if(Time.realtimeSinceStartup>deadline)break;
                    yield return null;continue;
                }
                CombatSquadMember target=null;float nearest=float.MaxValue;
                foreach(var m in yardSquad.Members)
                    if(m.Actor.health>0&&!m.Data.withdrawing&&City.Nav.Sight(Player.position,m.Actor.position)&&ApproachClearCrewFireLane(m.Actor.position))
                    {float distance=Vector3.Distance(Player.position,m.Actor.position);if(distance<WeaponSpec.SMG.range&&distance<nearest){nearest=distance;target=m;}}
                if(CurrentWeapon.magazine==0&&District.smgAmmo>0&&CurrentWeapon.reloadRemaining==0)ReloadPlayer();
                if(target!=null&&CurrentWeapon.cooldown==0&&CurrentWeapon.reloadRemaining==0&&CurrentWeapon.magazine>0)FirePlayerAt(target.Actor.position);
                if(Time.realtimeSinceStartup>deadline){smokeResults.Add("OUTCOME: Finite yard attack time elapsed; attempt pickup and withdraw under live opposition.");break;}
                yield return null;
            }
            ApproachCheck(District.smgAmmo<playerAmmo&&District.neri.ammo<neriAmmo&&District.crew.rell.ammo<rellAmmo,"All three crew members actually spent their own finite ammunition in coordinated fight");
            ApproachCheck(ProjectileHits.Exists(h=>h.owner=="player"&&h.victim.StartsWith("yard-"))&&ProjectileHits.Exists(h=>(h.owner=="neri"||h.owner=="rell")&&h.victim.StartsWith("yard-")),"Owner/victim receipts prove protagonist and partner projectiles hit actual yard opponents");
            smokeResults.Add("OUTCOME: Protagonist friendly crew hit receipts = "+ProjectileHits.FindAll(h=>h.owner=="player"&&(h.victim=="neri"||h.victim=="rell")).Count+"; actual collision consequences retained.");
            ApproachCheck(District.dock.yardAttacked&&District.dock.yardHostile,"Actual projectile hits establish hostile yard outcome");
            smokeResults.Add("METHOD: combat has no perfect-survival requirement. After the bounded yard attack, attempt the actual held pickup and continue east with living Follow partners. No standing response loop waits for every projectile to disappear. A downed controller is replaced by a real living crew member; bodies, wounds and component custody remain where play left them. Nearby component handoff uses the actual interaction only. An unresolved operation or total defeat is recorded as mission loss, not completion. Total wipe invokes existing emergency recovery, not permadeath.");
            RecordCrewApproach("full-crew-yard-combat-outcome");
            bool recovery=screen==ScreenMode.Recovery;screen=ScreenMode.Tactics;yield return Capture("AP-full-crew-live-fight-result");screen=recovery?ScreenMode.Recovery:ScreenMode.Play;
            if(ApproachSurvivor())
            {
                yield return ApproachEscortedTravel(new Vector3(30,0,49),true);
                yield return ApproachEscortedTravel(DockOperationState.ComponentPost,true);
                yield return ApproachFightPickup();
            }
            foreach(var point in new[]{new Vector3(44,0,42),new Vector3(52,0,40),new Vector3(52,0,20)})
            {
                if(!ApproachSurvivor())break;
                ApproachCheck(City.Nav.Walkable(point)&&City.Nav.Find(ControlledPosition,point).Count>0,"East withdrawal waypoint is physically navigable: "+point);
                yield return ApproachEscortedTravel(point);
            }
            if(ApproachSurvivor())
            {
                RecordCrewApproach("full-crew-east-cover");yield return ApproachTreatInCover();
                yield return ApproachEscortedTravel(DockOperationState.Workshop+Vector3.back*1.5f);
            }
            if(ApproachSurvivor())
            {
                // Give living followers a bounded opportunity to arrive. Incapacitated
                // Rell cannot receive the pump; this must remain an unresolved mission.
                if(CrewAlive("rell")&&ControlledCrewId!="rell")OrderCrew("rell","Move",DockOperationState.Workshop+Vector3.right*1.5f);
                float gatherUntil=District.clock+8,gatherWall=Time.realtimeSinceStartup+30;
                while(ApproachSurvivor()&&CrewAlive("rell")&&Vector3.Distance(CrewPosition("rell"),DockOperationState.Workshop)>4&&District.clock<gatherUntil)
                {
                    if(ApproachArmedResponse())break;
                    if(Time.realtimeSinceStartup>gatherWall){ApproachCheck(false,"Workshop regroup simulation failed to advance");yield break;}
                    yield return null;
                }
                if(ApproachSurvivor())
                {
                    yield return ApproachFightPickup();
                    if(CrewAlive("rell")&&District.dock.componentOwner==ControlledCrewId&&DockNear(DockOperationState.Workshop,3)&&Vector3.Distance(CrewPosition("rell"),DockOperationState.Workshop)<4)
                        UpdateDockInteraction(.05f,true,false);
                }
            }
            yield return FinishCrewCombatOutcome();
        }
        [Serializable] sealed class CrewCombatActorOutcome
        {
            public string id,order,carrying,memory;public Vector3 position;public float health;
            public bool bleeding,abandoned,armed,holstered;public int bandages,weapon,ammo,magazine;
        }
        [Serializable] sealed class CrewCombatPersistentOutcome
        {
            public string selected,componentOwner,acquisition,recoverySummary;public bool componentReturned;
            public int playerOwnedMask;public int[] playerAmmo=new int[4],playerMagazines=new int[4];
            public List<CrewCombatActorOutcome> actors=new List<CrewCombatActorOutcome>();
        }
        [Serializable] sealed class CrewCombatOutcome
        {
            public string missionOutcome,method;public bool componentReturned,reloadVerified,totalDefeat;
            public List<string> incapacitatedDuringCombat=new List<string>();
            public CrewCombatPersistentOutcome beforeReload,afterReload;
        }
        CrewCombatPersistentOutcome ReadCrewCombatOutcome()
        {
            var result=new CrewCombatPersistentOutcome{selected=ControlledCrewId,componentOwner=District.dock.componentOwner,acquisition=District.dock.acquisition,componentReturned=District.dock.stakeResolved,recoverySummary=District.recoverySummary,playerOwnedMask=District.arms.ownedMask};
            for(int kind=2;kind<=5;kind++){result.playerAmmo[kind-2]=District.ArmsAmmo(kind);result.playerMagazines[kind-2]=District.GunState(kind).magazine;}
            foreach(string id in new[]{"player","neri","rell"})
            {
                var member=District.crew.For(id);var actor=CrewActor(id);
                result.actors.Add(new CrewCombatActorOutcome{id=id=="player"?member.id:actor.id,order=member.order,carrying=member.carrying,memory=member.memory,position=CrewPosition(id),health=CrewHealth(id),bleeding=CrewBleeding(id),abandoned=member.abandoned,armed=member.armed,holstered=member.holstered,bandages=CrewBandages(id),weapon=id=="player"?District.equippedWeapon:actor.combat.kind,ammo=id=="player"?CombatTotalAmmo:actor.ammo,magazine=id=="player"?CurrentWeapon.magazine:actor.combat.magazine});
            }
            return result;
        }
        bool CrewCombatReloadEqual(CrewCombatPersistentOutcome before,CrewCombatPersistentOutcome after)
        {
            string saved=JsonUtility.ToJson(before);
            if(saved==JsonUtility.ToJson(after))return true;
            // Resume calls Teleport, which adds .12 world units of controller
            // clearance. Preserve raw evidence and normalize only an uncarried
            // protagonist's upward offset in a comparison copy. No other field,
            // horizontal coordinate or carried-body placement gets a tolerance.
            var comparison=JsonUtility.FromJson<CrewCombatPersistentOutcome>(JsonUtility.ToJson(after));
            var original=before.actors.Find(a=>a.id=="player");var resumed=comparison.actors.Find(a=>a.id=="player");
            if(original==null||resumed==null)return false;
            float clearance=resumed.position.y-original.position.y;
            if(!ProjectileMath.Finite(clearance)||clearance<0||clearance>.1201f)return false;
            if(clearance>0&&(before.actors.Exists(a=>a.carrying=="player")||after.actors.Exists(a=>a.carrying=="player")))return false;
            resumed.position.y=original.position.y;
            return saved==JsonUtility.ToJson(comparison);
        }
        IEnumerator FinishCrewCombatOutcome()
        {
            autoMove=null;autoInteract=false;CloseDockDialog();
            if(approachTotalDefeat&&screen!=ScreenMode.Recovery)
            {
                float wall=Time.realtimeSinceStartup+10;
                while(screen!=ScreenMode.Recovery)
                {if(Time.realtimeSinceStartup>wall){ApproachCheck(false,"Total defeat did not enter production emergency recovery");yield break;}yield return null;}
            }
            ApproachSurvivor();
            string outcome=approachTotalDefeat?"total-defeat-emergency-recovery":District.dock.stakeResolved?"component-returned":"survivor-withdrawn-operation-unresolved";
            ApproachCheck(approachTotalDefeat||CrewAlive(ControlledCrewId)&&Vector3.Distance(ControlledPosition,DockOperationState.Workshop)<4,"Full-crew combat outcome reached a real terminal location or emergency recovery");
            screen=ScreenMode.Tactics;Save();
            var result=new CrewCombatOutcome{missionOutcome=outcome,componentReturned=District.dock.stakeResolved,totalDefeat=approachTotalDefeat,beforeReload=ReadCrewCombatOutcome(),incapacitatedDuringCombat=new List<string>(approachFallen),method="One live finite-equipment attempt. Incapacitated actors stay at recorded positions unless production emergency recovery occurred. Abandoned flags are actual saved game flags; an unrecovered actor can be left behind without a clinic abandonment command. Mission loss is permitted. No new casualty fixtures or permadeath mechanism. Player ammunition arrays list pistol, shotgun, SMG, rifle totals including magazines. Both snapshots retain raw positions. Reload comparison allows only uncarried protagonist upward Y clearance from 0 to 0.1201 world units for existing Teleport resume placement; all other fields and coordinates remain exact. A protagonist carried by another actor receives no clearance tolerance."};
            File.WriteAllText(Path.Combine(evidencePath,"crew-combat-outcome.json"),JsonUtility.ToJson(result,true));
            smokeResults.Add("OUTCOME: Full-crew combat outcome "+outcome+"; componentReturned="+result.componentReturned+"; custody="+result.beforeReload.componentOwner);
            foreach(var actor in result.beforeReload.actors)smokeResults.Add("OUTCOME: "+actor.id+" health="+actor.health+" bleeding="+actor.bleeding+" at="+actor.position+" ammo="+actor.ammo+" magazine="+actor.magazine+" abandoned="+actor.abandoned);
            ApproachCheck(District.Valid(),"Full-crew combat outcome preserves integrated finite-state invariants");
            RecordCrewApproach("full-crew-combat-outcome / "+outcome);crewPanel=true;yield return Capture("AP-full-crew-combat-outcome");crewPanel=false;
            Save();StartRun(true);screen=ScreenMode.Tactics;result.afterReload=ReadCrewCombatOutcome();
            result.reloadVerified=CrewCombatReloadEqual(result.beforeReload,result.afterReload)&&District.Valid();
            File.WriteAllText(Path.Combine(evidencePath,"crew-combat-outcome.json"),JsonUtility.ToJson(result,true));
            ApproachCheck(result.reloadVerified,"Full-crew combat outcome reload verified: identities, custody, wounds and remaining ammunition");
            RecordCrewApproach("full-crew-reloaded / "+outcome);
        }
    }
}
