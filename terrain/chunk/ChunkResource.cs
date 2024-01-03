using System;
using Godot;
using ProcedureGeneration;
using ChunksSerialization;

// Summary:
//     This is Chunk container resource that can be saved.

[GlobalClass]
public partial class ChunkResource : Resource
{
    [Export] public Vector2I Position;
    [Export] private byte[] _data;

    public ChunkDataGenerator.BlockTypes[,,] Data;

    //
    // Summary:
    //     Create chunk without blocks
    public ChunkResource() { }

    public void CopyFromOneDimensionalIntoThreeDimensional()
    {
        Data = new ChunkDataGenerator.BlockTypes[ChunkDataGenerator.CHUNK_SIZE, ChunkDataGenerator.CHUNK_HEIGHT, ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(_data, 0, Data, 0, _data.Length);
    }
    
    public ChunkResource(byte[,,] data, Vector2I position)
    {
        Position = position;
        Data = new ChunkDataGenerator.BlockTypes[ChunkDataGenerator.CHUNK_SIZE, ChunkDataGenerator.CHUNK_HEIGHT, ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(data, 0, Data, 0, data.Length);
    }

    public Error Save(string resourcePath)
    {
        
        _data = new byte[ChunkDataGenerator.CHUNK_SIZE * ChunkDataGenerator.CHUNK_HEIGHT * ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(Data, 0, _data, 0, _data.Length);
        Error error = ResourceSaver.Save(this, resourcePath, ResourceSaver.SaverFlags.Compress);
        _data = null;
        return error;
    }

    public void Load(string resourcePath)
    {
        ChunkResource chunk = (ChunkResource)ResourceLoader.Load(resourcePath);
        if(chunk._data.Length != ChunkDataGenerator.CHUNK_SIZE * ChunkDataGenerator.CHUNK_HEIGHT * ChunkDataGenerator.CHUNK_SIZE)
        {
            throw new Exception();
        }
        Position = chunk.Position;
        Data = new ChunkDataGenerator.BlockTypes[ChunkDataGenerator.CHUNK_SIZE, ChunkDataGenerator.CHUNK_HEIGHT, ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(chunk._data, 0, Data, 0, chunk._data.Length);
    }
}