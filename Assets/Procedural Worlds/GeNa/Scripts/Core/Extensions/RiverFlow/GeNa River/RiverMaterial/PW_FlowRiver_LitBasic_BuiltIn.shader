Shader "PWS/PW_FlowRiver" {
	Properties
	{
		[Header(Water Properties)]
		_Color("Color", Color) = (0,0,0,0)
		_WaterFogColor("Water Fog Color", Color) = (36,60,67,255)
		_WaterFogDensity("Water Fog Density", Range(0.001, 5)) = 0.46

		_UseLUTColor("UseLUTColor", Int) = 0
		[NoScaleOffset]_ColorLUT("ColorLUT", 2D) = "white" {}
		_ColorLUTDepth("ColorLUTDepth", Float) = 1.69
		_ColorLUTShift("ColorLUTShift", Float) = 0
		_ColorLUTTint("ColorLUTTint", Color) = (255,255,255,255)
		_RefractionStrength("Refraction Strength", Range(0, 1)) = 0.221

		[NoScaleOffset] _MainTex("Deriv (AG) Height (B)", 2D) = "black" {}
		[NoScaleOffset] _FoamMap("Foam", 2D) = "black" {}
		[NoScaleOffset] _FlowMap("Flow (RG)", 2D) = "black" {}
		[NoScaleOffset] _NoiseMap("Noise", 2D) = "black" {}

		[Header(Flow Properties)]
		[Toggle(_DUAL_GRID)] _DualGrid("Dual Grid", Int) = 0
		_GridResolution("Grid Resolution", Float) = 60
		_Speed("Speed", Float) = 0.25
		_FlowStrength("Flow Strength", Float) = 0.1
		_HeightScale("Height Scale, Constant", Float) = 0.75
		_HeightScaleModulated("Height Scale, Modulated", Float) = 0.9
		_Scale("Scale", Float) = 4.8
		_RandomAmount("RandomAmount", Range(0, 1)) = 0.35
		_SlopeStrength("SlopeStrength", Range(0, 1)) = 0.1

		[Header(Reflections)]
		_Roughness("Roughness", Range(0, 1)) = 1

		[Header(Caustics)]
		[NoScaleOffset]_CausticsMap("CausticsMap", 2D) = "white" {}
		[NoScaleOffset]_CausticsFlipbook("CausticFlipbook", 2D) = "white" {}
		_CausticsScale("CausticsScale", Float) = 3.67
		_CausticsRatio("Caustics Ratio", Float) = 0.12
		_CausticsDepth("CausticsDepth", Float) = 4
		_CausticsNormalIntensity("CausticsNormalIntensity", Float) = 0.69
		_CausticsHeightIntensity("CausticsHeightIntensity", Float) = 1.08
		_CausticsBrightness("CausticsBrightness", Float) = 6
		_FocalLength("FocalLength", Float) = 1
		_Aperture("Aperture", Float) = 1

		[Header(Edge Blend)]
		_DepthFalloff("Depth Falloff", Float) = 0.59
		_DepthEdgePower("Depth Edge Power", Float) = 7.48

		[Header(UnderwaterFog)]
		_FogBlurDepth("Fog Blur Depth", Float) = 1.3
		_FogBlurAmount("Fog Blur Amount", Range(0.0003, 0.1)) = 0.0076
		_Wetness("Wetness", Range(0, 1)) = 0.673

		[Header(Foam)]
		_FoamScale("Foam Scale", Float) = 30
		_FoamSpeed("Foam Speed", Float) = 1
		_FoamSlopeStrength("Foam Slope Strength", Float) = 7.6
		_FoamContrast("Foam Contrast", Float) = 1.89
		_FoamBrightness("Foam Brightness", Float) = 0.38
		_FoamGridResolution("FoamGridResolution", Float) = 1
		[NoScaleOffset]_FoamDiffuse("Foam Diffuse", 2D) = "white" {}
		[NoScaleOffset]_FoamNormal("Foam Normal", 2D) = "white" {}

		_Specular("Specular", Float) = 0.01

		[Header(Bounds Information)]
		_BoundsMinimum("BoundsMinimum", Vector) = (0,0,0,1)
		_BoundsMaximum("BoundsMaximum", Vector) = (1,1,1,1)
		_Center("Center", Vector) = (0.5,0.5,0.5,1)
		_Extent("Extent", Vector) = (100,100,100,100)

		[Header(Sea Level)]
		_PWOceanHeight("Sea Level Height", Float) = 12.56
		_SeaLevelBlend("Sea Level Blend", Range(0.001,10)) = 1.25
		_ShoreBlend("Shore Blend", Range(0.01, 1)) = 0.13

		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0

		[NoScaleOffset]_TestTexAlbedo("TestTexAlbedo", 2DArray) = "white" {}
		[NoScaleOffset]_TestTexNormal("TestTexNormal", 2DArray) = "black" {}
		_Index("Index", Int) = 0


		[Header(Foam)]
		_FoamColor("Foam Color", Color) = (1,1,1,0.2196078)
		_FoamShoreAlbedo("Foam Albedo", 2D) = "white" {}
		[NoScaleOffset] _FoamShoreNormal("Foam Normal", 2D) = "bump" {}
		_FoamNormalStrength("Foam Normal Strength", Range(0.01, 2)) = 0.633
		[NoScaleOffset] _FoamShoreMask("Foam Mask", 2D) = "white" {}
		_FoamBlend("Foam Shore Blend", Range(0.01, 1)) = 0.125
		_FoamHeight("Foam Height", Range(0.01, 1)) = 0.129
		_FoamRipple("Foam Ripple", Range(0.01, 1)) = 0.09
		_FoamShoreSpeed("Foam Speed", Range(0.001,2)) = 0.137

		_NormalShoreBlend("Shore Normal Blend", Range(0.01, 1)) = 0.4
		_SeaLevelFoamColor("Sea Level Foam Color", Color) = (1,1,1,0.78)
		_SeaLevelFoamNormalStrength("Sea Level Foam Normal Strength", Range(0.01, 2)) = 1

	}
	
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "ForceNoShadowCasting" = "True"}
		LOD 200
		Cull Back


		//First pass Zprime to check for camera going behind water
		//Set stencil buffer to 2 when this happens
		Pass
		{

			Name "Zprime"

			ZTest Lequal
			Zwrite On
			Cull Back
			ColorMask 0

			Stencil {
				Ref 2
				Comp always
				Pass replace
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

			struct v2f
		    {
				float4 pos: POSITION;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}


			half4 frag(v2f i) : COLOR
			{

				return half4(0,0,0,0);
			}
			ENDCG
		}

		ZTest Lequal
		Zwrite off

		Stencil
		{
			Ref 2
			Comp equal
			Pass keep
		}

		CGPROGRAM
		#pragma surface surf StandardSpecular alpha:blend vertex:vert fullforwardshadows addshadow
		#pragma target 4.0

		#pragma shader_feature _DUAL_GRID
		#pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF

		uniform sampler2D   _CameraOpaqueTexture;
		uniform sampler2D   _CameraGBufferTexture2;

		//Water Properties
		sampler2D _FoamMap;
		sampler2D _ColorDepthLUT;
		sampler2D _CameraDepthTexture, _WaterBackground;
		float4 _CameraDepthTexture_TexelSize;
		float3 _WaterFogColor;
		float _WaterFogDensity;
		float _RefractionStrength;
		half _Metallic;

		//Color LUT
		bool _UseLUTColor;
		sampler2D _ColorLUT;
		float _ColorLUTDepth;
		float _ColorLUTShift;
		fixed4 _ColorLUTTint;

		//Reflection Properties
		float _Roughness;

		//Casutics Properties
		sampler2D _CausticsMap;
		sampler2D _CausticsFlipbook;
		float _CausticsScale;
		float _CausticsRatio;
		float _CausticsDepth;
		float _CausticsNormalIntensity;
		float _CausticsHeightIntensity;
		float _CausticsBrightness;
		float _FocalLength;
		float _Aperture;

		//Edge Falloff
		float _DepthFalloff;
		float _DepthEdgePower;

		//Underwater Fog
		float _FogBlurAmount;
		float _FogBlurDepth;
		float _Wetness;

		float _Alpha;
		float _Specular;

		//Foam Properties
		float _FoamScale;
		float _FoamSlopeStrength;
		float _FoamContrast;
		float _FoamSpeed;
		float _FoamBrightness;
		float _FoamGridResolution;
		sampler2D _FoamDiffuse;
		sampler2D _FoamNormal;

		//Flow Properties
		sampler2D _MainTex;
		sampler2D _FlowMap;
		sampler2D _NoiseMap;
		float _GridResolution;
		float _Speed;
		float _FlowStrength;
		float _HeightScale, _HeightScaleModulated;
		float _Scale;
		float _RandomAmount;
		float _SlopeStrength;
		fixed4 _Color;

		//Bounds Information
		float4 _BoundsMinimum;
		float4 _BoundsMaximum;
		float4 _Center;
		float4 _Extent;

		//Sea Level Blend
		float _PWOceanHeight;
		float _SeaLevelBlend;
		float _ShoreBlend;


		//Foam
		sampler2D _FoamShoreAlbedo;
		sampler2D _FoamShoreMask;
		sampler2D _FoamShoreNormal;
		float4 _FoamColor;
		float _FoamBlend, _FoamHeight, _FoamRipple, _FoamNormalStrength, _FoamShoreSpeed;

		//Ocean Foam
		float _SeaLevelFoamNormalStrength;
		float4 _SeaLevelFoamColor;

		//Test Array
		UNITY_DECLARE_TEX2DARRAY(_TestTexAlbedo);
		UNITY_DECLARE_TEX2DARRAY(_TestTexNormal);
		int _Index;

		#include "Flow_BuiltIn.cginc"
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		//Input Frag Structure
		struct Input {
			float2 uv_MainTex;
			float2 uv_FoamShoreAlbedo;
			float3 worldPos;
			float3 viewDir;
			float3 sWorldNormal;
			float4 tangent;
			float4 screenPos;
			float4 direction;
			float3 vertexNormal;
			float3 worldNormal;
			float4 vertex;
			float3 tangentViewDir;
			INTERNAL_DATA
		};

		float cheapContrast(float IN, float contrast)
		{
			return lerp(0 - contrast, contrast + 1, IN);
		}

		void heightLerp(float a, float b, float transitionPhase, float height, float contrast, out float result)
		{
			float IN = saturate((height - 1) + (transitionPhase * 2));
			result = cheapContrast(IN, contrast);
		}

		float3 derivNormals(float4 normal, float intensity)
		{
			float2 derivNormals = normal.xy* intensity;
			float3 xDeriv = float3(1, derivNormals.x, 0);
			float3 yDeriv = float3(0, derivNormals.y, 1);
			return normalize(cross(yDeriv, xDeriv));
		}

		//Remap position into (0 to 1) range
		float RemapFloat(float value, float from1, float to1, float from2, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		//Vertex Function
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.sWorldNormal = mul((float3x3)unity_ObjectToWorld, SCALED_NORMAL);
			o.tangent = mul(unity_ObjectToWorld, v.tangent);

			float3x3 objectToTangent = float3x3(v.tangent.xyz, cross(v.normal, v.tangent.xyz) * v.tangent.w, v.normal);
			o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));

			float3 bitangent = cross(v.tangent.xyz, v.normal) * v.tangent.w;
			bitangent = normalize(bitangent);
			o.direction.xyz = bitangent * 0.5 + 0.5;

			o.direction.y = 1 - abs(v.normal.y);

			o.direction.w = 1.0;
			o.vertex = v.vertex;
		}

		//Fragment Function
		void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
			
			/*---Flow Data---*/
			//create time
			float time = _Time.y * -_Speed;

			//sample world position uvs scaled
			float2 worldUV = IN.worldPos.xz / _Scale;

			//Create Object UVs
			float3 center = _BoundsMaximum - ((_BoundsMaximum - _BoundsMinimum) / 2);
			float2 boxBounds = (_BoundsMinimum - _BoundsMaximum).xz;
			float3 objectUVs;
			Remap(IN.worldPos, _BoundsMinimum, _BoundsMaximum, float3(0, 0, 0), float3(1, 1, 1), objectUVs);

			//create FlowGrid
			float3 dh;
			FlowGrid_float(objectUVs.xz, time, false, worldUV, dh);

			//sample second FlowGrid if enabled
			#if defined(_DUAL_GRID)
			float3 dh2;
			FlowGrid_float(objectUVs.xz, time, true, worldUV, dh2);
			dh = (dh + dh2) * 0.5;
			#endif

			/*---World Space Tangent Matrix---*/

			//Construct Tangent To World Matrix
			float3 tangent = normalize(IN.tangent.xyz);
			float3 normal = normalize(IN.sWorldNormal);
			float3 binormal = normalize(cross(normal, tangent) * IN.tangent.w);
			float3x3 tangentToWorld = transpose(float3x3(tangent, binormal, normal));

			//Transform the normals from tangent space to world space
			float3 worldNormal = saturate(mul(tangentToWorld, normalize(float3(-dh.xy, 1))));

			//Grab the forward view direction
			float3 camForward = mul(unity_CameraToWorld, float3(0, 0, 1));

			//preview colour 
			fixed4 c = _Color;

			//Normal Creation
			float3 flowNormal = normalize(float3(-dh.xy, 1));
			float3 Normal = flowNormal;

			/*---Foam---*/
			float foamSpeed = _Time.y * (_FoamSpeed * -_Speed);
			float2 foamUV = IN.worldPos.xz / _FoamScale;

			float3 foamData;
			FlowGridFoam_float(objectUVs.xz, foamSpeed, false, foamUV, foamData);
			
			float3 foam;
			getFoamMask_float(foamData, IN.direction.xyz, foam);

			float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, IN.screenPos).r;
			float sceneZ = LinearEyeDepth(rawZ).r;
			sceneZ = unity_OrthoParams.w == 1 ? 2.0 : sceneZ;
			float surfaceZ = IN.screenPos.w;
			float zdepth = sceneZ - surfaceZ;

			//zdepth = unity_OrthoParams.w == 1 ? 1.0 : zdepth;

			/*---Caustics---*/
			float causticSurfaceDepth;
			//getDepthAtSurface_float(IN.screenPos, causticSurfaceDepth);

			causticSurfaceDepth = zdepth;

			//Light Direction Mask
			float causticsLightMask;
			getLightDirectionMask_float(IN.screenPos, causticsLightMask);

			//Refraction Angle Calculation
			float refractionDepth;
			getRefractionAngle_float(IN.worldPos, causticSurfaceDepth, refractionDepth);

			//Refraction Vector
			float3 refractVector;
			calculateRefractionVector_float(refractionDepth, worldNormal, refractVector);
			float2 causticsWorldUV = IN.worldPos.xz / _CausticsScale;

			//Create Caustics
			float3 caustics;
			getCaustics_float(causticsWorldUV, refractVector, causticsLightMask, refractionDepth, caustics);

			/*---Color Below Water---*/
			//get the depth
			float depth;
			depth = zdepth;
			//getDepthAtSurface_float(IN.screenPos, depth);

			//Create offset for refraction
			float2 uvOffset = Normal.xy * _RefractionStrength * saturate(depth  / 4);
			uvOffset.y *= _CameraDepthTexture_TexelSize.z * abs(_CameraDepthTexture_TexelSize.y);

			//screen uvs for refraction
			float2 screenUV = AlignWithGrabTexel((IN.screenPos.xy + uvOffset) / IN.screenPos.w);


			float causticsDepth = depth * _CausticsDepth;
			causticsWorldUV = causticsWorldUV + causticsDepth;

			float3 flipCaustics;
			FlowGridCaustics_float(objectUVs.xz, time, false, causticsWorldUV, _CausticsFlipbook, flipCaustics);
			flipCaustics *= _CausticsBrightness / 2;
			flipCaustics *= causticsLightMask;
			flipCaustics = flipCaustics * _LightColor0;
			float causticsHeight = pow(depth, _CausticsHeightIntensity);
			flipCaustics *= saturate(causticsHeight);


			/*---Sea Level Blend---*/
			float seaLevelBlendIn;
			float edgeFalloff;
			getSeaLevelBlend_float(IN.uv_MainTex, IN.worldPos, seaLevelBlendIn, edgeFalloff);
			edgeFalloff = pow(edgeFalloff * 8, 2);

			float3 colorBelowWater;
			colorBelowWater_float(screenUV, depth, flipCaustics, colorBelowWater);


			/*--- Create Water Surface---*/
			float4 waterSurface_albedo;
			float4 waterSurface_normal;
			float4 waterSurface_mask;

			//Emission
			float3 Emission = colorBelowWater;


			//Create water surface
			createSurfaceStruct_float(
				c.rgb, //Albedo
				//seaLevelBlendIn* saturate(edgeFalloff)* saturate(pow(depth, 2) * 50), //Alpha
				saturate(seaLevelBlendIn * 1.5)* saturate(edgeFalloff)* saturate(pow(depth * 2, 10)), //Alpha
				Normal, //Normal
				0, //Height
				0, //Metallic
				1, //AO
				1, //Emission
				_Roughness, //Smoothness
				waterSurface_albedo, waterSurface_normal, waterSurface_mask); //Outputs

			//Foam Data
			float4 foam_albedo;
			float4 foam_normalMask;
			FlowGridSurface_float(objectUVs.xz, foamSpeed, false, foamUV, 0, _FoamDiffuse, _FoamNormal, foam_albedo, foam_normalMask);

			float4 foamSurface_albedo, foamSurface_normal, foamSurface_mask;

			//Create foam surface
			createSurfaceStruct_float(
				foam_albedo.rgb / 4, //Albedo
				1, //Alpha
				normalize(float3(-foam_normalMask.xy, 1)), //Normal
				foam_albedo.w, //Height
				0, //Metallic
				foam_normalMask.b, //AO
				0, //Emission
				0.7, //Smoothness
				foamSurface_albedo, foamSurface_normal, foamSurface_mask //Outputs
			);

			float foamTransitionPhase = saturate(pow((abs(IN.direction.y)), _FoamContrast) * _FoamSlopeStrength);
			float foamBlendMask;
			heightLerp(0, 1, foamTransitionPhase, foam_albedo.a, 0, foamBlendMask);

			float4 finalSurface_albedo, finalSurface_normal, finalSurface_mask;

			blendSurfaceStructAlpha_float(waterSurface_albedo, waterSurface_normal, waterSurface_mask,
				foamSurface_albedo, foamSurface_normal, foamSurface_mask,
				foamBlendMask,
				finalSurface_albedo, finalSurface_normal, finalSurface_mask);

			//Struct outputs
			o.Albedo = finalSurface_albedo.rgb;
			o.Normal = finalSurface_normal.rgb;
			o.Smoothness = finalSurface_mask.a;
			o.Emission = (Emission* (1 - foamBlendMask)) + (foamSurface_albedo * foamBlendMask);
			o.Specular = _Specular;
			o.Alpha = 1;
			//o.Alpha = waterSurface_albedo.a;




			//Sea Level Blend
			float zseaLevelBlendIn = (IN.worldPos.y - _PWOceanHeight) * _SeaLevelBlend;
			float zsealevelBlendOut = saturate(zseaLevelBlendIn * 3.0f);
			float zuvWidth = (saturate(IN.uv_MainTex.xx) - 0.5f) * 2.0f;
			float zedgeFalloff = 1.0f - dot(zuvWidth, zuvWidth);
			zseaLevelBlendIn = 1.0f - saturate(lerp(zedgeFalloff * zseaLevelBlendIn, saturate(1 + zedgeFalloff) * zseaLevelBlendIn, zseaLevelBlendIn * zseaLevelBlendIn));

			//o.Alpha = saturate((zdepth) / _ShoreBlend) * zsealevelBlendOut * (1 - zseaLevelBlendIn);


			float2 foamUv = IN.uv_FoamShoreAlbedo;
			foamUv.y += frac(_Time.y * -_FoamShoreSpeed);

			zseaLevelBlendIn *= _SeaLevelFoamColor.a;

			float4 foamAlbedo = tex2D(_FoamShoreAlbedo, foamUv);
			float3 foamNormal = UnpackNormal(tex2D(_FoamShoreNormal, foamUv));
			float4 foamMask = tex2D(_FoamShoreMask, foamUv);
			float foamBlend = 1.0f - saturate((zdepth - (foamMask.b - 0.5f) * _FoamHeight) / _FoamBlend);

			//Add Ocean foam values into Foam
			foamBlend = saturate(foamBlend + zseaLevelBlendIn);
			_FoamColor = lerp(_FoamColor, _SeaLevelFoamColor, zseaLevelBlendIn);
			_FoamNormalStrength = lerp(_FoamNormalStrength, _SeaLevelFoamNormalStrength, zseaLevelBlendIn);

			float4 foamColor = foamBlend * _FoamColor * foamAlbedo;

			float depthOffset = (foamMask.b * foamColor.a * _FoamHeight);
			o.Alpha = saturate((zdepth) / _ShoreBlend) * zsealevelBlendOut;
			
			
			o.Albedo = (o.Albedo * (1 - foamColor.a));
			o.Emission = (o.Emission * (1 - foamColor.a)) + (foamColor * 3 * foamColor.a);
			//o.Emission = zsealevelBlendOut;
		}
		ENDCG
	}
		FallBack "Diffuse"
}