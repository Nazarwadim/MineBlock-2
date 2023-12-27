using System.Windows.Markup;
using Godot;
using ProcedureGeneration;


public partial class ChunkStaticBody : StaticBody3D
{
    public MeshInstance3D MeshInstance{get;set;}
    public CollisionShape3D ColisionShape{get;set;}
    private VisibleOnScreenNotifier3D _visibleOnScreenNotifier;

    private static readonly Material _MATERIAL_OVERRIDE  = GD.Load<Material>("res://textures/material.tres");
    public ChunkStaticBody(Mesh mesh, Shape3D shape, Vector3 position) : this(mesh,position)
    {
        ColisionShape = new()
        {
            Shape = shape
        };

    }

    public void SetNewMesh(Mesh mesh)
    {
        MeshInstance.QueueFree();
        MeshInstance = new()
        {
            Mesh = mesh,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.DoubleSided,
            MaterialOverride = _MATERIAL_OVERRIDE
        };
        AddChild(MeshInstance);
    }

    public ChunkStaticBody(MeshInstance3D meshInstance3D, CollisionShape3D collisionShape3Dshape, Vector3 position)
    {
        MeshInstance = meshInstance3D;
        ColisionShape = collisionShape3Dshape;
        Position = position;
    }

    public ChunkStaticBody(Vector3 position)
    {
        Position = position;
    }
    public ChunkStaticBody(Mesh mesh, Vector3 position)
    {
        MeshInstance = new()
        {
            Mesh = mesh
        };
        Position = position;
    }
    
    public override void _Ready()
    {
        
        CollisionLayer = 3;
        MeshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.DoubleSided;
        if(ColisionShape == null)
        {
            ColisionShape = new ();
        }
        if(MeshInstance == null)
        {
            MeshInstance = new();
        }
        MeshInstance.MaterialOverride = _MATERIAL_OVERRIDE;

        _visibleOnScreenNotifier = new VisibleOnScreenNotifier3D();
        Vector3 _size = new (ChunkDataGenerator.CHUNK_SIZE, ChunkDataGenerator.CHUNK_HEIGHT,ChunkDataGenerator.CHUNK_SIZE);
        _visibleOnScreenNotifier.Aabb = new Aabb(Vector3.Zero, _size);
        
        AddChild(_visibleOnScreenNotifier);
        AddChild(ColisionShape);
        AddChild(MeshInstance);

        _visibleOnScreenNotifier.ScreenEntered += () => MeshInstance.Visible = true;
        _visibleOnScreenNotifier.ScreenExited += () => MeshInstance.Visible = false;
    }
}