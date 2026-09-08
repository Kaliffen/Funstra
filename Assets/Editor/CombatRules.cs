using System;
using System.IO;
using Funstra;
using UnityEngine;

public static class CombatRules
{
    public static void Verify()
    {
        int count=0;Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception("Combat rule: "+label);count++;Debug.Log("COMBAT PASS / "+label);};
        var pistol=new WeaponState();int rounds=12;pistol.Initialize(rounds);
        check(pistol.magazine==6&&rounds==12,"Magazine migration partitions existing rounds without granting ammunition");
        check(pistol.Fire(ref rounds)&&rounds==11&&pistol.magazine==5,"Firing consumes exactly one total and loaded round");
        check(!pistol.Fire(ref rounds)&&rounds==11,"Cooldown prevents repeated same-frame fire");
        pistol.Step(.5f,rounds);check(pistol.BeginReload(rounds),"Partial magazine can reload");
        pistol.Step(.7f,rounds);pistol.CancelReload();pistol.Step(2,rounds);
        check(pistol.magazine==5&&rounds==11&&pistol.reloadRemaining==0,"Interrupted reload preserves magazine and ammunition");
        pistol.BeginReload(rounds);pistol.Step(1.5f,rounds);
        check(pistol.magazine==6&&rounds==11,"Completed reload repartitions total without inventing rounds");
        rounds=2;pistol.Initialize(rounds);check(pistol.magazine==2,"External confiscation clamps magazine to remaining stock");
        var empty=new WeaponState();int zero=0;empty.Initialize(zero);
        check(!empty.Fire(ref zero)&&!empty.BeginReload(zero),"Empty weapon cannot fire or complete a free reload");
        var shotgun=new WeaponState{kind=3};int shells=8;shotgun.Initialize(shells,3);
        check(shotgun.magazine==2&&shotgun.Fire(ref shells)&&shells==7,"Shotgun spends one shell per multi-projectile shot");
        check(WeaponSpec.Shotgun.pellets==7&&WeaponSpec.Shotgun.interval>WeaponSpec.Pistol.interval&&WeaponSpec.Shotgun.reload>WeaponSpec.Pistol.reload,"Shotgun has separate pellets and committed handling");
        var center=new Vector3(0,1,0);
        check(ProjectileMath.MovingSphere(new Vector3(-2,1,0),new Vector3(2,1,0),center,center,.44f,out float at)&&at>0&&at<1,"Swept bullet strikes stationary actor between endpoints");
        check(ProjectileMath.MovingSphere(new Vector3(-2,1,0),new Vector3(2,1,0),new Vector3(0,1,-2),new Vector3(0,1,2),.44f,out _),"Relative sweep catches actor crossing the bullet during a frame");
        check(!ProjectileMath.MovingSphere(new Vector3(-2,1,0),new Vector3(2,1,0),new Vector3(0,1,3),new Vector3(0,1,4),.44f,out _),"Moving actor outside path remains unhit");
        Bounds thin=new Bounds(new Vector3(0,1,0),new Vector3(.02f,2,4));
        foreach(float dt in new[]{1f/15,1f/30,1f/60,1f/144})
        {
            Vector3 from=new Vector3(-3,1,0);bool hit=false;
            for(int i=0;i<200&&!hit;i++){Vector3 to=from+Vector3.right*48*dt;hit=ProjectileMath.MovingBounds(from,to,thin,thin,ProjectileMath.Radius,out _);from=to;}
            check(hit,"Thin cover cannot be tunneled at "+Mathf.RoundToInt(1/dt)+" FPS");
        }
        Bounds previous=new Bounds(new Vector3(0,1,-2),new Vector3(.1f,2,.2f)),current=new Bounds(new Vector3(0,1,2),new Vector3(.1f,2,.2f));
        check(ProjectileMath.MovingBounds(new Vector3(-2,1,0),new Vector3(2,1,0),previous,current,ProjectileMath.Radius,out _),"Moving cover crossing between snapshots stops a swept round");
        check(ProjectileMath.MovingBounds(center,center+Vector3.right,thin,thin,ProjectileMath.Radius,out at)&&at==0,"A muzzle already inside cover produces a zero-distance obstruction");
        var state=new RunState();state.district.InitializeWeapons();state.district.pistolWeapon.Fire(ref state.district.ammo);state.district.pistolWeapon.Step(.5f,state.district.ammo);state.district.pistolWeapon.BeginReload(state.district.ammo);state.district.pistolWeapon.Step(.4f,state.district.ammo);
        state.district.projectiles.Add(new CombatProjectile{position=new Vector3(1,1,0),velocity=Vector3.right*48,remaining=34,damage=28,owner="player",kind=2});
        string path=Path.Combine(Path.GetTempPath(),"funstra-combat-rules-"+Guid.NewGuid().ToString("N")+".json");
        try
        {
            check(state.Save(path),"In-flight and reload checkpoint writes");var loaded=RunState.Load(path);
            check(loaded!=null&&loaded.district.projectiles.Count==1&&loaded.district.projectiles[0].position==new Vector3(1,1,0),"Reload resumes physical bullet at saved position");
            check(loaded.district.ammo==11&&loaded.district.pistolWeapon.magazine==5&&Mathf.Abs(loaded.district.pistolWeapon.reloadRemaining-1.1f)<.001f,"Mid-reload save retains committed ammo and remaining duration");
            loaded.district.pistolWeapon.Step(1.1f,loaded.district.ammo);
            check(loaded.district.ammo==11&&loaded.district.pistolWeapon.magazine==6,"Resumed reload seats rounds exactly once");
            loaded.district.projectiles.Clear();loaded.Save(path);check(RunState.Load(path).district.projectiles.Count==0,"Spent projectile is absent from subsequent checkpoint");
        }
        finally {if(File.Exists(path))File.Delete(path);}
        state.district.projectiles[0].velocity.x=float.NaN;check(!state.district.Valid(),"Malformed nonfinite projectile rejected");
        Debug.Log("COMBAT RULES COMPLETE / "+count+" checks");
    }
}
