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
	var compreased_result = byte_result.compress(FileAccess.COMPRESSION_GZIP)
	var expected :PackedByteArray = [31, 139, 8, 0, 0, 0, 0, 0, 0, 3, 147, 97, 96, 96, 224, 5, 98, 21, 32, 102, 97, 64, 6, 13, 251, 145, 216, 
		246, 168, 24, 1, 96, 250, 190, 176, 152, 218, 3, 49, 88, 15, 46, 54, 50, 80, 0, 98, 1, 32, 254, 12, 148, 7, 97, 152, 61, 132, 248, 168, 246, 67, 216, 202, 
		112, 183, 195, 221, 184, 31, 89, 158, 24, 32, 7, 196, 108, 80, 54, 35, 16, 51, 65, 105, 16, 96, 6, 98, 0, 5, 19, 194, 252, 40, 1, 0, 0]
		 
		  
	assert_eq(compreased_result, expected)
	
func test_DrawBlockMesh():
	var surface_tool_test = SurfaceTool.new()
	surface_tool_test.begin(Mesh.PRIMITIVE_TRIANGLES)
	var block_sub_position := Vector3i(0,132,51)
	chunks_mesh_generator._DrawBlockMesh(surface_tool_test,block_sub_position, ChunkBlocks.BlockTypes.LogUp, [true, true,true, true,true,true]);
	surface_tool_test.generate_normals()
	surface_tool_test.index()
	var result :PackedByteArray= var_to_bytes(surface_tool_test.commit_to_arrays()) 
	var compresed_result = result.compress(FileAccess.COMPRESSION_GZIP)
	var expected :PackedByteArray = [31, 139, 8, 0, 0, 0, 0, 0, 0, 3, 147, 97, 96, 96, 224, 5, 98, 21, 32, 150, 96, 128, 1, 22, 103, 6, 6, 31, 39, 8, 155, 21, 200, 14, 112, 66, 
		136, 7, 32, 137, 131, 212, 52, 216, 35, 196, 65, 108, 116, 113, 31, 36, 241, 0, 52, 113, 
		100, 115, 144, 237, 69, 54, 7, 217, 94, 108, 230, 4, 96, 113, 39, 43, 14, 247, 160, 171, 65, 119, 63, 186, 223, 89, 48, 220, 12, 11, 167, 213, 171, 180, 246, 67, 240, 170, 125, 
		80, 182, 61, 144, 109, 247, 154, 245, 226, 62, 8, 14, 180, 135, 178, 129, 98, 129, 80, 121, 176, 122, 59, 40, 27, 164, 30, 42, 15, 86, 191, 31, 202, 182, 131, 234, 69, 22, 71, 
		54, 7, 217, 94, 100, 115, 144, 237, 133, 155, 131, 102, 47, 220, 157, 104, 108, 116, 247, 216, 17, 112, 63, 220, 13, 88, 236, 5, 187, 25, 150, 146, 148, 161, 225, 117, 125, 49, 131, 253, 146, 
		130, 90, 59, 205, 24, 121, 251, 152, 126, 38, 48, 13, 226, 131, 196, 65, 124, 218, 203, 59, 216, 7, 237, 248, 7, 20, 143, 183, 95, 231, 126, 16, 76, 131, 248, 32, 113, 16, 159, 144, 
		60, 3, 30, 32, 7, 205, 63, 32, 192, 8, 196, 76, 80, 26, 4, 152, 65, 169, 8, 148, 218, 128, 152, 13, 74, 131, 248, 236, 64, 204, 1, 196, 156, 64, 204, 5, 165, 65, 124, 110, 32, 230, 
		129, 230, 73, 62, 40, 13, 226, 243, 3, 177, 0, 16, 11, 2, 177, 16, 148, 6, 241, 133, 129, 88, 4, 136, 69, 129, 88, 12, 74, 131, 248, 226, 64, 12, 0, 184, 144, 101, 153, 220, 3, 0, 0]
		 
	assert_eq(compresed_result, expected)

func test_GenerateChunkMesh():
	var midle_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(0, 0).res")
	var up_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(0, 1).res")
	var down_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(0, -1).res")
	var right_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(1, 0).res")
	var left_chunk :ChunkResource= ResourceLoader.load("res://tests/mocks/chunk_resources/Vector2i(-1, 0).res")
	midle_chunk.CopyFromOneDimensionalIntoThreeDimensional()
	up_chunk.CopyFromOneDimensionalIntoThreeDimensional()
	down_chunk.CopyFromOneDimensionalIntoThreeDimensional()
	right_chunk.CopyFromOneDimensionalIntoThreeDimensional()
	left_chunk.CopyFromOneDimensionalIntoThreeDimensional()
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
