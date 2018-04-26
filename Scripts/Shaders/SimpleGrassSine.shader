Shader "Custom/SimpleGrassSine" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _Illum ("Illumin (A)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.2
	_XStrength ("X Strength", Range (0.0, 1.0)) = 0.0
	_XDisp ("X Displacement", Range (-1.0, 1.0)) = 0.0
	_WindFrequency ("Wind Frequency", Range (0, 1.0)) = 0.0
}
 
SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    Cull Off

CGPROGRAM
#pragma target 5.0
#pragma surface surf Lambert alphatest:_Cutoff vertex:vert addshadow
 
sampler2D _MainTex;
sampler2D _Illum;
fixed4 _Color;
half _XStrength;
half _XDisp;
half _WindFrequency;
 
struct Input {
    float2 uv_MainTex;
    float2 uv_Illum;
};

void vert (inout appdata_full v) {
    half4 _waveXmove = half4 (_XStrength, 0.0, 0.0, 0.0);
	half xdisplacement = _XDisp / 3.00;
	half windfrequency = _WindFrequency * 4;
	// Use the v texture coordinate to determine top from base.
	half vheight_multiplier = v.texcoord.y;
	v.vertex.x = (v.vertex.x + (vheight_multiplier * xdisplacement));
	v.vertex.xyz += ((_waveXmove * vheight_multiplier) * sin(_Time.w * windfrequency));
}
 
 void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * (_Color + unity_AmbientSky) * 0.5;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG
}
Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
