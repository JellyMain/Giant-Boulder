Shader "Unlit/SH_PostProcess_Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Distance("Distance", Float) = 0.01
        _DistanceSteps("DistanceSteps", Float) = 16
        _RadialSteps("RadialSteps", Float) = 8
        _KernelPower("KernelPower", Float) = 1
        _RadialOffset("_RadialOffset", Float) = 0.618
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Distance;
            float _DistanceSteps;
            float _RadialSteps;
            float _KernelPower;
            float _RadialOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 CurColor = float3(0,0,0);
                float2 NewUV = i.uv;
                int incre = 0;
                float StepSize = _Distance / (int)_DistanceSteps;
                float CurDistance = 0;
                float2 CurOffset = 0;
                float SubOffset = 0;
                float TwoPi = 6.283185;
                float accumdist = 0;

                if (_DistanceSteps < 1)
                {
                    return fixed4(tex2D(_MainTex, i.uv));
                }
                else
                {
                    while (incre < (int)_DistanceSteps)
                    {
                        CurDistance += StepSize;
                        for (int j = 0; j < (int)_RadialSteps; j++)
                        {
                            SubOffset += 1;
                            CurOffset.x = cos(TwoPi * (SubOffset / _RadialSteps));
                            CurOffset.y = sin(TwoPi * (SubOffset / _RadialSteps));
                            NewUV.x = i.uv.x + CurOffset.x * CurDistance;
                            NewUV.y = i.uv.y + CurOffset.y * CurDistance;
                            float distpow = pow(CurDistance, _KernelPower);
                            CurColor += tex2D(_MainTex, NewUV) * distpow;
                            accumdist += distpow;
                        }
                        SubOffset += _RadialOffset;
                        incre++;
                    }
                    CurColor = CurColor;
                    CurColor /= accumdist;
                    return float4(CurColor.xyz, tex2D(_MainTex, NewUV).a);
                }

            }
            ENDCG
        }
    }
}
