using System;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using ProcedureGeneration;
using ChunksSerealisation;
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
        GrassBlock,
        Planks,
        Furnace,
        Godot,
        Bricks,
        Cobblestone,
        Deepslate,
        Sand,
        Gravel,
        Wood,
        DiamondBlock,
        IronBlock,
        GoldBlock,
        GoldOre ,
        CopperOre,
        Coal,
        BookShelf,
        MossyCobblestone,
        Obsidian,
        DiamondOre,
        IronOre,
        Sponge,
        OakLeaves,
        BirchLeaves,
        Grass,
        WitheredGrass
    }

    [Export]public Vector2I Position;
    [Export] private byte[] _data;

    public Types[,,] Data;
    
    //
    // Summary:
    //     Create chunk without blocks
    public ChunkResource(){}
    public ChunkResource(byte[,,] data, Vector2I position)
    {
        Position = position;
        Data = new Types[ChunkDataGenerator.CHUNK_SIZE,ChunkDataGenerator.CHUNK_HEIGHT,ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(data, 0, Data,0,data.Length);
    }

    public Error Save()
    {   
        _data = new byte[ChunkDataGenerator.CHUNK_SIZE * ChunkDataGenerator.CHUNK_HEIGHT * ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(Data, 0, _data,0, _data.Length);
        return ResourceSaver.Save(this, ChunkSaver.SAVE_PATH + GD.VarToStr(Position) + ".res", ResourceSaver.SaverFlags.Compress);
    }


    public static ChunkResource Load(Vector2I position)
    {
        try{
        ChunkResource chunk = (ChunkResource)ResourceLoader.Load(ChunkSaver.SAVE_PATH+ GD.VarToStr(position) + ".res");
        chunk.Data = new Types[ChunkDataGenerator.CHUNK_SIZE,ChunkDataGenerator.CHUNK_HEIGHT,ChunkDataGenerator.CHUNK_SIZE];
        Buffer.BlockCopy(chunk._data, 0, chunk.Data,0, chunk._data.Length);
        return chunk;
        }

        catch(NullReferenceException)
        {
            DirAccess.RemoveAbsolute(ChunkSaver.SAVE_PATH + GD.VarToStr(position) + ".res");
            return null;
        }
        
    }
}