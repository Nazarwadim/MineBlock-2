using Godot;
using ProcedureGeneration;


namespace ChunkBodyGeneration
{
	public partial class ChunksBodyGenerator : GodotObject
	{
		//Dont Add or remove elements, this will couse serious error!
		public enum BlockSide
		{
			Front,
			Back,
			Left,
			Right,
			Down,
			Up
		}
		const int CHUNK_LAST_INDEX = ChunkDataGenerator.CHUNK_SIZE - 1;

		public static Vector3[] CalculateBlockVerts(Vector3 blockPosition)
		{
			Vector3[] array = {
			new(blockPosition.X, blockPosition.Y, blockPosition.Z),
			new(blockPosition.X, blockPosition.Y, blockPosition.Z + 1),
			new(blockPosition.X, blockPosition.Y + 1, blockPosition.Z),
			new(blockPosition.X, blockPosition.Y +  1, blockPosition.Z + 1),
			new(blockPosition.X + 1, blockPosition.Y, blockPosition.Z),
			new(blockPosition.X + 1, blockPosition.Y, blockPosition.Z + 1),
			new(blockPosition.X + 1, blockPosition.Y + 1, blockPosition.Z),
			new(blockPosition.X + 1, blockPosition.Y + 1, blockPosition.Z + 1)
		};
			return array;
		}

		public static bool IsBlockSidesToCreate(bool[] sides)
		{
			long counter = 0;
			foreach (bool item in sides)
			{
				if (item)
				{
					++counter;
				}
			}
			if (counter == 0)
			{
				return false;
			}
			return true;
		}

	}
}