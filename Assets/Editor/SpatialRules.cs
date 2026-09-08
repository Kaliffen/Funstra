using System;
using System.IO;
using Funstra;
using UnityEditor;
using UnityEngine;

public static class SpatialRules
{
    [MenuItem("Funstra/Verify spatial foundations")]
    public static void Verify()
    {
        int count=0;
        Action<bool,string> check=(ok,why)=>{if(!ok)throw new Exception("Spatial rule failed: "+why);count++;};
        var nav=new CityNavigation();
        nav.Props.Add(new Bounds(new Vector3(0,1.2f,0),new Vector3(8,2.4f,.3f)));
        nav.Obstacles.Add(new Bounds(new Vector3(-4.5f,1.2f,-1),new Vector3(.5f,2.4f,8)));
        nav.Obstacles.Add(new Bounds(new Vector3(4.5f,1.2f,-1),new Vector3(.5f,2.4f,8)));
        nav.Bake();
        Vector3 south=new Vector3(0,0,-5),north=new Vector3(0,0,5);
        check(!nav.ClearWalk(south,north),"Closed gate blocks direct passage");
        check(!nav.Sight(south,north),"Closed gate blocks sight through its physical panel");
        var path=nav.Find(south,north);
        check(path.Count>0,"Closed service gate retains public bypass");
        for(int i=1;i<path.Count;i++)check(nav.ClearWalk(path[i-1],path[i]),"Closed-gate detour clears every corner");
        nav.Props.Clear();nav.Bake();
        check(nav.ClearWalk(south,north)&&nav.Sight(south,north),"Open gate clears traversal and sight together");
        nav.Props.Add(new Bounds(new Vector3(0,.3f,0),new Vector3(2,.6f,1)));
        check(!nav.ClearWalk(south,north)&&nav.Sight(south,north),"Low cover blocks feet while leaving eye-height sight open");
        nav.Props.Clear();nav.Props.Add(new Bounds(new Vector3(0,1,0),new Vector3(2,2,.1f)));
        check(!nav.Sight(south,north),"Thin physical cover blocks vision");
        check(!nav.Sight(Vector3.zero,north),"Starting inside solid cover cannot see through it");
        var restored=nav.SafePoint(Vector3.zero);
        check(nav.Walkable(restored),"Old embedded save position relocates to valid free space");
        VerifyGeometry(check);
        Directory.CreateDirectory("Evidence");
        File.WriteAllText("Evidence/spatial-rules-result.txt","PASS: "+count+" spatial route, gate and sight assertions\n");
    }
    static void VerifyGeometry(Action<bool,string> check)
    {
        for(int level=0;level<=4;level++)
        {
            var city=new CityArt();
            try
            {
                if(level==0)city.Build();else city.BuildTestLevel(level);
                string label=level==0?"Old Port":"Test "+level;
                VerifyGroundFaces(city,check);
                Vector3 spawn=level==0?Jobs.Home:city.TestSpawn;
                check(city.Nav.Walkable(spawn),label+" spawn clear");
                foreach(var post in city.TestEnemySpawns)CheckRoute(city.Nav,spawn,post,label+" enemy approach",check);
                foreach(var post in city.TestRetreatPoints)CheckRoute(city.Nav,spawn,post,label+" retreat access",check);
                if(level==0)
                {
                    VerifyStreetGrid(city,check);
                    foreach(var anchor in new[]{Jobs.Mara,DistrictState.Clinic,DistrictState.Garage,DistrictState.Buyer,DistrictState.CollectorPost})
                        CheckRoute(city.Nav,spawn,anchor,"Preserved story anchor",check);
                    foreach(var job in Jobs.All)
                    {
                        var approach=city.Nav.SafePoint(job.position);
                        check(Vector3.Distance(approach,job.position)<2.2f,"Preserved job remains in interaction reach");
                        CheckRoute(city.Nav,spawn,approach,"Preserved Mara job",check);
                    }
                    foreach(var cargo in CargoRun.Sites)CheckRoute(city.Nav,spawn,cargo.position,"Preserved cargo site",check);
                    CheckRoute(city.Nav,DistrictState.Clinic,Jobs.Mara,"Clinic to market",check);
                    CheckRoute(city.Nav,Jobs.Mara,new Vector3(10,0,39),"Market to public quay",check);
                    // Original patrol and resident turning points remain traversable after reshaping blocks.
                    foreach(var point in new[]{new Vector3(8,0,-13),new Vector3(42,0,-13),new Vector3(45,0,-38),new Vector3(8,0,-38),new Vector3(-43,0,13),new Vector3(-7,0,13),new Vector3(-7,0,-13),new Vector3(-43,0,-13),new Vector3(8,0,39),new Vector3(44,0,39),new Vector3(46,0,13),new Vector3(8,0,13)})
                        check(city.Nav.Walkable(point,false),"Original patrol waypoint clear "+point);
                }
                if(city.HasServiceGate)
                {
                    VerifyGateJambs(city,level,check);
                    var gate=city.GatePosition;
                    var a=gate+Vector3.back*(level==0?6:4);var b=gate+Vector3.forward*(level==0?8:4);
                    check(!city.Nav.ClearWalk(a,b),label+" actual closed gate blocks feet");
                    check(!city.Nav.Sight(a,b),label+" actual closed gate blocks sight");
                    CheckRoute(city.Nav,a,b,label+" actual gate public bypass",check);
                    check(city.SetServiceGate(false,null),label+" gate opens");
                    check(city.Nav.ClearWalk(a,b)&&city.Nav.Sight(a,b),label+" open gate permits direct passage and sight");
                    check(!city.SetServiceGate(true,new[]{gate})&&!city.GateClosed,label+" gate refuses occupied closure");
                    check(city.SetServiceGate(true,new[]{spawn})&&city.GateClosed,label+" clear gate closes");
                    check(!city.Nav.ClearWalk(a,b),label+" reclosing invalidates direct route");
                }
            }
            finally { if(city.Root)UnityEngine.Object.DestroyImmediate(city.Root.gameObject); }
        }
    }
    static void VerifyStreetGrid(CityArt city,Action<bool,string> check)
    {
        check(!city.IsStreet(Jobs.Home),"Home frontcourt is outside every asphalt street region");
        check(city.Nav.Walkable(Jobs.Home),"New terrace home is a usable extraction and recovery anchor");
        var threshold=city.Root.Find("Home door threshold");
        check(threshold&&Vector3.Distance(new Vector3(threshold.position.x,0,threshold.position.z),Jobs.Home)<2,"Home has visible nearby front-door threshold");
        CheckRoute(city.Nav,Jobs.Home,Jobs.Mara,"Terrace home to market",check);
        CheckRoute(city.Nav,Jobs.Home,DistrictState.Clinic,"Terrace home to clinic",check);
        check(city.Nav.ClearWalk(new Vector3(-43,0,-34),new Vector3(-43,0,6),false),"West local street has continuous physical clearance past market and clinic");
        check(city.Nav.ClearWalk(new Vector3(48,0,-34),new Vector3(48,0,6),false),"East local street has continuous physical clearance past shops");
        CheckRoute(city.Nav,new Vector3(-43,0,-30),new Vector3(-43,0,43),"West grid joins north quay",check);
        CheckRoute(city.Nav,new Vector3(48,0,-30),new Vector3(48,0,39),"East grid joins working quay",check);
        foreach(var building in city.Buildings)foreach(var street in city.Streets)
        {
            float overlapX=Mathf.Min(building.max.x,street.xMax)-Mathf.Max(building.min.x,street.xMin);
            float overlapZ=Mathf.Min(building.max.z,street.yMax)-Mathf.Max(building.min.z,street.yMin);
            check(overlapX<.001f||overlapZ<.001f,"Building footprint stays out of connected street asphalt");
        }
    }
    static void VerifyGateJambs(CityArt city,int level,Action<bool,string> check)
    {
        var jambs=new System.Collections.Generic.List<Bounds>();int joined=0;
        foreach(Transform child in city.Root)
        {
            if(child.name!="Gate jamb")continue;
            var collider=child.GetComponent<Collider>();var renderer=child.GetComponent<Renderer>();
            check(collider&&renderer,"Gate jamb has physical and rendered geometry");
            var post=renderer.bounds;jambs.Add(post);
            check(Vector3.Distance(collider.bounds.size,post.size)<.001f,"Gate jamb collider matches proud rendered faces");
            bool overlapsWall=false;
            foreach(Transform other in city.Root)
            {
                if(!(other.name.Contains("wall")||other.name.StartsWith("Passage ")||other.name=="Service partition"))continue;
                var wall=other.GetComponent<Collider>();if(!wall||!post.Intersects(wall.bounds))continue;
                overlapsWall=true;var b=wall.bounds;
                float xSeparation=Mathf.Min(Mathf.Abs(post.min.x-b.min.x),Mathf.Abs(post.max.x-b.max.x));
                float zSeparation=Mathf.Min(Mathf.Abs(post.min.z-b.min.z),Mathf.Abs(post.max.z-b.max.z));
                check(xSeparation>.019f&&zSeparation>.019f,"Overlapping gate jamb/wall side faces have visible separation: "+other.name);
            }
            if(overlapsWall)joined++;
        }
        check(jambs.Count==2,"Gate retains two solid jambs");
        jambs.Sort((a,b)=>a.center.x.CompareTo(b.center.x));
        check(jambs[1].min.x-jambs[0].max.x>3,"Proud gate posts retain over three metres of physical opening");
        check(joined>=(level==4?1:2),"Gate jambs overlap intended adjacent walls without coplanar faces");
    }
    static void VerifyGroundFaces(CityArt city,Action<bool,string> check)
    {
        var surfaces=new System.Collections.Generic.List<Renderer>();
        foreach(Transform child in city.Root)
        {
            var renderer=child.GetComponent<MeshRenderer>();var scale=child.localScale;
            if(!renderer||scale.y>.25f||child.position.y<-.2f||child.position.y>.12f||child.name=="Harbor reflection")continue;
            surfaces.Add(renderer);
            // Ground paint/paving must not introduce thin physics ledges for the controller.
            if(child.position.y>0)
            {var collider=child.GetComponent<Collider>();check(!collider||!collider.enabled,"Visual ground overlay has no collision ledge: "+child.name);}
        }
        for(int i=0;i<surfaces.Count;i++)for(int j=i+1;j<surfaces.Count;j++)
        {
            var a=surfaces[i].bounds;var b=surfaces[j].bounds;
            float overlapX=Mathf.Min(a.max.x,b.max.x)-Mathf.Max(a.min.x,b.min.x);
            float overlapZ=Mathf.Min(a.max.z,b.max.z)-Mathf.Max(a.min.z,b.min.z);
            if(overlapX>.001f&&overlapZ>.001f)
                check(Mathf.Abs(a.max.y-b.max.y)>.001f,"Overlapping ground top faces are separated: "+surfaces[i].name+" / "+surfaces[j].name);
        }
    }
    static void CheckRoute(CityNavigation nav,Vector3 from,Vector3 to,string label,Action<bool,string> check)
    {
        check(nav.Walkable(from)&&nav.Walkable(to),label+" endpoints clear");
        var route=nav.Find(from,to);check(route.Count>0,label+" exists");
        Vector3 previous=from;
        foreach(var point in route)
        { check(nav.ClearWalk(previous,point),label+" edge clears collision");previous=point; }
        check(Vector3.Distance(previous,to)<.01f,label+" reaches intended endpoint");
    }

}
