using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed class TownAgent
    {
        public Transform Body;
        public DistrictActor Record;
        public bool Police;
        public bool SeesPlayer;
        public bool Pursuing;
        public float Suspicion;
        public Vector3 LastSeen;
        public Vector3[] Route;
        public int Stop;
        public List<Vector3> Path = new List<Vector3>();
        public float Repath;
        public float Phase;
        public Vector3 Position => Body.position;
        public void Step(FunstraGame game, float dt)
        {
            if(game.DistrictEnabled&&Record!=null&&Record.health<=0)
            { Body.localScale=new Vector3(1.7f,.22f,1);SeesPlayer=Pursuing=false;return; }
            var nav = game.City.Nav;
            float distance = Vector3.Distance(Position, game.Player.position);
            var delta = game.Player.position - Position;
            float range = game.Sneaking ? (game.State.HasPerk(0) ? 7 : 9) : 14;
            bool inCone = distance < 3 || Vector3.Dot(Body.forward, delta.normalized) > .12f;
            SeesPlayer = !game.Hidden && distance < (Pursuing ? 19 : range) && (Pursuing || inCone) && nav.Sight(Position, game.Player.position);
            if (Police)
            {
                bool suspicious = game.Stealing || game.Heat > 0;
                if (SeesPlayer && suspicious)
                {
                    Suspicion += dt * (game.Heat > 0 ? 4 : 1.25f);
                    if (Suspicion >= 1) { Pursuing = true; LastSeen = game.Player.position; game.ReportSight(LastSeen); }
                }
                else Suspicion = Mathf.Max(0, Suspicion - dt*.5f);
                if (game.Heat <= 0) Pursuing = false;
            }
            else if (SeesPlayer && game.Stealing && distance < 10)
            {
                Suspicion += dt;
                if (Suspicion > 1.1f) { game.RaiseAlarm("A witness called the police.", Position); Suspicion = 0; }
            }
            Vector3 target;
            if (Police && game.Heat > 0)
            {
                Pursuing = true;
                target = SeesPlayer ? game.Player.position : game.LastSeen;
                if (!SeesPlayer && Vector3.Distance(Position, target) < 2)
                {
                    // Sweep nearby streets around the last sighting instead of tracking through walls.
                    float a = (game.Elapsed*.3f + Phase) % (Mathf.PI*2);
                    Vector3 sweep = game.LastSeen + new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*7;
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
            Repath -= dt;
            if(Repath <= 0) { Path = nav.Find(Position,target); Repath = Police && game.Heat > 0 ? .45f : 1.8f; }
            float speed = Police ? (game.Heat > 0 ? 4.35f : 1.9f) : 1.35f;
            Vector3 previous = Position;
            if(Path.Count > 0)
            {
                var next = Path[0]; next.y = 0;
                if(Vector3.Distance(Position,next) < .16f) Path.RemoveAt(0);
                else
                {
                    Vector3 move = Vector3.MoveTowards(Position,next,speed*dt);
                    if(nav.ClearWalk(Position,move)) Body.position = move;
                    else Repath = 0;
                    Vector3 heading = next - Position;
                    if(heading.sqrMagnitude > .01f) Body.rotation = Quaternion.Slerp(Body.rotation,Quaternion.LookRotation(heading),dt*9);
                }
            }
            Phase += dt;
            CityArt.Animate(Body,Phase,Vector3.Distance(previous,Position)/Mathf.Max(dt,.001f));
            if(game.DistrictEnabled&&Record!=null)Record.position=Position;
        }
    }
}
