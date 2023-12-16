using System;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class VoxelWorld:Node
{
    public VoxelWorld()
    {
        _perline_noise = new PerlineNoise(Seed);
        _chunksResources = new Dictionary<Vector2I, ChunkResource>();
        _chunksBodies = new Dictionary<Vector2I, ChunkStaticBody>();
    }
    [Export] int Seed;
    private PerlineNoise _perline_noise;
    private Dictionary<Vector2I, ChunkResource> _chunksResources;
    private Dictionary<Vector2I, ChunkStaticBody> _chunksBodies;

    public override void _Ready()
    {
        var chunkPosition = new Vector2I(0,0);
        ushort[, ,] chunkData = ChunkDataGenerator.GetChunkWithTerrain(_perline_noise, chunkPosition);
        GD.Print(chunkData);
        _chunksResources.Add(chunkPosition,new ChunkResource(chunkData, chunkPosition));
    }

}