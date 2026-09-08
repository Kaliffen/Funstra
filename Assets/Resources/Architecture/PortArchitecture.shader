Shader "Funstra/PortArchitecture"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        #pragma multi_compile_instancing
        struct Input { float4 color : COLOR; float3 worldPos; };
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            // Subtle broad masonry variation and a darker splash course; no texture samples.
            float wear = sin(input.worldPos.x * 1.7 + input.worldPos.z * .8) * sin(input.worldPos.y * 2.2);
            float baseShade = lerp(.76, 1, saturate(input.worldPos.y / 1.6));
            output.Albedo = input.color.rgb * (1 + wear * .035) * baseShade;
            float glass = 1 - step(.18, max(input.color.r, max(input.color.g, input.color.b)));
            output.Metallic = glass * .25;
            output.Smoothness = lerp(.16, .56, glass);
            // A few occupied windows catch warm interior light, leaving most glazing dark.
            float3 cell = floor(input.worldPos / float3(3, 3, 3));
            float occupied = step(.68, frac(sin(dot(cell, float3(12.9898, 78.233, 37.719))) * 43758.5453));
            output.Emission = float3(.62, .29, .08) * glass * occupied * .45;
            output.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
