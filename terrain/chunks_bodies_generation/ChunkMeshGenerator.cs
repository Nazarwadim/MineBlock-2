using System;
using Godot;
using ProcedureGeneration;

using BlockSides = ChunkBodyGeneration.ChunksBodyGenerator.BlockSide;
namespace ChunkBodyGeneration
{
    public static class ChunksMeshGenerator
    {
        public static ulong coutnerTemp = 0;
        const int TEXTURE_SHEET_WIDTH = 8;
        const float TEXTURE_TILE_SIZE = 1.0f / TEXTURE_SHEET_WIDTH;
        public const short CHUNK_SIZE = ChunkDataGenerator.CHUNK_SIZE;
        public const short CHUNK_HEIGHT = ChunkDataGenerator.CHUNK_HEIGHT;
        //Summary:
        //  This method creates mesh by algoritm from 2d array of blocks and from neighbouring chanks

        public static Mesh GenerateChunkMesh(ChunkResource chunkResource, VoxelWorld world)
        {
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunkResource.Data;
            if (mainDataChunk.Length == 0)
            {
                throw new Exception("No elements in array ChunksMeshGenerator.cs 21");
            }

            bool existsSideChunks = true;
            ChunkResource leftChunk;
            ChunkResource rightChunk;
            ChunkResource upChunk;
            ChunkResource downChunk;
            if (!world.ChunksResources.TryGetValue(new Vector2I(chunkResource.Position.X - 1, chunkResource.Position.Y), out leftChunk))
            {
                existsSideChunks = false;
            }
            if (!world.ChunksResources.TryGetValue(new Vector2I(chunkResource.Position.X + 1, chunkResource.Position.Y), out rightChunk))
            {
                existsSideChunks = false;
            }
            if (!world.ChunksResources.TryGetValue(new Vector2I(chunkResource.Position.X, chunkResource.Position.Y + 1), out upChunk))
            {
                existsSideChunks = false;
            }
            if (!world.ChunksResources.TryGetValue(new Vector2I(chunkResource.Position.X, chunkResource.Position.Y - 1), out downChunk))
            {
                existsSideChunks = false;
            }
            if (!existsSideChunks)
            {
                throw new NullReferenceException("Try to generate chunkBody without chunkResources near it!");
            }


            SurfaceTool surfaceTool = new();
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            
            _GenerateInsideChunkSurfaceTool(surfaceTool, mainDataChunk);

            _GenerateUpDownYSurfaceTool(surfaceTool, chunkResource, leftChunk, rightChunk, downChunk, upChunk);

            _GenerateChunkSidesSurfaceTool(surfaceTool, chunkResource, leftChunk, rightChunk, downChunk, upChunk);
            
            surfaceTool.GenerateNormals();
            surfaceTool.GenerateTangents();
            surfaceTool.Index();
            ArrayMesh arrayMesh = surfaceTool.Commit();
            return arrayMesh;
        }
        //Summary:
        //  This is unsafe functions!


