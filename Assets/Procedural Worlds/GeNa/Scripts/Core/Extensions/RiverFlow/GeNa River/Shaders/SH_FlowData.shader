Shader "PWS/SH_FlowData"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 direction : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float3 bitangent = cross((v.tangent.xyz * v.tangent.w), v.normal);
                o.direction.xyz = bitangent * 0.5 + 0.5;
                o.direction.y = 1 - abs(v.normal.y);
                o.direction.w = 1.0;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.direction.x, i.direction.z, i.direction.y, 1); 
            }
            ENDCG
        }
    }

       
    SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent-1"}
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    float4 tangent : TANGENT;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float4 direction : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    float3 bitangent = cross((v.tangent.xyz * v.tangent.w), v.normal);
                    o.direction.xyz = bitangent * 0.5 + 0.5;
                    o.direction.y = 1 - abs(v.normal.y);
                    o.direction.w = 1.0;

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    return float4(i.direction.x, i.direction.z, i.direction.y, 1);
                }
            ENDCG
        }
        }

        SubShader
            {
                Tags { "RenderType" = "Transparent" "Queue" = "Transparent-2"}
                LOD 100

                Pass
                {
                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag

                    #include "UnityCG.cginc"

                    struct appdata
                    {
                        float4 vertex : POSITION;
                        float2 uv : TEXCOORD0;
                        float3 normal : NORMAL;
                        float4 tangent : TANGENT;
                    };

                    struct v2f
                    {
                        float2 uv : TEXCOORD0;
                        float4 vertex : SV_POSITION;
                        float4 direction : TEXCOORD1;
                    };

                    sampler2D _MainTex;
                    float4 _MainTex_ST;

                    v2f vert(appdata v)
                    {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                        float3 bitangent = cross((v.tangent.xyz * v.tangent.w), v.normal);
                        o.direction.xyz = bitangent * 0.5 + 0.5;
                        o.direction.y = 1 - abs(v.normal.y);
                        o.direction.w = 1.0;

                        return o;
                    }

                    fixed4 frag(v2f i) : SV_Target
                    {
                        return float4(i.direction.x, i.direction.z, i.direction.y, 1);
                    }
                ENDCG
            }
            }

}
