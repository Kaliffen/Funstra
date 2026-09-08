Shader "Funstra/PortArchitecture"
{
    Properties
    {
        _Color ("Material tint", Color) = (1,1,1,1)
        _VertexColor ("Use authored colors", Float) = 1
        _Weathering ("Exposure to salt and soot", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        #pragma multi_compile_instancing
        fixed4 _Color;
        float _VertexColor, _Weathering;
        struct Input { float4 color : COLOR; float3 worldPos; float3 worldNormal; };
        float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
        float noise(float2 p)
        {
            float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
            return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
        }
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            float3 material=lerp(_Color.rgb,input.color.rgb,saturate(_VertexColor));
            float glass=(1-input.color.a)*_VertexColor;
            float roof=smoothstep(.65,.88,abs(input.worldNormal.y));
            float2 face=abs(input.worldNormal.x)>.5?input.worldPos.zy:input.worldPos.xy;
            face=lerp(face,input.worldPos.xz,roof);
            // Meter-scale runoff and a ragged salt tide express exposure, not a screen filter.
            float runoff=smoothstep(.4,.8,noise(face*float2(3.2,.17)));
            float stain=noise(face*.63+19);
            float tide=(1-smoothstep(.7,2.2,input.worldPos.y+noise(face*1.7)*.8))*(1-roof);
            float chips=smoothstep(.66,.84,noise(face*6.5));
            // Fine chips fade when they are smaller than a pixel rather than glittering in motion.
            chips*=1-saturate(max(fwidth(face.x),fwidth(face.y))*6);
            float exposure=_Weathering*(1-glass);
            float3 worn=material*(.92+stain*.15-runoff*.24*(1-roof)-tide*.25);
            worn=lerp(worn,float3(.57,.59,.54),tide*stain*.22);
            worn=lerp(worn,float3(.31,.23,.17),runoff*roof*.15);
            worn+=chips*.075;
            output.Albedo=lerp(material,worn,exposure);
            output.Metallic=glass*.25+roof*(1-glass)*.12;
            output.Smoothness=lerp(.12+roof*stain*.19,.56,glass);
            // Occupied rooms are scarce; the clinic's warm light is a deliberate destination.
            float3 cell = floor(input.worldPos / float3(3, 3, 3));
            float occupied = step(.89, frac(sin(dot(cell, float3(12.9898, 78.233, 37.719))) * 43758.5453));
            output.Emission = float3(.72, .33, .09) * glass * occupied * .55;
            output.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
