using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Myeongjin
{
	public class FowTile
	{
		public TilePos pos;
		public int index;
		public int X => pos.x;
		public int Y => pos.y;
		public float Height => pos.height;

		public FowTile(float height, int x, int y, int width)
		{
			pos.x = x;
			pos.y = y;
			pos.height = height;

			index = x + y * width;
		}
		public int Distance(FowTile other)
		{
			int distX = other.X - X;
			int distY = other.Y - Y;
			return distX * distX + distY * distY;
		}
	}
}