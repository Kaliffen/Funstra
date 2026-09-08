using System;
using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    // These records own orders and remembered outcomes. Existing protagonist and Neri
    // health/inventory remain at their canonical save locations; selection never swaps them.
    [Serializable] public sealed class CrewOrderState
    {
        public string id, order="Hold", patient="", carrying="", memory="";
        public Vector3 destination, aidOrigin;
        public float aidRemaining, aidHealth, stamina=100;
        public int medicinePractice, rescuedCount, abandonedCount;
        public bool abandoned, armed, holstered=true;
        public CrewOrderState(string actorId){id=actorId;}
    }
    [Serializable] public sealed class CrewState
    {
        public int version=1, mechanicsPractice;
        public string selectedId="player";
        public bool rellRecruited;
        public DistrictActor rell=new DistrictActor("rell","RELL",new Vector3(61,0,20)){ammo=0,bandages=1};
        public List<CrewOrderState> members=new List<CrewOrderState>{new CrewOrderState("player"),new CrewOrderState("neri"),new CrewOrderState("rell")};
        public CrewOrderState For(string id)=>members.Find(m=>m.id==id);
        public bool Valid()
        {
            if(version!=1||members==null||members.Count!=3||rell==null||rell.id!="rell"||mechanicsPractice<0)return false;
            var ids=new HashSet<string>();var patients=new HashSet<string>();
            foreach(var m in members)
            {
                if(m==null||(m.id!="player"&&m.id!="neri"&&m.id!="rell")||!ids.Add(m.id)||!ProjectileMath.Finite(m.destination)||!ProjectileMath.Finite(m.aidOrigin)||
                   !ProjectileMath.Finite(m.aidRemaining)||m.aidRemaining<0||m.aidRemaining>4||!ProjectileMath.Finite(m.stamina)||m.stamina<0||m.stamina>135||m.medicinePractice<0||m.rescuedCount<0||m.abandonedCount<0)return false;
                if(m.carrying!=""&&(m.carrying==m.id||(m.carrying!="player"&&m.carrying!="neri"&&m.carrying!="rell")||!patients.Add(m.carrying)))return false;
            }
            foreach(var m in members)if(m.carrying!=""&&For(m.carrying).carrying!="")return false;
            return ids.Contains(selectedId)&&ProjectileMath.Finite(rell.position)&&ProjectileMath.Finite(rell.health)&&rell.health>=0&&rell.health<=100&&rell.bandages>=0&&rell.ammo>=0&&rell.combat!=null&&rell.combat.Valid(rell.ammo);
        }
    }
    public sealed partial class DistrictState
    {
        public int crewVersion;
        public CrewState crew;
        public void InitializeCrew()
        {
            if(crewVersion!=0)return;
            crew=new CrewState();crewVersion=1;
            crew.For("neri").order=neri.order;
            crew.For("neri").armed=neri.ammo>0;
            // Neri's existing firearm and ammunition are retained, not granted on recruitment.
            neri.combat=neri.combat??new WeaponState();neri.combat.Initialize(neri.ammo,neri.combat.kind);
            crew.rell.combat.Initialize(0,2);
        }
        public bool CrewValid()=>crewVersion==0||crewVersion==1&&crew!=null&&crew.Valid();
    }
}
