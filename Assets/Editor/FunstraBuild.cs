using System;
using System.IO;
using Funstra;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class FunstraBuild
{
    [MenuItem("Funstra/Build playable Windows demo")]
    public static void Build()
    { BuildPolice(); }
    [MenuItem("Funstra/Build police response candidate")]
    public static void BuildPolice()
    { BuildPlayer("PoliceResponse", "0.4.1"); }
    static void BuildPlayer(string folder, string version)
    {
        VerifyRules();
        DistrictRules.Verify();
        RefugeRules.Verify();
        SpatialRules.Verify();
        ClinicRules.Verify();
        CombatSquadRules.Verify();
        CombatRules.Verify();
        PoliceRules.Verify();
        // Runtime-created geometry still needs explicit build-time shader references.
        Directory.CreateDirectory("Assets/Resources/Rendering");
        EnsureMaterial("Standard", "Surface", false);
        EnsureMaterial("Standard", "Emission", true);
        EnsureMaterial("Sprites/Default", "Marker", false);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Funstra / Game").AddComponent<FunstraGame>();
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/OldPort.unity");
        PlayerSettings.companyName="Funstra";PlayerSettings.productName="Funstra";
        PlayerSettings.bundleVersion=version;
        PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
        PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone,ApiCompatibilityLevel.NET_Standard);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
        AssetDatabase.SaveAssets();Directory.CreateDirectory("Build/"+folder);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{"Assets/Scenes/OldPort.unity"},locationPathName="Build/"+folder+"/Funstra.exe",
            target=BuildTarget.StandaloneWindows64,options=BuildOptions.None
        });
        Directory.CreateDirectory("Evidence");
        File.WriteAllText("Evidence/build-result.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize+"\nDuration: "+report.summary.totalTime);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);
    }
    static void EnsureMaterial(string shader,string name,bool emission)
    {
        string path="Assets/Resources/Rendering/"+name+".mat";
        if(AssetDatabase.LoadAssetAtPath<Material>(path))return;
        var material=new Material(Shader.Find(shader));
        if(emission) { material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.white); }
        AssetDatabase.CreateAsset(material,path);
    }
    [MenuItem("Funstra/Verify game rules")]
    public static void VerifyRules()
    {
        int checks=0;
        Action<bool,string> check=(ok,text)=> { if(!ok)throw new Exception("Rule check failed: "+text);checks++; };
        var run=new RunState();check(!run.Steal(),"Cannot steal before accepting a job");
        for(int i=0;i<3;i++)
        {
            run.Accept();check(run.Steal(),"Accepted target can be stolen");check(!run.Steal(),"Cannot duplicate goods");
            check(!run.Deliver(1),"Heat blocks delivery");check(run.Deliver(0),"Clear delivery accepted");
            check(!run.Deliver(0),"Cannot duplicate reward");
            if(i<2) { check(run.NeedsPerk,"Completion awards perk");check(run.ChoosePerk(i),"Choose new perk");check(!run.ChoosePerk(i),"No duplicate perk"); }
        }
        check(run.Finished&&run.cash==740,"Complete arc pays $740");check(!run.Steal(),"Finished run has no fourth job");
        var retry=new RunState();retry.Accept();retry.Steal();retry.Arrest();check(retry.cash==0&&retry.accepted&&!retry.carrying,"Arrest retry invariant");check(retry.Steal(),"Target available again");
        string save=Path.Combine(Path.GetTempPath(),"funstra-test-"+Guid.NewGuid()+".json");
        try { check(run.Save(save),"Save succeeds");var loaded=RunState.Load(save);check(loaded!=null&&loaded.cash==740&&loaded.Finished,"Save round trip");File.WriteAllText(save,"bad json");check(RunState.Load(save)==null,"Malformed save recoverable"); }
        finally { if(File.Exists(save))File.Delete(save); }
        VerifyCargo(check);
        var nav=new CityNavigation();nav.Obstacles.Add(new Bounds(Vector3.zero,new Vector3(12,8,12)));nav.Bake();
        check(!nav.Sight(new Vector3(-10,0,0),new Vector3(10,0,0)),"Wall occludes vision");
        check(nav.Sight(new Vector3(-10,0,12),new Vector3(10,0,12)),"Open street preserves vision");
        var path=nav.Find(new Vector3(-10,0,0),new Vector3(10,0,0));check(path.Count>0,"Navigation routes around wall");
        for(int i=1;i<path.Count;i++)check(nav.ClearWalk(path[i-1],path[i]),"Route segment cannot clip wall");
        Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/rules-result.txt","PASS: "+checks+" rule, persistence and navigation checks\n");
        Debug.Log("FUNSTRA RULES PASSED: "+checks);
    }

    static void VerifyCargo(Action<bool,string> check)
    {
        var state=new RunState();var cargo=new CargoRun();
        check(state.CargoCapacity==6&&cargo.SpeedMultiplier==1f,"Starter bag holds six and starts unburdened");
        check(!cargo.Take(-1,state)&&!cargo.Take(CargoRun.Sites.Length,state)&&!cargo.Take(0,null),"Invalid cargo requests rejected");
        check(!cargo.Taken(-1)&&!cargo.Taken(CargoRun.Sites.Length),"Invalid cargo queries are safe");
        check(cargo.Bank(state,0)==0&&state.cargoRuns==0,"Empty bag cannot create a run");
        check(cargo.Take(0,state)&&cargo.Weight==2&&cargo.Value==60&&cargo.Count==1,"First cargo enters bag without paying cash");
        check(state.cash==0&&cargo.SpeedMultiplier==1f&&!cargo.Take(0,state),"Cargo cannot be duplicated and light load keeps speed");
        check(cargo.Take(1,state)&&cargo.Weight==5&&cargo.Value==170&&cargo.Count==2&&cargo.SpeedMultiplier==.8f,"Combined load totals correctly and slows at five");
        check(!cargo.Take(2,state)&&!cargo.Taken(2)&&cargo.Weight==5&&cargo.Value==170,"Capacity rejection preserves bag and source");
        check(cargo.Bank(state,1)==0&&cargo.Bank(state,float.NaN)==0&&cargo.Value==170&&state.cash==0,"Heat blocks cash out without consuming cargo");
        check(cargo.Bank(state,0)==170&&state.cash==170&&state.cargoRuns==1&&state.cargoEarnings==170,"Cold extraction awards exact value and records run");
        check(cargo.Count==0&&cargo.Weight==0&&cargo.Value==0&&!cargo.Taken(0)&&cargo.SpeedMultiplier==1f,"Extraction resets bag, sites and movement");
        check(cargo.Bank(state,0)==0&&state.cash==170&&state.cargoRuns==1,"Extraction cannot pay twice");
        check(!state.BuySatchel()&&state.cash==170&&!state.satchel,"Unaffordable satchel cannot spend cash");
        check(cargo.Take(2,state)&&cargo.Value==220,"Bonded cargo fits starter bag alone");
        cargo.Lose();
        check(cargo.Count==0&&cargo.Weight==0&&cargo.Value==0&&!cargo.Taken(2)&&state.cash==170&&state.cargoRuns==1&&state.cargoEarnings==170,"Lost cargo resets sites without banking or altering earnings");
        check(cargo.Take(2,state)&&cargo.Bank(state,0)==220,"Cargo can be recovered on a later run");
        check(state.BuySatchel()&&state.cash==210&&state.CargoCapacity==9,"Satchel costs exactly 180 and increases capacity");
        check(!state.BuySatchel()&&state.cash==210,"Satchel cannot be bought twice");
        check(cargo.Take(1,state)&&cargo.Take(2,state)&&cargo.Weight==8&&cargo.Value==330,"Satchel enables garage plus bonded cargo");
        check(!cargo.Take(0,state)&&cargo.Weight==8&&cargo.Value==330,"Even upgraded bag cannot carry all three sites");
        var exact=new RunState { cash=180 };check(exact.BuySatchel()&&exact.cash==0,"Exact satchel price is affordable");
        string save=Path.Combine(Path.GetTempPath(),"funstra-cargo-test-"+Guid.NewGuid()+".json");
        try
        {
            state.carrying=true;
            check(state.Save(save),"Cargo progression saves");
            var loaded=RunState.Load(save);
            check(loaded!=null&&loaded.satchel&&loaded.CargoCapacity==9&&loaded.cash==210&&loaded.cargoRuns==2&&loaded.cargoEarnings==390&&!loaded.carrying,"Upgrade and extracted earnings persist; carried goods do not");
            File.WriteAllText(save,"{\"version\":1,\"completed\":1,\"cash\":120,\"arrests\":0,\"perks\":1,\"accepted\":false}");
            loaded=RunState.Load(save);
            check(loaded!=null&&loaded.completed==1&&loaded.cash==120&&loaded.perks==1&&!loaded.satchel&&loaded.CargoCapacity==6&&loaded.cargoRuns==0&&loaded.cargoEarnings==0,"Legacy version one saves retain progress with starter cargo defaults");
            state.cargoRuns=-1;check(state.Save(save)&&RunState.Load(save)==null,"Negative cargo run count rejected");
            state.cargoRuns=0;state.cargoEarnings=-1;check(state.Save(save)&&RunState.Load(save)==null,"Negative cargo earnings rejected");
        }
        finally { if(File.Exists(save))File.Delete(save); }
    }
}
