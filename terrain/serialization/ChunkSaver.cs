using System;
using System.Reflection.Metadata;
using Godot;
using Godot.Collections;

namespace ChunksSerealisation
{
    public partial class ChunkSaver : GodotObject
    {
        public const string SAVE_PATH = "user://world/";
        public static Array<Error> SaveStaticTerrain(Dictionary<Vector2I, ChunkResource> chunks)
        {
            _CreateDirectoryWorldInUserOrNothing();
            Array<Error> errors = new();
            foreach (ChunkResource resource in chunks.Values)
            {
                
                errors.Add(resource.Save());
            }
            return errors;
        }
        public static void SaveChunk(ChunkResource resource)
        {
            _CreateDirectoryWorldInUserOrNothing();
            resource.Save();
        }


        private static void _CreateDirectoryWorldInUserOrNothing()
        {
            if(!DirAccess.DirExistsAbsolute(SAVE_PATH.Remove(SAVE_PATH.Length - 1)))
            {
                DirAccess.MakeDirAbsolute(SAVE_PATH.Remove(SAVE_PATH.Length - 1));
            }
        }
    }
}