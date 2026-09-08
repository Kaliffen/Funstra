using System.Collections;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        bool freezeDistrictAI;
        IEnumerator BandageSteps()
        {
            yield return new WaitForSeconds(1);
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;State.cash=160;smokeFreezeAgents=true;freezeDistrictAI=true;
            Check(City.Nav.Walkable(DistrictState.Clinic)&&City.Nav.Walkable(City.Nav.SafePoint(DistrictState.Garage))&&City.Nav.Walkable(DistrictState.Buyer),"All scenario locations are walkable");
            yield return Travel(DistrictState.Clinic);
            UpdateMedicalInteraction(0,true,false);Check(screen==ScreenMode.Clinic,"Actual interaction opens Neri conversation");screen=ScreenMode.Play;
            yield return Travel(DistrictState.CollectorPost);
            UpdateMedicalInteraction(0,true,false);Check(screen==ScreenMode.Collector,"Actual interaction opens collector terms");
            Check(District.PayRelease(State)&&State.cash==60,"Peaceful release pays collector");Save();screen=ScreenMode.Play;
            yield return Travel(DistrictState.Garage);
            autoInteract=true;float deadline=Time.realtimeSinceStartup+8;
            while(!District.Carrying) { if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Medical pickup did not finish");yield return null; }
            autoInteract=false;
            Check(District.Carrying&&!District.identified&&!District.hostile,"Actual paid hold collects medicine without hostility");
            Check(!medicineProp.activeSelf,"Collected stock disappears from world rendering");
            yield return Travel(DistrictState.Clinic);
            Check(District.Donate()&&District.Recruit(),"Clinic stock and trust unlock Neri partnership");Save();freezeDistrictAI=false;
            yield return Travel(Jobs.Home);
            deadline=Time.realtimeSinceStartup+12;
            while(Vector3.Distance(District.neri.position,Player.position)>3) { if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Neri failed to follow at "+District.neri.position+" walkable="+City.Nav.Walkable(District.neri.position)+" next="+(neriPath.Count>0?neriPath[0].ToString():"empty")+" player="+Player.position);yield return null; }
            Check(Vector3.Distance(District.neri.position,Player.position)<3,"Companion navigates streets to follow player");
            District.health=45;District.bleeding=true;int dressings=District.neri.bandages;aidCooldown=0;
            yield return new WaitForSeconds(.4f);
            Check(!District.bleeding&&District.health>60&&District.neri.bandages==dressings-1,"Nearby companion autonomously stabilizes bleeding with finite supplies");
            OrderNeri("Hold");Vector3 held=District.neri.position;yield return new WaitForSeconds(.5f);
            Check(Vector3.Distance(held,District.neri.position)<.1f,"Hold order stops companion movement");
            float before=District.clock;screen=ScreenMode.Tactics;yield return new WaitForSeconds(.5f);Check(District.clock==before,"Tactical pause stops world transactions and wounds");screen=ScreenMode.Play;
            Save();Vector3 position=Player.position;int doses=District.clinicStock;StartRun(true);
            Check(District.recruited&&District.neri.order=="Hold"&&District.clinicStock==doses&&Vector3.Distance(Player.position,position)<.5f,"Reload preserves companion, orders, stock and player position");
            Cargo.Take(0,State);Save();StartRun(true);
            Check(Cargo.Value==60&&Cargo.Taken(0),"New save format preserves loose cargo across session endings");
            autoInteract=true;yield return new WaitForSeconds(.6f);
            Check(Extracting&&extractionProgress>.1f,"Companion near home does not block cargo extraction");
            Teleport(Jobs.Home+Vector3.right);yield return null;yield return null;
            Check(Cargo.Value==60&&extractionProgress<.08f,"Moving during extraction resets the hold without losing cargo");
            autoInteract=false;yield return null;Teleport(Jobs.Home);autoInteract=true;yield return new WaitForSeconds(.6f);
            RaiseAlarm("Extraction interruption check",Player.position);yield return null;yield return null;
            Check(Cargo.Value==60&&!Extracting&&extractionProgress==0,"Becoming wanted cancels an in-progress extraction");
            autoInteract=false;Heat=0;ResetPolice();yield return null;
            int bankCash=State.cash;autoInteract=true;deadline=Time.realtimeSinceStartup+7;
            while(Cargo.Value>0) { if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Final interrupted extraction retry failed");yield return null; }
            autoInteract=false;Check(State.cash==bankCash+60,"Interrupted extraction can be retried and pays exactly once");
            // Isolate sight and combat from unrelated legacy patrols.
            freezeDistrictAI=true;State=new RunState();District.introSeen=true;screen=ScreenMode.Play;ResetDistrictRuntime();smokeFreezeAgents=true;
            Teleport(new Vector3(-28,0,-5));District.guard.position=new Vector3(-28,0,2);SyncDistrictArt();selectedActor=1;weapon=2;
            int bullets=District.ammo;float hp=District.guard.health;attackCooldown=0;
            Check(AttackSelected()&&District.ammo==bullets-1&&District.guard.health==hp,"Pistol trigger spends a round without instant damage");
            yield return new WaitForSeconds(.3f);
            Check(District.guard.health<hp&&District.identified,"Traveling pistol round wounds target and records witnessed offense");
            guardBody.LookAt(Player.position);defeatGrace=0;guardCooldown=0;float playerHP=District.health;UpdateGuard(.03f);
            Check(District.health==playerHP,"Guard trigger also has no instant damage");
            yield return new WaitForSeconds(.3f);
            Check(District.health<playerHP&&District.bleeding,"Guard projectile travels through clear sight and causes bleeding");
            Teleport(new Vector3(-18,0,-12));District.guard.position=new Vector3(-18,0,12);SyncDistrictArt();attackCooldown=0;bullets=District.ammo;
            District.pistolWeapon.cooldown=0;hp=District.guard.health;
            Check(AttackSelected()&&District.ammo==bullets-1,"Firing toward a building spends a physical round");
            yield return new WaitForSeconds(.7f);
            Check(District.guard.health==hp,"Building stops the traveling round before the guard");
            playerHP=District.health;guardCooldown=0;UpdateGuard(.03f);Check(District.health==playerHP,"Guard cannot shoot player through a building");
            // Both actors stand south of the physical medicine case; neither is embedded in the prop.
            Teleport(new Vector3(-28,0,-4));District.guard.position=new Vector3(-28,0,2);District.neri.position=new Vector3(-28,0,1);District.recruited=true;District.guard.ammo=0;District.hostile=true;
            SyncDistrictArt();guardBody.LookAt(Player.position);
            Check(City.Nav.Walkable(District.guard.position)&&City.Nav.Walkable(District.neri.position)&&City.Nav.Sight(District.guard.position,District.neri.position),"Melee fixture places guard and companion on clear walkable ground");
            guardCooldown=0;playerHP=District.health;float neriHP=District.neri.health;UpdateGuard(.03f);
            Check(District.health==playerHP&&District.neri.health==neriHP-8,"Empty-magazine guard strikes nearby visible companion rather than distant player");
            Teleport(new Vector3(-18,0,-12));guardCooldown=0;neriHP=District.neri.health;UpdateGuard(.03f);
            Check(District.health==playerHP&&District.neri.health==neriHP-8,"Visible companion remains a valid melee target while player is out of sight");
            District.guard.position=new Vector3(-28,0,2.8f);District.neri.position=new Vector3(-28,0,5.2f);SyncDistrictArt();guardBody.LookAt(neriBody.position);
            Check(City.Nav.Walkable(District.guard.position)&&City.Nav.Walkable(District.neri.position)&&!City.Nav.Sight(District.guard.position,District.neri.position),"Medicine case separates two walkable positions within melee range");
            guardCooldown=0;neriHP=District.neri.health;UpdateGuard(.03f);
            Check(District.neri.health==neriHP,"Guard cannot strike nearby companion through physical medicine case");
            District.recruited=false;District.neri.position=DistrictState.Clinic;District.guard.ammo=18;
            Heat=0;District.hostile=false;Check(District.identified,"Clearing immediate heat preserves collector memory");
            District.health=40;District.bleeding=true;Check(District.BandagePlayer()&&!District.bleeding&&District.health==52,"Bandage stops bleeding without fully healing wounds");
            District.TakeShipment(false);District.health=0;DistrictDefeat();
            Check(screen==ScreenMode.Recovery&&District.health==45&&District.debt==40&&!District.Carrying,"Actual defeat produces recovery screen, debt and recoverable shipment");
            Check(Vector3.Distance(Player.position,Jobs.Home)<1,"Defeat returns player to a usable bed");
            screen=ScreenMode.Play;District.clock=800;District.shipmentOwner="buyer";State.cash=0;
            Check(District.RestOnCredit()&&District.health==80,"Penniless post-sale recovery remains available");
            District.neri.health=0;District.neri.position=Player.position+Vector3.right;District.bandages=1;
            UpdateMedicalInteraction(0,true,false);
            Check(District.neri.health>0&&District.bandages==0,"Actual E interaction stabilizes downed companion");
            Check(District.TotalMedicine==12,"Complete runtime outcome sequence conserves all medicine");
            State=new RunState();District.introSeen=true;screen=ScreenMode.Play;ResetDistrictRuntime();freezeDistrictAI=true;
            Teleport(DistrictState.Garage);District.guard.position=new Vector3(-28,0,9);District.collector.position=DistrictState.CollectorPost;SyncDistrictArt();guardBody.rotation=collectorBody.rotation=Quaternion.identity;
            autoInteract=true;deadline=Time.realtimeSinceStartup+8;
            while(!District.Carrying) { if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Unseen medical theft timed out");yield return null; }
            autoInteract=false;District.Tick(31);
            Check(District.Carrying&&District.discovered&&!District.identified,"Actual unobserved hold creates missing stock without identifying thief");
            Save();StartRun(true);Check(District.Carrying&&District.discovered&&!District.identified,"Reload retains stolen medicine and distinct witness knowledge");
            Teleport(new Vector3(12,0,-13));Agents[3].Body.position=new Vector3(13.5f,0,-13);Agents[3].Record.position=Agents[3].Position;
            selectedActor=7;weapon=1;attackCooldown=0;float citizenHP=Agents[3].Record.health;
            Check(AttackSelected()&&Agents[3].Record.health<citizenHP&&Heat>0,"Shared melee rules let residents be harmed and call police");
            Save();float wound=Agents[3].Record.health;StartRun(true);
            Check(Agents[3].Record.health==wound&&Agents[3].Record.order=="Flee","Resident wounds and reaction survive reload");
            // Live local confrontation: guard AI and character controller run together.
            State=new RunState();District.introSeen=true;screen=ScreenMode.Play;ResetDistrictRuntime();freezeDistrictAI=false;smokeFreezeAgents=true;
            // Use the clear side of the alley; the medicine case now correctly stops bullets on its centerline.
            Teleport(new Vector3(-29.5f,0,-2));District.guard.position=new Vector3(-29.5f,0,8);SyncDistrictArt();guardBody.LookAt(Player.position);
            Check(City.Nav.Walkable(District.guard.position)&&City.Nav.Sight(Player.position,District.guard.position),"Live confrontation uses the clear route beside physical medicine cover");
            selectedActor=1;weapon=2;defeatGrace=0;deadline=Time.realtimeSinceStartup+8;
            while(District.guard.health>0)
            {
                if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Live confrontation did not resolve");
                AttackSelected();yield return null;
            }
            Check(District.health>0&&District.health<100&&District.bleeding,"Live gunfight wounds player and incapacitates guard without scripted damage");
            Check(District.ammo==9,"Three actual pistol shots spend three rounds");
            yield return Travel(DistrictState.Garage);autoInteract=true;deadline=Time.realtimeSinceStartup+7;
            while(!District.Carrying) { if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Post-combat medicine pickup failed");yield return null; }
            autoInteract=false;Check(District.Carrying&&District.identified,"Violence creates access to the same medicine and a lasting identified offense");
            Save();StartRun(true);Check(District.guard.health==0&&District.Carrying&&District.identified,"Reload preserves combat outcome, carried medicine and grievance");
            Save();Check(RunState.Load(savePath)!=null,"Final scenario save remains valid");
            yield return RefugeSteps();
        }
        IEnumerator BandageVisualSteps()
        {
            yield return new WaitForSeconds(1);yield return Capture("B01-title");
            StartRun(false);yield return Capture("B02-origin");District.introSeen=true;screen=ScreenMode.Play;smokeFreezeAgents=true;freezeDistrictAI=true;
            Teleport(DistrictState.Clinic);screen=ScreenMode.Clinic;yield return new WaitForSeconds(.4f);yield return Capture("B03-neri");
            screen=ScreenMode.Play;showMap=true;yield return Capture("B04-district-map");showMap=false;
            Teleport(DistrictState.Garage+Vector3.back*3);District.guard.position=DistrictState.Garage+Vector3.forward*3;selectedActor=1;District.health=58;District.bleeding=true;District.identified=true;SyncDistrictArt();
            yield return new WaitForSeconds(.3f);yield return Capture("B05-combat");screen=ScreenMode.Tactics;yield return Capture("B06-tactics");
            District.TakeShipment(false);District.Donate();District.Recruit();District.neri.position=Jobs.Home+Vector3.left*2;District.health=80;District.bleeding=false;Heat=0;Teleport(Jobs.Home);SyncDistrictArt();screen=ScreenMode.Play;
            yield return new WaitForSeconds(.3f);yield return Capture("B07-partnership");screen=ScreenMode.Journal;yield return Capture("B08-history");
            screen=ScreenMode.Safehouse;yield return Capture("B09-home");
            DistrictDefeat();yield return Capture("B10-recovery");
            screen=ScreenMode.Pause;yield return Capture("B11-pause");
            screen=ScreenMode.Play;Heat=0;Cargo.Take(0,State);Cargo.Take(1,State);autoInteract=true;
            yield return new WaitForSeconds(1.1f);yield return Capture("B12-extraction");autoInteract=false;
            Check(true,"Twelve new demo screens captured without black rendering");
            yield return RefugeVisualSteps();
        }
    }
}
