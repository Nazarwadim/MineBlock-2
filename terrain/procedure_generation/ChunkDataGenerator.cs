using System;
using Godot;
using Godot.Collections;


public static class ChunkDataGenerator
{
    public const short CHUNK_SIZE = 16;
    public const short CHUNK_HEIGHT = 255;
    public const float RANDOM_TO_GENERATE_BLOCK = 0.05f;
    public static Block[, ,] GetRandomChunk(ulong seed)
    {
        Block[, ,] chunk = new Block[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
        RandomNumberGenerator random = new RandomNumberGenerator();
        random.Seed = seed;
        for(int x = 0; x < chunk.GetLength(0);x++)
        {
            for(int y = 0; y < chunk.GetLength(1);y++)
            {
                for(int z = 0; z < chunk.GetLength(2);z++)
                {
                    if(RANDOM_TO_GENERATE_BLOCK > random.Randf())
                    {
                        chunk[x,y,z].type = (Block.Type)(random.Randi() % 29);
                    }
                }
            }
        }
        return chunk;
    }
    public static ushort[,] GetChunkHeightsFromNoise(PerlineNoise noise, Vector2I chunkPosition)
    {
        ushort[,] heights = new ushort[CHUNK_SIZE, CHUNK_SIZE];
        for(int i =0 ; i < CHUNK_SIZE; ++i)
        {
            for(int j =0 ; j < CHUNK_SIZE; ++j)
            {
                float noise_pos = noise.GetNoise2D(i + chunkPosition.X,j + chunkPosition.Y);
                heights[i,j] = (ushort)Math.Round( Mathf.Remap(noise_pos, -1, 1 ,0, 255) );
            }
        }
        return heights;
    }

    public static ushort[, ,] GetChunkWithTerrain(PerlineNoise noise, Vector2I chunkPosition)
    {
        ushort[, ,] chunk = new ushort[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
        ushort[,] chunk_heights = GetChunkHeightsFromNoise(noise, chunkPosition);
        for(int x = 0; x < chunk.GetLength(0); ++x)
        {
            for(int z = 0; z < chunk.GetLength(2); ++z)
            {
                for(int y = 0; y < chunk_heights[x,z]; ++y)
                {
                    chunk[x,y,z] = (ushort)Block.Type.CobbleStone;
                }
            }
        }
        return chunk;
    }
}