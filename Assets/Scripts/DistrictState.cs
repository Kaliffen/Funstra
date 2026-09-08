using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    [Serializable] public sealed class DistrictActor
    {
        public string id, name, order = "Hold";
        public Vector3 position;
        public float health = 100;
        public bool bleeding, looted, weaponRecoverable;
        public ResidentPersona resident;
        public int bandages;
        public int ammo=18;
        public WeaponState combat=new WeaponState();
        public DistrictActor(string id,string name,Vector3 position) { this.id=id;this.name=name;this.position=position; }
    }
    [Serializable] public sealed class DistrictIncident
    {
        public float time;
        public string kind, observer, text;
        public DistrictIncident(float time,string kind,string observer,string text) { this.time=time;this.kind=kind;this.observer=observer;this.text=text; }
    }
    [Serializable] public sealed partial class DistrictState
    {
        public static readonly Vector3 Clinic = new Vector3(-27,0,-12);
        public static readonly Vector3 Garage = new Vector3(-28,0,4);
        public static readonly Vector3 Buyer = new Vector3(-28,0,39);
        public static readonly Vector3 CollectorPost = new Vector3(-26,0,11);
        public const float SaleTime=720;
        public float clock, nextPatient=90, nextSupply=240, nextInspection=30;
        public float health=100;
        public bool bleeding, introSeen, recruited, identified, shortageKnown, discovered, released, hostile;
        public int bandages=2, ammo=12, trust, debt, treatments, consumed;
        public int clinicStock=2, supplierStock=4, marketStock;
        public int clinicMoney=20, supplierMoney, collectorMoney, buyerMoney=200, patientMoney=200, marketMoney=1000;
        public string shipmentOwner="collector";
        public int shipmentUnits=6;
        public Vector3 playerPosition=Jobs.Home;
        public PoliceResponse police=new PoliceResponse();
        public float savedHeat;
        public Vector3 savedLastSeen;
        public bool hasPosition;
        public bool metNeri;
        public int cargoTakenMask;
        public bool savedJobCarrying;
        public string recoverySummary="", recoveryDetails="";
        public string RecoverySummary => recoverySummary;
        public string RecoveryDetails => recoveryDetails;
        public Vector3 guardLastSeen;
        public float guardSawAt;
        public DistrictActor neri=new DistrictActor("neri","NERI",Clinic) { bandages=3 };
        public DistrictActor guard=new DistrictActor("rook","ROOK",new Vector3(-28,0,8)) { health=80 };
        public DistrictActor collector=new DistrictActor("ivo","IVO",CollectorPost) { health=70 };
        public List<DistrictIncident> incidents=new List<DistrictIncident>();
        public List<DistrictActor> citizens=new List<DistrictActor>();
        public bool Carrying => shipmentOwner=="player" && shipmentUnits>0;
        public Vector3 ShipmentPosition => shipmentOwner=="buyer"?Buyer:Garage;
        public int TotalMedicine => clinicStock+supplierStock+marketStock+shipmentUnits+consumed+(armsVersion==0||arms==null?0:arms.divertedMedicine+(arms.medicineInTransit?2:0));
        public string ClinicStatus => clinicStock==0?"OUT OF MEDICINE":refuge&&reserveMedicine&&clinicStock<=2?"PUBLIC CARE PAUSED / CREW RESERVE":"TREATING PATIENTS";
        public int ReleasePrice => shipmentOwner=="buyer"?140:100;
        public void Record(string kind,string observer,string text)
        { incidents.Add(new DistrictIncident(clock,kind,observer,text)); if(incidents.Count>60)incidents.RemoveAt(0); }

        public void Tick(float dt)
        {
            if(dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;
            float end=clock+dt;
            // Fixed scheduled transactions give the same result across frame rates and reloads.
            while(Mathf.Min(Mathf.Min(nextPatient,Mathf.Min(nextSupply,nextInspection)),shipmentOwner=="collector"&&!released&&buyerMoney>=100?SaleTime:float.MaxValue)<=end)
            {
                float next=Mathf.Min(Mathf.Min(nextPatient,Mathf.Min(nextSupply,nextInspection)),shipmentOwner=="collector"&&!released&&buyerMoney>=100?SaleTime:float.MaxValue);
                clock=next;
                if(nextPatient==next)
                {
                    nextPatient+=90;
                    if(clinicStock>(refuge&&reserveMedicine?2:0)&&patientMoney>=8) { clinicStock--;consumed++;patientMoney-=8;clinicMoney+=8;treatments++; }
                    else if(refuge&&reserveMedicine&&clinicStock>0) { refusedPatients++;if(refusedPatients==1)Record("policy","neri","A dockworker left untreated. Two doses remain, but you reserved them for the crew."); }
                    if(clinicStock==0&&!shortageKnown) { shortageKnown=true;Record("shortage","neri","Clinic notice: treatment suspended. The medicine shelf is empty."); }
                }
                else if(nextSupply==next)
                {
                    nextSupply+=240;
                    if(clinicStock<2&&supplierStock>=2&&clinicMoney>=30)
                    { supplierStock-=2;clinicStock+=2;clinicMoney-=30;supplierMoney+=30;shortageKnown=false;Record("supply","supplier","Clinic buys 2 doses for $30 from the supplier's reserve."); }
                }
                else if(nextInspection==next)
                {
                    nextInspection+=30;
                    if(shipmentOwner!="collector"&&shipmentOwner!="buyer"&&!released&&!discovered)
                    { discovered=true;Record("missing","ivo","Ivo's stock count finds the shipment missing. No thief identified unless witnessed."); }
                }
                else { buyerMoney-=100;collectorMoney+=100;shipmentOwner="buyer";Record("sale","ivo","The buyer paid $100. Impounded medicine moved to the north quay. Release now costs $140."); }
            }
            clock=end;
        }
        public bool PayRelease(RunState run)
        {
            if(released||identified||!(shipmentOwner=="collector"||shipmentOwner=="buyer")||run.cash<ReleasePrice)return false;
            run.cash-=ReleasePrice;
            if(shipmentOwner=="buyer")buyerMoney+=ReleasePrice;else collectorMoney+=ReleasePrice;
            released=true;Record("payment","ivo","You paid for the medicine. The shipment is cleared for collection.");return true;
        }
        public bool TakeShipment(bool witnessed)
        {
            if(shipmentUnits==0||!(shipmentOwner=="collector"||shipmentOwner=="buyer"))return false;
            shipmentOwner="player";
            Record("pickup",released?"ivo":"player",released?"Collected the released medicine.":"Took the impounded medicine. Six doses are in your bag.");
            if(witnessed&&!released)Identify("Rook witnessed you taking impounded medicine.");
            return true;
        }
        public void Identify(string reason)
        { if(!identified)Record("identified","rook",reason+" The collector remembers your face after the chase ends."); identified=true;hostile=true; }
        public bool Restitution(RunState run)
        {
            if(!identified||run.cash<60)return false;
            run.cash-=60;collectorMoney+=60;identified=hostile=false;Record("settlement","ivo","Paid $60 restitution. The collector has withdrawn Rook's standing order against you.");return true;
        }
        public bool Donate()
        {
            if(!Carrying||neri.health<=0)return false;
            clinicStock+=shipmentUnits;shipmentUnits=0;shipmentOwner="clinic";shortageKnown=false;
            trust+=3;Record("donation","neri","You supplied the clinic. Neri offers a bed, treatment and a place beside you.");return true;
        }
        public bool SellMedicine(RunState run)
        {
            if(!Carrying||marketMoney<160)return false;
            marketMoney-=160;run.cash+=160;marketStock+=shipmentUnits;shipmentUnits=0;shipmentOwner="market";
            Record("sale","player","Sold six doses through Mara for $160. The clinic received none.");return true;
        }
        public bool Recruit()
        {
            if(recruited||trust<3||neri.health<=0)return false;
            recruited=true;metNeri=true;neri.order="Follow";Record("partnership","neri","Neri joined you. Keep each other alive. G follow / H hold / R retreat / T aid.");return true;
        }
        public bool BandagePlayer()
        {
            if(bandages<=0||(!bleeding&&health>=90))return false;
            bandages--;bleeding=false;health=Mathf.Min(100,health+12);Record("stabilized","player","Used a bandage. Bleeding stopped; wounds still need rest.");return true;
        }
        public bool AidNeri()
        {
            if(bandages<=0||neri.health>=90&&!neri.bleeding)return false;
            bandages--;neri.health=Mathf.Min(100,Mathf.Max(25,neri.health)+15);neri.bleeding=false;trust++;
            Record("stabilized","neri","You stabilized Neri. They remember that you came back.");return true;
        }
        public bool NeriAid()
        {
            if(!recruited||neri.health<=0||neri.bandages==0||(!bleeding&&health>=85))return false;
            neri.bandages--;health=Mathf.Min(100,health+22);bleeding=false;
            Record("stabilized","neri","Neri used a field dressing on you.");return true;
        }
        public bool Treat(RunState run)
        {
            int price=trust>=3?0:20;
            if(clinicStock==0||neri.health<=0||run.cash<price||health>=100&&!bleeding&&neri.health>=100&&!neri.bleeding)return false;
            run.cash-=price;clinicMoney+=price;clinicStock--;consumed++;health=neri.health=100;bleeding=neri.bleeding=false;
            Record("treatment","neri","One dose and a clinic bed restored you both. Treatment used real clinic stock.");return true;
        }
        public bool RestOnCredit()
        {
            if(health>=80&&!bleeding)return false;
            debt+=40;health=Mathf.Max(80,health);bleeding=false;Record("debt","player","A rented bed and emergency care cost $40 on credit. You can work it off.");return true;
        }
        public void Defeat(RunState run,bool rescued,int lostCargoValue=0)
        {
            bool lostMedicine=Carrying, lostJob=run.carrying;
            int stolen=Mathf.Min(30,run.cash);run.cash-=stolen;collectorMoney+=stolen;
            if(Carrying) { shipmentOwner=clock>=SaleTime?"buyer":"collector";released=false;Record("confiscated","rook","Rook recovered the medicine. It can still be obtained from its owner."); }
            health=rescued?60:45;bleeding=false;hostile=false;
            if(!rescued)debt+=40;
            recoverySummary=rescued?"Neri dragged you out. You have someone to come home with.":"You woke in an emergency bed. $40 has been added to your debt.";
            var losses=new List<string>();
            if(stolen>0)losses.Add("Lost $"+stolen+" cash.");
            if(lostCargoValue>0)losses.Add("Lost unbanked cargo worth $"+lostCargoValue+". It can be collected again.");
            if(lostJob)losses.Add("Mara's job item was lost. Return to its pickup to try again.");
            if(lostMedicine)losses.Add("Lost medicine: "+(shipmentOwner=="buyer"?"the buyer holds it at the north quay.":"it is back behind Vico's. It can still be recovered."));
            if(losses.Count==0)losses.Add("You lost no cash or carried goods.");
            if(identified)losses.Add("Ivo still knows your face. Defeat does not settle that offense.");
            losses.Add("Recorded incidents, equipment and completed work remain.");
            recoveryDetails=string.Join("\n",losses);
            Record("defeat",rescued?"neri":"player",(rescued?"Neri got you home.":"Emergency care added $40 debt.")+" Cash lost: $"+stolen+"; cargo lost: $"+lostCargoValue+"."+(lostJob?" Mara's job item was lost.":""));
        }
        public bool Valid()
        {
            if(police==null)police=new PoliceResponse();
            if(!police.Valid()||armsVersion<0||armsVersion>1||armsVersion==1&&(arms==null||!ArmsRuntimeValid()||!arms.Valid()))return false;
            return DockValidation()&&CrewValid()&&ResidentsValidation()&&CombatValid()&&!float.IsNaN(clock)&&!float.IsInfinity(clock)&&clock>=0&&clock<10000000&&health>=0&&health<=100&&
                refusedPatients>=0&&clinicContributions>=0&&nextPatient>0&&nextSupply>0&&nextInspection>0&&bandages>=0&&ammo>=0&&debt>=0&&
                clinicStock>=0&&supplierStock>=0&&marketStock>=0&&shipmentUnits>=0&&consumed>=0&&TotalMedicine==12+(armsVersion==0||arms==null?0:arms.importedMedicine)&&
                (shipmentOwner=="collector"||shipmentOwner=="buyer"||shipmentOwner=="player"||shipmentOwner=="clinic"||shipmentOwner=="market")&&
                clinicMoney>=0&&supplierMoney>=0&&collectorMoney>=0&&buyerMoney>=0&&patientMoney>=0&&marketMoney>=0&&
                neri!=null&&guard!=null&&collector!=null&&incidents!=null&&neri.health>=0&&neri.health<=100&&guard.health>=0&&guard.health<=100&&collector.health>=0&&collector.health<=100;
        }
    }
}
