using System;
using System.Collections;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        bool environmentTest;
        IEnumerator EnvironmentSteps()
        {
            Check(Smoke&&DistrictEnabled&&!FoundationMode,"Environment walkthrough uses isolated normal campaign");
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;
            smokeFreezeAgents=false;freezeDistrictAI=true;yardSquad=null;
            cameraSize=16;yield return new WaitForSeconds(1);
            yield return Capture("E01-old-port-entry");
            yield return Travel(Jobs.Mara);
            yield return Travel(City.ClinicSouthDoor+Vector3.back*4);
            Check(!InsideClinic,"Clinic approached from public market court");
            yield return Capture("E02-clinic-street-entrance");
            yield return Travel(City.ClinicSouthDoor+Vector3.forward*2);
            yield return Travel(DistrictState.Clinic);
            Check(InsideClinic&&CanReachPerson(District.neri.position),"Walk through actual doorway reaches Neri inside treatment room");
            yield return Capture("E03-treatment-room");
            UpdateMedicalInteraction(0,true,false);
            Check(screen==ScreenMode.Clinic,"Neri's service opens inside the actual clinic");screen=ScreenMode.Play;
            yield return Travel(new Vector3(-34,0,-14));
            Check(InsideClinic,"Medicine store is connected to treatment room without teleport");
            yield return Capture("E04-clinic-store");
            yield return Travel(City.ClinicEastDoor+Vector3.right*3);
            Check(!InsideClinic,"Second clinic doorway leads onto back lane");
            yield return Capture("E05-receiving-lane");
            StartCoroutine(EnvironmentMotion());
            yield return Travel(new Vector3(-43,0,13));
            yield return Travel(new Vector3(-59,0,17));
            yield return Capture("E06-church-forecourt");
            yield return Travel(new Vector3(-45,0,38));
            yield return Travel(new Vector3(-28,0,39));
            yield return Capture("E07-market-and-quay");
            yield return Travel(new Vector3(8,0,39));
            yield return Travel(new Vector3(8,0,13));
            yield return Travel(new Vector3(50,0,14));
            yield return Capture("E08-workshop-court");
            yield return Travel(new Vector3(50,0,-40));
            yield return Travel(new Vector3(34,0,-46));
            yield return Capture("E09-residential-street");
            Check(City.Nav.Min.x<-60&&City.Nav.Max.y>60,"Expanded district is traversable beyond old map bounds");
            yield return Travel(Jobs.Home);Save();
            Check(District.Valid(),"Environment walkthrough preserves valid campaign state");
            screen=ScreenMode.Pause;
        }
        IEnumerator EnvironmentMotion()
        {
            // Consecutive rendered movement samples for ground/cutaway flicker inspection.
            // The normal route above continues to drive the production controller.
            for(int frame=0;frame<24;frame++)
            {
                yield return new WaitForSeconds(.08f);
                yield return FoundationFrame("E-motion-"+frame.ToString("D2"));
            }
        }
    }
}
