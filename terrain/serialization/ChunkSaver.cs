using System;
using System.Reflection.Metadata;
using Godot;
using Godot.Collections;

namespace ChunksSerialization
{
    public partial class ChunkSaver : GodotObject
    {
        public static void SaveChunk(ChunkResource resource, string chunksPath)
        {
            string _chunkResourcePath = chunksPath + GD.VarToStr(resource.Position) + ".res";
            resource.Save(_chunkResourcePath);
        }
        
    }
}