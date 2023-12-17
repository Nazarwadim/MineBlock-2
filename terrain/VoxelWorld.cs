using Godot;
using Godot.Collections;
using ChunkBodyGeneration;
using ProcedureGeneration;

[GlobalClass]
public partial class VoxelWorld:Node
{
    public VoxelWorld()
    {
        _chunksResources = new Dictionary<Vector2I, ChunkResource>();
        _chunksBodies = new Dictionary<Vector2I, ChunkStaticBody>();
    }
    [Export] ulong Seed;
    private Dictionary<Vector2I, ChunkResource> _chunksResources;
    private Dictionary<Vector2I, ChunkStaticBody> _chunksBodies;

    public override void _Ready()
    {
        
        ChunkDataGenerator.Seed = Seed;
        var chunkPosition = new Vector2I(0,0);
        byte[,] chunkHeights = ChunkDataGenerator.GetChunkHeightsFromNoise(chunkPosition);
        byte[, ,] chunkData = ChunkDataGenerator.GetChunkWithTerrain(chunkPosition);
        
        var chunk = new ChunkResource(chunkData, chunkPosition, chunkHeights);
        
        _chunksResources.Add(chunkPosition, chunk);
        ChunkStaticBody chunkBody = new ChunkStaticBody();
        chunkBody.MeshInstance.Mesh = ChunksMeshGenerator.GenerateChunkMesh(chunk, this);
        chunkBody.MeshInstance.MaterialOverride = GD.Load<Material>("res://textures/material.tres");
        //AddChild(chunkBody);
        
    }



    //Summary:
    //  This get you block type (Block.Type enum) from block global position.
    //  If chunk is not chunk it get you Block.Type.CobbleStone. This is for render chunk mesh and collision.
    public ChunkResource.Types GetBlockTypeInGlobalPosition(Vector3I blockGlobalPosition)
    {
        Vector2I chunkPosition = _GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition);
        ChunkResource chunkResource;
        if(_chunksResources.TryGetValue(chunkPosition, out chunkResource))
        {   
            Vector3I subPosition = blockGlobalPosition - new Vector3I(chunkPosition.X, 0, chunkPosition.Y) * ChunkDataGenerator.CHUNK_SIZE;
            return chunkResource.Data[subPosition.X,subPosition.Y,subPosition.Z];
        }
        return ChunkResource.Types.CobbleStone;
    }
    public void SetBlockTypeInGlobalPosition(Vector3I blockGlobalPosition, ChunkResource.Types blockType)
    {
        Vector2I chunkPosition = _GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition);
        ChunkResource chunkResource = _chunksResources[chunkPosition];
        Vector3I subPosition = blockGlobalPosition % ChunkDataGenerator.CHUNK_SIZE;
        chunkResource.Data[subPosition.X,subPosition.Y,subPosition.Z] = blockType;
    }
    private Vector2I _GetChunkGlobalPositionFromBlockGlobalPosition(Vector3I blockGlobalPosition)
    {
        Vector2I chunkPosition = new Vector2I(blockGlobalPosition.X / ChunkDataGenerator.CHUNK_SIZE,blockGlobalPosition.Z /ChunkDataGenerator.CHUNK_SIZE);
        if(blockGlobalPosition.X < 0)
        {
            --chunkPosition.X;
        }
        if(blockGlobalPosition.Z < 0)
        {
            --chunkPosition.Y;
        }
        return chunkPosition;
    }
}