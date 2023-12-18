using Godot;
using Godot.Collections;
using ChunkBodyGeneration;
using ProcedureGeneration;
using System.Threading;
using System;
using ChunksSerealisation;

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
    private Material _materialOverride = GD.Load<Material>("res://textures/material.tres");
    private Thread[] _thread  = new Thread[4];
    public override void _Ready()
    {
        ChunkDataGenerator.Seed = Seed;
        

        _GenerateChunkData();


        
        _thread[0] = new Thread(_GenerateChunksByThreadBackRight);
        _thread[1] = new Thread(_GenerateChunksByThreadBackLeft);
        _thread[2] = new Thread(_GenerateChunksByThreadForwardRight);
        _thread[3] = new Thread(_GenerateChunksByThreadForwardLeft);
        foreach (Thread thread in _thread)
        {
            thread.Start();
        }
        
    }

    public override void _ExitTree()
    {
        ChunkSaver.SaveStaticTerrain(_chunksResources);
        foreach (var thread in _thread)
        {
            thread.Join();
        }
    }

    private int _renderDistance = 50;
    private void _GenerateChunksByThreadForwardRight()
    {
        for(int i = 0; i < _renderDistance;i++)
        {
            for(int j =0; j > -_renderDistance; --j)
                GenerateChunk(new Vector2I(i,j));
        }
    }
    private void _GenerateChunksByThreadForwardLeft()
    {
        for(int i = 0; i > -_renderDistance;i--)
        {
            for(int j = 0; j > -_renderDistance; --j)
                GenerateChunk(new Vector2I(i,j));
        }
    }
    private void _GenerateChunksByThreadBackRight()
    {
        for(int i = 0; i < _renderDistance;i++)
        {
            for(int j = 0; j < _renderDistance;j++)
                GenerateChunk(new Vector2I(i,j));
        }
    }
    private void _GenerateChunksByThreadBackLeft()
    {
        for(int i = 0 ; i > -_renderDistance;i--)
        {
            for(int j = 0; j < _renderDistance;j++)
                GenerateChunk(new Vector2I(i,j));
        }
    }



    private void _GenerateChunkData()
    {
        ulong start = Time.GetTicksMsec();
        for(int i = -_renderDistance; i < _renderDistance; i++)
        {
            for(int j = -_renderDistance;  j < _renderDistance; j++)
            {   
                Vector2I chunkPosition = new Vector2I(i,j);
                ChunkResource chunkResource = ChunkLoader.GetChunkResourceOrNull(chunkPosition);
                if(chunkResource == null)
                {
                    byte[, ,] chunkData = ChunkDataGenerator.GetChunkWithTerrain(chunkPosition);
                    chunkResource = new ChunkResource(chunkData, new Vector2I(i,j));
                }
                _chunksResources.Add(chunkPosition, chunkResource);
                
            }
        }
        GD.Print(Time.GetTicksMsec() - start);
    }
    private Vector2I chunkPosition;
    public void GenerateChunk(Vector2I chunkPosition)
    {   
        ChunkResource chunk = _chunksResources[chunkPosition];
        ChunkStaticBody chunkBody = new()
        {
            Position = new Vector3(chunkPosition.X * ChunkDataGenerator.CHUNK_SIZE, 0, chunkPosition.Y * ChunkDataGenerator.CHUNK_SIZE)
        };
        chunkBody.MeshInstance.Mesh = ChunksMeshGenerator.GenerateChunkMesh(chunk, this);       
        chunkBody.MeshInstance.MaterialOverride = _materialOverride;
        CallDeferred("add_child", chunkBody);
        
        //chunkBody.MeshInstance.CallDeferred("create_trimesh_collision");
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
        if(blockGlobalPosition.X < 0 && blockGlobalPosition.X % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.X;
        }
        if(blockGlobalPosition.Z < 0 && blockGlobalPosition.Z % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.Y;
        }
        return chunkPosition;
    }
    
}