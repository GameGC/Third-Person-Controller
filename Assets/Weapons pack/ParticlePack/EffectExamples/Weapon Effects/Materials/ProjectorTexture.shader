Shader "Projector/Projector Texture" {
	Properties {
        _Color ("Color", Color) = (1,1,1,1)
	    _MainTex ("Albedo", 2D) = "white" { }
		_MetallicGlossMap ("Metallic", 2D) = "white" { }
		[Gamma] _Metallic ("Metallic", Range(0,1)) = 0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_BumpScale ("Bump Scale", Float) = 1
		_BumpMap ("Normal Map", 2D) = "bump" { }
    	
		
		_DetailAlbedoMap ("Detail Albedo x2", 2D) = "white" { }
		_DetailNormalMapScale ("Normal Map x2 Scale", Float) = 1
		_DetailNormalMap ("Normal Map x2", 2D) = "bump" { }
    }
    SubShader {
		ZWrite Off
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
		Offset -1, -1
    	Tags {"Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector"="False" }

    	
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:fProjUVs alpha:fade
        
        #include "UnityCG.cginc"

        fixed4 _Color;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _BumpScale;
        sampler2D _MetallicGlossMap;
        half _Metallic;
        half _Glossiness;

		sampler2D _DetailAlbedoMap;
        sampler2D _DetailNormalMap;
        half _DetailNormalMapScale;
        
        uniform float4x4 unity_Projector;

        struct Input {
            float2 uv_MainTex;
            float4 posProj : TEXCOORD0; // position in projector space
        };

        void fProjUVs (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.posProj = mul(unity_Projector, v.vertex);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            if (IN.posProj.x < -0) discard;
			if (IN.posProj.x > 1) discard;
			if (IN.posProj.y < -0) discard;
			if (IN.posProj.y > 1) discard;
			if (IN.posProj.z < -1) discard;
			if (IN.posProj.z > 1) discard;

            const float4 projection =UNITY_PROJ_COORD(IN.posProj);
            float4 mainTex1 = tex2D(_MainTex , projection);
        	float4 mainTex2 = tex2D(_DetailAlbedoMap , projection);

        	float secondaryBlend = mainTex2.a * mainTex1.a;

        	
            o.Albedo = (mainTex1.rgb *_Color) + (mainTex2.rgb * secondaryBlend);
        	o.Alpha = mainTex1.a;
        	o.Metallic = tex2D(_MetallicGlossMap , projection) * _Metallic;
        	o.Smoothness = _Glossiness;

        	half3 normal1 = UnpackNormal(tex2D(_BumpMap , projection)) * _BumpScale;
        	half3 normal2 = UnpackNormal(tex2D(_DetailNormalMap , projection)) * _DetailNormalMapScale;
            o.Normal = normal1 + normal2;
        }
        ENDCG
    }
}


