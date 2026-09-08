using System;
using System.IO;
using Funstra;
using UnityEditor;
using UnityEngine;

public static class ClinicRules
{
    [MenuItem("Funstra/Verify clinic interior")]
    public static void Verify()
    {
        int count=0;
        Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception("Clinic rule: "+label);count++;Debug.Log("CLINIC PASS / "+label);};
        var city=new CityArt();GameObject cameraObject=null;
        try
        {
            city.Build();Physics.SyncTransforms();
            check(city.ClinicFloorBounds.Contains(DistrictState.Clinic+Vector3.up),"Treatment anchor is in the actual building");
            Route(city.Nav,city.ClinicSouthDoor+Vector3.back*2,DistrictState.Clinic,"South entrance to treatment",check);
            Route(city.Nav,city.ClinicEastDoor+Vector3.right*2,DistrictState.Clinic,"East entrance to treatment",check);
            Route(city.Nav,DistrictState.Clinic,new Vector3(-35,0,-15),"Treatment to medicine store",check);
            Route(city.Nav,city.ClinicSouthDoor,city.ClinicEastDoor,"Connected building exits",check);
            var westOutside=new Vector3(-40,0,-17);var westInside=new Vector3(-36,0,-17);
            check(!city.Nav.ClearWalk(westOutside,westInside)&&!city.Nav.Sight(westOutside,westInside),"Tall clinic wall blocks both feet and sight");
            check(Physics.Raycast(westOutside+Vector3.up,Vector3.right,4,1<<8),"Tall wall also blocks physics rays");
            var clinic=city.Root.Find("MUTUAL CLINIC - treatment and stores");
            check(clinic!=null,"Clinic is a coherent building hierarchy");
            var roof=clinic.Find("Clinic pitched standing-seam roof").GetComponent<Renderer>();
            var south=clinic.Find("South reception wall").GetComponent<Renderer>();
            var north=clinic.Find("North treatment wall").GetComponent<Renderer>();
            var east=clinic.Find("East north wall").GetComponent<Renderer>();
            var colliders=clinic.GetComponentsInChildren<Collider>();int obstacleCount=city.Nav.Obstacles.Count;
            cameraObject=new GameObject("Clinic validation camera");var camera=cameraObject.AddComponent<Camera>();
            SetView(city,camera,DistrictState.Clinic);
            check(!roof.enabled&&!south.enabled&&!east.enabled,"Inside camera reveals roof and foreground walls");
            check(north.enabled,"Inside cutaway retains background wall context");
            check(clinic.Find("Door jamb").GetComponent<Renderer>().enabled,"Cutaway preserves doorway frame context");
            SetView(city,camera,new Vector3(-30,0,-4));
            check(!roof.enabled&&!north.enabled,"Outside player behind clinic is revealed through actual camera obstruction");
            SetView(city,camera,new Vector3(-27,0,-26));
            check(roof.enabled&&south.enabled&&north.enabled&&east.enabled,"Unobstructed exterior restores the building");
            check(city.Nav.Obstacles.Count==obstacleCount,"Cutaways leave navigation obstacles unchanged");
            foreach(var collider in colliders)check(collider.enabled,"Cutaways leave physical colliders enabled: "+collider.name);
            check(!city.Nav.ClearWalk(westOutside,westInside)&&!city.Nav.Sight(westOutside,westInside),"Restored view has identical wall traversal and sight");
            Directory.CreateDirectory("Evidence");
            File.WriteAllText("Evidence/clinic-rules-result.txt","PASS: "+count+" clinic route, collision and cutaway assertions\n");
            Debug.Log("CLINIC RULES COMPLETE / "+count+" checks");
        }
        finally
        {
            if(cameraObject)UnityEngine.Object.DestroyImmediate(cameraObject);
            if(city.Root)UnityEngine.Object.DestroyImmediate(city.Root.gameObject);
        }
    }
    static void SetView(CityArt city,Camera camera,Vector3 player)
    {
        camera.transform.position=player+new Vector3(19,32,-23);
        camera.transform.LookAt(player+Vector3.up);Physics.SyncTransforms();city.UpdateClinicCutaway(camera,player);
    }
    static void Route(CityNavigation nav,Vector3 from,Vector3 to,string label,Action<bool,string> check)
    {
        check(nav.Walkable(from,false)&&nav.Walkable(to,false),label+" endpoints clear");
        var route=nav.Find(from,to);check(route.Count>0,label+" path exists");
        var previous=from;
        foreach(var point in route){check(nav.ClearWalk(previous,point),label+" path segment clears furnishings and walls");previous=point;}
        check(Vector3.Distance(previous,to)<.01f,label+" reaches intended destination");
    }
}
