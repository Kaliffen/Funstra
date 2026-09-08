using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        void DrawArmsDealer()
        {
            var a=District.arms;
            if(a==null){screen=ScreenMode.Play;return;}
            bool trading=District.sella.health>0&&!a.dealerHostile&&Heat<=0;
            Shade();Panel(245,100,1110,704);Rect(245,100,1110,4,CityArt.Amber);
            Text("MARKET COURT / SELLA / CASH $"+State.cash,280,124,1030,28,16,CityArt.Amber,FontStyle.Bold);
            Text("THE PRICE OF A GUN",278,170,1030,58,40,paper,FontStyle.Bold);
            Text(District.sella.health<=0?"Sella is down. You can stabilize her in the street.":a.dealerHostile?"I know who took my stock. I won't sell you the next round.":Heat>0?"Lose the patrol first. I'm not opening the ledger with blue coats behind you.":"I repair what the Compact says you shouldn't own. Keep it wrapped. The price buys the gun; cartridges cost extra.",282,236,1020,70,20,quiet);
            Text("WEAPON / GUNS LEFT",282,326,300,24,14,quiet,FontStyle.Bold);
            Text("BUY / NO AMMUNITION INCLUDED",565,326,340,24,14,quiet,FontStyle.Bold);
            Text("CARTRIDGES / SHOP STOCK",946,326,360,24,14,quiet,FontStyle.Bold);
            for(int kind=2;kind<=(CrewEnabled?5:4);kind++)
            {
                int k=kind;float y=365+(kind-2)*(CrewEnabled?54:70);
                Text(WeaponSpec.For(k).name+" / "+a.gunStock[k]+(a.Owns(k)?" / OWNED":""),282,y+12,265,34,20,paper,FontStyle.Bold);
                DistrictAction(a.Owns(k)?"OWNED":"BUY / $"+ArmsState.GunPrice(k),trading&&!a.Owns(k)&&a.gunStock[k]>0&&State.cash>=ArmsState.GunPrice(k),565,y,350,()=>District.BuyGun(State,k));
                DistrictAction(ArmsState.AmmoPack(k)+" ROUNDS / $"+ArmsState.AmmoPrice(k)+" / "+a.ammoStock[k]+" LEFT",trading&&a.Owns(k)&&a.ammoStock[k]>=ArmsState.AmmoPack(k)&&State.cash>=ArmsState.AmmoPrice(k),936,y,378,()=>District.BuyAmmo(State,k));
            }
            string favor=a.favorStage==0?"NO CASH? CARRY PAPERS FOR A PISTOL":a.favorStage==1&&!a.favorReceipt?"PAPERS IN BAG / TAKE THEM TO NERI":a.favorStage==1?(a.Owns(2)?"RETURN RECEIPT / 6 ROUNDS":"RETURN RECEIPT / PISTOL + 6 ROUNDS"):"FAVOR SETTLED / SELLA REMEMBERS";
            DistrictAction(favor,trading&&(a.favorStage==0&&!a.Owns(2)||a.favorStage==1&&a.favorReceipt)&&(a.Owns(2)||a.gunStock[2]>0)&&a.ammoStock[2]>=6,282,584,715,()=>{if(a.favorStage==0){District.BeginArmsFavor();Notify("Deliver the shipping papers to Neri, then return the receipt.");}else RedeemArmsFavorAtDealer();});
            DistrictAction("STEAL PISTOL",!a.Owns(2)&&a.gunStock[2]>0,1014,584,300,()=>
            {
                bool witnessed=ArmsTheftWitness();
                if(District.TakeDealerGun(2,witnessed)){if(witnessed)RaiseAlarm("Sella saw the theft and called the patrol. Trading is closed to you.",Player.position);Notify("Pistol taken without rounds. The courier carries ammunition.");screen=ScreenMode.Play;}
            });
            Text("SUPPLY "+a.supplyCycle+" / 3  |  "+SupplyStatus+"\n1 conceals / 2 pistol / 3 shotgun / 4 SMG"+(CrewEnabled?" / 5 rifle":"")+" / R reload. Guns are sold without cartridges.",282,654,1025,54,16,quiet);
            DistrictAction("BACK TO MARKET COURT",true,282,733,1032,()=>screen=ScreenMode.Play);
        }
        void DrawArmsSupply()
        {
            var a=District.arms;
            DistrictPanel("TOMAS / CONSIGNMENT "+a.supplyCycle+" OF 3","TWO ADDRESSES",(a.armsInTransit?"Sella: 12 pistol rounds, 2 shells, 24 SMG rounds. ":"Sella's portion is already unloaded. ")+(a.medicineInTransit?"Neri: two medical doses. The parcels travel with Tomas; they are not on either shelf yet.":"The clinic's parcel is already unloaded."));
            Text(SupplyStatus+"\nIf you take these, Sella loses ammunition and the clinic loses medicine. Tomas can report a theft he sees. Stolen medicine stays in your bag until delivered or sold.",395,421,805,130,20,paper);
            DistrictAction("TAKE THE REMAINING CONSIGNMENT",a.armsInTransit||a.medicineInTransit,395,589,805,()=>{DivertSupplyAtPlayer();screen=ScreenMode.Play;});
            DistrictAction("LET TOMAS THROUGH",true,395,680,805,()=>screen=ScreenMode.Play);
        }
    }
}
