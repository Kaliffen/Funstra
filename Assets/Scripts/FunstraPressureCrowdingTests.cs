using System.Collections;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        IEnumerator PressureCrowdingSteps()
        {
            StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;
            Teleport(new Vector3(-45,0,-45));City.RefreshProps();
            smokeResults.Add("METHOD: crowded doors and officer passing use controlled actor placement and explicit production steps on real city geometry; these are isolated movement fixtures, not the live pressure/escape route.");

            Vector3 parking=Vector3.zero;bool found=false;
            for(int z=-48;z<=48;z+=4)
            {
                var candidate=new Vector3(3,0,z);
                if(TruckSpaceClear(0,candidate)&&OfficerExitClear(candidate+new Vector3(2.1f,0,-1.4f))&&OfficerExitClear(candidate+new Vector3(-2.1f,0,-1.4f)))
                {parking=candidate;found=true;break;}
            }
            Check(found,"Crowded-door fixture finds clear real road and two walkable side exits");
            Police.Violence(parking,2,"controlled-door-fixture");
            var truck=Police.trucks[0];truck.entered=truck.arrived=true;truck.position=truck.destination=parking;truck.delay=0;
            CreatePoliceTruck(0);
            Vector3 primary=parking+new Vector3(2.1f,0,-1.4f),alternate=parking+new Vector3(-2.1f,0,-1.4f);
            var primaryBlocker=Agents[0];var alternateBlocker=Agents[1];Vector3 originalAlternate=alternateBlocker.Position;
            primaryBlocker.Body.position=primary;primaryBlocker.Record.position=primary;
            alternateBlocker.Body.position=alternate;alternateBlocker.Record.position=alternate;
            for(int frame=0;frame<30;frame++)StepPoliceResponse(1f/30);
            Check(truck.deployed==0&&PoliceReinforcementCount==0,"Both occupied side doors keep the crew aboard without overlap or remote spawn");
            alternateBlocker.Body.position=originalAlternate;alternateBlocker.Record.position=originalAlternate;
            StepPoliceResponse(1f/30);
            Check(truck.deployed==1&&PoliceReinforcementCount==1,"Clearing only the alternate side deploys exactly one waiting officer");
            Check(Vector3.Distance(Police.officers[0].position,alternate)<.01f&&City.Nav.Walkable(Police.officers[0].position),"Waiting officer emerges at the genuinely clear opposite door on walkable ground");
            Check(Vector3.Distance(primaryBlocker.Position,primary)<.01f&&Police.Valid(),"Deployment preserves the blocking person and valid finite roster");
            Teleport(parking+new Vector3(-7,0,-9));cameraSize=17;SnapCamera();screen=ScreenMode.Tactics;
            yield return Capture("P42-crowded-truck-alternate-exit");

            StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;
            Teleport(new Vector3(-45,0,-45));City.RefreshProps();
            Vector3 start=Vector3.zero,goal=Vector3.zero;found=false;
            for(int z=-40;z<=40&&!found;z+=4)for(int x=-40;x<=40&&!found;x+=4)
            {
                Vector3 candidate=new Vector3(x,0,z),end=candidate+Vector3.forward*8;
                bool clear=City.Nav.ClearWalk(candidate,end)&&OfficerExitClear(candidate)&&OfficerExitClear(candidate+Vector3.forward*.79f)&&OfficerExitClear(end);
                for(int side=-1;side<=1&&clear;side+=2)
                    clear=City.Nav.ClearWalk(candidate,candidate+Vector3.right*side*1.3f)&&City.Nav.ClearWalk(candidate+Vector3.right*side*1.3f,end);
                if(clear){start=candidate;goal=end;found=true;}
            }
            Check(found,"Passing fixture finds a clear real street with room beside a stationary colleague");
            Teleport(goal);
            var follower=Agents[0];var colleague=Agents[1];
            follower.Body.position=start;follower.Record.position=start;follower.Body.LookAt(Player.position);follower.Path.Clear();follower.Repath=0;
            follower.Record.ammo=0;follower.Record.combat=new WeaponState();follower.Record.combat.Initialize(0);
            colleague.Body.position=start+Vector3.forward*.79f;colleague.Record.position=colleague.Position;
            ReportPoliceViolence(Player.position,0,"controlled-passing-fixture");
            float minimumSeparation=Vector3.Distance(follower.Position,colleague.Position),sideways=0;bool stayedWalkable=true;
            for(int frame=0;frame<180&&follower.Position.z-start.z<3;frame++)
            {
                follower.Step(this,1f/30);
                minimumSeparation=Mathf.Min(minimumSeparation,Vector3.Distance(follower.Position,colleague.Position));
                sideways=Mathf.Max(sideways,Mathf.Abs(follower.Position.x-start.x));
                stayedWalkable&=City.Nav.Walkable(follower.Position);
            }
            Check(stayedWalkable,"Passing officer stays on shared walkable ground throughout movement");
            Check(follower.Position.z-start.z>=3&&sideways>.6f,"Blocked officer steps sideways and progresses beyond the stationary colleague");
            Check(minimumSeparation>=.779f&&Vector3.Distance(colleague.Position,start+Vector3.forward*.79f)<.01f,"Passing keeps person separation without pushing the stationary colleague");
            Check(follower.Record.ammo==0&&follower.Record.combat.magazine==0,"Empty-ammunition pursuit remains mobile without minting rounds");
            cameraSize=17;SnapCamera();screen=ScreenMode.Tactics;
            yield return Capture("P42-officer-passing");
            StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
        }
    }
}
