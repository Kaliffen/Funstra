using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        IEnumerator RefugeSteps()
        {
            State=new RunState();District.introSeen=true;ResetDistrictRuntime();smokeFreezeAgents=true;freezeDistrictAI=true;screen=ScreenMode.Pause;Heat=0;
            foreach(var bin in City.Hides)
            {
                Check(!City.Nav.Walkable(bin),"Recycling bin blocks navigation at "+bin);
                Teleport(bin);Check(City.Nav.Walkable(Player.position),"Embedded saved/spawn position resolves outside bin");
            }
            screen=ScreenMode.Play;var obstacle=City.Hides[1];Teleport(obstacle+Vector3.back*3);autoMove=obstacle;
            yield return new WaitForSeconds(1.2f);autoMove=null;
            Check(Mathf.Abs(Player.position.z-obstacle.z)>.9f&&Player.position.y<.4f,"Real player controller cannot enter or step onto recycling bin");
            District.TakeShipment(false);District.Donate();District.Recruit();State.cash=180;
            Teleport(DistrictState.Clinic);Check(District.OpenRefuge(State),"Refuge opens from an earned partnership");SyncDistrictArt();
            Check(refugeSign.activeSelf&&refugeLamp.activeSelf,"Purchased refuge changes the rendered street");
            District.health=45;District.neri.health=60;
            Check(District.RefugeRest()&&District.health==80&&District.debt==0,"Refuge gives practical recovery without adding debt");
            District.SetClinicPolicy(true);District.tallyPetted=true;Save();StartRun(true);
            Check(District.refuge&&District.reserveMedicine&&District.tallyPetted&&State.cash==60,"Exported player reload retains room, care policy and cat relationship");
            screen=ScreenMode.Pause;
            var totals=new float[City.Cars.Count];var people=new List<Vector3>();
            for(int frame=0;frame<3600;frame++)
            {
                var previous=new Vector3[City.Cars.Count];for(int i=0;i<previous.Length;i++)previous[i]=City.Cars[i].body.position;
                City.AdvanceTraffic(1f/30,people);
                for(int i=0;i<previous.Length;i++)totals[i]+=Vector3.Distance(previous[i],City.Cars[i].body.position);
                for(int i=0;i<previous.Length;i++)for(int j=i+1;j<previous.Length;j++)
                    if(City.Nav.Traffic[i].Intersects(City.Nav.Traffic[j]))throw new System.Exception("Traffic intersected at frame "+frame);
            }
            for(int i=0;i<totals.Length;i++)Check(totals[i]>120,"Traffic car "+i+" completes sustained routes without intersection deadlock: "+totals[i]+"m at "+City.Cars[i].body.position);
            var car=City.Cars[0];var stop=car.body.position+car.body.forward*4.2f;people.Add(stop);
            for(int frame=0;frame<60;frame++)City.AdvanceTraffic(1f/30,people);
            var space=City.Nav.Traffic[0];space.Expand(new Vector3(.8f,3,.8f));
            Check(car.yielding&&!space.Contains(new Vector3(stop.x,.8f,stop.z)),"Car brakes before pedestrian and preserves physical clearance");
            Vector3 stopped=car.body.position;people.Clear();for(int frame=0;frame<30;frame++)City.AdvanceTraffic(1f/30,people);
            Check(Vector3.Distance(car.body.position,stopped)>1,"Car resumes after pedestrian clears lane");
            Check(!City.Nav.Walkable(car.body.position)&&car.collider.enabled,"Moving car updates both route obstacle and physical collider");
            Vector3 paused=car.body.position;yield return new WaitForSeconds(.3f);
            Check(car.body.position==paused,"Tactical/menu pause also freezes traffic");
            screen=ScreenMode.Play;District.neri.position=obstacle+Vector3.back*4;District.neri.order="Follow";District.neri.health=100;SyncDistrictArt();Teleport(obstacle+Vector3.forward*4);freezeDistrictAI=false;
            float deadline=Time.realtimeSinceStartup+12;
            while(Vector3.Distance(District.neri.position,Player.position)>3)
            {
                if(Time.realtimeSinceStartup>deadline)throw new System.Exception("Companion did not navigate around bin");
                if(!City.Nav.Walkable(District.neri.position))throw new System.Exception("Companion entered prop during route");
                yield return null;
            }
            Check(true,"Live companion routes around solid recycling bin without clipping");
            freezeDistrictAI=true;Teleport(DistrictState.Clinic);PetTally();Check(District.tallyPetted,"Cat interaction records familiarity");Save();
        }
        IEnumerator RefugeVisualSteps()
        {
            State=new RunState();District.introSeen=true;screen=ScreenMode.Play;smokeFreezeAgents=true;freezeDistrictAI=true;Heat=0;
            Teleport(DistrictState.Clinic);yield return new WaitForSeconds(.4f);yield return Capture("C01-clinic-and-tally");
            screen=ScreenMode.Clinic;yield return Capture("C02-neri-before");
            conversationIvo=false;screen=ScreenMode.Conversation;yield return Capture("C03-neri-story");
            State.cash=300;District.PayRelease(State);District.TakeShipment(false);District.Donate();District.Recruit();screen=ScreenMode.Clinic;
            yield return Capture("C04-paid-medicine-response");screen=ScreenMode.Refuge;yield return Capture("C05-refuge-offer");
            District.OpenRefuge(State);SyncDistrictArt();yield return Capture("C06-refuge-management");
            District.SetClinicPolicy(true);District.Tick(1000);yield return Capture("C07-policy-consequence");
            screen=ScreenMode.Play;yield return new WaitForSeconds(.3f);yield return Capture("C08-refuge-street");
            District.Identify("Rook saw the theft.");District.guard.health=0;screen=ScreenMode.Collector;yield return Capture("C09-ivo-remembers");
            screen=ScreenMode.Journal;yield return Capture("C10-timed-history");
            screen=ScreenMode.Play;Teleport(new Vector3(6,0,-6));yield return new WaitForSeconds(.4f);yield return Capture("C11-traffic-before");
            yield return new WaitForSeconds(3);yield return Capture("C12-traffic-after");
            screen=ScreenMode.Pause;Check(true,"Twelve Demo 03 review screens rendered");
        }
    }
}
