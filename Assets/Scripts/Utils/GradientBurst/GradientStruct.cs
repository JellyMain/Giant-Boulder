using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;


namespace TerrainGenerator.GradientBurst
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GradientStruct
    {
        private const int COLORS_SIZE = 4 * 4 * 8;
        private const int COLOR_TIMES_SIZE = 2 * 8;
        private const int ALPHA_TIMES_SIZE = 2 * 8;
        private const int COLOR_COUNT_SIZE = 1;
        private const int ALPHA_COUNT_SIZE = 1;

        private const int COLORS_OFFSET = 0;
        private const int COLOR_TIMES_OFFSET = COLORS_SIZE;
        private const int ALPHA_TIMES_OFFSET = COLORS_SIZE + COLOR_TIMES_SIZE;
        private const int COLOR_COUNT_OFFSET = COLORS_SIZE + COLOR_TIMES_SIZE + ALPHA_TIMES_SIZE;
        private const int ALPHA_COUNT_OFFSET = COLORS_SIZE + COLOR_TIMES_SIZE + ALPHA_TIMES_SIZE + COLOR_COUNT_SIZE;

        [FieldOffset(COLORS_OFFSET)] private fixed byte colors[COLORS_SIZE];
        [FieldOffset(COLOR_TIMES_OFFSET)] private fixed byte colorTimes[COLOR_TIMES_SIZE];
        [FieldOffset(ALPHA_TIMES_OFFSET)] private fixed byte alphaTimes[ALPHA_TIMES_SIZE];
        [FieldOffset(COLOR_COUNT_OFFSET)] private byte colorCount;
        [FieldOffset(ALPHA_COUNT_OFFSET)] private byte alphaCount;

#if UNITY_2022_2_OR_NEWER
        private const int MODE_SIZE = 1;

        private const int MODE_OFFSET =
            COLORS_SIZE + COLOR_TIMES_SIZE + ALPHA_TIMES_SIZE + COLOR_COUNT_SIZE + ALPHA_COUNT_SIZE;

        private const int COLOR_SPACE_OFFSET = COLORS_SIZE + COLOR_TIMES_SIZE + ALPHA_TIMES_SIZE +
                                               COLOR_COUNT_SIZE + ALPHA_COUNT_SIZE + MODE_SIZE;

        [FieldOffset(MODE_OFFSET)] private byte mode;
        [FieldOffset(COLOR_SPACE_OFFSET)] private byte colorSpace;
#else
            private const int DUMMY0_SIZE = 1;
            private const int DUMMY1_SIZE = 1;

            private const int DUMMY0_OFFSET =
 COLORS_SIZE + COLOR_TIMES_SIZE + ALPHA_TIMES_SIZE + COLOR_COUNT_SIZE + ALPHA_COUNT_SIZE;
            private const int DUMMY1_OFFSET =
 COLORS_SIZE + COLOR_TIMES_SIZE + ALPHA_TIMES_SIZE + COLOR_COUNT_SIZE + ALPHA_COUNT_SIZE + DUMMY0_SIZE;
            private const int MODE_OFFSET =
 COLORS_SIZE + COLOR_TIMES_SIZE + ALPHA_TIMES_SIZE + COLOR_COUNT_SIZE + ALPHA_COUNT_SIZE + DUMMY0_SIZE + DUMMY1_SIZE;

            [FieldOffset(DUMMY0_OFFSET)] private byte dummy0;
            [FieldOffset(DUMMY1_OFFSET)] private byte dummy1;
            [FieldOffset(MODE_OFFSET)] private byte mode;
#endif


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float4* Color(int index)
        {
            fixed (byte* colorsPtr = colors) return (float4*)colorsPtr + index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort* ColorTime(int index)
        {
            fixed (byte* colorTimesPtr = colorTimes) return (ushort*)colorTimesPtr + index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort* AlphaTime(int index)
        {
            fixed (byte* alphaTimesPtr = alphaTimes) return (ushort*)alphaTimesPtr + index;
        }


        public float4 Color0
        {
            get => *Color(0);
            set => *Color(0) = value;
        }

        public float4 Color1
        {
            get => *Color(1);
            set => *Color(1) = value;
        }

        public float4 Color2
        {
            get => *Color(2);
            set => *Color(2) = value;
        }

        public float4 Color3
        {
            get => *Color(3);
            set => *Color(3) = value;
        }

        public float4 Color4
        {
            get => *Color(4);
            set => *Color(4) = value;
        }

        public float4 Color5
        {
            get => *Color(5);
            set => *Color(5) = value;
        }

        public float4 Color6
        {
            get => *Color(6);
            set => *Color(6) = value;
        }

        public float4 Color7
        {
            get => *Color(7);
            set => *Color(7) = value;
        }

        public float ColorTime0
        {
            get => *ColorTime(0) / 65535f;
            set => *ColorTime(0) = (ushort)(65535 * value);
        }

        public float ColorTime1
        {
            get => *ColorTime(1) / 65535f;
            set => *ColorTime(1) = (ushort)(65535 * value);
        }

        public float ColorTime2
        {
            get => *ColorTime(2) / 65535f;
            set => *ColorTime(2) = (ushort)(65535 * value);
        }

        public float ColorTime3
        {
            get => *ColorTime(3) / 65535f;
            set => *ColorTime(3) = (ushort)(65535 * value);
        }

        public float ColorTime4
        {
            get => *ColorTime(4) / 65535f;
            set => *ColorTime(4) = (ushort)(65535 * value);
        }

        public float ColorTime5
        {
            get => *ColorTime(5) / 65535f;
            set => *ColorTime(5) = (ushort)(65535 * value);
        }

        public float ColorTime6
        {
            get => *ColorTime(6) / 65535f;
            set => *ColorTime(6) = (ushort)(65535 * value);
        }

        public float ColorTime7
        {
            get => *ColorTime(7) / 65535f;
            set => *ColorTime(7) = (ushort)(65535 * value);
        }

        public float AlphaTime0
        {
            get => *AlphaTime(0) / 65535f;
            set => *AlphaTime(0) = (ushort)(65535 * value);
        }

        public float AlphaTime1
        {
            get => *AlphaTime(1) / 65535f;
            set => *AlphaTime(1) = (ushort)(65535 * value);
        }

        public float AlphaTime2
        {
            get => *AlphaTime(2) / 65535f;
            set => *AlphaTime(2) = (ushort)(65535 * value);
        }

        public float AlphaTime3
        {
            get => *AlphaTime(3) / 65535f;
            set => *AlphaTime(3) = (ushort)(65535 * value);
        }

        public float AlphaTime4
        {
            get => *AlphaTime(4) / 65535f;
            set => *AlphaTime(4) = (ushort)(65535 * value);
        }

        public float AlphaTime5
        {
            get => *AlphaTime(5) / 65535f;
            set => *AlphaTime(5) = (ushort)(65535 * value);
        }

        public float AlphaTime6
        {
            get => *AlphaTime(6) / 65535f;
            set => *AlphaTime(6) = (ushort)(65535 * value);
        }

        public float AlphaTime7
        {
            get => *AlphaTime(7) / 65535f;
            set => *AlphaTime(7) = (ushort)(65535 * value);
        }

        public int ColorCount
        {
            get => colorCount;
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (value < 2 || value > 8) IncorrectCount();
#endif

                colorCount = (byte)value;
            }
        }

        public int AlphaCount
        {
            get => alphaCount;
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (value < 2 || value > 8) IncorrectCount();
#endif

                alphaCount = (byte)value;
            }
        }

        public GradientMode Mode
        {
            get => (GradientMode)mode;
            set => mode = (byte)value;
        }

#if UNITY_2022_2_OR_NEWER
        public ColorSpace ColorSpace
        {
            get => colorSpace == 255 ? ColorSpace.Uninitialized : (ColorSpace)colorSpace;
            set => colorSpace = value == ColorSpace.Uninitialized ? (byte)255 : (byte)value;
        }
#endif


        public void SetColorKey(int index, GradientColorKeyBurst value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index > 7) IncorrectIndex();
#endif

            Color(index)->xyz = value.color.xyz;
            *ColorTime(index) = (ushort)(65535 * value.time);
        }


        public GradientColorKeyBurst GetColorKey(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index > 7) IncorrectIndex();
