using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class DistrictState
    {
        public DistrictActor sella=new DistrictActor("sella","SELLA",new Vector3(-35,0,-27)){ammo=0};
        public DistrictActor supplyCourier=new DistrictActor("tomas","TOMAS / SUPPLY",new Vector3(-39,0,-48)){ammo=0};
        public int supplyStage;
        public float supplyWait=8;
        public bool ArmsRuntimeValid()=>arms==null||supplyStage>=0&&supplyStage<=3&&ProjectileMath.Finite(supplyWait)&&supplyWait>=0&&supplyWait<=30&&
            (sella==null||ProjectileMath.Finite(sella.position)&&ProjectileMath.Finite(sella.health)&&sella.health>=0&&sella.health<=100)&&
            (supplyCourier==null||ProjectileMath.Finite(supplyCourier.position)&&ProjectileMath.Finite(supplyCourier.health)&&supplyCourier.health>=0&&supplyCourier.health<=100);
    }

    public sealed partial class FunstraGame
    {
        public static readonly Vector3 ArmsDealerPosition=new Vector3(-35,0,-27);
        public static readonly Vector3 ArmsFavorPosition=DistrictState.Clinic;
        public static readonly Vector3 SupplyDepotPosition=new Vector3(-39,0,-48);
        Transform dealerBody,supplyBody;
        GameObject supplyPack;
        readonly List<Vector3> supplyPath=new List<Vector3>();
        float supplyRepath;
        bool supplyBlocked;
        public Vector3 SupplyPosition=>District.supplyCourier.position;
        public Vector3 SupplyNextPoint=>supplyPath.Count>0?supplyPath[0]:SupplyPosition;
        bool ArmsEnabled=>DistrictEnabled&&District.arms!=null&&!FoundationMode;
        public string SupplyStatus=>!ArmsEnabled?"":District.supplyCourier.health<=0?"TOMAS DOWN / DELIVERY STOPPED":supplyBlocked?"COURIER WAITING / PATH BLOCKED":District.supplyStage==1?"AMMUNITION TO SELLA":District.supplyStage==2?"MEDICINE TO NERI":District.supplyStage==3?"COURIER RETURNING":District.arms.supplyCycle>=3?"DEPOT EMPTY / 3 CONSIGNMENTS SENT":"NEXT CONSIGNMENT IN "+Mathf.CeilToInt(District.supplyWait)+"s";
        string ArmsObjective=>District.arms.favorStage==1?(District.arms.favorReceipt?"Return Neri's signed receipt to Sella for your pistol and six rounds.":"Take Sella's shipping papers to Neri inside the clinic. E delivers them."):!District.arms.Owns(2)&&!District.arms.Owns(3)&&!District.arms.Owns(4)?(District.arms.favorStage==2?"Disarmed: cargo and Mara can fund another pistol. Sella has finite stock; fallen gunmen carry recoverable equipment.":"Visit Sella in Market Court. A safe delivery favor can earn your first gun."):"1 conceals your gun. Sella sells finite ammunition; the courier brings three consignments.";

        public void InitializeArmsRuntime()
        {
            supplyPath.Clear();supplyRepath=0;supplyBlocked=false;
            if(dealerBody)dealerBody.gameObject.SetActive(ArmsEnabled);
            if(supplyBody)supplyBody.gameObject.SetActive(ArmsEnabled);
            if(!ArmsEnabled)return;
            if(District.sella==null)District.sella=new DistrictActor("sella","SELLA",ArmsDealerPosition){ammo=0};
            if(District.supplyCourier==null)District.supplyCourier=new DistrictActor("tomas","TOMAS / SUPPLY",SupplyDepotPosition){ammo=0};
            District.sella.position=City.Nav.SafePoint(ArmsDealerPosition);
            District.supplyCourier.position=City.Nav.SafePoint(District.supplyCourier.position);
            if(!dealerBody)
            {
                dealerBody=City.Human("Sella / unlicensed dealer",District.sella.position,CityArt.Hex("BC9165"));
                City.Box("Sella's ledger",new Vector3(.4f,.95f,.22f),new Vector3(.3f,.1f,.4f),CityArt.Amber,dealerBody);
                City.Sign("SELLA / REPAIRS & SUPPLIES",ArmsDealerPosition+new Vector3(0,2.65f,0),CityArt.Amber,.13f);
                City.Ring("Sella / trade",ArmsDealerPosition,1.15f,CityArt.Amber);
                supplyBody=City.Human("Tomas / finite supply courier",SupplyPosition,CityArt.Hex("788D84"));
                supplyPack=City.Box("Consignment / ammunition and medicine",new Vector3(0,1,-.38f),new Vector3(.7f,.85f,.5f),CityArt.Hex("AD8A60"),supplyBody);
                City.Box("Clinic parcel stripe",new Vector3(0,1,-.65f),new Vector3(.5f,.16f,.02f),medical,supplyBody);
                City.Sign("FREIGHT / 3 CONSIGNMENTS",SupplyDepotPosition+new Vector3(0,2.8f,0),CityArt.Amber,.13f);
            }
            District.guard.weaponRecoverable=!District.guard.looted;
            foreach(var a in Agents)if(a.Record!=null)a.Record.weaponRecoverable=a.Police&&!a.Record.looted;
            if(yardSquad!=null)foreach(var m in yardSquad.Members)m.Actor.weaponRecoverable=!m.Actor.looted;
            RegisterCombatActor(District.sella,dealerBody);RegisterCombatActor(District.supplyCourier,supplyBody);
            CombatActorHit-=ArmsActorHit;CombatActorHit+=ArmsActorHit;
            if(District.supplyStage==0&&(District.arms.armsInTransit||District.arms.medicineInTransit))District.supplyStage=District.arms.armsInTransit?1:2;
            SyncArmsArt();
        }
        void ArmsActorHit(DistrictActor actor,string owner)
        {
            if(ArmsEnabled&&owner=="player"&&actor==District.sella)
            {District.arms.dealerHostile=true;District.Record("dealer attacked","sella","Sella refuses trade after you attacked her.");}
        }
        void SyncArmsArt()
        {
            if(!ArmsEnabled||!dealerBody)return;
            dealerBody.position=District.sella.position;PoseActor(dealerBody,District.sella);
            dealerBody.rotation=Quaternion.Euler(0,Mathf.FloorToInt(District.clock/10)%2==0?90:0,0);
            supplyBody.position=SupplyPosition;PoseActor(supplyBody,District.supplyCourier);
            supplyPack.SetActive(District.arms.armsInTransit||District.arms.medicineInTransit);
        }
        public void ObserveArmsPossession()
        {
            if(CrewEnabled)
            {
                if(!ArmsEnabled||Heat>0)return;
                foreach(string id in new[]{"player","neri","rell"})if(IsCrewId(id)&&CrewAlive(id)&&CrewGunDrawn(id))
                    foreach(var officer in Agents)if(officer.Police&&officer.Record!=null&&ActorSees(officer.Record,officer.Body,CrewPosition(id),12))
                    {
                        District.Record("illegal possession",officer.Record.id,"An officer saw "+id.ToUpper()+" carrying a drawn firearm.");
                        RaiseAlarm("Illegal gun spotted on "+id.ToUpper()+". Conceal it and break sight.",CrewPosition(id));
                        officer.Pursuing=true;officer.LastSeen=CrewPosition(id);officer.Repath=0;return;
                    }
                return;
            }
            if(!ArmsEnabled||weapon==1||Heat>0||District.health<=0)return;
            foreach(var a in Agents)
            {
                if(!a.Police||a.Record==null||!ActorSees(a.Record,a.Body,Player.position,12))continue;
                District.Record("illegal possession",a.Record.id,"An officer saw your drawn firearm. Concealed inventory was not inspected.");
                RaiseAlarm("Illegal gun spotted. 1 conceals / break sight. Arrest confiscates weapons.",Player.position);
                a.Pursuing=true;a.LastSeen=Player.position;a.Repath=0;break;
            }
        }
        public void StepArms(float dt)
        {
            if(!ArmsEnabled||dt<=0)return;
            ObserveArmsPossession();SyncArmsArt();
            var a=District.arms;var courier=District.supplyCourier;
            if(courier.bleeding)courier.health=Mathf.Max(0,courier.health-dt*.35f);
            if(District.sella.bleeding)District.sella.health=Mathf.Max(0,District.sella.health-dt*.35f);
            if(courier.health<=0)return;
            if(District.supplyStage==0)
            {
                District.supplyWait=Mathf.Max(0,District.supplyWait-dt);
                if(District.supplyWait<=0&&District.DispatchSupply()){District.supplyStage=1;supplyPath.Clear();supplyRepath=0;Save();}
                return;
            }
            Vector3 target=District.supplyStage==1?ArmsDealerPosition+Vector3.right*2:District.supplyStage==2?ArmsFavorPosition:SupplyDepotPosition;
            if(Vector3.Distance(courier.position,target)<.7f)
            {
                if(District.supplyStage==1)
                {
                    if(District.sella.health<=0){supplyBlocked=true;return;}
                    District.DeliverArmsSupply();District.supplyStage=2;
                }
                else if(District.supplyStage==2)
                {
                    if(District.neri.health<=0){supplyBlocked=true;return;}
                    District.DeliverClinicSupply();District.supplyStage=3;
                }
                else{District.supplyStage=0;District.supplyWait=30;}
                supplyPath.Clear();supplyRepath=0;Save();return;
            }
            supplyRepath-=dt;
            if(supplyPath.Count==0||supplyRepath<=0)
            {supplyPath.Clear();supplyPath.AddRange(City.Nav.Find(courier.position,target));supplyRepath=2;}
            while(supplyPath.Count>0&&Vector3.Distance(courier.position,supplyPath[0])<.2f)supplyPath.RemoveAt(0);
            if(supplyPath.Count==0){supplyBlocked=true;return;}
            Vector3 next=Vector3.MoveTowards(courier.position,supplyPath[0],2.8f*dt);
            supplyBlocked=!City.Nav.ClearWalk(courier.position,next)||CourierApproachesPerson(Player.position,next,1.2f);
            foreach(var person in Agents)if(CourierApproachesPerson(person.Position,next,1.05f))supplyBlocked=true;
            foreach(var person in new[]{District.neri,District.guard,District.collector,District.sella})if(CourierApproachesPerson(person.position,next,.85f)&&Vector3.Distance(person.position,target)>.8f)supplyBlocked=true;
            if(!supplyBlocked)
            {
                Vector3 delta=next-courier.position;
                if(delta.sqrMagnitude>.000001f)supplyBody.rotation=Quaternion.LookRotation(delta);
                courier.position=next;CityArt.Animate(supplyBody,District.clock,2.8f);
            }
            SyncArmsArt();
        }
        bool CourierApproachesPerson(Vector3 person,Vector3 next,float gap)
        {
            // Deliveries end beside the recipient. Permit moving away from an existing
            // close overlap, while refusing a step further into any person's space.
            float future=Vector3.Distance(person,next);
            return future<gap&&future<=Vector3.Distance(person,SupplyPosition)+.0001f;
        }
        bool ArmsReach(Vector3 point,float distance=3)=>Vector3.Distance(Player.position,point)<distance&&CanReachPerson(point);
        public bool OpenArmsDealer()
        {
            if(!ArmsEnabled||!ArmsReach(ArmsDealerPosition))return false;
            screen=ScreenMode.Dealer;return true;
        }
        public bool CompleteArmsFavorAtClinic()
        {
            if(!ArmsEnabled||!ArmsReach(ArmsFavorPosition,3.2f)||!ArmsReach(District.neri.position,3.2f)||District.neri.health<=0||!District.CollectArmsReceipt())return false;
            Save();Notify("Neri signed Sella's papers. Return the receipt to Market Court.");return true;
        }
        public bool RedeemArmsFavorAtDealer()
        {
            if(!ArmsEnabled||!ArmsReach(ArmsDealerPosition)||District.sella.health<=0||!District.CompleteArmsFavor())return false;
            Save();Notify("Sella: Keep it wrapped. Six rounds go quickly. R loads; 1 keeps it concealed.");return true;
        }
        bool ArmsTheftWitness()=>District.sella.health>0&&ActorSees(District.sella,dealerBody,Player.position,9);
        public bool DivertSupplyAtPlayer()
        {
            if(!ArmsEnabled||!ArmsReach(SupplyPosition))return false;
            bool saw=District.supplyCourier.health>0&&ActorSees(District.supplyCourier,supplyBody,Player.position,9);
            bool taken=District.DivertArmsSupply(saw);taken|=District.DivertClinicSupply(false);
            if(!taken)return false;
            if(saw)RaiseAlarm("Tomas reported the stolen consignment. The clinic and Sella get none.",Player.position);
            Save();Notify("Consignment taken. Carried doses can go to Neri or be sold through Mara.");return true;
        }
        bool UpdateArmsInteraction(bool pressed)
        {
            if(!ArmsEnabled)return false;
            foreach(var actor in new[]{District.sella,District.supplyCourier})
                if(actor.health<=0&&ArmsReach(actor.position,2.6f))
                {
                    if(actor==District.supplyCourier&&(District.arms.armsInTransit||District.arms.medicineInTransit))
                    {prompt="E / INSPECT TOMAS'S DROPPED CONSIGNMENT";if(pressed)screen=ScreenMode.Supply;return true;}
                    prompt="E / STABILIZE "+actor.name+" / 1 BANDAGE";if(pressed&&District.bandages>0){District.bandages--;actor.health=30;actor.bleeding=false;Save();}return true;
                }
            RefreshCombatTargets();
            foreach(var t in combatTargets)
                if(t.actor.health<=0&&t.actor.weaponRecoverable&&!t.actor.looted&&ArmsReach(t.actor.position,2.6f))
                {
                    prompt="E / RECOVER "+t.actor.name+"'S GUN & AMMO";
                    if(pressed&&District.RecoverWeapon(t.actor)){Save();Notify("Recovered a finite weapon and remaining ammunition. 1 keeps it concealed.");}
                    return true;
                }
            foreach(var t in combatTargets)
                if(t.actor.health<=0&&t.actor.looted&&ArmsReach(t.actor.position,2.6f))
                {prompt="E / STABILIZE "+t.actor.name+" / 1 BANDAGE";if(pressed&&District.bandages>0){District.bandages--;t.actor.health=30;t.actor.bleeding=false;PoseActor(t.body,t.actor);Save();}return true;}
            if(District.arms.favorStage==1&&!District.arms.favorReceipt&&District.neri.health>0&&ArmsReach(District.neri.position,3.2f)&&ArmsReach(ArmsFavorPosition,3.2f))
            {prompt="E / DELIVER SELLA'S PAPERS TO NERI";if(pressed)CompleteArmsFavorAtClinic();return true;}
            if(District.arms.divertedMedicine>0&&ArmsReach(Jobs.Mara,3.2f))
            {prompt="E / SELL "+District.arms.divertedMedicine+" DIVERTED DOSES / $"+(District.arms.divertedMedicine*20);if(pressed&&District.SellDivertedMedicine(State)){Save();Notify("Mara bought the courier's medicine. The clinic received none.");}return true;}
            if(District.arms.divertedMedicine>0&&District.neri.health>0&&ArmsReach(District.neri.position,3.2f)&&ArmsReach(ArmsFavorPosition,3.2f))
            {prompt="E / GIVE "+District.arms.divertedMedicine+" DIVERTED DOSES TO NERI";if(pressed&&District.DonateDivertedMedicine()){Save();Notify("Neri stocked the carried doses. The clinic can use them now.");}return true;}
            if(ArmsReach(ArmsDealerPosition))
            {prompt="E / SELLA / WEAPONS, FAVORS & SUPPLY";if(pressed)OpenArmsDealer();return true;}
            if((District.arms.armsInTransit||District.arms.medicineInTransit)&&ArmsReach(SupplyPosition))
            {prompt="E / TOMAS / INSPECT CONSIGNMENT";if(pressed)screen=ScreenMode.Supply;return true;}
            return false;
        }
    }
}
