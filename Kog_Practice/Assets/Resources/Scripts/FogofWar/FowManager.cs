using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Myeongjin
{
	public class FowManager : MonoBehaviour
	{
		private static FowManager f_instance;
		private const string FOWMANAGERNAME = "_FowManager";

		public static FowManager Instance
		{
			get
			{
				if (f_instance == null)
				{
					GameObject newUIManagerObject = new GameObject(FOWMANAGERNAME);
					f_instance = newUIManagerObject.AddComponent<FowManager>();
				}
				return f_instance;
			}
		}

		private Material _fogMaterial;
		public GameObject _rendererPrefab;

		private void InitFogTexture()
		{
			var renderer = Instantiate(_rendererPrefab, transform);
			renderer.transform.localPosition = Vector3.zero;
			//renderer.transform.localScale = new Vector3(_fog);
		}

		public LayerMask _groundLayer;	//
		public float _fogWidthX = 40;
		public float _fogWidthZ = 40;
		public float _tileSize = 1;
		public float _updateCycle = 0.5f;

		public class FogAlpha
		{
			[Range(0, 1)] public float current = 0.0f;
			[Range(0, 1)] public float visited = 0.8f;
			[Range(0, 1)] public float never = 1.0f;
		}
		public FogAlpha fogAlpha = new FogAlpha();

		public bool showGizmos = true;

		public FowMap Map { get; private set; }
	}
}