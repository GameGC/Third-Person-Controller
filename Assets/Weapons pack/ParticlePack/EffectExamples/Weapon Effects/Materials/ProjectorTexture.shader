Shader "Projector/Projector Texture" {
	Properties {
        _Color ("Color", Color) = (1,1,1,1)
	    _MainTex ("Albedo", 2D) = "grey" { }
		_MetallicGlossMap ("Metallic", 2D) = "white" { }
		[Gamma] _Metallic ("Metallic", Range(0,1)) = 0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_BumpScale ("Bump Scale", Float) = 1
		_BumpMap ("Normal Map", 2D) = "bump" { }
    	
    }
    SubShader {
    	ZWrite Off
    	Tags {"Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector"="False" }
    	Blend SrcAlpha OneMinusSrcAlpha

    	
        CGPROGRAM
        #pragma surface surf Standard vertex:fProjUVs alpha:fade
        #include "UnityCG.cginc"

        float4 _Color;
        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;
        float _Metallic;
        float _Glossiness;
        float _BumpScale;
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
            float4 projCol = tex2D(_MainTex , projection);

            o.Albedo = projCol.rgb * _Color;
        	o.Alpha = projCol.a;
        	o.Metallic = tex2D(_MetallicGlossMap , projection) * _Metallic;
        	o.Smoothness = _Glossiness;
            o.Normal = UnpackNormal(tex2D(_BumpMap , projection)) * _BumpScale;
        }
        ENDCG
    }
}


