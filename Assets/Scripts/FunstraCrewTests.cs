using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        void CrewSnapshot(string label)
        {
            Save();
            File.WriteAllText(Path.Combine(evidencePath,label+"-state.json"),JsonUtility.ToJson(State,true));
        }
        void CrewFixturePause()
        {
            screen=ScreenMode.Pause;CloseDockDialog();autoMove=null;autoInteract=false;
            smokeFreezeAgents=true;freezeDistrictAI=true;showMap=false;
            foreach(var m in District.crew.members){m.order="Hold";m.patient="";m.aidRemaining=0;}
        }
        IEnumerator CrewTravel(Vector3 destination)
        {
            Vector3 target=City.Nav.SafePoint(destination);string actor=ControlledCrewId;
            var path=City.Nav.Find(ControlledPosition,target);int point=0;
            Check(path.Count>0,"Crew route exists for "+actor+" to "+target);
            float deadline=Time.realtimeSinceStartup+120,repath=Time.realtimeSinceStartup+1;
            while(Vector2.Distance(new Vector2(ControlledPosition.x,ControlledPosition.z),new Vector2(target.x,target.z))>.38f)
            {
                if(ControlledCrewId!=actor)throw new Exception("Controlled actor changed during guided travel from "+actor+" to "+ControlledCrewId);
                if(Time.realtimeSinceStartup>deadline)throw new Exception("Crew controller blocked: "+actor+" at "+ControlledPosition+" toward "+target);
                if(point>=path.Count||!City.Nav.Walkable(path[point])||Time.realtimeSinceStartup>repath)
                {path=City.Nav.Find(ControlledPosition,target);point=0;repath=Time.realtimeSinceStartup+2;}
                while(point<path.Count&&Vector2.Distance(new Vector2(ControlledPosition.x,ControlledPosition.z),new Vector2(path[point].x,path[point].z))<.38f)point++;
                autoMove=point<path.Count?(Vector3?)path[point]:null;yield return null;
            }
            autoMove=null;yield return null;
            Check(Vector3.Distance(ControlledPosition,target)<1,"Actual selected controller arrived: "+actor+" at "+target);
        }
        IEnumerator CrewSteps()
        {
            yield return new WaitForSeconds(1);yield return Capture("C00-title");
            yield return CrewMovementRegression();
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--crew-movement")>=0)yield break;
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;smokeFreezeAgents=true;freezeDistrictAI=true;
            VerifyCrewUIRules();
            Check(crewTest&&Smoke&&CrewEnabled&&DockEnabled&&muteTests&&AudioListener.volume==0,"Crew route uses fresh isolated muted exported campaign");
            Check(!District.recruited&&!District.crew.rellRecruited&&ControlledCrewId=="player","Paid route begins genuinely solo");
            smokeResults.Add("METHOD: solo paid route uses production controller, live yard, finite $100 cash fixture, actual held pickup, and reached menu-equivalent payment. Citizens and companion movement held; traffic/calendar continue. No earned-income or human-play claim.");
            State.cash=100;
            yield return CrewTravel(new Vector3(8,0,13));yield return CrewTravel(new Vector3(8,0,39));
            yield return CrewTravel(DockOperationState.PublicCounter);
            Check(!DockYardEngaged(ControlledPosition)&&DockValeAtCounter,"Live Vale's public counter is reachable before trespass engagement");
            screen=ScreenMode.Tactics;yield return Capture("C01a-public-quay-street");screen=ScreenMode.Play;
            Check(UpdateDockInteraction(.05f,true,false)&&DockDialogOpen,"Reached public counter opens actual release dialogue");
            yield return Capture("C01-vale-release-terms");
            Check(District.dock.TryPayRelease(State,DockValeAtCounter)&&State.cash==10&&District.dock.valeMoney==90,"Reached payment moves finite $90 to live Vale");
            District.Record("pump release","vale","Guided route paid $90 at the reached public counter.");yardSquad.ClearContact();Save();CloseDockDialog();
            float health=District.health;int yardRounds=0;foreach(var m in yardSquad.Members)yardRounds+=m.Actor.ammo;
            yield return CrewTravel(DockOperationState.ComponentPost);autoInteract=true;
            float deadline=Time.realtimeSinceStartup+8;
            while(District.dock.componentOwner=="yard")
            {if(Time.realtimeSinceStartup>deadline)throw new Exception("Paid pump hold failed: "+prompt);yield return null;}
            autoInteract=false;
            Check(District.dock.componentOwner=="player"&&District.dock.acquisition=="paid release"&&!District.dock.yardHostile,"Actual held pickup gives the sole component to the solo collector");
            int remaining=0;foreach(var m in yardSquad.Members)remaining+=m.Actor.ammo;
            Check(District.health==health&&remaining==yardRounds,"Paid passage remains nonviolent with the live yard squad");
            screen=ScreenMode.Tactics;yield return Capture("C02-paid-yard-collection");screen=ScreenMode.Play;
            yield return CrewTravel(new Vector3(48,0,39));yield return CrewTravel(DockOperationState.Workshop+Vector3.back*1.5f);
            Check(UpdateDockInteraction(.05f,true,false)&&District.dock.stakeResolved&&District.crew.rellRecruited,"Controller returns actual component and earns Rell partnership");
            OrderCrew("rell","Hold");CrewSnapshot("C03-pump-returned");screen=ScreenMode.Tactics;yield return Capture("C03-rell-workshop-return");
            yield return CrewIdentityAndRescueSteps();
            yield return CrewRepairSteps();
            yield return CrewOppositionSteps();
            smokeResults.Add("COVERAGE LIMIT: no organically earned cash, full hostile crew assault victory, or autonomous unseen theft victory is claimed by this route. Pure dock rules cover finite alternate custody; opposition suites separately exercise live report/fire/retreat behavior.");
            File.WriteAllText(Path.Combine(evidencePath,"crew-method.txt"),string.Join("\n",smokeResults.FindAll(s=>s.StartsWith("METHOD:")||s.StartsWith("COVERAGE LIMIT:")))+"\n"+SystemInfo.processorType+" / "+SystemInfo.graphicsDeviceName+" / "+Screen.width+"x"+Screen.height);
        }
        IEnumerator CrewMovementRegression()
        {
            StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=true;freezeDistrictAI=false;District.recruited=true;District.crew.rellRecruited=true;
            SetCrewPosition("player",new Vector3(-7.997f,0,47.633f));
            var car=City.Cars[0];Vector3 carStart=car.body.position;
            SetCrewPosition("rell",carStart+car.body.forward*4);
            Check(City.Nav.Walkable(CrewPosition("rell")),"Traffic fixture stages Rell outside the existing vehicle footprint");
            for(int frame=0;frame<30;frame++)City.StepTraffic(this,1f/30);
            Check(Vector3.Distance(carStart,car.body.position)<.01f&&car.yielding,"Actual street traffic yields to Rell's physical body");
            // Preserve the observed relative offsets and grid alignment, four
            // metres west of the fresh car whose original run position is unknown.
            SetCrewPosition("neri",new Vector3(-7.701f,0,21.701f));
            SetCrewPosition("rell",new Vector3(-7.618f,0,22.579f));
            Check(City.Nav.Walkable(CrewPosition("neri"))&&City.Nav.Walkable(CrewPosition("rell")),"Congestion fixture begins with two physically walkable origins");
            Vector3 neriGoal=new Vector3(-10,0,48),rellGoal=new Vector3(-12,0,48);
            smokeResults.Add("METHOD: crowding regression preserves the ninth-run pair's relative positions/grid alignment, translated four metres west to avoid the fresh traffic car. Original failure traffic positions were not recorded, so this is a controlled connector/crowding fixture, not an exact scene replay. Shared StepCrew advances at 1/30s while town movement and calendar are held; no collision/walkability override. A separate actual StepTraffic fixture requires braking for Rell alone.");
            OrderCrew("neri","Move",neriGoal);OrderCrew("rell","Move",rellGoal);
            float minimum=Vector3.Distance(CrewPosition("neri"),CrewPosition("rell"));int steps=0;
            for(;steps<900;steps++)
            {
                StepCrew(1f/30);minimum=Mathf.Min(minimum,Vector3.Distance(CrewPosition("neri"),CrewPosition("rell")));
                if(Vector3.Distance(CrewPosition("neri"),neriGoal)<.65f&&Vector3.Distance(CrewPosition("rell"),rellGoal)<.65f)break;
            }
            string details="Steps: "+steps+"\nNeri: "+CrewPosition("neri")+"\nRell: "+CrewPosition("rell")+"\nMinimum separation: "+minimum;
            foreach(string id in new[]{"neri","rell"})
            {
                var path=crewPaths[id];var position=CrewPosition(id);
                details+="\n"+id+" walkable="+City.Nav.Walkable(position)+" path="+string.Join(", ",path);
                if(path.Count>0){var next=position+(path[0]-position).normalized*.15f;details+=" next="+next+" ground="+City.Nav.ClearWalk(position,next)+" body="+CrewMoveClear(id,next);}
                foreach(var a in Agents)if(Vector3.Distance(position,a.Position)<3)details+=" nearby="+a.Record?.id+"/"+a.Position;
            }
            File.WriteAllText(Path.Combine(evidencePath,"crew-movement-result.txt"),details);
            Check(Vector3.Distance(CrewPosition("neri"),neriGoal)<.65f&&Vector3.Distance(CrewPosition("rell"),rellGoal)<.65f,"Simultaneous crew orders clear a shared grid connector and reach both destinations");
            Check(minimum>=.849f,"Crew movement regression preserves physical separation without passing through a partner");
            screen=ScreenMode.Tactics;yield return Capture("C00a-crew-movement-regression");
        }
        IEnumerator CrewIdentityAndRescueSteps()
        {
            CrewFixturePause();
            smokeResults.Add("METHOD: identity/rescue fixtures stage locations and wounds while paused. Neri is recruited via the actual finite shipment donation/trust APIs, without claiming controller medicine acquisition. Equipment is bought with explicit $500 fixture cash. Aid uses accelerated .05-second production steps; carry uses actual selected-controller movement around the clinic wall.");
            Check(District.TakeShipment(false)&&District.Donate()&&District.Recruit(),"Neri fixture transfers finite six-dose shipment and earns existing clinic partnership");
            OrderCrew("neri","Hold");
            SetCrewPosition("player",new Vector3(13,0,39));SetCrewPosition("rell",new Vector3(14.5f,0,39));SetCrewPosition("neri",new Vector3(13,0,36));
            District.health=71;District.neri.health=63;District.crew.rell.health=87;
            Vector3 player=Player.position,neri=District.neri.position,rell=District.crew.rell.position;int neriAmmo=District.neri.ammo,neriMagazine=District.neri.combat.magazine;
            Check(SelectCrew("rell")&&SelectCrew("neri")&&SelectCrew("player"),"All three persistent actors can be selected");
            Check(Player.position==player&&District.neri.position==neri&&District.crew.rell.position==rell&&District.health==71&&District.neri.health==63&&District.crew.rell.health==87&&District.neri.ammo==neriAmmo&&District.neri.combat.magazine==neriMagazine,"Selection leaves independent positions, wounds, ammunition and magazines in place");
            State.cash=500;Check(District.BuyGun(State,2)&&District.BuyAmmo(State,2),"Transfer fixture obtains one stocked pistol and finite rounds");
            SelectCombatWeapon(2);if(CurrentWeapon.magazine==0){Check(ReloadPlayer(),"Loose purchase starts timed reload");StepCombat(WeaponSpec.Pistol.reload);}
            var gun=CurrentWeapon;int rounds=District.ammo;int magazine=gun.magazine;
            Check(TransferCrewGun("player","rell")&&ReferenceEquals(District.crew.rell.combat,gun)&&District.ammo==0&&District.crew.rell.ammo==rounds&&District.crew.rell.combat.magazine==magazine,"Transfer moves the actual gun object, magazine and all rounds into Rell custody");
            Check(!TransferCrewGun("player","rell")&&District.crew.rell.ammo==rounds,"Repeated gun handoff cannot clone equipment");
            int dressings=District.bandages+District.crew.rell.bandages;
            Check(TransferCrewBandage("player","rell")&&District.bandages+District.crew.rell.bandages==dressings,"Dressing handoff conserves actual personal inventory");
            Check(SelectCrew("rell"),"Select armed Rell for the staged shared projectile sightline");
            var vale=DockVale;float before=vale.health;int receipts=ProjectileHits.Count;
            Check(City.Nav.Sight(District.crew.rell.position,vale.position)&&FireActorAt(District.crew.rell,rellBody,vale.position,2),"Rell fires the shared finite pistol across a checked clear sightline");
            Check(District.projectiles.Count>0&&District.projectiles[0].owner=="rell"&&District.crew.rell.ammo==rounds-1,"Projectile owner is firing actor and one round is spent before impact");
            Check(SelectCrew("neri"),"Control switches away while Rell's projectile is in flight");
            Vector3 flight=District.projectiles[0].position;Save();StartRun(true);CrewFixturePause();
            Check(ControlledCrewId=="neri"&&District.projectiles.Count>0&&District.projectiles[0].owner=="rell"&&Vector3.Distance(District.projectiles[0].position,flight)<.001f&&District.crew.rell.ammo==rounds-1,"Actual reload preserves selected Neri, Rell gun custody, spent round and projectile owner/position");
            for(int i=0;i<30;i++){StepCombat(.025f);yield return null;}
            Check(DockVale.health==before-WeaponSpec.Pistol.damage&&ProjectileHits.Count>receipts&&ProjectileHits.Exists(h=>h.owner=="rell"&&h.victim=="yard-0"),"Reloaded Rell projectile hits Vale once with owner/victim receipt after control switch");
            Check(District.dock.yardAttacked&&District.dock.yardHostile,"Real crew projectile damage revokes paid yard passage");
            CrewSnapshot("C04-identity-projectile-transfer");
            yardSquad=null;District.projectiles.Clear();Heat=0;ResetPolice();
            SetCrewPosition("player",new Vector3(-16,0,-16.7f));SetCrewPosition("neri",new Vector3(-16,0,-18));SetCrewPosition("rell",new Vector3(-13,0,-18));
            District.health=35;District.bleeding=true;District.neri.health=70;District.neri.bleeding=false;
            int bandages=District.neri.bandages;int practice=District.crew.For("neri").medicinePractice;
            Check(BeginCrewAid("neri","player"),"Neri begins three-second field aid beside the actual wounded player");StepCrew(.5f);
            MoveCrew("neri",Vector3.right,4,.2f);StepCrew(.05f);
            Check(District.crew.For("neri").aidRemaining==0&&District.neri.bandages==bandages&&District.health==35,"Moving helper interrupts aid and retains dressing");
            Check(BeginCrewAid("neri","player"),"Stationary helper can restart interrupted aid");ApplyCombatHit(District.neri,neriBody,1,"rook",false);StepCrew(.05f);
            Check(District.crew.For("neri").aidRemaining==0&&District.neri.bandages==bandages,"New helper damage interrupts aid without spending dressing");
            Check(BeginCrewAid("neri","player"),"Quiet nearby aid can restart after damage");
            for(int i=0;i<62;i++)StepCrew(.05f);
            Check(District.health==47&&!District.bleeding&&District.neri.bandages==bandages-1&&District.crew.For("neri").medicinePractice==practice+1,"Completed timed aid spends one dressing, stops actual bleeding and earns one practice");
            smokeResults.Add("METHOD: self-aid regression then runs 3.5 seconds of normal Update with actual ongoing Neri bleeding; damage interruption above uses a production combat hit, not bleed attrition.");
            Check(SelectCrew("neri"),"Bleeding Neri is directly controlled for self-aid regression");District.neri.bleeding=true;
            float selfHealth=District.neri.health;int selfDressings=District.neri.bandages;
            Check(BeginCrewAid("neri","neri"),"Bleeding helper can begin timed self-aid");screen=ScreenMode.Play;
            yield return new WaitForSeconds(3.5f);screen=ScreenMode.Pause;
            Check(!District.neri.bleeding&&District.neri.health>selfHealth+9&&District.neri.bandages==selfDressings-1&&District.crew.For("neri").medicinePractice==practice+2,"Normal Update self-aid completes through bleed attrition and spends one dressing");
            Check(SelectCrew("player"),"Protagonist control resumes before explicit downed-player fixture");District.health=0;StepCrew(.05f);
            Check(ControlledCrewId=="neri"&&District.health==0&&CrewDefeatHandled(),"Downed protagonist remains down while living Neri takes control and prevents solo recovery teleport");
            Check(!City.Nav.ClearWalk(District.neri.position,DistrictState.Clinic),"Carry fixture's direct route is obstructed by the clinic east wall");
            Check(CarryCrew("neri","player"),"Neri physically lifts nearby downed protagonist");StepCrew(.05f);
            Vector3 carryingStart=District.neri.position;
            screen=ScreenMode.Tactics;crewPanel=true;crewTarget="player";SnapCamera();yield return Capture("C05-crew-carry-panel");crewPanel=false;
            yield return Capture("C05b-physical-carry");
            screen=ScreenMode.Play;yield return CrewTravel(DistrictState.Clinic);
            Check(Vector3.Distance(carryingStart,District.neri.position)>5&&CarrierOf("player")=="neri"&&Vector3.Distance(Player.position,District.neri.position)<1.1f,"Actual Neri controller carried protagonist around wall and through clinic doorway");
            screen=ScreenMode.Tactics;int stock=District.clinicStock,consumed=District.consumed,total=District.TotalMedicine;
            Check(ReturnCrew("neri")&&District.health==45&&CarrierOf("player")==null&&District.clinicStock==stock-1&&District.consumed==consumed+1&&District.TotalMedicine==total,"Physical clinic admission spends one existing dose and restores carried protagonist");
            Check(District.crew.For("player").rescuedCount==1&&District.crew.For("player").memory.Contains("carried me"),"Rescued protagonist remembers who physically returned them");
            crewPanel=true;crewTarget="player";yield return Capture("C06-clinic-rescue-memory");crewPanel=false;
            yield return Capture("C06b-clinic-admission");
            CrewSnapshot("C06-rescue");Save();StartRun(true);CrewFixturePause();yardSquad=null;
            Check(District.health==45&&District.crew.For("player").rescuedCount==1&&District.clinicStock==stock-1,"Actual save/reload preserves rescue, survivor health and finite clinic cost");
            SetCrewPosition("rell",new Vector3(-16,0,-18));District.crew.rell.health=0;District.crew.rell.bleeding=false;
            Check(ControlledCrewId=="neri"&&AbandonCrew("rell")&&!AbandonCrew("rell"),"Sheltered Neri explicitly records leaving downed Rell behind exactly once");
            Vector3 left=District.crew.rell.position;Save();StartRun(true);CrewFixturePause();yardSquad=null;
            Check(District.crew.For("rell").abandoned&&District.crew.For("rell").abandonedCount==1&&District.crew.rell.health==0&&District.crew.rell.position==left&&District.crew.For("rell").memory.Contains("left me behind"),"Abandonment and actual downed body location survive reload without respawn");
            screen=ScreenMode.Tactics;crewPanel=true;crewTarget="rell";yield return Capture("C07-remembered-abandonment");crewPanel=false;
            Check(District.Valid(),"Crew rescue/abandonment campaign preserves integrated state invariants");CrewSnapshot("C07-abandonment");
            smokeResults.Add("METHOD: total-wipe regression explicitly downs the recovered protagonist, lifts them using real carry API, then downs the carrier and invokes production defeat. This is a failure-state fixture, not a claimed combat encounter.");
            District.health=0;Check(CarryCrew("neri","player"),"Total-wipe fixture begins with a real carrier link");District.neri.health=0;
            Check(!CrewDefeatHandled(),"No living recruited actor remains in total-wipe fixture");DistrictDefeat();
            Check(District.health>0&&ControlledCrewId=="player"&&CarrierOf("player")==null&&District.crew.members.TrueForAll(m=>m.carrying==""&&m.patient==""&&m.aidRemaining==0)&&Vector3.Distance(Player.position,Jobs.Home)<1,"Total defeat clears stale carry/aid links before protagonist emergency recovery");
            yield return Capture("C07b-total-wipe-recovery");Save();StartRun(true);CrewFixturePause();yardSquad=null;
            Check(CarrierOf("player")==null&&ControlledCrewId=="player"&&District.health>0&&District.Valid(),"Reloaded total-wipe survivor remains playable without stale carrier ownership");CrewSnapshot("C07b-total-wipe");
        }
        IEnumerator CrewRepairSteps()
        {
            smokeResults.Add("METHOD: separate fresh solo nonviolent preparation route; real controller reaches Rell and his auxiliary pump, then actual autoInteract holds E for twelve real simulation seconds. Accepting work uses the same dialogue state action. No repair completion or recruitment state is injected.");
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;smokeFreezeAgents=true;freezeDistrictAI=true;
            yield return CrewTravel(DockOperationState.Workshop+Vector3.back*1.5f);
            Check(UpdateDockInteraction(.05f,true,false)&&DockDialogOpen,"Fresh solo controller reaches Rell's actual workshop dialogue");
            yield return Capture("C08-rell-workshop-offer");
            Check(District.dock.BeginRepair(),"Reached dialogue accepts concrete finite-gasket repair");CloseDockDialog();
            yield return CrewTravel(DockOperationState.RepairPost);
            int mechanics=District.crew.mechanicsPractice;autoInteract=true;float deadline=Time.realtimeSinceStartup+25;
            while(!District.dock.auxiliaryRepaired)
            {if(Time.realtimeSinceStartup>deadline)throw new Exception("Workshop hold repair timed out: "+prompt);yield return null;}
            autoInteract=false;OrderCrew("rell","Hold");
            Check(District.crew.rellRecruited&&!District.recruited&&District.dock.componentOwner=="yard"&&District.dock.gasketStock==0&&District.dock.gasketsUsed==1,"Nonviolent solo repair uses last gasket and recruits Rell while main impeller remains in yard");
            Check(District.crew.mechanicsPractice==mechanics+1&&District.dock.WorkshopDrained&&!dockWater.activeSelf,"Completed real held repair drains visible workshop and awards practical learning once");
            screen=ScreenMode.Tactics;SnapCamera();yield return Capture("C09-repaired-workshop-partnership");
            Save();StartRun(true);CrewFixturePause();
            Check(District.crew.rellRecruited&&District.dock.auxiliaryRepaired&&District.dock.gasketStock==0&&District.crew.mechanicsPractice==mechanics+1&&District.dock.componentOwner=="yard","Actual reload retains independent repair partnership and finite unrecovered main pump");
            Check(District.Valid(),"Repaired workshop campaign remains integrated-valid");CrewSnapshot("C09-repair-final");
        }
    }
}