#endif

            return new GradientColorKeyBurst(*Color(index), *ColorTime(index) / 65535f);
        }


        public void SetColorKeys(NativeArray<GradientColorKeyBurst> colorKeys)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (colorKeys.Length < 2 || colorKeys.Length > 8) IncorrectLength();
#endif

            var colorKeysTmp = new NativeArray<GradientColorKeyBurst>(colorKeys, Allocator.Temp);
            colorKeysTmp.Sort<GradientColorKeyBurst, GradientColorKeyComparer>(default);

            colorCount = (byte)colorKeys.Length;

            for (var i = 0; i < colorCount; i++)
            {
                SetColorKey(i, colorKeysTmp[i]);
            }

            colorKeysTmp.Dispose();
        }


        public void SetColorKeysWithoutSort(NativeArray<GradientColorKeyBurst> colorKeys)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (colorKeys.Length < 2 || colorKeys.Length > 8) IncorrectLength();
#endif

            colorCount = (byte)colorKeys.Length;

            for (var i = 0; i < colorCount; i++)
            {
                SetColorKey(i, colorKeys[i]);
            }
        }


        public NativeArray<GradientColorKeyBurst> GetColorKeys(Allocator allocator)
        {
            var colorKeys = new NativeArray<GradientColorKeyBurst>(colorCount, allocator);

            for (var i = 0; i < colorCount; i++)
            {
                colorKeys[i] = GetColorKey(i);
            }

            return colorKeys;
        }


        public void SetAlphaKey(int index, GradientAlphaKey value)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index > 7) IncorrectIndex();
