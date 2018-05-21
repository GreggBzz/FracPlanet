Shader "Custom/Splatmap" {
	Properties{
	_Color("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_Specular("Specular", Color) = (0.0, 0.0, 0.0, 0.0)
	_Smoothness("Smoothness", Range(0,1)) = 0.0
	_Control("Control Splat Elevation (RGBA)", 2D) = "white" {}
    _Special("Special Terrain Properties", 2D) = "white" {}
	_Texture1("Layer 1 (R)", 2D) = "white" {}
	_Normal1("Normal 1 (R)", 2D) = "bump"  {}
	_Texture2("Layer 2 (G)", 2D) = "white" {}
	_Normal2("Normal 2 (G)", 2D) = "bump"  {}
	_Texture3("Layer 3 (B)", 2D) = "white" {}
	_Normal3("Normal 3 (B)", 2D) = "bump"  {}
	_Texture4("Layer 4 (Black)", 2D) = "white" {}
	_Normal4("Normal 4 (Black)", 2D) = "bump"  {}
	_Texture5("Layer 5 (R)", 2D) = "white" {}
	_Normal5("Normal 5 (R)", 2D) = "bump"  {}
	_Texture6("Layer 6 (G)", 2D) = "white" {}
	_Normal6("Normal 6 (G)", 2D) = "bump"  {}
}
		SubShader{
		Tags{
		"SplatCount" = "6"
		"BumpCount" = "6"
		"RenderType" = "Opaque"
	}
	
	LOD 300

		CGPROGRAM
        #pragma surface surf StandardSpecular addshadow finalcolor:setColor
        #pragma target 5.0

		struct Input {
		  float2 uv4_Control;
		  float2 uv3_Special;
		  float2 uv_Texture1;
		  float2 uv_Texture2;
		  float2 uv_Texture3;
		  float2 uv_Texture4;
		  float2 uv_Texture5;
		  float2 uv_Texture6;
		  float2 uv_Texture7;
	   };

	fixed4 _Color;
	fixed4 _Specular;
	half _Smoothness;

	sampler2D _Control;
	sampler2D _Special;
	sampler2D _Texture1;
	sampler2D _Texture2;
	sampler2D _Texture3;
	sampler2D _Texture4;
	sampler2D _Texture5;
	sampler2D _Texture6;
	sampler2D _Normal1;
	sampler2D _Normal2;
	sampler2D _Normal3;
	sampler2D _Normal4;
	sampler2D _Normal5;
	sampler2D _Normal6;

	void setColor(Input IN, SurfaceOutputStandardSpecular o, inout fixed4 color)
	{
		color *= _Color;
	}


	void surf(Input i, inout SurfaceOutputStandardSpecular o) {
		fixed4 splatmapMask = tex2D(_Control, i.uv4_Control);
		fixed3 combinedColor;
		fixed3 combinedNormal;
    	if (i.uv4_Control.x == 0)
	    {
            // Calculate and combine the diffuse color.
            combinedColor = tex2D(_Texture1, i.uv_Texture1).rgb * splatmapMask.r;
            combinedColor += tex2D(_Texture2, i.uv_Texture2).rgb * splatmapMask.g;
            combinedColor += tex2D(_Texture3, i.uv_Texture3).rgb * splatmapMask.b;
            combinedColor += tex2D(_Texture4, i.uv_Texture4).rgb * (1 - splatmapMask.r - splatmapMask.g - splatmapMask.b);
            // Calculate normal.
            float3 nRed = UnpackNormal(tex2D(_Normal1, i.uv_Texture1));
            float3 nGreen = UnpackNormal(tex2D(_Normal2, i.uv_Texture2));
            float3 nBlue = UnpackNormal(tex2D(_Normal3, i.uv_Texture3));
            float3 nBlack = UnpackNormal(tex2D(_Normal4, i.uv_Texture4));
            // Combine the normal.
            combinedNormal = nBlack.rgb*(1 - splatmapMask.r - splatmapMask.g - splatmapMask.b);
            combinedNormal += nRed.rgb*splatmapMask.r;
            combinedNormal += nGreen.rgb*splatmapMask.g;
            combinedNormal += nBlue.rgb*splatmapMask.b;
			if (i.uv3_Special.x != 0)
		    {
		        combinedColor = (combinedColor * (1 - i.uv3_Special.x)) + tex2D(_Texture4, i.uv_Texture4).rgb * i.uv3_Special.x;
			    combinedNormal = (combinedNormal * (1 - i.uv3_Special.x)) + UnpackNormal(tex2D(_Normal4, i.uv_Texture4)).rgb * i.uv3_Special.x;
		    }
		}
	    else
        {
            combinedColor = tex2D(_Texture4, i.uv_Texture4).rgb * (1 - splatmapMask.r - splatmapMask.g - splatmapMask.b);
            combinedColor += tex2D(_Texture5, i.uv_Texture5).rgb * splatmapMask.r;
            combinedColor += tex2D(_Texture6, i.uv_Texture6).rgb * splatmapMask.g;
            float3 nBlack = UnpackNormal(tex2D(_Normal4, i.uv_Texture4));
            float3 nRed = UnpackNormal(tex2D(_Normal5, i.uv_Texture5));
            float3 nGreen = UnpackNormal(tex2D(_Normal6, i.uv_Texture6));
            combinedNormal = nBlack.rgb*(1 - splatmapMask.r - splatmapMask.g - splatmapMask.b);
            combinedNormal += nRed.rgb*splatmapMask.r;
            combinedNormal += nGreen.rgb*splatmapMask.g;
			if (i.uv3_Special.x != 0)
		    {
		        combinedColor = (combinedColor * (1 - i.uv3_Special.x)) + tex2D(_Texture4, i.uv_Texture4).rgb * i.uv3_Special.x;
			    combinedNormal = (combinedNormal * (1 - i.uv3_Special.x)) + UnpackNormal(tex2D(_Normal4, i.uv_Texture4)).rgb * i.uv3_Special.x;
		    }
		}

        o.Smoothness = _Smoothness;
        o.Specular = _Specular;
        o.Albedo = combinedColor;
        o.Normal = clamp(combinedNormal, -1, 1);
        o.Alpha = 0.0;
	}
	ENDCG
    }
    FallBack "Diffuse"
}
