// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Knife/Fire PBR"
{
	Properties
	{
		_Noise("Noise", 2D) = "white" {}
		_Alpha("Alpha", 2D) = "white" {}
		[HDR]_Color0("Color 0", Color) = (1,1,1,1)
		[HDR]_Color1("Color 1", Color) = (1,1,1,1)
		_Opacity("Opacity", Range( 0 , 1)) = 1
		_NoiseSoftness("NoiseSoftness", Range( 0 , 1)) = 0
		_NoiseSpeed("NoiseSpeed", Vector) = (0,1,0,0)
		_DepthFade("DepthFade", Float) = 0
		_Rotation("Rotation", Float) = 1
		_Offset("Offset", Vector) = (1,0,0,0)
		_AlphaSoftness("AlphaSoftness", Range( 0 , 1)) = 1
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.0
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 vertexColor : COLOR;
			float4 uv_tex4coord;
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float4 screenPos;
		};

		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float _NoiseSoftness;
		uniform sampler2D _Noise;
		uniform float2 _NoiseSpeed;
		uniform float4 _Noise_ST;
		uniform float2 _Offset;
		uniform float _Rotation;
		uniform float _AlphaSoftness;
		uniform sampler2D _Alpha;
		uniform float4 _Alpha_ST;
		uniform float _Opacity;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DepthFade;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float NoiseSoftnessMul60 = i.uv_tex4coord.w;
			float2 uv0_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float cos37 = cos( ( i.uv_tex4coord.z * _Rotation ) );
			float sin37 = sin( ( i.uv_tex4coord.z * _Rotation ) );
			float2 rotator37 = mul( ( uv0_Noise + ( i.uv2_texcoord2 * _Offset ) ) - float2( 0.5,0.5 ) , float2x2( cos37 , -sin37 , sin37 , cos37 )) + float2( 0.5,0.5 );
			float2 panner24 = ( 1.0 * _Time.y * _NoiseSpeed + rotator37);
			float4 tex2DNode2 = tex2D( _Noise, panner24 );
			float smoothstepResult11 = smoothstep( 0.0 , ( NoiseSoftnessMul60 * _NoiseSoftness ) , tex2DNode2.r);
			float4 lerpResult9 = lerp( _Color0 , _Color1 , smoothstepResult11);
			float2 uv_Alpha = i.uv_texcoord * _Alpha_ST.xy + _Alpha_ST.zw;
			float smoothstepResult69 = smoothstep( 0.0 , _AlphaSoftness , tex2D( _Alpha, uv_Alpha ).r);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth50 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth50 = abs( ( screenDepth50 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) );
			float clampResult52 = clamp( distanceDepth50 , 0.0 , 1.0 );
			float smoothstepResult58 = smoothstep( 0.0 , 1.0 , tex2DNode2.r);
			float4 temp_output_13_0 = ( lerpResult9 * ( smoothstepResult69 * _Opacity * clampResult52 * ( smoothstepResult69 - smoothstepResult58 ) ) );
			float4 temp_output_75_0 = ( i.vertexColor * temp_output_13_0 );
			o.Albedo = temp_output_75_0.rgb;
			o.Emission = temp_output_13_0.rgb;
			float clampResult73 = clamp( (temp_output_75_0).a , 0.0 , 1.0 );
			o.Alpha = clampResult73;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float4 screenPos : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xyzw = customInputData.uv_tex4coord;
				o.customPack1.xyzw = v.texcoord;
				o.customPack2.xy = customInputData.uv_texcoord;
				o.customPack2.xy = v.texcoord;
				o.customPack2.zw = customInputData.uv2_texcoord2;
				o.customPack2.zw = v.texcoord1;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_tex4coord = IN.customPack1.xyzw;
				surfIN.uv_texcoord = IN.customPack2.xy;
				surfIN.uv2_texcoord2 = IN.customPack2.zw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.screenPos = IN.screenPos;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18000
-1885;7;1839;1004;775.2991;471.7101;1;True;False
Node;AmplifyShaderEditor.Vector2Node;65;-2895.64,-232.0716;Float;False;Property;_Offset;Offset;9;0;Create;True;0;0;False;0;1,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;63;-2980.21,-338.5386;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-2725.64,-276.0716;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-2595.706,54.99451;Float;False;Property;_Rotation;Rotation;8;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;38;-2697.164,-501.3408;Inherit;False;0;2;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;39;-2612.864,-169.7409;Inherit;False;0;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-2356.764,-81.34088;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-2462.64,-400.0716;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RotatorNode;37;-2120.164,-246.4408;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;25;-1909.564,-72.24103;Float;False;Property;_NoiseSpeed;NoiseSpeed;6;0;Create;True;0;0;False;0;0,1;0,-0.8;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;24;-1618.364,-180.141;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;60;-2342.706,27.99451;Float;False;NoiseSoftnessMul;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;-1601.706,54.99451;Inherit;False;60;NoiseSoftnessMul;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-1072.198,657.0425;Float;False;Property;_DepthFade;DepthFade;7;0;Create;True;0;0;False;0;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1436.3,-246.6;Inherit;True;Property;_Noise;Noise;0;0;Create;True;0;0;False;0;-1;None;2140d5caeca76404cadd35cc48f45f10;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-1326,152;Inherit;True;Property;_Alpha;Alpha;1;0;Create;True;0;0;False;0;-1;None;0000000000000000f000000000000000;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-1587.519,148.1208;Float;False;Property;_NoiseSoftness;NoiseSoftness;5;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-1306.498,383.3688;Float;False;Property;_AlphaSoftness;AlphaSoftness;10;0;Create;True;0;0;False;0;1;0.486;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;69;-979.498,177.3688;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-1274.706,74.99451;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;58;-995.1234,-104.3162;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;50;-884.198,589.0425;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-1153.8,-674.3;Float;False;Property;_Color1;Color 1;3;1;[HDR];Create;True;0;0;False;0;1,1,1,1;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;55;-641.1234,13.68384;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;4;-1147.8,-830.3;Float;False;Property;_Color0;Color 0;2;1;[HDR];Create;True;0;0;False;0;1,1,1,1;31.62679,11.92214,2.649365,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;52;-576.198,573.0425;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-836,407;Float;False;Property;_Opacity;Opacity;4;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;11;-979.5187,-352.8792;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-475,312;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-647.9,-640.7999;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;14;52.1369,-333.6667;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;106.481,-47.87921;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;327.7009,-117.7101;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;72;448.5002,64.82181;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;73;638.5002,131.8218;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;70;810,-77;Float;False;True;-1;4;ASEMaterialInspector;0;0;Standard;Knife/Fire PBR;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;66;0;63;0
WireConnection;66;1;65;0
WireConnection;41;0;39;3
WireConnection;41;1;59;0
WireConnection;64;0;38;0
WireConnection;64;1;66;0
WireConnection;37;0;64;0
WireConnection;37;2;41;0
WireConnection;24;0;37;0
WireConnection;24;2;25;0
WireConnection;60;0;39;4
WireConnection;2;1;24;0
WireConnection;69;0;3;1
WireConnection;69;2;67;0
WireConnection;62;0;61;0
WireConnection;62;1;12;0
WireConnection;58;0;2;1
WireConnection;50;0;51;0
WireConnection;55;0;69;0
WireConnection;55;1;58;0
WireConnection;52;0;50;0
WireConnection;11;0;2;1
WireConnection;11;2;62;0
WireConnection;8;0;69;0
WireConnection;8;1;7;0
WireConnection;8;2;52;0
WireConnection;8;3;55;0
WireConnection;9;0;4;0
WireConnection;9;1;5;0
WireConnection;9;2;11;0
WireConnection;13;0;9;0
WireConnection;13;1;8;0
WireConnection;75;0;14;0
WireConnection;75;1;13;0
WireConnection;72;0;75;0
WireConnection;73;0;72;0
WireConnection;70;0;75;0
WireConnection;70;2;13;0
WireConnection;70;9;73;0
ASEEND*/
//CHKSM=45D45B20A0710CB95DD7C90BD6B09FF0ED270421