using Godot;
public partial class ChunkStaticBody : StaticBody3D
{
    public MeshInstance3D MeshInstance{get;set;}
    public CollisionShape3D ColisionShape{get;set;}

    private static readonly Material _MATERIAL_OVERRIDE  = GD.Load<Material>("res://textures/material.tres");
    public ChunkStaticBody(Mesh mesh, Shape3D shape, Vector3 position) : this(mesh,position)
    {
        ColisionShape = new()
        {
            Shape = shape
        };
    }

    public ChunkStaticBody(MeshInstance3D meshInstance3D, CollisionShape3D collisionShape3Dshape, Vector3 position) : this()
    {
        MeshInstance = meshInstance3D;
        ColisionShape = collisionShape3Dshape;
        Position = position;
    }

    public ChunkStaticBody(Vector3 position) : this()
    {
        Position = position;
    }
    public ChunkStaticBody(Mesh mesh, Vector3 position) : this(position)
    {
        MeshInstance = new()
        {
            Mesh = mesh
        };
        
    }
    public ChunkStaticBody()
    {
        CollisionLayer = 3;
        InputRayPickable = false;
    }
    public void SetNewMesh(Mesh mesh)
    {
        MeshInstance.QueueFree();
        GD.Print("aboba");
        MeshInstance = new()
        {
            Mesh = mesh,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.DoubleSided,
            MaterialOverride = _MATERIAL_OVERRIDE
        };
        AddChild(MeshInstance);
    }

    public override void _Ready()
    {
        
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
        
        AddChild(ColisionShape);
        AddChild(MeshInstance);

        
    }
}