        private static void _GenerateInsideChunkSurfaceTool(SurfaceTool surfaceTool, ChunkDataGenerator.BlockTypes[,,] mainDataChunk)
        {
            bool[] neighbourBlocksAreTransparent = new bool[6];
            for (long i = 1; i < CHUNK_SIZE - 1; ++i)
            {
                for (long j = 1; j < CHUNK_HEIGHT - 1; ++j)
                {
                    for (long k = 1; k < CHUNK_SIZE - 1; ++k)
                    {

                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, j, k];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        
                        neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[i, j, k - 1]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[i, j, k + 1]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[i - 1, j, k]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[i + 1, j, k]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[i, j - 1, k]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[i, j + 1, k]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)i, (int)j, (int)k), blockId, neighbourBlocksAreTransparent);
                        }

                    }
                }
            }
        }

        private static void _GenerateUpDownYSurfaceTool(SurfaceTool surfaceTool, ChunkResource chunk,
            ChunkResource chunkLeft, ChunkResource chunkRight, ChunkResource chunkDown, ChunkResource chunkUp)
        {
            Vector3I chunkPosition = new(chunk.Position.X, 0, chunk.Position.Y);
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunk.Data;

            ChunkDataGenerator.BlockTypes[,,] leftDataChunk = chunkLeft.Data;
            ChunkDataGenerator.BlockTypes[,,] rightDataChunk = chunkRight.Data;
            ChunkDataGenerator.BlockTypes[,,] downDataChunk = chunkDown.Data;
            ChunkDataGenerator.BlockTypes[,,] upDataChunk = chunkUp.Data;
            bool[] neighbourBlocksAreTransparent = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            for (long y = 0; y < CHUNK_HEIGHT; y += CHUNK_HEIGHT - 1)
            {
                long blockYSideCheck = y == 0 ? 1 : -1;
                neighbourBlocksAreTransparent[4] = y == 0;
                neighbourBlocksAreTransparent[5] = y != 0;
                for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        neighbourBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                        }

                    }
                }


                for (long x = 0; x < CHUNK_SIZE; x += CHUNK_SIZE - 1)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);

                        if (x == 0)
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                        }


                        neighbourBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                        }
                    }
                }
                for (long z = 0; z < CHUNK_SIZE; z += CHUNK_SIZE - 1)
                {
                    for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        if (z == 0)
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                        }
                        neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        neighbourBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                        }
                    }
                }
                for (long x = 0; x < CHUNK_SIZE; x += CHUNK_SIZE - 1)
                {
                    for (long z = 0; z < CHUNK_SIZE; z += CHUNK_SIZE - 1)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == 0)
                        {
                            continue;
                        }
                        if (x == 0)
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                        }
                        if (z == 0)
                        {

                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, 1]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                        }

                        neighbourBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                        }
                    }
                }
            }
        }

        private static void _GenerateChunkSidesSurfaceTool(SurfaceTool surfaceTool, ChunkResource chunk,
            ChunkResource chunkLeft, ChunkResource chunkRight, ChunkResource chunkDown, ChunkResource chunkUp)
        {
            bool[] neighbourBlocksAreTransparent = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunk.Data;

            ChunkDataGenerator.BlockTypes[,,] leftDataChunk = chunkLeft.Data;
            ChunkDataGenerator.BlockTypes[,,] rightDataChunk = chunkRight.Data;
            ChunkDataGenerator.BlockTypes[,,] downDataChunk = chunkDown.Data;
            ChunkDataGenerator.BlockTypes[,,] upDataChunk = chunkUp.Data;

            Vector3I chunkPositionGlobal = new(chunk.Position.X * CHUNK_SIZE, 0, chunk.Position.Y * CHUNK_SIZE);
            for (long i = 0; i < CHUNK_SIZE; i += CHUNK_SIZE - 1)
            {
                for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                {
                    for (long y = 1; y < CHUNK_HEIGHT - 1; ++y)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, i];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        if (i == 0)
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, 1]);

                        }
                        else
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, i - 1]);
                        }


                        neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, i]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, i]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[x, y - 1, i]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + 1, i]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)i), blockId, neighbourBlocksAreTransparent);
                        }


                    }
                }

                for (long y = 1; y < CHUNK_HEIGHT - 1; ++y)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {

                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        if (i == 0)
                        {

                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[i - 1, y, z]);
                        }


                        neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[i, y, z - 1]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[i, y, z + 1]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[i, y - 1, z]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[i, y + 1, z]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)i, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                        }

                    }
                }
            }

            for (long x = 0; x < CHUNK_SIZE; x += CHUNK_SIZE - 1)
            {
                for (long y = 1; y < CHUNK_HEIGHT - 1; ++y)
                {
                    for (long z = 0; z < CHUNK_SIZE; z += CHUNK_SIZE - 1)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        if (x == 0)
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[1, y, z]);
                        }
                        else
                        {

                            neighbourBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                        }
                        if (z == 0)
                        {

                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, 1]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighbourBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                        }
                        neighbourBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[x, y - 1, z]);
                        neighbourBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + 1, z]);

                        if (ChunksBodyGenerator.IsBlockSidesToCreate(neighbourBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                        }
                    }
                }
            }

        }
        private static void _DrawBlockMesh(SurfaceTool surfaceTool, Vector3I blockSubPosition, ChunkDataGenerator.BlockTypes blockId, bool[] sidesToDraw)
        {

            Vector3[] verts = ChunksBodyGenerator.CalculateBlockVerts(blockSubPosition);
            Vector2[] uvs = CalculateBlockUvs((long)blockId);

            Vector2[] top_uvs = uvs;
            Vector2[] bottom_uvs = uvs;




            if (blockId == ChunkDataGenerator.BlockTypes.WitheredGrass || blockId == ChunkDataGenerator.BlockTypes.Grass)
            {
                Vector3[] vects1 = { verts[2], verts[0], verts[7], verts[5] };
                Vector3[] vects2 = { verts[7], verts[5], verts[2], verts[0] };
                Vector3[] vects3 = { verts[3], verts[1], verts[6], verts[4] };
                Vector3[] vects4 = { verts[6], verts[4], verts[3], verts[1] };
                DrawBlockFace(surfaceTool, vects1, uvs);
                DrawBlockFace(surfaceTool, vects2, uvs);
                DrawBlockFace(surfaceTool, vects3, uvs);
                DrawBlockFace(surfaceTool, vects4, uvs);
                return;
            }

            if (blockId == ChunkDataGenerator.BlockTypes.Grass)
            {
                top_uvs = (Vector2[])CalculateBlockUvs(0).Clone();
                bottom_uvs = (Vector2[])CalculateBlockUvs(2).Clone();
            }
            else if (blockId == ChunkDataGenerator.BlockTypes.Furnace)
            {
                top_uvs = (Vector2[])CalculateBlockUvs(31).Clone();
                bottom_uvs = top_uvs;
            }
            switch (blockId)
            {
                case ChunkDataGenerator.BlockTypes.GrassBlock:
                    top_uvs = (Vector2[])CalculateBlockUvs(0).Clone();
                    bottom_uvs = (Vector2[])CalculateBlockUvs(2).Clone();
                    break;
                case ChunkDataGenerator.BlockTypes.Furnace:
                    top_uvs = (Vector2[])CalculateBlockUvs(31).Clone();
                    bottom_uvs = top_uvs;
                    break;
                case ChunkDataGenerator.BlockTypes.Log:
                    top_uvs = CalculateBlockUvs(31);
                    bottom_uvs = top_uvs;
                    break;
                case ChunkDataGenerator.BlockTypes.BookShelf:
                    top_uvs = CalculateBlockUvs(4);
                    bottom_uvs = top_uvs;
                    break;
            }




            if (sidesToDraw[(int)BlockSides.Left])
            {
                Vector3[] verts1_ = { verts[2], verts[0], verts[3], verts[1] };
                DrawBlockFace(surfaceTool, verts1_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Right])
            {
                Vector3[] verts2_ = { verts[7], verts[5], verts[6], verts[4] };
                DrawBlockFace(surfaceTool, verts2_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Front])
            {
                Vector3[] verts3_ = { verts[6], verts[4], verts[2], verts[0] };
                DrawBlockFace(surfaceTool, verts3_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Back])
            {
                Vector3[] verts4_ = { verts[3], verts[1], verts[7], verts[5] };
                DrawBlockFace(surfaceTool, verts4_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Up])
            {
                Vector3[] verts5_ = { verts[2], verts[3], verts[6], verts[7] };
                DrawBlockFace(surfaceTool, verts5_, top_uvs);
            }
            if (sidesToDraw[(int)BlockSides.Down])
            {
                Vector3[] verts6_ = { verts[4], verts[5], verts[0], verts[1] };
                DrawBlockFace(surfaceTool, verts6_, bottom_uvs);
            }

        }

        public static void DrawBlockFace(SurfaceTool surfaceTool, Vector3[] verts, Vector2[] uvs)
        {
            surfaceTool.SetUV(uvs[1]); surfaceTool.AddVertex(verts[1]);
            surfaceTool.SetUV(uvs[2]); surfaceTool.AddVertex(verts[2]);
            surfaceTool.SetUV(uvs[3]); surfaceTool.AddVertex(verts[3]);

            surfaceTool.SetUV(uvs[2]); surfaceTool.AddVertex(verts[2]);
            surfaceTool.SetUV(uvs[1]); surfaceTool.AddVertex(verts[1]);
            surfaceTool.SetUV(uvs[0]); surfaceTool.AddVertex(verts[0]);
        }


        public static Vector2[] CalculateBlockUvs(long blockId)
        {
            long row = blockId / TEXTURE_SHEET_WIDTH;
            long col = blockId % TEXTURE_SHEET_WIDTH;
            Vector2[] array = {
                // we adding offset becouse godot lags
                new Vector2(col + 0.02f, row + 0.02f) * TEXTURE_TILE_SIZE,
                new Vector2(col + 0.02f, row + 0.98f) * TEXTURE_TILE_SIZE,
                new Vector2(col + 0.98f, row + 0.02f) * TEXTURE_TILE_SIZE,
                new Vector2(col + 0.98f, row + 0.98f) * TEXTURE_TILE_SIZE};

            return array;
        }
        public static bool IsBlockTransparent(ChunkDataGenerator.BlockTypes blockId)
        {
            return blockId == ChunkDataGenerator.BlockTypes.Air || ((int)blockId > 25 && (int)blockId < 30);
        }
    }
}