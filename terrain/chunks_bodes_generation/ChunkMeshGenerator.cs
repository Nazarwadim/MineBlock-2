using System;
using System.Collections.Generic;

using Godot;
using Godot.Collections;
using ProcedureGeneration;

namespace ChunkBodyGeneration
{
    public static class ChunksMeshGenerator
    {
        private enum Side
        {
            Back,
            Forward,
            Left,
            Right,
            Up,
            Down
        }
        //Summary:
        //  This method creates mesh by algoritm from 2d array of blocks and from neighbouring chanks

        public static Mesh GenerateChunkMesh(ChunkResource chunkResource, VoxelWorld world)
        {
            ChunkResource.Types[,,] mainDataChunk = chunkResource.Data;
            if (mainDataChunk.Length == 0)
            {
                throw new Exception("No elements in array ChunksMeshGenerator.cs 21");
            }
            SurfaceTool surfaceTool = new SurfaceTool();
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            _GenerateInsideChunkSurfaceTool(surfaceTool, mainDataChunk);

            _GenerateUpDownYSurfaceTool(surfaceTool, mainDataChunk);

            _GenerateChunkSidesSurfaceTool(surfaceTool, chunkResource, world);

            surfaceTool.GenerateNormals();
            surfaceTool.GenerateTangents();
            surfaceTool.Index();
            ArrayMesh arrayMesh = surfaceTool.Commit();
            return arrayMesh;
        }
        //Summary:
        //  This is unsafe function!


