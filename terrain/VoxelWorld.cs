using Godot;
using Godot.Collections;
using ChunkBodyGeneration;
using ProcedureGeneration;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class VoxelWorld : Node
{
    
    [Signal] public delegate void MiddleChunkPositionChangedEventHandler(Vector2I position);
    [Signal] public delegate void CurrentRenderDistanseChangedEventHandler(int currentRenderDistanse);
    public VoxelWorld()
    {
        ChunksResources = new Dictionary<Vector2I, ChunkResource>();
        ChunksBodies = new Dictionary<Vector2I, ChunkStaticBody>();
        _middleChunkPos = new Vector2I();
    }
    [Export] ulong Seed;
    [Export] public int RenderDistance = 10;
    [Export] private WorldEnvironment _worldEnvironment;
    [Export] private Node3D _generationRelativePlayer;
    [Export]private ChunkUpdater _chunkUpdater;
    [Export] public bool IsSerialization;
    
    [Export] public string WorldName{get; private set;}
    public readonly Dictionary<Vector2I, ChunkResource> ChunksResources;
    public readonly Dictionary<Vector2I, ChunkStaticBody> ChunksBodies;
    
    
    private Vector2I _middleChunkPos;
    public override void _Ready()
    {
        if(_generationRelativePlayer == null)
        {
            GD.PrintErr("Set base player! VoxelWorld.cs Ready()");
            GetTree().Quit();
        }
        if(_worldEnvironment == null)
        {
            GD.PrintErr("Set world environment!!!! VoxelWorld.cs Ready()");
            GetTree().Quit();
        }
        if(_chunkUpdater == null)
        {
            GD.PrintErr("Set ChunkUpdater!!!! VoxelWorld.cs Ready()");
            GetTree().Quit();
        }

        ChunkDataGenerator.Seed = Seed;
        _chunkUpdater.CurrentRenderDistanseChanged += _OnCurrentRenderDistanceChanged;
        Signal signal = (Signal)_generationRelativePlayer.Call("get_position_XY_changed");
        _generationRelativePlayer.Connect(signal.Name, new Callable(this, MethodName._OnPlayerPositioXYChanged));
    }
    private void _OnCurrentRenderDistanceChanged(int renderDistance)
    {
        EmitSignal(SignalName.CurrentRenderDistanseChanged,renderDistance);
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

    public void Save()
    {
        if(IsSerialization) _chunkUpdater.SaveChangedChunks();
        else throw new Exception("Trying to save chunks, when it doesn`t allowed. Set IsSerialization var to true!");
    }
    // Summary:
    //      This get you block type (Block.Type enum) from block global position.
    //      If chunk is not chunk it get you Block.Type.CobbleStone. This is for render chunk mesh and collision.
    
    public void SetBlockTypeInGlobalPosition(Vector3I blockGlobalPosition, ChunkDataGenerator.BlockTypes blockType)
    {
        Vector2I chunkPosition = GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition);
        ChunkResource chunkResource = ChunksResources[chunkPosition];
        Vector3I subPosition = blockGlobalPosition - new Vector3I(chunkPosition.X, 0, chunkPosition.Y) * ChunkDataGenerator.CHUNK_SIZE;
        chunkResource.Data[subPosition.X, subPosition.Y, subPosition.Z] = blockType;
        _chunkUpdater.UpdateChunkBody(chunkPosition);
        if(subPosition.X == 0)
        {
            _chunkUpdater.UpdateChunkBody(new Vector2I(chunkPosition.X - 1, chunkPosition.Y));
        }
        if(subPosition.X == ChunkDataGenerator.CHUNK_SIZE -1)
        {
            _chunkUpdater.UpdateChunkBody(new Vector2I(chunkPosition.X + 1, chunkPosition.Y));
        }

        if(subPosition.Z == 0)
        {
            _chunkUpdater.UpdateChunkBody(new Vector2I(chunkPosition.X , chunkPosition.Y - 1));
        }
        if(subPosition.Z == ChunkDataGenerator.CHUNK_SIZE - 1)
        {
            _chunkUpdater.UpdateChunkBody(new Vector2I(chunkPosition.X, chunkPosition.Y + 1));
        }
    }
    public static Vector2I GetChunkGlobalPositionFromBlockGlobalPosition(Vector3I blockGlobalPosition)
    {
       return GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition.X, blockGlobalPosition.Z);
    }
    public static Vector2I GetChunkGlobalPositionFromBlockGlobalPosition(Vector2I blockGlobalPosition)
    {
        return GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition.X, blockGlobalPosition.Y);
    }
    public ChunkDataGenerator.BlockTypes GetBlockTypeInGlobalPosition(Vector3I blockGlobalPosition)
    {
        return GetBlockTypeInGlobalPosition(blockGlobalPosition.X, blockGlobalPosition.Y, blockGlobalPosition.Z);
    }
    public ChunkDataGenerator.BlockTypes GetBlockTypeInGlobalPosition(long x, long y,long z)
    {
        Vector2I chunkPosition = GetChunkGlobalPositionFromBlockGlobalPosition(x,z);
        ChunkResource chunkResource;
        if (ChunksResources.TryGetValue(chunkPosition, out chunkResource))
        {
            return chunkResource.Data[x - chunkPosition.X * ChunkDataGenerator.CHUNK_SIZE, y, z - chunkPosition.Y * ChunkDataGenerator.CHUNK_SIZE];
        }
        if(_chunkUpdater.CurrentRenderDistanse < RenderDistance - 2)
        {
            throw new Exception("Not exist resourse near your chunk to draw!.");
        }
        return ChunkDataGenerator.BlockTypes.CobbleStone;
    }


    public static Vector2I GetChunkGlobalPositionFromBlockGlobalPosition(long x, long z)
    {
        Vector2I chunkPosition = new((int)x / ChunkDataGenerator.CHUNK_SIZE, (int)z / ChunkDataGenerator.CHUNK_SIZE);
        if (x < 0 && x % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.X;
        }
        if (z < 0 && z % ChunkDataGenerator.CHUNK_SIZE != 0)
        {
            --chunkPosition.Y;
        }
        return chunkPosition;
    }

}