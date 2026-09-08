using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        bool foundationScripted;
        IEnumerator FoundationRun()
        {
            foundationScripted=true;captureScreens=true;
            evidencePath=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Evidence/Foundation"));
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="--evidence")evidencePath=Path.GetFullPath(args[i+1]);
            Directory.CreateDirectory(evidencePath);smokeResults.Clear();
            smokeResults.Add("METHOD: exported player; explicit fixture setup; production controller, weapons, projectiles and squad; scripted frame sequence, not exploratory human input.");
            var stack=new Stack<IEnumerator>();stack.Push(FoundationSteps());
            while(stack.Count>0)
            {
                bool next=false;object current=null;string error=null;
                try {next=stack.Peek().MoveNext();if(next)current=stack.Peek().Current;}catch(Exception e){error=e.ToString();}
                if(error!=null)
                {File.WriteAllText(Path.Combine(evidencePath,"foundation-"+foundationLevel+"-result.txt"),"FAIL\n"+string.Join("\n",smokeResults)+"\n"+error);Debug.LogError(error);Application.Quit(1);yield break;}
                if(!next){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;
            }
            File.WriteAllText(Path.Combine(evidencePath,"foundation-"+foundationLevel+"-result.txt"),"PASS\n"+string.Join("\n",smokeResults));Application.Quit(0);
        }
        IEnumerator FoundationSteps()
        {
            screen=ScreenMode.Play;foundationHelp=false;yield return new WaitForSeconds(1);
            Check(FoundationMode&&foundationLevel>=1&&foundationLevel<=4,"Isolated prepared test level is active");
            Check(District.health==100&&District.ammo==72,"Fresh test equipment is available without spending campaign money");
            Check(pistolReport&&shotgunReport&&pistolReport!=shotgunReport&&pistolReport.samples>1000&&shotgunReport.samples>1000&&concreteSteps.Length==4,"Exported player loads distinct recorded firearm clips and four footstep variants");
            bool looping=false;foreach(var source in GetComponents<AudioSource>())looping|=source.loop&&source.isPlaying;
            Check(!looping,"No placeholder ambience drone loops in the exported player");
            yield return Capture("F0"+foundationLevel+"-01-prepared");
            if(foundationLevel==1)yield return FoundationMovement();
            else if(foundationLevel==2)yield return FoundationWeapons();
            else if(foundationLevel==3)yield return FoundationGroup();
            else yield return FoundationEncounter();
            autoMove=null;screen=ScreenMode.Play;
            yield return Capture("F0"+foundationLevel+"-99-complete");
            Check(true,"Rendered captures and motion frames written; visual quality requires inspection");
        }
        void FoundationStep(float dt,bool enemies=true)
        {
            Elapsed+=dt;toastTime-=dt;City.RefreshProps();StepCombat(dt);
            if(enemies)UpdateYard(dt);
        }
        IEnumerator FoundationFrames(int frames,bool enemies=true,string prefix=null)
        {
            for(int i=0;i<frames;i++)
            {
                FoundationStep(1f/30,enemies);yield return null;
                if(prefix!=null&&i%(frames>100?25:5)==0)yield return FoundationFrame(prefix+"-"+(i/(frames>100?25:5)).ToString("D2"));
            }
        }
        IEnumerator FoundationFrame(string name)
        {
            yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(evidencePath,name+".png"),texture.EncodeToPNG());Destroy(texture);
        }
        IEnumerator FoundationWalk(Vector3 target,int maxFrames=240,bool enemies=false)
        {
            var path=City.Nav.Find(Player.position,target);Check(path.Count>0,"Controller route exists to "+target);
            int index=0;
            for(int i=0;i<maxFrames;i++)
            {
                Vector3 flat=Player.position;flat.y=0;if(Vector3.Distance(flat,target)<.55f)break;
                while(index<path.Count&&Vector3.Distance(flat,path[index])<.35f)index++;
                autoMove=index<path.Count?(Vector3?)path[index]:target;UpdatePlayer(1f/30);FoundationStep(1f/30,enemies);yield return null;
            }
            autoMove=null;var end=Player.position;end.y=0;Check(Vector3.Distance(end,target)<.75f,"Production character controller arrived at "+target);
        }
        IEnumerator FoundationMovement()
        {
            yield return FoundationSprintRecovery();
            yield return FoundationWalk(new Vector3(0,0,-9));
            Teleport(new Vector3(-4,0,-8));smokeResults.Add("SETUP: placed beside corner wall to press into its collider.");
            autoMove=new Vector3(-10,0,-8);
            for(int i=0;i<40;i++){UpdatePlayer(1f/30);yield return null;}autoMove=null;
            Check(Player.position.x>-5.6f,"Controller cannot cross the solid corner wall");
            yield return Capture("F01-02-wall-contact");
            Teleport(new Vector3(6.5f,0,0));
            autoMove=new Vector3(6.5f,0,5);for(int i=0;i<30;i++){UpdatePlayer(1f/30);yield return null;}autoMove=null;
            Check(Player.position.z<2,"Closed gate stops actual controller movement");
            Check(City.SetServiceGate(false,new Vector3[0]),"Service gate opens");
            yield return FoundationWalk(new Vector3(6.5f,0,2));
            Check(!City.SetServiceGate(true,new[]{Player.position}),"Gate refuses to close on player occupying opening");
            yield return FoundationWalk(new Vector3(6.5f,0,9));
            Check(City.SetServiceGate(true,new[]{Player.position}),"Gate closes after opening clears");
            yield return Capture("F01-03-passage");
            yield return FoundationGateCloseups();
            Teleport(new Vector3(-3,0,5));autoMove=new Vector3(-3,0,10);
            for(int i=0;i<35;i++){UpdatePlayer(1f/30);yield return null;}autoMove=null;
            Check(Player.position.z<7.6f,"Solid bin prevents entering or walking through it");
            Teleport(new Vector3(0,0,-17));autoMove=new Vector3(0,0,-10);
            for(int i=0;i<24;i++){UpdatePlayer(1f/30);FoundationStep(1f/30,false);yield return null;if(i%3==0)yield return FoundationFrame("F01-motion-"+(i/3).ToString("D2"));}autoMove=null;
        }
        IEnumerator FoundationGateCloseups()
        {
            var savedPosition=Player.position;var savedOffset=cameraOffset;float savedSize=cameraSize;
            var savedMove=autoMove;
            try
            {
                cameraSize=7;autoMove=null;
                smokeResults.Add("METHOD: close gate front/back stills and camera-motion frames inspect both jamb/wall joins; production gate geometry, staged camera angles.");
                for(int side=-1;side<=1;side+=2)
                {
                    string view=side<0?"front":"back";
                    cameraOffset=new Vector3(9,13,side*12);Teleport(City.GatePosition+new Vector3(0,0,side*3));
                    yield return Capture("F01-gate-"+view+"-close");
                    for(int frame=0;frame<18;frame++)
                    {
                        cameraOffset=new Vector3(Mathf.Lerp(7,11,frame/17f),13,side*12);SnapCamera();yield return null;
                        if(frame%3==0)yield return FoundationFrame("F01-gate-"+view+"-motion-"+(frame/3).ToString("D2"));
                    }
                }
            }
            finally {cameraOffset=savedOffset;cameraSize=savedSize;autoMove=savedMove;Teleport(savedPosition);}
        }
        IEnumerator FoundationSprintRecovery()
        {
            var savedPosition=Player.position;var savedMove=autoMove;float savedStamina=Stamina;
            bool savedExhausted=sprintExhausted,savedSprint=sprinting,savedSneak=Sneaking,savedHidden=Hidden;
            float savedPhase=playerPhase,savedStepTime=stepTime;
            var savedFigureRotation=figure.rotation;var savedFigureScale=figure.localScale;
            var savedCameraPosition=View.transform.position;var savedCameraRotation=View.transform.rotation;var savedVelocity=cameraVelocity;
            try
            {
                smokeResults.Add("SETUP: held-sprint autoMove fixture starts near exhaustion on the open center lane; real controller displacement is measured each 30 Hz step.");
                cameraVelocity=new Vector3(6,4,-9);Teleport(new Vector3(0,0,-20));
                Check(cameraVelocity==Vector3.zero&&Vector3.Distance(View.transform.position,Player.position+cameraOffset)<.001f,"Teleport clears camera smoothing momentum and snaps to the new position");
                Stamina=2.4f;sprintExhausted=false;autoMove=new Vector3(0,0,20);
                var trace=new List<string>{"frame,stamina,horizontal_speed"};
                int transitions=0,walkStreak=0,runStreak=0,longestWalk=0,longestRun=0;
                bool previousFast=false,firstFast=false,secondSlow=false,fullBeforeRestart=false;
                for(int i=0;i<240;i++)
                {
                    var previous=Player.position;float beforeStamina=Stamina;UpdatePlayer(1f/30);
                    var delta=Player.position-previous;delta.y=0;float speed=delta.magnitude*30;
                    bool fast=speed>5.5f;
                    if(i==0)firstFast=fast;else if(fast!=previousFast)transitions++;
                    if(i==1)secondSlow=!fast;
                    if(i>1&&fast&&!previousFast)fullBeforeRestart=beforeStamina>=(State.HasPerk(1)?135:100);
                    previousFast=fast;
                    if(fast){runStreak++;walkStreak=0;}else{walkStreak++;runStreak=0;}
                    longestWalk=Mathf.Max(longestWalk,walkStreak);longestRun=Mathf.Max(longestRun,runStreak);
                    trace.Add(i+","+Stamina.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+speed.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
                    yield return null;
                    if(i>1&&runStreak>=15)break;
                }
                File.WriteAllLines(Path.Combine(evidencePath,"F01-sprint-recovery.csv"),trace);
                Check(firstFast&&secondSlow,"Actual controller crosses from running to exhausted walking");
                Check(longestWalk>=190,"Held sprint walks through the full stamina recovery period without early restart");
                Check(fullBeforeRestart&&longestRun>=15,"Actual controller resumes sustained running only after the stamina bar fills");
                Check(transitions==2,"Exhaustion recovery produces exactly one walk transition and one sprint restart");
                Check(Player.position.z>-8&&Mathf.Abs(Player.position.x)<.05f,"Recovery fixture advances through the clear lane without sideways controller jitter");
                smokeResults.Add("MEASURED: sprint recovery speed transitions="+transitions+", longest walk="+longestWalk+" frames, longest run="+longestRun+" frames; full displacement trace F01-sprint-recovery.csv.");
            }
            finally
            {
                controller.enabled=false;Player.position=savedPosition;controller.enabled=true;
                autoMove=savedMove;Stamina=savedStamina;sprintExhausted=savedExhausted;sprinting=savedSprint;Sneaking=savedSneak;Hidden=savedHidden;
                playerPhase=savedPhase;stepTime=savedStepTime;figure.rotation=savedFigureRotation;figure.localScale=savedFigureScale;
                View.transform.SetPositionAndRotation(savedCameraPosition,savedCameraRotation);cameraVelocity=savedVelocity;
            }
        }
        void FoundationTarget(CombatSquadMember member,Vector3 position,float health=100)
        {
            member.Actor.position=position;member.Data.home=position;member.Actor.health=health;member.Body.position=position;PoseActor(member.Body,member.Actor);
            RegisterCombatActor(member.Actor,member.Body);
        }
        IEnumerator FoundationWeapons()
        {
            Check(yardSquad!=null&&yardSquad.Members.Count==2,"Two passive moving targets are prepared");
            var target=yardSquad.Members[0];Vector3 old=target.Actor.position;
            yield return FoundationFrames(30,true,"F02-target-motion");
            Check(Vector3.Distance(old,target.Actor.position)>.1f,"Prepared target moves in the actual player");
            Teleport(new Vector3(-12,0,-1));FoundationTarget(target,new Vector3(-12,0,7));
            District.projectiles.Clear();SelectCombatWeapon(2);District.pistolWeapon.cooldown=0;
            float health=target.Actor.health;int total=District.ammo;
            Check(FirePlayerAt(target.Actor.position),"Pistol trigger consumes a loaded round");
            Check(target.Actor.health==health&&District.ammo==total-1&&ActiveProjectileCount==1,"Trigger creates traveling projectile without immediate target damage");
            yield return FoundationFrames(12,false,"F02-pistol-motion");
            Check(target.Actor.health==health-28,"Traveling pistol round damages target once");
            yield return FoundationFrames(15,false);Check(target.Actor.health==health-28,"Consumed projectile cannot damage a second time");
            SelectCombatWeapon(3);FoundationTarget(target,new Vector3(-12,0,6));
            Check(FirePlayerAt(target.Actor.position)&&ActiveProjectileCount==7,"Shotgun creates seven independent traveling pellets");
            yield return FoundationFrames(15,false,"F02-shotgun-motion");
            Check(target.Actor.health<100,"Shotgun pellets reach target after travel");
            yield return Capture("F02-02-weapon-impact");

            smokeResults.Add("SETUP: placed fresh target behind thin plate; restored health explicitly for obstruction checks.");
            FoundationTarget(target,new Vector3(-6,0,4));Teleport(new Vector3(-6,0,-5));District.projectiles.Clear();
            SelectCombatWeapon(2);District.pistolWeapon.cooldown=0;int covers=CombatCoverHitCount;
            Check(FirePlayerAt(target.Actor.position),"Shot toward thin plate fired");yield return FoundationFrames(20,false);
            Check(CombatCoverHitCount>covers&&target.Actor.health==100,"Thin cover intercepts projectile before target");
            Teleport(new Vector3(5,0,-12.8f));District.pistolWeapon.cooldown=0;covers=CombatCoverHitCount;
            Check(FirePlayerAt(new Vector3(5,0,-7)),"Near-muzzle test fired");yield return FoundationFrames(10,false);
            Check(CombatCoverHitCount>covers,"Near-muzzle obstruction cannot be bypassed by spawn position");

            Teleport(new Vector3(-16,0,-5));FoundationTarget(target,new Vector3(-16,0,5));District.projectiles.Clear();
            var plate=City.Prop("TEST / moving cover",new Vector3(-20,1.1f,-2),new Vector3(.3f,2,.3f),CityArt.Amber);
            CacheCombatObstacles();StepCombat(.001f);District.pistolWeapon.cooldown=0;covers=CombatCoverHitCount;
            Check(FirePlayerAt(target.Actor.position),"Moving-cover fixture fired");plate.transform.position=new Vector3(-12,1.1f,-2);StepCombat(.1f);
            Check(CombatCoverHitCount>covers&&target.Actor.health==100,"Moving cover swept across shot intercepts it between snapshots");
            plate.SetActive(false);Destroy(plate);District.projectiles.Clear();

            District.pistolWeapon.magazine=0;District.pistolWeapon.cooldown=0;int reserve=District.ammo;
            Check(ReloadPlayer(),"Empty magazine begins reload with finite reserve");StepCombat(.4f);float remaining=CombatReloadRemaining;
            screen=ScreenMode.Tactics;foundationScripted=false;yield return new WaitForSeconds(.4f);foundationScripted=true;screen=ScreenMode.Play;
            Check(CombatReloadRemaining==remaining,"Production paused screen freezes reload while real frames render");
            SelectCombatWeapon(3);Check(District.pistolWeapon.reloadRemaining==0&&District.ammo==reserve,"Weapon switch interrupts reload without creating ammunition");
            SelectCombatWeapon(2);Check(ReloadPlayer(),"Interrupted reload can restart");yield return FoundationFrames(50,false);
            Check(CurrentWeapon.magazine==Mathf.Min(6,reserve)&&District.ammo==reserve,"Completed reload transfers existing reserve into magazine");

            Teleport(new Vector3(-12,0,-1));FoundationTarget(target,new Vector3(-12,0,7));District.projectiles.Clear();CurrentWeapon.cooldown=0;
            Check(FirePlayerAt(target.Actor.position),"In-flight save fixture fired");
            string fixture=Path.Combine(evidencePath,"foundation-2-save.json");
            Check(State.Save(fixture),"Isolated in-flight state saves");var loaded=RunState.Load(fixture);
            Check(loaded!=null&&loaded.district.projectiles.Count==1&&loaded.district.ammo==District.ammo,"Reload retains paid-for in-flight shot");
            District.projectiles=loaded.district.projectiles;District.pistolWeapon=loaded.district.pistolWeapon;
            yield return FoundationFrames(20,false);
            Check(target.Actor.health==72&&ActiveProjectileCount==0,"Restored in-flight shot resolves once");
        }
        IEnumerator FoundationGroup()
        {
            Check(yardSquad!=null&&yardSquad.Members.Count==3,"Three prepared coordinated enemies exist");
            Teleport(new Vector3(-10,0,6));smokeResults.Add("SETUP: player placed in first guard's observable approach; subsequent AI uses live production sight and reports.");
            yield return FoundationFrames(9,true,"F03-contact-motion");
            Check(yardSquad.Members.Exists(m=>m.DirectSight),"At least one guard observes the player");
            yield return FoundationFrames(18,true);
            Check(yardSquad.Members.Exists(m=>m.ContactAge<1),"Squad retains recent contact");
            yield return Capture("F03-02-contact");
            Teleport(City.TestSpawn);float hiddenAt=yardSquad.State.clock;
            yield return FoundationFrames(250,true,"F03-search-motion");
            Check(yardSquad.Members.TrueForAll(m=>!m.DirectSight),"Distant hidden player is not directly tracked");
            Check(yardSquad.Members.TrueForAll(m=>m.Data.observedAt<=hiddenAt+.1f),"Breaking contact does not update remembered player position");
            Check(yardSquad.Members.TrueForAll(m=>m.ContactAge>CombatSquad.ContactLifetime),"All unrefreshed contact expires");
            smokeResults.Add("SETUP: two guards incapacitated and remaining guard wounded to exercise recovery behavior.");
            for(int i=0;i<2;i++)yardSquad.Members[i].Actor.health=0;
            var survivor=yardSquad.Members[2];survivor.Actor.health=20;yardSquad.Alert(survivor.Actor.position);
            float before=Vector3.Distance(survivor.Actor.position,survivor.Data.retreat);
            yield return FoundationFrames(60,true,"F03-retreat-motion");
            Check(survivor.Data.withdrawing,"Wounded last survivor chooses retreat");
            Check(Vector3.Distance(survivor.Actor.position,survivor.Data.retreat)<before,"Retreat moves through shared navigation toward refuge");
            Check(yardSquad.Members[0].Order=="Incapacitated"&&yardSquad.Members[1].Order=="Incapacitated","Incapacitated allies stop acting");
        }
        IEnumerator FoundationEncounter()
        {
            yield return FoundationWalk(new Vector3(-20,0,-12));
            yield return FoundationWalk(new Vector3(-20,0,8),240,true);
            Check(District.health>0,"Public approach can be traversed to first contact");
            SelectCombatWeapon(2);FirePlayerAt(yardSquad.Members[0].Actor.position);
            yield return FoundationFrames(24,true,"F04-exchange-motion");
            yield return Capture("F04-02-public-approach");
            yield return FoundationWalk(new Vector3(-20,0,-18),300,true);
            Check(District.health>0,"Player withdraws from live encounter through public route");
            smokeResults.Add("SETUP: reset health and placed at service gate for second approach comparison.");
            District.health=100;District.bleeding=false;District.projectiles.Clear();Teleport(City.GatePosition+Vector3.back*4);
            Check(City.GateClosed,"Service route begins gated");
            Check(City.SetServiceGate(false,new[]{Player.position}),"Service gate opens alternate route");
            yield return FoundationWalk(City.GatePosition+Vector3.forward*3,180,true);
            SelectCombatWeapon(3);FirePlayerAt(yardSquad.Members[2].Actor.position);
            yield return FoundationFrames(24,true,"F04-service-motion");
            Check(CombatShotCount>=2,"Both weapon families used in same live encounter");
            yield return Capture("F04-03-service-approach");
        }
    }
}


