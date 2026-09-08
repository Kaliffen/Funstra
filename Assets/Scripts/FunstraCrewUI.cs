using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        string crewMoveTarget="";
        bool CrewHudBlocksPointer(Vector2 point)
        {
            if(!CrewEnabled)return false;
            return crewPanel||crewMoveTarget!=""||new Rect(24,128,310,139).Contains(point)||new Rect(1250,128,326,44).Contains(point);
        }
        bool CrewPointerBlocked(Vector2 screenPoint)
        {return CrewHudBlocksPointer(new Vector2(screenPoint.x/Screen.width*W,(1-screenPoint.y/Screen.height)*H));}
        void DrawCrewUI()
        {
            if(!CrewEnabled||District.crew==null||screen!=ScreenMode.Play&&screen!=ScreenMode.Tactics)return;
            Panel(24,128,310,139);
            int row=0;
            foreach(string id in new[]{"player","neri","rell"})
            {
                string key=id=="player"?"F1":id=="neri"?"F3":"F4";
                string status=IsCrewId(id)?Mathf.CeilToInt(CrewHealth(id))+" HP":"not recruited";
                if(Button(key+" "+id.ToUpper()+" / "+status,32,136+row*39,294,35,ControlledCrewId==id))SelectCrew(id);
                row++;
            }
            if(Button("K / CREW & RESCUE",1250,128,326,44))ToggleCrewPanel();
            if(ControlledCrewId!="player")
            {
                var actor=CrewActor(ControlledCrewId);bool armed=CrewGunDrawn(ControlledCrewId);
                Panel(605,24,410,84);
                Text(ControlledCrewId.ToUpper()+" / "+(armed?WeaponSpec.For(actor.combat.kind).name:"FISTS"),623,35,376,29,18,CityArt.Amber,FontStyle.Bold);
                Text(armed?actor.combat.magazine+" / "+(actor.ammo-actor.combat.magazine)+" reserve  / R reload":"2 draw carried gun / B field dressing",623,67,376,25,14,paper);
            }
            var action=District.crew.For(ControlledCrewId);
            if(action.aidRemaining>0)Text("STABILIZING "+action.patient.ToUpper()+" / "+action.aidRemaining.ToString("0.0")+"s",530,699,630,32,20,CityArt.Mint);
            if(action.carrying!="")Text("CARRYING "+action.carrying.ToUpper()+" / K to drop or admit at clinic",470,707,780,35,18,CityArt.Amber);
            if(crewMoveTarget!="")Text("CLICK A WALKABLE DESTINATION FOR "+crewMoveTarget.ToUpper(),430,320,800,40,22,CityArt.Mint);
            if(!crewPanel)return;
            Panel(370,280,860,540);
            Text("CREW / TIME IS STOPPED",395,300,800,40,24,CityArt.Mint,FontStyle.Bold);
            Text("Control stays with "+ControlledCrewId.ToUpper()+". Select a partner below to address them.",395,342,800,35,16,quiet);
            int x=0;foreach(string id in new[]{"player","neri","rell"})
            {if(Button(id.ToUpper(),395+x*265,386,245,43,crewTarget==id))crewTarget=id;x++;}
            var target=District.crew.For(crewTarget);
            Text(IsCrewId(crewTarget)?CrewHealth(crewTarget).ToString("0")+" health / "+CrewBandages(crewTarget)+" dressings / "+target.order:"Not part of the crew yet. Help them with their work.",395,440,800,32,18,paper);
            if(Button("FOLLOW",395,482,180,42))OrderCrew(crewTarget,"Follow");
            if(Button("HOLD",591,482,180,42))OrderCrew(crewTarget,"Hold");
            if(Button("MOVE TO...",787,482,180,42)){crewMoveTarget=crewTarget;crewPanel=false;}
            if(Button("RETREAT",983,482,222,42))OrderCrew(crewTarget,"Retreat");
            if(Button("COVER / ENGAGE",395,536,250,42))OrderCrew(crewTarget,"Engage");
            if(Button("STABILIZE / 3s",661,536,260,42)){BeginCrewAid(ControlledCrewId,crewTarget);crewPanel=false;screen=ScreenMode.Play;}
            if(Button("CARRY DOWNED",937,536,268,42))CarryCrew(ControlledCrewId,crewTarget);
            if(Button("GIVE DRESSING",395,590,250,42))TransferCrewBandage(ControlledCrewId,crewTarget);
            if(Button("GIVE HELD GUN + AMMO",661,590,330,42))TransferCrewGun(ControlledCrewId,crewTarget);
            if(Button("DROP",1007,590,198,42))DropCrew(ControlledCrewId);
            if(Button("ADMIT AT CLINIC / 1 DOSE",395,644,400,42))ReturnCrew(ControlledCrewId);
            if(Button("LEAVE THEM BEHIND",811,644,394,42))AbandonCrew(crewTarget);
            Text(target.memory==""?"No rescue or abandonment remembered yet.":target.memory,395,699,800,45,16,quiet);
            if(Button("RESUME / CLOSE",915,755,290,42,true))ResumeCrewPlay();
        }
    }
}
