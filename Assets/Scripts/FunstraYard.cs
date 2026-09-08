using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        void InitializeYard()
        {
            foreach(var body in yardBodies)if(body)Destroy(body.gameObject);
            yardBodies.Clear();
            ClearExtraCombatActors();
            if(!DistrictEnabled||FoundationMode&&foundationLevel==1) {yardSquad=null;return;}
            if(District.squad==null)District.squad=new CombatSquadState();
            yardSquad=new CombatSquad(District.squad,City.Nav,FireActorAt,StepActorWeapon);
            if(!District.squad.initialized)
            {
                int count=FoundationMode&&foundationLevel==2?2:3;
                for(int i=0;i<count;i++)
                {
                    var position=City.Nav.SafePoint(City.TestEnemySpawns[i%City.TestEnemySpawns.Count]);
                    var actor=new DistrictActor("yard-"+i,FoundationMode&&foundationLevel==2?"MOVING TARGET "+(i+1):new[]{"VALE / WATCH","OSS / RUNNER","LEN / REAR GUARD"}[i],position);
                    actor.health=80;actor.ammo=FoundationMode&&foundationLevel==2?0:18;actor.combat=new WeaponState{kind=i==1?3:2};
                    yardSquad.Add(actor,null,(CombatRole)i,position,City.TestRetreatPoints[i%City.TestRetreatPoints.Count],i==1?3:2);
                }
            }
            if(CrewEnabled)District.squad.InitializeCrewOpposition();
            foreach(var member in yardSquad.Members)
            {
                var body=City.Human(member.Actor.name,member.Actor.position,FoundationMode&&foundationLevel==2?CityArt.Amber:CityArt.Hex("A96F58"));
                City.Box(member.Data.gun==5?"Guard rifle":"Guard weapon",new Vector3(.4f,1,.4f),new Vector3(.13f,.16f,member.Data.gun==5?1.1f:member.Data.gun==3?.85f:.45f),CityArt.Hex("27303C"),body);
                body.rotation=Quaternion.Euler(0,180,0);yardBodies.Add(body);
                yardSquad.Bind(member.Actor.id,body);RegisterCombatActor(member.Actor,body);
            }
        }
        void UpdateYard(float dt)
        {
            if(City.HasServiceGate&&Vector3.Distance(ControlledPosition,City.GatePosition)<6&&Input.GetKeyDown(KeyCode.E))
            {
                var occupants=new List<Vector3>{Player.position,District.neri.position,District.guard.position,District.collector.position};
                if(CrewEnabled)occupants.AddRange(LivingCrewPositions);
                foreach(var agent in Agents)occupants.Add(agent.Position);
                if(yardSquad!=null)foreach(var m in yardSquad.Members)occupants.Add(m.Actor.position);
                bool changed=City.SetServiceGate(!City.GateClosed,occupants);
                if(changed) {District.yardGateClosed=City.GateClosed;Save();}
                Notify(changed?(City.GateClosed?"Service gate closed. The public approach remains open.":"Service gate open. Watch the new sightline."):"Gate blocked by a person. Clear the opening first.");
            }
            if(yardSquad==null)return;
            if(FoundationMode&&foundationLevel==2)
            {
                for(int i=0;i<yardSquad.Members.Count;i++)
                {
                    var m=yardSquad.Members[i];if(m.Actor.health<=0) {PoseActor(m.Body,m.Actor);continue;}
                    Vector3 target=m.Data.home+Vector3.right*(Mathf.Sin(Elapsed*.65f+i)*3);
                    Vector3 next=Vector3.MoveTowards(m.Actor.position,target,2*dt);
                    if(City.Nav.ClearWalk(m.Actor.position,next))m.Actor.position=next;
                    m.Body.position=m.Actor.position;CityArt.Animate(m.Body,Elapsed,2);m.Order="Moving target";
                }
                return;
            }
            if(CrewEnabled)
            {
                var targets=new List<Vector3>();bool engaged=false;
                foreach(var point in LivingCrewPositions)
                {
                    if(DockYardEngaged(point))engaged=true;
                    if(!Hidden||(point-Player.position).sqrMagnitude>.0001f)targets.Add(point);
                }
                yardSquad.Step(dt,targets,engaged);
            }
            else yardSquad.Step(dt,Player.position,!Hidden,FoundationMode||Player.position.x>8&&Player.position.z>29);
            foreach(var m in yardSquad.Members)PoseActor(m.Body,m.Actor);
        }
        void DrawYardStatus()
        {
            if(yardSquad==null)return;
            foreach(var member in yardSquad.Members)
            {
                if(!FoundationMode&&Vector3.Distance(ControlledPosition,member.Actor.position)>24)continue;
                var sp=View.WorldToScreenPoint(member.Actor.position+Vector3.up*2.5f);if(sp.z<=0)continue;
                float x=sp.x/Screen.width*W,y=(1-sp.y/Screen.height)*H;
                if(x<340||x>1560||y<100||y>780)continue;
                Text(member.Actor.name+" / "+Mathf.CeilToInt(member.Actor.health),x-140,y-20,280,25,14,CityArt.Amber,FontStyle.Bold,TextAnchor.MiddleCenter);
                if(FoundationMode||CrewEnabled)Text(member.Actor.health<=0?"DOWN":FoundationMode&&foundationLevel==2?"MOVING TARGET":member.Order+(member.DirectSight?" / CONTACT":member.ContactAge<7?" / LAST CONTACT":" / NO CONTACT"),x-145,y+6,290,25,12,paper,FontStyle.Normal,TextAnchor.MiddleCenter);
            }
        }
    }
    public sealed partial class DistrictState
    {
        public bool yardGateClosed=true;
    }
}
