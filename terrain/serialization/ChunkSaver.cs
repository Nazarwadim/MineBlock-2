using System;
using System.Reflection.Metadata;
using Godot;
using Godot.Collections;

namespace ChunksSerealisation
{
    public static class ChunkSaver
    {
        public const string SAVE_PATH = "user://world/";
        public static Array<Error> SaveStaticTerrain(Dictionary<Vector2I, ChunkResource> chunks)
        {

            Array<Error> errors = new();
            foreach (ChunkResource resource in chunks.Values)
            {
                _CreateDirectoryWorldInUserOrNothing();
                errors.Add(resource.Save());
            }
            return errors;
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