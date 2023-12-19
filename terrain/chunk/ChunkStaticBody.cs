using Godot;


public partial class ChunkStaticBody : Node3D
{
    [Export]public MeshInstance3D MeshInstance;
    //[Export]public CollisionShape3D ColisionShape;

    public ChunkStaticBody(Mesh mesh, Vector3 position)
    {
        MeshInstance = new MeshInstance3D
        {
            Mesh = mesh
        };
        Position = position;
        //ColisionShape = new CollisionShape3D();        
    }
    
    public override void _EnterTree()
    {
        //AddChild(ColisionShape);
        AddChild(MeshInstance);
    }
}