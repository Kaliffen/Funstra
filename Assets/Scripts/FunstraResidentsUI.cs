using UnityEngine;

namespace Funstra
{
    public sealed partial class FunstraGame
    {
        void DrawResidentStatus()
        {
            if(!ResidentsEnabled||showMap||(screen!=ScreenMode.Play&&screen!=ScreenMode.Tactics)||CrewEnabled&&(crewPanel||District.crew.For(ControlledCrewId).carrying!=""))return;
            TownAgent nearest=null;float distance=11;
            foreach(var a in Agents)
            {
                if(a.Police||a.Record?.resident==null||a.Record.health<=0)continue;
                float d=Vector3.Distance(ControlledPosition,a.Position);
                if(d<distance&&City.Nav.Sight(ControlledPosition,a.Position)){nearest=a;distance=d;}
            }
            if(nearest==null)return;
            var p=nearest.Record.resident;
            Panel(455,578,690,116);
            Text(nearest.Record.name+" / "+p.occupation.ToUpperInvariant(),472,588,655,22,14,CityArt.Amber,FontStyle.Bold);
            Text(p.action,472,613,655,23,17,paper,FontStyle.Bold);
            Text(p.reason,472,641,655,43,14,quiet);
        }
    }
}
