using System;
using System.Collections;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        IEnumerator PoliceSteps()
        {
            yield return new WaitForSeconds(1);
            StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;
            smokeFreezeAgents=true;freezeDistrictAI=true;yardSquad=null;
            Check(Smoke&&DistrictEnabled&&!FoundationMode,"Police route uses isolated normal Old Port campaign");
            Check(muteTests&&mute&&AudioListener.volume==0,"Test audio is forced muted before gameplay");
            Check(PoliceOfficerCount==3&&PoliceReinforcementCount==0,"Fresh campaign starts with three patrols and no reinforcements");
            smokeResults.Add("METHOD: controlled reports and explicit simulation steps test dispatch, knowledge and persistence. Final firefight uses actual Update/AI/projectiles. No human free-play claim.");
            Vector3 report=new Vector3(8,0,-13);
            ReportPoliceNoise(report);
            Check(!Police.identifiedGunman&&Heat==0&&Police.Searching,"Gunshot sound prompts area investigation without identifying a hidden shooter");
            Teleport(new Vector3(-45,0,-45));
            for(int i=0;i<3;i++)Agents[i].Step(this,1f/30);
            Check(Police.lastKnown==report,"Noise search retains reported position rather than reading hidden player's new position");
            ResetPoliceResponse();
            ReportPoliceViolence(report,2,"fixture-witness");
            Check(Police.identifiedGunman&&Heat>0&&Police.trucks[0].requested&&!Police.trucks[1].requested,"Witnessed serious harm identifies attacker and requests first truck only");
            for(int i=0;i<3;i++)Agents[i].Step(this,1f/30);
            Check(Agents[0].Pursuing&&Agents[1].Pursuing&&Agents[2].Pursuing,"Every living patrol responds to shared gunman report");
            Vector3 known=Police.lastKnown;float remaining=Police.searchRemaining;float delay=Police.trucks[0].delay;
            yield return new WaitForSeconds(.4f);
            Check(Police.searchRemaining==remaining&&Police.trucks[0].delay==delay,"Tactical pause freezes search and dispatch clocks in actual Update");
            Check(Police.lastKnown==known,"Unseen relocated player does not update shared gunman coordinates");
            ReportPoliceViolence(report,2,"fixture-witness");
            Check(Police.trucks[1].requested,"Continued harm requests the second bounded truck");
            Save();StartRun(true);screen=ScreenMode.Pause;yardSquad=null;
            Check(Police.harm==4&&Police.identifiedGunman&&Police.trucks[0].requested&&Police.trucks[1].requested,"Save/load retains severity and both dispatch requests");
            Check(PoliceReinforcementCount==0,"Loading a requested truck does not teleport its crew into town");
            // Run production road/dispatch/agent functions at 30Hz. Combat is deliberately omitted
            // in this logistics fixture so the player survives to inspect both arrivals.
            Teleport(new Vector3(-45,0,-45));
            bool moved=false,entered=false;Vector3 first=Police.trucks[0].position;
            for(int frame=0;frame<3600&&PoliceReinforcementCount<6;frame++)
            {
                if(frame%300==0)ReportPoliceViolence(report,0,"fixture-witness");
                City.RefreshProps();City.StepTraffic(this,1f/30);StepPoliceResponse(1f/30);
                foreach(var a in Agents)a.Step(this,1f/30);
                entered|=PoliceTruckCount>0;moved|=Vector3.Distance(first,Police.trucks[0].position)>2;
                if(frame==240){Teleport(new Vector3(-7,0,-39));cameraSize=22;SnapCamera();screen=ScreenMode.Tactics;yield return Capture("P01-trucks-arriving");screen=ScreenMode.Pause;Teleport(new Vector3(-45,0,-45));}
                if(frame%30==0)yield return null;
            }
            Check(entered&&moved,"Response trucks enter visibly and move along the road before deployment");
            Check(PoliceReinforcementCount==6&&PoliceOfficerCount==9,"Two physical arrivals deliver six finite officers for nine-police maximum");
            Check(Police.trucks[0].arrived&&Police.trucks[1].arrived&&Police.trucks[0].deployed==3&&Police.trucks[1].deployed==3,"Both truck records account for three deployed people");
            foreach(var a in Agents)if(a.Reinforcement)Check(City.Nav.Walkable(a.Position),"Deployed "+a.Record.id+" occupies walkable ground");
            var casualty=Police.officers[0];casualty.health=0;casualty.ammo=7;casualty.combat.Initialize(7);casualty.combat.magazine=2;
            Save();StartRun(true);screen=ScreenMode.Pause;yardSquad=null;
            Check(PoliceReinforcementCount==6&&PoliceOfficerCount==8&&PoliceTruckCount==2,"Reload reconstructs exact crew/trucks and preserves casualty without duplicate spawn");
            Check(Police.officers[0].health==0&&Police.officers[0].ammo==7&&Police.officers[0].combat.magazine==2,"Reinforcement wounds and finite magazine survive reload");
            ReportPoliceViolence(report,30,"fixture-witness");
            for(int i=0;i<90;i++)StepPoliceResponse(1f/30);
            Check(PoliceReinforcementCount==6,"Further violence cannot farm additional trucks or personnel beyond slice cap");
            Teleport(new Vector3(-7,0,-13));cameraSize=22;SnapCamera();screen=ScreenMode.Tactics;
            yield return Capture("P02-response-deployed");
            screen=ScreenMode.Pause;
            for(int i=0;i<1500;i++)StepPoliceResponse(1f/30);
            Check(!Police.Searching&&!Police.identifiedGunman,"A full period without fresh observations expires armed search");
            ResetPoliceResponse();Heat=0;
            ReportPoliceViolence(report,2,"fixture-witness");
            Police.Step(46);Heat=0;ReportPoliceNoise(report);
            for(int i=0;i<180;i++)StepPoliceResponse(1f/30);
            Check(PoliceTruckCount==0&&!Police.trucks[0].entered,"Unidentified noise cannot activate a pending truck from an expired violent incident");
            // Fresh fixture: real player shot and real patrol return fire in the exported Update loop.
            StartRun(false);District.introSeen=true;screen=ScreenMode.Pause;yardSquad=null;
            Teleport(new Vector3(16,0,-13));
            var officer=Agents[0];officer.Body.position=new Vector3(27,0,-13);officer.Record.position=officer.Position;officer.Body.LookAt(Player.position);
            var resident=Agents[3];resident.Body.position=new Vector3(20,0,-13);resident.Record.position=resident.Position;resident.Body.LookAt(Player.position);
            InitializeCombat();SelectCombatWeapon(2);
            float civilianHP=resident.Record.health;
            Check(FirePlayerAt(resident.Position)&&resident.Record.health==civilianHP,"Actual player trigger creates projectile before civilian damage");
            for(int i=0;i<8;i++)StepCombat(1f/30);
            Check(resident.Record.health<civilianHP&&Police.identifiedGunman,"Actual projectile wounds an attackable resident and witnesses identify gunman");
            resident.Body.position=new Vector3(20,0,-18);resident.Record.position=resident.Position;
            int initialAmmo=officer.Record.ammo;float initialHP=District.health;
            smokeFreezeAgents=false;screen=ScreenMode.Play;float deadline=Time.realtimeSinceStartup+8;
            while(Time.realtimeSinceStartup<deadline&&District.health>=initialHP)yield return null;
            Check(officer.Record.ammo<initialAmmo&&District.health<initialHP,"Live patrol Update spends ammunition and real return-fire projectiles wound exposed player");
            screen=ScreenMode.Tactics;yield return Capture("P03-live-return-fire");
            int afterAmmo=officer.Record.ammo;float afterHP=District.health;
            yield return new WaitForSeconds(.4f);
            Check(officer.Record.ammo==afterAmmo&&District.health==afterHP,"Pause freezes live return fire and projectile damage");
            officer.Record.ammo=officer.Record.combat.magazine=0;officer.Path.Clear();officer.Repath=0;
            Vector3 emptyStart=officer.Position;
            for(int i=0;i<60;i++)officer.Step(this,1f/30);
            Check(Vector3.Distance(emptyStart,officer.Position)>1,"Officer with exhausted ammunition resumes movement instead of holding an empty firing position");
            District.health=0;DistrictDefeat();
            Check(!Police.identifiedGunman&&PoliceReinforcementCount==0&&Heat==0&&District.health>0,"Defeat clears immediate response and gives playable recovery");
            bool patrolSynced=true;foreach(var a in Agents)if(a.Police)patrolSynced&=Vector3.Distance(a.Record.position,a.Position)<.001f;
            Check(patrolSynced,"Recovery saves base patrol positions where their bodies were reset");
            Check(District.incidents.Count>0,"Recovery retains recorded incident consequences");
            yield return Capture("P04-recovery");
            Check(District.Valid(),"Campaign state remains valid after response, reload and recovery");
        }
    }
}
