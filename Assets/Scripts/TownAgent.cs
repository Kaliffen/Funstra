using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed class TownAgent
    {
        public Transform Body;
        public DistrictActor Record;
        public bool Police;
        public bool Reinforcement;
        public float FireDelay;
        public bool SeesPlayer;
        public bool Pursuing;
        public float Suspicion;
        public Vector3 LastSeen;
        public Vector3[] Route;
        public int Stop;
        public List<Vector3> Path = new List<Vector3>();
        public float Repath;
        public float Phase;
        Vector3 passingPoint;
        bool passing;
        public Vector3 Position => Body.position;
        public void Step(FunstraGame game, float dt)
        {
            if(Path.Count==0)passing=false;
            if(game.DistrictEnabled&&Record!=null&&Record.health<=0)
            { Body.localScale=new Vector3(1.7f,.22f,1);SeesPlayer=Pursuing=passing=false;return; }
            var nav = game.City.Nav;
            float distance = Vector3.Distance(Position, game.Player.position);
            var delta = game.Player.position - Position;
            float range = game.Sneaking ? (game.State.HasPerk(0) ? 7 : 9) : 14;
            bool inCone = distance < 3 || Vector3.Dot(Body.forward, delta.normalized) > .12f;
            SeesPlayer = !game.Hidden && distance < (Pursuing ? 19 : range) && (Pursuing || inCone) && nav.Sight(Position, game.Player.position);
            if (Police)
            {
                if(Record!=null)game.StepActorWeapon(Record,dt);FireDelay=Mathf.Max(0,FireDelay-dt);
                bool suspicious = game.Stealing || game.Heat > 0;
                if (SeesPlayer && suspicious)
                {
                    Suspicion += dt * (game.Heat > 0 ? 4 : 1.25f);
                    if (Suspicion >= 1) { Pursuing = true; LastSeen = game.Player.position; game.ReportSight(LastSeen); }
                }
                else Suspicion = Mathf.Max(0, Suspicion - dt*.5f);
                if (game.Heat <= 0&&!game.Police.Searching) Pursuing = false;
            }
            else if (SeesPlayer && game.Stealing && distance < 10)
            {
                Suspicion += dt;
                if (Suspicion > 1.1f) { game.RaiseAlarm("A witness called the police.", Position); Suspicion = 0; }
            }
            Vector3 target;
            if (Police && (game.Heat > 0||game.Police.Searching))
            {
                Pursuing = true;
                bool identified=game.Heat>0;
                Vector3 report=game.Police.Searching?game.Police.lastKnown:game.LastSeen;
                target = identified&&SeesPlayer ? game.Player.position : report;
                LastSeen=target;
                if (!SeesPlayer && Vector3.Distance(Position, target) < 2)
                {
                    // Sweep nearby streets around the last sighting instead of tracking through walls.
                    float a = (game.Elapsed*.3f + Phase) % (Mathf.PI*2);
                    Vector3 sweep = report + new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*7;
                    if(nav.Walkable(sweep)) target = sweep;
                }
            }
            else
            {
                target = Route[Stop];
                if(game.DistrictEnabled&&Record!=null&&Record.order=="Flee"&&Vector3.Distance(Position,game.Player.position)<12)
                {
                    var escape=Position+(Position-game.Player.position).normalized*10;
                    if(nav.Walkable(escape))target=escape;
                }
                if (Vector3.Distance(Position,target) < 1) { Stop = (Stop+1)%Route.Length; target = Route[Stop]; }
            }
            bool armedContact=Police&&game.Police.identifiedGunman&&SeesPlayer&&Record!=null&&Record.health>0&&Record.ammo>0;
            bool withdrawing=armedContact&&distance<4;
            if(withdrawing)
            {
                // Preserve room to use the pistol if an advancing player or another
                // officer has forced this shooter inside its useful firing distance.
                Vector3 away=Position-game.Player.position;away.y=0;
                if(away.sqrMagnitude<.01f)away=-Body.forward;
                Vector3 standOff=Position+away.normalized*3;
                if(nav.ClearWalk(Position,standOff))
                {target=standOff;Path.Clear();Path.Add(target);Repath=.45f;passing=false;}
                else if(!passing)passing=TryPolicePass(game,game.Player.position,out passingPoint);
            }
            bool firing=armedContact&&distance<17&&distance>3;
            bool clearShot=firing&&!withdrawing&&PoliceShotClear(game);
            if(clearShot)passing=false;
            else if(firing&&!withdrawing&&!passing)
                passing=TryPolicePass(game,game.Player.position,out passingPoint);
            if(clearShot&&FireDelay<=0)
            {if(game.FireActorAt(Record,Body,game.Player.position,2))FireDelay=1.45f+(Phase%3)*.13f;}
            Repath -= dt;
            if(Repath <= 0)
            {
                Path = nav.Find(Position,target);
                // Frequent pursuit replans may choose the grid node just behind us.
                // Skip that connector only when the following segment is fully clear;
                // otherwise diagonal edges longer than a replan interval cause oscillation.
                if(Police&&Path.Count>1&&nav.ClearWalk(Position,Path[1]))Path.RemoveAt(0);
                Repath = Police && game.Heat > 0 ? .45f : 1.8f;
            }
            float speed = Police ? (game.Heat > 0 ? 4.35f : 1.9f) : 1.35f;
            Vector3 previous = Position;
            if(Path.Count > 0&&!clearShot)
            {
                var next = passing?passingPoint:Path[0]; next.y = 0;
                if(Vector3.Distance(Position,next) < .16f)
                {if(passing)passing=false;else Path.RemoveAt(0);}
                else
                {
                    Vector3 move = Vector3.MoveTowards(Position,next,speed*dt);
                    bool groundClear=nav.ClearWalk(Position,move);
                    if(groundClear&&(!Police||PoliceMoveClear(game,move))) Body.position = move;
                    else
                    {
                        // Grid routes cannot see people. Give a blocked officer a short
                        // sideways step around a stationary colleague, on shared clear ground.
                        bool wasPassing=passing;passing=false;
                        if(Police&&groundClear&&!wasPassing)passing=TryPolicePass(game,next,out passingPoint);
                        Repath=Mathf.Min(Repath,.2f);
                    }
                    Vector3 heading = next - Position;
                    if(heading.sqrMagnitude > .01f) Body.rotation = Quaternion.Slerp(Body.rotation,Quaternion.LookRotation(heading),dt*9);
                }
            }
            Phase += dt;
            CityArt.Animate(Body,Phase,Vector3.Distance(previous,Position)/Mathf.Max(dt,.001f));
            if(game.DistrictEnabled&&Record!=null)Record.position=Position;
        }
        bool PoliceMoveClear(FunstraGame game,Vector3 next)
        {
            foreach(var other in game.Agents)if(other!=this&&other.Record!=null&&other.Record.health>0)
                if(Vector3.Distance(next,other.Position)<.78f&&Vector3.Distance(next,other.Position)<Vector3.Distance(Position,other.Position))return false;
            if(Vector3.Distance(next,game.Player.position)<.78f&&Vector3.Distance(next,game.Player.position)<Vector3.Distance(Position,game.Player.position))return false;
            if(game.DistrictEnabled)
                foreach(var other in new[]{game.District.neri,game.District.guard,game.District.collector})if(other.health>0)
                    if(Vector3.Distance(next,other.position)<.78f&&Vector3.Distance(next,other.position)<Vector3.Distance(Position,other.position))return false;
            return true;
        }
        bool TryPolicePass(FunstraGame game,Vector3 next,out Vector3 point)
        {
            Vector3 forward=(next-Position).normalized,side=new Vector3(forward.z,0,-forward.x);
            int identity=0;if(Record!=null)foreach(char c in Record.id)identity+=c;
            if(identity%2==1)side=-side;
            for(int attempt=0;attempt<2;attempt++)
            {
                point=Position+side*(attempt==0?1.15f:-1.15f);
                if(game.City.Nav.ClearWalk(Position,point)&&PoliceMoveClear(game,point))return true;
            }
            point=Position;return false;
        }
        bool PoliceShotClear(FunstraGame game)
        {
            Vector3 origin=Position+Vector3.up*1.1f,end=game.Player.position+Vector3.up*1.1f;
            foreach(var other in game.Agents)if(other!=this&&other.Record!=null&&other.Record.health>0)
                if(ProjectileMath.MovingSphere(origin,end,other.Position+Vector3.up*1.1f,other.Position+Vector3.up*1.1f,.65f,out _))return false;
            foreach(var other in new[]{game.District.neri,game.District.guard,game.District.collector})if(other.health>0)
                if(ProjectileMath.MovingSphere(origin,end,other.position+Vector3.up*1.1f,other.position+Vector3.up*1.1f,.65f,out _))return false;
            return true;
        }

    }
}
