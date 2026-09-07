using System;
using System.IO;
using Funstra;
using UnityEngine;

public static class RefugeRules
{
    public static void Verify()
    {
        int checks=0;Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception(message);checks++;};
        Func<RunState,int> money=r=>r.cash+r.district.clinicMoney+r.district.supplierMoney+r.district.marketMoney+r.district.patientMoney+r.district.buyerMoney+r.district.collectorMoney;
        var run=new RunState{cash=240};var d=run.district;int total=money(run);
        check(!d.OpenRefuge(run),"A refuge cannot buy a relationship");d.TakeShipment(false);d.Donate();d.Recruit();
        check(d.OpenRefuge(run)&&run.cash==120&&d.clinicMoney==80&&d.refuge,"Refuge payment repairs room and funds clinic");
        check(!d.OpenRefuge(run)&&money(run)==total,"No duplicate purchase or lost money");
        check(d.FundClinic(run)&&d.BuyClinicReserve()&&d.supplierStock==2&&d.TotalMedicine==12&&money(run)==total,"Contributions and supply conserve cash and stock");
        d.health=35;d.bleeding=true;check(!d.RefugeRest(),"Refuge cannot substitute for stopping bleeding");d.BandagePlayer();
        check(d.RefugeRest()&&d.health==80&&d.debt==0&&!d.RefugeRest(),"Shared shelter removes debt cost, not serious wounds");
        d.SetClinicPolicy(true);d.Tick(900);
        check(d.clinicStock==2&&d.refusedPatients>0&&d.NeriWords.Contains("Edda"),"Reserved last doses turn away patients and change Neri dialogue");
        int treated=d.treatments;d.SetClinicPolicy(false);d.Tick(90);
        check(d.treatments>treated&&d.TotalMedicine==12&&money(run)==total,"Reopening actually treats patients without creating resources");
        d.trust=-3;check(!d.RefugeRest()&&!d.FundClinic(run)&&!d.SetClinicPolicy(true),"Betrayal removes shared refuge privileges");
        var paid=new RunState{cash=100};paid.district.PayRelease(paid);paid.district.TakeShipment(false);paid.district.Donate();
        check(paid.district.NeriWords.Contains("paid a debt")&&paid.district.IvoWords.Contains("Neri got"),"Paid donation has distinct dialogue");
        var violent=new DistrictState();violent.TakeShipment(true);violent.guard.health=0;
        check(violent.IvoWords.Contains("daughter")&&violent.IvoTitle=="I REMEMBER YOU","Violence changes collector response");
        var unseen=new DistrictState();unseen.TakeShipment(false);unseen.Tick(31);
        check(unseen.IvoTitle=="SIX EMPTY SPACES"&&!unseen.identified,"Missing stock dialogue does not invent suspect identity");
        var clock=new DistrictState();clock.Tick(721);
        check(clock.incidents.Find(e=>e.kind=="sale").time==720&&clock.incidents.Find(e=>e.kind=="shortage").time==180,"Scheduled incidents retain event time under coarse advancement");
        var nav=new CityNavigation();nav.Obstacles.Add(new Bounds(new Vector3(1,0,1),new Vector3(.8f,1.3f,.8f)));nav.Bake();
        var path=nav.Find(new Vector3(-4,0,0),new Vector3(4,0,2));
        check(path.Count>0,"Navigation finds route around small prop between grid nodes");
        for(int i=1;i<path.Count;i++)check(nav.ClearWalk(path[i-1],path[i]),"Every route edge clears small prop");
        check(nav.Walkable(nav.SafePoint(new Vector3(1,0,1))),"Old embedded position relocates outside prop");
        var corner=new CityNavigation();corner.Obstacles.Add(new Bounds(new Vector3(-6.7f,2,-32),new Vector3(.2f,4.2f,.2f)));corner.Bake();
        check(!corner.ClearWalk(new Vector3(-6.08f,0,-31.21f),new Vector3(-6,0,-32)),"Exact segment rejects lamppost corner missed by sampled clearance");
        string json=JsonUtility.ToJson(run);var loaded=JsonUtility.FromJson<RunState>(json);
        check(loaded.district.refuge&&loaded.district.refusedPatients==d.refusedPatients&&loaded.district.Valid(),"Refuge policy and consequences survive serialization");
        var legacy=JsonUtility.FromJson<DistrictState>("{\"clock\":1}");check(!legacy.refuge&&!legacy.reserveMedicine,"Old saves start without unearned refuge or restrictions");
        Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/refuge-rules-result.txt","PASS: "+checks+" refuge, dialogue, schedule and prop route assertions\n");
    }
}
