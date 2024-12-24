using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Myeongjin
{
    [DefaultExecutionOrder(-100)]
    public class FowManager : MonoBehaviour
	{
		private static FowManager f_instance;
		private const string FOWMANAGERNAME = "_FowManager";

		public static FowManager Instance
		{
			get
			{
				if (f_instance == null)
					CheckExsistance();

                return f_instance;
			}
		}
		private static void CheckExsistance()
		{
            f_instance = FindObjectOfType<FowManager>();

			if (f_instance == null)
			{
				GameObject container = new GameObject("FowManager Singleton Container");

				f_instance = container.AddComponent<FowManager>();
			}
        }

		private Material _fogMaterial;
		public GameObject _rendererPrefab;

		private void InitFogTexture()
		{
			var renderer = Instantiate(_rendererPrefab, transform);
			renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localScale = new Vector3(_fogWidthX * 0.5f, 1, _fogWidthZ * 0.5f);
            _fogMaterial = renderer.GetComponentInChildren<Renderer>().material;
        }
        void UpdateFogTexture()
        {
            if (Map.FogTexture != null)
            {
                _fogMaterial.SetTexture("_MainTex", Map.FogTexture);
            }
        }

        public LayerMask _groundLayer;
		public float _fogWidthX = 40;	// fogX 크기
		public float _fogWidthZ = 40;	// fogY 크기
		public float _tileSize = 1;
		public float _updateCycle = 0.5f;

        [System.Serializable]
        public class FogAlpha
		{
			[Range(0, 1)] public float current = 0.0f;
			[Range(0, 1)] public float visited = 0.8f;
			[Range(0, 1)] public float never = 1.0f;
		}
		public FogAlpha fogAlpha = new FogAlpha();

		public bool showGizmos = true;

		public FowMap Map { get; private set; }

		private float[,] HeightMap { get; set; }
		private List<FowUnit> UnitList { get; set; }

        private void Awake()
        {
			UnitList = new List<FowUnit>();
			InitMap();
			InitFogTexture();

            Debug.Log(Application.targetFrameRate);
        }
        private void OnEnable()
        {
			StartCoroutine(UpdateFogRoutine());
        }
        private void Update()
        {
			Map.LerpBlur();
			UpdateFogTexture();
        }
        private void OnDestroy()
        {
			Map.Release();
        }
		public static void AddUnit(FowUnit unit)
		{
			if (!f_instance.UnitList.Contains(unit))
			{
				f_instance.UnitList.Add(unit);
			}
		}
		public static void RemoveUnit(FowUnit viewer)
		{
			if (f_instance.UnitList.Contains(viewer))
			{
				f_instance.UnitList.Remove(viewer);
			}
		}
		public void InitMap()
		{
			HeightMap = new float[(int)(_fogWidthX / _tileSize), (int)(_fogWidthZ / _tileSize)];
			for (int i = 0; i < HeightMap.GetLength(0); i++)
			{
				for (int j = 0; j < HeightMap.GetLength(1); j++)
				{
					var tileCenter = GetTileCenterPoint(i, j);

					Vector3 ro = new Vector3(tileCenter.x, 100f, tileCenter.y);
					Vector3 rd = Vector3.down;

					float height = 0f;

					if (Physics.Raycast(ro, rd, out var hit, 200f, _groundLayer))
					{
						height = hit.point.y;
					}

					HeightMap[i, j] = height;
				}
			}

			Map = new FowMap();
			Map.InitMap(HeightMap);
		}
		private TilePos GetTilePos(FowUnit unit)
		{
            int x = (int)((unit.transform.position.x - transform.position.x + _fogWidthX * 0.5f) / _tileSize);
            int y = (int)((unit.transform.position.z - transform.position.z + _fogWidthZ * 0.5f) / _tileSize);
            float height = unit.transform.position.y;

            return new TilePos(x, y, height);
        }
		private Vector2 GetTileCenterPoint(in int x, in int y)
		{
            return new Vector2(
                x * _tileSize + _tileSize * 0.5f - _fogWidthX * 0.5f,
                y * _tileSize + _tileSize * 0.5f - _fogWidthZ * 0.5f
            );
        }
		public IEnumerator UpdateFogRoutine()
		{
			while (true)
			{
				if (Map != null)
				{
					Map.RefreshFog();

					foreach (var unit in UnitList)
					{
						TilePos pos = GetTilePos(unit);

						// unit이 볼 수 있는 타일을 찾음
						Map.ComputeFog(pos,
							unit.sightRange / _tileSize,
							unit.sightHeight
							);
                    }
                    Map.ApplyFogAlpha();
                }
                yield return new WaitForSeconds(_updateCycle);
            }
		}
        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false) return;

            if (showGizmos == false) return;

            if (HeightMap != null)
            {
                // 전체 타일 그리드, 장애물 그리드 보여주기
                for (int i = 0; i < HeightMap.GetLength(0); i++)
                {
                    for (int j = 0; j < HeightMap.GetLength(1); j++)
                    {
                        Vector2 center = GetTileCenterPoint(i, j);

                        Gizmos.color = new Color(HeightMap[i, j] - transform.position.y, 0, 0);
                        Gizmos.DrawWireCube(new Vector3(center.x, transform.position.y, center.y)
                            , new Vector3(_tileSize - 0.02f, 0f, _tileSize - 0.02f));
                    }
                }
                foreach (var unit in UnitList)
                {
                    TilePos tilePos = GetTilePos(unit);
                    Vector2 center = GetTileCenterPoint(tilePos.x, tilePos.y);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(new Vector3(center.x, 0f, center.y),
                        new Vector3(_tileSize, 1f, _tileSize));

                }
            }
        }
    }
}