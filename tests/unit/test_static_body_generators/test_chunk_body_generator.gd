extends GutTest

var chunk_body_generator
func before_all():
	var ChunksBodyGenerator :CSharpScript= load("res://terrain/chunks_bodies_generation/ChunkBodyGenerator.cs")
	chunk_body_generator = ChunksBodyGenerator.new()
	

func after_all():
	chunk_body_generator.free()


func test_IsBlockSidesToCreate():
	var result1:bool = chunk_body_generator.IsBlockSidesToCreate([false, false, false, false, false, false])
	var result2:bool = chunk_body_generator.IsBlockSidesToCreate([false, true, false, false, false, false])
	var result3:bool = chunk_body_generator.IsBlockSidesToCreate([false, true, false, false, false, true])
	assert_false(result1)
	assert_true(result2)
	assert_true(result3)
	

func test_CalculateBlockVerts():
	var position1 = Vector3(310,12,5235)
	var position2 = Vector3(0,0,0)
	var result1 = chunk_body_generator.CalculateBlockVerts(position1)
	var result2 = chunk_body_generator.CalculateBlockVerts(position2)
	var expected1 :PackedVector3Array= [Vector3(310, 12, 5235), Vector3(310, 12, 5236), Vector3(310, 13, 5235), Vector3(310, 13, 5236), Vector3(311, 12, 5235), Vector3(311, 12, 5236), Vector3(311, 13, 5235), Vector3(311, 13, 5236)]
	var expected2 :PackedVector3Array = [Vector3(0, 0, 0), Vector3(0, 0, 1), Vector3(0, 1, 0), Vector3(0, 1, 1), Vector3(1, 0, 0), Vector3(1, 0, 1), Vector3(1, 1, 0), Vector3(1, 1, 1)]
	  
	assert_eq(result1, expected1)
	assert_eq(result2, expected2)

func test_IsBlockNotStatic():
	var result = chunk_body_generator.IsBlockNotStatic(ChunkBlocks.BlockTypes.Water) and chunk_body_generator.IsBlockNotStatic(ChunkBlocks.BlockTypes.Air)
	assert_true(result)
