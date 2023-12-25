using Godot;


public partial class ChunkStaticBody : StaticBody3D
{
    [Export]public MeshInstance3D MeshInstance{get;set;}
    [Export]public CollisionShape3D ColisionShape{get;set;}

    private static readonly Material _MATERIAL_OVERRIDE  = GD.Load<Material>("res://textures/material.tres");
    private Shape3D _shape;
    private Mesh _mesh;
    public ChunkStaticBody(Mesh mesh, Shape3D shape, Vector3 position) : this(mesh,position)
    {
        _shape = shape;
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
        _mesh = mesh;
        Position = position;
    }
    
    public override void _Ready()
    {
        CollisionLayer = 3;
        if(ColisionShape == null)
        {
            ColisionShape = new ();
        }
        if(MeshInstance == null)
        {
            MeshInstance = new();
        }
        MeshInstance.MaterialOverride = _MATERIAL_OVERRIDE;
        if(_shape != null)
        {
            ColisionShape.Shape = _shape;
        }
        if(_mesh != null)
        {
            MeshInstance.Mesh = _mesh;
        }
        AddChild(ColisionShape);
        AddChild(MeshInstance);
    }
}