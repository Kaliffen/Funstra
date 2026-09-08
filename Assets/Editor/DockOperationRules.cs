using System;
using UnityEngine;
using Funstra;

public static class DockOperationRules
{
    public static void Verify()
    {
        int checks=0;
        Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception("Dock operation rule failed: "+message);checks++;};
        var paid=new RunState{cash=120};paid.district.InitializeDockState();var d=paid.district.dock;
        check(!d.TryPayRelease(paid,false)&&paid.cash==120,"Down Vale cannot sell papers");
        check(d.TryPayRelease(paid,true)&&paid.cash==30&&d.valeMoney==90,"Release transfers actual $90 to finite Vale balance");
        check(!d.TryPayRelease(paid,true)&&paid.cash+d.valeMoney==120,"No repeated payment or money creation");
        check(d.TryTake("neri",true,"public quay")&&!d.yardHostile&&d.acquisition=="paid release","Paid collection remains peaceful and belongs to actual collector");
        check(!d.TryTake("player",false,"service gate"),"No second copy after collection");
        check(!d.TryReturn("player",true)&&!d.TryReturn("neri",false),"Wrong carrier and down Rell cannot deliver");
        check(d.TryTransfer("neri","player")&&!d.TryTransfer("neri","rell"),"Component moves once from its actual custodian");
        check(d.TryReturn("player",true)&&d.componentOwner=="rell-workshop"&&d.WorkshopDrained,"Return installs the same component and drains workshop");
        check(!d.TryReturn("player",true)&&!d.TryTransfer("rell-workshop","player")&&d.Valid(),"Installed component cannot duplicate");
        var sneaked=new DockOperationState();
        check(sneaked.TryTake("player",false,"service gate")&&sneaked.approach=="service gate"&&!sneaked.theftWitnessed&&!sneaked.yardHostile,"Unseen service approach does not invent witness knowledge");
        var seen=new DockOperationState();
        check(seen.TryTake("rell",true,"public quay")&&seen.theftWitnessed&&seen.yardHostile&&seen.acquisition=="theft","Witnessed theft identifies a theft without inventing a gunfight");
        check(!seen.TryPayRelease(paid,true),"Cannot buy back safe passage after witnessed theft");
        var repair=new DockOperationState();
        check(!repair.WorkRepair("player",12)&&repair.BeginRepair(),"Repair requires accepting the concrete work");
        check(!repair.WorkRepair("player",6)&&repair.gasketStock==1,"Partial repair neither consumes nor creates resources");
        check(!repair.WorkRepair("neri",6)&&repair.repairWorker=="neri"&&repair.repairProgress==6,"Control change cannot combine unrelated worker progress");
        repair.InterruptRepair();
        check(repair.repairProgress==0&&!repair.WorkRepair("neri",11.9f),"Interrupted repair requires sustained work");
        check(repair.WorkRepair("neri",.2f)&&repair.gasketStock==0&&repair.gasketsUsed==1&&repair.WorkshopDrained,"Completed repair uses sole gasket and restores pump");
        check(!repair.WorkRepair("neri",12)&&!repair.BeginRepair()&&repair.Valid(),"Repair is consequential and cannot be farmed");
        repair.repairPracticeAwarded=true;
        var loaded=JsonUtility.FromJson<DockOperationState>(JsonUtility.ToJson(repair));
        check(loaded.Valid()&&loaded.repairPracticeAwarded&&loaded.auxiliaryRepaired&&loaded.componentOwner=="yard","Repair and finite unused main component retain distinct persisted outcomes");
        var carrier=JsonUtility.FromJson<DockOperationState>(JsonUtility.ToJson(sneaked));
        check(carrier.Valid()&&carrier.componentOwner=="player"&&carrier.approach=="service gate","Reload retains actor custody and the actual approach");
        carrier.componentOwner="copy";check(!carrier.Valid(),"Unknown component custody rejected");
        loaded.gasketStock=1;check(!loaded.Valid(),"Duplicated consumed gasket rejected");
        var fresh=new DistrictState();fresh.InitializeDockState();fresh.dock.TryTake("player",false,"public quay");fresh.InitializeDockState();
        check(fresh.DockValidation()&&fresh.dock.componentOwner=="player","Migration is idempotent and does not respawn collected component");
        Debug.Log("DOCK_OPERATION_RULES_PASS checks="+checks);
    }
}
