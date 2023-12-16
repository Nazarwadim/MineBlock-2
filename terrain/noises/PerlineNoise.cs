using Godot;

public partial class PerlineNoise : FastNoiseLite
{
    public PerlineNoise(int seed)
    {
        Seed = seed;
        Frequency = 0.005f;
        FractalGain = 0.4f;
    }
}