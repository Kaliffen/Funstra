using System;
using System.IO;
using UnityEngine;
using Funstra;

public static class DistrictRules
{
    public static void Verify()
    {
        int checks=0;
        Action<bool,string> check=(ok,message)=>{if(!ok)throw new Exception("District rule failed: "+message);checks++;};
        Func<RunState,int> money=r=>r.cash+r.district.clinicMoney+r.district.supplierMoney+r.district.collectorMoney+r.district.buyerMoney+r.district.patientMoney+r.district.marketMoney;
        var idle=new RunState();int initialMoney=money(idle);
        idle.district.Tick(721);
        check(idle.district.shipmentOwner=="buyer"&&idle.district.buyerMoney==100&&idle.district.collectorMoney==100,"Buyer executes sale without a quest");
        check(idle.district.treatments>0&&idle.district.supplierStock<4,"Clinic treats patients and buys finite replenishment");
        check(idle.district.TotalMedicine==12&&money(idle)==initialMoney,"Autonomous baseline conserves medicine and money");
        var stepped=new RunState();for(int i=0;i<7210;i++)stepped.district.Tick(.1f);
        check(stepped.district.TotalMedicine==12&&stepped.district.shipmentOwner==idle.district.shipmentOwner&&stepped.district.treatments==idle.district.treatments,"Scheduled outcomes independent of update step");
        var paid=new RunState{cash=160};initialMoney=money(paid);
        check(paid.district.PayRelease(paid)&&paid.cash==60,"Release consumes exactly $100");
        check(!paid.district.PayRelease(paid)&&paid.cash==60,"Cannot pay for release twice");
        check(paid.district.TakeShipment(true)&&!paid.district.identified,"Paid collection does not identify a thief");
        check(!paid.district.TakeShipment(false),"Physical stock cannot duplicate");
        check(paid.district.Donate()&&paid.district.clinicStock==8&&paid.district.trust==3,"Donation moves six doses and earns trust");
        check(!paid.district.Donate()&&!paid.district.SellMedicine(paid),"Delivered stock cannot be sold or donated again");
        check(paid.district.Recruit()&&!paid.district.Recruit(),"Relationship enables one persistent recruitment");
        paid.district.health=40;paid.district.bleeding=true;
        check(paid.district.NeriAid()&&paid.district.health==62&&!paid.district.bleeding&&paid.district.neri.bandages==2,"Companion spends a finite dressing to stabilize player");
        paid.district.neri.health=0;
        check(!paid.district.NeriAid()&&paid.district.AidNeri()&&paid.district.neri.health==40,"Downed companion needs and receives player aid");
        check(paid.district.Treat(paid)&&paid.district.health==100&&paid.district.TotalMedicine==12,"Treatment consumes medicine and heals wounds");
        check(money(paid)==initialMoney,"Payment and trusted treatment conserve money");
        var theft=new RunState();check(theft.district.TakeShipment(false),"Unwitnessed theft obtains finite stock");theft.district.Tick(31);
        check(theft.district.discovered&&!theft.district.identified,"Stock audit discovers theft without inventing suspect knowledge");
        check(theft.district.SellMedicine(theft)&&theft.cash==160&&theft.district.clinicStock==2&&theft.district.marketStock==6,"Selling benefits player and market but not clinic");
        var violence=new RunState{cash=200};violence.district.TakeShipment(true);
        check(violence.district.identified&&violence.district.hostile,"Witnessed theft identifies player and creates hostility");
        violence.district.hostile=false;violence.district.Tick(40);
        check(violence.district.identified,"Ending pursuit does not erase identity");
        check(violence.district.Restitution(violence)&&violence.cash==140&&!violence.district.identified,"Restitution consumes $60 and withdraws standing hostility");
        violence.district.Defeat(violence,false);
        check(!violence.district.Carrying&&violence.district.shipmentOwner=="collector"&&violence.district.debt==40&&violence.district.health==45,"Solo defeat returns stock and leaves recoverable wounds and debt");
        check(violence.district.TotalMedicine==12,"Confiscation conserves medicine");
        var poor=new RunState();poor.district.Tick(2000);poor.district.health=20;poor.district.bleeding=true;
        check(poor.district.RestOnCredit()&&poor.district.health==80&&!poor.district.bleeding&&poor.district.debt==40,"Penniless solo recovery works after shipment sale and supply depletion");
        check(!poor.district.RestOnCredit(),"Healthy player cannot accumulate meaningless recovery debt");
        var late=new RunState{cash=200};late.district.Tick(721);
        check(late.district.PayRelease(late)&&late.cash==60&&late.district.TakeShipment(false),"Sold shipment remains obtainable at higher buyer price");
        var rescue=new RunState();rescue.district.Defeat(rescue,true);check(rescue.district.health==60&&rescue.district.debt==0,"Companion rescue has tangible recovery advantage");
        string path=Path.Combine(Path.GetTempPath(),"funstra-district-"+Guid.NewGuid()+".json");
        try
        {
            paid.district.playerPosition=new Vector3(-28,0,4);paid.district.hasPosition=true;paid.district.identified=true;
            check(paid.Save(path),"World checkpoint writes");var loaded=RunState.Load(path);
            check(loaded!=null&&loaded.version==2&&loaded.district.recruited&&loaded.district.identified&&loaded.district.TotalMedicine==12&&loaded.district.playerPosition==paid.district.playerPosition,"World state including stock, companion, position and identity round trips");
            File.WriteAllText(path,"{\"version\":1,\"cash\":340,\"completed\":2,\"perks\":6,\"satchel\":true,\"cargoRuns\":2,\"cargoEarnings\":170}");
            loaded=RunState.Load(path);check(loaded!=null&&loaded.version==2&&loaded.cash==340&&loaded.satchel&&loaded.perks==6&&loaded.district.TotalMedicine==12,"Legacy save migrates without losing campaign or cargo progress");
            check(loaded.Save(path)&&File.Exists(path+".v1-backup"),"First migrated save preserves original version one file");
            loaded.district.shipmentUnits=99;loaded.Save(path);check(RunState.Load(path)==null,"Invalid world stock rejected");
        }
        finally { if(File.Exists(path))File.Delete(path);if(File.Exists(path+".v1-backup"))File.Delete(path+".v1-backup"); }
        Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/district-rules-result.txt","PASS: "+checks+" district simulation, conservation, recovery and migration assertions\n");
        Debug.Log("DISTRICT RULES PASSED: "+checks);
    }
}
