using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Funstra
{
    public sealed partial class CityArt
    {
        public static readonly Color Mint = Hex("70E3C0"), Amber = Hex("FFCE79"), Blue = Hex("6A9DFF"), Red = Hex("FF6973");
        public readonly CityNavigation Nav = new CityNavigation();
        public readonly List<GameObject> Targets = new List<GameObject>();
        public readonly List<Vector3> Hides = new List<Vector3>();
        public readonly List<Bounds> Buildings = new List<Bounds>();
        public readonly List<Renderer[]> BuildingRenderers = new List<Renderer[]>();
        readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        Transform root;
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
        public Material Mat(Color c, bool glow = false)
        {
            string key = ColorUtility.ToHtmlStringRGBA(c) + glow;
            if (materials.TryGetValue(key, out var found)) return found;
            var m = new Material(Shader.Find("Standard"));
            m.color = c; m.SetFloat("_Glossiness", .18f);
            if (glow) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", c * .75f); }
            materials[key] = m; return m;
        }
        public GameObject Shape(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color, Transform parent = null, bool solid = false, bool glow = false)
        {
            var o = GameObject.CreatePrimitive(type); o.name = name;
            o.transform.SetParent(parent == null ? root : parent, false);
            o.transform.localPosition = pos; o.transform.localScale = scale;
            o.GetComponent<Renderer>().sharedMaterial = Mat(color, glow);
            var col = o.GetComponent<Collider>();
            if (!solid) { col.enabled = false; Object.Destroy(col); }
            else o.layer = 8;
            return o;
        }
        public GameObject Box(string name, Vector3 pos, Vector3 size, Color c, Transform parent = null, bool solid = false, bool glow = false)
            => Shape(name, PrimitiveType.Cube, pos, size, c, parent, solid, glow);
        readonly List<Collider> looseProps=new List<Collider>();
        public GameObject Prop(string name,Vector3 p,Vector3 size,Color color,Transform parent=null)
        { var go=Box(name,p,size,color,parent,true);looseProps.Add(go.GetComponent<Collider>());return go; }
        public void RefreshProps()
        { Physics.SyncTransforms();Nav.Props.Clear();foreach(var c in looseProps)if(c&&c.gameObject.activeInHierarchy)Nav.Props.Add(c.bounds); }
        public void Solid(string name, Vector3 p, Vector3 s, Color c)
        { Box(name, p, s, c, null, true); Nav.Obstacles.Add(new Bounds(p, s)); }
        public void Sign(string text, Vector3 p, Color c, float size = .38f, Transform parent = null)
        {
            var go = new GameObject(text); go.transform.SetParent(parent == null ? root : parent, false); go.transform.localPosition = p;
            var t = go.AddComponent<TextMesh>(); t.text = text; t.fontSize = 64; t.characterSize = size;
            t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.color = c;
            go.transform.localRotation = Quaternion.identity;
        }
        public Transform Human(string name, Vector3 p, Color coat, bool officer = false)
        {
            var o = new GameObject(name); o.transform.SetParent(root); o.transform.position = p;
            var skin = Hex(officer ? "C69678" : "D6AC91");
            Box("Coat", new Vector3(0, 1.08f, 0), new Vector3(.64f, .72f, .37f), coat, o.transform);
            Shape("Head", PrimitiveType.Sphere, new Vector3(0, 1.7f, 0), new Vector3(.44f, .48f, .43f), skin, o.transform);
            Box("Hair", new Vector3(0, 1.89f, -.03f), new Vector3(.45f, .15f, .4f), Hex("222633"), o.transform);
            for (int side = -1; side <= 1; side += 2)
            {
                var leg = new GameObject(side == -1 ? "Left leg" : "Right leg"); leg.transform.SetParent(o.transform, false); leg.transform.localPosition = new Vector3(side * .18f, .77f, 0);
                Box("Trouser", new Vector3(0, -.3f, 0), new Vector3(.23f, .6f, .27f), Hex("252D41"), leg.transform);
                Box("Shoe", new Vector3(0, -.69f, .075f), new Vector3(.25f, .16f, .42f), Hex("141B27"), leg.transform);
                var arm = new GameObject(side == -1 ? "Left arm" : "Right arm"); arm.transform.SetParent(o.transform, false); arm.transform.localPosition = new Vector3(side * .43f, 1.37f, 0);
                Box("Sleeve", new Vector3(0, -.25f, 0), new Vector3(.2f, .52f, .24f), coat, arm.transform);
                Box("Hand", new Vector3(0, -.53f, 0), new Vector3(.18f, .16f, .2f), skin, arm.transform);
            }
            if (officer)
            {
                Box("Cap", new Vector3(0, 1.98f, 0), new Vector3(.52f, .14f, .51f), Hex("253756"), o.transform);
                Box("Badge", new Vector3(.16f, 1.29f, .193f), new Vector3(.12f, .17f, .02f), Amber, o.transform, false, true);
            }
            else Box("Bag strap", new Vector3(.12f, 1.12f, -.22f), new Vector3(.36f, .49f, .14f), Hex("303344"), o.transform);
            return o.transform;
        }
        public static void Animate(Transform human, float phase, float speed)
        {
            float a = Mathf.Sin(phase * 9) * Mathf.Min(speed / 4, 1) * 32;
            human.Find("Left leg").localRotation = Quaternion.Euler(a, 0, 0);
            human.Find("Right leg").localRotation = Quaternion.Euler(-a, 0, 0);
            human.Find("Left arm").localRotation = Quaternion.Euler(-a * .8f, 0, 0);
            human.Find("Right arm").localRotation = Quaternion.Euler(a * .8f, 0, 0);
        }
        public void Build()
        {
            Resources.LoadAll<Material>("Rendering");
            root = new GameObject("FUNSTRA / Old Port").transform;
            Box("Foundation", new Vector3(0, -.8f, 0), new Vector3(103, 1.2f, 105), Hex("263548"), null, true);
            Box("Paving", new Vector3(0, -.13f, 0), new Vector3(96, .22f, 96), Hex("68727D"), null, true);
            Box("Canal", new Vector3(0, -.05f, 52), new Vector3(125, .1f, 12), Hex("234F67"));
            for (int i = 0; i < 50; i++) Box("Water shimmer", new Vector3(-60 + (i * 7.7f % 120), .02f, 48 + (i % 5) * 1.7f), new Vector3(2.7f, .02f, .08f), Hex("5B8190"));
            Box("High street", new Vector3(0, .005f, 0), new Vector3(10, .08f, 95), Hex("303B4D"));
            foreach (float z in new[] { -39f, -13f, 13f, 39f })
            {
                Box("Cross street", new Vector3(0, .01f, z), new Vector3(94, .08f, 8), Hex("303B4D"));
                for (int x = -42; x <= 42; x += 7) if (Mathf.Abs(x) > 5) Box("Road dash", new Vector3(x, .062f, z), new Vector3(2.7f, .018f, .14f), Hex("A5A395"));
                for (int x = -4; x <= 4; x += 2) { Box("Crosswalk", new Vector3(x, .07f, z + 4.8f), new Vector3(1, .02f, 2), Hex("C3BBA6")); Box("Crosswalk", new Vector3(x, .07f, z - 4.8f), new Vector3(1, .02f, 2), Hex("C3BBA6")); }
            }
            for (int z = -32; z <= 32; z += 6) Box("Center dash", new Vector3(0, .062f, z), new Vector3(.16f, .018f, 2.5f), Hex("A5A395"));
            // Each block has a gap between buildings and a rear escape alley.
            Building(-17, -25, 15, 16, 6.7f, "PAWN & CO.", Hex("507C78"));
            Building(-36, -25, 13, 15, 8.2f, "ROOMS", Hex("BA7869"));
            Building(17, -25, 15, 16, 7.5f, "ARCADIA", Hex("766787"));
            Building(36, -25, 13, 16, 5.1f, "NOODLE BAR", Hex("B9926E"));
            Building(-18, 0, 16, 16, 5.4f, "VICO'S", Hex("64828C"));
            Building(-37, 0, 12, 16, 4.2f, "AUTO PARTS", Hex("BD8265"));
            Building(18, 0, 16, 16, 7.9f, "HOTEL LUNA", Hex("AC797D"));
            Building(37, 0, 12, 16, 6.2f, "LAUNDRY", Hex("689590"));
            Building(-18, 26, 16, 16, 6, "THE ANCHOR", Hex("BB986E"));
            Building(-37, 26, 12, 16, 8.4f, "PORT HOUSE", Hex("697C91"));
            Building(19, 26, 18, 16, 5.2f, "NORTH DOCK", Hex("68858C"));
            Building(39, 26, 10, 16, 6.5f, "STORAGE", Hex("8C7D82"));
            foreach (float x in new[] { -46f, 46f }) for (int z = -32; z <= 32; z += 16) Tree(new Vector3(x, 0, z));
            for (int z = -32; z <= 34; z += 22) { Lamp(new Vector3(-6.7f, 0, z)); Lamp(new Vector3(6.7f, 0, z + 6)); }
            for (int x = -38; x <= 38; x += 19) { Lamp(new Vector3(x, 0, -44)); Lamp(new Vector3(x, 0, 44)); }
            foreach (Vector3 p in new[] { new Vector3(-28,0,-16), new Vector3(28,0,-16), new Vector3(-28,0,10), new Vector3(29,0,10), new Vector3(-28,0,36), new Vector3(32,0,36) })
            {
                Hides.Add(p); Solid("Hide / recycling bins", p + new Vector3(0,.65f,0), new Vector3(1.8f,1.43f,1.1f), Hex("2F6B5E"));
                Box("Bin lid", p + new Vector3(0,1.34f,0), new Vector3(1.8f,.13f,1.1f), Hex("73A48B"));
                Ring("Safe cover", p, 1.6f, Mint);
            }
            Car(new Vector3(3,0,-24), Hex("DDBB74"), false);
            Car(new Vector3(-3,0,24), Hex("406879"), true);
            Car(new Vector3(18,0,-40), Hex("AE6965"), false, 90);
            Car(new Vector3(-18,0,12), Hex("8B9D9C"), false, 90);
            for (int i = 0; i < 3; i++)
            {
                Vector3 p = Jobs.All[i].position;
                var target = Prop(Jobs.All[i].item, p + Vector3.up * .6f, new Vector3(.85f,.85f,.6f), Amber);
                Box("Clasp", new Vector3(0,0,.51f), new Vector3(.16f,.25f,.04f), Hex("4F4448"), target.transform);
                Targets.Add(target); Ring("Job marker", p, 1.2f, Amber);
            }
            // Mara's curbside stall leaves space for approaching from the street.
            Solid("Pawn stall", Jobs.Mara + new Vector3(0,.65f,1.4f), new Vector3(3.2f,1.3f,.9f), Hex("394C59"));
            Box("Striped awning", Jobs.Mara + new Vector3(0,2.8f,1.3f), new Vector3(4,.18f,2.8f), Mint);
            for (int i = -1; i <= 1; i += 2) Box("Awning pole", Jobs.Mara + new Vector3(i*1.8f,1.4f,1.3f), new Vector3(.1f,2.8f,.1f), Hex("D8C6AB"));
            Human("Mara", Jobs.Mara + new Vector3(0,0,.1f), Hex("DD9576"));
            Ring("Mara's marker", Jobs.Mara, 2.2f, Mint);
            Sign("MARA / FENCE", Jobs.Mara + new Vector3(0,3.4f,1.2f), Mint, .26f);
            for (int x = -46; x <= 46; x += 4) { Box("Quay bollard", new Vector3(x,.6f,46), new Vector3(.32f,1.2f,.32f), Hex("27374A")); }
            Solid("West boundary", new Vector3(-49,1,0), new Vector3(2,2,98), Hex("475461"));
            Solid("East boundary", new Vector3(49,1,0), new Vector3(2,2,98), Hex("475461"));
            Solid("North boundary", new Vector3(0,.5f,47), new Vector3(98,1,1), Hex("4F626C"));
            Solid("South boundary", new Vector3(0,1,-47), new Vector3(98,2,2), Hex("475461"));
            RefreshProps();Nav.Bake();
        }
        void Building(float x, float z, float w, float d, float h, string sign, Color c)
        {
            var group = new GameObject(sign); group.transform.SetParent(root); group.transform.position = new Vector3(x,0,z);
            var b = new Bounds(new Vector3(x,h/2,z), new Vector3(w,h,d)); Nav.Obstacles.Add(b); Buildings.Add(b);
            Box("Masonry", new Vector3(0,h/2,0), new Vector3(w,h,d), c, group.transform, true);
            Box("Plinth", new Vector3(0,.3f,0), new Vector3(w+.15f,.6f,d+.15f), Hex("455260"), group.transform);
            Box("Roof cornice", new Vector3(0,h+.1f,0), new Vector3(w+.5f,.35f,d+.5f), Hex("D4B89A"), group.transform);
            Box("Flat roof", new Vector3(0,h+.3f,0), new Vector3(w-.7f,.18f,d-.7f), Hex("3E4A60"), group.transform);
            Box("Rooftop unit", new Vector3(w*.22f,h+.9f,d*.2f), new Vector3(2.8f,1.3f,2.2f), Hex("7E8991"), group.transform);
            for (float y = 2.4f; y < h-1; y += 2.35f) for (float xx = -w/2+1.8f; xx < w/2-1; xx += 2.7f)
            {
                bool lit = Mathf.RoundToInt(xx+y+x) % 3 != 0;
                foreach (float zz in new[] { -d/2-.02f, d/2+.02f })
                    Box("Window", new Vector3(xx,y,zz), new Vector3(1.3f,1.3f,.08f), lit ? Hex("E8BC87") : Hex("334C63"), group.transform, false, lit);
            }
            for (float zz = -d/2+2; zz < d/2-1; zz += 3) foreach(float xx in new[] { -w/2-.02f, w/2+.02f })
                Box("Side window", new Vector3(xx,3.1f,zz), new Vector3(.08f,1.4f,1.3f), Hex("CFA680"), group.transform, false, true);
            Box("Shopfront", new Vector3(0,1.5f,-d/2-.05f), new Vector3(w*.7f,2.1f,.16f), Hex("233E51"), group.transform);
            Box("Door", new Vector3(-w*.23f,1.1f,-d/2-.15f), new Vector3(1.5f,2.2f,.12f), Hex("B19D82"), group.transform);
            Box("Signboard", new Vector3(0,3.1f,-d/2-.22f), new Vector3(w*.85f,.85f,.25f), Hex("293648"), group.transform);
            Sign(sign, new Vector3(0,3.12f,-d/2-.37f), sign == "ARCADIA" ? Hex("F5A1BE") : Amber, .28f, group.transform);
            BuildingRenderers.Add(group.GetComponentsInChildren<Renderer>());
        }
        public GameObject Ring(string name, Vector3 p, float radius, Color color)
        {
            var o = new GameObject(name); o.transform.SetParent(root); o.transform.position = p;
            var line = o.AddComponent<LineRenderer>(); line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = color; line.startWidth = line.endWidth = .065f;
            line.positionCount = 49; line.useWorldSpace = false;
            for (int i = 0; i <= 48; i++) { float a = i * Mathf.PI / 24; line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,.12f,Mathf.Sin(a)*radius)); }
            return o;
        }
        void Tree(Vector3 p)
        {
            Shape("Trunk", PrimitiveType.Cylinder, p + Vector3.up*1.1f, new Vector3(.3f,1.1f,.3f), Hex("71675C"));
            Shape("Tree crown", PrimitiveType.Sphere, p + Vector3.up*3.1f, new Vector3(3.3f,3.8f,3.1f), Hex("477F73"));
            Solid("Planter", p + Vector3.up*.15f, new Vector3(2.5f,.3f,2.5f), Hex("939084"));
        }
        void Lamp(Vector3 p)
        {
            Nav.Obstacles.Add(new Bounds(p+Vector3.up*2.1f,new Vector3(.2f,4.2f,.2f)));
            Shape("Streetlight pole", PrimitiveType.Cylinder, p + Vector3.up*2.1f, new Vector3(.13f,2.1f,.13f), Hex("2B3846"),null,true);
            Box("Streetlight", p + Vector3.up*4.3f, new Vector3(.9f,.22f,.6f), Amber, null, false, true);
            var l = new GameObject("Warm pool").AddComponent<Light>(); l.transform.SetParent(root); l.transform.position = p + Vector3.up*3.5f;
            l.type = LightType.Point; l.color = Hex("FFD099"); l.range = 8; l.intensity = 1.4f; l.shadows = LightShadows.None;
        }
        void Car(Vector3 p, Color c, bool police, float angle = 0)
        {
            var o = new GameObject(police ? "Parked police car" : "Parked car"); o.transform.SetParent(root); o.transform.position = p; o.transform.rotation = Quaternion.Euler(0,angle,0);
            Box("Car body", new Vector3(0,.7f,0), new Vector3(1.8f,.65f,3.7f), c, o.transform);
            Box("Cabin", new Vector3(0,1.25f,-.25f), new Vector3(1.55f,.6f,1.9f), Hex("273E55"), o.transform);
            for(int s=-1;s<=1;s+=2) for(int t=-1;t<=1;t+=2)
            {
                var wheel = Shape("Wheel",PrimitiveType.Cylinder,new Vector3(s*.9f,.4f,t*1.14f),new Vector3(.64f,.16f,.64f),Hex("192331"),o.transform);
                wheel.transform.localRotation = Quaternion.Euler(0,0,90);
            }
            for(int s=-1;s<=1;s+=2) Box("Headlight",new Vector3(s*.6f,.72f,1.87f),new Vector3(.4f,.23f,.05f),Amber,o.transform,false,true);
            if(police) { Box("Light bar",new Vector3(0,1.64f,0),new Vector3(1,.15f,.25f),Blue,o.transform,false,true); }
            RegisterTraffic(o.transform,p,angle);
            /* Previous stationary collider
            var size = angle == 0 ? new Vector3(2,1.5f,4) : new Vector3(4,1.5f,2);
            var col = new GameObject("Car collision"); col.transform.SetParent(root); col.transform.position = p + Vector3.up*.75f; col.layer = 8;
            col.AddComponent<BoxCollider>().size = size; Nav.Obstacles.Add(new Bounds(p+Vector3.up*.75f,size)); */
        }
    }
}
