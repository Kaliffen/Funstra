using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        bool bandageTest, bandageVisual, trackDistrict=true;
        public bool DistrictEnabled => !Smoke||bandageTest||bandageVisual;
        public DistrictState District => State.district;
        Transform neriBody, guardBody, collectorBody, catTail;
        GameObject medicineProp, medicineRing, pistol;
        LineRenderer shotLine;
        int weapon=2, selectedActor;
        float medicineProgress, attackCooldown, guardCooldown, aidCooldown, autosaveTime, shotTime, defeatGrace;
        string districtMessage="";
        readonly Color medical=CityArt.Hex("7CDEEB");
        readonly List<Vector3> neriPath=new List<Vector3>(), guardPath=new List<Vector3>();
        float neriRepath,guardRepath;

        void BuildDistrict()
        {
            neriBody=City.Human("Neri / clinic orderly",District.neri.position,medical);
            guardBody=City.Human("Rook / collector's guard",District.guard.position,CityArt.Hex("A65C53"));
            collectorBody=City.Human("Ivo / debt collector",District.collector.position,CityArt.Hex("CFAC72"));
            City.Box("Neri medical pack",new Vector3(0,1.1f,-.35f),new Vector3(.55f,.65f,.3f),CityArt.Hex("EBE2C8"),neriBody);
            City.Box("Medical cross",new Vector3(0,1.1f,-.51f),new Vector3(.32f,.09f,.02f),medical,neriBody,false,true);
            City.Box("Medical cross",new Vector3(0,1.1f,-.51f),new Vector3(.09f,.32f,.02f),medical,neriBody,false,true);
            City.Box("Rook carbine",new Vector3(.39f,.9f,.35f),new Vector3(.13f,.16f,.85f),CityArt.Hex("27303C"),guardBody);
            City.Ring("Clinic",DistrictState.Clinic,2.7f,medical);
            City.Solid("Clinic supplies counter",DistrictState.Clinic+new Vector3(-1.5f,.65f,0),new Vector3(.8f,1.3f,2.2f),CityArt.Hex("3A6569"));
            City.Box("Clinic awning",DistrictState.Clinic+new Vector3(-1.5f,2.9f,0),new Vector3(2,.12f,3.3f),medical);
            City.Sign("REPAIR",DistrictState.Clinic+new Vector3(-1.5f,3.3f,0),medical,.15f);
            City.Solid("Camp bed",DistrictState.Clinic+new Vector3(0,.35f,-3.5f),new Vector3(1.1f,.45f,2.2f),CityArt.Hex("BFBBA0"));
            var cat=new GameObject("Tally / clinic cat").transform;cat.position=DistrictState.Clinic+new Vector3(1.6f,0,-1.8f);
            City.Shape("Sleeping cat",PrimitiveType.Sphere,new Vector3(0,.23f,0),new Vector3(.6f,.4f,.8f),CityArt.Hex("C49464"),cat);
            City.Shape("Cat head",PrimitiveType.Sphere,new Vector3(0,.38f,.35f),new Vector3(.39f,.35f,.35f),CityArt.Hex("D7B184"),cat);
            for(int s=-1;s<=1;s+=2)City.Box("Cat ear",new Vector3(s*.14f,.58f,.33f),new Vector3(.13f,.19f,.13f),CityArt.Hex("C49464"),cat);
            catTail=City.Box("Cat tail",new Vector3(.2f,.18f,-.4f),new Vector3(.1f,.1f,.6f),CityArt.Hex("8E684F"),cat).transform;
            City.Ring("Collector",DistrictState.CollectorPost,1.5f,CityArt.Amber);
            medicineProp=City.Prop("Six sealed medical doses",DistrictState.Garage+Vector3.up*.6f,new Vector3(.9f,1.1f,.9f),medical);
            City.Box("Case stripe",new Vector3(0,0,-.51f),new Vector3(.65f,.15f,.03f),Color.white,medicineProp.transform,false,true);
            medicineRing=City.Ring("Impounded medical stock",DistrictState.Garage,1.6f,medical);
            pistol=City.Box("Player pistol",new Vector3(.43f,.95f,.3f),new Vector3(.12f,.15f,.45f),CityArt.Hex("303848"),figure);
            var tracer=new GameObject("Shot tracer");shotLine=tracer.AddComponent<LineRenderer>();
            shotLine.sharedMaterial=new Material(Shader.Find("Sprites/Default"));shotLine.positionCount=2;shotLine.startWidth=.055f;shotLine.endWidth=.015f;
            shotLine.startColor=CityArt.Amber;shotLine.endColor=Color.white;shotLine.enabled=false;
            BuildRefugeArt();City.RefreshProps();City.Nav.Bake();
            AttachCitizens();
            if(!DistrictEnabled) { neriBody.gameObject.SetActive(false);guardBody.gameObject.SetActive(false);collectorBody.gameObject.SetActive(false);medicineProp.SetActive(false);medicineRing.SetActive(false);pistol.SetActive(false); }
        }
        void ResetDistrictRuntime()
        {
            medicineProgress=attackCooldown=guardCooldown=aidCooldown=0;autosaveTime=10;selectedActor=0;defeatGrace=8;
            neriPath.Clear();guardPath.Clear();neriRepath=guardRepath=0;
            District.neri.position=City.Nav.SafePoint(District.neri.position);
            District.guard.position=City.Nav.SafePoint(District.guard.position);
            District.collector.position=City.Nav.SafePoint(District.collector.position);
            AttachCitizens();SyncDistrictArt();
        }
        void AttachCitizens()
        {
            if(!DistrictEnabled)return;
            if(District.citizens==null)District.citizens=new List<DistrictActor>();
            for(int i=0;i<Agents.Count;i++)
            {
                string id="citizen-"+i;
                var record=District.citizens.Find(c=>c.id==id);
                if(record==null) { record=new DistrictActor(id,Agents[i].Police?"OFFICER "+(i+1):"RESIDENT "+(i-2),Agents[i].Position);District.citizens.Add(record); }
                record.position=City.Nav.SafePoint(record.position);Agents[i].Record=record;Agents[i].Body.position=record.position;PoseActor(Agents[i].Body,record);
            }
        }
        void SyncDistrictArt()
        {
            if(!DistrictEnabled||!neriBody)return;
            neriBody.position=District.neri.position;guardBody.position=District.guard.position;collectorBody.position=District.collector.position;
            PoseActor(neriBody,District.neri);PoseActor(guardBody,District.guard);PoseActor(collectorBody,District.collector);
            bool available=District.shipmentUnits>0&&(District.shipmentOwner=="collector"||District.shipmentOwner=="buyer");
            medicineProp.SetActive(available);medicineRing.SetActive(available);
            medicineProp.transform.position=District.ShipmentPosition+Vector3.up*.6f;medicineRing.transform.position=District.ShipmentPosition;
            pistol.SetActive(weapon==2);SyncRefugeArt();City.RefreshProps();
        }
        void PoseActor(Transform body,DistrictActor actor)
        { body.localScale=actor.health<=0?new Vector3(1.7f,.22f,1):Vector3.one; }
        DistrictActor TargetActor => selectedActor==1?District.guard:selectedActor==2?District.collector:selectedActor==3?District.neri:selectedActor>=4&&selectedActor-4<Agents.Count?Agents[selectedActor-4].Record:null;
        Transform TargetBody => selectedActor==1?guardBody:selectedActor==2?collectorBody:selectedActor==3?neriBody:selectedActor>=4&&selectedActor-4<Agents.Count?Agents[selectedActor-4].Body:null;
        bool ActorSees(DistrictActor actor,Transform body,Vector3 target,float range)
        {
            Vector3 delta=target-actor.position;delta.y=0;
            return actor.health>0&&delta.magnitude<range&&City.Nav.Sight(actor.position,target)&&(delta.magnitude<2.6f||Vector3.Dot(body.forward,delta.normalized)>.3f);
        }
        bool MedicalWitness() => ActorSees(District.guard,guardBody,Player.position,Sneaking?5:10)||ActorSees(District.collector,collectorBody,Player.position,Sneaking?4:8);

        void UpdateDistrict(float dt)
        {
            District.Tick(dt);defeatGrace=Mathf.Max(0,defeatGrace-dt);
            attackCooldown-=dt;guardCooldown-=dt;aidCooldown-=dt;autosaveTime-=dt;
            if(shotTime>0) { shotTime-=dt;shotLine.enabled=shotTime>0; }
            catTail.localRotation=Quaternion.Euler(0,25+Mathf.Sin(District.clock*.8f)*12,0);
            if(District.bleeding)District.health=Mathf.Max(0,District.health-dt*.65f);
            if(District.neri.bleeding)District.neri.health=Mathf.Max(0,District.neri.health-dt*.35f);
            if(Input.GetKeyDown(KeyCode.Alpha1))weapon=1;
            if(Input.GetKeyDown(KeyCode.Alpha2))weapon=2;
            if(Input.GetKeyDown(KeyCode.B)) { if(District.BandagePlayer()) { Save();Notify("Bandaged. Bleeding stopped. Clinic rest heals serious wounds."); } else Notify("No dressing needed, or no bandages. Home stocks bandages for $12."); }
            if(Input.GetKeyDown(KeyCode.G))OrderNeri("Follow");
            if(Input.GetKeyDown(KeyCode.H))OrderNeri("Hold");
            if(Input.GetKeyDown(KeyCode.R))OrderNeri("Retreat");
            if(Input.GetKeyDown(KeyCode.T))OrderNeri("Aid");
            if(Input.GetKeyDown(KeyCode.L))trackDistrict=!trackDistrict;
            UpdateRefugeInteraction();
            SelectDistrictTarget();
            if(Input.GetMouseButton(0)&&!showMap&&Input.mousePosition.x/Screen.width>.27f&&Input.mousePosition.x/Screen.width<.74f&&Input.mousePosition.y/Screen.height>.18f&&Input.mousePosition.y/Screen.height<.85f)AttackSelected();
            if(!freezeDistrictAI) { UpdateGuard(dt);UpdateNeri(dt); }
            if(District.health<=0)DistrictDefeat();
            SyncDistrictArt();
            if(autosaveTime<=0) { Save();autosaveTime=10; }
        }
        void SelectDistrictTarget()
        {
            if(!Input.GetMouseButtonDown(1)||showMap)return;
            float best=65;selectedActor=0;
            var bodies=new List<Transform>{guardBody,collectorBody,neriBody};foreach(var agent in Agents)bodies.Add(agent.Body);
            for(int i=0;i<bodies.Count;i++)
            {
                var sp=View.WorldToScreenPoint(bodies[i].position+Vector3.up);if(sp.z<0)continue;
                float d=Vector2.Distance(new Vector2(sp.x,sp.y),Input.mousePosition);
                if(d<best) { best=d;selectedActor=i+1; }
            }
        }
        bool AttackSelected()
        {
            var target=TargetActor;if(target==null||target.health<=0||attackCooldown>0)return false;
            float distance=Vector3.Distance(Player.position,target.position);
            if(distance>(weapon==1?2.8f:18)||!City.Nav.Sight(Player.position,target.position)) { Notify("No clear shot. Close distance or move around cover.");attackCooldown=.5f;return false; }
            if(weapon==2&&District.ammo<=0) { Notify("Pistol empty. Switch to fists with 1, retreat, or buy ammo at home.");attackCooldown=.5f;return false; }
            if(weapon==2)District.ammo--;
            attackCooldown=weapon==1?.7f:.65f;
            Vector3 heading=target.position-Player.position;heading.y=0;if(heading.sqrMagnitude>.01f)figure.rotation=Quaternion.LookRotation(heading);
            target.health=Mathf.Max(0,target.health-(weapon==1?18:28));
            if(weapon==2)Trace(Player.position,target.position);
            Sound(weapon==2?alertSound:stepSound);
            if(target==District.neri)
            { District.trust=-3;District.recruited=false;District.neri.bleeding=weapon==2;District.Record("betrayal","neri","You attacked Neri. The partnership is broken."); }
            bool seen=target==District.guard||target==District.collector||MedicalWitness();
            if(seen)District.Identify("An attack was witnessed and reported to Ivo.");
            if(selectedActor>=4)
            { target.order="Flee";District.Record("assault",target.id,"You attacked "+target.name+". The victim called for police.");RaiseAlarm("Assault reported. Police are responding.",Player.position);PoseActor(TargetBody,target); }
            if(weapon==2)RaiseAlarm("Gunfire reported. Break sight. Ivo's grievance will outlast the chase.",Player.position);
            if(target.health==0) { District.Record("incapacitated",target.id,target.name+" is incapacitated. Their inventory and relationships remain.");Notify(target.name+" down. You can withdraw or help them with E and a bandage."); }
            Save();return true;
        }
        void Trace(Vector3 from,Vector3 to)
        { shotLine.SetPosition(0,from+Vector3.up*1.2f);shotLine.SetPosition(1,to+Vector3.up);shotLine.enabled=true;shotTime=.1f; }
        void WalkActor(DistrictActor actor,Transform body,Vector3 target,float speed,float dt,List<Vector3> path,ref float repath)
        {
            repath-=dt;
            if(repath<=0) { path.Clear();path.AddRange(City.Nav.Find(actor.position,target));repath=.6f; }
            Vector3 previous=actor.position;
            while(path.Count>0&&Vector3.Distance(actor.position,path[0])<.25f)path.RemoveAt(0);
            if(path.Count>0)
            {
                Vector3 next=Vector3.MoveTowards(actor.position,path[0],speed*dt);
                if(City.Nav.ClearWalk(actor.position,next))actor.position=next;
                Vector3 heading=actor.position-previous;if(heading.sqrMagnitude>.0001f)body.rotation=Quaternion.Slerp(body.rotation,Quaternion.LookRotation(heading),dt*12);
            }
            CityArt.Animate(body,District.clock,Vector3.Distance(previous,actor.position)/Mathf.Max(.001f,dt));
        }
        void UpdateGuard(float dt)
        {
            var guard=District.guard;if(guard.health<=0)return;
            bool sees=ActorSees(guard,guardBody,Player.position,14);
            if(sees) { District.guardLastSeen=Player.position;District.guardSawAt=District.clock; }
            if(defeatGrace<=0&&District.identified&&sees&&Vector3.Distance(Player.position,guard.position)<8)District.hostile=true;
            if(District.hostile&&defeatGrace<=0)
            {
                bool neriTarget=District.recruited&&District.neri.health>0&&ActorSees(guard,guardBody,District.neri.position,14)&&Vector3.Distance(guard.position,District.neri.position)<Vector3.Distance(guard.position,Player.position)-1;
                if(neriTarget) { District.guardLastSeen=District.neri.position;District.guardSawAt=District.clock; }
                Vector3 target=neriTarget?District.neri.position:sees?Player.position:District.guardLastSeen;
                float distance=Vector3.Distance(guard.position,target);
                bool sight=City.Nav.Sight(guard.position,target);
                if(distance<15&&sight&&(sees||neriTarget)&&guard.ammo>0)
                {
                    guardBody.LookAt(new Vector3(target.x,guardBody.position.y,target.z));
                    if(guardCooldown<=0)
                    {
                        guardCooldown=1.9f;guard.ammo--;Trace(guard.position,target);Sound(alertSound);
                        if(neriTarget) { District.neri.health=Mathf.Max(0,District.neri.health-13);District.neri.bleeding=true; }
                        else { District.health=Mathf.Max(0,District.health-13);District.bleeding=true;Notify("Hit! B bandage. SPACE pauses. Retreat behind a building."); }
                    }
                }
                else if(guard.ammo==0&&(sees||neriTarget)&&distance<2.6f)
                { if(guardCooldown<=0) { guardCooldown=1.2f;if(neriTarget)District.neri.health=Mathf.Max(0,District.neri.health-8);else District.health=Mathf.Max(0,District.health-8); } }
                else if(District.clock-District.guardSawAt<8&&Vector3.Distance(guard.position,DistrictState.Garage)<20)WalkActor(guard,guardBody,target,3.2f,dt,guardPath,ref guardRepath);
                else District.hostile=false;
            }
            else
            {
                // A visible gap behind the guard makes observation and sneaking useful.
                Vector3 post=District.shipmentOwner=="buyer"?DistrictState.Buyer:new Vector3(-28,0,Mathf.FloorToInt(District.clock/9)%2==0?9:0);
                if(Vector3.Distance(guard.position,post)>.5f)WalkActor(guard,guardBody,post,1.7f,dt,guardPath,ref guardRepath);
                else guardBody.rotation=Quaternion.Euler(0,Mathf.FloorToInt(District.clock/9)%2==0?0:180,0);
            }
        }
        void UpdateNeri(float dt)
        {
            var neri=District.neri;if(neri.health<=0||!District.recruited)return;
            if(neri.order=="Retreat")WalkActor(neri,neriBody,DistrictState.Clinic,4,dt,neriPath,ref neriRepath);
            if(neri.order=="Follow"||neri.order=="Aid")
            {
                if(Vector3.Distance(neri.position,Player.position)>2)WalkActor(neri,neriBody,Player.position,4.5f,dt,neriPath,ref neriRepath);
                if(Vector3.Distance(neri.position,Player.position)<3&&aidCooldown<=0&&District.NeriAid()) { aidCooldown=8;Save();Notify("Neri stabilized you. Field dressings remaining: "+neri.bandages); }
            }
        }
        void OrderNeri(string order)
        { if(!District.recruited) { Notify("Earn Neri's trust at the clinic to form a partnership.");return; } District.neri.order=order;neriRepath=0;Save();Notify("Neri: "+order+"."); }
        bool UpdateMedicalInteraction(float dt,bool pressed,bool held)
        {
            if(!DistrictEnabled)return false;
            if(Input.GetKeyDown(KeyCode.Y)&&Vector3.Distance(Player.position,District.guard.position)<12&&District.hostile) { DistrictDefeat();return true; }
            var casualties=new List<DistrictActor>{District.neri,District.guard,District.collector};casualties.AddRange(District.citizens);
            foreach(var actor in casualties)
            {
                if(actor.health>0||Vector3.Distance(Player.position,actor.position)>2.6f)continue;
                prompt="E / STABILIZE "+actor.name+"  /  1 BANDAGE";
                if(pressed&&District.bandages>0)
                {
                    if(actor==District.neri)District.AidNeri();else { District.bandages--;actor.health=30;actor.bleeding=false;District.Record("aid",actor.id,"You helped "+actor.name+" back to their feet."); }
                    Save();Notify(actor.name+" stabilized.");
                    foreach(var agent in Agents)if(agent.Record==actor)PoseActor(agent.Body,actor);
                }
                return true;
            }
            bool busySpot=Vector3.Distance(Player.position,Jobs.Home)<3||Vector3.Distance(Player.position,Jobs.Mara)<3.2f||Vector3.Distance(Player.position,District.ShipmentPosition)<2.5f;
            foreach(var site in CargoRun.Sites)if(Vector3.Distance(Player.position,site.position)<2.6f)busySpot=true;
            if(!busySpot&&Vector3.Distance(Player.position,District.neri.position)<2.9f&&District.neri.health>0)
            { prompt="E / NERI  /  CLINIC & CREW";if(pressed) { District.metNeri=true;screen=ScreenMode.Clinic;Save(); }return true; }
            if(Vector3.Distance(Player.position,District.collector.position)<2.8f&&District.collector.health>0)
            { prompt="E / IVO  /  RELEASE PAPERS & RESTITUTION";if(pressed) { screen=ScreenMode.Collector;Save(); }return true; }
            if((District.shipmentOwner=="collector"||District.shipmentOwner=="buyer")&&District.shipmentUnits>0&&Vector3.Distance(Player.position,District.ShipmentPosition)<2.4f)
            {
                prompt=District.released?"HOLD E / COLLECT RELEASED MEDICINE":"HOLD E / STEAL MEDICINE  /  ROOK CAN SEE THE CRATE";
                if(held)
                {
                    Stealing=!District.released;medicineProgress+=dt/(District.released?.7f:3f);
                    if(!District.released&&MedicalWitness()) { District.Identify("The collector's people saw you opening the medicine case."); }
                    if(medicineProgress>=1)
                    { District.TakeShipment(MedicalWitness());medicineProgress=0;Save();SyncDistrictArt();Notify("Six doses packed. Give to Neri, sell through Mara, or keep them.");Sound(pickupSound); }
                }
                else medicineProgress=0;
                return true;
            }
            medicineProgress=0;
            if(District.Carrying&&Vector3.Distance(Player.position,Jobs.Mara)<3.2f)
            { prompt="E / MARA  /  SELL MEDICINE OR KEEP IT";if(pressed) { screen=ScreenMode.MedicineSale;Save(); }return true; }
            return false;
        }
        void DistrictDefeat()
        {
            bool rescued=District.recruited&&District.neri.health>0&&Vector3.Distance(District.neri.position,Player.position)<12;
            District.Defeat(State,rescued);Cargo.Lose();RefreshCargoArt();State.carrying=false;Heat=arrestProgress=0;ResetPolice();
            Teleport(Jobs.Home);District.neri.position=District.recruited&&District.neri.health>0?Jobs.Home+Vector3.left*2:District.neri.position;
            defeatGrace=20;District.guard.position=new Vector3(-28,0,8);guardPath.Clear();districtMessage=rescued?"Neri dragged you out. You have someone to come home with.":"You woke in an emergency bed. $40 has been added to your debt.";
            screen=ScreenMode.Recovery;Save();SyncDistrictArt();
        }
        void DistrictCheckpoint()
        {
            if(!DistrictEnabled||!Player)return;
            District.playerPosition=Player.position;District.hasPosition=true;District.savedHeat=Heat;District.savedLastSeen=LastSeen;
            District.savedJobCarrying=State.carrying;District.cargoTakenMask=0;
            for(int i=0;i<CargoRun.Sites.Length;i++)if(Cargo.Taken(i))District.cargoTakenMask|=1<<i;
        }
        void OnApplicationQuit() { if(screen!=ScreenMode.Title&&screen!=ScreenMode.ConfirmRestart)Save(); }
    }
}
