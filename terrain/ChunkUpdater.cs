using Godot;
using Godot.Collections;
using System;
using System.Threading;
using ProcedureGeneration;
using ChunkBodyGeneration;
using ChunksSerealisation;
public partial class ChunkUpdater : Node
{
    
    [Signal] public delegate void CurrentRenderDistanseChangedEventHandler(int currentRenderDistanse);
    public int CurrentRenderDistanse{get;private set;}
    public ChunkUpdater(Dictionary<Vector2I, ChunkResource> chunksResources, Dictionary<Vector2I, ChunkStaticBody> chunksBodies, VoxelWorld world)
    {
        _voxelWorld = world;
        _chunksResources = chunksResources;
        _chunksBodies = chunksBodies;
        _mtx = new(true);
    }
    private Dictionary<Vector2I, ChunkResource> _chunksResources;
    private Dictionary<Vector2I, ChunkStaticBody> _chunksBodies;

    private VoxelWorld _voxelWorld;

    private Vector2I _middleChunkPos;
    private bool _exitLoops;
    private Thread _thread1;
    private Thread _thread2;
    private System.Threading.Mutex _mtx;
    public bool IsUpdatingChunks{get; private set;}

    public override void _Ready()
    {
        _voxelWorld.MiddleChunkPositionChanged += _OnMidleChunkPositionChanged;
    }


    private void _OnMidleChunkPositionChanged(Vector2I chunk_position)
    {
        _middleChunkPos = chunk_position;
        _UpdateChunks();
    }
    
    private void _UpdateChunks()
    {
        
        if(_thread1 != null && _thread1.IsAlive)
        {
            IsUpdatingChunks = false;
            _thread1.Join();   
        }
        _UpdateChunkData();
        IsUpdatingChunks = true;
        _thread1 = new Thread(_UpdateChunksMesh);
        _thread1.Start();
        
        
        
    }


    private void _UpdateChunksMesh()
    {
        try
        {
            _RemoveFarChunksBodies();
            for (long cur_render = 0; cur_render <= _voxelWorld.RenderDistance; ++cur_render)
            {
                //start at top-left
                long x = _middleChunkPos.X - (cur_render / 2);
                long y = _middleChunkPos.Y- (cur_render / 2);

                //point to the right
                long dx = 1;
                long dy = 0;
                int generatedMeshes = 0;
                for (int side = 0; side < 4; ++side)
                {
                    for (long i = 1; i <= cur_render && IsUpdatingChunks; ++i, x += dx,y += dy)
                    {
                        if(_TryGenerateChunkMesh(new Vector2I((int)x,(int)y)))
                        {
                            ++generatedMeshes;
                        }                        
                    }
                    //turn right
                    long t = dx;
                    dx = -dy;
                    dy = t;
                    
                }
                if(generatedMeshes >= cur_render/1.5f)
                {
                    CurrentRenderDistanse = (int)cur_render;
                    
                    CallDeferred("emit_signal",SignalName.CurrentRenderDistanseChanged, cur_render);
                }
            }

            IsUpdatingChunks = false;
        }
        catch (Exception ex)
        {
            GD.PrintErr(ex);
        }

    }
    //Summary:
    //  Check if can generate mesh
    // Returns:
    //     Whether or not generated.
    private bool _TryGenerateChunkMesh(Vector2I pos)
    {
        if (_chunksBodies.ContainsKey(pos) || !_chunksResources.ContainsKey(pos))
        {
            return false;
        }
        ChunkStaticBody chunk = _voxelWorld.GenerateChunkBody(pos);
        CallDeferred("add_child", chunk);
        _chunksBodies.Add(pos,chunk);
        //ChunkSaver.SaveChunk(_chunksResources[pos]);
        if (chunk != null) return true;
        return false;
    }



    private void _RemoveChunkBody(Vector2I chunkPosition)
    {
        _chunksBodies[chunkPosition].CallDeferred("queue_free");
        _chunksBodies.Remove(chunkPosition);
    }

    private void _RemoveFarChunksResources()
    {
        foreach (Vector2I chunkPosition in _chunksResources.Keys)
        {
            if(_DistanceSquaredFromToVector2I(chunkPosition, _middleChunkPos) > _voxelWorld.RenderDistance * _voxelWorld.RenderDistance)
            {
                _chunksResources.Remove(chunkPosition);
            }
        }
    }

    private void _RemoveFarChunksBodies()
    {
        foreach (Vector2I chunkPosition in _chunksBodies.Keys)
        {
            if(_DistanceSquaredFromToVector2I(chunkPosition, _middleChunkPos) > _voxelWorld.RenderDistance * _voxelWorld.RenderDistance)
            {
                _RemoveChunkBody(chunkPosition);
                
            }
        }
        
    }

    private int _DistanceSquaredFromToVector2I(Vector2I from, Vector2I to)
    {
        return (to.X - from.X)*(to.X - from.X) + (to.Y - from.Y)*(to.Y - from.Y);
    }

    private void _UpdateChunkData()
    {
        ulong start = Time.GetTicksUsec();
        for (long i = -_voxelWorld.RenderDistance + _middleChunkPos.X; i <= _voxelWorld.RenderDistance + _middleChunkPos.X; ++i)
        {
            for (long j = -_voxelWorld.RenderDistance + _middleChunkPos.Y; j <= _voxelWorld.RenderDistance + _middleChunkPos.Y; ++j)
            {
                Vector2I chunkPosition = new Vector2I((int)i, (int)j);
                if (_chunksResources.ContainsKey(chunkPosition))
                {
                    continue;
                }
                ChunkResource chunkResource = ChunkLoader.GetChunkResourceOrNull(chunkPosition);
                if (chunkResource == null)
                {
                    byte[,,] chunkData = ChunkDataGenerator.GetChunkWithTerrain(chunkPosition);
                    chunkResource = new ChunkResource(chunkData, chunkPosition);
                }
                if (chunkResource != null)
                {
                    _mtx.WaitOne();
                    _chunksResources.Add(chunkPosition, chunkResource);
                    _mtx.ReleaseMutex();
                }

            }
        }

    }
    

}