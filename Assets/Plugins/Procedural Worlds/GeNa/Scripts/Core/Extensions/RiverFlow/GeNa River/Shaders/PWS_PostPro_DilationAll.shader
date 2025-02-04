Shader "Unlit/PWS_PostPro_DilationAll"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _Opacity("Opacity", 2D) = "White" {}
        _MaxSteps("MaxSteps", Float) = 256
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" "PostProcessType" = "dilate"}
            LOD 100

            Ztest Always
            zwrite off

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
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _Opacity;
                float4 _MainTex_ST;
                float _MaxSteps;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f IN) : SV_Target
                {

                    //int MaxSteps = 256;
                    int MaxSteps = _MaxSteps;
                    float4 preSample = tex2D(_MainTex, IN.uv);
                    float4 outSample = float4(0,0,0,0);

                    float2 texelsize = float2(1,1) / _ScreenParams.xy;
                    float2 offsets[8] = {float2(-1,0), float2(1,0), float2(0,1), float2(0,-1), float2(-1,1), float2(1,1), float2(1,-1), float2(-1,-1)};
                    float hasHit = 0;

                    for (int i = 0; i < MaxSteps; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            float2 curUV = IN.uv + offsets[j] * texelsize * i;
                            float4 offsetSample = tex2D(_MainTex, curUV);
                            //float4 offsetSampleAlpha = tex2D(_Opacity, curUV);
                            if (offsetSample.a != 0 && hasHit == 0)
                             {

                                outSample = offsetSample;
                                hasHit = 1;
                             }
                        }
                    }

                    return float4(outSample.rgb, preSample.a);

                }
                ENDCG
            }
        }
}
