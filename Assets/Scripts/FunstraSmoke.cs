using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        bool smokeFreezeAgents;
        bool visualCheck, captureScreens;
        string evidencePath;
        IEnumerator SmokeRun()
        {
            evidencePath=Path.GetFullPath(Path.Combine(Application.dataPath,"../../Evidence"));
            string[] args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="--evidence")evidencePath=args[i+1];
            Directory.CreateDirectory(evidencePath);
            string resultName=bandageVisual?"bandage-visual-result.txt":bandageTest?"bandage-runtime-result.txt":visualCheck?"visual-result.txt":"runtime-result.txt";
            var stack=new Stack<IEnumerator>();stack.Push(bandageVisual?BandageVisualSteps():bandageTest?BandageSteps():visualCheck?VisualSteps():SmokeSteps());
            while(stack.Count>0)
            {
                bool next=false;object current=null;string error=null;
                try { next=stack.Peek().MoveNext();if(next)current=stack.Peek().Current; }
                catch(Exception e) { error=e.ToString(); }
                if(error!=null)
                {
                    File.WriteAllText(Path.Combine(evidencePath,resultName),"FAIL\n"+string.Join("\n",smokeResults)+"\n"+error);
                    Debug.LogError(error);Application.Quit(1);yield break;
                }
                if(!next) { stack.Pop();continue; }
                if(current is IEnumerator nested)stack.Push(nested);else yield return current;
            }
            File.WriteAllText(Path.Combine(evidencePath,resultName),"PASS\n"+string.Join("\n",smokeResults));
            Application.Quit(0);
        }
        void Check(bool value,string message)
        { if(!value)throw new Exception(message);smokeResults.Add("PASS: "+message);Debug.Log("CHECK: "+message); }
        IEnumerator Capture(string name)
        {
            if(!captureScreens)yield break;
            yield return new WaitForEndOfFrame();
            var capture=ScreenCapture.CaptureScreenshotAsTexture();
            float brightness=0;
            for(int y=0;y<capture.height;y+=32)for(int x=0;x<capture.width;x+=32)brightness+=capture.GetPixel(x,y).grayscale;
            if(brightness<1) { Destroy(capture);throw new Exception("Screenshot is black. Run visual checks in a visible window."); }
            File.WriteAllBytes(Path.Combine(evidencePath,name+".png"),capture.EncodeToPNG());Destroy(capture);
        }
        IEnumerator VisualSteps()
        {
            yield return new WaitForSeconds(2);yield return Capture("01-title");
            StartRun(false);smokeFreezeAgents=true;Teleport(Jobs.Mara);Talk();
            yield return new WaitForSeconds(.3f);yield return Capture("02-mara");
            State.Accept();screen=ScreenMode.Play;Teleport(Jobs.All[0].position);
            yield return new WaitForSeconds(.5f);showMap=true;yield return Capture("03-map");showMap=false;
            yield return null;yield return Capture("04-street");
            State.Steal();State.Deliver(0);screen=ScreenMode.Perk;
            yield return Capture("05-upgrade");
            State.ChoosePerk(2);State.completed=2;State.cash=340;State.perks=6;State.accepted=true;screen=ScreenMode.Play;Teleport(Jobs.All[2].position);
            State.Steal();RaiseAlarm("Depot alarm! Break sight, then hide.",Player.position);
            yield return new WaitForSeconds(.4f);yield return Capture("06-alarm");
            Heat=0;State.Deliver(0);screen=ScreenMode.Ending;
            yield return Capture("07-ending");screen=ScreenMode.Pause;yield return Capture("08-pause");
            ContinueFreeroam();Heat=0;ResetPolice();Teleport(Jobs.Home);showMap=true;
            yield return Capture("09-cargo-map");showMap=false;
            Cargo.Take(2,State);RefreshCargoArt();Teleport(CargoRun.Sites[2].position);RaiseAlarm("Hot cargo! Break sight before heading home.",Player.position);
            yield return new WaitForSeconds(.4f);yield return Capture("10-hot-cargo");
            Heat=0;ResetPolice();Teleport(Jobs.Home);OpenSafehouse();
            yield return Capture("11-safehouse");screen=ScreenMode.Play;autoInteract=true;
            yield return new WaitForSeconds(1.4f);yield return Capture("12-extraction");autoInteract=false;
            Check(true,"Twelve non-black screens captured from Windows player");
        }
        IEnumerator TakeCargo(int index)
        {
            yield return Travel(CargoRun.Sites[index].position);
            autoInteract=true;float deadline=Time.realtimeSinceStartup+12;
            while(!Cargo.Taken(index))
            {
                if(Time.realtimeSinceStartup>deadline)throw new Exception("Cargo hold interaction did not finish at site "+index);
                yield return null;
            }
            autoInteract=false;yield return null;
            Check(Cargo.Taken(index),"Actual hold interaction collected cargo site "+index);
        }
        IEnumerator BankCargo()
        {
            autoInteract=true;float deadline=Time.realtimeSinceStartup+8;
            while(Cargo.Value>0)
            {
                if(Time.realtimeSinceStartup>deadline)throw new Exception("Safehouse extraction did not finish");
                yield return null;
            }
            autoInteract=false;yield return null;
        }
        IEnumerator CargoSteps()
        {
            ContinueFreeroam();Check(screen==ScreenMode.Play&&State.Finished,"Completed campaign continues into free roam");
            Check(State.CargoCapacity==6&&Cargo.Count==0,"New cargo run starts with empty six-slot bag");
            yield return TakeCargo(0);yield return TakeCargo(1);
            Check(Cargo.Value==170&&Cargo.Weight==5&&Cargo.Count==2,"Two cargo sites add $170 and five weight to bag");
            Check(Cargo.SpeedMultiplier<1,"Loaded cargo reduces movement speed");
            int value=Cargo.Value;Cargo.Take(2,State);
            Check(!Cargo.Taken(2)&&Cargo.Value==value,"Cargo capacity refuses oversized pickup without changing bag");
            yield return Travel(Jobs.Home);yield return BankCargo();
            Check(State.cash==910&&Cargo.Count==0&&State.cargoRuns==1&&State.cargoEarnings==170,"Actual extraction banks cargo once and clears bag");
            var loaded=RunState.Load(savePath);
            Check(loaded!=null&&loaded.cash==910&&loaded.cargoRuns==1,"Banked cargo and successful run persist");
            OpenSafehouse();Check(screen==ScreenMode.Safehouse,"Cool player opens safehouse management");BuyCargoSatchel();
            Check(State.satchel&&State.CargoCapacity==9&&State.cash==730,"Satchel purchase costs $180 and expands bag to nine");
            BuyCargoSatchel();Check(State.cash==730,"Purchased satchel cannot charge player again");screen=ScreenMode.Play;
            yield return TakeCargo(2);Check(Heat>0,"Alarmed cargo pickup creates heat");
            int hotValue=Cargo.Value;Teleport(Jobs.Home);autoInteract=true;
            yield return new WaitForSeconds(.5f);
            Check(Cargo.Value==hotValue&&State.cash==730&&!Extracting,"Heat blocks safehouse extraction");
            OpenSafehouse();Check(screen==ScreenMode.Play,"Heat blocks opening safehouse");
            autoInteract=false;Heat=0;ResetPolice();yield return null;
            autoInteract=true;yield return new WaitForSeconds(.7f);
            Check(Extracting&&Cargo.Value==hotValue,"Extraction requires a sustained hold");
            autoInteract=false;yield return null;yield return null;
            Check(!Extracting&&extractionProgress==0&&Cargo.Value==hotValue,"Releasing interaction resets extraction without banking");
            yield return BankCargo();
            Check(State.cash==730+hotValue&&State.cargoRuns==2,"Second extraction banks alarmed cargo after heat clears");
            int secured=State.cash;Cargo.Take(0,State);StartRun(true);
            Check(State.satchel&&State.cash==secured&&State.cargoRuns==2&&Cargo.Count==0,"Reload preserves satchel and secured cash but clears loose cargo");
        }
        IEnumerator Travel(Vector3 target)
        {
            Vector3 interaction=target;target=City.Nav.SafePoint(target);
            var path=City.Nav.Find(Player.position,target);
            Check(path.Count>0,"Navigable route to "+target);
            float deadline=Time.realtimeSinceStartup+60, repath=Time.realtimeSinceStartup+1;
            int point=0;
            while(Vector2.Distance(new Vector2(Player.position.x,Player.position.z),new Vector2(target.x,target.z))>.38f)
            {
                if(Time.realtimeSinceStartup>deadline)throw new Exception("Movement blocked at "+Player.position+" toward "+target);
                if(point>=path.Count||!City.Nav.Walkable(path[point])||Time.realtimeSinceStartup>repath)
                { path=City.Nav.Find(Player.position,target);point=0;repath=Time.realtimeSinceStartup+2; }
                while(point<path.Count&&Vector2.Distance(new Vector2(Player.position.x,Player.position.z),new Vector2(path[point].x,path[point].z))<.38f)point++;
                autoMove=point<path.Count?(Vector3?)path[point]:null;
                yield return null;
            }
            autoMove=null;yield return null;
            Check(Vector3.Distance(Player.position,target)<1&&Vector3.Distance(Player.position,interaction)<2.5f,"Controller arrived beside interaction at "+interaction);
        }
        IEnumerator SmokeSteps()
        {
            foreach(var agent in Agents)Check(City.Nav.Walkable(agent.Position),"Agent starts on walkable street: "+agent.Body.name);
            yield return new WaitForSeconds(2);yield return Capture("01-title");
            StartRun(false);smokeFreezeAgents=true;
            yield return Travel(Jobs.Mara);
            Talk();Check(screen==ScreenMode.Talk,"Mara dialogue opens in interaction range");
            yield return Capture("02-mara");screen=ScreenMode.Play;
            State.Accept();Save();
            for(int j=0;j<3;j++)
            {
                if(j>0)State.Accept();
                yield return Travel(Jobs.All[j].position);
                autoInteract=true;
                float deadline=Time.realtimeSinceStartup+12;
                while(!State.carrying) { if(Time.realtimeSinceStartup>deadline)throw new Exception("Theft did not finish");yield return null; }
                autoInteract=false;
                Check(State.carrying,"Actual hold interaction acquired job "+(j+1));
                if(j==0) { showMap=true;yield return Capture("03-map");showMap=false;yield return Capture("04-street"); }
                if(j==2)
                {
                    Check(Heat>0,"Depot theft always triggers alarm");
                    yield return Capture("06-alarm");
                    Check(!State.Deliver(Heat),"Wanted player cannot deliver goods");
                }
                yield return Travel(Jobs.Mara);
                while(Heat>0)yield return null;
                Talk();Check(State.completed==j+1,"Delivery advances job "+(j+1));
                if(j<2)
                {
                    Check(screen==ScreenMode.Perk,"Delivery offers upgrade");
                    if(j==0)yield return Capture("05-upgrade");
                    Check(State.ChoosePerk(j==0?2:1),"Permanent perk selected");Save();screen=ScreenMode.Play;
                }
            }
            Check(State.Finished&&State.cash==740,"Three jobs reach ending with $740");
            yield return Capture("07-ending");
            var loaded=RunState.Load(savePath);Check(loaded!=null&&loaded.Finished&&loaded.perks==State.perks,"Completed run persists and reloads");
            yield return CargoSteps();
            // Exercise actual visibility, chase, obstacle occlusion, cooldown and arrest independently.
            State=new RunState();State.Accept();screen=ScreenMode.Play;smokeFreezeAgents=false;
            Teleport(new Vector3(12,0,-13));
            foreach(var a in Agents) if(a.Police) { a.Body.position=new Vector3(-44,0,42);a.Repath=0;a.Pursuing=false; }
            var cop=Agents[0];cop.Body.position=new Vector3(18,0,-13);cop.Body.LookAt(Player.position);cop.Suspicion=0;
            RaiseAlarm("Police behavior check",Player.position);
            yield return new WaitForSeconds(.7f);
            Check(cop.Pursuing&&cop.SeesPlayer,"Officer sees and pursues wanted player");
            Vector3 remembered=LastSeen;
            Teleport(new Vector3(18,0,-37));yield return new WaitForSeconds(.25f);
            Check(!cop.SeesPlayer,"Building interrupts officer line of sight");
            Check(Vector3.Distance(LastSeen,remembered)<2,"Unseen player does not update search location");
            smokeFreezeAgents=true;
            foreach(var a in Agents)a.SeesPlayer=false;
            yield return new WaitForSeconds(13);
            Check(Heat==0,"Unseen heat expires");
            smokeFreezeAgents=false;State.Steal();State.cash=120;Cargo.Take(0,State);
            Check(Cargo.Value>0,"Arrest check begins with loose cargo");
            Teleport(new Vector3(12,0,-13));cop.Body.position=new Vector3(12.4f,0,-13);cop.Body.LookAt(Player.position);
            RaiseAlarm("Arrest check",Player.position);int previous=State.arrests;
            float timeout=Time.realtimeSinceStartup+6;
            while(State.arrests==previous) { if(Time.realtimeSinceStartup>timeout)throw new Exception("Officer failed to arrest");yield return null; }
            Check(State.cash==80&&!State.carrying&&State.accepted,"Arrest fines cash, removes goods and preserves retryable job");
            Check(Cargo.Value==0&&Cargo.Count==0,"Actual arrest confiscates loose cargo");
            Check(Vector3.Distance(Player.position,Jobs.Home)<1,"Arrest returns player to safehouse");
            smokeFreezeAgents=true;RaiseAlarm("Pause check",Player.position);screen=ScreenMode.Pause;
            float heat=Heat;yield return new WaitForSeconds(.5f);Check(Heat==heat,"Pause freezes simulation");
            yield return Capture("08-pause");
            screen=ScreenMode.Play;Heat=0;ResetPolice();smokeFreezeAgents=false;
            var starts=new List<Vector3>();foreach(var agent in Agents)starts.Add(agent.Position);
            yield return new WaitForSeconds(4);
            for(int i=0;i<Agents.Count;i++)Check(Vector3.Distance(starts[i],Agents[i].Position)>1,"Town agent walks its route: "+Agents[i].Body.name);
        }
    }
}
