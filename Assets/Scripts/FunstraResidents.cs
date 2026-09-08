using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        public bool residentsTest;
        public bool ResidentsEnabled => DistrictEnabled&&!FoundationMode&&(!Smoke||residentsTest||crewTest||crewPressure);
        public bool ResidentSees(TownAgent resident,Vector3 point,float range=16)
        {
            if(resident==null||resident.Record==null||resident.Record.health<=0)return false;
            Vector3 delta=point-resident.Position;delta.y=0;
            return delta.magnitude<range&&(delta.magnitude<3||Vector3.Dot(resident.Body.forward,delta.normalized)>.12f)&&City.Nav.Sight(resident.Position,point);
        }
        public void ResidentGunshot(Vector3 source,string owner)
        {
            if(!ResidentsEnabled)return;
            foreach(var a in Agents)if(!a.Police&&a.Record!=null&&a.Record.health>0&&a.Record.resident!=null&&Vector3.Distance(a.Position,source)<23)
            {
                bool first=a.Record.resident.dangerRemaining<=0;a.Record.resident.Hear(source);a.Path.Clear();a.Repath=0;
                if(first)District.Record("resident alarm",a.Record.id,a.Record.name+" heard a shot and sought shelter without identifying a shooter.");
            }
        }
        public void ResidentViolence(DistrictActor victim,Vector3 source,string owner)
        {
            if(!ResidentsEnabled||victim==null)return;
            foreach(var a in Agents)if(!a.Police&&a.Record!=null&&a.Record.resident!=null&&a.Record!=victim&&ResidentSees(a,source)&&ResidentSees(a,victim.position))
            {
                a.Record.resident.Witness(victim,source,owner);a.Path.Clear();a.Repath=0;
                District.Record("resident witness",a.Record.id,a.Record.name+" personally saw "+(owner=="player"?"you":owner)+" hurt "+victim.name+". "+(a.Record.resident.WillHelp(victim.id)?"Will look for a safe chance to help.":"Chose to reach shelter first."));
            }
        }
        public void ResidentRememberAid(DistrictActor patient,string helperId)
        {
            if(!ResidentsEnabled||patient==null||patient.resident==null)return;
            patient.resident.RememberHelp(helperId);
            District.Record("resident memory",patient.id,patient.name+": "+patient.resident.memory);
        }
        public void StepResident(TownAgent a,float dt)
        {
            var actor=a.Record;var p=actor.resident;
            if(p==null||p.version!=1||dt<=0)return;
            p.Step(dt);a.SeesPlayer=ResidentSees(a,Player.position)&&!Hidden;
            if(a.SeesPlayer&&Stealing&&Vector3.Distance(a.Position,Player.position)<10)
            {
                a.Suspicion+=dt;
                if(a.Suspicion>1.1f)
                {
                    RaiseAlarm(actor.name+" saw you stealing and called the police.",a.Position);a.Suspicion=0;
                    p.witnessedPlayer=true;p.offenderId="player";p.memory="I saw you stealing at this corner.";
                    p.threatPosition=Player.position;p.watchRemaining=p.disposition==2?6:0;
                }
            }
            else a.Suspicion=Mathf.Max(0,a.Suspicion-dt);
            if(p.witnessedPlayer&&a.SeesPlayer&&weapon>1)
            {p.threatPosition=Player.position;p.dangerRemaining=12;p.reason="Recognized the person I saw attacking a neighbor, still carrying a weapon.";}
            Vector3 target=a.Position;float speed=1.35f;bool moving=false;
            if(p.dangerRemaining>0)
            {
                if(!p.hasRefuge){p.refuge=ResidentRefuge(a,p.threatPosition);p.hasRefuge=true;a.Path.Clear();a.Repath=0;}
                target=p.refuge;speed=3.1f;moving=Vector3.Distance(a.Position,target)>.65f;
                p.action=moving?"Seeking shelter":"Keeping low";
                if(p.reason.Length==0)p.reason="Waiting for twelve quiet seconds after the last danger I perceived.";
            }
            else
            {
                p.hasRefuge=false;
                DistrictActor patient=string.IsNullOrEmpty(p.casualtyId)?null:District.citizens.Find(c=>c.id==p.casualtyId);
                if(patient==null&&p.casualtyId==District.neri.id)patient=District.neri;
                bool wounded=patient!=null&&patient.health>0&&(patient.bleeding||patient.health<75);
                if(wounded&&p.WillHelp(patient.id)&&actor.bandages>0)
                {
                    target=p.casualtyPosition;moving=Vector3.Distance(a.Position,target)>1.4f;speed=1.8f;
                    p.action="Returning to "+patient.name;p.reason="I saw them hurt. It has been quiet long enough to try my one dressing.";
                    if(ResidentSees(a,patient.position,18))
                    {
                        p.casualtyPosition=patient.position;
                        Vector3 aside=a.Position-patient.position;aside.y=0;if(aside.sqrMagnitude<.01f)aside=a.Body.forward;
                        target=patient.position+aside.normalized*1.35f;moving=Vector3.Distance(a.Position,patient.position)>1.6f;
                    }
                    else if(!moving)
                    {
                        p.action="Looking for "+patient.name;p.reason="Reached the place I saw them hurt; looking around before assuming where they went.";
                        a.Body.Rotate(0,dt*65,0);
                    }
                    if(p.TryAid(actor,patient,true,Vector3.Distance(a.Position,patient.position)<1.7f&&City.Nav.Sight(a.Position,patient.position)))
                    {District.Record("resident aid",actor.id,actor.name+" used one personal dressing on "+patient.name+" after waiting for safety.");p.pauseRemaining=4;a.Path.Clear();moving=false;}
                }
                else if(p.witnessedPlayer&&!p.helpedByPlayer&&a.SeesPlayer&&Vector3.Distance(a.Position,Player.position)<6)
                {
                    target=ResidentRefuge(a,Player.position);moving=true;speed=2;
                    p.action="Keeping away from you";p.reason="I remember seeing you hurt someone. I will not stop to talk.";
                }
                else if(p.helpedByPlayer&&a.SeesPlayer&&Vector3.Distance(a.Position,Player.position)<8)
                {
                    target=Player.position;moving=Vector3.Distance(a.Position,target)>2.4f;
                    p.action=moving?"Coming over to thank you":"Acknowledging you";p.reason="You spent a dressing on me. I remember who came back.";
                }
                else if(p.disposition==2&&p.watchRemaining>0)
                {
                    p.action="Watching the scene";p.reason="Keeping the place I personally witnessed in view, without following someone through walls.";
                    Vector3 look=p.threatPosition-a.Position;look.y=0;
                    if(look.sqrMagnitude>.1f)a.Body.rotation=Quaternion.Slerp(a.Body.rotation,Quaternion.LookRotation(look),dt*5);
                }
                else if(p.pauseRemaining>0)
                {p.action=p.disposition==2?"Watching the street":"Taking a brief pause";p.reason=ResidentCatalog.RoutineReason(p);}
                else
                {
                    p.routineStop%=a.Route.Length;target=a.Route[p.routineStop];moving=true;
                    p.action="On the evening round";p.reason=ResidentCatalog.RoutineReason(p);
                    if(Vector3.Distance(a.Position,target)<.8f)
                    {p.pauseRemaining=p.disposition==2?7:3+p.disposition;p.routineStop=(p.routineStop+1)%a.Route.Length;moving=false;a.Path.Clear();}
                }
            }
            ResidentMove(a,target,moving?speed:0,dt);actor.position=a.Position;
        }
        Vector3 ResidentRefuge(TownAgent a,Vector3 danger)
        {
            Vector3 best=a.Position;float score=Vector3.Distance(best,danger);
            Vector3 away=a.Position-danger;away.y=0;if(away.sqrMagnitude<.01f)away=-a.Body.forward;
            for(int i=0;i<7;i++)
            {
                float angle=(i-3)*25;
                Vector3 candidate=City.Nav.SafePoint(a.Position+Quaternion.Euler(0,angle,0)*away.normalized*(i%2==0?10:15));
                EvaluateResidentRefuge(a,danger,candidate,ref best,ref score);
            }
            foreach(Vector3 route in a.Route)
                EvaluateResidentRefuge(a,danger,City.Nav.SafePoint(route),ref best,ref score);
            return best;
        }
        void EvaluateResidentRefuge(TownAgent a,Vector3 danger,Vector3 candidate,ref Vector3 best,ref float bestScore)
        {
            var path=City.Nav.Find(a.Position,candidate);if(path.Count==0)return;
            Vector3 before=a.Position;float length=0,minDistance=Mathf.Max(1,Vector3.Distance(before,danger)-1);
            foreach(Vector3 next in path)
            {
                Vector3 segment=next-before;float t=segment.sqrMagnitude<.001f?0:Mathf.Clamp01(Vector3.Dot(danger-before,segment)/segment.sqrMagnitude);
                if(Vector3.Distance(before+segment*t,danger)<minDistance)return;
                length+=segment.magnitude;before=next;
            }
            if(length>20)return;
            float score=Vector3.Distance(candidate,danger)-length*.15f;
            if(score>bestScore){bestScore=score;best=candidate;}
        }
        bool ResidentMoveClear(TownAgent a,Vector3 next)
        {
            foreach(var other in Agents)if(other!=a&&other.Record!=null&&other.Record.health>0&&Vector3.Distance(next,other.Position)<.78f&&Vector3.Distance(next,other.Position)<Vector3.Distance(a.Position,other.Position))return false;
            if(Vector3.Distance(next,Player.position)<.85f&&Vector3.Distance(next,Player.position)<Vector3.Distance(a.Position,Player.position))return false;
            foreach(var other in new[]{District.neri,District.guard,District.collector})if(other!=null&&other.health>0&&Vector3.Distance(next,other.position)<.78f&&Vector3.Distance(next,other.position)<Vector3.Distance(a.Position,other.position))return false;
            return true;
        }
        void ResidentMove(TownAgent a,Vector3 target,float speed,float dt)
        {
            Vector3 before=a.Position;
            if(speed<=0){a.Path.Clear();a.Repath=0;}
            else
            {
                a.Repath-=dt;
                if(a.Repath<=0)
                {
                    a.Path=City.Nav.Find(a.Position,target);
                    if(a.Path.Count>1&&City.Nav.ClearWalk(a.Position,a.Path[1]))a.Path.RemoveAt(0);
                    a.Repath=.8f;
                }
                if(a.Path.Count>0)
                {
                    Vector3 next=a.Path[0];next.y=0;
                    if(Vector3.Distance(a.Position,next)<.18f)a.Path.RemoveAt(0);
                    else
                    {
                        Vector3 move=Vector3.MoveTowards(a.Position,next,speed*dt);
                        if(City.Nav.ClearWalk(a.Position,move)&&ResidentMoveClear(a,move))a.Body.position=move;
                        else
                        {
                            Vector3 forward=(next-a.Position).normalized,side=new Vector3(forward.z,0,-forward.x);
                            bool passed=false;
                            for(int attempt=0;attempt<2&&!passed;attempt++)
                            {
                                Vector3 pass=a.Position+side*(attempt==0?1:-1)*speed*dt;
                                if(City.Nav.ClearWalk(a.Position,pass)&&ResidentMoveClear(a,pass)){a.Body.position=pass;passed=true;}
                            }
                            a.Repath=0;
                        }
                        Vector3 heading=next-a.Position;if(heading.sqrMagnitude>.01f)a.Body.rotation=Quaternion.Slerp(a.Body.rotation,Quaternion.LookRotation(heading),dt*9);
                    }
                }
            }
            a.Phase+=dt;CityArt.Animate(a.Body,a.Phase,Vector3.Distance(before,a.Position)/Mathf.Max(dt,.001f));
        }
    }
}
