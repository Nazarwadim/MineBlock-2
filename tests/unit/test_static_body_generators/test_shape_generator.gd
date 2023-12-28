extends GutTest

var chunks_shape_generator

func before_all():
	var ChunkShapeGenerator :CSharpScript= load("res://terrain/chunks_bodies_generation/ChunkShapeGenerator.cs")
	chunks_shape_generator = ChunkShapeGenerator.new()
	

func after_all():
	chunks_shape_generator.free()

func test_IsBlockNotCollide():
	var result1 = chunks_shape_generator.IsBlockNotCollide(ChunkBlocks.BlockTypes.Air)
	var result2 = chunks_shape_generator.IsBlockNotCollide(ChunkBlocks.BlockTypes.Dirt)
	assert_true(result1)
	assert_false(result2)

func test_SetBlockColisionTriangle():
	var arr:Array[Vector3] = []
	var verts :PackedVector3Array= [Vector3.ZERO, Vector3.LEFT, Vector3.RIGHT, Vector3.UP]
	chunks_shape_generator._SetBlockColisionTriangle(arr,verts)
	var expected:Array = [Vector3(-1, 0, 0), Vector3(1, 0, 0), Vector3(0, 1, 0), Vector3(1, 0, 0), Vector3(-1, 0, 0), Vector3(0, 0, 0)]
	assert_eq(arr, expected)
	
func test_BuildBlockColision():
	var arr:Array[Vector3] = []
	var position := Vector3(12,51,89)
	chunks_shape_generator._BuildBlockColision(arr,position,[false,true,true,false,false,true])
	var expected:Array = [Vector3(12, 51, 89), Vector3(12, 52, 90), Vector3(12, 51, 90),
		Vector3(12, 52, 90), Vector3(12, 51, 89), Vector3(12, 52, 89), Vector3(12, 51, 90), 
		Vector3(13, 52, 90), Vector3(13, 51, 90), Vector3(13, 52, 90), Vector3(12, 51, 90), 
		Vector3(12, 52, 90), Vector3(12, 52, 90), Vector3(13, 52, 89), Vector3(13, 52, 90), 
		Vector3(13, 52, 89), Vector3(12, 52, 90), Vector3(12, 52, 89)]
	assert_eq(arr, expected)
	
func test_GenerateChunkShape():
	var midle_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(0, 0).res")
	var up_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(0, 1).res")
	var down_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(0, -1).res")
	var right_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(1, 0).res")
	var left_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(-1, 0).res")
	midle_chunk.CopyFromOneDimentionalIntoThreeDimentional()
	up_chunk.CopyFromOneDimentionalIntoThreeDimentional()
	down_chunk.CopyFromOneDimentionalIntoThreeDimentional()
	right_chunk.CopyFromOneDimentionalIntoThreeDimentional()
	left_chunk.CopyFromOneDimentionalIntoThreeDimentional()
	var chunk_resources := {midle_chunk.Position : midle_chunk,
						up_chunk.Position : up_chunk,
						down_chunk.Position : down_chunk,
						right_chunk.Position : right_chunk,
						left_chunk.Position : left_chunk}
	var shape :ConcavePolygonShape3D = chunks_shape_generator.GenerateChunkShape(midle_chunk, chunk_resources)
	var expect :ConcavePolygonShape3D = ResourceLoader.load("res://tests/mocks/test_shape.res")
	assert_eq(shape.data, expect.data)
