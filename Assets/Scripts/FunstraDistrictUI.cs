using System;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        Vector2 journalScroll;
        string TimeLeft(float seconds) => Mathf.Max(0,Mathf.FloorToInt(seconds/60)).ToString("00")+":"+Mathf.Max(0,Mathf.FloorToInt(seconds)%60).ToString("00");
        string ClockAt(float time) => ((1300+(int)(time/60))/60%24).ToString("00")+":"+((1300+(int)(time/60))%60).ToString("00")+":"+((int)time%60).ToString("00");
        string PortClock => ClockAt(District.clock).Substring(0,5);
        string ShipmentSchedule => District.shipmentOwner=="collector"&&!District.released?"BUYER DUE IN "+TimeLeft(DistrictState.SaleTime-District.clock):District.shipmentOwner=="buyer"?"BUYER HOLDS THE STOCK AT THE NORTH QUAY":District.shipmentOwner=="collector"?"RELEASED STOCK WAITS BEHIND VICO'S":"YOU CHANGED THE SHIPMENT'S DESTINATION";
        string MedicalObjective => District.refuge?"Your shared refuge is open. F at the clinic: fund, stock and care policy.":District.recruited?"A room of your own: $120 at the clinic. Ask Neri about the spare room.":District.Carrying?"Neri needs these six doses. Mara offers $160. The choice is yours.":District.shipmentUnits==0?"Your choice changed the clinic. Visit Neri and inspect the consequences.":"Meet Neri at MUTUAL CLINIC. Medicine is held in Vico's alley. TAB shows the places.";
        void DrawDistrictVitals()
        {
            Panel(28,295,389,259);Rect(28,295,4,259,medical);
            Text("YOU / "+(District.bleeding?"BLEEDING":"STABLE"),50,312,340,26,14,District.bleeding?CityArt.Red:medical,FontStyle.Bold);
            Text(Mathf.CeilToInt(District.health)+" / 100",50,343,200,35,27,paper,FontStyle.Bold);
            Text(CombatWeaponName,233,353,160,25,15,CityArt.Amber,FontStyle.Bold,TextAnchor.UpperRight);
            Rect(50,391,344,6,CityArt.Hex("344754"));Rect(50,391,344*District.health/100,6,District.bleeding?CityArt.Red:medical);
            Text("BANDAGES "+District.bandages+"   /   "+(Cargo.Weight>=5?"HEAVY: SPEED -20%":"DEBT $"+District.debt),50,410,345,26,15,Cargo.Weight>=5?CityArt.Amber:quiet);
            Text("Mouse aim   LMB fire / melee\n1 conceal   2 pistol   3 shotgun   4 SMG / R reload",50,450,345,55,15,paper);
            Text(District.identified?"IVO KNOWS YOUR FACE / $60 restitution":"NO IDENTIFIED OFFENSE WITH IVO",50,516,345,26,12,District.identified?CityArt.Red:quiet,FontStyle.Bold);
            Panel(28,567,389,170);
            Text(District.recruited?"NERI / "+District.neri.order.ToUpperInvariant():"A PLACE BESIDE YOU",50,583,345,26,14,medical,FontStyle.Bold);
            Text(District.recruited?Mathf.CeilToInt(District.neri.health)+" HP   /   "+District.neri.bandages+" dressings":District.trust<0?"Neri remembers being hurt.":"Help the clinic. Earn a partnership.",50,619,345,34,20,paper,FontStyle.Bold);
            Text(District.recruited?"G follow   H hold   SHIFT+R retreat   T aid\nE beside a downed person: stabilize":"Clinic: "+District.clinicStock+" doses / "+District.treatments+" treated\nJ history   L track Mara / clinic",50,665,345,48,15,quiet);
            if(!showMap&&screen==ScreenMode.Play)
            {
                if(District.shipmentOwner=="buyer"||District.shipmentOwner=="collector"&&!District.released)
                {Panel(1190,260,382,59);Text(District.shipmentOwner=="buyer"?"MEDICINE AT NORTH QUAY / J":"BUYER ARRIVES 21:52 / DETAILS IN J",1206,276,354,40,14,CityArt.Amber);}
                if(Vector3.Distance(Player.position,TallyPosition)<3)Text("P / PET TALLY    F / CLINIC REFUGE",458,677,693,30,16,medical,FontStyle.Bold,TextAnchor.MiddleCenter);
            }
            if(Cargo.Value>0)Text("Cargo $"+Cargo.Value+" / "+Cargo.Weight+" load: hold E at HOME to bank",457,716,693,30,15,cargoColor,FontStyle.Bold,TextAnchor.MiddleCenter);
            if(TargetActor!=null)
            {
                var sp=View.WorldToScreenPoint(TargetActor.position+Vector3.up*3.2f);
                if(sp.z>0)
                {
                    float x=sp.x/Screen.width*W,y=(1-sp.y/Screen.height)*H;
                    Panel(x-110,y-28,220,56);
                    Text(TargetActor.name+" / "+Mathf.CeilToInt(TargetActor.health)+" HP",x-100,y-17,200,28,16,CityArt.Red,FontStyle.Bold,TextAnchor.MiddleCenter);
                    Text("SELECTED",x-100,y+10,200,20,11,quiet,FontStyle.Normal,TextAnchor.MiddleCenter);
                }
            }
        }
        void DistrictMap(Rect r,bool expanded)
        {
            if(!DistrictEnabled)return;
            Vector3[] positions={DistrictState.Clinic,District.ShipmentPosition,District.collector.position,District.neri.position};
            string[] names={"MUTUAL CLINIC","MEDICINE","IVO / COLLECTOR",District.recruited?"NERI / CREW":"NERI"};
            for(int i=0;i<positions.Length;i++)
            {
                if(i==1&&(District.shipmentUnits==0||District.Carrying))continue;
                var p=MapPoint(positions[i],r);Dot(p.x,p.y,expanded?7:4,i==2?CityArt.Amber:medical);
                if(expanded&&i!=3)
                {
                    float x=r.xMax-183,y=r.y+105+i*38;
                    MapLeader(p,new Vector2(x,y+12),medical,i);
                    Rect(x,y,165,25,ink);Text(names[i],x+6,y+4,155,23,11,medical,FontStyle.Bold);
                }
            }
            if(ArmsEnabled)
            {
                var p=MapPoint(ArmsDealerPosition,r);Dot(p.x,p.y,expanded?7:4,CityArt.Amber);
                if(expanded){float x=r.xMax-183,y=r.y+240;Rect(x,y,175,27,ink);Text("SELLA / MARKET COURT",x+6,y+4,165,24,12,CityArt.Amber,FontStyle.Bold);MapLeader(p,new Vector2(x,y+12),CityArt.Amber,3);}
                var c=MapPoint(SupplyPosition,r);Dot(c.x,c.y,expanded?6:3,medical);
            }
        }
        void DistrictAction(string title,bool allowed,float x,float y,float width,Action action)
        {
            bool old=GUI.enabled;GUI.enabled=allowed;
            if(Button(title,x,y,width,49,allowed)) { action();Save();SyncDistrictArt(); }
            GUI.enabled=old;
        }
        void DistrictPanel(string eyebrow,string title,string description)
        {
            Shade();Panel(355,117,890,673);Rect(355,117,890,4,medical);
            Text(eyebrow,395,147,807,30,14,medical,FontStyle.Bold);
            Text(title,391,191,810,65,41,paper,FontStyle.Bold);
            Text(description,395,276,805,123,23,quiet);
        }
        void DrawDistrictScreen()
        {
            switch(screen)
            {
                case ScreenMode.Intro:
                    DistrictPanel("OLD PORT / THE SALT YEARS NEVER ENDED","KEEP THE LIGHTS ON","The city rebuilt itself on emergency credit. Now debt buys people's homes, work and bodies. You lost your work permit. Mara bought you one more night.");
                    Text("Neri's clinic needs medicine held by a collector. You can pay, steal, fight, or leave it alone. Nobody is waiting for you to accept a quest.",395,409,805,90,22,paper);
                    Text(District.arms!=null?"You start with two bandages. Sella in Market Court offers a pistol for a delivery favor. Civilian guns are illegal: 1 conceals, drawn guns can alert police.\nSPACE pauses / TAB map / J history. Practice guns in the title test levels.":"You carry a pistol, 12 rounds and 2 bandages. You are vulnerable.\nSPACE pauses tactics. TAB maps the district. J records what changes.\nMara's jobs and purple cargo can earn money for a peaceful approach.",395,521,805,94,19,quiet);
                    DistrictAction("STEP INTO OLD PORT",true,395,680,805,()=>{District.introSeen=true;screen=ScreenMode.Play;Notify("Find Neri inside MUTUAL CLINIC. TAB opens your map.");});break;
                case ScreenMode.Dealer: DrawArmsDealer();break;
                case ScreenMode.Supply: DrawArmsSupply();break;
                case ScreenMode.Refuge: DrawRefuge();break;
                case ScreenMode.Conversation: DrawConversation();break;
                case ScreenMode.Clinic: DrawClinic();break;
                case ScreenMode.Collector: DrawCollector();break;
                case ScreenMode.Journal: DrawJournal();break;
                case ScreenMode.Tactics:
                    Panel(458,108,693,89);Rect(458,108,4,89,medical);
                    Text("TACTICAL PAUSE / TIME IS STOPPED",478,122,650,28,18,medical,FontStyle.Bold);
                    Text("RMB select. Scroll inspect. Resume to attack. Buildings block shots.",478,158,650,30,16,paper);
                    if(District.recruited&&!CrewEnabled)
                    {
                        Panel(1190,280,382,318);
                        Text("CREW ORDERS",1210,300,340,30,22,medical,FontStyle.Bold);
                        Text("Neri: "+Mathf.CeilToInt(District.neri.health)+" HP / "+District.neri.bandages+" dressings",1210,341,340,38,16,quiet);
                        string[] orders={"Follow","Hold","Retreat","Aid"};
                        for(int i=0;i<4;i++) { string order=orders[i];DistrictAction(order.ToUpperInvariant(),true,1208+i%2*177,390+i/2*60,163,()=>OrderNeri(order)); }
                        DistrictAction("RESUME / SPACE",true,1208,530,340,()=>screen=ScreenMode.Play);
                    }
                    else if(!crewPanel)DistrictAction("RESUME / SPACE",true,1208,280,340,ResumeCrewPlay);
                    break;
                case ScreenMode.Recovery:
                    DistrictPanel("DEFEAT / YOUR STORY CONTINUES","STILL BREATHING",District.RecoverySummary);
                    Text("HEALTH "+Mathf.CeilToInt(District.health)+" / 100   |   DEBT $"+District.debt,395,407,805,30,20,medical,FontStyle.Bold);
                    Text(District.RecoveryDetails,395,451,805,195,19,paper);
                    DistrictAction("GET BACK ON YOUR FEET",true,395,680,805,()=>screen=ScreenMode.Play);break;
                case ScreenMode.MedicineSale:
                    DistrictPanel("MARA / ANOTHER KIND OF CHOICE","SIX DOSES. $160.","I can move those. Neri can't pay you this much. You need to decide what having that clinic open is worth to you.");
                    Text("Selling moves the medicine into the market. The clinic receives none.\nCurrent clinic stock: "+District.clinicStock+" doses. Money stays useful; trust may open a different door.",395,427,805,110,23,paper);
                    DistrictAction(Heat>0?"LOSE HEAT BEFORE SELLING":"SELL THE MEDICINE / $160",Heat<=0&&District.Carrying,395,604,805,()=>{District.SellMedicine(State);screen=ScreenMode.Play;Notify("Mara paid $160. The clinic received no medicine.");});
                    DistrictAction("KEEP IT",true,395,680,805,()=>screen=ScreenMode.Play);break;
            }
        }
        void DrawClinic()
        {
            bool atClinic=Vector3.Distance(Player.position,DistrictState.Clinic)<4;
            DistrictPanel("NERI / COLLEGE OF REPAIR","PEOPLE NEED THIS PLACE",District.NeriWords);
            Text("CLINIC "+District.clinicStock+" DOSES   /   "+District.treatments+" PATIENTS TREATED   /   TRUST "+District.trust+"\nSupplier reserve: "+District.supplierStock+" doses. Clinic funds: $"+District.clinicMoney+".",395,415,805,66,18,medical,FontStyle.Bold);
            DistrictAction(atClinic?"GIVE MEDICINE / EARN TRUST":"DELIVER AT THE CLINIC",atClinic&&District.Carrying,395,514,395,()=>{District.Donate();Notify("Clinic supplied. Neri trusts you. Ask them to join.");});
            DistrictAction(District.recruited?"NERI IS YOUR COMPANION":"ASK NERI TO JOIN",District.trust>=3&&!District.recruited,806,514,395,()=>{District.Recruit();Notify("You are no longer alone. G follow / H hold / R retreat / T aid.");});
            DistrictAction("REST & TREAT / "+(District.trust>=3?"TRUSTED / FREE":"$20"),atClinic&&District.clinicStock>0&&(District.trust>=3||State.cash>=20),395,578,395,()=>{if(District.Treat(State))Notify("Rested and treated. One clinic dose consumed.");});
            DistrictAction("NERI: 2 DRESSINGS / 1 DOSE",atClinic&&District.recruited&&District.clinicStock>0&&District.neri.bandages<6,806,578,395,()=>{District.clinicStock--;District.consumed++;District.neri.bandages+=2;District.Record("equipment","neri","Converted a clinic dose into two field dressings.");});
            DistrictAction("THE SPARE ROOM / F",atClinic,395,642,395,()=>screen=ScreenMode.Refuge);
            DistrictAction("ASK ABOUT THE BED",true,806,642,395,()=>{conversationIvo=false;screen=ScreenMode.Conversation;});
            DistrictAction("BACK TO THE STREET",true,395,705,805,()=>screen=ScreenMode.Play);
        }
        void DrawCollector()
        {
            DistrictPanel("IVO / HARBOR COMBINE",District.IvoTitle,District.IvoWords);
            Text("SHIPMENT: "+District.shipmentOwner.ToUpperInvariant()+" / "+District.shipmentUnits+" DOSES\n"+ShipmentSchedule+"   |   CASH $"+State.cash,395,424,805,71,20,medical,FontStyle.Bold);
            bool purchasable=District.shipmentUnits>0&&(District.shipmentOwner=="collector"||District.shipmentOwner=="buyer");
            DistrictAction("PAY RELEASE / $"+District.ReleasePrice,purchasable&&!District.released&&!District.identified&&State.cash>=District.ReleasePrice,395,536,395,()=>{District.PayRelease(State);Notify("Release purchased. Hold E at the medicine case.");});
            DistrictAction("SETTLE OFFENSE / $60",District.identified&&State.cash>=60,806,536,395,()=>District.Restitution(State));
            Text("There is an alley behind the guard. Watch which way he faces.\nTaking the case is possible without release; discovery and identification are different.",395,607,805,58,18,quiet);
            DistrictAction("WHO KEEPS THE LEDGER?",true,395,680,395,()=>{conversationIvo=true;screen=ScreenMode.Conversation;});
            DistrictAction("LEAVE",true,806,680,395,()=>screen=ScreenMode.Play);
        }
        void DrawJournal()
        {
            DistrictPanel("YOUR HISTORY / KNOWN EVENTS","THE CITY KEEPS GOING","Clinic: "+District.ClinicStatus+". Medicine owner: "+District.shipmentOwner+".\n"+ShipmentSchedule+". Time pauses while you read.");
            Text("YOU â†’ PARTNERSHIP â†’ CREW â†’ A FOOTHOLD",395,408,805,29,19,medical,FontStyle.Bold);
            Text(District.recruited?District.refuge?"Shared refuge: free recovery, a clinic fund and responsibility for the shelf.":"First partnership: Neri trusts you. Ask about the spare room: $120.":"First ambition: earn someone who will help you survive. Help Neri's clinic.",395,447,805,47,18,paper);
            journalScroll=GUI.BeginScrollView(new Rect(395,509,805,144),journalScroll,new Rect(0,0,775,Mathf.Max(140,District.incidents.Count*58)));
            for(int i=0;i<District.incidents.Count;i++) { var incident=District.incidents[District.incidents.Count-1-i];Text("#"+(District.incidents.Count-i)+"  "+ClockAt(incident.time)+"  /  "+incident.text,5,i*58,750,55,17,quiet); }
            if(District.incidents.Count==0)Text("No incidents yet. The collector's buyer arrives twelve minutes after you enter Old Port.",5,0,750,70,18,quiet);
            GUI.EndScrollView();
            DistrictAction("BACK / J",true,395,680,805,()=>screen=ScreenMode.Play);
        }
        void DrawDistrictHome()
        {
            DistrictPanel("HOME / SHELTER, NOT IMMUNITY","A BED TO COME BACK TO","Bank loose cargo by standing in the home circle and holding E. Medical stock is a separate choice: Neri or Mara. Use this desk to prepare for an operation.");
            Text("CASH $"+State.cash+"   /   DEBT $"+District.debt+"   /   CARGO $"+Cargo.Value+"\nHEALTH "+Mathf.CeilToInt(District.health)+"   BANDAGES "+District.bandages+"   PISTOL ROUNDS "+District.ammo,395,411,805,67,19,medical,FontStyle.Bold);
            DistrictAction("BANDAGE / $12",State.cash>=12,395,510,255,()=>{State.cash-=12;District.marketMoney+=12;District.bandages++;});
            if(District.arms==null)DistrictAction("6 ROUNDS / $24",State.cash>=24,670,510,255,()=>{State.cash-=24;District.marketMoney+=24;District.ammo+=6;});
            else Text("AMMO: SELLA / MARKET COURT",670,521,255,40,16,CityArt.Amber);
            DistrictAction("REST / $40 CREDIT",District.health<80||District.bleeding,945,510,255,()=>District.RestOnCredit());
            DistrictAction("SATCHEL / $180",!State.satchel&&State.cash>=180,395,575,395,()=>BuyCargoSatchel());
            DistrictAction("PAY DEBT / $"+District.debt,District.debt>0&&State.cash>=District.debt,806,575,395,()=>{State.cash-=District.debt;District.marketMoney+=District.debt;District.debt=0;District.Record("debt","player","Your emergency-care debt is paid.");});
            DistrictAction("BACK TO THE STREET",true,395,680,805,()=>screen=ScreenMode.Play);
        }
        void DrawDistrictPause()
        {
            DistrictPanel("PAUSED / "+Application.version,"A MOMENT TO BREATHE","The world pauses here. F5 saves where you stand, including wounds, carried goods and crew. Autosave runs every ten seconds. Quitting also saves.");
            Text(CrewEnabled?"WASD move / SHIFT sprint / CTRL sneak\nF1 protagonist / F3 Neri / F4 Rell / K crew & rescue\nMouse aim / LMB fire / 1 fists / 2 pistol / 3 shotgun / 4 SMG / 5 rifle\nR reload / B timed self-aid / SPACE tactical pause\nE interact / hold to work / G follow / H hold / T aid addressed partner\nTAB map / J history / F5 save / F2 HUD details":"WASD / arrows   Move       SHIFT sprint       CTRL sneak\nMouse aim / LMB fire   1 conceal   2 pistol   3 shotgun   4 SMG / R reload\nSPACE tactical pause   G follow   H hold   SHIFT+R retreat   T aid\nE interact / hold to take   F home supplies   TAB map\nJ history   L track story   P pet Tally   F at clinic: refuge",395,410,805,170,20,paper);
            DistrictAction("RESUME",true,395,605,395,()=>screen=ScreenMode.Play);
            DistrictAction("SOUND / "+(mute?"OFF":"ON"),true,806,605,395,()=>mute=!mute);
            DistrictAction("SAVE & MAIN MENU",true,395,680,395,()=>{Save();screen=ScreenMode.Title;});
            DistrictAction("SAVE & QUIT",true,806,680,395,()=>{Save();Application.Quit();});
        }
    }
}
