extends GutTest


var chunks_mesh_generator

func before_all():
	var ChunksMeshGenerator :CSharpScript= load("res://terrain/chunks_bodies_generation/ChunksMeshGenerator.cs")
	chunks_mesh_generator = ChunksMeshGenerator.new()
	

func after_all():
	chunks_mesh_generator.free()
	
func test_IsBlockTransparent():
	assert_true(chunks_mesh_generator.IsBlockTransparent(ChunkBlocks.BlockTypes.Air), "Or this func is bobo, or you have problems with ChunkBlocks.BlockTypes enum. Check if block types enum is the same as ChunkDataGenerator BlockTypes")
	assert_true(chunks_mesh_generator.IsBlockTransparent(ChunkBlocks.BlockTypes.BirchLeaves), "Or this func is bobo, or you have problems with ChunkBlocks.BlockTypes enum. Check if block types enum is the same as ChunkDataGenerator BlockTypes")
	assert_false(chunks_mesh_generator.IsBlockTransparent(ChunkBlocks.BlockTypes.BookShelf), "Or this func is bobo, or you have problems with ChunkBlocks.BlockTypes enum. Check if block types enum is the same as ChunkDataGenerator BlockTypes")
	
func test_CalculateBlockUvs():
	var blockId1 = ChunkBlocks.BlockTypes.BirchLeaves
	var blockId2 = ChunkBlocks.BlockTypes.Coal
	var blockId3 = ChunkBlocks.BlockTypes.CobbleStone
	var expected1 := PackedVector2Array([Vector2(0.2525, 0.3775), Vector2(0.2525, 0.4975), Vector2(0.3725, 0.3775), Vector2(0.3725, 0.4975)])
	var expected2 := PackedVector2Array([Vector2(0.2525, 0.2525), Vector2(0.2525, 0.3725), Vector2(0.3725, 0.2525), Vector2(0.3725, 0.3725)])
  
	var expected3 := PackedVector2Array([Vector2(0.0025, 0.1275), Vector2(0.0025, 0.2475), Vector2(0.1225, 0.1275), Vector2(0.1225, 0.2475)])  
	
	   
	assert_eq(expected1, chunks_mesh_generator.CalculateBlockUvs(blockId1))
	assert_eq(expected2, chunks_mesh_generator.CalculateBlockUvs(blockId2))
	assert_eq(expected3, chunks_mesh_generator.CalculateBlockUvs(blockId3))

func test_DrawBlockFace():
	var surface_tool_test = SurfaceTool.new()
	surface_tool_test.begin(Mesh.PRIMITIVE_TRIANGLES)
	var verts := PackedVector3Array([Vector3.ZERO,Vector3.FORWARD ,Vector3.BACK,Vector3.ONE])
	var uvs := PackedVector2Array([Vector2.ZERO, Vector2.ONE, Vector2.LEFT, Vector2.RIGHT])
	chunks_mesh_generator.DrawBlockFace(surface_tool_test, verts, uvs)
	
	surface_tool_test.generate_normals()
	surface_tool_test.generate_tangents()
	surface_tool_test.index()
	var result := surface_tool_test.commit_to_arrays()
	var byte_result = var_to_bytes(result)
	
	var expected := PackedByteArray([28, 0, 0, 0, 13, 0, 0, 0, 36, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 
		0, 0, 0, 0, 0, 0, 128, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 0, 128, 63, 
		0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 36, 0, 0, 0, 4, 0,
		 0, 0, 244, 4, 53, 63, 244, 4, 53, 191, 0, 0, 0, 0, 244, 4, 53, 63, 244, 4, 53, 191, 0, 0, 
		0, 0, 244, 4, 53, 63, 244, 4, 53, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 0
		, 0, 0, 16, 0, 0, 0, 243, 4, 53, 63, 243, 4, 53, 63, 0, 0, 0, 0, 0, 0, 128, 63, 243, 4, 53, 63
		, 243, 4, 53, 63, 0, 0, 0, 0, 0, 0, 128, 63, 243, 4, 53, 63, 243, 4, 53, 63, 0, 0, 0, 0, 0, 0
		, 128, 63, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63, 0, 0, 0, 0, 35, 0, 0, 0, 4, 
		0, 0, 0, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 191, 0, 0, 0, 0, 0, 0, 128, 63, 0, 0, 0, 0
		, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
		0, 0, 0, 0, 0, 0, 30, 0, 0, 0, 6, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
		3, 0, 0, 0])
		  
	assert_eq(byte_result, expected)
	
