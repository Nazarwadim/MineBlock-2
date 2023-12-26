using Godot;
using Godot.Collections;
using System;
using System.Threading;
using ProcedureGeneration;
using ChunkBodyGeneration;
using ChunksSerealisation;

using Generic = System.Collections.Generic;
public partial class ChunkUpdater : Node
{

    [Signal] public delegate void CurrentRenderDistanseChangedEventHandler(int currentRenderDistanse);
    public int CurrentRenderDistanse { get; private set; }
    public ChunkUpdater(Dictionary<Vector2I, ChunkResource> chunksResources, Dictionary<Vector2I, ChunkStaticBody> chunksBodies, VoxelWorld world)
    {
        _voxelWorld = world;
        _chunksResources = chunksResources;
        _chunksBodies = chunksBodies;
        _mtxDic = new();
        _mtxUpdateQueue = new();
        _ChangedChunksToSave = new();
        _ChangedChunksToUpdate = new();
    }
    public ChunkUpdater(Dictionary<Vector2I, ChunkResource> chunksResources, Dictionary<Vector2I, ChunkStaticBody> chunksBodies, VoxelWorld world, Vector2I midleChunkPos) : this(chunksResources, chunksBodies, world)
    {
        _middleChunkPos = midleChunkPos;
    }
    private Dictionary<Vector2I, ChunkResource> _chunksResources;
    private Dictionary<Vector2I, ChunkStaticBody> _chunksBodies;

    private VoxelWorld _voxelWorld;

