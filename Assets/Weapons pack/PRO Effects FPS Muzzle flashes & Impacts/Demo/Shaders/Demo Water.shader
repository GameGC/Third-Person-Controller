// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Knife/Demo Water"
{
	Properties
	{
		_Normal1("Normal 1", 2D) = "bump" {}
		_NormalScale1("Normal Scale 1", Float) = 1
		_NormalSpeed1("Normal Speed 1", Vector) = (0.1,0.1,0,0)
		_Normal2("Normal 2", 2D) = "bump" {}
		_NormalScale2("Normal Scale 2", Float) = 1
		_NormalSpeed2("Normal Speed 2", Vector) = (-0.1,-0.1,0,0)
		_Color("Color", Color) = (0.4764151,0.7418258,1,1)
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.98
		_Specular("Specular", Range( 0 , 1)) = 0.5
		_Distortion("Distortion", Float) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf StandardSpecular alpha:fade keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform float _NormalScale1;
		uniform sampler2D _Normal1;
		uniform float2 _NormalSpeed1;
		uniform float4 _Normal1_ST;
		uniform float _NormalScale2;
		uniform sampler2D _Normal2;
		uniform float2 _NormalSpeed2;
		uniform float4 _Normal2_ST;
		uniform float4 _Color;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _Distortion;
		uniform float _Specular;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv0_Normal1 = i.uv_texcoord * _Normal1_ST.xy + _Normal1_ST.zw;
			float2 panner7 = ( 1.0 * _Time.y * _NormalSpeed1 + uv0_Normal1);
			float2 uv0_Normal2 = i.uv_texcoord * _Normal2_ST.xy + _Normal2_ST.zw;
			float2 panner8 = ( 1.0 * _Time.y * _NormalSpeed2 + uv0_Normal2);
			float3 normalizeResult13 = normalize( ( UnpackScaleNormal( tex2D( _Normal1, panner7 ), _NormalScale1 ) + UnpackScaleNormal( tex2D( _Normal2, panner8 ), _NormalScale2 ) ) );
			o.Normal = normalizeResult13;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float4 screenColor24 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( float3( (ase_screenPosNorm).xy ,  0.0 ) + ( _Distortion * normalizeResult13 ) ).xy);
			float4 lerpResult26 = lerp( _Color , screenColor24 , ( 1.0 - _Color.a ));
			o.Albedo = lerpResult26.rgb;
			o.Specular = (_Specular).xxx;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
-1885;7;1839;1004;772.3893;591.5262;1;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-1969.5,447.5;Inherit;False;0;2;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-1970.5,134.5;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;9;-1829.5,297.5;Inherit;False;Property;_NormalSpeed1;Normal Speed 1;2;0;Create;True;0;0;False;0;0.1,0.1;0.02,0.02;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;10;-1579.5,721.5;Inherit;False;Property;_NormalSpeed2;Normal Speed 2;5;0;Create;True;0;0;False;0;-0.1,-0.1;-0.05,-0.05;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;3;-1529.5,303.5;Inherit;False;Property;_NormalScale1;Normal Scale 1;1;0;Create;True;0;0;False;0;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;8;-1334.5,445.5;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;7;-1485.5,184.5;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1297.5,708.5;Inherit;False;Property;_NormalScale2;Normal Scale 2;4;0;Create;True;0;0;False;0;1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-967.5,184.5;Inherit;True;Property;_Normal1;Normal 1;0;0;Create;True;0;0;False;0;-1;None;3e642b290e1041c45bbd75a4ab51cba7;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-940.5,469.5;Inherit;True;Property;_Normal2;Normal 2;3;0;Create;True;0;0;False;0;-1;None;3e642b290e1041c45bbd75a4ab51cba7;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-523.4321,352.1908;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;13;-362.4321,369.1908;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-416.4321,104.1908;Inherit;False;Property;_Distortion;Distortion;9;0;Create;True;0;0;False;0;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;23;-572.6641,-176.4413;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;25;-326.6641,-145.4413;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-200.4321,47.1908;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;22;-28.66406,-120.4413;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;12;-29.43213,-499.8092;Inherit;False;Property;_Color;Color;6;0;Create;True;0;0;False;0;0.4764151,0.7418258,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;30;195.6107,-350.5262;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;220.9679,112.1908;Inherit;False;Property;_Specular;Specular;8;0;Create;True;0;0;False;0;0.5;0.804;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;24;106.3359,-291.4413;Inherit;False;Global;_GrabScreen0;Grab Screen 0;11;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;26;384.3359,-309.4413;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwizzleNode;17;525.9678,117.1908;Inherit;False;FLOAT3;0;0;0;3;1;0;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;14;303.7679,307.1908;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;False;0;0.98;0.98;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;857.7,-95.2;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Knife/Demo Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;6;0
WireConnection;8;2;10;0
WireConnection;7;0;5;0
WireConnection;7;2;9;0
WireConnection;1;1;7;0
WireConnection;1;5;3;0
WireConnection;2;1;8;0
WireConnection;2;5;4;0
WireConnection;11;0;1;0
WireConnection;11;1;2;0
WireConnection;13;0;11;0
WireConnection;25;0;23;0
WireConnection;21;0;20;0
WireConnection;21;1;13;0
WireConnection;22;0;25;0
WireConnection;22;1;21;0
WireConnection;30;0;12;4
WireConnection;24;0;22;0
WireConnection;26;0;12;0
WireConnection;26;1;24;0
WireConnection;26;2;30;0
WireConnection;17;0;15;0
WireConnection;0;0;26;0
WireConnection;0;1;13;0
WireConnection;0;3;17;0
WireConnection;0;4;14;0
ASEEND*/
//CHKSM=BBFC91748FC18649EA26C36060074BAF21A4F9F8