using Godot;

namespace ChunksSerealisation
{
    public static class ChunkLoader
    {
        public const string LOAD_PATH = "user://world/";

        public static ChunkResource GetChunkResourceOrNull(Vector2I chunkPosition)
        {
            if(FileAccess.FileExists(LOAD_PATH + GD.VarToStr(chunkPosition) + ".res"))
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