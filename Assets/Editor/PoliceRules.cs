using System;
using System.IO;
using Funstra;
using UnityEngine;
public static class PoliceRules
{
    public static void Verify()
    {
        int count=0;Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception("Police rule: "+label);count++;};
        var p=new PoliceResponse();check(p.Valid()&&!p.Searching,"Fresh response is valid and inactive");
        p.Noise(new Vector3(8,0,-13));check(p.Searching&&!p.identifiedGunman&&p.harm==0,"Sound alone does not identify or escalate");
        p.Step(0);p.Step(float.NaN);check(p.searchRemaining==12,"Pause and invalid time preserve clocks");
        p.Step(12);check(!p.Searching,"Uncorroborated noise expires");
        p.Violence(Vector3.zero,1,"resident");check(p.identifiedGunman&&!p.trucks[0].requested,"First reported wound identifies without immediate truck");
        p.Violence(Vector3.zero,1,"officer");check(p.trucks[0].requested&&!p.trucks[1].requested,"Second harm point requests first finite crew");
        p.trucks[0].delay=1;p.Violence(Vector3.one,2,"officer");check(p.trucks[1].requested&&p.trucks[0].delay==1,"Escalation cannot restart existing dispatch timer");
        p.Noise(new Vector3(9,0,9));check(p.lastKnown==Vector3.one,"Noise cannot overwrite an identified sighting");
        p.Step(20);p.Sight(new Vector3(4,0,3));check(p.searchRemaining==45&&p.lastKnown==new Vector3(4,0,3),"Fresh observation renews search with observed position");
        var saved=JsonUtility.FromJson<PoliceResponse>(JsonUtility.ToJson(p));check(saved.Valid()&&saved.harm==4&&saved.trucks[1].requested&&saved.trucks[0].delay==1,"Dispatch survives JSON round trip");
        saved.Step(46);check(!saved.identifiedGunman&&!saved.Searching&&saved.harm==0,"Unobserved search ends without fabricated sightings");
        saved.Violence(Vector3.zero,1000,"officer");check(saved.harm==100&&saved.trucks.Length==2,"Escalation has finite bounds");
        var bad=new PoliceResponse();bad.trucks[0].deployed=1;check(!bad.Valid(),"Save rejects deployed person without physical arrival or roster");
        bad=new PoliceResponse();bad.trucks=null;check(!bad.Valid(),"Save rejects missing dispatch records");
        bad=new PoliceResponse();bad.searchRemaining=float.NaN;check(!bad.Valid(),"Save rejects nonfinite search time");
        Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/police-rules-result.txt","PASS: "+count+" police knowledge, dispatch and persistence checks\n");
        Debug.Log("FUNSTRA POLICE RULES PASSED: "+count);
    }
}
