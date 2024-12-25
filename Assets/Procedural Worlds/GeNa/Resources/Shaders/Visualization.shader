// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Procedural Worlds/GeNa/Visualization"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CompareFunction)] _zTestMode ("Z Test mode", Int) = 4 //4 = LEqual
        _previewColor0("Preview Color 1",Color) = (.99, .85, .92, 0)
        _colorTexture0("Color Texture 1", any) = "" {}
        _seaLevelTintColor ("Sea Level Tint Color",Color) = (.34, .85, .92, 1)
        _normalMapColorPower ("Normal Map Color Power",Float) = 0.3
        _seaLevel ("Sea Level",Float) = 0
    }

    SubShader
    {
        ZTest [_zTestMode] Cull Back ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
        // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
        #pragma exclude_renderers gles

        #include "UnityCG.cginc"
        #include "TerrainPreview.cginc"

        sampler2D _BrushTex;
        ENDCG

        Pass // 0
        {
            Name "SpawnerPreview"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _HeightmapOrig;

            float4 _previewColor0;
            float4 _previewColor1;
            float4 _previewColor2;
            float4 _previewColor3;
            float4 _previewColor4;
            sampler2D _colorTexture0;
            sampler2D _colorTexture1;
            sampler2D _colorTexture2;
            sampler2D _colorTexture3;
            sampler2D _colorTexture4;
            float4 _seaLevelTintColor;
            float _normalMapColorPower;

            float _seaLevel;

            struct v2f
            {
                float4 clipPosition : SV_POSITION;
                float3 positionWorld : TEXCOORD0;
                float3 positionWorldOrig : TEXCOORD1;
                float2 pcPixels : TEXCOORD2;
                float2 brushUV : TEXCOORD3;
                float2 heightmapUV : TEXCOORD4;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                // build a quad mesh, with one vertex per paint context pixel (pcPixel)
                float2 pcPixels = BuildProceduralQuadMeshVertex(vid);
                // compute heightmap UV and sample heightmap
                float2 heightmapUV = PaintContextPixelsToHeightmapUV(pcPixels);
                float heightmapSample = UnpackHeightmap(tex2Dlod(_Heightmap, float4(heightmapUV, 0, 0)));
                float heightmapSampleOrig = UnpackHeightmap(tex2Dlod(_HeightmapOrig, float4(heightmapUV, 0, 0)));
                // compute brush UV
                float2 brushUV = PaintContextPixelsToBrushUV(pcPixels);
                // compute object position (in terrain space) and world position
                float3 positionObject = PaintContextPixelsToObjectPosition(pcPixels, heightmapSample);
                float3 positionWorld = TerrainObjectToWorldPosition(positionObject);
                float3 positionObjectOrig = PaintContextPixelsToObjectPosition(pcPixels, heightmapSampleOrig);
                float3 positionWorldOrig = TerrainObjectToWorldPosition(positionObjectOrig);
                v2f o;
                o.pcPixels = pcPixels;
                o.positionWorld = positionWorld;
                o.positionWorldOrig = positionWorldOrig;
                o.clipPosition = UnityWorldToClipPos(positionWorld);
                o.brushUV = brushUV;
                o.heightmapUV = heightmapUV;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float brushSample = UnpackHeightmap(tex2D(_BrushTex, i.brushUV));
                float colorSample0 = UnpackHeightmap(tex2D(_colorTexture0, i.heightmapUV));
                // normal based coloring
                float3 dx = ddx(i.positionWorld);
                float3 dy = ddy(i.positionWorld);
                float3 normal = normalize(cross(dy, dx));
                float4 color = float4(0, 0, 0, 0);
                color = lerp(color, _previewColor0, _previewColor0.a * colorSample0);
                color.a *= brushSample;
                //apply sea level tint
                if (i.positionWorld.y < _seaLevel)
                {
                    color.rgb = (color.rgb * (1 - _seaLevelTintColor.a)) + (_seaLevelTintColor.rgb * _seaLevelTintColor.a);
                }
                color.rgb = color.rgb * (0.1f + 0.9f * dot(0.75f, normal / 1.15f));
                return color;
            }
            ENDCG
        }

        Pass // 1
        {
            Name "StampPreview"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _HeightmapOrig;

            float4 _positiveHeightColor;
            float4 _negativeHeightColor;
            float4 _seaLevelTintColor;
            float _normalMapColorPower;

            float _seaLevel;

            struct v2f
            {
                float4 clipPosition : SV_POSITION;
                float3 positionWorld : TEXCOORD0;
                float3 positionWorldOrig : TEXCOORD1;
                float2 pcPixels : TEXCOORD2;
                float2 brushUV : TEXCOORD3;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                // build a quad mesh, with one vertex per paint context pixel (pcPixel)
                float2 pcPixels = BuildProceduralQuadMeshVertex(vid);

                // compute heightmap UV and sample heightmap
                float2 heightmapUV = PaintContextPixelsToHeightmapUV(pcPixels);
                float heightmapSample = UnpackHeightmap(tex2Dlod(_Heightmap, float4(heightmapUV, 0, 0)));
                float heightmapSampleOrig = UnpackHeightmap(tex2Dlod(_HeightmapOrig, float4(heightmapUV, 0, 0)));

                // compute brush UV
                float2 brushUV = PaintContextPixelsToBrushUV(pcPixels);

                // compute object position (in terrain space) and world position
                float3 positionObject = PaintContextPixelsToObjectPosition(pcPixels, heightmapSample);
                float3 positionWorld = TerrainObjectToWorldPosition(positionObject);

                float3 positionObjectOrig = PaintContextPixelsToObjectPosition(pcPixels, heightmapSampleOrig);
                float3 positionWorldOrig = TerrainObjectToWorldPosition(positionObjectOrig);

                v2f o;
                o.pcPixels = pcPixels;
                o.positionWorld = positionWorld;
                o.positionWorldOrig = positionWorldOrig;
                o.clipPosition = UnityWorldToClipPos(positionWorld);
                o.brushUV = brushUV;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float brushSample = UnpackHeightmap(tex2D(_BrushTex, i.brushUV));

                // out of bounds multiplier
                float oob = all(saturate(i.brushUV) == i.brushUV) ? 1.0f : 0.0f;

                float deltaHeight = abs(i.positionWorld.y - i.positionWorldOrig.y);

                // normal based coloring
                float3 dx = ddx(i.positionWorld);
                float3 dy = ddy(i.positionWorld);
                float3 normal = normalize(cross(dy, dx));

                float3 lightDir = UnityWorldSpaceLightDir(i.positionWorld.xyz);

                float4 color;
                color.r = 1.0f;
                color.g = 1.0f;
                color.b = 1.0f;
                color.a = 1.0f;

                //positive or negative color?
                if (i.positionWorld.y - i.positionWorldOrig.y >= 0)
                {
                    //color.rgb = saturate(normal.xzy * (_positiveHeightColor.rgb + float3(-0.5f, -0.5f, 0.5f)) + 0.4f);
                    //	color.rgb = normal.xzy;// + float3(0.5f, 0.5f, 0.5f);
                    color.rgb = _positiveHeightColor.rgb;
                    color.a = _positiveHeightColor.a;
                }
                else
                {
                    //color.rgb = saturate(normal.xzy * (_negativeHeightColor.rgb + float3(1.0f, -0.5f, -0.5f)) + 0.4f);
                    //color.rgb = normal.xzy;
                    color.rgb = _negativeHeightColor.rgb;
                    color.a = _negativeHeightColor.a;
                }

                //apply sea level tint
                if (i.positionWorld.y < _seaLevel)
                {
                    color.rgb = (color.rgb * (1 - _seaLevelTintColor.a)) + (_seaLevelTintColor.rgb * _seaLevelTintColor.a);
                }

                //apply normal map influence
                color.rgb = color.rgb * (1 - _normalMapColorPower) + normal.xzy * _normalMapColorPower;

                //color.rgb = lerp(color.rgb, float3(1.0f, 1.0f, 1.0f), 0.3f);
                color.rgb = color.rgb * (0.1f + 0.9f * dot(0.75f, normal / 1.15f));
                color.a = saturate(1.0f * deltaHeight) * color.a;


                return color;
            }
            ENDCG
        }

    }
    Fallback Off
}