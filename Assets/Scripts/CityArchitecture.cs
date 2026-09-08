using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Funstra
{
    public sealed partial class CityArt
    {
        static readonly Dictionary<string, Mesh> architectureMeshes = new Dictionary<string, Mesh>();
        static Material architectureMaterial;
        readonly List<Transform> architectureGroups = new List<Transform>();
        readonly List<int> architectureBuildingIndices = new List<int>();

        /// <summary>Place an opaque exterior. Center is the footprint center at ground elevation.
        /// Width/depth are local axes before rotation; the renderer and solid navigation bounds agree.
        /// Inhabitable rooms need explicit wall/door construction, never an opaque exterior over them.</summary>
        public GameObject Architecture(string kind, Vector3 center, float width, float depth, float height, float rotation = 0)
        {
            if (width <= 0 || depth <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            // A multi-deck parking prototype must not be flattened into a tiny workshop.
            bool workshop = kind == "garage" && height < 12;
            var mesh = workshop ? WorkshopMesh(width, depth, height) : ArchitectureMesh(kind);
            if (!architectureMaterial)
            {
                var shader = Resources.Load<Shader>("Architecture/PortArchitecture");
                if (!shader) throw new InvalidOperationException("Missing PortArchitecture shader resource");
                architectureMaterial = new Material(shader) { name = "Old Port / shared masonry", enableInstancing = true };
            }
            var group = new GameObject("Architecture / " + kind);
            group.transform.SetParent(root, false);
            group.transform.position = center;
            group.transform.rotation = Quaternion.Euler(0, rotation, 0);
            var facade = new GameObject("Authored facade and roof");
            facade.transform.SetParent(group.transform, false);
            facade.transform.localScale = workshop ? Vector3.one : new Vector3(width, height, depth);
            facade.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = facade.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = architectureMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            group.layer = 8;
            var collider = group.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, height * .5f, 0);
            collider.size = new Vector3(width, height, depth);
            // Explicitly compute the same rotated footprint without depending on Physics.SyncTransforms.
            float radians = rotation * Mathf.Deg2Rad;
            var bounds = new Bounds(center + Vector3.up * (height * .5f), new Vector3(
                Mathf.Abs(Mathf.Cos(radians)) * width + Mathf.Abs(Mathf.Sin(radians)) * depth,
                height, Mathf.Abs(Mathf.Sin(radians)) * width + Mathf.Abs(Mathf.Cos(radians)) * depth));
            Nav.Obstacles.Add(bounds);
            Buildings.Add(bounds);
            BuildingRenderers.Add(new Renderer[] { renderer });
            architectureGroups.Add(group.transform);
            architectureBuildingIndices.Add(Buildings.Count - 1);
            return group;
        }

        // Sign locations are world-space facade positions, like the old labels. The board, text
        // and facade now share cutaway visibility, rather than leaving a floating building name.
        public void BuildingSign(string text, Vector3 position, Color color, float size = .2f)
        {
            int nearest = -1; float distance = float.MaxValue;
            for (int i = 0; i < architectureGroups.Count; ++i)
            {
                var bounds = Buildings[architectureBuildingIndices[i]];
                float candidate = (bounds.ClosestPoint(position) - position).sqrMagnitude;
                if (candidate < distance) { nearest = i; distance = candidate; }
            }
            if (nearest < 0) { Sign(text, position, color, size * .6f); return; }
            var parent = architectureGroups[nearest];
            var local = parent.InverseTransformPoint(position);
            var building = Buildings[architectureBuildingIndices[nearest]];
            // The kit's shopfronts face local -Z. Snap the board to the actual wall, not
            // an old label position inside the roof or hovering in a neighboring court.
            local.z = -parent.GetComponent<BoxCollider>().size.z*.5f-.18f;
            if (parent.name.EndsWith("garage") && building.size.y < 12)
                local.y = Mathf.Max(local.y, Mathf.Min(3.8f,building.size.y*.78f*.75f)+.5f);
            float textSize = Mathf.Min(size * .58f, .14f);
            float boardWidth = Mathf.Max(2, text.Length * textSize * 2.05f);
            Box("Painted shop sign / " + text, local + Vector3.forward * .065f,
                new Vector3(boardWidth, .68f, .1f), Hex("30494C"), parent);
            Sign(text, local, color, textSize, parent);
            BuildingRenderers[architectureBuildingIndices[nearest]] = parent.GetComponentsInChildren<Renderer>();
        }

        static Mesh WorkshopMesh(float width, float depth, float height)
        {
            string key = "workshop/" + width + "/" + depth + "/" + height;
            if (architectureMeshes.TryGetValue(key, out var found) && found) return found;
            var vertices = new List<Vector3>(); var normals = new List<Vector3>();
            var colors = new List<Color32>(); var indices = new List<int>();
            var masonry = Hex("AC9B7E"); var trim = Hex("CFBE9D");
            var roof = Hex("4E686A"); var metal = Hex("677E7D"); var glass = Hex("233F49");
            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
            {
                int offset = vertices.Count; var normal = Vector3.Cross(b-a,c-a).normalized;
                vertices.AddRange(new[]{a,b,c,d});
                for(int i=0;i<4;i++){normals.Add(normal);colors.Add(color);}
                indices.AddRange(new[]{offset,offset+1,offset+2,offset,offset+2,offset+3});
            }
            void Block(Vector3 p, Vector3 size, Color color)
            {
                var lo=p-size*.5f;var hi=p+size*.5f;
                Quad(new Vector3(lo.x,lo.y,lo.z),new Vector3(lo.x,hi.y,lo.z),new Vector3(hi.x,hi.y,lo.z),new Vector3(hi.x,lo.y,lo.z),color);
                Quad(new Vector3(hi.x,lo.y,hi.z),new Vector3(hi.x,hi.y,hi.z),new Vector3(lo.x,hi.y,hi.z),new Vector3(lo.x,lo.y,hi.z),color);
                Quad(new Vector3(lo.x,lo.y,hi.z),new Vector3(lo.x,hi.y,hi.z),new Vector3(lo.x,hi.y,lo.z),new Vector3(lo.x,lo.y,lo.z),color);
                Quad(new Vector3(hi.x,lo.y,lo.z),new Vector3(hi.x,hi.y,lo.z),new Vector3(hi.x,hi.y,hi.z),new Vector3(hi.x,lo.y,hi.z),color);
                Quad(new Vector3(lo.x,hi.y,lo.z),new Vector3(lo.x,hi.y,hi.z),new Vector3(hi.x,hi.y,hi.z),new Vector3(hi.x,hi.y,lo.z),color);
                Quad(new Vector3(lo.x,lo.y,hi.z),new Vector3(lo.x,lo.y,lo.z),new Vector3(hi.x,lo.y,lo.z),new Vector3(hi.x,lo.y,hi.z),color);
            }
            float x=width*.5f,z=depth*.5f,eave=height*.78f;
            float doorWidth=Mathf.Min(4.1f,width*.42f),doorHeight=Mathf.Min(3.8f,eave*.75f);
            Block(new Vector3(0,eave*.5f,z-.14f),new Vector3(width,eave,.28f),masonry);
            for(int side=-1;side<=1;side+=2)
            {
                Block(new Vector3(side*(x-.14f),eave*.5f,0),new Vector3(.28f,eave,depth),masonry);
                float panel=(width-doorWidth)*.5f;
                Block(new Vector3(side*(doorWidth*.5f+panel*.5f),eave*.5f,-z+.14f),new Vector3(panel,eave,.28f),masonry);
                // Applied trim must project beyond the masonry: matching front planes flicker
                // as the camera moves because both materials win the same depth samples.
                Block(new Vector3(side*(doorWidth*.5f+.10f),doorHeight*.5f,-z+.03f),new Vector3(.20f,doorHeight,.12f),trim);
                // High industrial glazing, deliberately a single floor rather than repeated deck stripes.
                Block(new Vector3(side*(x-.03f),eave*.66f,0),new Vector3(.08f,.9f,depth*.60f),glass);
                for(int bar=-1;bar<=1;bar++)
                    Block(new Vector3(side*(x-.01f),eave*.66f,bar*depth*.2f),new Vector3(.1f,1.02f,.10f),trim);
                Block(new Vector3(side*(x-.06f),eave*.5f,-z+.06f),new Vector3(.18f,eave,.18f),trim);
            }
            Block(new Vector3(0,(eave+doorHeight)*.5f,-z+.14f),new Vector3(doorWidth,eave-doorHeight,.28f),masonry);
            Block(new Vector3(0,doorHeight*.5f,-z+.23f),new Vector3(doorWidth,doorHeight,.12f),metal);
            for(float y=.4f;y<doorHeight;y+=.48f)
                Block(new Vector3(0,y,-z+.16f),new Vector3(doorWidth,.035f,.03f),roof);
            Block(new Vector3(0,doorHeight+.10f,-z+.05f),new Vector3(doorWidth+.35f,.2f,.16f),trim);
            // Roof slopes and gable ends give industrial buildings a useful distinct silhouette.
            Quad(new Vector3(-x,eave,-z),new Vector3(-x,eave,z),new Vector3(0,height,z),new Vector3(0,height,-z),roof);
            Quad(new Vector3(0,height,-z),new Vector3(0,height,z),new Vector3(x,eave,z),new Vector3(x,eave,-z),roof);
            Quad(new Vector3(-x,eave,-z),new Vector3(0,height,-z),new Vector3(x,eave,-z),new Vector3(0,eave,-z),masonry);
            Quad(new Vector3(x,eave,z),new Vector3(0,height,z),new Vector3(-x,eave,z),new Vector3(0,eave,z),masonry);
            // Keep ridge end caps behind the gable plane rather than coplanar with it.
            Block(new Vector3(0,height-.05f,0),new Vector3(.16f,.1f,Mathf.Max(.1f,depth-.08f)),trim);
            var mesh = new Mesh { name = "Old Port / one-storey workshop" };
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetColors(colors);mesh.SetTriangles(indices,0);
            mesh.RecalculateBounds();mesh.UploadMeshData(true);architectureMeshes[key]=mesh;return mesh;
        }

        static Mesh ArchitectureMesh(string kind)
        {
            if (architectureMeshes.TryGetValue(kind, out var cached) && cached) return cached;
            string source;
            switch (kind)
            {
                case "rowhouse": source = "Fab_RowhouseTerrace"; break;
                case "retail": source = "Fab_RetailBlock"; break;
                case "loft": source = "Brick_LoftBlock"; break;
                case "market": source = "Market_Hall"; break;
                case "garage": source = "Garage_ParkingDeck"; break;
                case "church": source = "Fab_Church"; break;
                default: throw new ArgumentException("Unknown architecture kind: " + kind, nameof(kind));
            }
            var asset = Resources.Load<TextAsset>("Architecture/" + source);
            if (!asset) throw new InvalidOperationException("Missing architecture mesh: " + source);
            using (var stream = new MemoryStream(asset.bytes, false))
            using (var reader = new BinaryReader(stream))
            {
                int count = reader.ReadInt32();
                if (count < 3 || count % 3 != 0 || count > 100000 || stream.Length != 4L + count * 40L)
                    throw new InvalidDataException("Invalid architecture mesh: " + source);
                var vertices = new Vector3[count];
                var normals = new Vector3[count];
                var colors = new Color32[count];
                var triangles = new int[count];
                var bounds = new Bounds();
                for (int i = 0; i < count; ++i)
                {
                    vertices[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    normals[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    var c = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    // Retain authored material separation; warm/lighten masonry for the existing dusk palette.
                    bool glass = Mathf.Max(c.r, Mathf.Max(c.g, c.b)) < .18f;
                    if (!glass) c = Color.Lerp(c, new Color(.70f, .61f, .48f, 1), .16f);
                    colors[i] = c;
                    triangles[i] = i;
                    if (i == 0) bounds = new Bounds(vertices[i], Vector3.zero); else bounds.Encapsulate(vertices[i]);
                }
                var size = bounds.size;
                if (size.x < .01f || size.y < .01f || size.z < .01f) throw new InvalidDataException("Degenerate architecture bounds");
                var origin = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                for (int i = 0; i < count; ++i)
                {
                    vertices[i] = Vector3.Scale(vertices[i] - origin, new Vector3(1 / size.x, 1 / size.y, 1 / size.z));
                    // Normals use inverse transpose of the normalization transform.
                    normals[i] = Vector3.Scale(normals[i], size).normalized;
                }
                var mesh = new Mesh { name = "Old Port / " + source, indexFormat = count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                mesh.vertices = vertices; mesh.normals = normals; mesh.colors32 = colors; mesh.triangles = triangles;
                mesh.RecalculateBounds();
                mesh.UploadMeshData(true);
                architectureMeshes[kind] = mesh;
                Resources.UnloadAsset(asset);
                return mesh;
            }
        }
    }
}
