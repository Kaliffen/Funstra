using System;
using System.IO;
using Funstra;
using UnityEngine;

public static class ResidentRules
{
    static DistrictActor Person(int index)
    {var a=new DistrictActor("citizen-"+(index+3),"old resident",Vector3.zero);ResidentCatalog.Attach(a,index);return a;}
    public static void Verify()
    {
        int count=0;Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception("Resident rule: "+label);count++;};
        var names=new System.Collections.Generic.HashSet<string>();int dressings=0;
        for(int i=0;i<8;i++)
        {
            var actor=Person(i);names.Add(actor.name);dressings+=actor.bandages;
            check(actor.resident.Valid()&&actor.ammo==0&&!actor.weaponRecoverable,"Named civilian profile is valid and unarmed "+i);
        }
        check(names.Count==8&&dressings==4,"Eight distinct residents share exactly four finite personal dressings");
        var helper=Person(2);var patient=Person(3);patient.health=40;patient.bleeding=true;
        var p=helper.resident;
        p.Hear(new Vector3(2,0,4));check(p.dangerRemaining==12&&!p.witnessedPlayer&&p.offenderId==""&&p.casualtyId=="","A heard shot creates danger but no invented attacker or casualty");
        p.Step(float.NaN);p.Step(-1);check(p.dangerRemaining==12,"Invalid time cannot expire remembered danger");
        p.Witness(patient,new Vector3(3,0,4),"player");
        check(p.witnessedPlayer&&p.casualtyId==patient.id&&p.WillHelp(patient.id)&&!p.abandonedNeighbor,"Helpful resident remembers the specific personally witnessed victim and attacker");
        check(!p.TryAid(helper,patient,true,true)&&helper.bandages==1&&patient.health==40,"Helper waits until perceived danger has subsided");
        p.Step(12);check(!p.TryAid(helper,patient,true,false)&&!p.TryAid(helper,patient,false,true),"Aid requires a physically reached casualty and current safety");
        check(p.TryAid(helper,patient,true,true)&&helper.bandages==0&&p.dressingsUsed==1&&patient.health==52&&!patient.bleeding,"One real personal dressing stabilizes a living neighbor for twelve health");
        check(patient.resident.memory.Contains(helper.name),"Recipient remembers the named resident who came back");
        patient.health=35;patient.bleeding=true;p.casualtyId=patient.id;
        check(!p.TryAid(helper,patient,true,true)&&p.dressingsUsed==1&&patient.health==35,"Repeated injury cannot regenerate the helper's dressing");
        var recovered=JsonUtility.FromJson<DistrictActor>(JsonUtility.ToJson(helper));ResidentCatalog.Attach(recovered,2);
        check(recovered.bandages==0&&recovered.resident.dressingsUsed==1&&recovered.resident.witnessedPlayer,"Reload and catalog attachment preserve spent inventory and personal memory");
        helper=Person(4);patient.health=0;helper.resident.Witness(patient,Vector3.zero,"player");helper.resident.Step(12);
        check(!helper.resident.TryAid(helper,patient,true,true)&&helper.bandages==1&&patient.health==0,"Personal dressing never resurrects an incapacitated actor");
        var loyal=Person(0);var buddy=Person(1);buddy.health=45;
        loyal.resident.Witness(buddy,Vector3.zero,"player");check(loyal.resident.WillHelp(buddy.id)&&!loyal.resident.WillHelp(patient.id),"Loyal resident prioritizes their specific paired neighbor");
        var selfish=Person(5);selfish.resident.Witness(buddy,Vector3.zero,"player");
        check(!selfish.resident.WillHelp(buddy.id)&&selfish.resident.abandonedNeighbor&&selfish.resident.memory.Contains(buddy.name),"Self-preserving resident remembers the specific neighbor left behind");
        var watcher=Person(7);watcher.resident.Witness(buddy,new Vector3(6,0,7),"officer-1");watcher.resident.Step(12);
        check(!watcher.resident.witnessedPlayer&&watcher.resident.offenderId=="officer-1"&&watcher.resident.watchRemaining==6,"Watchful resident retains the actual offender and watches only the remembered scene after safety");
        watcher.resident.Step(3);check(watcher.resident.watchRemaining==3,"Watching is bounded after danger, not endless global tracking");
        patient=Person(1);patient.resident.RememberHelp("player");
        var saved=JsonUtility.FromJson<DistrictActor>(JsonUtility.ToJson(patient));
        check(saved.resident.helpedByPlayer&&saved.resident.memory.Contains("dressing"),"Help from the player persists as a specific reason to approach");
        p=Person(2).resident;p.routineStop=3;p.pauseRemaining=4;p.Hear(new Vector3(9,0,8));p.hasRefuge=true;p.refuge=new Vector3(-4,0,1);
        var savedPersona=JsonUtility.FromJson<ResidentPersona>(JsonUtility.ToJson(p));
        check(savedPersona.Valid()&&savedPersona.routineStop==3&&savedPersona.pauseRemaining==4&&savedPersona.dangerRemaining==12&&savedPersona.refuge==p.refuge&&savedPersona.hasRefuge,"Routine progress, pause, observed danger and selected refuge survive reload");
        var bad=Person(0).resident;bad.dangerRemaining=float.NaN;check(!bad.Valid(),"Save rejects nonfinite personal danger time");
        bad=Person(0).resident;bad.dressingsUsed=2;check(!bad.Valid(),"Save rejects more personal dressings used than the initial finite supply");
        var district=new DistrictState();district.citizens.Add(helper);check(district.ResidentsValidation(),"District accepts persistent valid resident profiles");helper.resident.version=99;check(!district.ResidentsValidation(),"District rejects unsupported resident profile schema");
        Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/resident-rules-result.txt","PASS: "+count+" resident identity, perception, finite aid and persistence checks\n");
        Debug.Log("FUNSTRA RESIDENT RULES PASSED: "+count);
    }
}
