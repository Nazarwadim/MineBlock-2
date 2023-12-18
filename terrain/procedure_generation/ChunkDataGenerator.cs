using System;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;


namespace ProcedureGeneration
{
    public static class ChunkDataGenerator
    {
        public const short CHUNK_SIZE = 16;
        public const short CHUNK_HEIGHT = 255;
        public static RandomNumberGenerator random = new RandomNumberGenerator(){Seed = Seed};
        public const float RANDOM_TO_GENERATE_BLOCK = 0.05f;
        private static readonly PerlineNoise _noise = new PerlineNoise((int)Seed);
        public static ulong Seed = 0;

        public static ChunkResource.Types[,,] GetRandomChunk()
        {
            ChunkResource.Types[,,] chunk = new ChunkResource.Types[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            
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
                    heights[i, j] = GetBlockHeightGeneratedFromGlobalPosition(i + chunkPosition.X * CHUNK_SIZE, j + chunkPosition.Y * CHUNK_SIZE, 35, 63);
                }
            }
            return heights;
        }
        public static byte GetBlockHeightGeneratedFromGlobalPosition(Vector3I blockPosition)
        {
            float noise_pos = _noise.GetNoise2D(blockPosition.X, blockPosition.Z);
            return (byte)Math.Round(Mathf.Remap(noise_pos, -1, 1, 0, 255));
        }

        public static byte GetBlockHeightGeneratedFromGlobalPosition(int x, int z, byte from, byte to)
        {
            float noise_pos = _noise.GetNoise2D(x, z);
            return (byte)Math.Round(Mathf.Remap(noise_pos, -1, 1, from, to));
        }
        public static byte[,,] GetChunkWithTerrain(Vector2I chunkPosition, byte[,] chunk_heights)
        {
            byte[,,] chunk = new byte[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            for (int x = 0; x < CHUNK_SIZE; ++x)
            {
                for (int z = 0; z < CHUNK_SIZE; ++z)
                {
                    int y = 0;
                    for (y = 0; y < chunk_heights[x, z]; ++y)
                    {
                        chunk[x, y, z] = (byte)ChunkResource.Types.CobbleStone;
                    }
                    
                    if(y < 90)
                    {
                        chunk[x, y, z] = (byte)ChunkResource.Types.Gravel;
                        for(int i = 1; i < GD.RandRange(2,5);++i)
                            chunk[x, y - i, z] = (byte)ChunkResource.Types.Gravel;
                    }
                    else if(y < Mathf.Remap(GD.Randf(),0,1,90, 120))
                    {
                        chunk[x, y, z] = (byte)ChunkResource.Types.Grass;
                    }
                    else if(y < Mathf.Remap(GD.Randf(),0,1,120, 140))
                    {
                        chunk[x, y, z] = (byte)ChunkResource.Types.Furnace;
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