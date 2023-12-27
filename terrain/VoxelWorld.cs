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
    [Export] private Node3D GenerationRelativePoint;
    [Export] public bool IsSerialization;
    
    public readonly Dictionary<Vector2I, ChunkResource> ChunksResources;
    public readonly Dictionary<Vector2I, ChunkStaticBody> ChunksBodies;
    
    private ChunkUpdater _chunkUpdater;
    private Vector2I _middleChunkPos;
    public override void _Ready()
    {
        if(GenerationRelativePoint == null)
        {
            GD.PrintErr("Set node3d from set a point from which the generation will start");
            GetTree().Quit();
        }
        if(_worldEnvironment == null)
        {
            GD.PrintErr("Set world environment!!!! VoxelWorld.cs line 33");
            GetTree().Quit();
        }
        
        ChunkDataGenerator.Seed = Seed;
        _chunkUpdater = new ChunkUpdater(ChunksResources, ChunksBodies, this);
        _chunkUpdater.CurrentRenderDistanseChanged += _OnCurrentRenderDistanceChanged;
        Signal signal = (Signal)GenerationRelativePoint.Call("get_position_XY_changed");
        //GenerationRelativePoint.Call("connect")
        GD.Print( GenerationRelativePoint.Connect(signal.Name, new Callable(this, MethodName._OnPlayerPositioXYChanged)));
        AddChild(_chunkUpdater);
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
    public ChunkDataGenerator.BlockTypes GetBlockTypeInGlobalPosition(Vector3I blockGlobalPosition)
    {
        Vector2I chunkPosition = GetChunkGlobalPositionFromBlockGlobalPosition(blockGlobalPosition);
        ChunkResource chunkResource;
        if (ChunksResources.TryGetValue(chunkPosition, out chunkResource))
        {
            Vector3I subPosition = blockGlobalPosition - new Vector3I(chunkPosition.X, 0, chunkPosition.Y) * ChunkDataGenerator.CHUNK_SIZE;
            return chunkResource.Data[subPosition.X, subPosition.Y, subPosition.Z];
        }
        if(_chunkUpdater.CurrentRenderDistanse < RenderDistance - 2)
        {
            throw new Exception("Not exist resourse near your chunk to draw!.");
        }
        return ChunkDataGenerator.BlockTypes.CobbleStone;
    }
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



    //Fast code bad readable!
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