using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Myeongjin
{
	public class FowMap
	{
		private List<FowTile> map = new List<FowTile>();
		private int mapWidth;
		private int mapHeight;
		private int mapLength;

		private float[] visit;

		private Color[] colorBuffer;
		private Material blurMat;

		private Texture2D texBuffer;
		private RenderTexture blurBuffer;
		private RenderTexture blurBuffer2;

		private RenderTexture curTexture;
		private RenderTexture lerpBuffer;
		private RenderTexture nextTexture;

		public Texture FogTexture => curTexture;

		public void InitMap(float[,] heightMap)
		{
			map.Clear();
			mapWidth = heightMap.GetLength(0);	// ���� Ÿ�� ����
			mapHeight = heightMap.GetLength(1);	// ���� Ÿ�� ����

			mapLength = mapWidth * mapHeight;

			visit = new float[mapLength];
			colorBuffer = new Color[mapLength];

			for (int i = 0; i < mapLength; i++)
				visit[i] = FowManager.Instance.fogAlpha.never;

			blurMat = new Material(Shader.Find("FogOfWar/AverageBlur"));
			texBuffer = new Texture2D(
				mapWidth,	// texture ����ũ��
				mapHeight,	// texture ����ũ��
				TextureFormat.ARGB32,	// texture ���� - RGBA 8bit ����
				false		// mipmap ����
				);

			texBuffer.wrapMode = TextureWrapMode.Clamp;		// texture wrapMode�� Clamp�� ���� - Clamp : Texture�� �����ڸ��� ������ �ȼ��� ����

			int width = (int)(mapWidth * 1.5f);
			int height = (int)(mapHeight * 1.5f);

			// >> �ӽ� ������ Texture �Ҵ�
			blurBuffer = RenderTexture.GetTemporary(width, height, 0);
			blurBuffer2 = RenderTexture.GetTemporary(width, height, 0);

			curTexture = RenderTexture.GetTemporary(width, height, 0);
			lerpBuffer = RenderTexture.GetTemporary(width, height, 0);
			nextTexture = RenderTexture.GetTemporary(width, height, 0);
			// <<

			for (int j = 0; j < mapHeight; j++)
			{
				for (int i = 0; i < mapWidth; i++)
				{
					// Ÿ�� ���� : ����, X��ǥ, Y��ǥ, �ʺ�
					map.Add(new FowTile(heightMap[i, j], i, j, mapWidth));
				}
			}
		}
		public void Release()
		{
			RenderTexture.ReleaseTemporary(blurBuffer);
			RenderTexture.ReleaseTemporary(blurBuffer2);
			RenderTexture.ReleaseTemporary(curTexture);
			RenderTexture.ReleaseTemporary(lerpBuffer);
			RenderTexture.ReleaseTemporary(nextTexture);
		}
		private FowTile GetTile(in int x, in int y)
		{
			if (InMapRange(x, y))
			{
				return map[GetTileIndex(x, y)];
			}
			else
			{
				return null;
			}
		}
		public bool InMapRange(in int x, in int y)
		{
			return x >= 0 && y >= 0 &&
				x < mapWidth && y < mapHeight;
		}
		public int GetTileIndex(in int x, in int y)
		{
			return x + y * mapWidth;
		}
		public void LerpBlur()
		{
			Graphics.Blit(curTexture, lerpBuffer);	// source �ؽ�ó�� dest �ؽ�ó�� ���̴��� ����
			blurMat.SetTexture("_LastTex", lerpBuffer);

			Graphics.Blit(nextTexture, curTexture, blurMat, 1);
		}
		public void RefreshFog()
		{
			for (int i = 0; i < mapLength; i++)
			{
				if (visit[i] == FowManager.Instance.fogAlpha.current)
					visit[i] = FowManager.Instance.fogAlpha.visited;
			}
		}
		List<FowTile> visibleTileList = new List<FowTile>();
		List<FowTile> tilesInSight = new List<FowTile>();

		/// <summary>
		/// �þ� �� �Ȱ� ����
		/// </summary>
		/// <param name="pos">
		/// �þ߸� ���� ��ü�� ��ġ
		/// </param>
		/// <param name="sightXZ">
		/// ��ü�� XZ�� �þ� ����
		/// </param>
		/// <param name="sightY">
		/// ��ü�� �þ� ����
		/// </param>
		public void ComputeFog(TilePos pos, in float sightXZ, in float sightY)
		{
			int sightRangeInt = (int)sightXZ;
			int rangeSquare = sightRangeInt * sightRangeInt;
			tilesInSight.Clear();

			// XZ�� �þ� �� Ÿ�� ã��
			for (int i = -sightRangeInt; i <= sightRangeInt; i++)
			{
				for (int j = -sightRangeInt; j <= sightRangeInt; j++)
				{
					if (i * i + j * j <= rangeSquare)
					{
						var tile = GetTile(pos.x + i, pos.y + j);
						if (tile != null)
						{
							tilesInSight.Add(tile);
						}
					}
				}
			}

			// ������ ã�� Ÿ�� �� �þ� ���� �� Ÿ�� ã��
			visibleTileList = GetVisibleTilesInRange(tilesInSight, pos, sightY);

			foreach (FowTile visibleTile in visibleTileList)
			{
				visit[visibleTile.index] = FowManager.Instance.fogAlpha.current;
			}

			//ApplyFogAlpha();
		}

		List<FowTile> visibleTiles = new List<FowTile>();

		/// <summary>
		/// "tilesInSight" ����Ʈ ������ �� �� �ִ� �þ� ���̸� ���� Ÿ�� ã��
		/// </summary>
		/// <param name="tilesInSight"></param>
		/// <param name="centerPos"></param>
		/// <param name="sightHeight"></param>
		/// <returns></returns>
		private List<FowTile> GetVisibleTilesInRange(List<FowTile> tilesInSight, in TilePos centerPos, in float sightHeight)
		{
			visibleTileList.Clear();

			foreach (FowTile tile in tilesInSight)
			{
				// 1. Ÿ���� ���̰� ���� ���� ���̺��� ���� ��� �Ұ�
				if (tile.Height > centerPos.height + sightHeight)
					continue;

				// 2. ���ְ� ��ǥ Ÿ�� ���̿� ���� ���ú��� ���� Ÿ���� ������ ��� �Ұ�
				if (TileCast(centerPos, tile, sightHeight))
					continue;

				visibleTiles.Add(tile);
			}

			return visibleTiles;
		}

		/// <summary>
		/// ��ǥ Ÿ�� ���̿� ���ú��� ���� Ÿ���� �����ϴ��� Ȯ��
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="dest"></param>
		/// <param name="sightHeight"></param>
		/// <returns></returns>
		private bool TileCast(in TilePos origin, FowTile dest, in float sightHeight)
		{
			if (origin.x == dest.X && origin.y == dest.Y)	// ���� ��ġ�� ������ Ÿ��
				return false;

			float exceededHeight = origin.height + sightHeight;		// �� �� �ִ� �ִ� ����

			int destX = dest.X;
			int destY = dest.Y;
			int xLen = destX - origin.x;
			int yLen = destY - origin.y;

			int xSign = System.Math.Sign(xLen);
			int ySign = System.Math.Sign(yLen);

			xLen = System.Math.Abs(xLen);
			yLen = System.Math.Abs(yLen);

			int x = origin.x;
			int y = origin.y;

			if (yLen == 0)
			{
				if (xSign > 0)
				{
					for (; x <= destX; x++)
					{
						if (isBlocked(x, y))
							return true;
					}
				}
				else
				{
					for (; x >= destX; x--)
					{
						if (isBlocked(x, y))
							return true;
					}
				}
			}
			if (xLen == 0)
			{
				if (ySign > 0)
				{
					for (; y <= destY; y++)
					{
						if (isBlocked(x, y))
							return true;
					}
				}
				else
				{
					for (; y >= destY; y--)
					{
						if (isBlocked(x, y))
							return true;
					}
				}
			}

			float xyRatio = (float)xLen / yLen;
			float yxRatio = (float)yLen / xLen;
			int xMove = 0;
			int yMove = 0;

			// �����
			if (xSign > 0 && ySign > 0)
			{

				if (xyRatio > yxRatio)
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
						else yMove++;

						if (isBlocked(x + xMove, y + yMove))
							return true;
					}
				}
				else
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
						else xMove++;

						if (isBlocked(x + xMove, y + yMove))
							return true;
					}
				}
			}
			// �»���
			if (xSign < 0 && ySign > 0)
			{
				if (xyRatio > yxRatio)
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
						else yMove++;

						if (isBlocked(x - xMove, y + yMove))
							return true;
					}
				}
				else
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
						else xMove++;

						if (isBlocked(x - xMove, y + yMove))
							return true;
					}
				}
			}
			// ������
			if (xSign < 0 && ySign < 0)
			{
				if (xyRatio > yxRatio)
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
						else yMove++;

						if (isBlocked(x - xMove, y - yMove))
							return true;
					}
				}
				else
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
						else xMove++;

						if (isBlocked(x - xMove, y - yMove))
							return true;
					}
				}
			}
			// ������
			if (xSign > 0 && ySign < 0)
			{
				if (xyRatio > yxRatio)
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)xMove / (yMove + 1) < xyRatio) xMove++;
						else yMove++;

						if (isBlocked(x + xMove, y - yMove))
							return true;
					}
				}
				else
				{
					while (xMove < xLen && yMove < yLen)
					{
						if ((float)yMove / (xMove + 1) < yxRatio) yMove++;
						else xMove++;

						if (isBlocked(x + xMove, y - yMove))
							return true;
					}
				}
			}

			return false;

			bool isBlocked(int a, int b)
			{
				int index = GetTileIndex(a, b);
				if (index > map.Count || index < 0) return true;
				return map[index].Height > exceededHeight;
			}
		}
		/// <summary>
		/// �갡 ���׸��� ���� �ٲ��ִ� ����
		/// </summary>
		public void ApplyFogAlpha()
		{
			foreach(var tile in map)
			{
				int index = tile.index;

				colorBuffer[index].a = visit[index];
			}

			// ���Ⱑ �÷��̾� �ֺ����� �þߺ����ִ� ���ε� �ڵ� �м� �ʿ�

			// �Ӹ� ������ �ȼ� ������ ����
			// ���� : 0, �ȼ� ���� : colorBuffer
			texBuffer.SetPixels(colorBuffer);

			// CPU �ؽ�ó���� ������ ������ GPU�� ����
			texBuffer.Apply();

			// �� ȿ���� ���� �� �����Ͽ� �� �ε巯�� ���� �����Ϸ��� �ǵ��� ����
			Graphics.Blit(texBuffer, blurBuffer, blurMat, 0);
			Graphics.Blit(blurBuffer, blurBuffer2, blurMat, 0);
			Graphics.Blit(blurBuffer2, blurBuffer, blurMat, 0);

			Graphics.Blit(blurBuffer, nextTexture);
		}
	}
}