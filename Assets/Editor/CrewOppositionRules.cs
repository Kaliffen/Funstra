using System;
using Funstra;
using UnityEngine;

public static class CrewOppositionRules
{
    // Focused production-rule checks. The candidate still requires rendered player routes.
    public static int Verify()
    {
        int count=0;
        Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception("Crew opposition: "+label);count++;Debug.Log("OPPOSITION PASS / "+label);};
        var rifle=new WeaponState{kind=5};int rounds=8;rifle.Initialize(rounds,5);
        check(WeaponSpec.For(5)==WeaponSpec.Rifle&&rifle.magazine==5&&rifle.Valid(rounds),"rifle has a valid separate five-round magazine");
        check(WeaponSpec.Rifle.speed>WeaponSpec.SMG.speed&&WeaponSpec.Rifle.interval>WeaponSpec.Pistol.interval&&WeaponSpec.Rifle.reload>WeaponSpec.Shotgun.reload,"rifle commits to slower handling and a faster projectile");
        for(int i=0;i<5;i++){rifle.Step(WeaponSpec.Rifle.interval,rounds);check(rifle.Fire(ref rounds),"finite rifle shot "+i);}
        check(rounds==3&&rifle.magazine==0&&!rifle.Fire(ref rounds),"five shots leave only the real reserve");
        check(rifle.BeginReload(rounds),"rifle starts reloading remaining reserve");rifle.Step(1,rounds);rifle.CancelReload();
        check(rifle.magazine==0&&rounds==3,"interrupted reload creates no rounds");
        rifle.BeginReload(rounds);rifle.Step(WeaponSpec.Rifle.reload,rounds);
        check(rifle.magazine==3&&rifle.Valid(rounds),"completed partial reload partitions existing reserve");
        var projectile=new CombatProjectile{owner="rell",kind=5,damage=WeaponSpec.Rifle.damage,velocity=Vector3.forward*WeaponSpec.Rifle.speed,remaining=WeaponSpec.Rifle.range};
        check(projectile.Valid(),"shared projectile accepts rifle and stable crew owner");
        foreach(float dt in new[]{1f/15,1f/30,1f/60,1f/144})
        {
            var wall=new Bounds(new Vector3(0,1,0),new Vector3(.02f,2,4));bool hit=false;Vector3 from=new Vector3(-3,1,0);
            for(int i=0;i<20&&!hit;i++){var to=from+Vector3.right*WeaponSpec.Rifle.speed*dt;hit=ProjectileMath.MovingBounds(from,to,wall,wall,ProjectileMath.Radius,out _);from=to;}
            check(hit,"rifle swept collision stops at thin cover at "+Mathf.RoundToInt(1/dt)+" fps");
        }

        var nav=new CityNavigation();nav.Bake();
        var state=new CombatSquadState();var squad=new CombatSquad(state,nav,null,null,(a,b)=>a.x<1);
        var vale=squad.Add(new DistrictActor("yard-0","VALE",Vector3.zero),null,CombatRole.Anchor,Vector3.zero,new Vector3(-8,0,0));
        var oss=squad.Add(new DistrictActor("yard-1","OSS",new Vector3(8,0,0)),null,CombatRole.Flank,new Vector3(8,0,0),new Vector3(16,0,0),3);
        var len=squad.Add(new DistrictActor("yard-2","LEN",new Vector3(12,0,0)),null,CombatRole.Support,new Vector3(12,0,0),new Vector3(20,0,0));
        vale.Actor.ammo=7;vale.Actor.health=72;state.InitializeCrewOpposition();
        check(state.crewVersion==1&&vale.Actor.health==72&&vale.Actor.ammo==7&&vale.Data.gun==5,"crew migration preserves wounds and ammunition");
        vale.Data.dressings=0;vale.Data.dressingsUsed=1;state.InitializeCrewOpposition();
        check(vale.Data.dressings==0&&vale.Data.dressingsUsed==1,"migration does not refill spent dressings");
        var seen=new Vector3(0,0,10);squad.Step(.01f,new[]{new Vector3(60,0,0),seen},false);
        check(vale.DirectSight&&vale.VisibleTarget==seen&&!oss.DirectSight,"each observer finds a visible crew member independent of selection");
        check(vale.Data.reportDue>state.clock&&oss.Data.observedAt<0,"crew sightings remain delayed reports");
        // A different sender also has a queued report when the leader is incapacitated.
        oss.Data.reportDue=state.clock+.2f;oss.Data.reportPosition=seen;oss.Data.reportObservedAt=state.clock;
        vale.Actor.health=0;squad.Step(1,Array.Empty<Vector3>(),false);
        check(state.leaderLost&&len.Data.observedAt<0&&oss.Data.reportDue<0,"leader loss cancels other members' already pending reports");
        oss.Data.reportDue=state.clock+.1f;oss.Data.reportPosition=seen;oss.Data.reportObservedAt=state.clock;
        squad.Step(.2f,Array.Empty<Vector3>(),false);
        check(len.Data.lastContact==seen,"remaining guards can communicate locally after leader loss");
        len.Actor.position=new Vector3(24,0,0);len.Data.observedAt=-100;
        oss.Data.reportDue=state.clock+.1f;oss.Data.reportObservedAt=state.clock;
        squad.Step(.2f,Array.Empty<Vector3>(),false);
        check(len.Data.observedAt<0,"leader loss limits surviving reports to nearby guards");

