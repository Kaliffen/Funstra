using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    // A small fixed grid is shared by patrols, witnesses and the runtime traversal checks.
    public sealed class CityNavigation
    {
        public const int Side = 81;
        public const float Step = 2f;
        const float GridOrigin=(Side-1)*Step*.5f;
        public Vector2 Min {get;private set;}=new Vector2(-78,-68);
        public Vector2 Max {get;private set;}=new Vector2(78,68);
        public void SetBounds(Vector2 min,Vector2 max)
        {
            Min=new Vector2(Mathf.Clamp(min.x,-78,78),Mathf.Clamp(min.y,-78,78));
            Max=new Vector2(Mathf.Clamp(max.x,Min.x,78),Mathf.Clamp(max.y,Min.y,78));
            Bake();
        }
        public readonly List<Bounds> Obstacles = new List<Bounds>();
        public readonly List<Bounds> Props = new List<Bounds>();
        public readonly List<Bounds> Traffic = new List<Bounds>();
        readonly bool[] blocked = new bool[Side * Side];
        readonly int[] parent = new int[Side * Side];
        readonly float[] cost = new float[Side * Side];
        readonly List<int> open = new List<int>();
        readonly bool[] closed = new bool[Side * Side];
        public void Bake()
        {
            for (int i = 0; i < blocked.Length; i++) blocked[i] = !Walkable(Point(i), false);
        }
        public bool Walkable(Vector3 p, bool traffic = true)
        {
            if (p.x<Min.x||p.x>Max.x||p.z<Min.y||p.z>Max.y) return false;
            foreach (var b in Obstacles)
                if (p.x > b.min.x - .5499f && p.x < b.max.x + .5499f && p.z > b.min.z - .5499f && p.z < b.max.z + .5499f) return false;
            foreach(var b in Props)
                if(p.x>b.min.x-.5499f&&p.x<b.max.x+.5499f&&p.z>b.min.z-.5499f&&p.z<b.max.z+.5499f)return false;
            if (traffic) foreach (var b in Traffic)
                if (p.x > b.min.x-.5499f && p.x < b.max.x+.5499f && p.z > b.min.z-.5499f && p.z < b.max.z+.5499f) return false;
            return true;
        }
        public Vector3 SafePoint(Vector3 p)
        {
            if(Walkable(p))return p;
            Vector3 best=Vector3.zero;float distance=float.MaxValue;
            for(int z=Mathf.CeilToInt(Min.y*2);z<=Mathf.FloorToInt(Max.y*2);z++)for(int x=Mathf.CeilToInt(Min.x*2);x<=Mathf.FloorToInt(Max.x*2);x++)
            { var q=new Vector3(x*.5f,0,z*.5f);float d=(q-p).sqrMagnitude;if(d<distance&&Walkable(q)) {best=q;distance=d;} }
            return best;
        }
        public bool Sight(Vector3 from, Vector3 to)
        {
            Vector3 start=from+Vector3.up, end=to+Vector3.up;
            Vector3 d=end-start;
            if(d.sqrMagnitude<.000001f)return true;
            var ray=new Ray(start,d.normalized);
            return !Occludes(Obstacles,ray,d.magnitude)&&!Occludes(Props,ray,d.magnitude)&&!Occludes(Traffic,ray,d.magnitude);
        }
        static bool Occludes(List<Bounds> bounds,Ray ray,float length)
        {
            foreach(var b in bounds)
                if(b.Contains(ray.origin)||(b.IntersectRay(ray,out float distance)&&distance<length))return true;
            return false;
        }
        public bool ClearWalk(Vector3 a, Vector3 b, bool traffic = true)
        {
            if(!Walkable(a,traffic)||!Walkable(b,traffic))return false;
            Vector3 direction=new Vector3(b.x-a.x,0,b.z-a.z);float length=direction.magnitude;
            if(length<.0001f)return true;
            var ray=new Ray(new Vector3(a.x,0,a.z),direction/length);
            if(Crosses(ray,length,Obstacles)||Crosses(ray,length,Props))return false;
            return !traffic||!Crosses(ray,length,Traffic);
        }
        bool Crosses(Ray ray,float length,List<Bounds> bounds)
        {
            foreach(var b in bounds)
            {
                var expanded=new Bounds(new Vector3(b.center.x,0,b.center.z),new Vector3(b.size.x+1.0998f,2,b.size.z+1.0998f));
                if(expanded.IntersectRay(ray,out float distance)&&distance<length-.0001f)return true;
            }
            return false;
        }
        int Index(Vector3 p) => Mathf.Clamp(Mathf.RoundToInt((p.z + GridOrigin) / Step), 0, Side - 1) * Side + Mathf.Clamp(Mathf.RoundToInt((p.x + GridOrigin) / Step), 0, Side - 1);
        Vector3 Point(int i) => new Vector3((i % Side) * Step - GridOrigin, 0, (i / Side) * Step - GridOrigin);
        int Nearest(Vector3 p)
        {
            int at = Index(p);
            if (!blocked[at]&&ClearWalk(p,Point(at))) return at;
            float best = float.MaxValue; int result = -1;
            for (int i = 0; i < blocked.Length; i++)
                if (!blocked[i]) { float d = (Point(i) - p).sqrMagnitude; if (d < best&&ClearWalk(p,Point(i))) { best = d; result = i; } }
            return result;
        }
        public List<Vector3> Find(Vector3 from, Vector3 to)
        {
            var result = new List<Vector3>();
            from=SafePoint(from);to=SafePoint(to);
            if (ClearWalk(from, to)) { result.Add(to); return result; }
            for(int i=0;i<blocked.Length;i++)blocked[i]=!Walkable(Point(i));
            int start = Nearest(from), goal = Nearest(to);
            if (start < 0 || goal < 0) return result;
            for (int i = 0; i < cost.Length; i++) { cost[i] = float.MaxValue; parent[i] = -1; closed[i] = false; }
            open.Clear(); open.Add(start); cost[start] = 0;
            while (open.Count > 0)
            {
                int best = 0; float score = float.MaxValue;
                for (int j = 0; j < open.Count; j++)
                {
                    float f = cost[open[j]] + Vector3.Distance(Point(open[j]), Point(goal));
                    if (f < score) { best = j; score = f; }
                }
                int current = open[best]; open.RemoveAt(best);
                if (current == goal)
                {
                    for (int p = goal; p != -1; p = parent[p]) result.Add(Point(p));
                    result.Reverse();
                    if (ClearWalk(result[result.Count - 1], to)) result.Add(to);
                    // Keep every grid corner: agents cannot cut through building corners.
                    return result;
                }
                closed[current] = true;
                int x = current % Side, z = current / Side;
                for (int dz = -1; dz <= 1; dz++) for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dz == 0 || x + dx < 0 || x + dx >= Side || z + dz < 0 || z + dz >= Side) continue;
                    int next = (z + dz) * Side + x + dx;
                    if (blocked[next] || closed[next] || !ClearWalk(Point(current),Point(next))) continue;
                    if (dx != 0 && dz != 0 && (blocked[z * Side + x + dx] || blocked[(z + dz) * Side + x])) continue;
                    float c = cost[current] + (dx != 0 && dz != 0 ? 2.82843f : 2f);
                    if (c >= cost[next]) continue;
                    cost[next] = c; parent[next] = current;
                    if (!open.Contains(next)) open.Add(next);
                }
            }
            return result;
        }
    }
}
