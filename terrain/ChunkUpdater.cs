using Godot;
using Godot.Collections;
using System;
using System.Threading;
using ProcedureGeneration;
using ChunksSerealisation;
using System.Linq;
public partial class ChunkUpdater : Node
{
    [Signal] delegate void CurrentRenderDistanseChangedEventHandler(int currentRenderDistanse);
    public int CurrentRenderDistanse{get;private set;}
    public ChunkUpdater(Dictionary<Vector2I, ChunkResource> chunksResources, Dictionary<Vector2I, ChunkStaticBody> chunksBodies, VoxelWorld world, System.Threading.Mutex mtx)
    {
        _voxelWorld = world;
        _chunksResources = chunksResources;
        _chunksBodies = chunksBodies;

        _mtx = mtx;
    }
    private const string PLAYER_SAVE_PATH = "user://PlayerTransform.bin";
    private Dictionary<Vector2I, ChunkResource> _chunksResources;
    private Dictionary<Vector2I, ChunkStaticBody> _chunksBodies;

    private VoxelWorld _voxelWorld;

    private Vector2I _middleChunkPos;
    private bool _exitLoops;
    private System.Threading.Mutex _mtx;
    private bool _stopUpdateMesh = false;
    public override void _EnterTree()
    {
        if (FileAccess.FileExists(PLAYER_SAVE_PATH))
        {
            Transform3D transform3 = (Transform3D)GD.BytesToVar(FileAccess.GetFileAsBytes(PLAYER_SAVE_PATH));
            _middleChunkPos = new Vector2I((int)transform3.Origin.X, (int)transform3.Origin.Z);
        }
        else
        {
            _middleChunkPos = new Vector2I(0, 0);
        }
    }

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
        _stopUpdateMesh = true;
        _UpdateChunkData();
        ThreadPool.QueueUserWorkItem(_UpdateChunksMesh);
        _RemoveFarChunks();
    }


    private void _UpdateChunksMesh(object obj)
    {
        try
        {
            _stopUpdateMesh = false;
            for (long cur_render = 0; cur_render <= _voxelWorld.RenderDistance; ++cur_render)
            {
                //start at top-left
                long x = _middleChunkPos.X - (cur_render / 2);
                long y = _middleChunkPos.Y- (cur_render / 2);

                //point to the right
                int dx = 1;
                int dy = 0;
                int generatedMeshes = 0;
                for (int side = 0; side < 4; ++side)
                {
                    for (int i = 1; i <= cur_render; ++i, x += dx,y += dy)
                    {
                        if(_TryGenerateChunkMesh(new Vector2I((int)x,(int)y)))
                        {
                            ++generatedMeshes;
                        }
                        
                        if(_stopUpdateMesh)
                        {
                            return;
                        }
                    }
                    //turn right
                    int t = dx;
                    dx = -dy;
                    dy = t;
                    
                }
                if(generatedMeshes > cur_render)
                {
                    CurrentRenderDistanse = (int)cur_render;
                    EmitSignal(SignalName.CurrentRenderDistanseChanged, cur_render);
                }
            }
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
        _mtx.WaitOne();
        if (_chunksBodies.ContainsKey(pos) || !_chunksResources.ContainsKey(pos))
        {
            return false;
        }
        _mtx.ReleaseMutex();
        ChunkStaticBody chunk = _voxelWorld.GenerateChunkBody(pos);
        CallDeferred("add_child", chunk);
        if (chunk != null) return true;
        return false;
    }



    private void _RemoveChunk(Vector2I chunkPosition)
    {
        _mtx.WaitOne();
        _chunksBodies[chunkPosition].QueueFree();
        _chunksBodies.Remove(chunkPosition);
        _chunksResources.Remove(chunkPosition);
        _mtx.ReleaseMutex();
    }

    private void _RemoveFarChunks()
    {

    }

    private void _UpdateChunkData()
    {
        ulong start = Time.GetTicksUsec();
        for (long i = -_voxelWorld.RenderDistance + _middleChunkPos.X; i < _voxelWorld.RenderDistance + _middleChunkPos.X; i++)
        {
            for (long j = -_voxelWorld.RenderDistance + _middleChunkPos.Y; j < _voxelWorld.RenderDistance + _middleChunkPos.Y; j++)
            {
                Vector2I chunkPosition = new Vector2I((int)i, (int)j);
                if (_chunksResources.ContainsKey(chunkPosition))
                {
                    continue;
                }
                ChunkResource chunkResource = null;
                //ChunkLoader.GetChunkResourceOrNull(chunkPosition);
                if (chunkResource == null)
                {
                    byte[,,] chunkData = ChunkDataGenerator.GetChunkWithTerrain(chunkPosition);
                    chunkResource = new ChunkResource(chunkData, chunkPosition);
                }
                if (chunkResource != null)
                {
                    _chunksResources.Add(chunkPosition, chunkResource);
                }

            }
        }

    }

}