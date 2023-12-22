using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using ProcedureGeneration;
using ChunksSerealisation;
using System.Threading;
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
    public ChunkResource(byte[,,] data, Vector2I position)
    {
        Position = position;
        Data = new ChunkDataGenerator.BlockTypes[ChunkDataGenerator.CHUNK_SIZE, ChunkDataGenerator.CHUNK_HEIGHT, ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(data, 0, Data, 0, data.Length);
    }

    public Error Save()
    {
        _data = new byte[ChunkDataGenerator.CHUNK_SIZE * ChunkDataGenerator.CHUNK_HEIGHT * ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(Data, 0, _data, 0, _data.Length);
        Error error = ResourceSaver.Save(this, ChunkSaver.SAVE_PATH + GD.VarToStr(Position) + ".res", ResourceSaver.SaverFlags.Compress);
        _data = null;
        return error;
    }


    public static ChunkResource Load(Vector2I position)
    {
        try
        {
            ChunkResource chunk = (ChunkResource)ResourceLoader.Load(ChunkSaver.SAVE_PATH + GD.VarToStr(position) + ".res");
            chunk.Data = new ChunkDataGenerator.BlockTypes[ChunkDataGenerator.CHUNK_SIZE, ChunkDataGenerator.CHUNK_HEIGHT, ChunkDataGenerator.CHUNK_SIZE];
            Buffer.BlockCopy(chunk._data, 0, chunk.Data, 0, chunk._data.Length);
            chunk._data = null;
            return chunk;
        }
        catch (NullReferenceException)
        {
            DirAccess.RemoveAbsolute(ChunkSaver.SAVE_PATH + GD.VarToStr(position) + ".res");
            return null;
        }

    }
}