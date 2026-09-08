using System;
using UnityEngine;

namespace Funstra
{
    public static class CombatSquadRules
    {
        public static int Verify()
        {
            int checks=0;
            Action<bool,string> check=(ok,message)=> {if(!ok)throw new Exception("Combat squad: "+message);checks++;};
            var nav=new CityNavigation();nav.Bake();
            Func<Vector3,Vector3,bool> firstOnly=(from,to)=>from.x<1;
            var state=new CombatSquadState();
            var group=new CombatSquad(state,nav,null,null,firstOnly);
            var spotter=group.Add(new DistrictActor("spotter","SPOTTER",Vector3.zero),null,CombatRole.Anchor,Vector3.zero,new Vector3(-8,0,0));
            var ally=group.Add(new DistrictActor("ally","ALLY",new Vector3(8,0,0)),null,CombatRole.Support,new Vector3(8,0,0),new Vector3(12,0,0));
            var remote=group.Add(new DistrictActor("remote","REMOTE",new Vector3(40,0,0)),null,CombatRole.Support,new Vector3(40,0,0),new Vector3(42,0,0));
            var observed=new Vector3(0,0,6);
            group.Step(.01f,observed,true,false);
            check(spotter.DirectSight&&!ally.DirectSight,"only unobstructed observer acquires sight");
            check(ally.Data.observedAt<0,"reports are delayed");
            group.Step(.3f,new Vector3(7,0,6),false,false);
            check(ally.Data.observedAt<0,"report does not arrive before delay");
            group.Step(.4f,new Vector3(7,0,6),false,false);
            check(ally.Data.lastContact==observed&&ally.Data.observedAt==.01f,"report carries old observation, not hidden live position");
            check(remote.Data.observedAt<0,"out of range ally receives no report");
            group.Step(CombatSquad.ContactLifetime+1,new Vector3(7,0,6),false,true);
            check(ally.ContactAge>CombatSquad.ContactLifetime&&ally.Order=="Return to post","stale contact expires and disengages");
            group.ClearContact();
            group.Step(.01f,observed,true,false);spotter.Actor.health=0;
            group.Step(1,observed,false,false);
            check(ally.Data.observedAt<0,"incapacitated observer cannot send pending report");
            check(spotter.Order=="Incapacitated","incapacitated actors stop acting");
            var saved=JsonUtility.FromJson<CombatSquadState>(JsonUtility.ToJson(state));
            check(saved.initialized&&saved.members.Count==3&&saved.members[0].actor.health==0,"save retains group identity and defeat");
            var restored=new CombatSquad(saved,nav,null,null,firstOnly);
            restored.Add(new DistrictActor("spotter","NEW",Vector3.zero),null,CombatRole.Anchor,Vector3.zero,Vector3.zero);
            check(restored.Members.Count==3&&restored.Members[0].Actor.health==0,"binding existing identity cannot respawn defeated guard");
            group.ClearContact();ally.Actor.health=20;group.Alert(ally.Actor.position);
            group.Step(.01f,observed,false,true);
            check(ally.Data.withdrawing&&ally.Order=="Retreat","injured actor retreats using known contact");
            float time=state.clock;group.Step(0,observed,true,true);
            check(state.clock==time,"paused squad advances no knowledge or timers");

            int shots=0;var shooting=new CombatSquad(new CombatSquadState(),nav,(actor,body,target,gun)=>{shots++;actor.ammo--;return true;},null,(a,b)=>true);
            var shooter=shooting.Add(new DistrictActor("fire","FIRE",Vector3.zero),null,CombatRole.Anchor,Vector3.zero,new Vector3(-6,0,0));
            for(int i=0;i<10;i++)shooting.Step(.1f,observed,false,true);
            check(shots==0,"hidden targets cannot be fired upon without sight");
            for(int i=0;i<4;i++)shooting.Step(.1f,observed,true,true);
            check(shots==0,"direct sight requires aim acquisition before firing");
            shooting.Step(.2f,observed,true,true);
            check(shots==1&&shooter.Actor.ammo==17,"visible shot uses injected shared weapon service");
            shooter.Actor.ammo=0;shooting.Step(.1f,observed,true,true);
            check(shooter.Data.withdrawing&&shots==1,"empty actor retreats instead of producing free shots");

            var roles=new CombatSquad(new CombatSquadState(),nav,null,null,(a,b)=>true);
            var anchor=roles.Add(new DistrictActor("anchor","ANCHOR",Vector3.zero),null,CombatRole.Anchor,Vector3.zero,new Vector3(-5,0,0));
            var flanker=roles.Add(new DistrictActor("flank","FLANK",new Vector3(8,0,0)),null,CombatRole.Flank,new Vector3(8,0,0),new Vector3(12,0,0));
            roles.Step(.1f,new Vector3(0,0,10),true,true);
            check(anchor.Order=="Cover approach"&&flanker.Order=="Reposition","covering ally enables a distinct flanking role");
            anchor.Actor.health=0;roles.Step(.1f,new Vector3(0,0,10),true,true);
            check(flanker.Order=="Hold / cover ally","loss of covering ally stops unsupported flank");
            flanker.Actor.combat=new WeaponState {reloadRemaining=1};roles.Step(.1f,new Vector3(0,0,10),true,true);
            check(flanker.Order=="Reload","reload prevents tactical advancing");
            var blockedNav=new CityNavigation();blockedNav.Obstacles.Add(new Bounds(new Vector3(2,1,0),new Vector3(1,2,12)));blockedNav.Bake();
            var blocked=new CombatSquad(new CombatSquadState(),blockedNav,null,null,(a,b)=>false);
            var searcher=blocked.Add(new DistrictActor("search","SEARCH",Vector3.zero),null,CombatRole.Support,Vector3.zero,new Vector3(-5,0,0));
            blocked.Alert(new Vector3(5,0,0));
            for(int i=0;i<20;i++) {Vector3 before=searcher.Actor.position;blocked.Step(.05f,new Vector3(5,0,0),false,true);check(blockedNav.ClearWalk(before,searcher.Actor.position),"search step respects blocking geometry");}
            Debug.Log("Combat squad rules: "+checks+" checks passed.");return checks;
        }
    }
}

