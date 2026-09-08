using System;
using UnityEngine;

namespace Funstra
{
    [Serializable] public sealed class ResidentPersona
    {
        public int version, disposition, routineStop, dressingsUsed;
        public string occupation="", buddyId="", action="On the way", reason="", memory="", casualtyId="", offenderId="";
        public bool witnessedPlayer, helpedByPlayer, abandonedNeighbor;
        public float dangerRemaining, pauseRemaining, watchRemaining;
        public Vector3 threatPosition, casualtyPosition, refuge;
        public bool hasRefuge;
        public string Disposition => disposition==0?"careful helper":disposition==1?"loyal neighbor":disposition==2?"watchful":"self-preserving";
        public bool WillHelp(string id) => disposition==0||disposition==1&&id==buddyId;
        public void Hear(Vector3 source)
        {
            threatPosition=source;dangerRemaining=12;hasRefuge=false;
            action="Seeking shelter";reason="Heard a nearby shot; the shooter is unknown.";
        }
        public void Witness(DistrictActor casualty,Vector3 source,string offender)
        {
            threatPosition=source;dangerRemaining=12;hasRefuge=false;offenderId=offender;watchRemaining=disposition==2?6:0;
            if(offender=="player")witnessedPlayer=true;
            if(casualty!=null)
            {
                casualtyId=casualty.id;casualtyPosition=casualty.position;
                memory="Saw "+(offender=="player"?"you":offender)+" hurt "+casualty.name+".";
                if(!WillHelp(casualty.id))
                {abandonedNeighbor=true;memory+=" I left "+casualty.name+" behind to reach safety.";}
            }
            action="Leaving the violence";reason="Personally saw the attacker. Staying alive comes first.";
        }
        public void Step(float dt)
        {
            if(!ProjectileMath.Finite(dt)||dt<=0)return;
            if(dangerRemaining<=0)watchRemaining=Mathf.Max(0,watchRemaining-dt);
            dangerRemaining=Mathf.Max(0,dangerRemaining-dt);pauseRemaining=Mathf.Max(0,pauseRemaining-dt);
        }
        public void RememberHelp(string helper)
        {
            if(helper=="player")helpedByPlayer=true;
            memory=(helper=="player"?"You":helper)+" used a dressing to help me. I remember who came back.";
        }
        public bool TryAid(DistrictActor helper,DistrictActor patient,bool safe,bool reached)
        {
            if(!safe||!reached||dangerRemaining>0||helper==null||helper.health<=0||helper.bandages<=0||patient==null||patient.health<=0||
               patient.id!=casualtyId||!WillHelp(patient.id)||!patient.bleeding&&patient.health>=75)return false;
            helper.bandages--;dressingsUsed++;patient.bleeding=false;patient.health=Mathf.Min(100,patient.health+12);
            action="Checking on "+patient.name;reason="Used my own last dressing after the shooting stopped.";
            memory="I came back for "+patient.name+" and used a dressing.";casualtyId="";
            patient.resident?.RememberHelp(helper.name);return true;
        }
        public bool Valid() => version==1&&disposition>=0&&disposition<=3&&routineStop>=0&&routineStop<5&&dressingsUsed>=0&&dressingsUsed<=1&&
            ProjectileMath.Finite(dangerRemaining)&&dangerRemaining>=0&&dangerRemaining<=12&&ProjectileMath.Finite(pauseRemaining)&&pauseRemaining>=0&&pauseRemaining<=30&&
            ProjectileMath.Finite(watchRemaining)&&watchRemaining>=0&&watchRemaining<=6&&
            ProjectileMath.Finite(threatPosition)&&ProjectileMath.Finite(casualtyPosition)&&ProjectileMath.Finite(refuge)&&occupation!=null&&buddyId!=null&&action!=null&&reason!=null&&memory!=null&&casualtyId!=null&&offenderId!=null;
    }
    public static class ResidentCatalog
    {
        static readonly string[] Names={"MILA","ELIAS","RUTH","JONAS","ADA","BRUNO","LENA","OMAR"};
        static readonly string[] Jobs={"net mender","dock porter","night cleaner","fish seller","retired nurse","warehouse clerk","baker","cab dispatcher"};
        static readonly int[] Dispositions={1,3,0,2,0,3,1,2};
        public static void Attach(DistrictActor actor,int index)
        {
            if(index<0||index>=Names.Length||actor.resident!=null&&actor.resident.version==1)return;
            actor.name=Names[index];actor.ammo=0;actor.weaponRecoverable=false;
            actor.resident=new ResidentPersona{version=1,occupation=Jobs[index],disposition=Dispositions[index],buddyId="citizen-"+(3+(index^1)),pauseRemaining=2+index*.4f};
            actor.bandages=Dispositions[index]<=1?1:0;
            actor.resident.action="Starting the evening round";actor.resident.reason="A "+Jobs[index]+" has places to be before closing.";
        }
        public static string RoutineReason(ResidentPersona p)
        {
            if(p.routineStop==0)return p.occupation=="baker"?"Checking the flour order before tomorrow's bake.":p.occupation=="dock porter"?"Checking tomorrow's dock shift on the noticeboard.":"Checking the evening noticeboard before work.";
            if(p.routineStop==1)return "Taking a short break from the "+p.occupation+" shift.";
            if(p.routineStop==2)return "Looking along the loading street for a familiar neighbor.";
            if(p.routineStop==3)return "Heading toward home after the evening round.";
            return "Waiting at the corner before returning to work.";
        }
    }
    public sealed partial class DistrictState
    {
        public bool ResidentsValidation()
        {
            if(citizens==null)return true;
            foreach(var citizen in citizens)if(citizen==null||citizen.resident!=null&&citizen.resident.version!=0&&!citizen.resident.Valid())return false;
            return true;
        }
    }
}
