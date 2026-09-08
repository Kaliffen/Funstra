using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        bool streetsSampling;
        readonly List<float> streetsFrameMilliseconds=new List<float>();
        IEnumerator StreetsSteps()
        {
            Check(Smoke&&!FoundationMode&&DistrictEnabled,"Streets runner uses isolated save and normal Old Port simulation");
            yield return BandageSteps();
            yield return TrafficFoundationSteps();
            StartRun(false);District.introSeen=true;screen=ScreenMode.Play;Heat=0;
            smokeFreezeAgents=false;freezeDistrictAI=false;
            // First separate spatial quality from combat difficulty: production Update and
            // controller run normally, with only the yard squad suspended during the route.
            yardSquad=null;
            smokeResults.Add("METHOD: route uses actual Update/controller/traffic/citizens; yard squad suspended for traversal. Later weapon/persistence probes use paused explicit fixtures.");
            streetsFrameMilliseconds.Clear();streetsSampling=true;
            var sampler=StartCoroutine(StreetsSampleFrames());
            try
            {
                Teleport(DistrictState.Clinic);yield return Travel(Jobs.Mara);
                yield return Travel(new Vector3(-7,0,-13));yield return Travel(new Vector3(8,0,13));
                yield return Travel(new Vector3(8,0,39));yield return Travel(new Vector3(30,0,39));
                Check(City.GateClosed,"Public quay approach reaches yard while service gate remains closed");
                yield return Travel(new Vector3(8,0,39));yield return Travel(new Vector3(8,0,13));
                yield return Travel(new Vector3(30,0,23));
                Check(!City.Nav.ClearWalk(new Vector3(30,0,23),new Vector3(30,0,37)),"Closed normal-campaign gate prevents direct service passage");
                Check(City.SetServiceGate(false,new[]{Player.position}),"Normal-campaign service gate can be opened safely");District.yardGateClosed=false;
                yield return Travel(new Vector3(30,0,37));
                Check(Player.position.z>35,"Production controller traverses open service approach into actual yard");
                yield return new WaitForSeconds(5);
            }
            finally {streetsSampling=false;StopCoroutine(sampler);WriteStreetsProfile();}
            cameraSize=16;Notify("OLD PORT / Public quay and gated service approach");
            if(captureScreens)yield return Capture("D04-normal-quay-service-route");
            screen=ScreenMode.Pause;smokeFreezeAgents=true;freezeDistrictAI=true;
            InitializeYard();yield return null;
            Check(yardSquad!=null&&yardSquad.Members.Count==3,"Normal campaign binds its three persistent yard guards");
            var guard=yardSquad.Members[0];
            // Put player on the clear quay sightline. Targets are the actual persistent yard actors.
            Teleport(new Vector3(13,0,39));InitializeCombat();
            Check(City.Nav.Sight(Player.position,guard.Actor.position),"Quay fixture has actual clear sight to persistent yard guard");
            SelectCombatWeapon(2);int ammo=District.ammo;float hp=guard.Actor.health;
            Check(FirePlayerAt(guard.Actor.position)&&District.ammo==ammo-1&&guard.Actor.health==hp,"Campaign pistol spends ammo and creates a traveling projectile before damage");
            Check(ActiveProjectileCount>0,"Campaign in-flight bullet exists before save");
            int projectiles=ActiveProjectileCount;Vector3 bullet=District.projectiles[0].position;int magazine=District.pistolWeapon.magazine;
            guard.Actor.ammo=11;guard.Actor.combat.magazine=3;
            Save();StartRun(true);screen=ScreenMode.Pause;smokeFreezeAgents=true;freezeDistrictAI=true;
            guard=yardSquad.Members[0];
            Check(ActiveProjectileCount==projectiles&&Vector3.Distance(District.projectiles[0].position,bullet)<.001f,"Campaign reload preserves in-flight projectile position/count");
            Check(District.ammo==ammo-1&&District.pistolWeapon.magazine==magazine&&guard.Actor.ammo==11&&guard.Actor.combat.magazine==3,"Reload preserves player and yard guard magazines without free refills");
            Check(!City.GateClosed&&!District.yardGateClosed,"Reload restores open physical gate and persisted gate state");
            for(int i=0;i<24;i++){StepCombat(1f/30);yield return null;}
            Check(guard.Actor.health==hp-WeaponSpec.Pistol.damage&&ActiveProjectileCount==0,"Reloaded campaign bullet arrives once and spends itself");
            float wounded=guard.Actor.health;int cash=State.cash;
            Save();StartRun(true);screen=ScreenMode.Pause;
            guard=yardSquad.Members[0];
            for(int i=0;i<24;i++){StepCombat(1f/30);yield return null;}
            Check(guard.Actor.health==wounded&&State.cash==cash&&District.ammo==ammo-1,"Second reload does not repeat damage, pay a reward or farm ammunition");
            SelectCombatWeapon(3);int shells=District.shotgunAmmo;
            Check(FirePlayerAt(guard.Actor.position)&&ActiveProjectileCount==WeaponSpec.Shotgun.pellets,"Campaign shotgun launches separately simulated pellets");
            for(int i=0;i<25;i++){StepCombat(1f/30);yield return null;}
            Check(guard.Actor.health<wounded&&District.shotgunAmmo==shells-1,"Shotgun produces a distinct multi-pellet hit on same actual quay sightline");
            float finalHealth=guard.Actor.health;int finalGuardAmmo=guard.Actor.ammo;
            Check(City.SetServiceGate(true,new[]{Player.position}),"Clear campaign gate closes after route");District.yardGateClosed=true;
            Save();StartRun(true);screen=ScreenMode.Pause;guard=yardSquad.Members[0];
            Check(guard.Actor.health==finalHealth&&guard.Actor.ammo==finalGuardAmmo&&City.GateClosed,"Final reload preserves yard casualties/ammo and closed gate without respawning rewards");
            Check(District.Valid(),"Integrated yard campaign state remains valid after weapon and gate round trips");
            yield return YardLiveSteps();
        }
        IEnumerator YardSteps()
        {
            Check(Smoke&&!FoundationMode&&DistrictEnabled,"Focused yard runner uses isolated normal campaign");
            yield return YardLiveSteps();
        }
        IEnumerator YardLiveSteps()
        {
            // Keep the other members alive, so the watchman is not a last-survivor retreat fixture.
            // They have empty guns; the watchman retains normal aim error and fire cadence.
            StartRun(false);District.introSeen=true;smokeFreezeAgents=true;freezeDistrictAI=true;screen=ScreenMode.Play;
            foreach(var m in yardSquad.Members)if(m.Actor.id!="yard-0")m.Actor.ammo=m.Actor.combat.magazine=0;
            Teleport(new Vector3(13,0,39));foreach(var m in yardSquad.Members)m.Body.LookAt(Player.position);
            float liveHP=District.health;int initialShots=CombatShotCount;
            float started=Time.realtimeSinceStartup,until=started+8,nextDiagnostic=started;
            while(Time.realtimeSinceStartup<until&&District.health>=liveHP)
            {
                if(Time.realtimeSinceStartup>=nextDiagnostic)
                {
                    nextDiagnostic=Time.realtimeSinceStartup+.5f;
                    var watch=yardSquad.Members[0];
                    string diagnostic="YARD LIVE: elapsed="+(Time.realtimeSinceStartup-started).ToString("F2")+
                        " shots="+(CombatShotCount-initialShots)+" projectiles="+ActiveProjectileCount+
                        " playerHP="+District.health.ToString("F2")+" order="+watch.Order+
                        " directSight="+watch.DirectSight+" geometricSight="+City.Nav.Sight(watch.Actor.position,Player.position)+
                        " contactAge="+watch.ContactAge.ToString("F2")+" guardAmmo="+watch.Actor.ammo+
                        " magazine="+watch.Actor.combat.magazine+" player="+Player.position+" guard="+watch.Actor.position;
                    smokeResults.Add(diagnostic);Debug.Log(diagnostic);
                }
                yield return null;
            }
            // Several legitimate misses are allowed, but no firing/no damage is never a pass.
            Check(CombatShotCount>initialShots,"Live normal-yard watchman actually fires during bounded acquisition window");
            Check(District.health<liveHP&&District.health>0,"Live normal-yard enemy Update fires traveling shots that wound player under actual frame timing");
            Notify("OLD PORT / Live yard contact. Retreat remains available.");
            if(captureScreens)yield return Capture("D04-normal-yard-live-contact");
            yield return Travel(new Vector3(8,0,39));yield return Travel(new Vector3(8,0,13));
            Check(District.health>0,"Controller can withdraw from live yard onto the public route");
            Save();screen=ScreenMode.Pause;
        }
        IEnumerator StreetsSampleFrames()
        {
            while(streetsSampling){yield return null;if(streetsSampling)streetsFrameMilliseconds.Add(Time.unscaledDeltaTime*1000);}
        }
        void WriteStreetsProfile()
        {
            if(streetsFrameMilliseconds.Count==0)return;
            streetsFrameMilliseconds.Sort();int count=streetsFrameMilliseconds.Count;
            int processes=-1;
            try{processes=System.Diagnostics.Process.GetProcessesByName("Funstra").Length;}catch{}
            var report="METHOD: actual rendered Update route, clinic-market-public quay-service gate; yard AI suspended for traversal, normal citizens/traffic/district active. Unscaled frame deltas include scheduling and frame caps; these are not isolated CPU/GPU timings.\n"+
                "GPU: "+SystemInfo.graphicsDeviceName+"\nCPU: "+SystemInfo.processorType+"\nResolution: "+Screen.width+"x"+Screen.height+
                "\nFrame cap: "+Application.targetFrameRate+"; vSync: "+QualitySettings.vSyncCount+"\nSamples: "+count+
                "\np50 ms: "+streetsFrameMilliseconds[(count-1)/2].ToString("F3")+"\np95 ms: "+streetsFrameMilliseconds[Mathf.Clamp(Mathf.CeilToInt(count*.95f)-1,0,count-1)].ToString("F3")+
                "\nmax ms: "+streetsFrameMilliseconds[count-1].ToString("F3")+"\nUnity allocated memory bytes (endpoint): "+UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()+
                "\nActors: "+(Agents.Count+yardBodies.Count+4)+"; cars: "+City.Cars.Count+"\nFunstra processes observed: "+processes+
                "\nLIMIT: only this available machine. Other running game processes may compete for CPU/GPU; no minimum-spec or isolated benchmark claim. No Demo03 matched baseline comparison in this report.\n";
            File.WriteAllText(Path.Combine(evidencePath,"streets-frame-profile.txt"),report);
        }
    }
}
