using System;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;


namespace ProcedureGeneration
{
    public static class ChunkDataGenerator
    {
            public enum Biomes
            {
                Mountains,
                Ocean,
                Plains,
            }
        public enum BlockTypes : byte
        {
            Air,
            CobbleStone,
            Dirt,
            GrassBlock,
            Planks,
            Furnace,
            Godot,
            Bricks,
            Cobblestone,
            Deepslate,
            Sand,
            Gravel,
            Log,
            DiamondBlock,
            IronBlock,
            GoldBlock,
            GoldOre,
            CopperOre,
            Coal,
            BookShelf,
            MossyCobblestone,
            Obsidian,
            DiamondOre,
            IronOre,
            Sponge,
            OakLeaves,
            BirchLeaves,
            Grass,
            WitheredGrass,
            Glass
        }
        public const short CHUNK_SIZE = 16;
        public const short CHUNK_HEIGHT = 255;
        public static RandomNumberGenerator random = new RandomNumberGenerator() { Seed = Seed };
        public const float RANDOM_TO_GENERATE_BLOCK = 0.05f;
        private static readonly PerlineNoise _noise = new PerlineNoise((int)Seed);
        public static ulong Seed = 0;

        public static BlockTypes[,,] GetRandomChunk()
        {
            BlockTypes[,,] chunk = new BlockTypes[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];

            for (ulong x = 0; x < (long)CHUNK_SIZE; x++)
            {
                for (ulong y = 0; y < (long)CHUNK_HEIGHT; y++)
                {
                    for (ulong z = 0; z < (long)CHUNK_SIZE; z++)
                    {
                        if (RANDOM_TO_GENERATE_BLOCK > random.Randf())
                        {
                            chunk[x, y, z] = (BlockTypes)(random.Randi() % 29);
                        }
                    }
                }
            }
            return chunk;
        }
        public static byte[,] GetChunkHeightsFromNoise(Vector2I chunkPosition)
        {
            byte[,] heights = new byte[CHUNK_SIZE, CHUNK_SIZE];
            for (ulong i = 0; i < (long)CHUNK_SIZE; ++i)
            {
                for (ulong j = 0; j < (long)CHUNK_SIZE; ++j)
                {
                    heights[i, j] = GetBlockHeightGeneratedFromGlobalPosition((long)i + chunkPosition.X * CHUNK_SIZE, (long)j + chunkPosition.Y * CHUNK_SIZE, 35, 63);
                }
            }
            return heights;
        }
        public static byte GetBlockHeightGeneratedFromGlobalPosition(Vector3I blockPosition)
        {
            float noise_pos = _noise.GetNoise2D(blockPosition.X, blockPosition.Z);
            return (byte)Math.Round(Mathf.Remap(noise_pos, -1, 1, 0, 255));
        }

        public static byte GetBlockHeightGeneratedFromGlobalPosition(long x, long z, byte from, byte to)
        {
            float noise_pos = _noise.GetNoise2D(x, z);
            return (byte)Math.Round(Mathf.Remap(noise_pos, -1, 1, from, to));
        }
        public static byte[,,] GetChunkWithTerrain(Vector2I chunkPosition, byte[,] chunk_heights)
        {
            byte[,,] chunk = new byte[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            for (ulong x = 0; x < (long)CHUNK_SIZE; ++x)
            {
                for (ulong z = 0; z < (long)CHUNK_SIZE; ++z)
                {
                    ulong y = 0;
                    for (y = 0; y < chunk_heights[x, z]; ++y)
                    {
                        chunk[x, y, z] = (byte)BlockTypes.CobbleStone;
                    }

                    if (y < 90 && y > 5)
                    {
                        chunk[x, y, z] = (byte)BlockTypes.Gravel;
                        for (ulong i = 1; i < (ulong)GD.RandRange(2, 5); ++i)
                            chunk[x, y - i, z] = (byte)BlockTypes.Gravel;
                    }
                    else if (y < Mathf.Remap(GD.Randf(), 0, 1, 90, 120))
                    {
                        chunk[x, y, z] = (byte)BlockTypes.Grass;
                    }
                    else if (y < Mathf.Remap(GD.Randf(), 0, 1, 120, 140))
                    {
                        chunk[x, y, z] = (byte)BlockTypes.Furnace;
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
        public static byte[,,] GenerateChunkEquidistantBlocks(Vector2I chunkPosition, BlockTypes type, ulong distance)
        {
            byte[,,] chunk = new byte[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];
            for (ulong x = 0; x < (ulong)CHUNK_SIZE; x += distance)
            {
                for (ulong y = 0; y < (ulong)CHUNK_HEIGHT; y += distance)
                {
                    for (ulong z = 0; z < (ulong)CHUNK_SIZE; z += distance)
                    {
                        chunk[x, y, z] = (byte)type;
                    }
                }
            }
            return chunk;
        }
    }

}