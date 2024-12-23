using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Myeongjin
{
	public struct TilePos
	{
		public int x;
		public int y;
		public float height;

		public TilePos(int x, int y, float height)
		{
			this.x = x;
			this.y = y;
			this.height = height;
		}
		public int Distance(in TilePos other)
		{
			int distX = other.x - x;
			int distY = other.y - y;
			return distX * distX + distY * distY;
		}
		public float NDot(in TilePos A, in TilePos B)
		{
			Vector2 nA = new Vector2(A.x - x, A.y - y).normalized;
			Vector2 nB = new Vector2(B.x - x, B.y - y).normalized;
			return Vector2.Dot(nA, nB);
		}
		public bool Equals(in TilePos obj)
		{
			return obj.x == x && obj.y == y;
		}
		public int GetTileIndex(in int mapWidth)
		{
			return x + y * mapWidth;
		}
	}
}