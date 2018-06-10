Shader "Custom/SimpleRockBump" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 300

CGPROGRAM
#pragma surface surf Lambert
#pragma multi_compile_instancing

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
};
sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG  
}

FallBack "Legacy Shaders/Diffuse"
}
