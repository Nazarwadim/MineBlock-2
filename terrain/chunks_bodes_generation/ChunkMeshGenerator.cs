using System;
using Godot;
using Godot.Collections;


namespace ChunkBodyGeneration{
    public static class ChunksMeshGenerator
    {
        //Summary:
        //  This method creates mesh by algoritm from 2d arrays of blocks. 
        //    ||
        //  ||\\||
        //    ||
        // || - not main chunk data
        // \\ - main chunk data
        public static MeshInstance3D GenerateChunkMesh(Block[, ,] mainData, Block[, ,] leftData, Block[, ,] rightData, Block[, ,] upData, Block[, ,] downData)
        {
            
            if(mainData.Length == 0 || leftData.Length == 0 || rightData.Length == 0 || upData.Length == 0 || downData.Length == 0)
            {
                throw new Exception("No elements in arrays ChunkCollision.cs 21");
            }
            SurfaceTool surfaceTool = new SurfaceTool();
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            for(int i = 0; i < mainData.GetLength(0);i++)
            {
                for(int j = 0; j < mainData.GetLength(1);j++)
                {
                    for(int k = 0; k < mainData.GetLength(2);k++)
                    {
                        Block.Type blockId = mainData[i,j,k].type;
                        if (blockId == Block.Type.Air)
                        {
                            continue;
                        }
                        if (i == 0 || i == ChunkDataGenerator.CHUNK_SIZE-1 || k == 0 || k == ChunkDataGenerator.CHUNK_SIZE-1)
                        {
                            
					        _DrawBlockMesh(surfaceTool, new Vector3I(i,j,k), blockId);
                        }
                    }
                }
            }
            return null;
        }
        private static void _DrawBlockMesh(SurfaceTool surfaceTool, Vector3I blockSubPosition, Block.Type blockId)
        {
            Vector3[] verts = ChunksBodyGenerator.CalculateBlockVerts(blockSubPosition);
            Vector2[] uvs = ChunksBodyGenerator.CalculateBlockUvs(blockId);
            Array<Vector2> top_uvs = new Array<Vector2>(uvs);
            Array<Vector2> bottom_uvs = new Array<Vector2>(uvs);


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
            

		
        }

        private static void _DrawBlockFace(SurfaceTool surfaceTool, Vector3[] verts, Vector2[] uvs)
        {
            surfaceTool.SetUV(uvs[1]); surfaceTool.AddVertex(verts[1]);
            surfaceTool.SetUV(uvs[2]); surfaceTool.AddVertex(verts[2]);
            surfaceTool.SetUV(uvs[3]); surfaceTool.AddVertex(verts[3]);

            surfaceTool.SetUV(uvs[2]); surfaceTool.AddVertex(verts[2]);
            surfaceTool.SetUV(uvs[1]); surfaceTool.AddVertex(verts[1]);
            surfaceTool.SetUV(uvs[0]); surfaceTool.AddVertex(verts[8]);
        }
    }
}