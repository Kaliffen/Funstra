using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        // Mixed evidence: first actual Update frames; then explicitly accelerated fixed-step
        // traffic + district transactions while the gameplay loop is paused. No production save.
        IEnumerator TrafficFoundationSteps()
        {
            Check(Smoke,"Traffic regression runs only in an isolated smoke save");
            Check(DistrictEnabled,"Traffic regression requires enabled campaign district simulation");
            Check(City.Cars.Count==4,"Traffic regression uses the four actual Old Port cars");
            State=new RunState();District.introSeen=true;Heat=0;
            ResetDistrictRuntime();smokeFreezeAgents=true;freezeDistrictAI=true;autoMove=null;autoInteract=false;
            screen=ScreenMode.Pause;
            var starts=new[]{new Vector3(3,0,-29),new Vector3(-3,0,24),new Vector3(18,0,-40),new Vector3(-18,0,12)};
            for(int i=0;i<City.Cars.Count;i++)
            {
                var setup=City.Cars[i];setup.body.position=starts[i];setup.stop=i==1?3:1;setup.crossing=false;setup.yielding=false;
                setup.body.rotation=Quaternion.LookRotation(setup.route[setup.stop]-setup.body.position);
                City.Nav.Traffic[i]=setup.BoundsAt(setup.body.position,setup.body.forward);
            }
            Physics.SyncTransforms();var car=City.Cars[0];
            Vector3 occupied=new Vector3(3,0,-23);Teleport(occupied);
            Vector3 initial=car.body.position;float startClock=District.clock;
            screen=ScreenMode.Play;
            yield return new WaitForSeconds(2);
            Check(Vector3.Distance(initial,car.body.position)>.5f&&car.yielding,"Actual Update traffic approaches then yields to player occupying its lane");
            Check(District.clock>startClock+1,"World clock progresses while actual Update traffic waits");
            AssertTrafficSeparation(occupied,"actual Update braking");
            screen=ScreenMode.Pause;Vector3 waiting=car.body.position;float pausedClock=District.clock;
            yield return new WaitForSeconds(.3f);
            Check(car.body.position==waiting&&District.clock==pausedClock,"Pause freezes both waiting traffic and district time");
            var people=new List<Vector3>{Player.position};int treatments=District.treatments;
            int medicine=District.TotalMedicine;float acceleratedStart=District.clock;
            const int frames=5400;const float dt=1f/30;
            for(int frame=0;frame<frames;frame++)
            {
                City.AdvanceTraffic(dt,people);District.Tick(dt);
                if(Vector3.Distance(waiting,car.body.position)>.001f||!car.yielding)
                    throw new Exception("Waiting car moved through sustained occupied lane at accelerated frame "+frame);
                AssertTrafficSeparation(occupied,"sustained obstruction frame "+frame);
                if(frame%120==0)yield return null;
            }
            Check(Mathf.Abs((District.clock-acceleratedStart)-frames*dt)<.08f,"Three accelerated minutes preserve district elapsed time during traffic blockage");
            Check(District.treatments>treatments&&District.TotalMedicine==medicine&&District.Valid(),"Clinic transactions continue and medicine stays conserved while a road is blocked");
            Check(car.collider.enabled&&!City.Nav.Walkable(waiting),"Sustained waiting car keeps matching physical and navigation obstruction");
            Check(Vector3.Distance(car.collider.bounds.center,City.Nav.Traffic[0].center)<.01f,"Waiting physical collider and navigation footprint share position");
            if(captureScreens)yield return Capture("D-traffic-sustained-blockage");
            Teleport(new Vector3(8,0,-23));people.Clear();people.Add(Player.position);
            for(int frame=0;frame<90;frame++)
            { City.AdvanceTraffic(dt,people);District.Tick(dt);AssertTrafficSeparation(Player.position,"traffic recovery frame "+frame); }
            Check(Vector3.Distance(waiting,car.body.position)>3&&!car.yielding,"Car resumes normal travel after sustained blocker leaves without reset");
            Check(District.TotalMedicine==medicine&&District.Valid(),"Traffic recovery preserves valid district transactions and inventory");
            Check(Vector3.Distance(car.collider.bounds.center,City.Nav.Traffic[0].center)<.01f,"Resumed car collider follows updated navigation footprint");
            Save();var loaded=RunState.Load(savePath);
            Check(loaded!=null&&Mathf.Abs(loaded.district.clock-District.clock)<.01f&&loaded.district.TotalMedicine==medicine,"Isolated save retains elapsed district outcome after traffic obstruction");
            if(captureScreens)yield return Capture("D-traffic-blockage-cleared");
        }
        void AssertTrafficSeparation(Vector3 person,string phase)
        {
            for(int i=0;i<City.Cars.Count;i++)
            {
                var footprint=City.Nav.Traffic[i];footprint.Expand(new Vector3(.76f,2,.76f));
                if(footprint.Contains(new Vector3(person.x,.8f,person.z)))throw new Exception("Car overlaps pedestrian during "+phase);
                for(int j=i+1;j<City.Cars.Count;j++)
                    if(City.Nav.Traffic[i].Intersects(City.Nav.Traffic[j]))throw new Exception("Cars overlap during "+phase);
            }
        }
    }
}
