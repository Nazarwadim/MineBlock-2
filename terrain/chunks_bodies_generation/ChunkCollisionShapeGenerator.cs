using System;
using System.Collections.Generic;
using Godot;
using ProcedureGeneration;
using Godot.Collections;
using System.Linq;

using BlockSides = ChunkBodyGeneration.ChunksBodyGenerator.BlockSide;
namespace ChunkBodyGeneration
{
    
    public static class ChunksShapeGenerator
    {
        public static ConcavePolygonShape3D GenerateChunkShape(ChunkResource chunkResource, VoxelWorld world)
        {
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunkResource.Data;
            if (mainDataChunk.Length == 0)
            {
                throw new Exception("No elements in array ChunksColisionGenerator.cs 18");
            }

            
            List<Vector3> points = new();
            
            _GenerateInsideChunkSurfaceTool(points, mainDataChunk);            
            _GenerateUpDownYSurfaceTool(points, mainDataChunk, world, chunkResource);
            _GenerateChunkSidesSurfaceTool(points, chunkResource, world);
            ConcavePolygonShape3D concavePolygonShape3D = new()
            {
                Data = points.ToArray()
            };
            return concavePolygonShape3D;
        }

        public static void SetBlockCollisionIntoShape(ConcavePolygonShape3D convexPolygonShape3D, ChunkDataGenerator.BlockTypes blockType, Vector3I blockPosition)
        {
            Vector3[] data = convexPolygonShape3D.Data;
            List<Vector3> pointsData = new List<Vector3>(data);
            List<Vector3> pointsToSet = new List<Vector3>();
            Vector3[] verts = ChunksBodyGenerator.CalculateBlockVerts(blockPosition);
            Vector3[] verts1_ = { verts[2], verts[0], verts[3], verts[1] };
            _SetBlockColisionTriangle(pointsToSet, verts1_);
         
            Vector3[] verts2_ = { verts[7], verts[5], verts[6], verts[4] };
            _SetBlockColisionTriangle(pointsToSet, verts2_);

            Vector3[] verts3_ = { verts[6], verts[4], verts[2], verts[0] };
            _SetBlockColisionTriangle(pointsToSet, verts3_);
           
            Vector3[] verts4_ = { verts[3], verts[1], verts[7], verts[5] };
           _SetBlockColisionTriangle(pointsToSet, verts4_);

            Vector3[] verts5_ = { verts[2], verts[3], verts[6], verts[7] };
            _SetBlockColisionTriangle(pointsToSet, verts5_);
            Vector3[] verts6_ = { verts[4], verts[5], verts[0], verts[1] };
            _SetBlockColisionTriangle(pointsToSet, verts6_);
            
            pointsData.RemoveAll(item => pointsToSet.Contains(item));
            convexPolygonShape3D.Data = pointsData.ToArray();
        }


