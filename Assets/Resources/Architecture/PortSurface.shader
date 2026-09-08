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
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float2 p = input.worldPos.xz;
            float variation = sin(p.x * .61 + p.y * .23) * sin(p.y * .73 - p.x * .19);
            float2 grid = p / float2(1.4, .85);
            grid.x += fmod(abs(floor(grid.y)), 2) * .5;
            float2 edge = min(frac(grid), 1 - frac(grid));
            float2 aa = max(fwidth(grid), .008);
            float joints = min(smoothstep(.012, .012 + aa.x, edge.x), smoothstep(.012, .012 + aa.y, edge.y));
            output.Albedo = _Color.rgb * (1 + variation * .055) * lerp(1, lerp(.83, 1, joints), _Paving);
            output.Smoothness = lerp(.12, .05, _Paving);
            output.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
