using Godot;



namespace Terrain.ProcedureGeneration
{
    public partial class PerlineNoise : FastNoiseLite
    {
        public PerlineNoise(int seed)
        {
            NoiseType = NoiseTypeEnum.Perlin;
            FractalType = FractalTypeEnum.Fbm;
            Seed = seed;
            Frequency = 0.005f;
            FractalOctaves = 6;
            FractalGain = 0.7f;
            FractalLacunarity = 1.35f;
        }
        public void SetSettingsByBiome(ChunkDataGenerator.Biomes biome)
        {
            switch (biome)
            {
                case ChunkDataGenerator.Biomes.Ocean:
                    Frequency = 0.005f;
                    FractalOctaves = 6;
                    FractalGain = 0.7f;
                    FractalLacunarity = 1.35f;
                    break;
                case ChunkDataGenerator.Biomes.Mountains:
                    Frequency = 0.004f;
                    FractalOctaves = 4;
                    FractalGain = 0.5f;
                    FractalLacunarity = 2.7f;
                    break;
            }
        }
    }
}