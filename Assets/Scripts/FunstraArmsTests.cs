using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        [Serializable] sealed class ArmsRouteRecord
        {
            public string method="Guided controller acquisition; explicit purchase, theft, combat, possession and recovery fixtures; accelerated production courier steps on real navigation. Not human play.";
            public string cpu,gpu;
            public int width,height,frameCap;
            public List<ArmsRouteSnapshot> snapshots=new List<ArmsRouteSnapshot>();
        }
        [Serializable] sealed class ArmsRouteSnapshot
        {
            public string label,armsJson,supplyStatus;
            public int cash,health,clinicStock,totalMedicine,weapon,ammo,magazine,supplyStage;
            public Vector3 player,courier;
        }
        ArmsRouteRecord armsRouteRecord;
        void RecordArmsRoute(string label)
        {
            armsRouteRecord.snapshots.Add(new ArmsRouteSnapshot{label=label,armsJson=JsonUtility.ToJson(District.arms),
                cash=State.cash,health=Mathf.CeilToInt(District.health),clinicStock=District.clinicStock,totalMedicine=District.TotalMedicine,
                weapon=District.equippedWeapon,ammo=CombatTotalAmmo,magazine=CurrentWeapon.magazine,
                player=Player.position,courier=SupplyPosition,supplyStage=District.supplyStage,supplyStatus=SupplyStatus});
            File.WriteAllText(Path.Combine(evidencePath,"arms-route-evidence.json"),JsonUtility.ToJson(armsRouteRecord,true));
        }
        void ArmsFixtureFresh()
        {
            StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;autoMove=null;showMap=false;
        }
        IEnumerator ArmsSteps()
        {
            yield return new WaitForSeconds(1);
            yield return Capture("A00-title-debug-toggle");
            Check(Smoke&&!FoundationMode&&DistrictEnabled&&muteTests&&AudioListener.volume==0,"Arms route uses isolated normal campaign and forced mute");
            armsRouteRecord=new ArmsRouteRecord{cpu=SystemInfo.processorType,gpu=SystemInfo.graphicsDeviceName,width=Screen.width,height=Screen.height,frameCap=Application.targetFrameRate};
            smokeResults.Add("METHOD: fresh unarmed start; production controller visits Sella, collects clinic receipt and returns. Menu transactions call the same actions after reaching the interaction. Yard squad is suspended during this acquisition route; citizens, traffic and other normal Update systems run.");
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;yardSquad=null;
            smokeFreezeAgents=false;freezeDistrictAI=false;
            Check(District.arms!=null&&District.arms.ownedMask==0&&District.ammo==0&&District.shotgunAmmo==0&&District.smgAmmo==0&&District.equippedWeapon==1,"New arms campaign starts unarmed with no inherited ammunition");
            SelectCombatWeapon(4);
            Check(District.equippedWeapon==1&&!FirePlayerAt(Player.position+Vector3.forward*10),"Unowned SMG cannot be selected or fired");
            screen=ScreenMode.Tactics;yield return Capture("A01-unarmed-start");screen=ScreenMode.Play;
            Check(!OpenArmsDealer(),"Dealer cannot be opened from outside interaction range");
            yield return Travel(ArmsDealerPosition);
            Check(OpenArmsDealer()&&screen==ScreenMode.Dealer,"Controller reaches Sella and opens the arms dealer");
            yield return Capture("A02-dealer-offer");
            int pistolStock=District.arms.gunStock[2],roundStock=District.arms.ammoStock[2];
            Check(District.BeginArmsFavor()&&!District.BeginArmsFavor(),"Dealer favor begins once without accepting a Mara job");
            Check(!RedeemArmsFavorAtDealer(),"Favor cannot pay out before its receipt is collected");
            screen=ScreenMode.Play;yield return Travel(ArmsFavorPosition);
            Check(CompleteArmsFavorAtClinic(),"Controller reaches clinic and collects the actual favor receipt");
            Check(!District.arms.Owns(2)&&!RedeemArmsFavorAtDealer(),"Collecting the receipt does not grant a remote weapon reward");
            screen=ScreenMode.Tactics;yield return Capture("A03-clinic-receipt");screen=ScreenMode.Play;
            yield return Travel(ArmsDealerPosition);Check(OpenArmsDealer(),"Controller returns to Sella to redeem the receipt");
            pistolStock=District.arms.gunStock[2];roundStock=District.arms.ammoStock[2];
            Check(RedeemArmsFavorAtDealer(),"Returned receipt grants the dealer's stocked pistol and six rounds");
            Check(District.arms.Owns(2)&&District.ammo==6&&District.arms.gunStock[2]==pistolStock-1&&District.arms.ammoStock[2]==roundStock-6,"Favor transfers a real gun and ammunition from dealer custody");
            int rewardedAmmo=District.ammo,rewardedStock=District.arms.gunStock[2];
            Check(!RedeemArmsFavorAtDealer()&&District.ammo==rewardedAmmo&&District.arms.gunStock[2]==rewardedStock,"Repeated favor redemption cannot duplicate goods");
            Check(!State.accepted&&State.completed==0&&District.arms.dealerTrust>0,"Pistol acquisition earns dealer trust without any Mara job progression");
            SelectCombatWeapon(2);yield return Capture("A04-favor-pistol");
            Save();StartRun(true);screen=ScreenMode.Pause;yardSquad=null;
            Check(District.arms.Owns(2)&&District.ammo==rewardedAmmo&&District.arms.favorStage==2&&!State.accepted,"Acquired pistol, remaining ammunition and completed favor survive reload");
            Check(District.Valid(),"Favor campaign remains valid after controller acquisition and reload");RecordArmsRoute("controller-favor-complete");
            screen=ScreenMode.Tactics;showMap=true;yield return Capture("A04b-map-dealer-ivo-garage");showMap=false;screen=ScreenMode.Pause;
            yield return ArmsCommerceAndCombatSteps();
            yield return ArmsPossessionAndRecoverySteps();
            yield return ArmsSupplySteps();
        }
        IEnumerator ArmsCommerceAndCombatSteps()
        {
            smokeResults.Add("METHOD: alternate purchase/theft and SMG tests use fresh paused fixtures, explicit cash, staged firing position and production transactions/combat steps. These are alternatives to the controller favor route, not organically earned shopping money or human aiming.");
            ArmsFixtureFresh();State.cash=1000;
            int cash=State.cash,stock=District.arms.gunStock[4];
            Check(District.BuyGun(State,4)&&State.cash==cash-ArmsState.GunPrice(4)&&District.arms.gunStock[4]==stock-1&&District.smgAmmo==0,"SMG purchase moves one stocked gun and cash without inventing included rounds");
            cash=State.cash;
            Check(!District.BuyGun(State,4)&&State.cash==cash,"Owned SMG cannot be purchased and charged twice");
            Check(District.BuyAmmo(State,4)&&District.BuyAmmo(State,4),"Two finite SMG ammunition packs can be purchased separately");
            Check(District.smgAmmo==2*ArmsState.AmmoPack(4)&&District.arms.dealerMoney==ArmsState.GunPrice(4)+2*ArmsState.AmmoPrice(4),"Ammunition and dealer receipts match the actual purchase transactions");
            Teleport(ArmsDealerPosition);Check(OpenArmsDealer(),"Purchased SMG and remaining dealer stock can be inspected in the real menu");
            yield return Capture("A05-purchased-smg");screen=ScreenMode.Pause;
            Teleport(new Vector3(-45,0,-45));InitializeCombat();SelectCombatWeapon(4);
            int initial=District.smgAmmo;Vector3 aim=Player.position+Vector3.right*15;
            if(CurrentWeapon.magazine==0){Check(ReloadPlayer(),"Purchased loose SMG ammunition requires a real first reload");StepCombat(WeaponSpec.SMG.reload);}
            Check(CurrentWeapon.magazine==WeaponSpec.SMG.magazine,"SMG loads its real finite magazine from purchased rounds");
            for(int i=0;i<WeaponSpec.SMG.magazine;i++)
            {
                Check(FirePlayerAt(aim),"SMG burst emits finite projectile "+(i+1));
                if(i==0)
                {
                    Check(ActiveProjectileCount>0,"SMG first shot exists in flight before any explicit combat step");
                    int count=ActiveProjectileCount,ammo=District.smgAmmo,magazine=CurrentWeapon.magazine;
                    Vector3 bullet=District.projectiles[0].position;
                    Save();StartRun(true);screen=ScreenMode.Pause;smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;
                    Check(District.arms.Owns(4)&&District.smgAmmo==ammo&&CurrentWeapon.magazine==magazine&&ActiveProjectileCount==count&&Vector3.Distance(District.projectiles[0].position,bullet)<.001f,"Save/reload preserves SMG ownership, spent round, partial magazine and in-flight projectile");
                }
                StepCombat(WeaponSpec.SMG.interval+.01f);yield return null;
            }
            Check(District.smgAmmo==initial-WeaponSpec.SMG.magazine&&CurrentWeapon.magazine==0&&District.arms.spent[4]==WeaponSpec.SMG.magazine,"SMG burst accounts for every round and stops at its magazine capacity");
            Check(!FirePlayerAt(aim)&&District.smgAmmo==initial-WeaponSpec.SMG.magazine,"Empty SMG magazine cannot spend reserve without a reload");
            Check(ReloadPlayer(),"SMG begins a timed reload from finite reserve");StepCombat(.4f);
            screen=ScreenMode.Tactics;yield return Capture("A06-smg-reload");screen=ScreenMode.Pause;
            float reload=CurrentWeapon.reloadRemaining;int reserve=District.smgAmmo;
            Save();StartRun(true);screen=ScreenMode.Pause;smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;
            Check(CurrentWeapon.reloadRemaining>0&&Mathf.Abs(CurrentWeapon.reloadRemaining-reload)<.001f&&District.smgAmmo==reserve,"Mid-reload save restores SMG progress without extra reserve");
            StepCombat(WeaponSpec.SMG.reload);
            Check(CurrentWeapon.reloadRemaining==0&&CurrentWeapon.magazine==reserve&&District.smgAmmo==reserve,"Completed reload transfers remaining reserve into magazine without creating rounds");
            while(District.smgAmmo>0){Check(FirePlayerAt(aim),"Reloaded SMG spends its remaining finite round");StepCombat(WeaponSpec.SMG.interval+.01f);yield return null;}
            Check(!FirePlayerAt(aim)&&!ReloadPlayer()&&CurrentWeapon.magazine==0&&District.arms.spent[4]==initial,"Exhausted SMG cannot fire, reload or refill its exhausted finite supply");
            Check(District.Valid(),"SMG campaign remains valid after exhaustion and save/reload");RecordArmsRoute("smg-purchase-fire-reload-exhaustion");

            ArmsFixtureFresh();Check(District.TakeDealerGun(3,false),"Unwitnessed theft fixture transfers one real shotgun");
            Check(District.arms.Owns(3)&&!District.arms.dealerHostile&&District.arms.dealerTrust==0,"Unwitnessed theft does not invent dealer identification");RecordArmsRoute("unwitnessed-theft");
            ArmsFixtureFresh();Check(District.TakeDealerGun(3,true),"Witnessed theft fixture transfers one real shotgun");
            State.cash=1000;
            Check(District.arms.dealerHostile&&District.arms.dealerTrust<0&&!District.BuyGun(State,4)&&!District.BuyAmmo(State,3),"Identified theft changes dealer relationship and closes subsequent trade");
            Teleport(ArmsDealerPosition);OpenArmsDealer();yield return Capture("A07-dealer-after-theft");screen=ScreenMode.Pause;
            Save();StartRun(true);screen=ScreenMode.Pause;yardSquad=null;
            Check(District.arms.dealerHostile&&District.arms.Owns(3)&&!District.BuyGun(State,4),"Dealer recognition and the stolen weapon survive reload");RecordArmsRoute("witnessed-theft");
        }
        IEnumerator ArmsPossessionAndRecoverySteps()
        {
            smokeResults.Add("METHOD: possession sightlines use a paused player/patrol placement and the production observation hook; downed-actor recovery directly stages a casualty with five actual rounds. No human stealth or combat victory is claimed by these fixtures.");
            ArmsFixtureFresh();State.cash=1000;Check(District.BuyGun(State,4),"Possession fixture owns its SMG through a real purchase");
            Teleport(new Vector3(20,0,-13));var patrol=Agents[0];patrol.Body.position=new Vector3(12,0,-13);patrol.Record.position=patrol.Position;patrol.Body.LookAt(Player.position);
            Check(City.Nav.Sight(patrol.Position,Player.position),"Possession fixture provides a real unobstructed patrol sightline");
            SelectCombatWeapon(1);ObserveArmsPossession();
            Check(Heat==0&&!Police.identifiedGunman,"Concealed owned gun with fists selected does not trigger visible-possession pursuit");
            SelectCombatWeapon(4);ObserveArmsPossession();
            Check(Heat>0&&!Police.identifiedGunman&&!Police.trucks[0].requested&&!Police.trucks[1].requested,"Openly carried illegal gun causes observed pursuit without inventing witnessed shooting or truck escalation");
            screen=ScreenMode.Tactics;yield return Capture("A08-open-possession");screen=ScreenMode.Pause;RecordArmsRoute("open-versus-concealed-possession");

            ArmsFixtureFresh();var fallen=Agents[0].Record;fallen.health=0;fallen.ammo=5;fallen.weaponRecoverable=true;fallen.combat=new WeaponState();fallen.combat.Initialize(5);
            Check(District.RecoverWeapon(fallen)&&District.arms.Owns(2)&&District.ammo==5&&fallen.ammo==0&&fallen.looted,"Downed officer's pistol and five remaining rounds transfer once from actual custody");
            Check(!District.RecoverWeapon(fallen)&&District.ammo==5,"Repeated recovery cannot clone a downed actor's weapon inventory");
            int priorArrests=State.arrests;Busted();
            Check(State.arrests==priorArrests+1,"Production arrest invokes weapon confiscation and ordinary recovery");
            Check(District.equippedWeapon==1&&District.arms.ownedMask==0&&District.ammo==0&&District.arms.confiscated[2]==5,"Confiscation records the remaining rounds and returns the player to fists");
            Save();StartRun(true);screen=ScreenMode.Pause;yardSquad=null;
            Check(District.arms.ownedMask==0&&District.ammo==0&&District.arms.confiscated[2]==5&&District.Valid(),"Reload preserves confiscated inventory rather than granting the old starting weapons");RecordArmsRoute("recovered-then-confiscated");
        }
        void ArmsSupplyStep()
        {
            // Courier and street people share their production navigation. The screen is
            // paused so only these explicit .05-second steps advance the fixture.
            foreach(var person in Agents)person.Step(this,.05f);
            StepArms(.05f);
        }
        IEnumerator ArmsCourierCapture(string name)
        {
            // Reframe the paused fixture for readable evidence; this does not count as
            // controller travel and cannot advance or unblock the paused courier.
            Vector3 original=Player.position;Teleport(SupplyPosition+Vector3.right*3);
            screen=ScreenMode.Tactics;yield return Capture(name);
            screen=ScreenMode.Pause;Teleport(original);
        }
        IEnumerator ArmsSupplyCycle(int cycle)
        {
            int steps=0;bool armsDelivered=false,clinicDelivered=false;
            while(District.arms.supplyCycle<cycle||District.supplyStage!=0||District.arms.armsInTransit||District.arms.medicineInTransit)
            {
                if(++steps>3600)throw new Exception("Physical supply cycle "+cycle+" did not complete: "+SupplyStatus+" courier="+SupplyPosition+" next="+SupplyNextPoint);
                ArmsSupplyStep();
                if(!City.Nav.Walkable(SupplyPosition))throw new Exception("Courier left shared walkable navigation at "+SupplyPosition);
                if(District.arms.supplyCycle==cycle&&!District.arms.armsInTransit&&!armsDelivered)
                {
                    armsDelivered=true;
                    Check(Vector3.Distance(SupplyPosition,ArmsDealerPosition+Vector3.right*2)<.85f,"Consignment "+cycle+" reaches Sella physically before ammunition leaves transit");
                    RecordArmsRoute("cycle-"+cycle+"-dealer-arrival");
                    if(cycle==1)yield return ArmsCourierCapture("A10-courier-dealer-arrival");
                }
                if(District.arms.supplyCycle==cycle&&!District.arms.medicineInTransit&&!clinicDelivered)
                {
                    clinicDelivered=true;
                    Check(Vector3.Distance(SupplyPosition,ArmsFavorPosition)<.85f,"Consignment "+cycle+" reaches clinic physically before medicine leaves transit");
                    RecordArmsRoute("cycle-"+cycle+"-clinic-arrival");
                    if(cycle==1)yield return ArmsCourierCapture("A11-courier-clinic-arrival");
                }
                if(steps%8==0)yield return null;
            }
            Check(armsDelivered&&clinicDelivered&&Vector3.Distance(SupplyPosition,SupplyDepotPosition)<.85f,"Consignment "+cycle+" delivers both loads and returns its real courier to the depot");
            smokeResults.Add("SUPPLY: cycle="+cycle+" explicitSeconds="+(steps*.05f).ToString("F2")+" status="+SupplyStatus);
        }
        IEnumerator ArmsSupplySteps()
        {
            smokeResults.Add("METHOD: supply fixture accelerates production StepArms and street-person Step in .05-second increments, eight steps per rendered frame while paused. Controller movement clears a directly staged player obstruction. Arrival captures temporarily reposition the paused player/camera beside the courier and restore it before stepping. This proves physical navigation/custody under those explicit steps, not real-time pacing or an unstaged crowd. Clinic calendar is held during deliveries; separate scheduled treatment steps inspect a later diversion shortage.");
            ArmsFixtureFresh();Teleport(new Vector3(-45,0,-45));
            int initialClinic=District.clinicStock;int[] initialAmmo=(int[])District.arms.ammoStock.Clone();
            Check(District.DispatchSupply(),"First finite depot manifest can enter transit without remotely increasing destination stock");InitializeArmsRuntime();
            Check(District.arms.ammoStock[2]==initialAmmo[2]&&District.clinicStock==initialClinic&&District.arms.armsInTransit&&District.arms.medicineInTransit,"Dispatch retains arms and medicine in transit until the physical courier arrives");
            StepArms(.05f);Vector3 direction=(SupplyNextPoint-SupplyPosition).normalized;
            Check(direction.sqrMagnitude>.5f,"Courier has a physical next navigation point for obstruction setup");
            Teleport(SupplyPosition+direction*.65f);Vector3 stopped=SupplyPosition;
            for(int i=0;i<40;i++){StepArms(.05f);if(i%8==0)yield return null;}
            Check(Vector3.Distance(stopped,SupplyPosition)<.01f&&District.arms.armsInTransit&&District.arms.medicineInTransit,"Player physically blocks courier without remote delivery or person pushing");
            screen=ScreenMode.Tactics;yield return Capture("A09-courier-blocked");screen=ScreenMode.Play;
            yield return Travel(new Vector3(-45,0,-45));screen=ScreenMode.Pause;
            for(int i=0;i<20;i++)ArmsSupplyStep();
            Check(Vector3.Distance(stopped,SupplyPosition)>.5f,"Courier resumes production movement after controller moves the player aside");
            Vector3 savedCourier=SupplyPosition;int savedStage=District.supplyStage,savedCycle=District.arms.supplyCycle;
            Save();StartRun(true);screen=ScreenMode.Pause;smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;
            Check(Vector3.Distance(savedCourier,SupplyPosition)<.01f&&District.supplyStage==savedStage&&District.arms.supplyCycle==savedCycle&&District.arms.armsInTransit&&District.arms.medicineInTransit,"Mid-transit save/reload restores courier position, route stage and both finite loads");
            yield return ArmsSupplyCycle(1);
            yield return ArmsSupplyCycle(2);
            yield return ArmsSupplyCycle(3);
            Check(District.clinicStock==initialClinic+6&&District.arms.importedMedicine==6&&District.TotalMedicine==18,"Three physical clinic deliveries add exactly six imported doses to the conserved district total");
            for(int kind=2;kind<=4;kind++)Check(District.arms.ammoStock[kind]==initialAmmo[kind]+3*ArmsState.Manifest(kind)&&District.arms.sourceAmmo[kind]==0,"Three physical deliveries conserve the finite source for weapon kind "+kind);
            Check(!District.DispatchSupply(),"Exhausted three-manifest depot refuses a fourth dispatch");
            for(int i=0;i<800;i++){ArmsSupplyStep();if(i%8==0)yield return null;}
            Check(District.supplyStage==0&&District.arms.supplyCycle==3&&!District.arms.armsInTransit&&!District.arms.medicineInTransit&&District.Valid(),"Empty depot stays exhausted through further elapsed production steps");
            screen=ScreenMode.Tactics;yield return Capture("A12-three-consignments-complete");screen=ScreenMode.Pause;RecordArmsRoute("three-physical-supply-cycles");

            ArmsFixtureFresh();int clinic=District.clinicStock,market=District.marketStock;
            int shopAmmo=District.arms.ammoStock[4];
            Check(District.DispatchSupply(),"Diversion fixture starts a real finite manifest in transit");InitializeArmsRuntime();
            Teleport(SupplyPosition+Vector3.right*2);
            Check(DivertSupplyAtPlayer(),"Nearby player interaction diverts the courier's actual consignment");
            Check(!District.arms.armsInTransit&&!District.arms.medicineInTransit&&District.smgAmmo==ArmsState.Manifest(4)&&District.arms.ammoStock[4]==shopAmmo&&District.clinicStock==clinic&&District.marketStock==market&&District.arms.divertedMedicine==2,"Diverted ammunition and medicine enter player custody while dealer and clinic receive none");
            Check(!DivertSupplyAtPlayer()&&District.arms.divertedMedicine==2,"A consignment cannot be diverted twice");
            Save();StartRun(true);screen=ScreenMode.Pause;smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;
            Check(District.arms.divertedMedicine==2&&District.smgAmmo==ArmsState.Manifest(4)&&District.Valid(),"Reload preserves the diverted goods in player custody");
            // Two ordinary scheduled treatments consume the original two clinic doses.
            // No supply mover steps run here, so another consignment cannot hide this loss.
            District.Tick(180);SyncDistrictArt();
            Check(District.clinicStock==0&&District.shortageKnown&&District.arms.divertedMedicine==2&&District.TotalMedicine==14,"With diverted doses withheld, scheduled treatments empty the clinic and expose its actual shortage");
            Teleport(ArmsFavorPosition);screen=ScreenMode.Clinic;yield return Capture("A13-diverted-supply-shortage");screen=ScreenMode.Pause;
            Check(District.Valid(),"Diversion and shortage preserve the medicine ledger");RecordArmsRoute("diverted-supply-shortage");
            ArmsFixtureFresh();District.DispatchSupply();InitializeArmsRuntime();District.supplyCourier.health=0;District.bandages=0;
            Teleport(SupplyPosition+Vector3.right*2);screen=ScreenMode.Play;
            Check(UpdateArmsInteraction(true)&&screen==ScreenMode.Supply,"Downed courier's dropped goods remain inspectable without requiring a healing bandage");
            Check(DivertSupplyAtPlayer()&&District.arms.divertedMedicine==2&&Heat==0,"Downed unwitnessing courier cannot report a theft and cargo transfers once");
            screen=ScreenMode.Pause;RecordArmsRoute("downed-courier-cargo");
        }
    }
}
