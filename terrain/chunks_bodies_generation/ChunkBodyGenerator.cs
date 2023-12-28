using System;
using System.Linq;
using System.Security.Cryptography;
using Godot;
using Godot.Collections;
using ProcedureGeneration;


namespace ChunkBodyGeneration
{
	public partial class ChunkBodyGenerator : GodotObject
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
		public static bool IsBlockSidesToCreate(Array<bool> sides)
		{
			if(sides.Count != 6)
			{
				throw new OutOfMemoryException("Should be 6 sides not " + Convert.ToString(sides.Count));
			}
			bool[] ar = sides.ToArray();
			return IsBlockSidesToCreate(ar);
		}
	}
}