// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CrossHair_BuildIn"
{
	Properties
	{
		[NoScaleOffset]_CameraOutputTEX("CameraOutputTEX", 2D) = "white" {}
		_BgColor("BgColor", Color) = (1,1,1,0)
		_Scope("Scope", 2D) = "black" {}
		_RotareCrossHair("RotareCrossHair", Range( -180 , 180)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Overlay+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _CameraOutputTEX;
		uniform float4 _BgColor;
		uniform sampler2D _Scope;
		uniform float4 _Scope_ST;
		uniform float _RotareCrossHair;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv0_Scope = i.uv_texcoord * _Scope_ST.xy + _Scope_ST.zw;
			float cos19 = cos( radians( _RotareCrossHair ) );
			float sin19 = sin( radians( _RotareCrossHair ) );
			float2 rotator19 = mul( uv0_Scope - float2( 0.5,0.5 ) , float2x2( cos19 , -sin19 , sin19 , cos19 )) + float2( 0.5,0.5 );
			float4 tex2DNode3 = tex2D( _Scope, rotator19 );
			float4 lerpResult14 = lerp( ( tex2D( _CameraOutputTEX, i.uv_texcoord ) * _BgColor ) , tex2DNode3 , tex2DNode3.a);
			o.Albedo = lerpResult14.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18301
-1;477;1244;508;1752.758;-43.74295;1.010001;True;True
Node;AmplifyShaderEditor.TexturePropertyNode;1;-1425.974,-196.1691;Inherit;True;Property;_Scope;Scope;3;0;Create;True;0;0;False;0;False;None;8b5bacecd5836464abf16f442d17adad;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1368.619,284.8319;Inherit;False;Property;_RotareCrossHair;RotareCrossHair;4;0;Create;True;0;0;False;0;False;0;68;-180;180;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;9;-1353.816,-718.246;Inherit;True;Property;_CameraOutputTEX;CameraOutputTEX;1;1;[NoScaleOffset];Create;True;0;0;False;0;False;None;f64acd62d09a643dd9796712af91c4a3;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-1107.075,-340.8611;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-1154.686,-77.87256;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RadiansOpNode;21;-1082.875,230.5931;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-718.1219,-350.4117;Inherit;True;Property;_TextureSample1;Texture Sample 1;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;8;-1338.398,-522.3437;Inherit;False;Property;_BgColor;BgColor;2;0;Create;True;0;0;False;0;False;1,1,1,0;0.5333334,1,0.5843138,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;19;-946.3969,187.5734;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;3;-714.4634,-97.70776;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-319.8799,-495.976;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;14;-247.2239,-243.5193;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;5;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;CrossHair_BuildIn;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Opaque;;Overlay;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;17;2;1;0
WireConnection;21;0;18;0
WireConnection;10;0;9;0
WireConnection;10;1;7;0
WireConnection;19;0;17;0
WireConnection;19;2;21;0
WireConnection;3;0;1;0
WireConnection;3;1;19;0
WireConnection;16;0;10;0
WireConnection;16;1;8;0
WireConnection;14;0;16;0
WireConnection;14;1;3;0
WireConnection;14;2;3;4
WireConnection;5;0;14;0
ASEEND*/
//CHKSM=0DE3ACA1D54668A5E296A6764D60F6FEE9E136D8