using System;
using System.Linq;
using Godot;
using Godot.Collections;
using ProcedureGeneration;

using BlockSides = ChunkBodyGeneration.ChunkBodyGenerator.BlockSide;
namespace ChunkBodyGeneration
{

    public partial class ChunkShapeGenerator : GodotObject
    {
        public const short CHUNK_SIZE = ChunkDataGenerator.CHUNK_SIZE;
        public const short CHUNK_HEIGHT = ChunkDataGenerator.CHUNK_HEIGHT;

        public static ConcavePolygonShape3D GenerateChunkShape(ChunkResource chunkResource, Dictionary<Vector2I, ChunkResource> chunksResources)
        {

            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunkResource.Data;
            if (mainDataChunk.Length == 0)
            {
                throw new Exception("No elements in array ChunksCollisionGenerator.cs 18");
            }

            bool existsSideChunks = true;
            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X - 1, chunkResource.Position.Y), out ChunkResource leftChunk))
            {
                existsSideChunks = false;
            }
            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X + 1, chunkResource.Position.Y), out ChunkResource rightChunk))
            {
                existsSideChunks = false;
            }
            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X, chunkResource.Position.Y + 1), out ChunkResource upChunk))
            {
                existsSideChunks = false;
            }
            if (!chunksResources.TryGetValue(new Vector2I(chunkResource.Position.X, chunkResource.Position.Y - 1), out ChunkResource downChunk))
            {
                existsSideChunks = false;
            }
            if (!existsSideChunks)
            {
                throw new NullReferenceException("Try to generate chunkBody without chunkResources near it!");
            }
            System.Collections.Generic.List<Vector3> points = new(1024);
            _GenerateInsideChunkSurfaceTool(points, mainDataChunk);
            _GenerateUpDownYSurfaceTool(points, chunkResource, leftChunk, rightChunk, downChunk, upChunk);
            _GenerateChunkSidesSurfaceTool(points, chunkResource, leftChunk, rightChunk, downChunk, upChunk);

            ConcavePolygonShape3D concavePolygonShape3D = new()
            {
                Data = points.ToArray()
            };
            concavePolygonShape3D.Margin = 0.2f;
            return concavePolygonShape3D;
        }

        //Summary:
        //  This is unsafe function!
        private static void _GenerateInsideChunkSurfaceTool(System.Collections.Generic.List<Vector3> pointsData, ChunkDataGenerator.BlockTypes[,,] mainDataChunk)
        {
            bool[] neighborBlocksArePhysics = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
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
                        neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[i, j, k - 1]);
                        neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[i, j, k + 1]);
                        neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[i - 1, j, k]);
                        neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[i + 1, j, k]);
                        neighborBlocksArePhysics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[i, j - 1, k]);
                        neighborBlocksArePhysics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[i, j + 1, k]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)i, (int)j, (int)k), neighborBlocksArePhysics);
                        }

                    }
                }
            }
        }

        private static void _GenerateUpDownYSurfaceTool(System.Collections.Generic.List<Vector3> pointsData, ChunkResource chunk,
            ChunkResource chunkLeft, ChunkResource chunkRight, ChunkResource chunkDown, ChunkResource chunkUp)
        {
            ChunkDataGenerator.BlockTypes[,,] mainDataChunk = chunk.Data;

            ChunkDataGenerator.BlockTypes[,,] leftDataChunk = chunkLeft.Data;
            ChunkDataGenerator.BlockTypes[,,] rightDataChunk = chunkRight.Data;
            ChunkDataGenerator.BlockTypes[,,] downDataChunk = chunkDown.Data;
            ChunkDataGenerator.BlockTypes[,,] upDataChunk = chunkUp.Data;
            bool[] neighborBlocksArePhysics = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
            for (long y = 0; y < CHUNK_HEIGHT; y += CHUNK_HEIGHT - 1)
            {
                long blockYSideCheck = y == 0 ? 1 : -1;
                neighborBlocksArePhysics[4] = y == 0;
                neighborBlocksArePhysics[5] = y != 0;
                for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                       if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                        neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, z + 1]);
                        neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                        neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, z]);
                        neighborBlocksArePhysics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)x, (int)y, (int)z), neighborBlocksArePhysics);
                        }
                    }
                }


                for (long x = 0; x < CHUNK_SIZE; x += CHUNK_SIZE - 1)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                       if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                        neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, z + 1]);

                        if (x == 0)
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, z]);
                        }
                        else
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(rightDataChunk[0, y, z]);
                        }


                        neighborBlocksArePhysics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)x, (int)y, (int)z), neighborBlocksArePhysics);
                        }
                    }
                }
                for (long z = 0; z < CHUNK_SIZE; z += CHUNK_SIZE - 1)
                {
                    for (long x = 1; x < CHUNK_SIZE - 1; ++x)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                       if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        if (z == 0)
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, z + 1]);
                        }
                        else
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(upDataChunk[x, y, 0]);
                        }
                        neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                        neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, z]);
                        neighborBlocksArePhysics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)x, (int)y, (int)z), neighborBlocksArePhysics);
                        }
                    }
                }
                for (long x = 0; x < CHUNK_SIZE; x += CHUNK_SIZE - 1)
                {
                    for (long z = 0; z < CHUNK_SIZE; z += CHUNK_SIZE - 1)
                    {
                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[x, y, z];
                        if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        if (x == 0)
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[1, y, z]);
                        }
                        else
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(rightDataChunk[0, y, z]);
                        }
                        if (z == 0)
                        {

                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, 1]);
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                        }
                        else
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(upDataChunk[x, y, 0]);
                        }

                        neighborBlocksArePhysics[y != 0 ? (int)BlockSides.Down : (int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + blockYSideCheck, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)x, (int)y, (int)z), neighborBlocksArePhysics);
                        }
                    }
                }
            }
        }

        private static void _GenerateChunkSidesSurfaceTool(System.Collections.Generic.List<Vector3> pointsData, ChunkResource chunk,
            ChunkResource chunkLeft, ChunkResource chunkRight, ChunkResource chunkDown, ChunkResource chunkUp)
        {
            bool[] neighborBlocksArePhysics = new bool[6];// 0 Front. 1 Back. 2 Left. 3 Right. 4 Down. 5 Up;
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
                       if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        if (i == 0)
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, 1]);

                        }
                        else
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(upDataChunk[x, y, 0]);
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, i - 1]);
                        }


                        neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, i]);
                        neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[x + 1, y, i]);
                        neighborBlocksArePhysics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[x, y - 1, i]);
                        neighborBlocksArePhysics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + 1, i]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)x, (int)y, (int)i), neighborBlocksArePhysics);
                        }
                    }
                }

                for (long y = 1; y < CHUNK_HEIGHT - 1; ++y)
                {
                    for (long z = 1; z < CHUNK_SIZE - 1; ++z)
                    {

                        ChunkDataGenerator.BlockTypes blockId = mainDataChunk[i, y, z];
                       if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }
                        if (i == 0)
                        {

                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[1, y, z]);
                        }
                        else
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(rightDataChunk[0, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[i - 1, y, z]);
                        }


                        neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[i, y, z - 1]);
                        neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[i, y, z + 1]);
                        neighborBlocksArePhysics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[i, y - 1, z]);
                        neighborBlocksArePhysics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[i, y + 1, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)i, (int)y, (int)z), neighborBlocksArePhysics);
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
                       if (IsBlockNotCollide(blockId))
                        {
                            continue;
                        }

                        if (x == 0)
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(
                                leftDataChunk[CHUNK_SIZE - 1, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(mainDataChunk[1, y, z]);
                        }
                        else
                        {

                            neighborBlocksArePhysics[(int)BlockSides.Left] = IsBlockNotCollide(mainDataChunk[x - 1, y, z]);
                            neighborBlocksArePhysics[(int)BlockSides.Right] = IsBlockNotCollide(rightDataChunk[0, y, z]);
                        }
                        if (z == 0)
                        {

                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(mainDataChunk[x, y, 1]);
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(
                                downDataChunk[x, y, CHUNK_SIZE - 1]);
                        }
                        else
                        {
                            neighborBlocksArePhysics[(int)BlockSides.Front] = IsBlockNotCollide(mainDataChunk[x, y, z - 1]);
                            neighborBlocksArePhysics[(int)BlockSides.Back] = IsBlockNotCollide(upDataChunk[x, y, 0]);
                        }
                        neighborBlocksArePhysics[(int)BlockSides.Down] = IsBlockNotCollide(mainDataChunk[x, y - 1, z]);
                        neighborBlocksArePhysics[(int)BlockSides.Up] = IsBlockNotCollide(mainDataChunk[x, y + 1, z]);

                        if (ChunkBodyGenerator.IsBlockSidesToCreate(neighborBlocksArePhysics))
                        {
                            _BuildBlockCollision(pointsData, new Vector3I((int)x, (int)y, (int)z), neighborBlocksArePhysics);
                        }
                    }
                }
            }

        }

        private static void _BuildBlockCollision(System.Collections.Generic.List<Vector3> pointsData, Vector3I blockSubPosition, bool[] sidesToDraw)
        {
            Vector3[] verts = ChunkBodyGenerator.CalculateBlockVerts(blockSubPosition);

            if (sidesToDraw[(int)BlockSides.Left])
            {
                Vector3[] verts1_ = { verts[2], verts[0], verts[3], verts[1] };
                _SetBlockCollisionTriangle(pointsData, verts1_);
            }
            if (sidesToDraw[(int)BlockSides.Right])
            {
                Vector3[] verts2_ = { verts[7], verts[5], verts[6], verts[4] };
                _SetBlockCollisionTriangle(pointsData, verts2_);
            }
            if (sidesToDraw[(int)BlockSides.Front])
            {
                Vector3[] verts3_ = { verts[6], verts[4], verts[2], verts[0] };
                _SetBlockCollisionTriangle(pointsData, verts3_);
            }
            if (sidesToDraw[(int)BlockSides.Back])
            {
                Vector3[] verts4_ = { verts[3], verts[1], verts[7], verts[5] };
                _SetBlockCollisionTriangle(pointsData, verts4_);
            }
            if (sidesToDraw[(int)BlockSides.Up])
            {
                Vector3[] verts5_ = { verts[2], verts[3], verts[6], verts[7] };
                _SetBlockCollisionTriangle(pointsData, verts5_);
            }
            if (sidesToDraw[(int)BlockSides.Down])
            {
                Vector3[] verts6_ = { verts[4], verts[5], verts[0], verts[1] };
                _SetBlockCollisionTriangle(pointsData, verts6_);
            }

        }

        private static void _SetBlockCollisionTriangle(System.Collections.Generic.List<Vector3> points, Vector3[] verts)
        {
            points.Add(verts[1]);
            points.Add(verts[2]);
            points.Add(verts[3]);

            points.Add(verts[2]);
            points.Add(verts[1]);
            points.Add(verts[0]);
        }

        private static void _BuildBlockCollision(Array<Vector3> pointsData, Vector3I blockSubPosition, Array<bool> sidesToDraw)
        {
            bool[] sides = sidesToDraw.ToArray();
            if (sides.Length != 6)
            {
                throw new OutOfMemoryException("Should be 6 sides not " + Convert.ToString(sides.Length));
            }

            System.Collections.Generic.List<Vector3> points = new(16);
            _BuildBlockCollision(points, blockSubPosition, sides);

            foreach (var item in points)
            {
                pointsData.Add(item);
            }
        }

        private static void _SetBlockCollisionTriangle(Array<Vector3> points, Vector3[] verts)
        {
            System.Collections.Generic.List<Vector3> pointsList = new(6);
            _SetBlockCollisionTriangle(pointsList, verts);
            foreach (var item in pointsList)
            {
                points.Add(item);
            }
        }
        public static bool IsBlockNotCollide(ChunkDataGenerator.BlockTypes blockId)
        {
            return ChunkBodyGenerator.IsBlockNotStatic(blockId) || ((int)blockId > 25 && (int)blockId < 30);
        }
    }
}