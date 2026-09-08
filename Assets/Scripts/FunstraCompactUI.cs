using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        bool compactHUD=true;
        void DrawCompactHUD()
        {
            Panel(24,24,310,94);
            Text((CrewEnabled?ControlledCrewId.ToUpper():"FUNSTRA")+" / "+(DistrictEnabled?PortClock:"OLD PORT"),43,37,272,29,20,paper,FontStyle.Bold);
            float health=CrewEnabled?CrewHealth(ControlledCrewId):District.health;bool bleeding=CrewEnabled?CrewBleeding(ControlledCrewId):District.bleeding;
            Text(DistrictEnabled?Mathf.CeilToInt(health)+" HEALTH / "+(bleeding?"BLEEDING":"STABLE"):"A NIGHT IN OLD PORT",43,72,272,28,16,bleeding?CityArt.Red:CityArt.Mint,FontStyle.Bold);
            Panel(1250,24,326,94);
            Text("$"+State.cash+" / MARA: "+(State.completed==0?"NEW FACE":State.completed==1?"RUNNER":State.completed==2?"TRUSTED":"CONNECTED"),1267,38,293,30,16,paper,FontStyle.Bold);
            Text(Police.Searching?PoliceStatus:Heat>0?"WANTED / "+(Elapsed-lastSight<.5f?"IN SIGHT":"SEARCHING"):Hidden?"HIDDEN":"OLD PORT / KEEP A LOW PROFILE",1267,75,293,25,14,Heat>0?CityArt.Red:quiet);
            Panel(24,738,365,135);
            Text(DistrictEnabled&&trackDistrict?"YOUR NEXT MOVE":State.Finished?"MARA / DEBT SETTLED":"MARA / JOB "+(State.completed+1),43,753,327,25,14,CityArt.Mint,FontStyle.Bold);
            string objective=DistrictEnabled&&trackDistrict?MedicalObjective:State.Finished?"Find a livelihood in Old Port.":State.carrying?"Bring the goods back to Mara.":State.accepted?Jobs.All[State.completed].title:"Visit Mara at the pawn shop.";
            if(ArmsEnabled&&trackDistrict)objective=ArmsObjective;
            if(DockEnabled&&trackDistrict)objective=DockObjective;
            Text(objective,43,784,327,66,16,quiet);
            if(showMap)DrawMap(new Rect(420,280,760,540),true);
            if(!showMap)DrawWorldMarkers();
            if(toastTime>0){Panel(420,210,760,57);Text(toast,440,220,720,42,16,paper,FontStyle.Normal,TextAnchor.MiddleCenter);}
            if(!string.IsNullOrEmpty(prompt)&&Active&&!showMap)
            {
                Panel(430,760,740,63);Text(prompt,447,772,706,42,18,paper,FontStyle.Bold,TextAnchor.MiddleCenter);
                float progress=Mathf.Max(Mathf.Max(theftProgress,cargoProgress),Mathf.Max(extractionProgress,medicineProgress));
                if(progress>0)Rect(430,820,740*progress,3,CityArt.Mint);
            }
            Text("WASD move / SHIFT sprint / E interact / SPACE pause / TAB map / F2 details",405,848,810,30,14,paper,FontStyle.Normal,TextAnchor.MiddleCenter);
            if(arrestProgress>0){Text("MOVE / ARREST IMMINENT",550,654,500,40,24,CityArt.Red,FontStyle.Bold,TextAnchor.MiddleCenter);Rect(650,698,300*arrestProgress,5,CityArt.Red);}
        }
    }
}
