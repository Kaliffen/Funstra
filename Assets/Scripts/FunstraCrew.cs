using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        public bool crewTest;
        bool crewPressure;
        public bool CrewEnabled=>DistrictEnabled&&!FoundationMode&&(!Smoke||crewTest||crewPressure);
        public string ControlledCrewId=>CrewEnabled&&District.crew!=null?District.crew.selectedId:"player";
        public Vector3 ControlledPosition=>CrewPosition(ControlledCrewId);
        Transform rellBody;
        readonly Dictionary<string,GameObject> crewGuns=new Dictionary<string,GameObject>();
        bool crewPanel;
        string crewTarget="neri";
        readonly Dictionary<string,List<Vector3>> crewPaths=new Dictionary<string,List<Vector3>>();
        readonly Dictionary<string,float> crewRepaths=new Dictionary<string,float>();
        public bool IsCrewId(string id)=>id=="player"||id=="neri"&&District.recruited||id=="rell"&&District.crew!=null&&District.crew.rellRecruited;
        public bool CrewAlive(string id)=>(id=="player"||id=="neri"||id=="rell")&&CrewHealth(id)>0;
        public bool CrewGunDrawn(string id)=>id=="player"?weapon>1:IsCrewId(id)&&District.crew.For(id).armed&&!District.crew.For(id).holstered;
        DistrictActor CrewActor(string id)=>id=="neri"?District.neri:id=="rell"?District.crew?.rell:null;
        public Vector3 CrewPosition(string id)=>id=="player"?Player.position:CrewActor(id)?.position??Player.position;
        Transform CrewBody(string id)=>id=="player"?Player:id=="neri"?neriBody:rellBody;
        float CrewHealth(string id)=>id=="player"?District.health:CrewActor(id)?.health??0;
        void SetCrewHealth(string id,float value){if(id=="player")District.health=value;else CrewActor(id).health=value;}
        bool CrewBleeding(string id)=>id=="player"?District.bleeding:CrewActor(id).bleeding;
        void SetCrewBleeding(string id,bool value){if(id=="player")District.bleeding=value;else CrewActor(id).bleeding=value;}
        int CrewBandages(string id)=>id=="player"?District.bandages:CrewActor(id).bandages;
        void SetCrewBandages(string id,int value){if(id=="player")District.bandages=value;else CrewActor(id).bandages=value;}
        public IEnumerable<Vector3> LivingCrewPositions
        {get {foreach(string id in new[]{"player","neri","rell"})if(IsCrewId(id)&&CrewAlive(id))yield return CrewPosition(id);}}
        string CarrierOf(string id)=>District.crew.members.Find(m=>m.carrying==id)?.id;
        void SetCrewPosition(string id,Vector3 p)
        {
            if(id=="player"){controller.enabled=false;Player.position=p;controller.enabled=true;District.playerPosition=p;}
            else {CrewActor(id).position=p;CrewBody(id).position=p;}
        }
        void InitializeCrewRuntime()
        {
            if(!CrewEnabled)return;
            District.InitializeCrew();
            if(!rellBody)
            {
                rellBody=City.Human("Rell / pump mechanic",District.crew.rell.position,CityArt.Hex("B28B55"));
                City.Box("Rell tool roll",new Vector3(0,1,-.3f),new Vector3(.65f,.35f,.3f),CityArt.Hex("544635"),rellBody);
            }
            rellBody.position=City.Nav.SafePoint(District.crew.rell.position);District.crew.rell.position=rellBody.position;
            PoseActor(rellBody,District.crew.rell);
            RegisterCombatActor(District.crew.rell,rellBody);crewPaths.Clear();crewRepaths.Clear();
            foreach(string id in new[]{"neri","rell"})if(!crewGuns.ContainsKey(id))crewGuns[id]=City.Box(id+" carried firearm",new Vector3(.4f,1,.35f),new Vector3(.13f,.16f,.6f),CityArt.Hex("27303C"),CrewBody(id));
            foreach(var m in District.crew.members){crewPaths[m.id]=new List<Vector3>();crewRepaths[m.id]=0;}
            if(!IsCrewId(ControlledCrewId)||!CrewAlive(ControlledCrewId))foreach(var m in District.crew.members)if(IsCrewId(m.id)&&CrewAlive(m.id)){District.crew.selectedId=m.id;break;}
            InitializeDockOperation();SnapCamera();
        }
        public bool SelectCrew(string id)
        {
            if(!CrewEnabled||!IsCrewId(id)||!CrewAlive(id)||CarrierOf(id)!=null)return false;
            District.crew.selectedId=id;autoMove=null;autoInteract=false;crewPanel=false;Stealing=false;
            District.crew.For(id).order="Hold";crewPaths[id].Clear();
            SnapCamera();Save();return true;
        }
        void CrewInput()
        {
            if(!CrewEnabled||District.crew==null||screen!=ScreenMode.Play&&screen!=ScreenMode.Tactics)return;
            if(!Smoke)
            {
                if(Input.GetKeyDown(KeyCode.F1))SelectCrew("player");
                if(Input.GetKeyDown(KeyCode.F3))SelectCrew("neri");
                if(Input.GetKeyDown(KeyCode.F4))SelectCrew("rell");
                if(Input.GetKeyDown(KeyCode.K))ToggleCrewPanel();
                if(Input.GetKeyDown(KeyCode.G))OrderCrew(crewTarget,"Follow");
                if(Input.GetKeyDown(KeyCode.H))OrderCrew(crewTarget,"Hold");
                if(Input.GetKeyDown(KeyCode.T))BeginCrewAid(ControlledCrewId,crewTarget);
                if(crewMoveTarget!=""&&Input.GetMouseButtonDown(0))
                {
                    var ray=View.ScreenPointToRay(Input.mousePosition);var ground=new Plane(Vector3.up,Vector3.zero);
                    if(ground.Raycast(ray,out float distance)){OrderCrew(crewMoveTarget,"Move",ray.GetPoint(distance));crewMoveTarget="";}
                }
            }
        }
        void ResumeCrewPlay()
        {crewPanel=false;crewMoveTarget="";screen=ScreenMode.Play;}
        void ToggleCrewPanel()
        {if(crewPanel)ResumeCrewPlay();else {crewPanel=true;crewMoveTarget="";screen=ScreenMode.Tactics;}}
        static string ChooseCrewArrestCandidate(IList<string> candidates,string previous)
        {return candidates.Contains(previous)?previous:candidates.Count>0?candidates[0]:"";}
        public bool OrderCrew(string id,string order,Vector3? destination=null)
        {
            if(!CrewEnabled||!IsCrewId(id)||!CrewAlive(id)||CarrierOf(id)!=null)return false;
            if(order!="Follow"&&order!="Hold"&&order!="Move"&&order!="Retreat"&&order!="Engage")return false;
            var m=District.crew.For(id);m.order=order;m.patient="";m.aidRemaining=0;
            if(id!="player")m.holstered=order!="Engage";
            if(destination.HasValue)m.destination=City.Nav.SafePoint(destination.Value);
            crewPaths[id].Clear();crewRepaths[id]=0;if(id=="neri")District.neri.order=order;
            Save();Notify(id.ToUpper()+": "+order);return true;
        }
        bool CrewMoveClear(string id,Vector3 next)
        {
            var before=CrewPosition(id);if(!City.Nav.ClearWalk(before,next))return false;
            foreach(var m in District.crew.members)if(m.id!=id&&IsCrewId(m.id)&&CarrierOf(m.id)==null&&CrewHealth(m.id)>0&&
                Vector3.Distance(next,CrewPosition(m.id))<.85f&&Vector3.Distance(next,CrewPosition(m.id))<Vector3.Distance(before,CrewPosition(m.id)))return false;
            foreach(var a in Agents)if(a.Record!=null&&a.Record.health>0&&Vector3.Distance(next,a.Position)<.8f&&Vector3.Distance(next,a.Position)<Vector3.Distance(before,a.Position))return false;
            return true;
        }
        void MoveCrew(string id,Vector3 movement,float speed,float dt)
        {
            var body=CrewBody(id);Vector3 before=CrewPosition(id);movement.y=0;
            var m=District.crew.For(id);if(m.carrying!="")speed*=.48f;
            Vector3 next=before+Vector3.ClampMagnitude(movement,1)*speed*dt;
            if(CrewMoveClear(id,next))
            {
                if(id=="player"){controller.Move((Vector3.ClampMagnitude(movement,1)*speed+Vector3.down*8)*dt);District.playerPosition=Player.position;}
                else SetCrewPosition(id,next);
            }
            Vector3 heading=CrewPosition(id)-before;var visual=id=="player"?figure:body;
            if(id=="player"&&heading.sqrMagnitude>.00001f)Hidden=false;
            if(heading.sqrMagnitude>.00001f)visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(heading),dt*14);
            CityArt.Animate(visual,District.clock,heading.magnitude/Mathf.Max(dt,.001f));
        }
        bool UpdateControlledCrew(float dt)
        {
            if(!CrewEnabled||District.crew==null)return false;
            if(ControlledCrewId=="player"&&CrewAlive("player")&&CarrierOf("player")==null&&District.crew.For("player").carrying=="")return false;
            string id=ControlledCrewId;if(!CrewAlive(id)||CarrierOf(id)!=null)return true;
            Vector3 movement;
            if(autoMove.HasValue)movement=Vector3.ClampMagnitude(autoMove.Value-ControlledPosition,1);
            else {var forward=View.transform.forward;forward.y=0;forward.Normalize();movement=View.transform.right*((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0))+forward*((Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0));}
            var m=District.crew.For(id);bool run=(Input.GetKey(KeyCode.LeftShift)||autoMove.HasValue)&&m.stamina>2;
            m.stamina=Mathf.Clamp(m.stamina+dt*(run&&movement.sqrMagnitude>.01f?-19:15),0,100);
            bool sneaking=Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.C);
            MoveCrew(id,movement,(sneaking?2.3f:run?7:4.2f)*(DebugFastRunning&&run&&!sneaking?3:1),dt);return true;
        }
        void StepCrew(float dt)
        {
            if(!CrewEnabled||District.crew==null)return;
            foreach(var m in District.crew.members)
            {
                if(!IsCrewId(m.id))continue;
                if(m.id!="player"&&crewGuns.TryGetValue(m.id,out var gunVisual))
                {gunVisual.SetActive(CrewGunDrawn(m.id)&&CrewAlive(m.id));gunVisual.transform.localScale=new Vector3(.13f,.16f,CrewActor(m.id).combat.kind==5?1.1f:CrewActor(m.id).combat.kind==3?.85f:.6f);}
                if(m.id=="rell"&&CrewBleeding(m.id))SetCrewHealth(m.id,Mathf.Max(0,CrewHealth(m.id)-dt*.35f));
                if(m.id!="player")
                {
                    if(m.id==ControlledCrewId)CrewActor(m.id).combat.Step(dt,CrewActor(m.id).ammo);
                    else StepActorWeapon(CrewActor(m.id),dt);
                }
                if(m.carrying!=""&&!CrewAlive(m.id))DropCrew(m.id);
                if(CarrierOf(m.id)!=null)continue;
                if(!CrewAlive(m.id)){if(m.id!="player")PoseActor(CrewBody(m.id),CrewActor(m.id));else figure.localScale=new Vector3(1.7f,.22f,1);continue;}
                if(m.aidRemaining>0){StepCrewAid(m,dt);continue;}
                if(m.id!=ControlledCrewId&&!freezeDistrictAI)
                {
                    Vector3 goal=m.order=="Follow"?ControlledPosition:m.order=="Retreat"?DistrictState.Clinic:m.destination;
                    bool move=m.order=="Follow"||m.order=="Retreat"||m.order=="Move";
                    if(move&&Vector3.Distance(CrewPosition(m.id),goal)>(m.order=="Follow"?2.7f:.6f))
                    {
                        crewRepaths[m.id]-=dt;var path=crewPaths[m.id];
                        if(crewRepaths[m.id]<=0)
                        {
                            path.Clear();path.AddRange(City.Nav.Find(CrewPosition(m.id),goal));
                            // Replanning can insert the grid connector behind a moving
                            // person. As with police paths, skip it only when the next
                            // segment is physically clear; retain obstructed corners.
                            if(path.Count>1&&City.Nav.ClearWalk(CrewPosition(m.id),path[1]))path.RemoveAt(0);
                            crewRepaths[m.id]=.6f;
                        }
                        while(path.Count>0&&Vector3.Distance(CrewPosition(m.id),path[0])<.3f)path.RemoveAt(0);
                        if(path.Count>0)MoveCrew(m.id,(path[0]-CrewPosition(m.id)).normalized,4.5f,dt);
                    }
                    if(m.order=="Engage"&&m.carrying=="")CrewEngage(m.id);
                }
                if(m.carrying!="")
                {
                    var patient=m.carrying;SetCrewPosition(patient,CrewPosition(m.id)+Vector3.up*.8f);
                    if(patient!="player")CrewBody(patient).localScale=new Vector3(1.2f,.35f,1);
                }
            }
            if(!CrewAlive(ControlledCrewId))foreach(var m in District.crew.members)if(IsCrewId(m.id)&&CrewAlive(m.id)&&CarrierOf(m.id)==null)
            {SelectCrew(m.id);Notify("Continue as "+m.id.ToUpper()+". Your downed partner remains where they fell. Carry them home.");break;}
            StepDockOperation(dt);
        }
        void CrewEngage(string id)
        {
            if(!CrewGunDrawn(id))return;
            var actor=CrewActor(id);int kind=id=="player"?weapon:actor.combat.kind;Vector3 position=CrewPosition(id);
            DistrictActor target=null;float best=WeaponSpec.For(kind).range;
            if(yardSquad!=null)foreach(var enemy in yardSquad.Members)if(enemy.Actor.health>0&&City.Nav.Sight(position,enemy.Actor.position))
            {float d=Vector3.Distance(position,enemy.Actor.position);if(d<best&&CrewShotClear(id,enemy.Actor.position,kind)){target=enemy.Actor;best=d;}}
            // Cover also protects the crew from pursuing officers. The shooter must
            // see the officer here; another actor's sight/report grants no wall knowledge.
            foreach(var enemy in Agents)if(enemy.Police&&enemy.Pursuing&&enemy.Record!=null&&enemy.Record.health>0&&!IsCrewId(enemy.Record.id)&&City.Nav.Sight(position,enemy.Record.position))
            {float d=Vector3.Distance(position,enemy.Record.position);if(d<best&&CrewShotClear(id,enemy.Record.position,kind)){target=enemy.Record;best=d;}}
            if(target!=null){if(id=="player"){if(CurrentWeapon.magazine==0)ReloadPlayer();FirePlayerAt(target.position);}else FireActorAt(actor,CrewBody(id),target.position,kind);}
        }
        bool CrewShotClear(string shooter,Vector3 target,int kind)
        {
            var spec=WeaponSpec.For(kind);Vector3 origin=CrewPosition(shooter)+Vector3.up*1.1f;
            Vector3 direction=target-origin;direction.y=0;if(direction.sqrMagnitude<.0001f)return false;direction.Normalize();
            // SpawnShot uses half the shotgun cone, or single-shot recoil plus
            // SMG/rifle spread. Cover the full possible flight, including a miss
            // past the target, without altering manual shots or projectile hits.
            float cone=spec.pellets>1?spec.spread*.5f:(shooter=="player"?recoil:0)+(kind>=4?spec.spread:0);
            float widening=Mathf.Tan(cone*Mathf.Deg2Rad);
            Vector3 end=origin+direction*(spec.range+.5f);
            foreach(string ally in new[]{"player","neri","rell"})
            {
                if(ally==shooter||!IsCrewId(ally))continue;
                Vector3 center=CrewPosition(ally);center.y=origin.y;
                // Carried/downed people retain their horizontal footprint here;
                // their raised pose is never treated as permission to shoot them.
                float along=Mathf.Clamp(Vector3.Dot(center-origin,direction),0,spec.range+.5f);
                float radius=.65f+along*widening;
                if(ProjectileMath.MovingSphere(origin,end,center,center,radius,out _))return false;
            }
            return true;
        }
        public bool BeginCrewAid(string helper,string patient)
        {
            if(!CrewEnabled||!IsCrewId(helper)||!CrewAlive(helper)||!IsCrewId(patient)||CrewBandages(helper)<=0||(!CrewBleeding(patient)&&CrewHealth(patient)>=90)||
               Vector3.Distance(CrewPosition(helper),CrewPosition(patient))>2.6f||!City.Nav.Sight(CrewPosition(helper),CrewPosition(patient)))return false;
            var m=District.crew.For(helper);if(m.carrying!=""||CarrierOf(patient)!=null)return false;
            m.patient=patient;m.aidOrigin=CrewPosition(helper);m.aidHealth=CrewHealth(helper);m.aidRemaining=3;m.order="Hold";
            Save();return true;
        }
        void StepCrewAid(CrewOrderState m,float dt)
        {
            if(!CrewAlive(m.id)||Vector3.Distance(CrewPosition(m.id),m.aidOrigin)>.25f||
               Vector3.Distance(CrewPosition(m.id),CrewPosition(m.patient))>2.6f||!City.Nav.Sight(CrewPosition(m.id),CrewPosition(m.patient))||CrewBandages(m.id)<=0)
            {m.aidRemaining=0;m.patient="";Notify("Field aid interrupted. Dressing retained.");return;}
            m.aidRemaining=Mathf.Max(0,m.aidRemaining-dt);if(m.aidRemaining>0)return;
            SetCrewBandages(m.id,CrewBandages(m.id)-1);SetCrewHealth(m.patient,Mathf.Min(100,Mathf.Max(15,CrewHealth(m.patient))+12));SetCrewBleeding(m.patient,false);
            m.medicinePractice++;var patient=District.crew.For(m.patient);patient.memory=m.id.ToUpper()+" stayed and used a dressing on me.";
            if(m.patient!="player")PoseActor(CrewBody(m.patient),CrewActor(m.patient));
            District.Record("crew aid",m.id,patient.memory);m.patient="";Save();
        }
        public bool CarryCrew(string carrier,string patient)
        {
            if(!CrewEnabled||carrier==patient||!IsCrewId(carrier)||!CrewAlive(carrier)||!IsCrewId(patient)||CrewHealth(patient)>0||CarrierOf(patient)!=null||
               District.crew.For(carrier).carrying!=""||Vector3.Distance(CrewPosition(carrier),CrewPosition(patient))>2.5f||!City.Nav.Sight(CrewPosition(carrier),CrewPosition(patient)))return false;
            District.crew.For(carrier).carrying=patient;District.crew.For(patient).order="Hold";Save();return true;
        }
        public bool DropCrew(string carrier)
        {
            var m=District.crew.For(carrier);if(m.carrying=="")return false;
            Vector3 p=CrewPosition(carrier);p.y=0;Vector3 drop=p;bool found=false;
            foreach(var offset in new[]{Vector3.left,Vector3.right,Vector3.forward,Vector3.back,(Vector3.left+Vector3.forward).normalized,(Vector3.right+Vector3.back).normalized,Vector3.zero})
            {
                Vector3 candidate=p+offset*1.4f;
                if(City.Nav.Walkable(candidate)&&City.Nav.ClearWalk(p,candidate)){drop=candidate;found=true;break;}
            }
            if(!found)return false;
            string patient=m.carrying;m.carrying="";SetCrewPosition(patient,drop);
            if(patient!="player")PoseActor(CrewBody(patient),CrewActor(patient));else figure.localScale=District.health<=0?new Vector3(1.7f,.22f,1):Vector3.one;Save();return true;
        }
        public bool ReturnCrew(string carrier)
        {
            if(!CrewEnabled||!CrewAlive(carrier)||Vector3.Distance(CrewPosition(carrier),DistrictState.Clinic)>4||!City.Nav.ClearWalk(CrewPosition(carrier),DistrictState.Clinic))return false;
            var m=District.crew.For(carrier);string patient=m.carrying;
            if(patient==""||District.clinicStock<=0)return false;
            if(!DropCrew(carrier))return false;
            District.clinicStock--;District.consumed++;SetCrewHealth(patient,45);SetCrewBleeding(patient,false);
            if(patient=="player")figure.localScale=Vector3.one;else PoseActor(CrewBody(patient),CrewActor(patient));
            var memory=District.crew.For(patient);memory.rescuedCount++;memory.abandoned=false;memory.memory=carrier.ToUpper()+" carried me to the clinic. I know who came back.";
            District.Record("crew rescue",patient,memory.memory);Save();Notify(memory.memory);return true;
        }
        public bool AbandonCrew(string patient)
        {
            if(!CrewEnabled||patient==ControlledCrewId||!IsCrewId(patient)||CrewHealth(patient)>0||CarrierOf(patient)!=null||Vector3.Distance(ControlledPosition,DistrictState.Clinic)>4||!City.Nav.ClearWalk(ControlledPosition,DistrictState.Clinic))return false;
            var m=District.crew.For(patient);if(m.abandoned)return false;
            m.abandoned=true;m.abandonedCount++;m.memory=ControlledCrewId.ToUpper()+" returned to shelter and left me behind.";
            District.Record("crew abandonment",patient,m.memory);Save();return true;
        }
        public bool TransferCrewBandage(string from,string to)
        {
            if(from==to||!IsCrewId(from)||!CrewAlive(from)||!IsCrewId(to)||CrewBandages(from)<=0||Vector3.Distance(CrewPosition(from),CrewPosition(to))>2.6f||!City.Nav.Sight(CrewPosition(from),CrewPosition(to)))return false;
            SetCrewBandages(from,CrewBandages(from)-1);SetCrewBandages(to,CrewBandages(to)+1);Save();return true;
        }
        bool CrewDefeatHandled()
        {
            if(!CrewEnabled||District.crew==null||District.health>0)return false;
            foreach(var m in District.crew.members)if(IsCrewId(m.id)&&CrewAlive(m.id))return true;
            return false;
        }
        void InterruptCrewAid(string id)
        {
            if(!CrewEnabled||District.crew==null)return;var m=District.crew.For(id);
            if(m==null||m.aidRemaining<=0)return;m.aidRemaining=0;m.patient="";Notify(id.ToUpper()+" was hit. Field aid interrupted; dressing retained.");
        }
        void ClearCrewRecoveryLinks()
        {
            if(!CrewEnabled||District.crew==null)return;
            foreach(var m in District.crew.members){m.carrying="";m.patient="";m.aidRemaining=0;m.order="Hold";crewPaths[m.id].Clear();}
            District.crew.selectedId="player";figure.localScale=Vector3.one;
        }
        void BustedCrew(string id)
        {
            var actor=CrewActor(id);if(actor==null)return;
            var m=District.crew.For(id);if(m.carrying!=""&&!DropCrew(id))return;
            int kind=actor.combat.kind;if(kind<District.arms.confiscated.Length)District.arms.confiscated[kind]+=actor.ammo;
            actor.ammo=0;actor.combat=new WeaponState{kind=kind,initialized=true};m.armed=false;m.holstered=true;m.aidRemaining=0;m.patient="";m.order="Hold";
            if(DockEnabled&&District.dock.componentOwner==id)District.dock.componentOwner="yard";
            State.arrests++;State.cash=Mathf.Max(0,State.cash-40);SetCrewPosition(id,City.Nav.SafePoint(Jobs.Home+Vector3.left*2));
            m.memory="The patrol seized my firearm and fined the crew. They released me at the refuge.";
            District.Record("crew arrest",id,m.memory);Heat=arrestProgress=0;ResetPolice();Save();Notify(id.ToUpper()+": "+m.memory);
        }
    }
}
