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
        check(d.clinicStock==2&&d.refusedPatients>0&&d.NeriWords.Contains("Edda")&&d.ClinicStatus.Contains("PAUSED"),"Reserved last doses turn away patients and change Neri dialogue");
        int treated=d.treatments;d.SetClinicPolicy(false);d.Tick(90);
        check(d.treatments>treated&&d.TotalMedicine==12&&money(run)==total,"Reopening actually treats patients without creating resources");
        var boundary=new RunState();var shelf=boundary.district;
        shelf.refuge=true;shelf.trust=3;shelf.clinicStock=3;shelf.supplierStock=3;
        int boundaryMoney=money(boundary),clinicMoney=shelf.clinicMoney,patientMoney=shelf.patientMoney;
        check(shelf.SetClinicPolicy(true)&&shelf.Valid(),"Reserve boundary starts from a valid three-dose shelf");
        shelf.Tick(89);
        check(shelf.clinicStock==3&&shelf.treatments==0&&shelf.refusedPatients==0,"Reserve policy does not treat or refuse before a patient arrives");
        shelf.Tick(1);
        check(shelf.clinicStock==2&&shelf.treatments==1&&shelf.consumed==1&&shelf.refusedPatients==0,
            "Reserve last two still treats a public patient when three doses remain");
        check(shelf.clinicMoney==clinicMoney+8&&shelf.patientMoney==patientMoney-8&&money(boundary)==boundaryMoney&&shelf.TotalMedicine==12,
            "Above-reserve treatment transfers exactly eight dollars and consumes one existing dose");
        shelf.Tick(90);
        check(shelf.clinicStock==2&&shelf.treatments==1&&shelf.consumed==1&&shelf.refusedPatients==1,
            "Next patient is refused exactly at the two-dose reserve boundary");
        check(shelf.clinicMoney==clinicMoney+8&&shelf.patientMoney==patientMoney-8&&money(boundary)==boundaryMoney&&shelf.TotalMedicine==12&&shelf.Valid(),
            "Reserve refusal neither charges the patient nor consumes medicine");
        shelf.SetClinicPolicy(false);shelf.Tick(90);
        check(shelf.clinicStock==1&&shelf.treatments==2&&shelf.consumed==2&&shelf.refusedPatients==1&&shelf.clinicMoney==clinicMoney+16&&shelf.patientMoney==patientMoney-16&&money(boundary)==boundaryMoney&&shelf.TotalMedicine==12&&shelf.Valid(),
            "Reopening the same two-dose shelf treats the next patient without another refusal");
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
        var crate=new CityNavigation();crate.Props.Add(new Bounds(new Vector3(-28,.6f,4),new Vector3(.9f,1.1f,.9f)));crate.Bake();
        check(crate.ClearWalk(new Vector3(-28,0,2),new Vector3(-28,0,3)),"Route may end beside a crate at the legal clearance boundary");
        var vehicle=new CityNavigation();vehicle.Traffic.Add(new Bounds(new Vector3(0,.8f,0),new Vector3(2,1.6f,4)));
        check(!vehicle.Sight(new Vector3(-4,0,0),new Vector3(4,0,0)),"Vehicle footprint blocks sight and gunfire");vehicle.Traffic.Clear();
        check(vehicle.Sight(new Vector3(-4,0,0),new Vector3(4,0,0)),"Departed vehicle no longer blocks sight");
        string json=JsonUtility.ToJson(run);var loaded=JsonUtility.FromJson<RunState>(json);
        check(loaded.district.refuge&&loaded.district.refusedPatients==d.refusedPatients&&loaded.district.Valid(),"Refuge policy and consequences survive serialization");
        var legacy=JsonUtility.FromJson<DistrictState>("{\"clock\":1}");check(!legacy.refuge&&!legacy.reserveMedicine,"Old saves start without unearned refuge or restrictions");
        Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/refuge-rules-result.txt","PASS: "+checks+" refuge, dialogue, schedule and prop route assertions\n");
    }
}
