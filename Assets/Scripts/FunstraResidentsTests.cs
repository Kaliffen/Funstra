using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        TownAgent Resident(string id)=>Agents.Find(a=>a.Record!=null&&a.Record.id==id);
        void PlaceResident(TownAgent a,Vector3 p,Vector3 facing)
        {
            a.Body.position=a.Record.position=City.Nav.SafePoint(p);a.Body.rotation=Quaternion.LookRotation(facing-a.Position);
            a.Path.Clear();a.Repath=0;
        }
        IEnumerator StepResidentsFor(float seconds)
        {
            for(float t=0;t<seconds;t+=.05f)
            {
                District.clock+=.05f;
                foreach(var a in Agents)if(!a.Police)a.Step(this,.05f);
                if(Mathf.RoundToInt(t*20)%8==0)yield return null;
            }
        }
        void ResidentSnapshot(string label)
        {
            File.WriteAllText(Path.Combine(evidencePath,label+".json"),JsonUtility.ToJson(District,true));
        }
        IEnumerator ResidentsSteps()
        {
            yield return new WaitForSeconds(1);yield return Capture("R00-title");
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;yardSquad=null;freezeDistrictAI=true;
            Check(residentsTest&&ResidentsEnabled&&muteTests&&AudioListener.volume==0,"Isolated resident campaign and forced mute");
            int count=0;foreach(var a in Agents)if(!a.Police){count++;Check(a.Record.resident!=null&&a.Record.resident.Valid()&&a.Record.ammo==0&&!a.Record.weaponRecoverable,a.Record.name+" has a persistent persona and no invented weapon");}
            Check(count==8,"Eight existing residents gain named conduct; no added crowd");
            var helper=Resident("citizen-5");var victim=Resident("citizen-4");var selfish=Resident("citizen-8");var watcher=Resident("citizen-6");
            Vector3 initial=helper.Position;yield return new WaitForSeconds(7);
            Check(Vector3.Distance(initial,helper.Position)>.2f,"Resident leaves routine pause and moves in normal Update");
            smokeFreezeAgents=true;screen=ScreenMode.Tactics;Teleport(ArmsDealerPosition+Vector3.back*5);SnapCamera();
            yield return Capture("R01-market");
            Teleport(DistrictState.Clinic+Vector3.back*2);SnapCamera();yield return Capture("R02-clinic-refuge");
            Teleport(new Vector3(-29,0,-38));SnapCamera();
            PlaceResident(helper,new Vector3(-34,0,-36),Player.position);
            PlaceResident(victim,new Vector3(-35,0,-38),Player.position);
            PlaceResident(selfish,new Vector3(-32,0,-40),Player.position);
            PlaceResident(watcher,new Vector3(-34,0,-40),Player.position);
            selfish.Body.rotation=Quaternion.LookRotation((Player.position+victim.Position)*.5f-selfish.Position);
            watcher.Body.rotation=Quaternion.LookRotation((Player.position+victim.Position)*.5f-watcher.Position);
            Check(ResidentSees(helper,Player.position)&&ResidentSees(helper,victim.Position),"Helper can actually see the prepared attacker and neighbor");
            Check(ResidentSees(selfish,Player.position)&&ResidentSees(selfish,victim.Position),"Self-preserving neighbor sees the same incident");
            ResidentGunshot(Player.position,"unknown");
            Check(helper.Record.resident.dangerRemaining>0&&!helper.Record.resident.witnessedPlayer,"Hearing a shot causes shelter seeking without identity");
            victim.Record.health=62;ApplyCombatHit(victim.Record,victim.Body,20,"player",true);
            Check(helper.Record.resident.witnessedPlayer&&helper.Record.resident.casualtyId==victim.Record.id,"Actual shared combat hit leaves personally witnessed casualty memory");
            Check(selfish.Record.resident.abandonedNeighbor&&!selfish.Record.resident.WillHelp(victim.Record.id),"Self-preserving observer chooses safety over returning to help");
            Check(helper.Record.resident.WillHelp(victim.Record.id),"Careful helper intends to return for the same neighbor");
            float wounded=victim.Record.health;int bandages=helper.Record.bandages;
            Vector3 before=helper.Position;yield return StepResidentsFor(4);
            Check(Vector3.Distance(before,helper.Position)>.5f,"Threatened helper physically leaves the incident using shared navigation");
            Check(helper.Record.bandages==bandages&&victim.Record.health==wounded,"Helper does not spend a dressing during perceived danger");
            ResidentSnapshot("R03-danger-state");yield return Capture("R03-taking-shelter");
            Teleport(Jobs.Home);Heat=0;ResetPolice();
            yield return StepResidentsFor(65);
            Check(helper.Record.bandages==bandages-1&&helper.Record.resident.dressingsUsed==1,"Helper returns after quiet and spends exactly one personal dressing");
            Check(!victim.Record.bleeding&&victim.Record.health==wounded+12,"Aid changes the actual wounded neighbor's health and bleeding");
            Check(selfish.Record.resident.dressingsUsed==0,"Self-preserving observer did not perform the helper's action");
            Teleport(victim.Position+Vector3.back*4);SnapCamera();ResidentSnapshot("R04-after-aid-state");yield return Capture("R04-neighbor-aid");
            victim.Record.health=45;victim.Record.bleeding=true;helper.Record.resident.casualtyId=victim.Record.id;helper.Record.resident.casualtyPosition=victim.Position;
            yield return StepResidentsFor(15);
            Check(helper.Record.bandages==0&&helper.Record.resident.dressingsUsed==1&&victim.Record.health==45,"Repeated need cannot conjure a second personal dressing");
            ResidentRememberAid(victim.Record,"player");
            string memory=helper.Record.resident.memory;Save();StartRun(true);screen=ScreenMode.Tactics;smokeFreezeAgents=true;yardSquad=null;
            helper=Resident("citizen-5");victim=Resident("citizen-4");
            Check(helper.Record.resident.memory==memory&&helper.Record.resident.dressingsUsed==1&&helper.Record.bandages==0,"Memory and consumed dressing survive actual save/reload");
            Check(victim.Record.resident.helpedByPlayer,"Remembered player aid survives save/reload");
            PlaceResident(victim,new Vector3(-34,0,-37),new Vector3(-29,0,-38));Teleport(new Vector3(-29,0,-38));SnapCamera();
            yield return StepResidentsFor(2);
            Check(victim.Record.resident.action.Contains("thank")||victim.Record.resident.action.Contains("Acknowledging"),"Remembered help changes later approach behavior");
            yield return Capture("R05-recognition");
            PlaceResident(helper,new Vector3(-60,0,-36),Player.position);helper.Record.resident.witnessedPlayer=false;
            ResidentViolence(victim.Record,Player.position,"player");
            Check(!helper.Record.resident.witnessedPlayer,"Distant unobserved violence grants no player identity");
            Check(District.Valid(),"Resident state remains valid alongside medicine, weapons and police");
            Teleport(Jobs.Home);SnapCamera();yield return Capture("R06-home-threshold");
            ResidentSnapshot("resident-final-state");
            File.WriteAllText(Path.Combine(evidencePath,"method.txt"),"Muted exported player, 1280x720, "+SystemInfo.processorType+" / "+SystemInfo.graphicsDeviceName+". Seven seconds of normal Update routine movement; staged local assault using shared combat; accelerated .05s production resident steps for shelter and finite aid, calendar/squad/police held. Explicit aid-memory hook fixture separate from actual resource-spending NPC aid. No human input or minimum-spec claim.");
        }
    }
}