        var aidState=new CombatSquadState();var aid=new CombatSquad(aidState,nav,null,null,(a,b)=>true);
        var medic=aid.Add(new DistrictActor("medic","MEDIC",Vector3.zero),null,CombatRole.Anchor,Vector3.zero,new Vector3(-8,0,0));
        var patient=aid.Add(new DistrictActor("patient","PATIENT",Vector3.right*1.5f){health=0},null,CombatRole.Support,Vector3.right*1.5f,Vector3.right*8);
        aidState.InitializeCrewOpposition();
        aid.Step(1,Array.Empty<Vector3>(),false);
        check(patient.Actor.health==0&&medic.Data.aidProgress==1&&medic.Data.dressings==1,"aid takes time before consuming the finite dressing");
        float before=medic.Data.aidProgress;aid.Step(0,Array.Empty<Vector3>(),false);
        check(medic.Data.aidProgress==before,"pause freezes opposition aid");
        medic.Actor.health-=5;aid.Step(.1f,Array.Empty<Vector3>(),false);
        check(medic.Data.aidProgress==0&&patient.Actor.health==0&&medic.Data.dressings==1,"damage interrupts field aid without healing or spending");
        aid.Step(1,Array.Empty<Vector3>(),false);
        aidState=JsonUtility.FromJson<CombatSquadState>(JsonUtility.ToJson(aidState));
        var resumed=new CombatSquad(aidState,nav,null,null,(a,b)=>true);
        resumed.Step(2,Array.Empty<Vector3>(),false);
        check(aidState.members[0].dressings==0&&aidState.members[0].dressingsUsed==1&&aidState.members[1].actor.health==25,"reloaded aid completes once and spends exactly one dressing");
        check(aidState.members[1].withdrawing,"recovered casualty withdraws instead of returning to full combat");
        resumed.Step(4,Array.Empty<Vector3>(),false);
        check(aidState.members[1].actor.health==25&&aidState.Valid(),"used dressing cannot heal again and persisted opposition stays valid");

        var police=new PoliceResponse();police.trucks[0].requested=police.trucks[0].entered=police.trucks[0].arrived=true;police.trucks[0].deployed=1;
        var officer=new DistrictActor("response-0-0","OFFICER",Vector3.zero){health=0,ammo=3};officer.combat.Initialize(3,2);police.officers.Add(officer);
        police.InitializeCrewResponse();
        check(police.crewVersion==1&&police.trucks.Length==2&&police.trucks[0].deployed==1&&officer.health==0&&officer.ammo==3&&officer.combat.kind==5&&police.Valid(),"response migration preserves finite trucks, delivered identity, defeat and ammunition");
        officer.ammo=0;officer.combat.Initialize(0,5);police.InitializeCrewResponse();
        check(officer.ammo==0&&officer.combat.magazine==0,"response migration cannot refill a depleted officer");
        return count;
    }
}
