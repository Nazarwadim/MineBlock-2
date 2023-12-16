using Godot;



namespace ChunkBodyGeneration{
public static class ChunksBodyGenerator
{
    const int TEXTURE_SHEET_WIDTH = 8;

    const int CHUNK_LAST_INDEX = ChunkDataGenerator.CHUNK_SIZE - 1;
    const float TEXTURE_TILE_SIZE = 1.0f / TEXTURE_SHEET_WIDTH;


    public static Vector2[] CalculateBlockUvs(Block.Type blockId)
    {
        int row = ((int)blockId) / TEXTURE_SHEET_WIDTH;
        int col = ((int)blockId) % TEXTURE_SHEET_WIDTH;
        Vector2[] array = {
        new Vector2(col + 0.01f, row + 0.01f) * TEXTURE_TILE_SIZE, 
        new Vector2(col + 0.01f, row + 0.99f) * TEXTURE_TILE_SIZE, 
        new Vector2(col + 0.99f, row + 0.01f) * TEXTURE_TILE_SIZE, 
        new Vector2(col + 0.99f, row + 0.99f) * TEXTURE_TILE_SIZE};

        return array;
    }
    
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

    public static bool IsBlockTransparent(byte blockId)
    {
        return blockId == 0 || (blockId > 25 && blockId < 30);
    }
}
}