#endif

            Color(index)->w = value.alpha;
            *AlphaTime(index) = (ushort)(65535 * value.time);
        }


        public GradientAlphaKey GetAlphaKey(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index > 7) IncorrectIndex();
#endif

            return new GradientAlphaKey(Color(index)->w, *AlphaTime(index) / 65535f);
        }


        public void SetAlphaKeys(NativeArray<GradientAlphaKey> alphaKeys)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (alphaKeys.Length < 2 || alphaKeys.Length > 8) IncorrectLength();
#endif

            var alphaKeysTmp = new NativeArray<GradientAlphaKey>(alphaKeys, Allocator.Temp);
            alphaKeysTmp.Sort<GradientAlphaKey, GradientAlphaKeyComparer>(default);

            alphaCount = (byte)alphaKeys.Length;

            for (var i = 0; i < alphaCount; i++)
            {
                SetAlphaKey(i, alphaKeys[i]);
            }

            alphaKeysTmp.Dispose();
        }


        public void SetAlphaKeysWithoutSort(NativeArray<GradientAlphaKey> alphaKeys)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (alphaKeys.Length < 2 || alphaKeys.Length > 8) IncorrectLength();
#endif

            alphaCount = (byte)alphaKeys.Length;

            for (var i = 0; i < alphaCount; i++)
            {
                SetAlphaKey(i, alphaKeys[i]);
            }
        }


        public NativeArray<GradientAlphaKey> GetAlphaKeys(Allocator allocator)
        {
            var alphaKeys = new NativeArray<GradientAlphaKey>(alphaCount, allocator);

            for (var i = 0; i < alphaCount; i++)
            {
                alphaKeys[i] = GetAlphaKey(i);
            }

            return alphaKeys;
        }


        private static void IncorrectIndex() => throw new Exception("The index should be between 0 and 7.");
        private static void IncorrectCount() => throw new Exception("The number should be in the range of 2 to 8.");


        private static void IncorrectLength() =>
            throw new Exception("The length of the array must be in the range of 2 to 8.");


        public float4 Evaluate(float time)
        {
            float3 color = default;
            var colorCalculated = false;

            var colorKey = GetColorKey(0);

            if (time <= colorKey.time)
            {
                color = colorKey.color.xyz;
                colorCalculated = true;
            }

            if (!colorCalculated)
                for (var i = 0; i < colorCount - 1; i++)
                {
                    var colorKeyNext = GetColorKey(i + 1);

                    if (time <= colorKeyNext.time)
                    {
                        if (Mode == GradientMode.Blend)
                        {
                            var localTime = (time - colorKey.time) / (colorKeyNext.time - colorKey.time);
                            color = math.lerp(colorKey.color.xyz, colorKeyNext.color.xyz, localTime);
                        }
#if UNITY_2022_2_OR_NEWER
                        else if (Mode == GradientMode.PerceptualBlend)
                        {
                            var localTime = (time - colorKey.time) / (colorKeyNext.time - colorKey.time);
                            color = OklabToLinear(math.lerp(LinearToOklab(colorKey.color.xyz),
                                LinearToOklab(colorKeyNext.color.xyz), localTime));
                        }
#endif
                        else
                        {
                            color = colorKeyNext.color.xyz;
                        }

                        colorCalculated = true;
                        break;
                    }

                    colorKey = colorKeyNext;
                }

            if (!colorCalculated) color = colorKey.color.xyz;


            float alpha = default;
            var alphaCalculated = false;

            var alphaKey = GetAlphaKey(0);

            if (time <= alphaKey.time)
            {
                alpha = alphaKey.alpha;
                alphaCalculated = true;
            }

            if (!alphaCalculated)
                for (var i = 0; i < alphaCount - 1; i++)
                {
                    var alphaKeyNext = GetAlphaKey(i + 1);

                    if (time <= alphaKeyNext.time)
                    {
                        if (Mode == GradientMode.Blend
#if UNITY_2022_2_OR_NEWER
                            || Mode == GradientMode.PerceptualBlend
#endif
                           )
                        {
                            var localTime = (time - alphaKey.time) / (alphaKeyNext.time - alphaKey.time);
                            alpha = math.lerp(alphaKey.alpha, alphaKeyNext.alpha, localTime);
                        }
                        else
                        {
                            alpha = alphaKeyNext.alpha;
                        }

                        alphaCalculated = true;
                        break;
                    }

                    alphaKey = alphaKeyNext;
                }

            if (!alphaCalculated) alpha = alphaKey.alpha;

            return new float4(color, alpha);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 CubeRoot(float3 val)
        {
            return math.pow(math.abs(val), 1f / 3f) * math.sign(val);
        }


#if UNITY_2022_2_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 LinearToGammaSpace(float3 linRGB)
        {
            linRGB = math.max(linRGB, float3.zero);
            return math.max(1.055f * math.pow(linRGB, 0.416666667f) - 0.055f, 0);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 GammaToLinearSpace(float3 sRGB)
        {
            return sRGB * (sRGB * (sRGB * 0.305306011f + 0.682171111f) + 0.012522878f);
        }


        private float3 LinearToOklab(float3 c)
        {
            if (ColorSpace != ColorSpace.Linear) c = GammaToLinearSpace(c);

            var result = new float3(
                math.dot(c, new float3(0.4122214708f, 0.5363325363f, 0.0514459929f)),
                math.dot(c, new float3(0.2119034982f, 0.6806995451f, 0.1073969566f)),
                math.dot(c, new float3(0.0883024619f, 0.2817188376f, 0.6299787005f)));

            result = CubeRoot(result);

            return new float3(
                math.csum(new float3(0.2104542553f, 0.7936177850f, -0.0040720468f) * result.xyz),
                math.csum(new float3(1.9779984951f, -2.4285922050f, 0.4505937099f) * result.xyz),
                math.csum(new float3(0.0259040371f, 0.7827717662f, -0.8086757660f) * result.xyz)
            );
        }


        private float3 OklabToLinear(float3 c)
        {
            var result = new float3(
                math.csum(new float3(1, 0.3963377774f, 0.2158037573f) * c.xyz),
                math.csum(new float3(1, -0.1055613458f, -0.0638541728f) * c.xyz),
                math.csum(new float3(1, -0.0894841775f, -1.2914855480f) * c.xyz)
            );

            result = result * result * result;

            result = new float3(
                math.csum(new float3(4.0767416621f, -3.3077115913f, 0.2309699292f) * result.xyz),
                math.csum(new float3(-1.2684380046f, 2.6097574011f, -0.3413193965f) * result.xyz),
                math.csum(new float3(-0.0041960863f, -0.7034186147f, 1.7076147010f) * result.xyz)
            );

            if (ColorSpace != ColorSpace.Linear) result = LinearToGammaSpace(result);

            return result;
        }
#endif


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnly AsReadOnly(GradientStruct* data) => new ReadOnly(data);


        public readonly struct ReadOnly
        {
            [NativeDisableUnsafePtrRestriction] private readonly GradientStruct* ptr;


            public ReadOnly(GradientStruct* ptr)
            {
                this.ptr = ptr;
            }


            public int ColorCount => ptr->ColorCount;

            public int AlphaCount => ptr->AlphaCount;

            public GradientMode Mode => ptr->Mode;

#if UNITY_2022_2_OR_NEWER
            public ColorSpace ColorSpace => ptr->ColorSpace;
#endif

            public GradientColorKeyBurst GetColorKey(int index) => ptr->GetColorKey(index);


            public NativeArray<GradientColorKeyBurst> GetColorKeys(Allocator allocator) =>
                ptr->GetColorKeys(allocator);


            public GradientAlphaKey GetAlphaKey(int index) => ptr->GetAlphaKey(index);

            public NativeArray<GradientAlphaKey> GetAlphaKeys(Allocator allocator) => ptr->GetAlphaKeys(allocator);

            public float4 Evaluate(float time) => ptr->Evaluate(time);
        }

        private struct GradientColorKeyComparer : IComparer<GradientColorKeyBurst>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(GradientColorKeyBurst v1, GradientColorKeyBurst v2)
            {
                return v1.time.CompareTo(v2.time);
            }
        }

        private struct GradientAlphaKeyComparer : IComparer<GradientAlphaKey>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(GradientAlphaKey v1, GradientAlphaKey v2)
            {
                return v1.time.CompareTo(v2.time);
            }
        }
    }
}
