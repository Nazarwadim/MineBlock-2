using Godot;
using System;
using System.Threading;

public partial class ChunkUpdater:Node
{

    public enum Sides
    {
        FrontRight,
        FrontLeft,
        BacktRight,
        BacktLeft
    }

    public ChunkUpdater()
    {
        _threads = new Thread[4];
    }
    [Export] public int RenderDistance{set;get;} = 2;
    [Export] private VoxelWorld _voxelWorld;
    private Vector2I _middleChunkPos;
    private readonly Thread[] _threads;
    private bool _exitLoops;

    public override void _Ready()
    {   
    }

    
    private void _OnPlayerPositionChanged(Vector2I position)
    {
        _middleChunkPos =  position;
    }
    
    private void _ThreadsUpdaterLoop(Sides side)
    {
        
    }
}