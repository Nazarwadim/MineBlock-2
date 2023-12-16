using Godot;

public partial class ChunkStaticBody : StaticBody3D
{
    public MeshInstance3D MeshInstance;
    public CollisionShape3D ColisionShape;

    public ChunkStaticBody()
    {
        MeshInstance = new MeshInstance3D();
        ColisionShape = new CollisionShape3D();        
    }
    
    public override void _EnterTree()
    {
        AddChild(ColisionShape);
        AddChild(MeshInstance);
    }
}