    private Vector2I _middleChunkPos = new();
    private bool _exitLoops;
    private Thread _thread1;
    private bool _updatingChunksQueue = false;
    private System.Threading.Mutex _mtxDic;
    private System.Threading.Mutex _mtxUpdateQueue;
    public bool IsUpdatingChunks { get; private set; }
    private readonly Generic.Queue<ChunkResource> _ChangedChunksToSave;
    private readonly Generic.Queue<Generic.KeyValuePair<ChunkResource, ChunkStaticBody>> _ChangedChunksToUpdate;
    public override void _Ready()
    {
        _voxelWorld.MiddleChunkPositionChanged += _OnMidleChunkPositionChanged;
        _UpdateChunks();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ChangedChunksToUpdate.Count > 0 && !_updatingChunksQueue)
        {
            Thread thread = new Thread(_UpdateChunkBodies);
            thread.Start();
        }
    }

    private void _OnMidleChunkPositionChanged(Vector2I chunk_position)
    {

        _middleChunkPos = chunk_position;
        _UpdateChunks();
    }

    public void SaveChangedChunks()
    {
        while (_ChangedChunksToSave.Count > 0)
        {
            ChunkResource chunkResource = _ChangedChunksToSave.Dequeue();
            ChunkSaver.SaveChunk(chunkResource);
        }
    }


    private void _UpdateChunks()
    {

        _SafeStopAndWaitToChunkBodyGeneration();
        _StartChunkBodyGenerationByThread();

    }

    private void _SafeStopAndWaitToChunkBodyGeneration()
    {
        if (_thread1 != null && _thread1.IsAlive)
        {
            IsUpdatingChunks = false;
            _thread1.Join();
        }
    }
    private void _StartChunkBodyGenerationByThread()
    {
        if (_thread1 != null && _thread1.IsAlive)
        {
            throw new Exception("Can`t start body generation thread. Firstly wait to finish thread (_SafeStopAndWaitToChunkBodyGeneration()) ChunkUpdater.cs line 65");
        }
        IsUpdatingChunks = true;
        _thread1 = new Thread(_UpdateChunksByThread);
        _thread1.Start();
    }
    private void _UpdateChunksByThread()
    {
        try
        {
            for (long cur_render = -1; cur_render <= _voxelWorld.RenderDistance; cur_render += 2)
            {
                //start at top-left
                long x1 = _middleChunkPos.X - ((cur_render + 2) / 2);
                long y1 = _middleChunkPos.Y - ((cur_render + 2) / 2);
                long y2 = _middleChunkPos.Y - (cur_render / 2);
                long x2 = _middleChunkPos.X - (cur_render / 2);
                //point to the right
                long dx1 = 1;
                long dy1 = 0;
                long dx2 = dx1;
                long dy2 = dy1;
                int generatedMeshes = 0;
                for (int side = 0; side < 4; ++side)
                {
                    for (long i = 1; i <= cur_render + 2 && IsUpdatingChunks; ++i, x1 += dx1, y1 += dy1)
                    {
                        Vector2I chunkPosition = new Vector2I((int)x1, (int)y1);

                        _TryGenerateChunkResource(chunkPosition);
                    }
                    //turn right
                    long t = dx1;
                    dx1 = -dy1;
                    dy1 = t;

                }
                for (int side = 0; side < 4; ++side)
                {
                    for (long i = 1; i <= cur_render && IsUpdatingChunks; ++i, x2 += dx2, y2 += dy2)
                    {
                        Vector2I chunkPosition = new Vector2I((int)x2, (int)y2);

                        if (_TryGenerateChunkBody(chunkPosition))
                        {
                            ++generatedMeshes;
                        }

                    }
                    //turn right
                    long t = dx2;
                    dx2 = -dy2;
                    dy2 = t;
                }

                if (generatedMeshes > cur_render && cur_render >= 0)
                {
                    CurrentRenderDistanse = (int)cur_render;

                    CallDeferred("emit_signal", SignalName.CurrentRenderDistanseChanged, cur_render);
                }
            }
            _RemoveFarChunksBodies();
            _RemoveFarChunksResources();

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

    private bool _TryGenerateChunkResource(Vector2I chunkPosition)
    {
        if (_chunksResources.ContainsKey(chunkPosition)) return false;
        ChunkResource chunkResource = null;
        if (_voxelWorld.IsSerialization)
        {
            chunkResource = ChunkLoader.GetChunkResourceOrNull(chunkPosition);
        }
        if (chunkResource == null)
        {
            byte[,,] chunkData = ChunkDataGenerator.GetChunkWithTerrain(chunkPosition);
            chunkResource = new ChunkResource(chunkData, chunkPosition);
        }
        if (chunkResource != null)
        {
            _chunksResources.Add(chunkPosition, chunkResource);
            if (_DistanceSquaredFromToVector2I(chunkPosition, _middleChunkPos) < _voxelWorld.RenderDistance)
            {
                if (!_ChangedChunksToSave.Contains(chunkResource))
                {
                    _ChangedChunksToSave.Enqueue(chunkResource);
                }
            }
        }
        else
        {
            throw new Exception("Bad chunk resource generation. ChunkUpdater.cs _UpdateChunksByThread");
        }
        return true;
    }

    private bool _TryGenerateChunkBody(Vector2I pos)
    {
        if (_chunksBodies.ContainsKey(pos))
        {
            return false;
        }
        ChunkStaticBody chunk = _voxelWorld.GenerateChunkBodyUsingThreads(pos);
        CallDeferred("add_child", chunk);
        _mtxDic.WaitOne();
        _chunksBodies.Add(pos, chunk);
        _mtxDic.ReleaseMutex();
        return true;
    }

    public void UpdateChunkBody(Vector2I chunkPos)
    {

        _mtxDic.WaitOne();
        ChunkResource chunkResource = _chunksResources[chunkPos];
        ChunkStaticBody chunkStaticBody = _chunksBodies[chunkPos];
        _mtxDic.ReleaseMutex();

        var chunkPair = new Generic.KeyValuePair<ChunkResource, ChunkStaticBody>(chunkResource, chunkStaticBody);
        _mtxUpdateQueue.WaitOne();
        if (!_ChangedChunksToUpdate.Contains(chunkPair))
        {
            _ChangedChunksToUpdate.Enqueue(chunkPair);
        }
        _mtxUpdateQueue.ReleaseMutex();
        if (_voxelWorld.IsSerialization)
        {
            if (!_ChangedChunksToSave.Contains(chunkResource))
            {
                _ChangedChunksToSave.Enqueue(chunkResource);
            }
        }
    }
    public void _UpdateChunkBodies()
    {
        _updatingChunksQueue = true;
        while (_ChangedChunksToUpdate.Count > 0)
        {
            _mtxUpdateQueue.WaitOne();
            Generic.KeyValuePair<ChunkResource, ChunkStaticBody> chunkPair = _ChangedChunksToUpdate.Dequeue();
            _mtxUpdateQueue.ReleaseMutex();
            ThreadPool.QueueUserWorkItem((object obj) => _UpdateChunkBody(chunkPair.Value, chunkPair.Key));
        }
        _updatingChunksQueue = false;
    }
    private void _UpdateChunkBody(ChunkStaticBody chunkStaticBody, ChunkResource chunkResource)
    {
        System.Threading.Tasks.Task<Shape3D> colisionTask = System.Threading.Tasks.Task<Shape3D>.Factory.StartNew(() =>
                ChunksShapeGenerator.GenerateChunkShape(chunkResource, _voxelWorld));
        Mesh mesh = ChunksMeshGenerator.GenerateChunkMesh(chunkResource, _voxelWorld);
        System.Threading.Tasks.Task.WaitAll(colisionTask);
        Shape3D shape = colisionTask.Result;
        CallDeferred("_SetChunkBodyMeshAndShape", chunkStaticBody, mesh, shape);
    }
    private void _SetChunkBodyMeshAndShape(ChunkStaticBody chunkStaticBody, Mesh mesh, Shape3D shape3D)
    {
        chunkStaticBody.MeshInstance.Mesh = mesh;
        chunkStaticBody.ColisionShape.Shape = shape3D;
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
            if (_DistanceSquaredFromToVector2I(chunkPosition, _middleChunkPos) > 25 * _voxelWorld.RenderDistance * _voxelWorld.RenderDistance)
            {
                _chunksResources.Remove(chunkPosition);
            }
        }
    }

    private void _RemoveFarChunksBodies()
    {
        foreach (Vector2I chunkPosition in _chunksBodies.Keys)
        {
            if (_DistanceSquaredFromToVector2I(chunkPosition, _middleChunkPos) > _voxelWorld.RenderDistance * _voxelWorld.RenderDistance)
            {
                _RemoveChunkBody(chunkPosition);

            }
        }

    }

    private int _DistanceSquaredFromToVector2I(Vector2I from, Vector2I to)
    {
        return (to.X - from.X) * (to.X - from.X) + (to.Y - from.Y) * (to.Y - from.Y);
    }


}