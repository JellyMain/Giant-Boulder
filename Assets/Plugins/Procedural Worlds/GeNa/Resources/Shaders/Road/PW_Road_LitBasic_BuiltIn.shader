Shader "Custom/PW_Road_LitBasic_BuiltIn"
{
    Properties
    {
        _BaseColorMap("_BaseColorMap", 2D) = "white" {}
        _BaseColor("_BaseColor", Color) = (1,1,1,1)
        [NoScaleOffset]_NormalMap("_NormalMap", 2D) = "bump"
        _NormalScale("_NormalScale", Float) = 1
        [NoScaleOffset]_MaskMap("_MaskMap", 2D) = "blue"
        _MaskMapMod("_MaskMapMod", Vector) = (1,1,1,1)
        _SurfaceBlend("_SurfaceBlend", Float) = 0.1
        _SurfaceBlendPower("_SurfaceBlendPower", Float) = 1
        _TerrainLODOffset("_TerrainLODOffset", Float) = 1
        _TerrainLODDistance("_TerrainLODDistance", Float) = 600
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
        LOD 200

        Cull Back
        ZTest Lequal
        Zwrite on


        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _BaseColorMap;
        sampler2D _NormalMap;
        sampler2D _MaskMap;

        float4 _BaseColor;
        float _NormalScale;
        float4 _MaskMapMod;
        float _SurfaceBlend;
        float _SurfaceBlendPower;
        float _TerrainLODOffset;
        float _TerrainLODDistance;

        struct Input
        {
            float2 uv_BaseColorMap;
            float4 vertex;
            float3 worldPos;
            float4 screenPos;
            float3 viewDir;
            float3 worldNormal; 
            INTERNAL_DATA
        };


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 NormalStrength(float3 Normal, float Strength)
        {
            return float3(Normal.xy * Strength, Normal.z);
        }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);

            float3 distanceVector = _WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex);
            float distance = saturate(length(distanceVector) / _TerrainLODDistance) * _TerrainLODOffset;
            float3 positionOffset = float3(0, distance, 0);
            v.vertex.xyz = v.vertex.xyz + positionOffset;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = tex2D(_BaseColorMap, IN.uv_BaseColorMap).xyz * _BaseColor.xyz;
            o.Normal = NormalStrength(UnpackNormal(tex2D(_NormalMap, IN.uv_BaseColorMap)), _NormalScale);
            float4 maskData = tex2D(_MaskMap, IN.uv_BaseColorMap) * _MaskMapMod;

            o.Metallic = maskData.r;
            o.Occlusion = maskData.g;
            o.Smoothness = maskData.a;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