        private static void _GenerateInsideChunkSurfaceTool(SurfaceTool surfaceTool, ChunkResource.Types[,,] mainDataChunk)
        {
            for (int i = 1; i < ChunkDataGenerator.CHUNK_SIZE - 1; ++i)
            {
                for (int j = 1; j < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++j)
                {
                    for (int k = 1; k < ChunkDataGenerator.CHUNK_SIZE - 1; ++k)
                    {
                        ChunkResource.Types blockId = mainDataChunk[i, j, k];
                        if (blockId == ChunkResource.Types.Air)
                        {
                            continue;
                        }
                        ChunkResource.Types blockFront = mainDataChunk[i, j, k - 1];
                        ChunkResource.Types blockBack = mainDataChunk[i, j, k + 1];
                        ChunkResource.Types blockLeft = mainDataChunk[i - 1, j, k];
                        ChunkResource.Types blockRight = mainDataChunk[i + 1, j, k];
                        ChunkResource.Types blockDown = mainDataChunk[i, j - 1, k];
                        ChunkResource.Types blockUp = mainDataChunk[i, j + 1, k];

                        List<Side> blockSides = _CreateListSidesToDrawbyBlocksType(blockFront,blockBack,blockLeft,blockRight,blockDown,blockUp);
                        if (blockSides.Count > 0)
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I(i, j, k), blockId, blockSides);
                        }
                    }
                }
            }
        }

        private static void _GenerateUpDownYSurfaceTool(SurfaceTool surfaceTool, ChunkResource.Types[,,] mainDataChunk)
        {
            for (int y = 0; y < ChunkDataGenerator.CHUNK_HEIGHT; y += ChunkDataGenerator.CHUNK_HEIGHT - 1)
            {
                int blockYSideCheck = y == 0 ? 1 : -1;
                Side currentYSideToDraw = y == 0 ? Side.Down : Side.Up;
                Side inverseYSideToDraw = y == 0 ? Side.Up : Side.Down;
                for (int x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; ++x)
                {
                    for (int z = 1; z < ChunkDataGenerator.CHUNK_SIZE - 1; ++z)
                    {
                        ChunkResource.Types blockId = mainDataChunk[x, y, z];
                        if (blockId == ChunkResource.Types.Air)
                        {
                            continue;
                        }
                        ChunkResource.Types blockFront = mainDataChunk[x, y, z - 1];
                        ChunkResource.Types blockBack = mainDataChunk[x, y, z + 1];
                        ChunkResource.Types blockLeft = mainDataChunk[x - 1, y, z];
                        ChunkResource.Types blockRight = mainDataChunk[x + 1, y, z];
                        ChunkResource.Types blockY = mainDataChunk[x + 1, y + blockYSideCheck, z];
                        List<Side> blockSides = new List<Side>() { currentYSideToDraw };
                        if (ChunksBodyGenerator.IsBlockTransparent(blockFront))
                        {
                            blockSides.Add(Side.Forward);
                        }
                        if (ChunksBodyGenerator.IsBlockTransparent(blockBack))
                        {
                            blockSides.Add(Side.Back);
                        }
                        if (ChunksBodyGenerator.IsBlockTransparent(blockLeft))
                        {
                            blockSides.Add(Side.Left);
                        }
                        if (ChunksBodyGenerator.IsBlockTransparent(blockRight))
                        {
                            blockSides.Add(Side.Right);
                        }
                        if (ChunksBodyGenerator.IsBlockTransparent(blockY))
                        {
                            blockSides.Add(inverseYSideToDraw);
                        }
                        _DrawBlockMesh(surfaceTool, new Vector3I(x, y, z), blockId, blockSides);

                    }
                }
            }
        }

        private static void _GenerateChunkSidesSurfaceTool(SurfaceTool surfaceTool, ChunkResource chunk, VoxelWorld voxelWorld)
        {
            ChunkResource.Types[,,] mainDataChunk = chunk.Data;
            Vector3I chunkPosition = new Vector3I(chunk.Position.X, 0, chunk.Position.Y);
            for (int i = 0; i < ChunkDataGenerator.CHUNK_SIZE; i += ChunkDataGenerator.CHUNK_SIZE - 1)
            {
                for (int x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; x++)
                {
                    for (int y = 1; y < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++y)
                    {
                        ChunkResource.Types blockId = mainDataChunk[x, y, i];
                        if (blockId == ChunkResource.Types.Air)
                        {
                            continue;
                        }
                        
                        ChunkResource.Types blockFront = voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I(x, y, i - 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE);
                        ChunkResource.Types blockBack = voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I(x, y, i + 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE);
                        ChunkResource.Types blockLeft = mainDataChunk[x - 1, y, i];
                        ChunkResource.Types blockRight = mainDataChunk[x + 1, y, i];
                        ChunkResource.Types blockDown = mainDataChunk[x, y - 1, i];
                        ChunkResource.Types blockUp = mainDataChunk[x, y + 1, i];
                        List<Side> blockSides = _CreateListSidesToDrawbyBlocksType(blockFront,blockBack,blockLeft,blockRight,blockDown,blockUp);
                        if (blockSides.Count > 0)
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I(x, y, i), blockId, blockSides);
                        }


                        
                    }
                }

                for (int y = 1; y < ChunkDataGenerator.CHUNK_HEIGHT - 1; y++)
                {
                    for (int z = 1; z < ChunkDataGenerator.CHUNK_SIZE -1; ++z)
                    {
                        ChunkResource.Types blockId = mainDataChunk[i, y, z];
                        if (blockId == ChunkResource.Types.Air)
                        {
                            continue;
                        }
                        ChunkResource.Types blockFront = mainDataChunk[i, y, z - 1];
                        ChunkResource.Types blockBack = mainDataChunk[i, y, z + 1];
                        ChunkResource.Types blockLeft = voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I(i - 1, y, z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE);
                        ChunkResource.Types blockRight = voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I(i + 1, y, z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE);
                        ChunkResource.Types blockDown = mainDataChunk[i, y - 1, z];
                        ChunkResource.Types blockUp = mainDataChunk[i, y + 1, z];
                        List<Side> blockSides = _CreateListSidesToDrawbyBlocksType(blockFront,blockBack,blockLeft,blockRight,blockDown,blockUp);
                        
                        if (blockSides.Count > 0)
                        {
                            _DrawBlockMesh(surfaceTool, new Vector3I(i, y, z), blockId, blockSides);
                        }
                    }
                }
            }
        }

        private static List<Side> _CreateListSidesToDrawbyBlocksType(ChunkResource.Types blockFront, ChunkResource.Types blockBack, ChunkResource.Types blockLeft, ChunkResource.Types blockRight,
            ChunkResource.Types blockDown, ChunkResource.Types blockUp)
        {
            List<Side> blockSides = new List<Side>();
            if (ChunksBodyGenerator.IsBlockTransparent(blockFront))
            {
                blockSides.Add(Side.Forward);
            }
            if (ChunksBodyGenerator.IsBlockTransparent(blockBack))
            {
                blockSides.Add(Side.Back);
            }
            if (ChunksBodyGenerator.IsBlockTransparent(blockLeft))
            {
                blockSides.Add(Side.Left);
            }
            if (ChunksBodyGenerator.IsBlockTransparent(blockRight))
            {
                blockSides.Add(Side.Right);
            }
            if (ChunksBodyGenerator.IsBlockTransparent(blockDown))
            {
                blockSides.Add(Side.Down);
            }
            if (ChunksBodyGenerator.IsBlockTransparent(blockUp))
            {
                blockSides.Add(Side.Up);
            }
            return blockSides;
        }

        private static void _DrawBlockMesh(SurfaceTool surfaceTool, Vector3I blockSubPosition, ChunkResource.Types blockId, List<Side> sidesToDraw)
        {
            if (sidesToDraw.Count > 6 || sidesToDraw.Count == 0)
            {
                throw new Exception("Too much or no elements in array. Want to draw 2 times that size?");
            }
            Vector3[] verts = ChunksBodyGenerator.CalculateBlockVerts(blockSubPosition);
            Vector2[] uvs = ChunksBodyGenerator.CalculateBlockUvs((int)blockId);
            Vector2[] top_uvs = (Vector2[])uvs.Clone();
            Vector2[] bottom_uvs = (Vector2[])uvs.Clone();



            // In processs!!!!!!!1
            // if (blockId == 27 || blockId == 28)
            // {
            //     Vector3[] vects1 = {verts[2], verts[0], verts[7], verts[5]};
            //     Vector3[] vects2 = {verts[7], verts[5], verts[2], verts[0]};
            //     Vector3[] vects3 = {verts[3], verts[1], verts[6], verts[4]};
            //     Vector3[] vects4 = {verts[6], verts[4], verts[3], verts[1]};
            //     _DrawBlockFace(surfaceTool, vects1, uvs);
            //     _DrawBlockFace(surfaceTool, vects2, uvs);
            //     _DrawBlockFace(surfaceTool, vects3, uvs);
            //     _DrawBlockFace(surfaceTool, vects4, uvs);
            //     return;
            // }
            if (blockId == ChunkResource.Types.Grass)
            {
                top_uvs = ChunksBodyGenerator.CalculateBlockUvs(0);
                bottom_uvs = ChunksBodyGenerator.CalculateBlockUvs(2);
            }
            else if (blockId == ChunkResource.Types.Furnace)
            {
                top_uvs = ChunksBodyGenerator.CalculateBlockUvs(31);
                bottom_uvs = top_uvs;
            }
            foreach (Side sideToDraw in sidesToDraw)
            {

                switch (sideToDraw)
                {
                    case Side.Left:
                        Vector3[] verts1_ = { verts[2], verts[0], verts[3], verts[1] };
                        _DrawBlockFace(surfaceTool, verts1_, uvs);
                        break;
                    case Side.Right:
                        Vector3[] verts2_ = { verts[7], verts[5], verts[6], verts[4] };
                        _DrawBlockFace(surfaceTool, verts2_, uvs);
                        break;
                    case Side.Forward:
                        Vector3[] verts3_ = { verts[6], verts[4], verts[2], verts[0] };
                        _DrawBlockFace(surfaceTool, verts3_, uvs);
                        break;
                    case Side.Back:
                        Vector3[] verts4_ = { verts[3], verts[1], verts[7], verts[5] };
                        _DrawBlockFace(surfaceTool, verts4_, uvs);
                        break;
                    case Side.Up:
                        Vector3[] verts5_ = { verts[2], verts[3], verts[6], verts[7] };
                        _DrawBlockFace(surfaceTool, verts5_, top_uvs);
                        break;
                    case Side.Down:
                        Vector3[] verts6_ = { verts[4], verts[5], verts[0], verts[1] };
                        _DrawBlockFace(surfaceTool, verts6_, bottom_uvs);
                        break;
                }
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
    }
}