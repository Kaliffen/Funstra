using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed class StreetCar
    {
        public Transform body;
        public BoxCollider collider;
        public Vector3[] route;
        public int stop;
        public bool yielding, crossing;
        public Bounds BoundsAt(Vector3 p,Vector3 direction)
        { return new Bounds(p+Vector3.up*.8f,Mathf.Abs(direction.x)>.5f?new Vector3(4,1.6f,2):new Vector3(2,1.6f,4)); }
    }
    public sealed partial class CityArt
    {
        public readonly List<StreetCar> Cars=new List<StreetCar>();
        void RegisterTraffic(Transform body,Vector3 p,float angle)
        {
            Vector3[] route=angle==0
                ? new[]{new Vector3(3,0,-39),new Vector3(3,0,39),new Vector3(-3,0,39),new Vector3(-3,0,-39)}
                : new[]{new Vector3(-43,0,p.z),new Vector3(43,0,p.z)};
            int stop=angle==0?(p.x>0?1:3):1;
            body.name="Street traffic / yields to pedestrians";
            var collider=body.gameObject.AddComponent<BoxCollider>();collider.center=Vector3.up*.8f;collider.size=new Vector3(2,1.6f,4);body.gameObject.layer=8;
            Cars.Add(new StreetCar{body=body,collider=collider,route=route,stop=stop});
            body.rotation=Quaternion.LookRotation(route[stop]-p);
            Nav.Traffic.Add(Cars[Cars.Count-1].BoundsAt(p,body.forward));
        }
        readonly List<Vector3> pedestrians=new List<Vector3>();
        public void StepTraffic(FunstraGame game,float dt)
        {
            pedestrians.Clear();pedestrians.Add(game.Player.position);pedestrians.Add(Jobs.Mara);
            foreach(var a in game.Agents)pedestrians.Add(a.Position);
            if(game.DistrictEnabled)
            { pedestrians.Add(game.District.neri.position);pedestrians.Add(game.District.guard.position);pedestrians.Add(game.District.collector.position); }
            AdvanceTraffic(dt,pedestrians);
        }
        public void AdvanceTraffic(float dt,List<Vector3> people)
        {
            for(int i=0;i<Cars.Count;i++)
            {
                var car=Cars[i];Vector3 p=car.body.position;
                if(Vector3.Distance(p,car.route[car.stop])<.02f)car.stop=(car.stop+1)%car.route.Length;
                Vector3 direction=(car.route[car.stop]-p).normalized;
                Vector3 next=Vector3.MoveTowards(p,car.route[car.stop],3.8f*dt);
                // Check the next footprint and a short braking corridor before translating OR turning.
                var future=car.BoundsAt(next,direction);var clearance=future;clearance.Expand(new Vector3(1.5f,4,1.5f));
                var ahead=car.BoundsAt(next+direction*1.2f,direction);ahead.Expand(new Vector3(1.5f,4,1.5f));
                bool blocked=false;
                if(car.route.Length==2)
                {
                    if(car.crossing&&Mathf.Abs(p.x)>10&&p.x*direction.x>0)car.crossing=false;
                    if(!car.crossing&&p.x*direction.x<0&&Mathf.Abs(next.x)<10)
                    {
                        bool junctionFree=true;
                        foreach(var other in Cars)if(other!=car&&Mathf.Abs(other.body.position.x)<10&&Mathf.Abs(other.body.position.z-p.z)<11)junctionFree=false;
                        if(junctionFree)car.crossing=true;else blocked=true;
                    }
                }
                else foreach(var other in Cars)
                    if(other.crossing&&Mathf.Abs(next.z-other.body.position.z)<10)blocked=true;
                foreach(var person in people)
                    if(clearance.Contains(new Vector3(person.x,.8f,person.z))||ahead.Contains(new Vector3(person.x,.8f,person.z))) {blocked=true;break;}
                for(int j=0;j<Cars.Count&&!blocked;j++)if(j!=i&&clearance.Intersects(Nav.Traffic[j]))blocked=true;
                car.yielding=blocked;
                if(!blocked) {car.body.position=next;car.body.rotation=Quaternion.LookRotation(direction);Nav.Traffic[i]=future;
                    foreach(Transform part in car.body)if(part.name=="Wheel")part.Rotate(Vector3.up,Vector3.Distance(p,next)*160,Space.Self); }
            }
            Physics.SyncTransforms();
        }
    }
}
