using System;
using UnityEngine;

namespace Funstra
{
    [Serializable] public sealed class DockOperationState
    {
        public const int ReleasePrice=90;
        public const float RepairSeconds=12;
        public static readonly Vector3 Workshop=new Vector3(61,0,20);
        public static readonly Vector3 ComponentPost=new Vector3(30,0,43);
        public static readonly Vector3 PublicCounter=new Vector3(16,0,39);
        public static readonly Vector3 RepairPost=new Vector3(65,0,20);
        // One component retains custody even when its carrier is down or abandoned.
        public string componentOwner="yard";
        public bool metRell, released, yardHostile, yardAttacked, theftWitnessed, stakeResolved;
        public bool repairAccepted, auxiliaryRepaired, repairPracticeAwarded;
        public int valeMoney, gasketStock=1, gasketsUsed;
        public float repairProgress;
        public string repairWorker="";
        public string acquisition="", approach="";
        public bool WorkshopDrained=>auxiliaryRepaired||stakeResolved;
        public static bool IsCarrier(string id)=>id=="player"||id=="neri"||id=="rell";
        public bool TryPayRelease(RunState run,bool liveVale)
        {
            if(run==null||!liveVale||released||yardHostile||componentOwner!="yard"||run.cash<ReleasePrice)return false;
            run.cash-=ReleasePrice;valeMoney+=ReleasePrice;released=true;return true;
        }
        public bool TryTake(string actorId,bool witnessed,string route)
        {
            if(componentOwner!="yard"||!IsCarrier(actorId))return false;
            componentOwner=actorId;acquisition=released?"paid release":yardAttacked?"fight":"theft";
            approach=route=="service gate"?route:"public quay";
            if(witnessed&&!released){theftWitnessed=true;yardHostile=true;}
            return true;
        }
        public bool TryTransfer(string from,string to)
        {
            if(componentOwner!=from||from==to||!IsCarrier(from)||!IsCarrier(to))return false;
            componentOwner=to;return true;
        }
        public bool TryReturn(string actorId,bool liveRell)
        {
            if(stakeResolved||!liveRell||!IsCarrier(actorId)||componentOwner!=actorId)return false;
            componentOwner="rell-workshop";stakeResolved=true;return true;
        }
        public bool BeginRepair()
        {
            if(repairAccepted||auxiliaryRepaired||gasketStock<1)return false;
            repairAccepted=true;return true;
        }
        public void InterruptRepair(){repairProgress=0;repairWorker="";}
        public bool WorkRepair(string actorId,float dt)
        {
            if(!repairAccepted||auxiliaryRepaired||gasketStock<1||!IsCarrier(actorId)||!ProjectileMath.Finite(dt)||dt<=0)return false;
            if(repairWorker!=actorId){repairProgress=0;repairWorker=actorId;}
            repairProgress=Mathf.Min(RepairSeconds,repairProgress+dt);
            if(repairProgress<RepairSeconds)return false;
            gasketStock--;gasketsUsed++;auxiliaryRepaired=true;repairProgress=0;repairWorker="";return true;
        }
        public bool Valid()
        {
            return (componentOwner=="yard"||componentOwner=="rell-workshop"||IsCarrier(componentOwner))&&
                stakeResolved==(componentOwner=="rell-workshop")&&valeMoney>=0&&valeMoney==(released?ReleasePrice:0)&&
                gasketStock>=0&&gasketsUsed>=0&&gasketStock+gasketsUsed==1&&auxiliaryRepaired==(gasketsUsed==1)&&
                (!repairPracticeAwarded||auxiliaryRepaired)&&(!auxiliaryRepaired||repairAccepted)&&
                ProjectileMath.Finite(repairProgress)&&repairProgress>=0&&repairProgress<RepairSeconds&&
                (repairWorker==""||IsCarrier(repairWorker))&&(repairProgress==0||repairAccepted&&!auxiliaryRepaired&&IsCarrier(repairWorker));
        }
    }

    public sealed partial class DistrictState
    {
        public int dockVersion;
        public DockOperationState dock=new DockOperationState();
        public void InitializeDockState()
        {
            if(dockVersion==0){dock=new DockOperationState();dockVersion=1;}
        }
        public bool DockValidation()=>dockVersion==0||dockVersion==1&&dock!=null&&dock.Valid();
    }
}
