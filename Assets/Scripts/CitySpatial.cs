using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class CityArt
    {
        public Transform Root => root;
        public Vector3 TestSpawn { get; private set; }
        public readonly List<Vector3> TestEnemySpawns=new List<Vector3>();
        public readonly List<Vector3> TestRetreatPoints=new List<Vector3>();
        public Vector3 GatePosition { get; private set; }
        public bool GateClosed { get; private set; }
        public bool HasServiceGate => serviceGate!=null;
        GameObject serviceGate;
        Bounds serviceGateBounds;

        // Returns false rather than closing a solid object through an actor.
        // One collider supplies both the physical obstruction and navigation footprint.
        public bool SetServiceGate(bool closed,IEnumerable<Vector3> occupants)
        {
            if(!serviceGate)return false;
            if(closed==GateClosed)return true;
            if(closed&&occupants!=null)
            {
                var safe=serviceGateBounds;safe.Expand(new Vector3(1.2f,4,1.2f));
                foreach(var p in occupants)if(safe.Contains(new Vector3(p.x,serviceGateBounds.center.y,p.z)))return false;
            }
            GateClosed=closed;serviceGate.SetActive(closed);
            RefreshProps();Nav.Bake();
            return true;
        }
        void ServiceGate(Vector3 p,float width)
        {
            GatePosition=p;
            serviceGate=Prop("Service gate / E to open or close",p+Vector3.up*1.2f,new Vector3(width,2.4f,.3f),Hex("627A7D"));
            Physics.SyncTransforms();serviceGateBounds=serviceGate.GetComponent<Collider>().bounds;GateClosed=true;
            for(int side=-1;side<=1;side+=2)
                // Jambs project beyond adjacent wall faces instead of sharing their planes.
                Solid("Gate jamb",p+new Vector3(side*(width*.5f+.2f),1.4f,0),new Vector3(.52f,2.8f,.56f),Hex("D0B48B"));
            Sign("SERVICE GATE / E",p+new Vector3(0,3,0),Amber,.19f);
        }
        void Cover(string name,Vector3 p,Vector3 size,Color color)
        {
            Solid(name,p+Vector3.up*(size.y*.5f),size,color);
            Box("Worn cover edge",p+Vector3.up*(size.y+.035f),new Vector3(size.x,.07f,size.z),Hex("B2AAA0"));
        }
        public readonly List<Rect> Streets=new List<Rect>();
        public bool IsStreet(Vector3 point)
        {foreach(var street in Streets)if(street.Contains(new Vector2(point.x,point.z)))return true;return false;}
        void BuildStreetGrid()
        {
            Streets.Clear();
            Streets.Add(new Rect(-5,-68,10,136));
            Streets.Add(new Rect(-46,-44,6,90));
            Streets.Add(new Rect(45,-44,6,87));
            Streets.Add(new Rect(-73,-44,148,8));
            Streets.Add(new Rect(-73,8,148,8));
            Streets.Add(new Rect(-73,40,68,6));
            Streets.Add(new Rect(-5,35,80,8));
            Streets.Add(new Rect(-8,-17,83,8));
            // Tessellate the union, so every ground point has exactly one asphalt face.
            var xs=new List<float>();var zs=new List<float>();
            foreach(var r in Streets)
            {if(!xs.Contains(r.xMin))xs.Add(r.xMin);if(!xs.Contains(r.xMax))xs.Add(r.xMax);if(!zs.Contains(r.yMin))zs.Add(r.yMin);if(!zs.Contains(r.yMax))zs.Add(r.yMax);}
            xs.Sort();zs.Sort();
            for(int x=0;x<xs.Count-1;x++)for(int z=0;z<zs.Count-1;z++)
            {
                var center=new Vector3((xs[x]+xs[x+1])*.5f,.02f,(zs[z]+zs[z+1])*.5f);
                if(IsStreet(center))Box("Street / district grid",center,new Vector3(xs[x+1]-xs[x],.02f,zs[z+1]-zs[z]),Hex("39434D"));
            }
            // Side streets stay local: the established four cars retain their existing routes.
            foreach(float z in new[]{-40f,12f})for(int x=-68;x<=70;x+=7)
                if(Mathf.Abs(x)>7&&Mathf.Abs(x+43)>5&&Mathf.Abs(x-48)>5)
                    Box("Road dash",new Vector3(x,.075f,z),new Vector3(2.7f,.01f,.14f),Hex("A5A395"));
            foreach(float x in new[]{-43f,48f})for(int z=-30;z<34;z+=7)
                if(z<5||z>19)Box("Local street dash",new Vector3(x,.075f,z),new Vector3(.14f,.01f,2.2f),Hex("A5A395"));
        }
        void BuildSpatialIdentity()
        {
            // Visual ground bands: road top .03, paving .055, lane paint .08, crossings .10.
            // Equal-height paving regions meet at their edges; none adds a movement collider.
            // Walkable district rooms are connected by legible courts and service lanes.
            // Clinic x[-38,-21],z[-22,-6] is reserved for the actual inhabitable building.
            Box("Market court cobbles",new Vector3(-24.75f,.045f,-29),new Vector3(27.5f,.02f,15),Hex("8F887B"));
            Box("Clinic west backlane",new Vector3(-39,.045f,-7.75f),new Vector3(2,.02f,27.5f),Hex("827D71"));
            Box("Clinic east pedestrian lane",new Vector3(-16,.045f,-12.75f),new Vector3(10,.02f,17.5f),Hex("918C80"));
            Box("Church forecourt stone",new Vector3(-59,.045f,19.25f),new Vector3(23,.02f,6.5f),Hex("989389"));

            Box("West residential footway",new Vector3(-40,.045f,-47),new Vector3(73,.02f,4),Hex("A09A8B"));
            Box("East residential footway",new Vector3(43,.045f,-47),new Vector3(68,.02f,4),Hex("A09A8B"));
            Box("West dock service lane",new Vector3(-47,.045f,31.25f),new Vector3(2,.02f,17.5f),Hex("878477"));
            Box("East workshop courtyard",new Vector3(63,.045f,20),new Vector3(20,.02f,4),Hex("90887A"));
            Box("Harbor promenade",new Vector3(0,.045f,65),new Vector3(154,.02f,7),Hex("98998D"));
            // Two-metre local sidewalks frame the connected grid. Courts keep their own wider thresholds.
            Box("West market sidewalk",new Vector3(-47,.045f,-22),new Vector3(2,.02f,28),Hex("A09A8B"));
            Box("East inner sidewalk",new Vector3(44,.045f,-.5f),new Vector3(2,.02f,17),Hex("A09A8B"));
            Box("East outer sidewalk",new Vector3(52,.045f,-.5f),new Vector3(2,.02f,17),Hex("A09A8B"));
            Box("East northern sidewalk",new Vector3(52,.045f,25.5f),new Vector3(2,.02f,19),Hex("A09A8B"));
            Box("Home frontcourt",Jobs.Home+new Vector3(0,.065f,0),new Vector3(4,.02f,2),Hex("B1AA97"));
            Box("Home door threshold",new Vector3(-11,.09f,-49.6f),new Vector3(1.5f,.02f,.6f),Hex("D2C3A4"));
            // Forecourt furniture creates places to pause, not arbitrary collision clutter.
            Cover("Church bench west",new Vector3(-67,0,17),new Vector3(3,.55f,.65f),Hex("75634E"));
            Cover("Church bench east",new Vector3(-51,0,17),new Vector3(3,.55f,.65f),Hex("75634E"));
            Solid("Church noticeboard posts",new Vector3(-68,1,18),new Vector3(.18f,2,.18f),Hex("75634E"));
            Box("Community noticeboard",new Vector3(-68,1.7f,18),new Vector3(2,1.3f,.15f),Hex("C9B992"));
            Sign("MISSING / MEALS / WORK",new Vector3(-68,1.7f,17.9f),Hex("433F35"),.10f);
            Cover("Market serving counter",new Vector3(-36.5f,0,-30),new Vector3(3,1.1f,1),Hex("817259"));
            Box("Market canvas awning",new Vector3(-36.5f,2.5f,-30),new Vector3(4,.12f,2.6f),Hex("B69F7C"));
            Cover("Market courtyard seat",new Vector3(-34,0,-34),new Vector3(3,.55f,.7f),Hex("75634E"));
            Cover("Residential garden wall",new Vector3(-47,0,-57),new Vector3(.5f,.8f,14),Hex("908578"));
            Cover("Workshop loading bench",new Vector3(55,0,20),new Vector3(3.5f,1.2f,1.4f),Hex("7A837C"));
            Cover("Workshop service equipment",new Vector3(69,0,20),new Vector3(3,1.9f,2),Hex("667B7D"));
            Box("Market court signboard",new Vector3(-36.5f,2.7f,-31.35f),new Vector3(3.6f,.55f,.12f),Hex("30494C"));
            Sign("MARKET COURT",new Vector3(-36.5f,2.7f,-31.43f),Amber,.12f);
            BuildingSign("SAINT BRIGID / OPEN TABLE",new Vector3(-59,2.3f,22.9f),Amber,.22f);
            BuildingSign("DOCKSIDE WORKSHOPS",new Vector3(63,3,21.8f),Amber,.21f);
            Box("Loading apron",new Vector3(30,.045f,35),new Vector3(30,.02f,14),Hex("77817E"));
            Box("Service passage paving",new Vector3(30,.045f,21.25f),new Vector3(11,.02f,13.5f),Hex("867C69"));
            Cover("Service west wall",new Vector3(25,0,30),new Vector3(.5f,2.4f,10),Hex("596E71"));
            Cover("Service east wall",new Vector3(35,0,30),new Vector3(.5f,2.4f,10),Hex("596E71"));
            ServiceGate(new Vector3(30,0,29),9.5f);
            Cover("Quay winch housing",new Vector3(19,0,35),new Vector3(3,1.8f,2),Hex("637777"));
            Cover("Bonded pallet stack",new Vector3(40,0,34),new Vector3(3,1.7f,2),Hex("A08865"));
            Cover("Quay low barrier",new Vector3(11,0,32),new Vector3(3,.75f,1),Hex("9A9D92"));
            for(int i=0;i<4;i++)Box("Loading bay stripe",new Vector3(16+i*6,.075f,41),new Vector3(3,.01f,.12f),Amber);
            Box("Quay wayfinding board",new Vector3(10,2.5f,40.07f),new Vector3(3.6f,.6f,.12f),Hex("30494C"));
            Box("Quay wayfinding post",new Vector3(10,1.2f,40.12f),new Vector3(.12f,2.4f,.12f),Hex("65736D"));
            Sign("PUBLIC QUAY",new Vector3(10,2.5f,40),Mint,.13f);
            BuildingSign("BONDED YARD",new Vector3(39,3,31.8f),Amber,.25f);
            // Reserved edges are visible context, not a claim that these areas are traversable.
            Box("Floodworks boundary sign",new Vector3(75,2.7f,64.07f),new Vector3(6.2f,.6f,.12f),Hex("30494C"));
            Sign("FLOODWORKS / NEXT DISTRICT",new Vector3(75,2.7f,64),Hex("D2B392"),.12f);
            Box("Old high water mark",new Vector3(40,1.1f,17.98f),new Vector3(8,.08f,.04f),Hex("B4ADA0"));
            TestEnemySpawns.AddRange(new[]{new Vector3(21,0,39),new Vector3(33,0,38),new Vector3(42,0,41)});
            TestRetreatPoints.AddRange(new[]{new Vector3(17,0,43),new Vector3(34,0,42)});
        }

        // Each arena is built on a fresh CityArt; game state, enemies and equipment are caller-owned.
        public void BuildTestLevel(int level)
        {
            Resources.LoadAll<Material>("Rendering");
            root=new GameObject("FUNSTRA / Foundation test "+level).transform;
            Box("Arena foundation",new Vector3(0,-.5f,0),new Vector3(48,1,48),Hex("455968"),null,true);
            Box("Arena paving",new Vector3(0,.015f,0),new Vector3(46,.025f,46),Hex("78827E"));
            for(int side=-1;side<=1;side+=2)
            {
                Solid("Arena boundary",new Vector3(side*23,1.25f,0),new Vector3(.5f,2.5f,46),Hex("475B66"));
                Solid("Arena boundary",new Vector3(0,1.25f,side*23),new Vector3(46,2.5f,.5f),Hex("475B66"));
            }
            TestSpawn=new Vector3(0,0,-17);
            string[] names={"MOVEMENT & OBSTACLES","WEAPON HANDLING","GROUP TACTICS","COMBINED ENCOUNTER"};
            Sign(names[Mathf.Clamp(level-1,0,3)],new Vector3(0,3,22.6f),Amber,.37f);
            Ring("Start / recovery",TestSpawn,2,Mint);
            if(level==1)
            {
                Cover("Corner wall",new Vector3(-6,0,-8),new Vector3(1,2.5f,12),Hex("627878"));
                Cover("Corner return",new Vector3(-10,0,-2),new Vector3(9,2.5f,1),Hex("627878"));
                Cover("Passage west",new Vector3(4,0,0),new Vector3(1,2.5f,14),Hex("627878"));
                Cover("Passage east",new Vector3(9,0,0),new Vector3(1,2.5f,14),Hex("627878"));
                ServiceGate(new Vector3(6.5f,0,2),4);
                Cover("Bin with collision",new Vector3(-3,0,8),new Vector3(1.8f,1.43f,1.1f),Hex("2F6B5E"));
                Cover("Low cover",new Vector3(-9,0,12),new Vector3(4,.65f,1.3f),Hex("999A89"));
                TestEnemySpawns.Add(new Vector3(-11,0,17));TestRetreatPoints.Add(new Vector3(-17,0,17));
            }
            else if(level==2)
            {
                for(int z=-8;z<=16;z+=8)
                { Box("Range distance",new Vector3(0,.05f,z),new Vector3(34,.02f,.08f),Amber);Sign((z+17)+" m",new Vector3(-19,.5f,z),Amber,.18f); }
                Cover("Muzzle obstruction",new Vector3(5,0,-12),new Vector3(2,1.7f,.3f),Hex("7F8A83"));
                Cover("Thin plate",new Vector3(-6,0,0),new Vector3(4,2,.18f),Hex("5D727E"));
                Cover("Partial cover",new Vector3(8,0,8),new Vector3(5,.7f,1),Hex("A08D73"));
                TestEnemySpawns.AddRange(new[]{new Vector3(-12,0,7),new Vector3(0,0,14),new Vector3(12,0,18)});
                TestRetreatPoints.AddRange(new[]{new Vector3(-17,0,16),new Vector3(17,0,16)});
            }
            else
            {
                Cover("Left storage",new Vector3(-8,0,1),new Vector3(7,2.8f,8),Hex("607984"));
                Cover("Right storage",new Vector3(8,0,4),new Vector3(7,2.8f,8),Hex("9A806A"));
                Cover("Crossfire cover",new Vector3(0,0,11),new Vector3(4,1.7f,2),Hex("788D88"));
                Cover("Approach cover",new Vector3(-2,0,-10),new Vector3(3,1.7f,1.5f),Hex("8F8B78"));
                Cover("Retreat screen",new Vector3(-15,0,13),new Vector3(1,2.3f,5),Hex("71818B"));
                TestEnemySpawns.AddRange(new[]{new Vector3(-10,0,9),new Vector3(3,0,14),new Vector3(13,0,12)});
                TestRetreatPoints.AddRange(new[]{new Vector3(-18,0,18),new Vector3(17,0,18)});
                if(level==4)
                {
                    Cover("Service partition",new Vector3(15,0,-6),new Vector3(1,2.5f,14),Hex("607984"));
                    ServiceGate(new Vector3(18.75f,0,-2),6.5f);
                    Cover("Loading stock",new Vector3(-16,0,-1),new Vector3(3,1.6f,4),Hex("A08D73"));
                    Ring("Escape",new Vector3(-18,0,-18),2,Mint);
                    Sign("ESCAPE",new Vector3(-18,1,-19),Mint,.22f);
                }
            }
            RefreshProps();Nav.Bake();
        }
    }
}
