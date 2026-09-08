using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    [Serializable] public sealed class PoliceDispatch
    {
        public bool requested, entered, arrived;
        public int deployed;
        public float delay, blockedTime;
        public Vector3 position, destination;
    }
    [Serializable] public sealed class PoliceResponse
    {
        public bool identifiedGunman;
        public int crewVersion;
        public int harm;
        public float searchRemaining, contactRemaining;
        public Vector3 lastKnown;
        public string observer="";
        public PoliceDispatch[] trucks={new PoliceDispatch(),new PoliceDispatch()};
        public List<DistrictActor> officers=new List<DistrictActor>();
        public bool Searching=>searchRemaining>0;
        public const float ContactLifetime=.5f;
        public bool HasContact=>identifiedGunman&&contactRemaining>0;
        static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        static bool Point(Vector3 v)=>Finite(v.x)&&Finite(v.y)&&Finite(v.z)&&Mathf.Abs(v.x)<=80&&Mathf.Abs(v.z)<=80;
        public bool Valid()
        {
            if(crewVersion<0||crewVersion>1||harm<0||harm>100||!Finite(searchRemaining)||searchRemaining<0||searchRemaining>45.01f||!Finite(contactRemaining)||contactRemaining<0||contactRemaining>ContactLifetime||contactRemaining>searchRemaining||contactRemaining>0&&!identifiedGunman||!Point(lastKnown)||trucks==null||trucks.Length!=2||officers==null||officers.Count>6)return false;
            int delivered=0;var ids=new HashSet<string>();
            for(int i=0;i<2;i++)
            {
                var t=trucks[i];if(t==null||t.deployed<0||t.deployed>3||!Finite(t.delay)||t.delay<0||!Finite(t.blockedTime)||t.blockedTime<0||!Point(t.position)||!Point(t.destination)||t.arrived&&!t.entered||t.entered&&!t.requested||t.deployed>0&&!t.arrived)return false;
                delivered+=t.deployed;
            }
            foreach(var a in officers)if(a==null||string.IsNullOrEmpty(a.id)||!a.id.StartsWith("response-")||!ids.Add(a.id)||!Point(a.position)||!Finite(a.health)||a.health<0||a.health>100||a.ammo<0||a.ammo>18||a.combat==null||!a.combat.Valid(a.ammo)||(a.combat.kind!=2&&a.combat.kind!=5))return false;
            for(int i=0;i<2;i++)for(int slot=0;slot<trucks[i].deployed;slot++)
                if(!ids.Contains("response-"+i+"-"+slot))return false;
            return delivered==officers.Count;
        }
        public void InitializeCrewResponse()
        {
            if(crewVersion!=0)return;
            crewVersion=1;
            foreach(var officer in officers)
            {
                if(officer.combat==null)officer.combat=new WeaponState();
                officer.combat.Initialize(officer.ammo,5);
            }
        }
        public void Noise(Vector3 point)
        { if(identifiedGunman)return;lastKnown=point;searchRemaining=12;observer="gunshot heard"; }
        public void Violence(Vector3 point,int severity,string witness)
        {
            identifiedGunman=true;lastKnown=point;observer=witness;harm=Mathf.Clamp(harm+Mathf.Max(0,severity),0,100);searchRemaining=45;contactRemaining=ContactLifetime;
            for(int i=0;i<2;i++)if(harm>=(i+1)*2&&!trucks[i].requested)
            {trucks[i].requested=true;trucks[i].delay=4+i*5;trucks[i].position=new Vector3(i==0?3:-3,0,i==0?-64:64);trucks[i].destination=new Vector3(i==0?3:-3,0,Mathf.Clamp(point.z,-28,28));}
        }
        public void Sight(Vector3 point){if(identifiedGunman){lastKnown=point;searchRemaining=45;contactRemaining=ContactLifetime;}}
        public void Step(float dt)
        {
            if(dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;
            searchRemaining=Mathf.Max(0,searchRemaining-dt);
            contactRemaining=Mathf.Max(0,contactRemaining-dt);
            if(searchRemaining==0){identifiedGunman=false;harm=0;}
        }
    }
    public sealed partial class FunstraGame
    {
        readonly Transform[] policeTrucks=new Transform[2];
        public PoliceResponse Police=>District.police??(District.police=new PoliceResponse());
        public int PoliceOfficerCount=>Agents.FindAll(a=>a.Police&&a.Record!=null&&a.Record.health>0).Count;
        public int PoliceReinforcementCount=>Police.officers.Count;
        public int PoliceTruckCount=>Array.FindAll(policeTrucks,t=>t!=null).Length;
        public string PoliceStatus=>Police.identifiedGunman?(Police.HasContact?"CONTACT / ":"SEARCH / ")+PoliceOfficerCount+" OFFICERS":Police.Searching?"GUNSHOT / AREA SEARCH":"";
        public void ReportPoliceNoise(Vector3 point)
        {
            if(FoundationMode||!DistrictEnabled)return;
            if(Police.identifiedGunman)return;Police.Noise(point);Notify("Gunshot heard. Officers are checking the reported location.");
        }
        public void ReportPoliceViolence(Vector3 point,int severity,string observer)
        {
            if(FoundationMode||!DistrictEnabled)return;
            bool first=!Police.identifiedGunman;Police.Violence(point,severity,observer);
            Heat=45;LastSeen=point;lastSight=Elapsed;
            foreach(var a in Agents)if(a.Police&&a.Record!=null&&a.Record.health>0){a.Pursuing=true;a.LastSeen=point;a.Repath=0;}
            if(first){Sound(alertSound);Notify("Armed attacker reported. All police share the last sighting. Break sight and leave the area.");}
        }
        string ViolenceWitness(DistrictActor victim=null,Vector3? attackPosition=null)
        {
            Vector3 source=attackPosition??Player.position;
            // A struck, surviving victim can turn toward the attack and identify a nearby
            // assailant across clear sight. A wall still prevents identification.
            if(victim!=null&&victim.health>0&&Vector3.Distance(victim.position,source)<19&&City.Nav.Sight(victim.position,source))return victim.id;
            // Re-evaluate at the event, rather than trusting a previous frame's perception.
            foreach(var a in Agents)
                if(a.Record!=null&&ActorSees(a.Record,a.Body,source,19))return a.Record.id;
            if(ActorSees(District.guard,guardBody,source,16))return District.guard.id;
            if(ActorSees(District.collector,collectorBody,source,14))return District.collector.id;
            if((!CrewEnabled||!District.recruited)&&ActorSees(District.neri,neriBody,source,14))return District.neri.id;
            return null;
        }
        public void ClearPoliceRuntime()
        {
            for(int i=Agents.Count-1;i>=0;i--)if(Agents[i].Reinforcement)
            {Agents[i].Body.gameObject.SetActive(false);Destroy(Agents[i].Body.gameObject);Agents.RemoveAt(i);}
            for(int i=0;i<2;i++)if(policeTrucks[i]){policeTrucks[i].gameObject.SetActive(false);Destroy(policeTrucks[i].gameObject);policeTrucks[i]=null;}
            City.ResponseTrafficObstacles.RemoveAll(c=>!c||!c.gameObject.activeInHierarchy);
            City.RefreshProps();
        }
        public void ResetPoliceResponse()
        {
            ClearPoliceRuntime();District.police=new PoliceResponse();District.projectiles.RemoveAll(p=>p.owner.StartsWith("response-")||Agents.Exists(a=>a.Police&&a.Record!=null&&a.Record.id==p.owner));
        }
        public void InitializePoliceResponse()
        {
            ClearPoliceRuntime();if(!DistrictEnabled||FoundationMode)return;
            if(CrewEnabled)Police.InitializeCrewResponse();
            for(int i=0;i<2;i++)if(Police.trucks[i].entered)CreatePoliceTruck(i);
            foreach(var record in Police.officers)CreateResponseOfficer(record);
            City.RefreshProps();CacheCombatObstacles();
        }
        void CreateResponseOfficer(DistrictActor record)
        {
            record.weaponRecoverable=!record.looted;
            if(Agents.Exists(a=>a.Record==record))return;
            var a=new TownAgent{Police=true,Reinforcement=true,Record=record,Route=new[]{record.position,record.position+Vector3.forward*3},Phase=Agents.Count*.73f};
            a.Body=City.Human(record.name,record.position,CityArt.Hex("577FC4"),true);
            bool rifle=record.combat!=null&&record.combat.kind==5;
            City.Box(rifle?"Police rifle":"Police pistol",new Vector3(.4f,.95f,.3f),new Vector3(.12f,.15f,rifle?1.1f:.45f),CityArt.Hex("303848"),a.Body);
            PoseActor(a.Body,record);Agents.Add(a);
        }
        void CreatePoliceTruck(int index)
        {
            if(policeTrucks[index])return;
            var d=Police.trucks[index];var root=new GameObject("Compact response truck "+(index+1)).transform;
            root.SetParent(City.Root);root.position=d.position;root.rotation=Quaternion.Euler(0,index==0?0:180,0);policeTrucks[index]=root;
            var hull=City.Prop("Response truck / shared solid hull",new Vector3(0,1,0),new Vector3(2.2f,2,5.2f),CityArt.Hex("3E5876"),root);
            City.ResponseTrafficObstacles.Add(hull.GetComponent<Collider>());
            City.Box("Truck cab",new Vector3(0,2,1.3f),new Vector3(2,1,1.9f),CityArt.Hex("657E99"),root);
            City.Box("Truck windscreen",new Vector3(0,2.1f,2.27f),new Vector3(1.75f,.6f,.06f),CityArt.Hex("243747"),root);
            City.Box("Police light bar",new Vector3(0,2.6f,1.3f),new Vector3(1.4f,.15f,.3f),CityArt.Blue,root,false,true);
            for(int side=-1;side<=1;side+=2)for(int end=-1;end<=1;end+=2)
                City.Shape("Truck wheel",PrimitiveType.Cylinder,new Vector3(side*1.12f,.5f,end*1.7f),new Vector3(.85f,.16f,.85f),CityArt.Hex("202635"),root).transform.localRotation=Quaternion.Euler(0,0,90);
            City.Sign("COMPACT",new Vector3(0,1.4f,-2.64f),Color.white,.16f,root);
            City.RefreshProps();CacheCombatObstacles();
        }
        Bounds TruckBounds(Vector3 point)=>new Bounds(point+Vector3.up,new Vector3(2.2f,2,5.2f));
        bool TruckSpaceClear(int index,Vector3 point)
        {
            var b=TruckBounds(point);var gap=b;gap.Expand(new Vector3(.9f,2,.9f));
            foreach(var obstacle in City.Nav.Obstacles)if(gap.Intersects(obstacle))return false;
            foreach(var car in City.Cars)if(gap.Intersects(car.collider.bounds))return false;
            foreach(var obstacle in City.Nav.Props)
            {
                if(policeTrucks[index]&&(obstacle.center-TruckBounds(policeTrucks[index].position).center).sqrMagnitude<.001f)continue;
                if(gap.Intersects(obstacle))return false;
            }
            if(gap.Contains(new Vector3(Player.position.x,1,Player.position.z)))return false;
            if(CrewEnabled)foreach(var pointInCrew in LivingCrewPositions)if(gap.Contains(new Vector3(pointInCrew.x,1,pointInCrew.z)))return false;
            foreach(var a in Agents)if(gap.Contains(new Vector3(a.Position.x,1,a.Position.z)))return false;
            foreach(var a in new[]{District.neri,District.guard,District.collector})if(gap.Contains(new Vector3(a.position.x,1,a.position.z)))return false;
            return true;
        }
        bool OfficerExitClear(Vector3 point)
        {
            if(!City.Nav.Walkable(point)||Vector3.Distance(point,Player.position)<1.2f)return false;
            if(CrewEnabled)foreach(var crewPoint in LivingCrewPositions)if(Vector3.Distance(point,crewPoint)<1.2f)return false;
            foreach(var a in Agents)if(Vector3.Distance(point,a.Position)<1.2f)return false;
            foreach(var a in new[]{District.neri,District.guard,District.collector})if(Vector3.Distance(point,a.position)<1.2f)return false;
            return true;
        }
        public void StepPoliceResponse(float dt)
        {
            if(!DistrictEnabled||FoundationMode||dt<=0)return;
            Police.Step(dt);
            for(int i=0;i<2;i++)
            {
                var d=Police.trucks[i];if(!d.requested)continue;
                if(!d.entered)
                {
                    if(!Police.identifiedGunman)continue;
                    d.delay=Mathf.Max(0,d.delay-dt);if(d.delay>0||!TruckSpaceClear(i,d.position))continue;
                    d.entered=true;CreatePoliceTruck(i);Notify("Police truck entering Old Port. Reinforcements are arriving by road.");
                }
                if(!d.arrived)
                {
                    Vector3 next=Vector3.MoveTowards(d.position,d.destination,5*dt);
                    if(TruckSpaceClear(i,next)){d.position=next;policeTrucks[i].position=next;City.RefreshProps();d.blockedTime=0;}
                    else d.blockedTime+=dt;
                    // A roadblock makes the truck halt and unload where it really is.
                    // No collision is bypassed; officers must still find clear side-door exits.
                    if(Vector3.Distance(d.position,d.destination)<.01f||d.blockedTime>=4)d.arrived=true;
                }
                if(d.arrived&&d.deployed<3)
                {
                    // Try both physical side doors. If both are occupied, keep this
                    // officer aboard rather than pushing a person aside or finding a remote spawn.
                    Vector3 exit=d.position+new Vector3(i==0?2.1f:-2.1f,0,(d.deployed-1)*1.4f);
                    if(!OfficerExitClear(exit))
                    {
                        exit=d.position+new Vector3(i==0?-2.1f:2.1f,0,(d.deployed-1)*1.4f);
                        if(!OfficerExitClear(exit))continue;
                    }
                    var record=new DistrictActor("response-"+i+"-"+d.deployed,"RESPONSE OFFICER "+(i*3+d.deployed+1),exit){ammo=18,combat=new WeaponState{kind=CrewEnabled?5:2}};
                    Police.officers.Add(record);d.deployed++;CreateResponseOfficer(record);
                }
            }
        }
    }
}
