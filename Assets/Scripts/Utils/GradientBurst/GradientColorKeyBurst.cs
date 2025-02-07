using Unity.Mathematics;


namespace TerrainGenerator.GradientBurst
{
    public readonly struct GradientColorKeyBurst
    {
        public readonly float4 color;
        public readonly float time;


        public GradientColorKeyBurst(float4 color, float time)
        {
            this.color = color;
            this.time = time;
        }
    }
}
