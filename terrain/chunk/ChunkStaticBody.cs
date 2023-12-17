using Godot;

[GlobalClass]
public partial class ChunkStaticBody : StaticBody3D
{
    [Export]public MeshInstance3D MeshInstance;
    [Export]public CollisionShape3D ColisionShape;

    public ChunkStaticBody()
    {
        MeshInstance = new MeshInstance3D();
        ColisionShape = new CollisionShape3D();        
    }
    
    public override void _EnterTree()
    {
        AddChild(ColisionShape);
        AddChild(MeshInstance);
        ulong start = Time.GetTicksMsec();
        GD.Print(Time.GetTicksMsec() - start);
    }
}