        //Summary:
        //  This is unsafe function!
        private static void _GenerateInsideChunkSurfaceTool(List<Vector3> pointsData, ChunkDataGenerator.BlockTypes[,,] mainDataChunk)
        {
            bool[] neighbourBlocksArePhisics = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            for (long i = 1; i < ChunkDataGenerator.CHUNK_SIZE - 1; ++i)
            {
                for (long j = 1; j < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++j)
                {
                    for (long k = 1; k < ChunkDataGenerator.CHUNK_SIZE - 1; ++k)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, j, k];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        long counter = 0;
                        neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[i, j, k - 1]);
                        neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[i, j, k + 1]);
                        neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[i - 1, j, k]);
                        neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[i + 1, j, k]);
                        neighbourBlocksArePhisics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[i, j - 1, k]);
                        neighbourBlocksArePhisics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[i, j + 1, k]);

                        foreach (bool item in neighbourBlocksArePhisics)
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

                        _BuildBlockColision(pointsData, new Vector3I((int)i, (int)j, (int)k), neighbourBlocksArePhisics);

                    }
                }
            }
        }

        private static void _GenerateUpDownYSurfaceTool(List<Vector3> pointsData, ChunkDataGenerator.BlockTypes[,,] mainDataChunk, VoxelWorld voxelWorld, ChunkResource chunk)
        {
            Vector3I chunkPosition = new(chunk.Position.X, 0, chunk.Position.Y);
            bool[] neighbourBlocksArePhisics = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            for (long y = 0; y < ChunkDataGenerator.CHUNK_HEIGHT; y += ChunkDataGenerator.CHUNK_HEIGHT - 1)
            {
                long blockYSideCheck = y == 0 ? 1 : -1;
                neighbourBlocksArePhisics[(int)BlockSides.Down] = y == 0;
                neighbourBlocksArePhisics[(int)BlockSides.Up] = y != 0;
                for (long x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; ++x)
                {
                    for (long z = 1; z < ChunkDataGenerator.CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        ulong counter = 0;

                        neighbourBlocksArePhisics[(int)BlockSides.Front] =IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                        neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, z + 1]);
                        neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                        neighbourBlocksArePhisics[(int)BlockSides.Right]  = IsBlockNotCollide(mainDataChunk[x + 1, y, z]);
                        neighbourBlocksArePhisics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksArePhisics)
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

                        _BuildBlockColision(pointsData, new Vector3I((int)x, (int)y, (int)z),  neighbourBlocksArePhisics);

                    }
                }


                for (long x = 0; x < ChunkDataGenerator.CHUNK_SIZE; x += ChunkDataGenerator.CHUNK_SIZE - 1)
                {
                    for (long z = 1; z < ChunkDataGenerator.CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        long counter = 0;
                        neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                        neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, z + 1]);

                        if (x == 0)
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x - 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                            neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                            neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x + 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        }


                        neighbourBlocksArePhisics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksArePhisics)
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

                        _BuildBlockColision(pointsData, new Vector3I((int)x, (int)y, (int)z), neighbourBlocksArePhisics);
                    }
                }
                for (long z = 0; z < ChunkDataGenerator.CHUNK_SIZE; z += ChunkDataGenerator.CHUNK_SIZE - 1)
                {
                    for (long x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; ++x)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        ulong counter = 0;
                        if (z == 0)
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z - 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                            neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, z + 1]);
                        }
                        else
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                            neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z + 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        }
                        neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                        neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, z]);
                        neighbourBlocksArePhisics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksArePhisics)
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

                        _BuildBlockColision(pointsData, new Vector3I((int)x, (int)y, (int)z),  neighbourBlocksArePhisics);
                    }
                }
                for (long x = 0; x < ChunkDataGenerator.CHUNK_SIZE; x += ChunkDataGenerator.CHUNK_SIZE - 1)
                {
                    for (long z = 0; z < ChunkDataGenerator.CHUNK_SIZE; z += ChunkDataGenerator.CHUNK_SIZE - 1)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        ulong counter = 0;
                        neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z - 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x, (int)y, (int)z + 1) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x - 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));
                        neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(new Vector3I((int)x + 1, (int)y, (int)z) + chunkPosition * ChunkDataGenerator.CHUNK_SIZE));

                        neighbourBlocksArePhisics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        foreach (bool item in neighbourBlocksArePhisics)
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

                        _BuildBlockColision(pointsData, new Vector3I((int)x, (int)y, (int)z),  neighbourBlocksArePhisics);
                    }
                }
            }
        }

        private static void _GenerateChunkSidesSurfaceTool(List<Vector3> pointsData, ChunkResource chunk, VoxelWorld voxelWorld)
        {
            bool[] neighbourBlocksArePhisics = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunk.Data;
            Vector3I chunkPositionGlobal = new(chunk.Position.X * ChunkDataGenerator.CHUNK_SIZE, 0, chunk.Position.Y * ChunkDataGenerator.CHUNK_SIZE);
            for (long i = 0; i < ChunkDataGenerator.CHUNK_SIZE; i += ChunkDataGenerator.CHUNK_SIZE - 1)
            {
                for (long x = 1; x < ChunkDataGenerator.CHUNK_SIZE - 1; ++x)
                {
                    for (long y = 1; y < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++y)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, i];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }

                        long counter = 0;
                        if (i == 0)
                        {
                            
                            neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(x + chunkPositionGlobal.X, y, i - 1 + chunkPositionGlobal.Z));

                            neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, i + 1]);
                        }
                        else
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(
                               voxelWorld.GetBlockTypeInGlobalPosition(
                                x + chunkPositionGlobal.X, y, i + 1 + chunkPositionGlobal.Z));
                            neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, i - 1]);
                        }


                        neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, i]);
                        neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, i]);
                        neighbourBlocksArePhisics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[x, y - 1, i]);
                        neighbourBlocksArePhisics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + 1, i]);

                        foreach (bool item in neighbourBlocksArePhisics)
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
                        _BuildBlockColision(pointsData, new Vector3I((int)x, (int)y, (int)i), neighbourBlocksArePhisics);


                    }
                }

                for (long y = 1; y < ChunkDataGenerator.CHUNK_HEIGHT - 1; ++y)
                {
                    for (long z = 1; z < ChunkDataGenerator.CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, y, z];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        long counter = 0;
                        if (i == 0)
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(i - 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));

                            neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[i + 1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(
                               voxelWorld.GetBlockTypeInGlobalPosition(i + 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));
                            neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[i - 1, y, z]);
                        }


                        neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[i, y, z - 1]);
                        neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[i, y, z + 1]);
                        neighbourBlocksArePhisics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[i, y - 1, z]);
                        neighbourBlocksArePhisics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[i, y + 1, z]);

                        foreach (bool item in neighbourBlocksArePhisics)
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
                        _BuildBlockColision(pointsData, new Vector3I((int)i, (int)y, (int)z), neighbourBlocksArePhisics);
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
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        long counter = 0;
                        if (x == 0)
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x - 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));
                            neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, z]);
                        }
                        else
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                            neighbourBlocksArePhisics[(int)BlockSides.Right] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x + 1 + chunkPositionGlobal.X, y, z + chunkPositionGlobal.Z));
                        }
                        if (z == 0)
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, z + 1]);
                            neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x + chunkPositionGlobal.X, y, z - 1 + chunkPositionGlobal.Z));
                        }
                        else
                        {
                            neighbourBlocksArePhisics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                            neighbourBlocksArePhisics[(int)BlockSides.Back] = IsBlockNotCollide(
                                voxelWorld.GetBlockTypeInGlobalPosition(
                                    x + chunkPositionGlobal.X, y, z + 1 + chunkPositionGlobal.Z));
                        }
                        neighbourBlocksArePhisics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[x, y - 1, z]);
                        neighbourBlocksArePhisics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + 1, z]);

                        
                        foreach (bool item in neighbourBlocksArePhisics)
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
                        _BuildBlockColision(pointsData, new Vector3I((int)x, (int)y, (int)z), neighbourBlocksArePhisics);
                    }
                }
            }

        }

        private static void _BuildBlockColision(List<Vector3> pointsData, Vector3I blockSubPosition, bool[] sidesToDraw)
        {
            Vector3[] verts = ChunksBodyGenerator.CalculateBlockVerts(blockSubPosition);

            if (sidesToDraw[(int)BlockSides.Left])
            {
                Vector3[] verts1_ = { verts[2], verts[0], verts[3], verts[1] };
                _SetBlockColisionTriangle(pointsData, verts1_);
            }
            if (sidesToDraw[(int)BlockSides.Right])
            {
                Vector3[] verts2_ = { verts[7], verts[5], verts[6], verts[4] };
                _SetBlockColisionTriangle(pointsData, verts2_);
            }
            if (sidesToDraw[(int)BlockSides.Front])
            {
                Vector3[] verts3_ = { verts[6], verts[4], verts[2], verts[0] };
                _SetBlockColisionTriangle(pointsData, verts3_);
            }
            if (sidesToDraw[(int)BlockSides.Back])
            {
                Vector3[] verts4_ = { verts[3], verts[1], verts[7], verts[5] };
                _SetBlockColisionTriangle(pointsData, verts4_);
            }
            if (sidesToDraw[(int)BlockSides.Up])
            {
                Vector3[] verts5_ = { verts[2], verts[3], verts[6], verts[7] };
                _SetBlockColisionTriangle(pointsData, verts5_);
            }
            if (sidesToDraw[(int)BlockSides.Down])
            {
                Vector3[] verts6_ = { verts[4], verts[5], verts[0], verts[1] };
                _SetBlockColisionTriangle(pointsData, verts6_);
            }

        }
	// new_shape.set_faces([Vector3.ZERO, Vector3(1,0,0), Vector3(0,1,0),
	// 	Vector3(1,1,0), Vector3(1,0,0), Vector3(0,1,0)])

        private static void _SetBlockColisionTriangle(List<Vector3> points, Vector3[] verts)
        {
            points.Add(verts[1]);
            points.Add(verts[2]);
            points.Add(verts[3]);

            points.Add(verts[2]);
            points.Add(verts[1]);
            points.Add(verts[0]);
        }

        public static bool IsBlockNotCollide(ChunkDataGenerator.BlockTypes blockId)
        {
            return blockId == ChunkDataGenerator.BlockTypes.Air || ((int)blockId > 25 && (int)blockId < 30);
        }
    }
}