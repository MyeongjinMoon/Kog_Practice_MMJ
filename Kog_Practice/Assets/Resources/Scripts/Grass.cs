using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grass : MonoBehaviour
{
	[SerializeField] Material baseColor;
	[SerializeField] Material hitColor;

	MeshRenderer meshRenderer;

	float transParentTime = 0;

	private void Awake()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.material = baseColor;
	}
    private void FixedUpdate()
    {
		//TimeFlow();
        meshRenderer.material = baseColor;
    }
    private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			meshRenderer.material = hitColor;
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			meshRenderer.material = baseColor;
		}
	}
	public void ChangeMaterial()
	{
		meshRenderer.material = hitColor;

		transParentTime += Time.deltaTime * 2;
		if (transParentTime > 1.5f)
			transParentTime = 1.5f;
    }
	private void TimeFlow()
	{
		transParentTime -= Time.deltaTime;

		if (transParentTime < 0)
		{
			transParentTime = 0;
            meshRenderer.material = baseColor;
        }
	}
}
