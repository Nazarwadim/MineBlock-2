using System;
using Godot;

namespace Terrain.ChunksSerialization
{
    public partial class ChunkLoader : GodotObject
    {
        public static ChunkResource GetChunkResourceOrNull(Vector2I chunkPosition, string chunksPath)
        {
            string _chunkResourcePath = chunksPath + GD.VarToStr(chunkPosition) + ".res";
            if (FileAccess.FileExists(_chunkResourcePath))
            {
                try
                {
                    ChunkResource resource = new();
                    resource.Load(_chunkResourcePath);
                    return resource;
                }
                catch (Exception)
                {

                    DirAccess.RemoveAbsolute(_chunkResourcePath);
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
    }
}