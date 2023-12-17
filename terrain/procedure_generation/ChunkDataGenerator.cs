using System;
using Godot;
using Godot.Collections;


namespace ProcedureGeneration
{
    public static class ChunkDataGenerator
    {
        public const short CHUNK_SIZE = 16;
        public const short CHUNK_HEIGHT = 255;
        public const float RANDOM_TO_GENERATE_BLOCK = 0.05f;
        private static readonly PerlineNoise _noise = new PerlineNoise((int)Seed);
        public static ulong Seed = 0;

        public static ChunkResource.Types[,,] GetRandomChunk()
        {
            ChunkResource.Types[,,] chunk = new ChunkResource.Types[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            RandomNumberGenerator random = new RandomNumberGenerator();
            random.Seed = Seed;
            for (int x = 0; x < chunk.GetLength(0); x++)
            {
                for (int y = 0; y < chunk.GetLength(1); y++)
                {
                    for (int z = 0; z < chunk.GetLength(2); z++)
                    {
                        if (RANDOM_TO_GENERATE_BLOCK > random.Randf())
                        {
                            chunk[x, y, z] = (ChunkResource.Types)(random.Randi() % 29);
                        }
                    }
                }
            }
            return chunk;
        }
        public static byte[,] GetChunkHeightsFromNoise(Vector2I chunkPosition)
        {
            byte[,] heights = new byte[CHUNK_SIZE, CHUNK_SIZE];
            for (int i = 0; i < CHUNK_SIZE; ++i)
            {
                for (int j = 0; j < CHUNK_SIZE; ++j)
                {
                    float noise_pos = _noise.GetNoise2D(i + chunkPosition.X, j + chunkPosition.Y);
                    heights[i, j] = (byte)Math.Round(Mathf.Remap(noise_pos, -1, 1, 0, 255));
                }
            }
            return heights;
        }

        public static byte[,,] GetChunkWithTerrain(Vector2I chunkPosition, byte[,] chunk_heights)
        {
            byte[,,] chunk = new byte[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            for (int x = 0; x < chunk.GetLength(0); ++x)
            {
                for (int z = 0; z < chunk.GetLength(2); ++z)
                {
                    for (int y = 0; y < chunk_heights[x, z]; ++y)
                    {
                        chunk[x, y, z] = (byte)ChunkResource.Types.CobbleStone;
                    }
                }
            }
            return chunk;
        }
        public static byte[,,] GetChunkWithTerrain(Vector2I chunkPosition)
        {
            byte[,] chunk_heights = GetChunkHeightsFromNoise(chunkPosition);
            return GetChunkWithTerrain(chunkPosition, chunk_heights);
        }
    }

}