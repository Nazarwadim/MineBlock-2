using Godot;

namespace ChunksSerealisation
{
    public static class ChunkLoader
    {
        public const string LOAD_PATH = "user://world/";

        public static bool IsChunkSavedAtPosition(Vector2I chunkPosition)
        {
            return FileAccess.FileExists(LOAD_PATH + GD.VarToStr(chunkPosition) + ".res");
        }
        public static ChunkResource GetChunkResourceOrNull(Vector2I chunkPosition)
        {
            if(IsChunkSavedAtPosition(chunkPosition))
            {
                return ChunkResource.Load(chunkPosition);
            }
            else
            {
                return null;
            }
        }
    }
}