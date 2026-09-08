using System.Collections.Generic;
using UnityEngine;

namespace Funstra
{
    public sealed partial class CityArt
    {
        public Bounds ClinicFloorBounds { get; private set; }
        public Vector3 ClinicSouthDoor => new Vector3(-27, 0, -22);
        public Vector3 ClinicEastDoor => new Vector3(-21, 0, -12);
        readonly List<Renderer> clinicRoof = new List<Renderer>();
        readonly List<Renderer> clinicRoomLabels = new List<Renderer>();
        readonly List<Renderer> clinicSouth = new List<Renderer>();
        readonly List<Renderer> clinicEast = new List<Renderer>();
        readonly List<Renderer> clinicWest = new List<Renderer>();
        readonly List<Renderer> clinicNorth = new List<Renderer>();
        Transform clinicRoot;
        bool clinicBuilt;

        public void BuildClinic()
        {
            if (clinicBuilt) return;
            clinicBuilt = true;
            clinicRoot = new GameObject("MUTUAL CLINIC - treatment and stores").transform;
            clinicRoot.SetParent(root, false);
            ClinicFloorBounds = new Bounds(new Vector3(-29.5f, 1.9f, -14), new Vector3(17, 4.2f, 16));
            var plaster = Hex("AFB2A3"); var basecoat = Hex("45685C"); var timber = Hex("514C40");
            Box("Clinic tiled foundation", new Vector3(-29.5f,.035f,-14),new Vector3(17,.08f,16),Hex("747A70"),clinicRoot);
            for(int x=-37;x<-21;x+=2) for(int z=-21;z<-6;z+=2)
            {
                var tile=Box("Worn terrazzo tile",new Vector3(x,.081f,z),new Vector3(1.94f,.012f,1.94f),Hex((x+z)%4==0?"92998A":"A3A797"),clinicRoot);
                tile.GetComponent<Renderer>().sharedMaterial=WeatheredMat(Hex((x+z)%4==0?"92998A":"A3A797"),.35f);
            }
            // Walls are separate from roof and trim: visibility never alters physics or navigation.
            ClinicWall("West store wall",new Vector3(-38,2.1f,-14),new Vector3(.36f,4.2f,16),plaster,clinicWest);
            ClinicWall("North treatment wall",new Vector3(-29.5f,2.1f,-6),new Vector3(17,4.2f,.36f),plaster,clinicNorth);
            ClinicWall("South reception wall",new Vector3(-33.3f,2.1f,-22),new Vector3(9.4f,4.2f,.36f),plaster,clinicSouth);
            ClinicWall("South treatment wall",new Vector3(-23.2f,2.1f,-22),new Vector3(4.4f,4.2f,.36f),plaster,clinicSouth);
            ClinicWall("East north wall",new Vector3(-21,2.1f,-8.2f),new Vector3(.36f,4.2f,4.4f),plaster,clinicEast);
            ClinicWall("East south wall",new Vector3(-21,2.1f,-17.8f),new Vector3(.36f,4.2f,8.4f),plaster,clinicEast);
            // A separate medicine store can be entered from the treatment room.
            ClinicWall("Store partition north",new Vector3(-32,1.6f,-9.7f),new Vector3(.24f,3.2f,7.4f),basecoat,clinicWest);
            ClinicWall("Store partition south",new Vector3(-32,1.6f,-19.3f),new Vector3(.24f,3.2f,5.4f),basecoat,clinicWest);
            ClinicDoor(ClinicSouthDoor,false,timber); ClinicDoor(ClinicEastDoor,true,timber);
            // Shallow, step-free entrance apron; no raised collider threshold.
            Box("Clinic entrance stone apron",new Vector3(-27,.085f,-23.05f),new Vector3(4,.08f,2.1f),Hex("A2AA99"),clinicRoot);
            Box("Clinic receiving apron",new Vector3(-20,.085f,-12),new Vector3(2,.08f,3.8f),Hex("A2AA99"),clinicRoot);
            ClinicWindow(new Vector3(-34,2.25f,-22.21f),false,clinicSouth);
            ClinicWindow(new Vector3(-23.4f,2.25f,-22.21f),false,clinicSouth);
            ClinicWindow(new Vector3(-20.79f,2.25f,-17.5f),true,clinicEast);
            ClinicWindow(new Vector3(-20.79f,2.25f,-8),true,clinicEast);
            Box("Clinic sign fascia",new Vector3(-27,3.52f,-22.3f),new Vector3(6.9f,.88f,.2f),basecoat,clinicRoot);
            Sign("MUTUAL CLINIC",new Vector3(-27,3.55f,-22.43f),Hex("F4E9C4"),.105f,clinicRoot);
            Box("Enamel cross vertical",new Vector3(-31.5f,2.55f,-22.25f),new Vector3(.2f,.9f,.08f),Mint,clinicRoot,false,true);
            Box("Enamel cross arms",new Vector3(-31.5f,2.55f,-22.26f),new Vector3(.7f,.2f,.08f),Mint,clinicRoot,false,true);
            ClinicRoomLabel("TREATMENT",new Vector3(-26,2.7f,-6.25f),3.3f,.075f);
            ClinicRoomLabel("MEDICAL STORES",new Vector3(-35,2.7f,-6.25f),4.1f,.065f);
            ClinicLamp(new Vector3(-29.15f,2.8f,-22.37f),clinicSouth);
            ClinicLamp(new Vector3(-25.5f,2.8f,-6.4f),clinicNorth);
            // Purpose-built furniture: open frames, shaped upholstery, shelf boards and stored supplies.
            ClinicTreatmentFurniture();
            ClinicBench(new Vector3(-24.2f,0,-20.2f));
            ClinicShelf(new Vector3(-35,0,-7));
            ClinicShelf(new Vector3(-36.9f,0,-20));
            ClinicWashstand(new Vector3(-23,0,-7.2f));
            var roof=ClinicPrism("Clinic pitched standing-seam roof",new Vector3(-29.5f,4.15f,-14),18.1f,17.1f,1.45f,Hex("344440"));
            roof.GetComponent<Renderer>().sharedMaterial=WeatheredMat(Hex("344440"),.8f);
            roof.layer=8; roof.AddComponent<MeshCollider>().sharedMesh=roof.GetComponent<MeshFilter>().sharedMesh;
            clinicRoof.Add(roof.GetComponent<Renderer>());
            for(int i=0;i<10;i++)
            {
                float x=-38.5f+i*2;
                var seam=Box("Roof seam",new Vector3(x,4.94f,-18.25f),new Vector3(.045f,.045f,8.75f),Hex("65766B"),clinicRoot);
                seam.transform.localRotation=Quaternion.Euler(-9.63f,0,0);clinicRoof.Add(seam.GetComponent<Renderer>());
                seam=Box("Roof seam",new Vector3(x,4.94f,-9.75f),new Vector3(.045f,.045f,8.75f),Hex("65766B"),clinicRoot);
                seam.transform.localRotation=Quaternion.Euler(9.63f,0,0);clinicRoof.Add(seam.GetComponent<Renderer>());
            }
        }
        void ClinicRoomLabel(string text,Vector3 p,float width,float size)
        {
            var board=Box(text+" wall plaque",p,new Vector3(width,.48f,.08f),Hex("E6DCBD"),clinicRoot);
            clinicRoomLabels.Add(board.GetComponent<Renderer>());
            Sign(text,p+Vector3.back*.055f,Hex("4E625A"),size,clinicRoot);
            clinicRoomLabels.Add(clinicRoot.Find(text).GetComponent<Renderer>());
            // TextMesh uses a font material that can draw through roofs: hide room labels
            // explicitly until the player is in the interior and its supporting wall is visible.
            ClinicVisibility(clinicRoomLabels,false);
        }
        void ClinicWall(string name,Vector3 p,Vector3 size,Color color,List<Renderer> cut)
        {
            var wall=Box(name,p,size,color,clinicRoot,true); Nav.Obstacles.Add(new Bounds(p,size)); cut.Add(wall.GetComponent<Renderer>());
            wall.GetComponent<Renderer>().sharedMaterial=WeatheredMat(color,.6f);
            var skirt=Box(name+" painted lower course",new Vector3(p.x,.5f,p.z),new Vector3(size.x+.015f,1,size.z+.015f),Hex("527364"),clinicRoot);
            skirt.GetComponent<Renderer>().sharedMaterial=WeatheredMat(Hex("527364"),.85f);
            cut.Add(skirt.GetComponent<Renderer>());
        }
        void ClinicLamp(Vector3 at,List<Renderer> cut)
        {
            var back=Box("Repaired clinic lamp / iron backplate",at+Vector3.forward*.13f,new Vector3(.39f,.64f,.08f),Hex("3A443D"),clinicRoot);
            cut.Add(back.GetComponent<Renderer>());
            var glass=Box("Clinic lamp / warm frosted glass",at,new Vector3(.25f,.42f,.22f),Hex("EDCF99"),clinicRoot,false,true);
            cut.Add(glass.GetComponent<Renderer>());
            foreach(float x in new[]{-.16f,.16f})
                cut.Add(Box("Clinic lamp cage",at+new Vector3(x,0,-.14f),new Vector3(.035f,.54f,.035f),Hex("50534A"),clinicRoot).GetComponent<Renderer>());
            var light=new GameObject("Clinic / a light kept burning").AddComponent<Light>();light.transform.SetParent(clinicRoot,false);light.transform.localPosition=at+Vector3.back*.38f;
            light.type=LightType.Point;light.color=Hex("FFD29C");light.range=6;light.intensity=2.7f;light.shadows=LightShadows.None;
        }
        void ClinicDoor(Vector3 at,bool east,Color c)
        {
            for(int side=-1;side<=1;side+=2)
                Box("Door jamb",at+new Vector3(east?0:side*1.7f,1.45f,east?side*1.7f:0),new Vector3(east?.48f:.16f,2.9f,east?.16f:.48f),c,clinicRoot);
            Box("Door lintel",at+Vector3.up*3,new Vector3(east?.48f:3.56f,.24f,east?3.56f:.48f),c,clinicRoot);
            // Above-door masonry is collision geometry too, but does not obstruct floor navigation.
            var over=Box("Doorway upper masonry",at+Vector3.up*3.68f,new Vector3(east?.36f:3.2f,.98f,east?3.2f:.36f),Hex("AFB2A3"),clinicRoot,true);
            over.GetComponent<Renderer>().sharedMaterial=WeatheredMat(Hex("AFB2A3"),.6f);
            (east?clinicEast:clinicSouth).Add(over.GetComponent<Renderer>());
        }
        void ClinicWindow(Vector3 at,bool east,List<Renderer> cut)
        {
            var glass=Box("Deep blue window panes",at,new Vector3(east?.05f:1.9f,1.25f,east?1.9f:.05f),Hex("263C3D"),clinicRoot);cut.Add(glass.GetComponent<Renderer>());
            for(int side=-1;side<=1;side+=2)
            {
                var frame=Box("Window timber upright",at+new Vector3(east?.035f:side*1.02f,0,east?side*1.02f:-.035f),new Vector3(east?.14f:.12f,1.5f,east?.12f:.14f),Hex("EEE1BC"),clinicRoot);cut.Add(frame.GetComponent<Renderer>());
                frame=Box("Window sill and head",at+new Vector3(east?.035f:0,side*.69f,east?0:-.035f),new Vector3(east?.24f:2.16f,.12f,east?2.16f:.24f),Hex("EEE1BC"),clinicRoot);cut.Add(frame.GetComponent<Renderer>());
            }
        }
        void ClinicTreatmentFurniture()
        {
            var desk=new Vector3(-28.5f,0,-12);
            ClinicBevel("Clinic counter rounded worktop",desk+Vector3.up*1.23f,new Vector3(.8f,.14f,2.2f),Hex("BEAD8B"),.09f);
            Box("Clinic counter timber side",desk+new Vector3(-.29f,.63f,0),new Vector3(.1f,1.1f,2.05f),Hex("527973"),clinicRoot);
            foreach(float z in new[]{-.9f,.9f})Box("Clinic counter end panel",desk+new Vector3(0,.63f,z),new Vector3(.65f,1.1f,.12f),Hex("527973"),clinicRoot);
            for(int i=0;i<3;i++)
            {
                Box("Counter drawer front",desk+new Vector3(.34f,.45f+i*.27f,0),new Vector3(.05f,.22f,1.55f),Hex("799184"),clinicRoot);
                Box("Brass drawer pull",desk+new Vector3(.39f,.45f+i*.27f,0),new Vector3(.055f,.045f,.3f),Hex("C0A369"),clinicRoot);
            }
            Box("Paper prescription ledger",desk+new Vector3(0,1.33f,-.4f),new Vector3(.47f,.035f,.58f),Hex("E0D8B8"),clinicRoot);
            ClinicFurnitureObstacle(desk+Vector3.up*.65f,new Vector3(.8f,1.3f,2.2f),"Clinic supplies counter");
            var bed=new Vector3(-27,0,-15.5f);
            ClinicBevel("Treatment mattress rounded canvas",bed+Vector3.up*.43f,new Vector3(1.1f,.25f,2.2f),Hex("D7D0AE"),.12f);
            ClinicBevel("Treatment pillow",bed+new Vector3(0,.6f,.74f),new Vector3(.86f,.17f,.46f),Hex("EDE2C6"),.14f);
            ClinicBevel("Folded treatment blanket",bed+new Vector3(0,.585f,-.55f),new Vector3(1.09f,.075f,.9f),Hex("6E9183"),.045f);
            foreach(float x in new[]{-.46f,.46f})
            {
                Box("Bed iron side rail",bed+new Vector3(x,.28f,0),new Vector3(.07f,.09f,2.05f),Hex("53615B"),clinicRoot);
                foreach(float z in new[]{-.93f,.93f})Box("Bed iron leg",bed+new Vector3(x,.18f,z),new Vector3(.07f,.36f,.07f),Hex("53615B"),clinicRoot);
            }
            ClinicFurnitureObstacle(bed+Vector3.up*.35f,new Vector3(1.1f,.45f,2.2f),"Camp bed");
        }
        void ClinicBench(Vector3 at)
        {
            ClinicBevel("Waiting bench worn leather",at+Vector3.up*.57f,new Vector3(3.1f,.24f,.86f),Hex("846E4F"),.1f);
            ClinicBevel("Waiting bench back",at+new Vector3(0,1,.32f),new Vector3(3.1f,.7f,.18f),Hex("846E4F"),.07f);
            foreach(float x in new[]{-1.2f,1.2f}) foreach(float z in new[]{-.28f,.28f})
                Box("Bench iron leg",at+new Vector3(x,.25f,z),new Vector3(.08f,.5f,.08f),Hex("394A4C"),clinicRoot);
            ClinicFurnitureObstacle(at+new Vector3(0,.65f,0),new Vector3(3.1f,1.3f,.86f));
        }
        void ClinicShelf(Vector3 at)
        {
            for(int side=-1;side<=1;side+=2)Box("Medicine shelf upright",at+new Vector3(side*1.1f,1.25f,0),new Vector3(.12f,2.5f,.58f),Hex("6C6251"),clinicRoot);
            for(int i=0;i<4;i++)
            {
                Box("Medicine shelf board",at+new Vector3(0,.35f+i*.62f,0),new Vector3(2.3f,.09f,.62f),Hex("A18D68"),clinicRoot);
                for(int j=0;j<4;j++)
                {
                    var pos=at+new Vector3(-.8f+j*.5f,.57f+i*.62f,0);
                    Shape("Brown medicine bottle",PrimitiveType.Cylinder,pos,new Vector3(.14f,.17f,.14f),Hex("977443"),clinicRoot);
                    Shape("Bottle stopper",PrimitiveType.Cylinder,pos+Vector3.up*.19f,new Vector3(.09f,.035f,.09f),Hex("D9CDB0"),clinicRoot);
                    Box("Bottle paper label",pos+new Vector3(0,0,-.075f),new Vector3(.1f,.15f,.01f),Hex("DDD5B9"),clinicRoot);
                }
            }
            ClinicFurnitureObstacle(at+Vector3.up*1.25f,new Vector3(2.3f,2.5f,.62f));
        }
        void ClinicWashstand(Vector3 at)
        {
            ClinicBevel("Glazed wash basin rim",at+Vector3.up*1,new Vector3(1.35f,.2f,.8f),Hex("ECE4CA"),.1f);
            ClinicBevel("Recessed basin bowl",at+Vector3.up*1.11f,new Vector3(.94f,.015f,.53f),Hex("91B2AF"),.12f);
            foreach(float x in new[]{-.5f,.5f})Box("Washstand iron legs",at+new Vector3(x,.5f,0),new Vector3(.06f,1,.06f),Hex("465758"),clinicRoot);
            Shape("Water tap",PrimitiveType.Cylinder,at+new Vector3(0,1.23f,.28f),new Vector3(.075f,.15f,.075f),Hex("ABB3A6"),clinicRoot);
            ClinicFurnitureObstacle(at+Vector3.up*.65f,new Vector3(1.35f,1.3f,.8f));
        }
        void ClinicFurnitureObstacle(Vector3 p,Vector3 size,string name="Clinic furniture collision")
        {
            var go=new GameObject(name);go.transform.SetParent(clinicRoot,false);go.transform.localPosition=p;go.layer=8;
            var col=go.AddComponent<BoxCollider>();col.size=size;Nav.Obstacles.Add(new Bounds(p,size));
        }
        GameObject ClinicBevel(string name,Vector3 pos,Vector3 size,Color color,float radius)
        {
            float x=size.x*.5f,z=size.z*.5f,r=Mathf.Min(radius,Mathf.Min(x,z)*.6f);
            var ring=new[]{new Vector2(-x+r,-z),new Vector2(x-r,-z),new Vector2(x,-z+r),new Vector2(x,z-r),new Vector2(x-r,z),new Vector2(-x+r,z),new Vector2(-x,z-r),new Vector2(-x,-z+r)};
            var verts=new List<Vector3>();var triangles=new List<int>();
            for(int level=0;level<2;level++)foreach(var v in ring)verts.Add(new Vector3(v.x,(level-.5f)*size.y,v.y));
            for(int i=1;i<7;i++){triangles.Add(8);triangles.Add(8+i+1);triangles.Add(8+i);triangles.Add(0);triangles.Add(i);triangles.Add(i+1);}
            for(int i=0;i<8;i++){int j=(i+1)%8;triangles.Add(i);triangles.Add(8+i);triangles.Add(j);triangles.Add(j);triangles.Add(8+i);triangles.Add(8+j);}
            return ClinicMesh(name,pos,verts.ToArray(),triangles.ToArray(),color);
        }
        GameObject ClinicPrism(string name,Vector3 p,float width,float depth,float height,Color color)
        {
            float x=width/2,z=depth/2;
            return ClinicMesh(name,p,new[]{new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(-x,height,0),new Vector3(x,height,0),new Vector3(-x,0,z),new Vector3(x,0,z)},new[]{0,2,1,1,2,3,2,4,3,3,4,5,0,4,2,1,3,5},color);
        }
        GameObject ClinicMesh(string name,Vector3 p,Vector3[] vertices,int[] triangles,Color c)
        {
            var go=new GameObject(name);go.transform.SetParent(clinicRoot,false);go.transform.localPosition=p;
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(c);return go;
        }
        public void UpdateClinicCutaway(Camera camera,Vector3 player)
        {
            if(!clinicBuilt||!camera)return;
            bool inside=player.x>-38.3f&&player.x<-20.7f&&player.z>-22.3f&&player.z<-5.7f;
            var view=camera.transform.position-player;
            Vector3 target=player+Vector3.up*1.2f;
            Vector3 direction=target-camera.transform.position;
            float distance=direction.magnitude;
            var ray=new Ray(camera.transform.position,direction.normalized);
            // Renderer bounds remain available while disabled. Reevaluate every frame so cutaways
            // restore on exit, and reveal a player behind the building as well as one inside it.
            ClinicVisibility(clinicRoof,!inside&&!ClinicOccludes(clinicRoof,ray,distance));
            ClinicVisibility(clinicSouth,(!inside||view.z>=0)&&!ClinicOccludes(clinicSouth,ray,distance));
            ClinicVisibility(clinicNorth,(!inside||view.z<=0)&&!ClinicOccludes(clinicNorth,ray,distance));
            ClinicVisibility(clinicWest,(!inside||view.x>=0)&&!ClinicOccludes(clinicWest,ray,distance));
            ClinicVisibility(clinicEast,(!inside||view.x<=0)&&!ClinicOccludes(clinicEast,ray,distance));
            ClinicVisibility(clinicRoomLabels,inside&&clinicNorth.Count>0&&clinicNorth[0].enabled);
        }
        static bool ClinicOccludes(List<Renderer> renderers,Ray ray,float targetDistance)
        {
            foreach(var renderer in renderers)
                if(renderer&&renderer.bounds.IntersectRay(ray,out float at)&&at<targetDistance-.15f)return true;
            return false;
        }
        static void ClinicVisibility(List<Renderer> renderers,bool visible)
        { foreach(var r in renderers)if(r)r.enabled=visible; }
    }
}
