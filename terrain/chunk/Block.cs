using Godot;

public class Block
{
    public byte LevelOfLight = 15;
    public enum Type : ushort
    {
        Air,
        CobbleStone,
        Dirt,
        Ground,
        Planks,
    }
    public Type type;
}
