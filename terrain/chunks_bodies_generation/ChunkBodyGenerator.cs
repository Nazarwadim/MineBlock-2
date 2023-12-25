using Godot;
using ProcedureGeneration;


namespace ChunkBodyGeneration{
public static class ChunksBodyGenerator
{
	//Dont Add or remove elements, this will couse serious error!
	public enum BlockSide
	{
		Front,
		Back,
		Left,
		Right,
		Down,
		Up
	}
    const int CHUNK_LAST_INDEX = ChunkDataGenerator.CHUNK_SIZE - 1;
    
    public static Vector3[] CalculateBlockVerts(Vector3 blockPosition)
    {
        Vector3[] array = {
		    new Vector3(blockPosition.X, blockPosition.Y, blockPosition.Z),
		    new Vector3(blockPosition.X, blockPosition.Y, blockPosition.Z + 1),
		    new Vector3(blockPosition.X, blockPosition.Y + 1, blockPosition.Z),
		    new Vector3(blockPosition.X, blockPosition.Y + 1, blockPosition.Z + 1),
		    new Vector3(blockPosition.X + 1, blockPosition.Y, blockPosition.Z),
		    new Vector3(blockPosition.X + 1, blockPosition.Y, blockPosition.Z + 1),
		    new Vector3(blockPosition.X + 1, blockPosition.Y + 1, blockPosition.Z),
		    new Vector3(blockPosition.X + 1, blockPosition.Y + 1, blockPosition.Z + 1)
        };
        return array;
    }

    
}
}