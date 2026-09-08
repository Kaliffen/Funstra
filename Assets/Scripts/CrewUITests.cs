using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        // Call after StartRun in the isolated Crew route. These exercise production
        // input guards and state decisions, not synthetic mouse events or human feel.
        void VerifyCrewUIRules()
        {
            bool panel=crewPanel,recruited=District.recruited;
            string moveTarget=crewMoveTarget;
            ScreenMode priorScreen=screen;
            float health=District.health,neriHealth=District.neri.health;
            try
            {
                crewPanel=false;crewMoveTarget="";
                Check(CrewHudBlocksPointer(new Vector2(140,190))&&CrewHudBlocksPointer(new Vector2(1400,150)),"Crew roster and K button block world-fire input in compact HUD");
                Check(!CrewHudBlocksPointer(new Vector2(800,450)),"Open street pointer remains available for combat input");
                crewPanel=true;
                Check(CrewHudBlocksPointer(new Vector2(800,450)),"Open crew panel blocks combat input");
                crewPanel=false;crewMoveTarget="neri";
                Check(CrewHudBlocksPointer(new Vector2(800,450)),"Choosing a crew destination cannot also fire a weapon");
                crewPanel=true;screen=ScreenMode.Tactics;ResumeCrewPlay();
                Check(screen==ScreenMode.Play&&!crewPanel&&crewMoveTarget=="","Resume shortcut clears the stopped-time overlay and pending destination");
                ToggleCrewPanel();Check(screen==ScreenMode.Tactics&&crewPanel,"K opens crew panel and stops time");
                ToggleCrewPanel();Check(screen==ScreenMode.Play&&!crewPanel,"K closes crew panel and resumes consistently");
                var contacts=new List<string>{"player","neri"};
                Check(ChooseCrewArrestCandidate(contacts,"neri")=="neri","Two arrest contacts preserve the existing target instead of resetting progress each frame");
                Check(ChooseCrewArrestCandidate(contacts,"rell")=="player","New arrest selects one deterministic eligible actor");
                contacts.Remove("player");
                Check(ChooseCrewArrestCandidate(contacts,"player")=="neri","Arrest changes target only when previous contact is absent");
                contacts.Clear();Check(ChooseCrewArrestCandidate(contacts,"neri")=="","No contact leaves the arrest timer free to decay");
                District.recruited=true;District.neri.health=50;District.health=50;
                Check(!CrewDefeatHandled(),"Living protagonist can explicitly surrender despite a living partner");
                District.health=0;
                Check(CrewDefeatHandled(),"Downed protagonist continues through a living recruited partner");
                smokeResults.Add("METHOD: crew UI regression checks call shared pointer guards, resume actions and arrest selection directly; no synthetic desktop input or human input-feel claim.");
            }
            finally
            {
                crewPanel=panel;crewMoveTarget=moveTarget;screen=priorScreen;
                District.health=health;District.recruited=recruited;District.neri.health=neriHealth;
            }
        }
    }
}
