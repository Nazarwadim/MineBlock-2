using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ProcedureGeneration;

using BlockSides = ChunkBodyGeneration.ChunksBodyGenerator.BlockSide;
namespace ChunkBodyGeneration
{
    public static class ChunksMeshGenerator
    {
        public static ulong coutnerTemp = 0;
        const int TEXTURE_SHEET_WIDTH = 8;
        const float TEXTURE_TILE_SIZE = 1.0f / TEXTURE_SHEET_WIDTH;

        //Summary:
        //  This method creates mesh by algoritm from 2d array of blocks and from neighbouring chanks
        public static Mesh GenerateChunkMesh(ChunkResource chunkResource, VoxelWorld world)
        {
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunkResource.Data;
            if (mainDataChunk.Length == 0)
            {
                throw new Exception("No elements in array ChunksMeshGenerator.cs 21");
            }

            SurfaceTool surfaceTool = new();
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            _GenerateInsideChunkSurfaceTool(surfaceTool, mainDataChunk);

            _GenerateUpDownYSurfaceTool(surfaceTool, mainDataChunk, world, chunkResource);
            _GenerateChunkSidesSurfaceTool(surfaceTool, chunkResource, world);

            surfaceTool.GenerateNormals();
            surfaceTool.GenerateTangents();
            surfaceTool.Index();

            ArrayMesh arrayMesh = surfaceTool.Commit();

            return arrayMesh;
        }
        //Summary:
        //  This is unsafe function!


        private static void _GenerateInsideChunkSurfaceTool(SurfaceTool surfaceTool, ChunkDataGenerator.BlockTypes[,,] mainDataChunk)
        {
            bool[] neighbourBlocksAreTransparent = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            for (long i = 1; i < ChunkDataGenerator.CHUNK_SIZE - 1; ++i)
            {
                for (long j = 1; j < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++j)
                {
                    for (long k = 1; k < ChunkDataGenerator.CHUNK_SIZE - 1; ++k)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, j, k];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        long counter = 0;
                        neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[i, j, k - 1]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[i, j, k + 1]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[i - 1, j, k]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[i + 1, j, k]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[i, j - 1, k]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[i, j + 1, k]);

                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }

