using Godot;
using Godot.Collections;
using ChunkBodyGeneration;
using ProcedureGeneration;
using System;

[GlobalClass]
public partial class VoxelWorld : Node
{
    
    [Signal] public delegate void MiddleChunkPositionChangedEventHandler(Vector2I position);
    public VoxelWorld()
    {
        _chunksResources = new Dictionary<Vector2I, ChunkResource>();
        _chunksBodies = new Dictionary<Vector2I, ChunkStaticBody>();
        _middleChunkPos = new Vector2I();
    }
    [Export] ulong Seed;
    
    [Export] public int RenderDistance = 10;
    [Export] public float FogDensity;
    [Export] private WorldEnvironment _worldEnvironment;
    
    private Dictionary<Vector2I, ChunkResource> _chunksResources;
    private Dictionary<Vector2I, ChunkStaticBody> _chunksBodies;
    private Material _materialOverride = GD.Load<Material>("res://textures/material.tres");
    private ChunkUpdater _chunkUpdater;
    private Vector2I _middleChunkPos;
    public override void _Ready()
    {
        if(_worldEnvironment == null)
        {
            GD.PrintErr("Set world environment!!!! VoxelWorld.cs line 33");
            GetTree().Quit();
        }
        
        ChunkDataGenerator.Seed = Seed;
        _chunkUpdater = new ChunkUpdater(_chunksResources, _chunksBodies, this);
        AddChild(_chunkUpdater);
        
    }

    
    private void _OnPlayerPositioXYChanged(Vector2I position)
    {
        Vector2I chunkPos = GetChunkGlobalPositionFromBlockGlobalPosition(position);
        if (_middleChunkPos != chunkPos)
        {
            _middleChunkPos = chunkPos;
            EmitSignal(SignalName.MiddleChunkPositionChanged, _middleChunkPos);
        }
        else if(_middleChunkPos == Vector2I.Zero)
        {
            _middleChunkPos = chunkPos;
            EmitSignal(SignalName.MiddleChunkPositionChanged, _middleChunkPos);
        }
    }


    public ChunkStaticBody GenerateChunkBody(Vector2I chunkPosition)
    {
        ChunkResource chunk = _chunksResources[chunkPosition];
        Mesh mesh = ChunksMeshGenerator.GenerateChunkMesh(chunk, this);
        ChunkStaticBody chunkBody = new ChunkStaticBody(
            mesh,
            new Vector3(chunkPosition.X * ChunkDataGenerator.CHUNK_SIZE, 0, chunkPosition.Y * ChunkDataGenerator.CHUNK_SIZE)
        );
        chunkBody.MeshInstance.MaterialOverride = _materialOverride;
        
        return chunkBody;
    }



    //Summary:
    //  This get you block type (Block.Type enum) from block global position.
    //  If chunk is not chunk it get you Block.Type.CobbleStone. This is for render chunk mesh and collision.
    public ChunkDataGenerator.BlockTypes GetBlockTypeInGlobalPosition(Vector3I blockGlobalPosition)
    {
        Vector2I chunkPosition = GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition);
        ChunkResource chunkResource;
        if (_chunksResources.TryGetValue(chunkPosition, out chunkResource))
        {
            Vector3I subPosition = blockGlobalPosition - new Vector3I(chunkPosition.X, 0, chunkPosition.Y) * ChunkDataGenerator.CHUNK_SIZE;
            return chunkResource.Data[subPosition.X, subPosition.Y, subPosition.Z];
        }
        return ChunkDataGenerator.BlockTypes.CobbleStone;
    }
    public void SetBlockTypeInGlobalPosition(Vector3I blockGlobalPosition, ChunkDataGenerator.BlockTypes blockType)
    {
        Vector2I chunkPosition = GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition);
        ChunkResource chunkResource = _chunksResources[chunkPosition];
        Vector3I subPosition = blockGlobalPosition % ChunkDataGenerator.CHUNK_SIZE;
        chunkResource.Data[subPosition.X, subPosition.Y, subPosition.Z] = blockType;
    }
    public static Vector2I GetChunkGlobalPositionFromBlockGlobalPosition(Vector3I blockGlobalPosition)
    {
        Vector2I chunkPosition = new Vector2I(blockGlobalPosition.X / ChunkDataGenerator.CHUNK_SIZE, blockGlobalPosition.Z / ChunkDataGenerator.CHUNK_SIZE);
        if (blockGlobalPosition.X < 0 && blockGlobalPosition.X % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.X;
        }
        if (blockGlobalPosition.Z < 0 && blockGlobalPosition.Z % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.Y;
        }
        return chunkPosition;
    }
    public static Vector2I GetChunkGlobalPositionFromBlockGlobalPosition(Vector2I blockGlobalPosition)
    {
        Vector2I chunkPosition = blockGlobalPosition / ChunkDataGenerator.CHUNK_SIZE;

        if (blockGlobalPosition.X < 0 && blockGlobalPosition.X % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.X;
        }
        if (blockGlobalPosition.Y < 0 && blockGlobalPosition.Y % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.Y;
        }
        return chunkPosition;
    }

}