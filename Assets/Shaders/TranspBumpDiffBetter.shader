Shader "Transparent/Diff Nrm Spec Gloss (fixed)" {
Properties {
	    _Color ("Main Color", Color) = (1,1,1,1)
	    _SpecColor ("Specular Map (RGB)", 2D) = "white"{}
	     _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}
 
	SubShader {
	    Tags {"RenderType"="Transparent" "Queue"="Transparent"}
	    // Render into depth buffer only
	    Pass {
	        ColorMask 0
	    }
	    // Render normally
	    Pass {
	        ZWrite Off
	        Blend SrcAlpha OneMinusSrcAlpha
	        ColorMask RGB
	        Material {
	            Diffuse [_Color]
	            Ambient [_Color]
            	Shininess [_Shininess]
	            Specular [_SpecColor]
	        }
	        Lighting On
			SeparateSpecular On
	        SetTexture [_MainTex] {
	            Combine texture * primary DOUBLE, texture * primary
	        } 
	    }
	    
	    CGPROGRAM
#pragma surface surf BlinnPhong


sampler2D _MainTex;
// sampler2D _BumpMap;
sampler2D _SpecMap;
fixed4 _Color;
half _Shininess;

struct Input {
	float2 uv_MainTex;
	// float2 uv_BumpMap;
	float2 uv_SpecMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 specTex = tex2D(_SpecMap, IN.uv_SpecMap);
	o.Albedo = tex.rgb * _Color.rgb;
	o.Gloss = specTex.r;
	o.Alpha = tex.a;
	o.Specular = _Shininess * specTex.g;
	//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
	    
	}
}