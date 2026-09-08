using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public enum CombatRole { Anchor, Flank, Support }

    [Serializable] public sealed class CombatMemberState
    {
        public DistrictActor actor;
        public CombatRole role;
        public Vector3 home, retreat, lastContact;
        public float observedAt=-100, nextReport, reportDue=-1, reportObservedAt, nextShot;
        public Vector3 reportPosition;
        public bool withdrawing;
        public int gun=2;
        public int dressings, dressingsUsed;
        public string aidTarget="";
        public float aidProgress, aidHealth;
    }

    [Serializable] public sealed class CombatSquadState
    {
        public bool initialized;
        public int crewVersion;
        public bool leaderLost;
        public float clock;
        public List<CombatMemberState> members=new List<CombatMemberState>();
        public bool Valid()
        {
            if(!ProjectileMath.Finite(clock)||clock<0||members==null||members.Count>3||crewVersion<0||crewVersion>1)return false;
            var ids=new HashSet<string>();
            foreach(var m in members)
            {
                if(m==null||m.actor==null||string.IsNullOrEmpty(m.actor.id)||!ids.Add(m.actor.id))return false;
                if(!ProjectileMath.Finite(m.actor.position)||!ProjectileMath.Finite(m.home)||!ProjectileMath.Finite(m.retreat)||!ProjectileMath.Finite(m.lastContact)||!ProjectileMath.Finite(m.reportPosition))return false;
                if(!ProjectileMath.Finite(m.actor.health)||m.actor.health<0||m.actor.health>100||m.actor.ammo<0)return false;
                if(m.actor.combat!=null&&!m.actor.combat.Valid(m.actor.ammo))return false;
                if(m.gun!=2&&m.gun!=3&&m.gun!=5||(int)m.role<0||(int)m.role>2)return false;
                if(m.dressings<0||m.dressingsUsed<0||m.dressings+m.dressingsUsed>1||!ProjectileMath.Finite(m.aidProgress)||m.aidProgress<0||m.aidProgress>CombatSquad.AidDuration||!ProjectileMath.Finite(m.aidHealth))return false;
                if(!ProjectileMath.Finite(m.observedAt)||!ProjectileMath.Finite(m.nextReport)||!ProjectileMath.Finite(m.reportDue)||!ProjectileMath.Finite(m.reportObservedAt)||!ProjectileMath.Finite(m.nextShot))return false;
            }
            return !initialized||members.Count>0;
        }
        // A versioned, one-time equipment allocation preserves existing wounds, ammunition,
        // loaded rounds, defeats and reports when a pre-crew save enters this slice.
        public void InitializeCrewOpposition()
        {
            if(crewVersion!=0)return;
            crewVersion=1;
            foreach(var m in members)
            {
                m.dressings=1;
                m.gun=m.role==CombatRole.Flank?3:5;
                if(m.actor.combat==null)m.actor.combat=new WeaponState();
                m.actor.combat.Initialize(m.actor.ammo,m.gun);
                m.actor.weaponRecoverable=!m.actor.looted;
            }
            leaderLost=members.Exists(m=>m.role==CombatRole.Anchor&&m.actor.health<=0);
            if(leaderLost)foreach(var m in members)m.reportDue=-1;
        }
    }

    public sealed class CombatSquadMember
    {
        public CombatMemberState Data;
        public DistrictActor Actor => Data.actor;
        public Transform Body;
        public bool DirectSight;
        public string Order="Hold";
        public Vector3 Goal;
        public float ContactAge;
        public Vector3 VisibleTarget;
        internal float repath, decision, aimedFor;
        internal readonly List<Vector3> path=new List<Vector3>();
    }

    // The entire group's knowledge comes from individual observations and delayed reports.
    // No projectile or weapon implementation is duplicated here: both are injected production services.
    public sealed class CombatSquad
    {
        public const float ReportDelay=.65f, ReportRange=16, ContactLifetime=7;
        public const float AidDuration=3;
        public readonly CombatSquadState State;
        public readonly List<CombatSquadMember> Members=new List<CombatSquadMember>();
        readonly CityNavigation nav;
        readonly Func<DistrictActor,Transform,Vector3,int,bool> fire;
        readonly Action<DistrictActor,float> stepWeapon;
        readonly Func<Vector3,Vector3,bool> sight;
        readonly List<Vector3> targets=new List<Vector3>();

        public CombatSquad(CombatSquadState state,CityNavigation navigation,
            Func<DistrictActor,Transform,Vector3,int,bool> fireWeapon,
            Action<DistrictActor,float> tickWeapon,Func<Vector3,Vector3,bool> canSee=null)
        {
            State=state??new CombatSquadState();nav=navigation;fire=fireWeapon;stepWeapon=tickWeapon;
            sight=canSee??navigation.Sight;
            if(State.members==null)State.members=new List<CombatMemberState>();
            foreach(var data in State.members)Members.Add(new CombatSquadMember {Data=data,Goal=data.actor.position});
        }

        public CombatSquadMember Add(DistrictActor actor,Transform body,CombatRole role,Vector3 home,Vector3 retreat,int gun=2)
        {
            var existing=Members.Find(m=>m.Actor.id==actor.id);
            if(existing!=null) { existing.Body=body;return existing; }
            var data=new CombatMemberState {actor=actor,role=role,home=home,retreat=retreat,gun=gun};
            State.members.Add(data);State.initialized=true;
            var member=new CombatSquadMember {Data=data,Body=body,Goal=home};Members.Add(member);return member;
        }
        public void Bind(string actorId,Transform body)
        { var m=Members.Find(v=>v.Actor.id==actorId);if(m!=null) {m.Body=body;body.position=m.Actor.position;} }

        // Noise is an uncertain location, not identification or permission to fire through walls.
        public void Alert(Vector3 position)
        {
            foreach(var m in Members)
                if(m.Actor.health>0&&Vector3.Distance(m.Actor.position,position)<=ReportRange)
                    Observe(m,position,State.clock);
        }
        public void ClearContact()
        {
            foreach(var m in Members)
            {m.Data.observedAt=-100;m.Data.reportDue=-1;m.DirectSight=false;m.path.Clear();m.Order="Hold";m.aimedFor=0;}
        }
        void Observe(CombatSquadMember m,Vector3 position,float when)
        {
            if(when<m.Data.observedAt)return;
            m.Data.lastContact=position;m.Data.observedAt=when;
        }
        public void Step(float dt,Vector3 targetPosition,bool targetExposed,bool engaged)
        { Step(dt,targetExposed?new[]{targetPosition}:Array.Empty<Vector3>(),engaged,targetPosition); }
        public void Step(float dt,IEnumerable<Vector3> exposedTargets,bool engaged)
        { Step(dt,exposedTargets,engaged,new Vector3(1000,0,1000)); }
        void Step(float dt,IEnumerable<Vector3> exposedTargets,bool engaged,Vector3 legacyTarget)
        {
            if(dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;
            targets.Clear();if(exposedTargets!=null)targets.AddRange(exposedTargets);
            State.clock+=dt;
            if(State.crewVersion>0&&!State.leaderLost&&Members.Exists(m=>m.Data.role==CombatRole.Anchor&&m.Actor.health<=0))
            {
                State.leaderLost=true;
                foreach(var m in Members){m.Data.reportDue=-1;m.Data.nextReport=State.clock+1.2f;m.aimedFor=0;}
            }
            // Reports retain the location and observation time from send, never a live target reference.
            foreach(var sender in Members)
            {
                var d=sender.Data;
                if(d.reportDue<0||d.reportDue>State.clock)continue;
                d.reportDue=-1;
                if(sender.Actor.health<=0)continue;
                foreach(var receiver in Members)
                    if(receiver!=sender&&receiver.Actor.health>0&&Vector3.Distance(sender.Actor.position,receiver.Actor.position)<=(State.leaderLost?5:ReportRange))
                        Observe(receiver,d.reportPosition,d.reportObservedAt);
            }
            int living=0;foreach(var m in Members)if(m.Actor.health>0)living++;
            foreach(var m in Members)
            {
                m.DirectSight=false;
                if(m.Actor.health<=0)continue;
                stepWeapon?.Invoke(m.Actor,dt);
                float nearest=m.Data.gun==5?28*28:20*20;
                foreach(var candidate in targets)
                {
                    Vector3 delta=Flat(candidate-m.Actor.position);
                    bool cone=delta.sqrMagnitude<9||m.Body==null||Vector3.Dot(m.Body.forward,delta.normalized)>.05f;
                    if(delta.sqrMagnitude<nearest&&cone&&sight(m.Actor.position,candidate))
                    {nearest=delta.sqrMagnitude;m.DirectSight=true;m.VisibleTarget=candidate;}
                }
                if(m.DirectSight)
                {
                    Observe(m,m.VisibleTarget,State.clock);
                    if(m.Data.reportDue<0&&State.clock>=m.Data.nextReport)
                    {
                        m.Data.reportPosition=m.VisibleTarget;m.Data.reportObservedAt=State.clock;
                        m.Data.reportDue=State.clock+(State.leaderLost?1.2f:ReportDelay);m.Data.nextReport=State.clock+(State.leaderLost?2.4f:1.2f);
                    }
                }
                m.ContactAge=State.clock-m.Data.observedAt;
            }
            foreach(var m in Members)StepMember(m,dt,m.DirectSight?m.VisibleTarget:legacyTarget,engaged,living);
        }
        void StepMember(CombatSquadMember m,float dt,Vector3 target,bool engaged,int living)
        {
            var a=m.Actor;var d=m.Data;
            if(a.health<=0)
            { CancelAid(m);m.Order="Incapacitated";m.path.Clear();if(m.Body)m.Body.localScale=new Vector3(1.7f,.22f,1);return; }
            if(m.Body)m.Body.localScale=Vector3.one;
            bool contact=m.ContactAge<ContactLifetime;
            if(engaged&&contact&&(a.health<30||Members.Count>=3&&living==1||a.ammo<=0))d.withdrawing=true;
            if(State.crewVersion>0&&State.leaderLost&&engaged&&contact)d.withdrawing=true;
            if(State.crewVersion>0&&TryAid(m,dt))return;
            m.decision-=dt;
            string order;
            Vector3 goal=a.position;
            if(!engaged||!contact)
            {order=d.withdrawing?"Withdrawn":"Return to post";goal=d.withdrawing?d.retreat:d.home;}
            else if(d.withdrawing)
            {order="Retreat";goal=d.retreat;}
            else if(a.combat!=null&&a.combat.reloadRemaining>0)
            {order="Reload";goal=a.position;}
            else if(!m.DirectSight)
            {
                Vector3 away=Flat(d.home-d.lastContact).normalized;if(away.sqrMagnitude<.01f)away=Vector3.back;
                if(d.role==CombatRole.Anchor) {order="Watch last contact";goal=a.position;}
                else {
                    order="Search last contact";
                    goal=d.lastContact+away*(d.role==CombatRole.Support?6:2)+Vector3.Cross(Vector3.up,away)*(d.role==CombatRole.Support?-3:3);
                    goal=d.home+Vector3.ClampMagnitude(goal-d.home,12);
                }
                if(m.Body) {Vector3 facing=Flat(d.lastContact-a.position);if(facing.sqrMagnitude>.01f)m.Body.rotation=Quaternion.Slerp(m.Body.rotation,Quaternion.LookRotation(facing),dt*5);}
            }
            else
            {
                float distance=Flat(d.lastContact-a.position).magnitude;
                bool covering=Members.Exists(other=>other!=m&&other.Actor.health>0&&other.DirectSight&&!other.Data.withdrawing&&other.Actor.ammo>0&&(other.Actor.combat==null||other.Actor.combat.reloadRemaining<=0));
                if(distance<4)
                {order="Make distance";goal=a.position+Flat(a.position-d.lastContact).normalized*4;}
                else if(d.role==CombatRole.Flank&&covering)
                {
                    order="Reposition";
                    Vector3 away=Flat(d.home-d.lastContact).normalized;if(away.sqrMagnitude<.01f)away=Vector3.back;
                    goal=d.lastContact+away*8+Vector3.Cross(Vector3.up,away)*5;
                    // A flank belongs to this site, not an unlimited chase across the town.
                    goal=d.home+Vector3.ClampMagnitude(goal-d.home,12);
                }
                else {order=d.role==CombatRole.Anchor?"Cover approach":"Hold / cover ally";goal=a.position;}
            }
            bool changed=m.Order!=order;m.Order=order;a.order=order;
            if(changed||m.decision<=0)
            {
                m.decision=1.3f;
                if(nav.Walkable(goal))m.Goal=goal;
                else {m.Goal=a.position;m.Order="Blocked / hold";}
                m.repath=0;
            }
            Vector3 previous=a.position;
            bool moving=Flat(m.Goal-a.position).sqrMagnitude>.35f*.35f;
            if(moving)Move(m,dt,target);
            if(m.DirectSight&&engaged&&contact&&!d.withdrawing)
            {
                Vector3 direction=Flat(target-a.position);
                if(m.Body&&direction.sqrMagnitude>.01f)m.Body.rotation=Quaternion.Slerp(m.Body.rotation,Quaternion.LookRotation(direction),dt*8);
                m.aimedFor+=dt;
                // Acquisition takes time; a moving flanker pauses firing until positioned.
                if(m.aimedFor>=(d.gun==5?.85f:.5f)&&State.clock>=d.nextShot&&(!moving||Flat(a.position-previous).sqrMagnitude<.00001f))
                {
                    float wobble=Mathf.Sin(State.clock*2.3f+(int)d.role*2)*.55f;
                    Vector3 aim=target+Vector3.Cross(Vector3.up,direction.normalized)*wobble;
                    if(fire!=null&&fire(a,m.Body,aim,d.gun))d.nextShot=State.clock+(d.gun==3?1.8f:d.gun==5?1.65f:1.25f);
                }
            }
            else m.aimedFor=0;
            if(m.Body)
            {m.Body.position=a.position;CityArt.Animate(m.Body,State.clock,Vector3.Distance(a.position,previous)/Mathf.Max(.001f,dt));}
        }
        void CancelAid(CombatSquadMember m)
        {m.Data.aidTarget="";m.Data.aidProgress=0;}
        bool TryAid(CombatSquadMember m,float dt)
        {
            var d=m.Data;
            if(m.DirectSight||d.dressings<=0||m.Actor.health<30||!string.IsNullOrEmpty(d.aidTarget)&&m.Actor.health<d.aidHealth)
            {CancelAid(m);return false;}
            var patient=Members.Find(other=>other.Actor.id==d.aidTarget);
            if(patient==null)
            {
                // Reachable, nearby casualties only. Helping requires physical access,
                // three uninterrupted seconds and one dressing; no remote restoration.
                patient=Members.Find(other=>other!=m&&!other.Actor.looted&&other.Actor.health<55&&Flat(other.Actor.position-m.Actor.position).sqrMagnitude<=8*8&&sight(m.Actor.position,other.Actor.position)&&nav.ClearWalk(m.Actor.position,other.Actor.position));
                if(patient==null)return false;
                d.aidTarget=patient.Actor.id;d.aidHealth=m.Actor.health;
            }
            if(patient.Actor.looted||patient.Actor.health>=55||Flat(patient.Actor.position-m.Actor.position).sqrMagnitude>8*8||!sight(m.Actor.position,patient.Actor.position)||!nav.ClearWalk(m.Actor.position,patient.Actor.position))
            {CancelAid(m);return false;}
            if(Flat(patient.Actor.position-m.Actor.position).sqrMagnitude>2*2)
            {
                m.Order=m.Actor.order="Reach wounded ally";
                m.Goal=patient.Actor.position+Flat(m.Actor.position-patient.Actor.position).normalized*1.5f;
                d.aidProgress=0;m.aimedFor=0;
                var before=m.Actor.position;Move(m,dt,new Vector3(1000,0,1000));
                if(m.Body){m.Body.position=m.Actor.position;CityArt.Animate(m.Body,State.clock,Vector3.Distance(before,m.Actor.position)/Mathf.Max(.001f,dt));}
                return true;
            }
            m.Order=m.Actor.order="Dressing "+patient.Actor.name;m.path.Clear();m.Goal=m.Actor.position;m.aimedFor=0;
            d.aidProgress=Mathf.Min(AidDuration,d.aidProgress+dt);
            if(d.aidProgress>=AidDuration)
            {
                d.dressings--;d.dressingsUsed++;patient.Actor.health=Mathf.Min(100,patient.Actor.health+25);patient.Actor.bleeding=false;
                // A recovered casualty leaves the encounter rather than refilling its role.
                if(patient.Actor.health<=30||State.leaderLost)patient.Data.withdrawing=true;
                CancelAid(m);
            }
            return true;
        }
        void Move(CombatSquadMember m,float dt,Vector3 target)
        {
            m.repath-=dt;
            if(m.repath<=0)
            {m.path.Clear();m.path.AddRange(nav.Find(m.Actor.position,m.Goal));m.repath=.75f;}
            while(m.path.Count>0&&Flat(m.path[0]-m.Actor.position).sqrMagnitude<.04f)m.path.RemoveAt(0);
            if(m.path.Count==0)return;
            Vector3 next=Vector3.MoveTowards(m.Actor.position,m.path[0],(m.Data.withdrawing?3.5f:2.8f)*dt);next.y=0;
            bool occupied=Flat(next-target).sqrMagnitude<.85f*.85f;
            foreach(var candidate in targets)if(Flat(next-candidate).sqrMagnitude<.85f*.85f)occupied=true;
            foreach(var other in Members)if(other!=m&&Flat(next-other.Actor.position).sqrMagnitude<.85f*.85f)occupied=true;
            if(!occupied&&nav.ClearWalk(m.Actor.position,next))
            {
                Vector3 direction=next-m.Actor.position;m.Actor.position=next;
                if(m.Body&&direction.sqrMagnitude>.00001f)m.Body.rotation=Quaternion.Slerp(m.Body.rotation,Quaternion.LookRotation(direction),dt*10);
            }
            else {m.repath=0;m.Order="Blocked / replan";}
        }
        static Vector3 Flat(Vector3 v) {v.y=0;return v;}
    }
}


