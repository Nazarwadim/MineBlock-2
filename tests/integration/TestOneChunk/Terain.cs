using ChunkBodyGeneration;
using Godot;
using ProcedureGeneration;
using System;

public partial class Terain : Node
{
    public override void _Ready()
    {
        VoxelWorld voxelWorld = new();


        Vector2I chunkPosition = new(0, 0);
        byte[,,] chunkData = ChunkDataGenerator.GetChunkWithTerrain(chunkPosition);
        ChunkResource chunkResource = new ChunkResource(chunkData, chunkPosition);
        voxelWorld.ChunksResources.Add(chunkPosition, chunkResource);
        ulong startMesh = Time.GetTicksUsec();
        Mesh mesh = ChunksMeshGenerator.GenerateChunkMesh(chunkResource, voxelWorld);
        GD.Print("Mesh time generation = ", Time.GetTicksUsec() - startMesh);
        ulong startShape = Time.GetTicksUsec();
        ConcavePolygonShape3D shape = ChunksShapeGenerator.GenerateChunkShape(chunkResource, voxelWorld);
        GD.Print("Shape time generation = ", Time.GetTicksUsec() - startShape);
        ChunkStaticBody chunkBody = new ChunkStaticBody(
            mesh,
            shape,
            new Vector3(chunkPosition.X * ChunkDataGenerator.CHUNK_SIZE, 0, chunkPosition.Y * ChunkDataGenerator.CHUNK_SIZE)
        );
        AddChild(chunkBody);
    }
}
