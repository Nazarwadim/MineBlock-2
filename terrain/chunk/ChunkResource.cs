using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using ProcedureGeneration;
// Summary:
//     This is Chunk container resource that can be saved.

[GlobalClass]
public partial class ChunkResource : Resource
{
    public enum Types : byte
    {
        Air,
        CobbleStone,
        Dirt,
        Grass,
        Planks,
        Furnace,
        
    }
    [Export]public Vector2I Position;
    [Export] private byte[] _data;

    public Types[,,] Data;
    public byte[,] HeightMap;
    
    //
    // Summary:
    //     Create chunk without blocks
    public ChunkResource(){}
    public ChunkResource(byte[,,] data, Vector2I position, byte[,] heightMap)
    {
        Position = position;
        HeightMap = heightMap;
        Data = new Types[ChunkDataGenerator.CHUNK_SIZE,ChunkDataGenerator.CHUNK_HEIGHT,ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(data, 0, Data,0,data.Length);
    }
    //Summary:
    // Use this method for saving!
    public void Save()
    {
        _data = new byte[ChunkDataGenerator.CHUNK_SIZE * ChunkDataGenerator.CHUNK_HEIGHT * ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(Data, 0, _data,0, _data.Length);
        ResourceSaver.Save(this, "res://chunksRes/" + GD.VarToStr(Position) + ".tres", ResourceSaver.SaverFlags.Compress);
    }

    //Summary:
    // Use this method for loading!
    public static ChunkResource Load(Vector2I position)
    {
        
        ChunkResource chunk = (ChunkResource)ResourceLoader.Load("res://chunksRes/"+ GD.VarToStr(position) + ".tres");
        chunk.Data = new Types[ChunkDataGenerator.CHUNK_SIZE,ChunkDataGenerator.CHUNK_HEIGHT,ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(chunk._data, 0, chunk.Data,0, chunk._data.Length);
        return chunk;
    }
}