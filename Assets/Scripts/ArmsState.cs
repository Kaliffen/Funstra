using System;
using UnityEngine;

namespace Funstra
{
    // Every consignment has one finite source. Kind indexes match the shared combat model.
    [Serializable] public sealed class ArmsState
    {
        public int ownedMask, dealerMoney, dealerTrust, favorStage, supplyCycle, importedMedicine, divertedMedicine;
        public bool dealerStolen, dealerHostile, goodwillRewarded, armsInTransit, medicineInTransit, favorReceipt;
        public int[] gunStock={0,0,2,1,1}, ammoStock={0,0,36,8,48};
        public int[] sourceAmmo={0,0,36,6,72};
        public int sourceMedicine=6;
        public int[] spent=new int[5], confiscated=new int[5], recovered=new int[5];
        public bool Owns(int kind) => kind==1||GunKind(kind)&&(ownedMask&(1<<kind))!=0;
        public static bool GunKind(int kind) => kind>=2&&kind<=5;
        public static int GunPrice(int kind) => kind==2?90:kind==3?160:kind==4?240:kind==5?220:0;
        public static int AmmoPrice(int kind) => kind==2?18:kind==3?14:kind==4?30:kind==5?25:0;
        public static int AmmoPack(int kind) => kind==2?6:kind==3?2:kind==4?12:kind==5?5:0;
        public static int Manifest(int kind) => kind==2?12:kind==3?2:kind==4?24:0;
        public void RecordFired(int kind) { if(GunKind(kind)&&kind<spent.Length)spent[kind]++; }
        static bool Nonnegative(int[] values)
        { if(values==null||(values.Length!=5&&values.Length!=6))return false;foreach(int n in values)if(n<0)return false;return values[0]==0&&values[1]==0; }
        public bool Valid()
        {
            if((ownedMask&~60)!=0||dealerMoney<0||favorStage<0||favorStage>2||supplyCycle<0||supplyCycle>3||
               importedMedicine!=supplyCycle*2||sourceMedicine!=6-importedMedicine||divertedMedicine<0||divertedMedicine>importedMedicine||
               (armsInTransit||medicineInTransit)&&supplyCycle==0||favorReceipt&&favorStage==0||favorStage==2&&!favorReceipt)return false;
            if(!Nonnegative(gunStock)||!Nonnegative(ammoStock)||!Nonnegative(sourceAmmo)||
               !Nonnegative(spent)||!Nonnegative(confiscated)||!Nonnegative(recovered))return false;
            return gunStock[2]<=2&&gunStock[3]<=1&&gunStock[4]<=1&&
                sourceAmmo[2]==(3-supplyCycle)*12&&sourceAmmo[3]==(3-supplyCycle)*2&&sourceAmmo[4]==(3-supplyCycle)*24;
        }
    }

