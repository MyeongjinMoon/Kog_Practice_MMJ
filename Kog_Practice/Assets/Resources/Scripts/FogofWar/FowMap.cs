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
			mapWidth = heightMap.GetLength(0);	// 가로 타일 개수
			mapHeight = heightMap.GetLength(1);	// 세로 타일 개수

			mapLength = mapWidth * mapHeight;

			visit = new float[mapLength];
			colorBuffer = new Color[mapLength];

			for (int i = 0; i < mapLength; i++)
				visit[i] = FowManager.Instance.fogAlpha.never;

			blurMat = new Material(Shader.Find("FogOfWar/AverageBlur"));
			texBuffer = new Texture2D(
				mapWidth,	// texture 가로크기
				mapHeight,	// texture 세로크기
				TextureFormat.ARGB32,	// texture 포맷 - RGBA 8bit 형식
				false		// mipmap 여부
				);

			texBuffer.wrapMode = TextureWrapMode.Clamp;		// texture wrapMode를 Clamp로 설정 - Clamp : Texture를 가장자리의 마지막 픽셀에 고정

			int width = (int)(mapWidth * 1.5f);
			int height = (int)(mapHeight * 1.5f);

			// >> 임시 렌더링 Texture 할당
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
					// 타일 정보 : 높이, X좌표, Y좌표, 너비
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
			Graphics.Blit(curTexture, lerpBuffer);	// source 텍스처를 dest 텍스처에 셰이더로 복사
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
		/// 시야 내 안개 제거
		/// </summary>
		/// <param name="pos">
		/// 시야를 가진 물체의 위치
		/// </param>
		/// <param name="sightXZ">
		/// 물체의 XZ축 시야 범위
		/// </param>
		/// <param name="sightY">
		/// 물체의 시야 높이
		/// </param>
		public void ComputeFog(TilePos pos, in float sightXZ, in float sightY)
		{
			int sightRangeInt = (int)sightXZ;
			int rangeSquare = sightRangeInt * sightRangeInt;
			tilesInSight.Clear();

			// XZ축 시야 내 타일 찾기
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

			// 위에서 찾은 타일 중 시야 높이 내 타일 찾기
			visibleTileList = GetVisibleTilesInRange(tilesInSight, pos, sightY);

			foreach (FowTile visibleTile in visibleTileList)
			{
				visit[visibleTile.index] = FowManager.Instance.fogAlpha.current;
			}

			//ApplyFogAlpha();
		}

		List<FowTile> visibleTiles = new List<FowTile>();

		/// <summary>
		/// "tilesInSight" 리스트 내에서 볼 수 있는 시야 높이를 가진 타일 찾기
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
				// 1. 타일의 높이가 유닛 가시 높이보다 높은 경우 불가
				if (tile.Height > centerPos.height + sightHeight)
					continue;

				// 2. 유닛과 목표 타일 사이에 유닛 가시보다 높은 타일이 존재할 경우 불가
				if (TileCast(centerPos, tile, sightHeight))
					continue;

				visibleTiles.Add(tile);
			}

			return visibleTiles;
		}

		/// <summary>
		/// 목표 타일 사이에 가시보다 높은 타일이 존재하는지 확인
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="dest"></param>
		/// <param name="sightHeight"></param>
		/// <returns></returns>
		private bool TileCast(in TilePos origin, FowTile dest, in float sightHeight)
		{
			if (origin.x == dest.X && origin.y == dest.Y)	// 현재 위치와 동일한 타일
				return false;

			float exceededHeight = origin.height + sightHeight;		// 볼 수 있는 최대 높이

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

			// 우상향
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
			// 좌상향
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
			// 좌하향
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
			// 우하향
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
		/// 얘가 머테리얼 색상 바꿔주는 애임
		/// </summary>
		public void ApplyFogAlpha()
		{
			foreach(var tile in map)
			{
				int index = tile.index;

				colorBuffer[index].a = visit[index];
			}

			// 여기가 플레이어 주변으로 시야비춰주는 놈인데 코드 분석 필요

			// 밉맵 레벨의 픽셀 색상을 설정
			// 레벨 : 0, 픽셀 색상 : colorBuffer
			texBuffer.SetPixels(colorBuffer);

			// CPU 텍스처에서 변경한 내용을 GPU에 복사
			texBuffer.Apply();

			// 블러 효과를 여러 번 누적하여 더 부드러운 블러를 적용하려는 의도로 보임
			Graphics.Blit(texBuffer, blurBuffer, blurMat, 0);
			Graphics.Blit(blurBuffer, blurBuffer2, blurMat, 0);
			Graphics.Blit(blurBuffer2, blurBuffer, blurMat, 0);

			Graphics.Blit(blurBuffer, nextTexture);
		}
	}
}