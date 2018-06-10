Shader "Custom/SimpleGrassStatic" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Illum ("Illumin (A)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.2
}
 
SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    Cull Off

CGPROGRAM
#pragma target 5.0
#pragma surface surf Lambert alphatest:_Cutoff addshadow
#pragma multi_compile_instancing
 
sampler2D _MainTex;
sampler2D _Illum;
fixed4 _Color;
 
struct Input {
    float2 uv_MainTex;
    float2 uv_Illum;
};

 void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * (_Color + unity_AmbientSky) * 0.5;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG
}
Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