    public sealed partial class DistrictState
    {
        public ArmsState arms;
        // Unity serializes inline null classes as empty objects. Explicit schema distinguishes
        // old equipment from a new campaign that intentionally owns no firearms.
        public int armsVersion;
        public void InitializeArms(bool fresh)
        {
            if(armsVersion!=0)return;
            arms=new ArmsState();armsVersion=1;
            if(fresh)
            {
                ammo=shotgunAmmo=smgAmmo=0;equippedWeapon=1;
                pistolWeapon=new WeaponState();shotgunWeapon=new WeaponState{kind=3};smgWeapon=new WeaponState{kind=4};
            }
            else arms.ownedMask=(1<<2)|(1<<3)|(smgAmmo>0?1<<4:0);
        }
        public int ArmsAmmo(int kind) => kind==2?ammo:kind==3?shotgunAmmo:kind==4?smgAmmo:kind==5?rifleAmmo:0;
        void AddArmsAmmo(int kind,int count)
        { if(kind==2)ammo+=count;else if(kind==3)shotgunAmmo+=count;else if(kind==4)smgAmmo+=count;else if(kind==5)rifleAmmo+=count; }
        public bool BuyGun(RunState run,int kind)
        {
            if(arms==null||run==null||!ArmsState.GunKind(kind)||kind>=arms.gunStock.Length||arms.dealerHostile||arms.Owns(kind)||arms.gunStock[kind]<=0||run.cash<ArmsState.GunPrice(kind))return false;
            int price=ArmsState.GunPrice(kind);run.cash-=price;arms.dealerMoney+=price;arms.gunStock[kind]--;arms.ownedMask|=1<<kind;
            Record("arms purchase","sella","Bought an illegal "+WeaponSpec.For(kind).name+" for $"+price+". Ammunition is sold separately.");return true;
        }
        public bool BuyAmmo(RunState run,int kind)
        {
            if(arms==null||run==null||!ArmsState.GunKind(kind)||kind>=arms.ammoStock.Length||arms.dealerHostile||!arms.Owns(kind)||arms.ammoStock[kind]<ArmsState.AmmoPack(kind)||run.cash<ArmsState.AmmoPrice(kind))return false;
            int count=ArmsState.AmmoPack(kind),price=ArmsState.AmmoPrice(kind);
            run.cash-=price;arms.dealerMoney+=price;arms.ammoStock[kind]-=count;AddArmsAmmo(kind,count);
            Record("ammo purchase","sella","Bought "+count+" "+WeaponSpec.For(kind).name+" rounds for $"+price+" from Sella's finite stock.");return true;
        }
        void ArmsTheft(bool witnessed,string goods)
        {
            arms.dealerStolen=true;
            if(witnessed){arms.dealerHostile=true;arms.dealerTrust-=2;}
            Record("arms theft",witnessed?"sella":"player","Stole "+goods+"."+(witnessed?" Sella identified you and refuses further trade.":" No witness identified the thief."));
        }
        public bool TakeDealerGun(int kind,bool witnessed)
        {
            if(arms==null||!ArmsState.GunKind(kind)||kind>=arms.gunStock.Length||arms.Owns(kind)||arms.gunStock[kind]<=0)return false;
            arms.gunStock[kind]--;arms.ownedMask|=1<<kind;ArmsTheft(witnessed,WeaponSpec.For(kind).name+" from Sella; no rounds included");return true;
        }
        public bool BeginArmsFavor()
        {
            if(arms==null||arms.dealerHostile||arms.Owns(2)||arms.favorStage!=0||arms.gunStock[2]==0||arms.ammoStock[2]<6)return false;
            arms.favorStage=1;Record("arms favor","sella","Sella asks you to deliver sealed shipping papers to Neri at the clinic. Bring back the signed receipt for a stocked pistol and six rounds. No medicine is included.");return true;
        }
        public bool CollectArmsReceipt()
        {
            if(arms==null||arms.favorStage!=1||arms.favorReceipt||neri==null||neri.health<=0)return false;
            arms.favorReceipt=true;Record("arms favor","neri","Neri accepted Sella's shipping papers and signed a receipt. The clinic received no medicine.");return true;
        }
        public bool CompleteArmsFavor()
        {
            if(arms==null||arms.dealerHostile||arms.favorStage!=1||!arms.favorReceipt||!arms.Owns(2)&&arms.gunStock[2]==0||arms.ammoStock[2]<6)return false;
            bool alreadyArmed=arms.Owns(2);
            arms.favorStage=2;if(!alreadyArmed){arms.gunStock[2]--;arms.ownedMask|=1<<2;}
            arms.ammoStock[2]-=6;ammo+=6;arms.dealerTrust++;
            Record("arms favor","sella",alreadyArmed?"Returned Neri's signed receipt. You already own a pistol, so Sella handed over six rounds and kept the spare gun in stock.":"Returned Neri's signed receipt. Sella handed over one stocked pistol and six rounds.");return true;
        }
        public bool RecoverWeapon(DistrictActor actor)
        {
            if(arms==null||actor==null||actor.health>0||actor.looted||!actor.weaponRecoverable||actor.combat==null||!ArmsState.GunKind(actor.combat.kind)||actor.ammo<0)return false;
            int kind=actor.combat.kind,count=actor.ammo;if(kind>=arms.recovered.Length)return false;arms.ownedMask|=1<<kind;AddArmsAmmo(kind,count);arms.recovered[kind]+=count;
            actor.ammo=0;actor.combat.magazine=0;actor.combat.CancelReload();actor.looted=true;actor.weaponRecoverable=false;
            Record("weapon recovery",actor.id,"Recovered "+actor.name+"'s "+WeaponSpec.For(kind).name+" and "+count+" remaining rounds.");return true;
        }
        public void ConfiscateArms()
        {
            if(arms==null)return;
            arms.confiscated[2]+=ammo;arms.confiscated[3]+=shotgunAmmo;arms.confiscated[4]+=smgAmmo;
            if(arms.confiscated.Length>5)arms.confiscated[5]+=rifleAmmo;rifleAmmo=0;rifleWeapon=new WeaponState{kind=5};
            arms.ownedMask=0;ammo=shotgunAmmo=smgAmmo=0;equippedWeapon=1;
            pistolWeapon=new WeaponState();shotgunWeapon=new WeaponState{kind=3};smgWeapon=new WeaponState{kind=4};
            Record("confiscated","police","Police confiscated your illegal guns and remaining ammunition. Sella's business and the district's supply records persist.");
            if(arms.divertedMedicine>0)
            {
                int doses=arms.divertedMedicine;supplierStock+=doses;arms.divertedMedicine=0;
                Record("medicine confiscated","police","Police returned "+doses+" diverted doses from your bag to the supplier's reserve.");
            }
        }
        public bool DispatchSupply()
        {
            if(arms==null||arms.supplyCycle>=3||arms.armsInTransit||arms.medicineInTransit)return false;
            for(int kind=2;kind<=4;kind++)arms.sourceAmmo[kind]-=ArmsState.Manifest(kind);
            arms.sourceMedicine-=2;arms.importedMedicine+=2;arms.supplyCycle++;arms.armsInTransit=arms.medicineInTransit=true;
            Record("supply dispatch","supplier","Consignment "+arms.supplyCycle+" / 3 left the finite depot: 12 pistol, 2 shotgun and 24 SMG rounds for Sella; two doses for the clinic.");return true;
        }
        public bool DeliverArmsSupply()
        {
            if(arms==null||!arms.armsInTransit)return false;
            for(int kind=2;kind<=4;kind++)arms.ammoStock[kind]+=ArmsState.Manifest(kind);
            arms.armsInTransit=false;Record("supply delivered","sella","Sella received the courier's ammunition. Her shop stock has increased.");return true;
        }
        public bool DeliverClinicSupply()
        {
            if(arms==null||!arms.medicineInTransit)return false;
            arms.medicineInTransit=false;clinicStock+=2;shortageKnown=false;
            if(!arms.goodwillRewarded){arms.goodwillRewarded=true;trust++;arms.dealerTrust++;}
            Record("supply delivered","neri","The courier delivered two doses to the clinic. The first completed medicine delivery earns goodwill once.");return true;
        }
        public bool DivertArmsSupply(bool witnessed=false)
        {
            if(arms==null||!arms.armsInTransit)return false;
            for(int kind=2;kind<=4;kind++)AddArmsAmmo(kind,ArmsState.Manifest(kind));
            arms.armsInTransit=false;ArmsTheft(witnessed,"the courier's ammunition consignment");return true;
        }
        public bool DivertClinicSupply(bool witnessed=false)
        {
            if(arms==null||!arms.medicineInTransit)return false;
            arms.medicineInTransit=false;arms.divertedMedicine+=2;
            if(witnessed)trust--;
            Record("medicine diverted",witnessed?"neri":"player","Took two courier doses into your bag. The clinic received none. Carry them to Mara to sell or to Neri to donate."+(witnessed?" Neri witnessed it and lost trust.":""));return true;
        }
        public bool SellDivertedMedicine(RunState run)
        {
            if(arms==null||run==null||arms.divertedMedicine<=0||marketMoney<arms.divertedMedicine*20)return false;
            int doses=arms.divertedMedicine,price=doses*20;
            marketMoney-=price;run.cash+=price;marketStock+=doses;arms.divertedMedicine=0;
            Record("medicine sale","mara","Sold "+doses+" diverted doses to Mara for $"+price+". The medicine is now held by the market.");return true;
        }
        public bool DonateDivertedMedicine()
        {
            if(arms==null||arms.divertedMedicine<=0||neri==null||neri.health<=0)return false;
            int doses=arms.divertedMedicine;clinicStock+=doses;arms.divertedMedicine=0;shortageKnown=false;trust++;
            Record("medicine donation","neri","Delivered "+doses+" diverted doses from your bag to the clinic. Neri can use them for treatment.");return true;
        }
    }
}
