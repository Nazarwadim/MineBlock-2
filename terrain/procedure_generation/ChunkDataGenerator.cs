using System;
using System.Runtime.CompilerServices;
using System.Xml;
using Godot;
using Godot.Collections;


namespace ProcedureGeneration
{
    public partial class ChunkDataGenerator : GodotObject
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
            Stone,
            Dirt,
            GrassBlock,
            Planks,
            Furnace,
            Godot,
            Bricks,
            CobbleStone,
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
        public const short CHUNK_FAR_AWAY_SIZE = 8;
        public const short CHUNK_FAR_AWAY_HEIGHT = 127;
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static RandomNumberGenerator random = new() { Seed = Seed };
#pragma warning restore CA2211 // Non-constant fields should not be visible
        public const float RANDOM_TO_GENERATE_BLOCK = 0.05f;
        private static readonly PerlineNoise _noise = new((int)Seed);
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
        public static byte[,] GetChunkHeightsFromNoise(Vector2I chunkPosition, Biomes biome)
        {
            _noise.SetSettingsByBiome(biome);
            byte[,] heights = new byte[CHUNK_SIZE, CHUNK_SIZE];
            for (ulong i = 0; i < (long)CHUNK_SIZE; ++i)
            {
                for (ulong j = 0; j < (long)CHUNK_SIZE; ++j)
                {
                    heights[i, j] = GetBlockHeightGeneratedFromGlobalPosition((long)i + chunkPosition.X * CHUNK_SIZE, (long)j + chunkPosition.Y * CHUNK_SIZE, 35, 255);
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
            for(int x = 0 ; x < CHUNK_SIZE; ++x)
            {
                for(int z = 0; z < CHUNK_SIZE; ++z)
                {
                    chunk[x,0,z] = (byte)BlockTypes.Obsidian;       
                }
            }
            for (ulong x = 0; x < (long)CHUNK_SIZE; ++x)
            {
                for (ulong z = 0; z < (long)CHUNK_SIZE; ++z)
                {
                    ulong y = 0;
                    for (y = 1; y < chunk_heights[x, z]; ++y)
                    {
                        chunk[x, y, z] = (byte)BlockTypes.Stone;
                    }

                    if (y < 120 && y > 5)
                    {
                        chunk[x, y, z] = (byte)BlockTypes.GrassBlock;
                        for (ulong i = 1; i < (ulong)random.RandfRange(2, 5); ++i)
                            chunk[x, y - i, z] = (byte)BlockTypes.Dirt;
                    }
                    else if (y < Mathf.Remap(random.Randf(), 0, 1, 120, 130))
                    {
                        chunk[x, y, z] = (byte)BlockTypes.GrassBlock;
                    }
                    else if (y < Mathf.Remap(random.Randf(), 0, 1, 130, 150))
                    {
                        chunk[x, y, z] = (byte)BlockTypes.Furnace;
                    }
                    else if (z >=4 && z < CHUNK_SIZE -4 && x >= 4 && x < CHUNK_SIZE - 4 &&  
                        y < Mathf.Remap(random.Randf(), 0, 1, 130, 255) && random.Randf() < 0.08f)
                    {
                        int radius = (int)random.Randi() % 2 + 1;
                        BlockTypes blockType;
                        float rand = random.Randf();
                        if(rand < 0.6f)
                        {
                            blockType = BlockTypes.Coal;
                        }
                        else if(rand < 0.9f)
                        {
                            blockType = BlockTypes.CopperOre;
                        }
                        else
                        {
                            blockType = BlockTypes.IronOre;
                        }
                        for(long iy = -radius; iy < radius; ++iy)
                        {
                            long dx = (int)Mathf.Sqrt(radius * radius - iy * iy);
                            for(long ix = - dx; ix < dx; ++ix)
                            {
                                if((long)x + ix >= CHUNK_SIZE || (long)z + iy >=CHUNK_SIZE) GD.Print(x, y);
                                chunk[ (long)x + ix, chunk_heights[(long)x + ix,(long)z + iy],(long)z + iy] = (byte)blockType;
                                
                            }
                        }
                    }
                }
            }
            for(long i = 3; i < CHUNK_SIZE - 3; ++i)
            {
                for(long j = 3; j < CHUNK_SIZE -3; ++j)
                {
                    if(chunk_heights[i,j] < 110)
                    {
                        if(random.Randf() > 0.2f)
                        {
                            continue;
                        }

                        uint radius = random.Randi() % 3 + 1;
                        
                        for(int iy = -(int)radius; iy < radius; ++iy)
                        {
                            int dx = (int)Mathf.Sqrt(radius * radius - iy * iy);
                            for(int ix = - dx; ix < dx; ++ix)
                            {
                                chunk[ i+ ix, chunk_heights[i + ix,j + iy] + 1,j + iy] = (byte)BlockTypes.Grass;
                            }
                        }  
                        }
                    
                }
            }
            return chunk;
        }

        public static byte[,,] GetChunkWithTerrain(Vector2I chunkPosition)
        {
            byte[,] chunk_heights = GetChunkHeightsFromNoise(chunkPosition, Biomes.Mountains);
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