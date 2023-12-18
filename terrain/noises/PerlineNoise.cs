using Godot;

public partial class PerlineNoise : FastNoiseLite
{
    public PerlineNoise(int seed)
    {
        Seed = seed;
        Frequency = 0.005f;
        FractalOctaves = 6;
        FractalGain = 0.7f;
        FractalLacunarity = 1.35f;
    }
}