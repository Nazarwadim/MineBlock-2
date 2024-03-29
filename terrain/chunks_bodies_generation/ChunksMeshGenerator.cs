using System;
using System.Drawing;
using System.Linq;
using Godot;
using Godot.Collections;


namespace Terrain.ChunkBodyGeneration
{
    using ProcedureGeneration;
    using BlockSides = ChunkBodyGenerator.BlockSide;
    public partial class ChunksMeshGenerator : GodotObject
    {
        public static ulong counterTemp = 0;
        const int TEXTURE_SHEET_WIDTH = 8;
        const float TEXTURE_TILE_SIZE = 1.0f / TEXTURE_SHEET_WIDTH;
        public const short CHUNK_SIZE = ChunkDataGenerator.CHUNK_SIZE;
        public const short CHUNK_HEIGHT = ChunkDataGenerator.CHUNK_HEIGHT;

        /// <summary>
        ///    This method creates mesh by algorithm from 2d array of blocks and from neighboring chanks
        /// </summary>
        public static Mesh GenerateChunkMesh(ChunkResource chunkResource, Dictionary<Vector2I, ChunkResource> chunksResources)
        {
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunkResource.Data;
            if (mainDataChunk.Length == 0)
            {
                throw new Exception("No elements in array ChunksMeshGenerator.cs ");
            }

            bool existsSideChunks = true;

            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X - 1, chunkResource.Position.Y), out ChunkResource leftChunk))
            {
                existsSideChunks = false;
                GD.Print("1", " ", chunkResource.Position);
            }
            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X + 1, chunkResource.Position.Y), out ChunkResource rightChunk))
            {
                existsSideChunks = false;
                GD.Print("2");
            }
            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X, chunkResource.Position.Y + 1), out ChunkResource upChunk))
            {
                existsSideChunks = false;
                GD.Print("3");
            }
            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X, chunkResource.Position.Y - 1), out ChunkResource downChunk))
            {
                existsSideChunks = false;
                GD.Print("4");
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

        private static void _GenerateInsideChunkSurfaceTool(SurfaceTool surfaceTool, ChunkDataGenerator.BlockTypes[,,] mainDataChunk)
        {
            bool[] neighborBlocksAreTransparent = new bool[6];
            for (long i = 1; i < CHUNK_SIZE - 1; ++i)
            {
                for (long j = 1; j < CHUNK_HEIGHT - 1; ++j)
                {
                    for (long k = 1; k < CHUNK_SIZE - 1; ++k)
                    {

                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, j, k];
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }

                        neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[i, j, k - 1]);
                        neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[i, j, k + 1]);
                        neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[i - 1, j, k]);
                        neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[i + 1, j, k]);
                        neighborBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[i, j - 1, k]);
                        neighborBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[i, j + 1, k]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)i, (int)j, (int)k), blockId, neighborBlocksAreTransparent);
                        }

                    }
                }
            }
        }

        private static void _GenerateUpDownYSurfaceTool(SurfaceTool surfaceTool, ChunkResource chunk,
            ChunkResource chunkLeft, ChunkResource chunkRight, ChunkResource chunkDown, ChunkResource chunkUp)
        {
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunk.Data;

            ChunkDataGenerator.BlockTypes[,,] leftDataChunk = chunkLeft.Data;
            ChunkDataGenerator.BlockTypes[,,] rightDataChunk = chunkRight.Data;
            ChunkDataGenerator.BlockTypes[,,] downDataChunk = chunkDown.Data;
            ChunkDataGenerator.BlockTypes[,,] upDataChunk = chunkUp.Data;
            bool[] neighborBlocksAreTransparent = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            for (long y = 0; y < CHUNK_HEIGHT; y += CHUNK_HEIGHT - 1)
            {
                long blockYSideCheck = y == 0 ? 1 : -1;
                neighborBlocksAreTransparent[4] = y == 0;
                neighborBlocksAreTransparent[5] = y != 0;
                for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }
                        neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                        neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);
                        neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                        neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        neighborBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighborBlocksAreTransparent);
                        }

                    }
                }


                for (long x = 0; x < CHUNK_SIZE; x += CHUNK_SIZE - 1)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }
                        neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                        neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);

                        if (x == 0)
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        }
                        else
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                        }


                        neighborBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighborBlocksAreTransparent);
                        }
                    }
                }
                for (long z = 0; z < CHUNK_SIZE; z += CHUNK_SIZE - 1)
                {
                    for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }
                        if (z == 0)
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, z + 1]);
                        }
                        else
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                        }
                        neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                        neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, z]);
                        neighborBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighborBlocksAreTransparent);
                        }
                    }
                }
                for (long x = 0; x < CHUNK_SIZE; x += CHUNK_SIZE - 1)
                {
                    for (long z = 0; z < CHUNK_SIZE; z += CHUNK_SIZE - 1)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }
                        if (x == 0)
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[1, y, z]);
                        }
                        else
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                        }
                        if (z == 0)
                        {

                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, 1]);
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                        }
                        else
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                        }

                        neighborBlocksAreTransparent[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighborBlocksAreTransparent);
                        }
                    }
                }
            }
        }

        private static void _GenerateChunkSidesSurfaceTool(SurfaceTool surfaceTool, ChunkResource chunk,
            ChunkResource chunkLeft, ChunkResource chunkRight, ChunkResource chunkDown, ChunkResource chunkUp)
        {
            bool[] neighborBlocksAreTransparent = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunk.Data;

            ChunkDataGenerator.BlockTypes[,,] leftDataChunk = chunkLeft.Data;
            ChunkDataGenerator.BlockTypes[,,] rightDataChunk = chunkRight.Data;
            ChunkDataGenerator.BlockTypes[,,] downDataChunk = chunkDown.Data;
            ChunkDataGenerator.BlockTypes[,,] upDataChunk = chunkUp.Data;

            for (long i = 0; i < CHUNK_SIZE; i += CHUNK_SIZE - 1)
            {
                for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                {
                    for (long y = 1; y < CHUNK_HEIGHT - 1; ++y)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, i];
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }
                        if (i == 0)
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, 1]);

                        }
                        else
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, i - 1]);
                        }


                        neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, i]);
                        neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[x + 1, y, i]);
                        neighborBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[x, y - 1, i]);
                        neighborBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + 1, i]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)i), blockId, neighborBlocksAreTransparent);
                        }


                    }
                }

                for (long y = 1; y < CHUNK_HEIGHT - 1; ++y)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {

                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, y, z];
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }
                        if (i == 0)
                        {

                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[1, y, z]);
                        }
                        else
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[i - 1, y, z]);
                        }


                        neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[i, y, z - 1]);
                        neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[i, y, z + 1]);
                        neighborBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[i, y - 1, z]);
                        neighborBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[i, y + 1, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)i, (int)y, (int)z), blockId, neighborBlocksAreTransparent);
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
                        if (ChunkBodyGenerator.IsBlockNotStatic(blockId))
                        {
                            continue;
                        }
                        if (x == 0)
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(mainDataChunk[1, y, z]);
                        }
                        else
                        {

                            neighborBlocksAreTransparent[(int)BlockSides.Left] = IsBlockTransparent(mainDataChunk[x - 1, y, z]);
                            neighborBlocksAreTransparent[(int)BlockSides.Right] = IsBlockTransparent(rightDataChunk[0, y, z]);
                        }
                        if (z == 0)
                        {

                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(mainDataChunk[x, y, 1]);
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                        }
                        else
                        {
                            neighborBlocksAreTransparent[(int)BlockSides.Front] = IsBlockTransparent(mainDataChunk[x, y, z - 1]);
                            neighborBlocksAreTransparent[(int)BlockSides.Back] = IsBlockTransparent(upDataChunk[x, y, 0]);
                        }
                        neighborBlocksAreTransparent[(int)BlockSides.Down] = IsBlockTransparent(mainDataChunk[x, y - 1, z]);
                        neighborBlocksAreTransparent[(int)BlockSides.Up] = IsBlockTransparent(mainDataChunk[x, y + 1, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksAreTransparent))
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I((int)x, (int)y, (int)z), blockId, neighborBlocksAreTransparent);
                        }
                    }
                }
            }


        }

        private static void _DrawBlockMesh(SurfaceTool surfaceTool, Vector3I blockSubPosition, ChunkDataGenerator.BlockTypes blockId, bool[] sidesToDraw)
        {

            Vector3[] verts = ChunkBodyGenerator.CalculateBlockVerts(blockSubPosition);
            Vector2[] uvs = CalculateBlockUvs((long)blockId);

            Vector2[] uvsFront = uvs;
            Vector2[] uvsBack = uvs;
            Vector2[] uvsLeft = uvs;
            Vector2[] uvsRight = uvs;
            Vector2[] uvsTop = uvs;
            Vector2[] uvsBottom = uvs;


            if (blockId == ChunkDataGenerator.BlockTypes.WitheredGrass || blockId == ChunkDataGenerator.BlockTypes.Grass)
            {
                Vector3[] vets1 = { verts[2], verts[0], verts[7], verts[5] };
                Vector3[] vets2 = { verts[7], verts[5], verts[2], verts[0] };
                Vector3[] vets3 = { verts[3], verts[1], verts[6], verts[4] };
                Vector3[] vets4 = { verts[6], verts[4], verts[3], verts[1] };
                DrawBlockFace(surfaceTool, vets1, uvs);
                DrawBlockFace(surfaceTool, vets2, uvs);
                DrawBlockFace(surfaceTool, vets3, uvs);
                DrawBlockFace(surfaceTool, vets4, uvs);
                return;
            }


            switch (blockId)
            {
                case ChunkDataGenerator.BlockTypes.GrassBlock:
                    uvsTop = (Vector2[])CalculateBlockUvs(0).Clone();
                    uvsBottom = (Vector2[])CalculateBlockUvs(2).Clone();
                    break;
                case ChunkDataGenerator.BlockTypes.Furnace:
                    uvsTop = (Vector2[])CalculateBlockUvs(31).Clone();
                    uvsBottom = uvsTop;
                    break;
                case ChunkDataGenerator.BlockTypes.LogUp:
                    uvsTop = CalculateBlockUvs(30);
                    uvsBottom = uvsTop;
                    break;
                case ChunkDataGenerator.BlockTypes.LogX:
                    uvsBack = uvsFront = uvsTop = uvsBottom = _Rotate90DegreesUvs(CalculateBlockUvs((int)ChunkDataGenerator.BlockTypes.LogUp));
                    break;
                case ChunkDataGenerator.BlockTypes.LogZ:
                    uvsBottom = uvsTop = CalculateBlockUvs((int)ChunkDataGenerator.BlockTypes.LogUp);
                    uvsRight = uvsLeft = _Rotate90DegreesUvs(uvsBottom);
                    uvsBack = uvsFront = CalculateBlockUvs(30);
                    break;
                case ChunkDataGenerator.BlockTypes.BookShelf:
                    uvsTop = CalculateBlockUvs(4);
                    uvsBottom = uvsTop;
                    break;
            }


            if (sidesToDraw[(int)BlockSides.Left])
            {
                Vector3[] verts1_ = { verts[2], verts[0], verts[3], verts[1] };
                DrawBlockFace(surfaceTool, verts1_, uvsLeft);
            }
            if (sidesToDraw[(int)BlockSides.Right])
            {
                Vector3[] verts2_ = { verts[7], verts[5], verts[6], verts[4] };
                DrawBlockFace(surfaceTool, verts2_, uvsRight);
            }
            if (sidesToDraw[(int)BlockSides.Front])
            {
                Vector3[] verts3_ = { verts[6], verts[4], verts[2], verts[0] };
                DrawBlockFace(surfaceTool, verts3_, uvsFront);
            }
            if (sidesToDraw[(int)BlockSides.Back])
            {
                Vector3[] verts4_ = { verts[3], verts[1], verts[7], verts[5] };
                DrawBlockFace(surfaceTool, verts4_, uvsBack);
            }
            if (sidesToDraw[(int)BlockSides.Up])
            {
                Vector3[] verts5_ = { verts[2], verts[3], verts[6], verts[7] };
                DrawBlockFace(surfaceTool, verts5_, uvsTop);
            }
            if (sidesToDraw[(int)BlockSides.Down])
            {
                Vector3[] verts6_ = { verts[4], verts[5], verts[0], verts[1] };
                DrawBlockFace(surfaceTool, verts6_, uvsBottom);
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
        private static void _DrawBlockMesh(SurfaceTool surfaceTool, Vector3I blockSubPosition, ChunkDataGenerator.BlockTypes blockId, Array<bool> sidesToDraw)
        {
            if (sidesToDraw.Count != 6)
            {
                throw new OutOfMemoryException("Sides to draw size should be 6");
            }
            bool[] bools = sidesToDraw.ToArray();
            _DrawBlockMesh(surfaceTool, blockSubPosition, blockId, bools);
        }

        private static Vector2[] _Rotate90DegreesUvs(Vector2[] uvs)
        {
            Vector2[] tempUvs = (Vector2[])uvs.Clone();
            tempUvs[0] = uvs[2];
            tempUvs[1] = uvs[0];
            tempUvs[2] = uvs[3];
            tempUvs[3] = uvs[1];
            return tempUvs;
        }
        public static Vector2[] CalculateBlockUvs(long blockId)
        {
            long row = blockId / TEXTURE_SHEET_WIDTH;
            long col = blockId % TEXTURE_SHEET_WIDTH;
            Vector2[] array = {
                // we adding offset because godot lags
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

