using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        const float W=1600,H=900;
        readonly Color ink=CityArt.Hex("101D2B"), paper=CityArt.Hex("F1EADD"), quiet=CityArt.Hex("CDD7DF");
        Font uiFont;
        Texture2D disc;
        GUIStyle label;
        void InitUI()
        {
            if(label!=null)return;
            uiFont=Font.CreateDynamicFontFromOSFont(new[]{"Bahnschrift","Segoe UI","Arial"},20);
            label=new GUIStyle { font=uiFont,wordWrap=true,richText=false };
            disc=new Texture2D(32,32,TextureFormat.RGBA32,false);
            for(int y=0;y<32;y++) for(int x=0;x<32;x++) disc.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(16-Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(16,16)))));
            disc.Apply();
        }
        void WarmFont()
        {
            const string glyphs=" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~\u2192\u2022\u2026\u2014\u2013\u2019\u2018\u201c\u201d";
            // Keep the active atlas populated before IMGUI submits any text. Large
            // headings only need bold capitals; avoid filling it with unused faces.
            foreach(int size in new[]{12,14,16,18,20,24})
                foreach(var style in new[]{FontStyle.Normal,FontStyle.Bold})uiFont.RequestCharactersInTexture(glyphs,size,style);
            uiFont.RequestCharactersInTexture(glyphs,28,FontStyle.Bold);
            uiFont.RequestCharactersInTexture("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 /&'?.:-",40,FontStyle.Bold);
            uiFont.RequestCharactersInTexture("FUNSTRA",80,FontStyle.Bold);
        }
        void Rect(float x,float y,float w,float h,Color c)
        { GUI.color=c; GUI.DrawTexture(new Rect(x,y,w,h),Texture2D.whiteTexture); GUI.color=Color.white; }
        void Dot(float x,float y,float r,Color c)
        { GUI.color=c; GUI.DrawTexture(new Rect(x-r,y-r,r*2,r*2),disc); GUI.color=Color.white; }
        void Text(string text,float x,float y,float w,float h,int size,Color c,FontStyle style=FontStyle.Normal,TextAnchor align=TextAnchor.UpperLeft)
        {
            size=size>60?80:size>36?40:size>26?28:size>22?24:size>19?20:size>16?18:size>14?16:size>12?14:12;
            label.fontSize=size; label.normal.textColor=c; label.fontStyle=style; label.alignment=align;
            GUI.Label(new Rect(x,y,w,h),text,label);
        }
        void Panel(float x,float y,float w,float h)
        {
            Rect(x+5,y+7,w,h,new Color(0,0,0,.17f)); Rect(x,y,w,h,new Color(ink.r,ink.g,ink.b,.96f));
            Rect(x,y,w,1,new Color(1,1,1,.13f));
        }
        bool Button(string text,float x,float y,float w,float h,bool primary=false)
        {
            bool hover=new Rect(x,y,w,h).Contains(Event.current.mousePosition);
            Color c=primary?CityArt.Mint:(hover?CityArt.Hex("344958"):CityArt.Hex("243644"));
            if(primary&&hover)c=Color.Lerp(c,Color.white,.15f);
            Rect(x,y,w,h,c); Text(text,x+18,y,w-36,h,19,primary?ink:paper,FontStyle.Bold,TextAnchor.MiddleLeft);
            return GUI.Button(new Rect(x,y,w,h),GUIContent.none,GUIStyle.none);
        }
        void OnGUI()
        {
            InitUI();if(Event.current.type==EventType.Repaint)WarmFont(); GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/W,Screen.height/H,1));
            if(FoundationMode) { DrawFoundation();return; }
            if(screen==ScreenMode.Title||screen==ScreenMode.ConfirmRestart) { DrawTitle(); if(screen==ScreenMode.ConfirmRestart) DrawRestart(); return; }
            DrawHUD();
            if(DistrictEnabled) {if(!CrewEnabled||ControlledCrewId=="player")DrawCombatOverlay();DrawYardStatus();if(!crewPanel)DrawResidentStatus();}
            switch(screen)
            {
                case ScreenMode.Talk: DrawTalk(); break;
                case ScreenMode.Perk: DrawPerks(); break;
                case ScreenMode.Pause: DrawPause(); break;
                case ScreenMode.Ending: DrawEnding(); break;
                case ScreenMode.Safehouse: DrawSafehouse(); break;
            }
            if(DistrictEnabled)DrawDistrictScreen();
            DrawCrewUI();DrawDockOperationUI();
        }
        void DrawTitle()
        {
            Rect(0,0,615,H,new Color(ink.r,ink.g,ink.b,.97f)); Rect(615,0,3,H,CityArt.Mint);
            Text(Application.version=="0.6.0"?"NOBODY GETS HOME ALONE / DEMO 06":Application.version=="0.5.1"?"OLD PORT UNDER PRESSURE":Application.version=="0.5.0"?"THE PRICE OF A GUN / DEMO 05":Application.version=="0.4.2"?"PRESSURE AND ESCAPE / PLAYABLE SLICE":Application.version=="0.4.1"?"POLICE RESPONSE / PLAYABLE SLICE":"STREETS WORTH FIGHTING FOR / DEMO 04",62,66,500,40,16,CityArt.Mint,FontStyle.Bold);
            Text("FUNSTRA",54,145,540,115,86,paper,FontStyle.Bold);
            Rect(62,280,65,4,CityArt.Mint);
            Text("One person.\nA city of debts.\nA place to keep.",62,318,475,185,42,paper,FontStyle.Bold);
            Text("Find an approach. Watch your ammunition. Get home with someone worth trusting.",64,526,450,88,21,quiet);
            var saved=LoadCurrentRun();
            if(saved!=null)
            {
                if(Button("CONTINUE YOUR STORY",62,640,488,62,true))StartRun(true);
                if(Button("START A NEW NIGHT",62,716,488,52))screen=ScreenMode.ConfirmRestart;
            }
            else if(Button("ENTER OLD PORT",62,654,488,66,true))StartRun(false);
            Text(Application.version=="0.5.0"?"0.5.0 / LOCAL DEMO CANDIDATE":Application.version=="0.4.2"?"0.4.2     /     PRESSURE AND ESCAPE":Application.version=="0.4.1"?"0.4.1     /     POLICE RESPONSE CANDIDATE":"DEMO 04     /     FOUNDATION CANDIDATE",62,822,430,30,14,quiet);
            if(Button("QUIT",450,810,100,44))Application.Quit();
            Panel(1146,60,390,109); Dot(1180,97,5,CityArt.Mint);
            Text("OLD PORT, FUNSTRA",1200,82,300,30,20,paper,FontStyle.Bold);
            Text("21:40  /  BUSINESS AFTER HOURS",1171,125,335,25,14,quiet);
            Text("SURVIVE THE CITY.   CHOOSE YOUR PEOPLE.   BUILD YOUR POWER.",710,805,800,44,17,paper,FontStyle.Bold,TextAnchor.MiddleCenter);
            DrawFoundationSelector();
            Panel(1050,674,486,105);
            if(Button("DEBUG FAST RUN / "+(debugFastRunning?"ON":"OFF"),1074,690,438,45,debugFastRunning))debugFastRunning=!debugFastRunning;
            Text("SHIFT: 3x sprint + unlimited stamina. This session only.",1074,746,438,25,14,quiet);
        }
        void DrawHUD()
        {
            if(compactHUD) {DrawCompactHUD();return;}
            Panel(28,24,389,70); Rect(28,24,5,70,CityArt.Mint);
            Text("FUNSTRA",50,36,200,45,30,paper,FontStyle.Bold);
            Text(DistrictEnabled?"PORT / "+PortClock:"OLD PORT / 21:40",247,51,150,25,13,quiet);
            Panel(28,108,389,169);
            string chapter=State.Finished?"THE PORT IS YOURS":State.accepted?"JOB 0"+(State.completed+1)+" / 03":"YOUR NEXT MOVE";
            if(DistrictEnabled&&trackDistrict)chapter="A BED & A BANDAGE";
            Text(chapter,50,126,344,25,13,CityArt.Mint,FontStyle.Bold);
            string title=State.Finished?"DEBT SETTLED":!State.accepted?"MEET MARA":State.carrying?"BRING IT HOME":Jobs.All[State.completed].title;
            if(DistrictEnabled&&trackDistrict)title=District.refuge?"A DOOR WITH TWO NAMES":District.recruited?"NOT ALONE ANYMORE":District.Carrying?"WHO GETS THE DOSES?":"A CLINIC IN DEBT";
            Text(title,50,161,344,42,25,paper,FontStyle.Bold);
            string objective=State.Finished?"Run hot cargo. Bank your take at home.\nUpgrade your bag. Go back for more.":!State.accepted?"Talk to the fence at the pawn counter.":State.carrying?(Heat>0?"Lose the police. Break sight in an alley.":"Return the goods to Mara for payment."):"Take the "+Jobs.All[State.completed].item.ToLowerInvariant()+".\n"+Jobs.All[State.completed].district+"  /  HOLD E";
            if(DistrictEnabled&&trackDistrict)objective=MedicalObjective;
            Text(objective,50,209,343,58,17,quiet);
            DrawCargoHUD();
            Panel(1190,24,382,70);
            Text("CASH",1212,37,105,22,12,quiet,FontStyle.Bold); Text("$"+State.cash,1210,56,122,34,25,paper,FontStyle.Bold);
            Text("MARA / STANDING",1381,37,175,22,12,quiet,FontStyle.Bold); Text(State.completed==0?"NEW FACE":State.completed==1?"RUNNER":State.completed==2?"TRUSTED":"CONNECTED",1381,59,180,30,19,CityArt.Mint,FontStyle.Bold);
            Panel(1250,108,322,86);
            Text(Police.Searching?PoliceStatus:Heat>0?"WANTED / "+(Elapsed-lastSight<.5f?"IN SIGHT":"SEARCHING"):Hidden?"HIDDEN":"KEEP A LOW PROFILE",1271,124,280,32,17,Heat>0?CityArt.Red:CityArt.Mint,FontStyle.Bold);
            Rect(1272,167,277,5,CityArt.Hex("344754")); Rect(1272,167,277*(Heat/12),5,CityArt.Red);
            if(Heat>0)Text("Break sight. Hold CTRL by a green bin to hide.",1190,204,382,60,15,paper);
            DrawMap(showMap?new Rect(420,280,760,540):new Rect(1312,612,260,260),showMap);
            Panel(28,758,327,114);
            Text(Sneaking?"SNEAKING":sprinting?"SPRINTING":"ON FOOT",48,775,285,25,14,CityArt.Mint,FontStyle.Bold);
            Rect(49,810,285,5,CityArt.Hex("344754")); Rect(49,810,285*(Stamina/(State.HasPerk(1)?135:100)),5,CityArt.Mint);
            Text("WASD move   SHIFT sprint   CTRL sneak",49,834,285,24,13,quiet);
            Text(DistrictEnabled?"TAB map   SPACE tactics   J history   L track story   ESC menu":"TAB map    M sound    ESC pause    Scroll zoom",465,851,750,27,15,paper,FontStyle.Normal,TextAnchor.MiddleCenter);
            DrawWorldMarkers();
            if(toastTime>0)
            {
                Panel(458,210,693,64); Rect(458,210,4,64,CityArt.Amber);
                Text(toast,479,222,650,45,17,paper,FontStyle.Normal,TextAnchor.MiddleLeft);
            }
            if(!string.IsNullOrEmpty(prompt)&&screen==ScreenMode.Play&&!showMap)
            {
                Panel(457,754,692,72); Text(prompt,478,768,650,43,19,Hidden?CityArt.Mint:paper,FontStyle.Bold,TextAnchor.MiddleCenter);
                if(theftProgress>0)Rect(457,822,692*theftProgress,4,CityArt.Amber);
                if(cargoProgress>0)Rect(457,822,692*cargoProgress,4,CityArt.Hex("C5A7ED"));
                if(extractionProgress>0)Rect(457,822,692*extractionProgress,4,CityArt.Mint);
                if(medicineProgress>0)Rect(457,822,692*medicineProgress,4,medical);
            }
            if(arrestProgress>0)
            {
                Text("GET AWAY!",600,622,400,50,30,CityArt.Red,FontStyle.Bold,TextAnchor.MiddleCenter);
                Rect(650,680,300,7,ink);Rect(650,680,300*arrestProgress,7,CityArt.Red);
            }
        }
        void DrawCargoHUD()
        {
            if(DistrictEnabled) { DrawDistrictVitals();return; }
            Color cargoColor=CityArt.Hex("C5A7ED");
            Panel(28,295,389,187);Rect(28,295,4,187,cargoColor);
            Text("HOT CARGO",50,312,200,25,13,cargoColor,FontStyle.Bold);
            Text(Cargo.Value>0?"$"+Cargo.Value+" AT RISK":"EMPTY BAG",50,343,340,39,25,paper,FontStyle.Bold);
            Text(Cargo.Weight+" / "+State.CargoCapacity+" LOAD",50,389,190,24,14,quiet,FontStyle.Bold);
            Text(Cargo.SpeedMultiplier<1?"HEAVY  /  MOVE SLOWER":"READY TO RUN",235,389,160,24,12,Cargo.SpeedMultiplier<1?CityArt.Amber:quiet,FontStyle.Bold,TextAnchor.UpperRight);
            Rect(50,418,344,5,CityArt.Hex("344754"));
            Rect(50,418,344*Mathf.Clamp01((float)Cargo.Weight/State.CargoCapacity),5,cargoColor);
            Text(Cargo.Value>0?"Get home. Lose heat. HOLD E to bank.":"TAB: find lilac cargo. F at home: upgrades.",50,439,344,29,15,Cargo.Value>0?CityArt.Mint:quiet);
        }
        Vector2 MapPoint(Vector3 p,Rect r) => new Vector2(r.x+(p.x-City.Nav.Min.x)/(City.Nav.Max.x-City.Nav.Min.x)*r.width,r.y+(City.Nav.Max.y-p.z)/(City.Nav.Max.y-City.Nav.Min.y)*r.height);
        void MapBox(Bounds b,Rect r,Color c)
        {
            var top=MapPoint(new Vector3(b.min.x,0,b.max.z),r);var bottom=MapPoint(new Vector3(b.max.x,0,b.min.z),r);
            Rect(top.x,top.y,bottom.x-top.x,bottom.y-top.y,c);
        }
        void MapLeader(Vector2 point,Vector2 labelPoint,Color color,int lane)
        {
            float elbow=labelPoint.x+(point.x<labelPoint.x?-1:1)*(12+lane*7);
            Rect(Mathf.Min(point.x,elbow),point.y,Mathf.Abs(point.x-elbow),1,color);
            Rect(elbow,Mathf.Min(point.y,labelPoint.y),1,Mathf.Abs(point.y-labelPoint.y),color);
            Rect(Mathf.Min(elbow,labelPoint.x),labelPoint.y,Mathf.Abs(elbow-labelPoint.x),1,color);
        }
        void DrawMap(Rect outer,bool expanded)
        {
            Panel(outer.x,outer.y,outer.width,outer.height);
            Text(expanded?"OLD PORT  /  STREET MAP":"OLD PORT",outer.x+17,outer.y+12,outer.width-34,27,expanded?21:13,paper,FontStyle.Bold);
            Rect r=new Rect(outer.x+16,outer.y+45,outer.width-32,outer.height-65);
            Rect(r.x,r.y,r.width,r.height,CityArt.Hex("273C49"));
            foreach(var b in City.Buildings)MapBox(b,r,CityArt.Hex("6C7982"));
            if(!FoundationMode)MapBox(City.ClinicFloorBounds,r,medical);
            foreach(var h in City.Hides) { var p=MapPoint(h,r);Dot(p.x,p.y,expanded?5:2,CityArt.Mint); }
            var mara=MapPoint(Jobs.Mara,r);Dot(mara.x,mara.y,expanded?8:5,CityArt.Mint);
            var home=MapPoint(Jobs.Home,r);float homeSize=expanded?12:7;
            Rect(home.x-homeSize/2,home.y-homeSize/2,homeSize,homeSize,CityArt.Mint);
            if(expanded)Text("HOME",home.x-30,home.y+14,75,24,12,CityArt.Mint,FontStyle.Bold);
            for(int i=0;i<CargoRun.Sites.Length;i++)
            {
                if(Cargo.Taken(i))continue;
                var site=MapPoint(CargoRun.Sites[i].position,r);
                Dot(site.x,site.y,expanded?7:4,CityArt.Hex("C5A7ED"));
                if(expanded)
                {
                    var cargoSite=CargoRun.Sites[i];
                    float labelX=r.x+12,labelY=r.y+45+i*67;
                    MapLeader(site,new Vector2(labelX+165,labelY+12),CityArt.Hex("C5A7ED"),i);
                    Rect(labelX-5,labelY-2,170,43,ink);
                    Text(cargoSite.name.ToUpperInvariant(),labelX,labelY,160,20,12,CityArt.Hex("C5A7ED"),FontStyle.Bold);
                    Text("$"+cargoSite.value+"  /  "+cargoSite.weight+" LOAD"+(cargoSite.alarm?"  /  ALARM":""),labelX,labelY+20,160,20,11,paper);
                }
            }
            if(State.accepted&&!State.carrying&&!State.Finished) { var p=MapPoint(Jobs.All[State.completed].position,r);Dot(p.x,p.y,expanded?9:5,CityArt.Amber); }
            foreach(var a in Agents) if(a.Police) { var p=MapPoint(a.Position,r);Dot(p.x,p.y,expanded?6:3,Heat>0?CityArt.Red:CityArt.Blue); }
            var player=MapPoint(Player.position,r);Dot(player.x,player.y,expanded?9:5,paper);Dot(player.x,player.y,expanded?4:2,ink);
            DistrictMap(r,expanded);
            if(DockEnabled)
            {
                foreach(string id in new[]{"neri","rell"})
                {var dot=MapPoint(CrewPosition(id),r);Dot(dot.x,dot.y,expanded?7:4,ControlledCrewId==id?paper:medical);if(expanded)Text(id.ToUpper(),Mathf.Min(dot.x+10,r.xMax-75),dot.y-23,75,23,12,medical,FontStyle.Bold);}
                if(District.dock.componentOwner=="yard"){var dot=MapPoint(DockOperationState.ComponentPost,r);Dot(dot.x,dot.y,7,CityArt.Amber);if(expanded)Text("PUMP",dot.x-24,dot.y-25,80,22,12,CityArt.Amber,FontStyle.Bold);}
            }
            Text("N",r.x+r.width-18,r.y+1,20,20,13,paper,FontStyle.Bold);
            if(expanded) Text("CYAN clinic / medicine   LILAC cargo   MINT home   BLUE police   TAB close",outer.x+22,outer.y+outer.height-27,outer.width-44,25,13,quiet);
        }
        void DrawWorldMarkers()
        {
            if(showMap)return;
            Vector3 target=State.accepted&&!State.carrying&&!State.Finished?Jobs.All[State.completed].position:Jobs.Mara;
            string name=State.accepted&&!State.carrying&&!State.Finished?Jobs.All[State.completed].item.ToUpperInvariant():"MARA";
            bool headingHome=Cargo.Value>0&&(!State.accepted||State.Finished);
            if(headingHome) { target=Jobs.Home;name="SAFEHOUSE"; }
            else if(State.Finished)
            {
                float nearest=float.MaxValue;
                for(int i=0;i<CargoRun.Sites.Length;i++)
                {
                    if(Cargo.Taken(i))continue;
                    float distance=Vector3.Distance(Player.position,CargoRun.Sites[i].position);
                    if(distance<nearest) { nearest=distance;target=CargoRun.Sites[i].position;name="HOT CARGO"; }
                }
            }
            if(DistrictEnabled&&trackDistrict)
            {
                target=District.Carrying||District.shipmentUnits==0||!District.metNeri?District.neri.position:District.ShipmentPosition;
                name=District.Carrying||District.shipmentUnits==0||!District.metNeri?"NERI":"MEDICINE";
                if(District.recruited) { target=DistrictState.Clinic;name="CLINIC"; }
            }
            if(ArmsEnabled&&trackDistrict&&(District.arms.favorStage<2&&!District.arms.Owns(2)||District.arms.favorStage==1))
            {target=District.arms.favorStage==1&&!District.arms.favorReceipt?ArmsFavorPosition:ArmsDealerPosition;name=target==ArmsFavorPosition?"NERI / PAPERS":"SELLA";}
            if(DockEnabled&&trackDistrict)
            {
                bool carryingPerson=District.crew.For(ControlledCrewId).carrying!="";
                if(carryingPerson||District.dock.stakeResolved){target=DistrictState.Clinic;name="CLINIC";}
                else if(District.dock.repairAccepted&&!District.dock.auxiliaryRepaired){target=DockOperationState.RepairPost;name="REPAIR PUMP";}
                else if(District.dock.componentOwner!="yard"||!District.dock.metRell&&!District.dock.released&&!District.dock.auxiliaryRepaired){target=DockOperationState.Workshop;name="RELL";}
                else {target=DockOperationState.ComponentPost;name="IMPOUNDED PUMP";}
            }
            var p=View.WorldToScreenPoint(target+Vector3.up*4.5f);float x=p.x/Screen.width*W,y=(1-p.y/Screen.height)*H;
            bool off=p.z<0||x<460||x>1170||y<120||y>712;
            x=Mathf.Clamp(x,475,1135); y=Mathf.Clamp(y,133,713);
            if(off)y=700;
            if(!DistrictEnabled||TargetActor==null)
            { Panel(x-104,y-19,208,46); Text((off?"TO ":"")+name+" / "+Mathf.RoundToInt(Vector3.Distance(ControlledPosition,target))+"m",x-95,y-10,190,30,14,headingHome?CityArt.Mint:CityArt.Amber,FontStyle.Bold,TextAnchor.MiddleCenter); }
            foreach(var a in Agents)
            {
                if(!a.Police||Vector3.Distance(a.Position,Player.position)>25)continue;
                var sp=View.WorldToScreenPoint(a.Position+Vector3.up*2.9f);
                if(sp.z<0)continue;
                float ax=sp.x/Screen.width*W,ay=(1-sp.y/Screen.height)*H;
                Dot(ax,ay,12,ink); Text(a.Pursuing?"!":"â€¢",ax-10,ay-14,20,28,21,a.Pursuing?CityArt.Red:CityArt.Blue,FontStyle.Bold,TextAnchor.MiddleCenter);
                if(a.Suspicion>0&&!a.Pursuing)Rect(ax-19,ay+18,38*Mathf.Clamp01(a.Suspicion),3,CityArt.Amber);
            }
        }
        void Shade() => Rect(0,0,W,H,new Color(.025f,.055f,.085f,.67f));
        void DrawTalk()
        {
            Shade();Panel(416,225,768,447);Rect(416,225,5,447,CityArt.Mint);
            Text("MARA",455,259,500,60,41,paper,FontStyle.Bold);Text("FENCE  /  PAWN & CO.",457,323,650,32,14,CityArt.Mint,FontStyle.Bold);
            int i=Mathf.Min(State.completed,2);
            Text(Jobs.All[i].dialogue,457,379,679,124,25,paper);
            Text("JOB 0"+(i+1)+"   /   "+Jobs.All[i].title+"   /   $"+Jobs.All[i].reward,457,525,680,32,16,CityArt.Amber,FontStyle.Bold);
            if(Button(State.accepted?"BACK TO THE JOB":"I'M IN. TAKE THE JOB.",456,592,435,54,true)) { State.Accept();Save();screen=ScreenMode.Play;Notify("Follow the amber marker. Hold E at the goods."); }
            if(Button("LEAVE",908,592,234,54))screen=ScreenMode.Play;
        }
        void DrawPerks()
        {
            Shade();Panel(248,209,1104,487);
            Text("YOU'RE GETTING KNOWN",284,240,1014,64,42,paper,FontStyle.Bold);
            Text("Job paid. Choose a permanent edge for the rest of this night.",286,313,1000,40,22,quiet);
            string[] titles={"LIGHT FEET","LONG STRIDE","QUICK HANDS"};
            string[] desc={"Sneak 35% faster.\nHarder to spot while sneaking.","Sprint 20% faster.\n35% more stamina.","Take valuables 40% faster.\nLess time in the open."};
            for(int i=0;i<3;i++)
            {
                float x=284+i*348;Rect(x,381,324,266,CityArt.Hex("243642"));
                Text("0"+(i+1),x+22,401,280,43,29,CityArt.Mint,FontStyle.Bold);
                Text(titles[i],x+22,456,280,40,24,paper,FontStyle.Bold);
                Text(desc[i],x+22,505,280,74,19,quiet);
                if(State.HasPerk(i))Text("ALREADY LEARNED",x+22,592,280,33,16,CityArt.Mint,FontStyle.Bold);
                else if(Button("CHOOSE",x+18,584,288,46,true)) { State.ChoosePerk(i);Save();screen=ScreenMode.Play;Notify("New perk learned. Talk to Mara for the next job."); }
            }
        }
        void DrawPause()
        {
            if(DistrictEnabled) { DrawDistrictPause();return; }
            Shade();Panel(493,116,614,678);Text("NIGHT ON HOLD",535,154,530,65,39,paper,FontStyle.Bold);
            Text("WASD / arrows     Move relative to camera\nSHIFT                    Sprint\nCTRL / C                 Sneak; hide by green bins\nHold E                    Steal / bank cargo at home\nF                             Safehouse upgrades\nTAB                         Street map\nScroll wheel            Camera zoom\nF11                          Fullscreen",537,237,518,260,20,quiet);
            if(Button("RESUME",535,513,530,55,true))screen=ScreenMode.Play;
            if(Button("SOUND  /  "+(mute?"OFF":"ON"),535,582,530,49))mute=!mute;
            if(Button("MAIN MENU",535,644,255,49))screen=ScreenMode.Title;
            if(Button("QUIT",807,644,258,49))Application.Quit();
            Text("Banked cargo and upgrades save. Arrest loses unbanked cargo.\nMade for Funstra. Original procedural art and sound. Unity engine.",537,715,524,60,14,quiet);
        }
        void DrawEnding()
        {
            Shade();Panel(359,139,882,617);Rect(359,139,882,5,CityArt.Mint);
            Text("A NAME IN OLD PORT",408,191,785,75,47,paper,FontStyle.Bold);
            Text("DEBT SETTLED. NIGHT SURVIVED.",410,286,780,35,17,CityArt.Mint,FontStyle.Bold);
            Text("\"We're square. And if you ever need work...\nyou know where to find me.\"",410,362,776,94,29,paper);
            Text("â€” Mara",412,470,700,38,20,quiet);
            Text("3 JOBS COMPLETE       $"+State.cash+" CASH       "+State.arrests+" ARREST"+(State.arrests==1?"":"S"),410,549,788,48,20,CityArt.Amber,FontStyle.Bold);
            if(Button("KEEP WORKING THE PORT",410,636,463,61,true))ContinueFreeroam();
            if(Button("MAIN MENU",892,636,297,61))screen=ScreenMode.Title;
        }
        void DrawSafehouse()
        {
            if(DistrictEnabled) { DrawDistrictHome();return; }
            Shade();Panel(376,149,848,602);Rect(376,149,848,5,CityArt.Mint);
            Text("YOUR CORNER OF OLD PORT",420,180,756,30,14,CityArt.Mint,FontStyle.Bold);
            Text("THE SAFEHOUSE",417,225,760,64,43,paper,FontStyle.Bold);
            Text("Cash is only yours when you make it home.",420,299,755,35,23,quiet);
            Rect(420,356,760,107,CityArt.Hex("243642"));
            Text("BANKED CASH",440,371,230,26,13,quiet,FontStyle.Bold);
            Text("$"+State.cash,440,404,235,42,29,paper,FontStyle.Bold);
            Text("BAG  /  "+Cargo.Weight+" OF "+State.CargoCapacity,711,371,430,26,13,CityArt.Hex("C5A7ED"),FontStyle.Bold);
            Text("$"+Cargo.Value+" UNBANKED",711,409,430,34,23,paper,FontStyle.Bold);
            Text(State.satchel?"COURIER SATCHEL  /  EQUIPPED":"COURIER SATCHEL  /  $180",420,492,756,33,22,CityArt.Mint,FontStyle.Bold);
            Text(State.satchel?"More room for a bigger take. Heavy loads still slow you down.":"Raise bag capacity from 6 to 9. Carry more; risk more.",420,536,756,38,19,quiet);
            if(!State.satchel&&Button(State.cash>=180?"BUY SATCHEL  /  $180":"NEED $"+(180-State.cash)+" MORE",420,597,402,55,State.cash>=180))BuyCargoSatchel();
            if(State.satchel)Text(State.cargoRuns+" RUNS BANKED  /  $"+State.cargoEarnings+" TOTAL",420,607,402,38,17,quiet,FontStyle.Bold);
            if(Button("BACK TO THE STREET",842,597,338,55,State.satchel))screen=ScreenMode.Play;
            Text("To bank cargo: return to the street, lose all heat, then HOLD E at home.",420,685,760,35,16,quiet);
        }
        void DrawRestart()
        {
            Shade();Panel(470,290,660,310);Text("START OVER?",510,330,580,60,37,paper,FontStyle.Bold);
            Text("This replaces your saved night, including completed jobs and perks.",510,410,578,70,22,quiet);
            if(Button("START NEW NIGHT",510,510,340,53,true))StartRun(false);
            if(Button("CANCEL",868,510,222,53))screen=ScreenMode.Title;
        }
    }
}
