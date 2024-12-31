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
 //   private void OnTriggerEnter(Collider other)
	//{
	//	if(other.CompareTag("Player"))
	//	{
	//		meshRenderer.material = hitColor;
	//	}
	//}
	//private void OnTriggerExit(Collider other)
	//{
	//	if (other.CompareTag("Player"))
	//	{
	//		meshRenderer.material = baseColor;
	//	}
	//}
	public void ChangeMaterial()
	{
		meshRenderer.material = hitColor;
    }
}
