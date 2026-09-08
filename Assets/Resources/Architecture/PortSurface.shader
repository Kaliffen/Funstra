Shader "Funstra/PortSurface"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) _Paving ("Stone joints", Float) = 0 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        fixed4 _Color;
        float _Paving;
        struct Input { float3 worldPos; };
        float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
        float noise(float2 p)
        {
            float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
            return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
        }
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float2 p = input.worldPos.xz;
            float variation = noise(p*.47);
            float2 grid = p / float2(1.4, .85);
            grid.x += fmod(abs(floor(grid.y)), 2) * .5;
            float2 edge = min(frac(grid), 1 - frac(grid));
            float2 aa = max(fwidth(grid), .008);
            float joints = min(smoothstep(.012, .012 + aa.x, edge.x), smoothstep(.012, .012 + aa.y, edge.y));
            float stone=hash(floor(grid));
            float damp=smoothstep(.58,.79,noise(p*.21+31));
            // Uneven stone batches, dirt-filled joints and broad damp patches stay legible at play height.
            float wear=(.88+variation*.19)*lerp(1,.88+stone*.2,_Paving);
            output.Albedo = _Color.rgb * wear * lerp(1,lerp(.48,1,joints),_Paving)*(1-damp*.19);
            output.Smoothness = lerp(.09,.05,_Paving)+damp*.27;
            output.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