                        _DrawBlockMesh(surfaceTool, new Vector3I((int)i, (int)j, (int)k), blockId, neighbourBlocksAreTransparent);

                    }
                }
            }
        }

        private static void _GenerateUpDownYSurfaceTool(SurfaceTool surfaceTool, ChunkDataGenerator.BlockTypes[,,] mainDataChunk, VoxelWorld voxelWorld, ChunkResource chunk)
        {
            Vector3I chunkPosition = new(chunk.Position.X, 0, chunk.Position.Y);
            bool[] neighbourBlocksAreTransparent = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            for (long y = 0; y < ChunkDataGenerator.CHUNK_HEIGHT; y += ChunkDataGenerator.CHUNK_HEIGHT - 1)
            {
                long blockYSideCheck = y == 0 ? 1 : -1;
                neighbourBlocksAreTransparent[4] = y == 0;
                neighbourBlocksAreTransparent[5] = y != 0;
                for (long x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; ++x)
                {
                    for (long z = 1; z < ChunkDataGenerator.CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        long counter = 0;
                        neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        neighbourBlocksAreTransparent [y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }

                        _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);

                    }
                }


                for (long x = 0; x < ChunkDataGenerator.CHUNK_SIZE; x += ChunkDataGenerator.CHUNK_SIZE - 1)
                {

                    for (long z = 1; z < ChunkDataGenerator.CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        long counter = 0;
                        neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);

                        if (x == 0)
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x - 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                            neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x + 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        }


                        neighbourBlocksAreTransparent [y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }

                        _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                    }
                }
                for (long z = 0; z < ChunkDataGenerator.CHUNK_SIZE; z += ChunkDataGenerator.CHUNK_SIZE - 1)
                {
                    for (long x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; ++x)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        long counter = 0;
                        if (z == 0)
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z - 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                            neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z + 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        }
                        neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        neighbourBlocksAreTransparent [y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }

                        _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                    }
                }
                for (long x = 0; x < ChunkDataGenerator.CHUNK_SIZE; x += ChunkDataGenerator.CHUNK_SIZE - 1)
                {
                    for (long z = 0; z < ChunkDataGenerator.CHUNK_SIZE; z += ChunkDataGenerator.CHUNK_SIZE - 1)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == 0)
                        {
                            continue;
                        }
                        long counter = 0;
                        neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z - 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z + 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x - 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x + 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));

                        neighbourBlocksAreTransparent [y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }

                        _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                    }
                }
            }
        }

        private static void _GenerateChunkSidesSurfaceTool(SurfaceTool surfaceTool, ChunkResource chunk, VoxelWorld voxelWorld)
        {
            bool[] neighbourBlocksAreTransparent = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunk.Data;
            Vector3I chunkPositionGlobal = new(chunk.Position.X * ChunkDataGenerator.CHUNK_SIZE, 0, chunk.Position.Y * ChunkDataGenerator.CHUNK_SIZE);
            for (long i = 0; i < ChunkDataGenerator.CHUNK_SIZE; i += ChunkDataGenerator.CHUNK_SIZE - 1)
            {
                for (long x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; ++x)
                {
                    for (long y = 1; y < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++y)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, i];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }

                        long counter = 0;
                        if (i == 0)
                        {

                            neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(x + chunkPositionGlobal.X, y, i - 1 + chunkPositionGlobal.Z));

                            neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, i + 1]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(
                               voxelWorld.GetBlockTypeInGlobalPosition(
                                x + chunkPositionGlobal.X, y, i + 1 + chunkPositionGlobal.Z));
                            neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, i - 1]);
                        }


                        neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, i]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, i]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[x, y - 1, i]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + 1, i]);

                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }
                        _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)i), blockId, neighbourBlocksAreTransparent);


                    }
                }

                for (long y = 1; y < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++y)
                {
                    for (long z = 1; z < ChunkDataGenerator.CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        long counter = 0;
                        if (i == 0)
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(i - 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));

                            neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[i + 1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(
                               voxelWorld.GetBlockTypeInGlobalPosition(i + 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));
                            neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[i - 1, y, z]);
                        }


                        neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[i, y, z - 1]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[i, y, z + 1]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[i, y - 1, z]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[i, y + 1, z]);

                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }
                        _DrawBlockMesh(surfaceTool, new Vector3I((int)i, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
                    }
                }
            }

            for (long x = 0; x < ChunkDataGenerator.CHUNK_SIZE; x += ChunkDataGenerator.CHUNK_SIZE - 1)
            {
                for (long y = 1; y < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++y)
                {
                    for (long z = 0; z < ChunkDataGenerator.CHUNK_SIZE; z += ChunkDataGenerator.CHUNK_SIZE - 1)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkDataGenerator.BlockTypes.Air)
                        {
                            continue;
                        }
                        long counter = 0;
                        if (x == 0)
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x - 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));
                            neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighbourBlocksAreTransparent [(int)BlockSides.Right] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x + 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));
                        }
                        if (z == 0)
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);
                            neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x + chunkPositionGlobal.X, y, z - 1 + chunkPositionGlobal.Z));
                        }
                        else
                        {
                            neighbourBlocksAreTransparent [(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighbourBlocksAreTransparent [(int)BlockSides.Back] = IsBlockTransparent(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x + chunkPositionGlobal.X, y, z + 1 + chunkPositionGlobal.Z));
                        }
                        neighbourBlocksAreTransparent [(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[x, y - 1, z]);
                        neighbourBlocksAreTransparent [(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + 1, z]);



                        foreach (bool item in neighbourBlocksAreTransparent)
                        {
                            if (item)
                            {
                                ++counter;
                            }
                        }
                        if (counter == 0)
                        {
                            continue;
                        }
                        _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighbourBlocksAreTransparent);
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
                _DrawBlockFace(surfaceTool, vects1, uvs);
                _DrawBlockFace(surfaceTool, vects2, uvs);
                _DrawBlockFace(surfaceTool, vects3, uvs);
                _DrawBlockFace(surfaceTool, vects4, uvs);
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
                _DrawBlockFace(surfaceTool, verts1_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Right])
            {
                Vector3[] verts2_ = { verts[7], verts[5], verts[6], verts[4] };
                _DrawBlockFace(surfaceTool, verts2_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Front])
            {
                Vector3[] verts3_ = { verts[6], verts[4], verts[2], verts[0] };
                _DrawBlockFace(surfaceTool, verts3_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Back])
            {
                Vector3[] verts4_ = { verts[3], verts[1], verts[7], verts[5] };
                _DrawBlockFace(surfaceTool, verts4_, uvs);
            }
            if (sidesToDraw[(int)BlockSides.Up])
            {
                Vector3[] verts5_ = { verts[2], verts[3], verts[6], verts[7] };
                _DrawBlockFace(surfaceTool, verts5_, top_uvs);
            }
            if (sidesToDraw[(int)BlockSides.Down])
            {
                Vector3[] verts6_ = { verts[4], verts[5], verts[0], verts[1] };
                _DrawBlockFace(surfaceTool, verts6_, bottom_uvs);
            }

        }

        private static void _DrawBlockFace(SurfaceTool surfaceTool, Vector3[] verts, Vector2[] uvs)
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