func test_DrawBlockMesh():
	var surface_tool_test = SurfaceTool.new()
	surface_tool_test.begin(Mesh.PRIMITIVE_TRIANGLES)
	var block_sub_position := Vector3i(0,132,51)
	chunks_mesh_generator._DrawBlockMesh(surface_tool_test,block_sub_position, ChunkBlocks.BlockTypes.Log, [true, true,true, true,true,true]);
	surface_tool_test.generate_normals()
	surface_tool_test.index()
	var result :PackedByteArray= var_to_bytes(surface_tool_test.commit_to_arrays()) 
	var expected :PackedByteArray = [28, 0, 0, 0, 13, 0, 0, 0, 36, 0, 0, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 67, 
		0, 0, 76, 66, 0, 0, 0, 0, 0, 0, 5, 67, 0, 0, 80, 66, 0, 0, 0, 0, 0, 0, 4, 67, 0, 0, 80, 66, 0, 0, 0, 0, 0, 0, 
		5, 67, 0, 0, 76, 66, 0, 0, 128, 63, 0, 0, 4, 67, 0, 0, 80, 66, 0, 0, 128, 63, 0, 0, 5, 67, 0, 0, 76, 66, 0, 0, 
		128, 63, 0, 0, 4, 67, 0, 0, 76, 66, 0, 0, 128, 63, 0, 0, 5, 67, 0, 0, 80, 66, 0, 0, 128, 63, 0, 0, 4, 67, 0, 0, 
		76, 66, 0, 0, 0, 0, 0, 0, 5, 67, 0, 0, 76, 66, 0, 0, 0, 0, 0, 0, 4, 67, 0, 0, 76, 66, 0, 0, 128, 63, 0, 0, 5, 67, 
		0, 0, 76, 66, 0, 0, 0, 0, 0, 0, 4, 67, 0, 0, 80, 66, 0, 0, 128, 63, 0, 0, 5, 67, 0, 0, 80, 66, 0, 0, 128, 63, 0, 
		0, 4, 67, 0, 0, 80, 66, 0, 0, 0, 0, 0, 0, 5, 67, 0, 0, 80, 66, 0, 0, 0, 0, 0, 0, 5, 67, 0, 0, 80, 66, 0, 0, 128, 
		63, 0, 0, 5, 67, 0, 0, 76, 66, 0, 0, 128, 63, 0, 0, 5, 67, 0, 0, 80, 66, 0, 0, 0, 0, 0, 0, 5, 67, 0, 0, 76, 66, 0, 
		0, 128, 63, 0, 0, 4, 67, 0, 0, 80, 66, 0, 0, 0, 0, 0, 0, 4, 67, 0, 0, 76, 66, 0, 0, 0, 0, 0, 0, 4, 67, 0, 0, 80, 66, 
		0, 0, 128, 63, 0, 0, 4, 67, 0, 0, 76, 66, 36, 0, 0, 0, 24, 0, 0, 0, 171, 170, 42, 191, 171, 170, 42, 191, 171, 170, 
		170, 190, 171, 170, 42, 191, 171, 170, 42, 63, 171, 170, 170, 62, 235, 5, 209, 190, 235, 5, 209, 190, 235, 5, 81, 63, 
		235, 5, 209, 190, 235, 5, 209, 62, 235, 5, 81, 191, 171, 170, 42, 63, 171, 170, 42, 191, 171, 170, 170, 62, 171, 170, 
		42, 63, 171, 170, 42, 63, 171, 170, 170, 190, 235, 5, 209, 62, 235, 5, 209, 190, 235, 5, 81, 191, 235, 5, 209, 62, 235,
		5, 209, 62, 235, 5, 81, 63, 235, 5, 209, 62, 235, 5, 209, 190, 235, 5, 81, 191, 235, 5, 209, 190, 235, 5, 209, 62, 235, 
		5, 81, 191, 171, 170, 42, 191, 171, 170, 42, 191, 171, 170, 170, 190, 171, 170, 42, 63, 171, 170, 42, 63, 171, 170, 170, 
		190, 235, 5, 209, 190, 235, 5, 209, 190, 235, 5, 81, 63, 235, 5, 209, 62, 235, 5, 209, 62, 235, 5, 81, 63, 171, 170, 42, 
		63, 171, 170, 42, 191, 171, 170, 170, 62, 171, 170, 42, 191, 171, 170, 42, 63, 171, 170, 170, 62, 171, 170, 42, 191, 171, 
		170, 42, 63, 171, 170, 170, 62, 171, 170, 42, 63, 171, 170, 42, 63, 171, 170, 170, 190, 235, 5, 209, 62, 235, 5, 209, 62, 
		235, 5, 81, 63, 235, 5, 209, 190, 235, 5, 209, 62, 235, 5, 81, 191, 171, 170, 42, 63, 171, 170, 42, 191, 171, 170, 170, 
		62, 171, 170, 42, 191, 171, 170, 42, 191, 171, 170, 170, 190, 235, 5, 209, 190, 235, 5, 209, 190, 235, 5, 81, 63, 235, 5, 
		209, 62, 235, 5, 209, 190, 235, 5, 81, 191, 0, 0, 0, 0, 0, 0, 0, 0, 35, 0, 0, 0, 24, 0, 0, 0, 215, 163, 0, 63, 164, 112, 
		125, 62, 41, 92, 31, 63, 92, 143, 2, 62, 41, 92, 31, 63, 164, 112, 125, 62, 215, 163, 0, 63, 92, 143, 2, 62, 215, 163, 0, 
		63, 164, 112, 125, 62, 41, 92, 31, 63, 92, 143, 2, 62, 41, 92, 31, 63, 164, 112, 125, 62, 215, 163, 0, 63, 92, 143, 2, 62, 
		215, 163, 0, 63, 164, 112, 125, 62, 41, 92, 31, 63, 92, 143, 2, 62, 41, 92, 31, 63, 164, 112, 125, 62, 215, 163, 0, 63, 92, 
		143, 2, 62, 215, 163, 0, 63, 164, 112, 125, 62, 41, 92, 31, 63, 92, 143, 2, 62, 41, 92, 31, 63, 164, 112, 125, 62, 215, 163, 
		0, 63, 92, 143, 2, 62, 215, 163, 64, 63, 82, 184, 254, 62, 41, 92, 95, 63, 174, 71, 193, 62, 41, 92, 95, 63, 82, 184, 254, 62, 
		215, 163, 64, 63, 174, 71, 193, 62, 215, 163, 64, 63, 82, 184, 254, 62, 41, 92, 95, 63, 174, 71, 193, 62, 41, 92, 95, 63, 82, 
		184, 254, 62, 215, 163, 64, 63, 174, 71, 193, 62, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
		0, 0, 0, 30, 0, 0, 0, 36, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 4, 0, 0, 0, 5, 0,
	 	0, 0, 6, 0, 0, 0, 5, 0, 0, 0, 4, 0, 0, 0, 7, 0, 0, 0, 8, 0, 0, 0, 9, 0, 0, 0, 10, 0, 0, 0, 9, 0, 0, 0, 8, 0, 0, 0, 11, 0, 
		0, 0, 12, 0, 0, 0, 13, 0, 0, 0, 14, 0, 0, 0, 13, 0, 0, 0, 12, 0, 0, 0, 15, 0, 0, 0, 16, 0, 0, 0, 17, 0, 0, 0, 18, 0, 0, 0, 17, 
		0, 0, 0, 16, 0, 0, 0, 19, 0, 0, 0, 20, 0, 0, 0, 21, 0, 0, 0, 22, 0, 0, 0, 21, 0, 0, 0, 20, 0, 0, 0, 23, 0, 0, 0]
	assert_eq(result, expected)

func test_GenerateChunkMesh():
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
	var chunkres:ChunkResource = ChunkResource.new()
	var chunk_resources := {midle_chunk.Position : midle_chunk,
						up_chunk.Position : up_chunk,
						down_chunk.Position : down_chunk,
						right_chunk.Position : right_chunk,
						left_chunk.Position : left_chunk}
						 
	var array_mesh :ArrayMesh = chunks_mesh_generator.GenerateChunkMesh(midle_chunk, chunk_resources)
	var result = array_mesh.surface_get_arrays(0)
	var mesh_expected :ArrayMesh= ResourceLoader.load("res://tests/mocks/test_mesh.res")
	var expected = mesh_expected.surface_get_arrays(0)
	assert_eq(result, expected)
	
	
