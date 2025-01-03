using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
	[Range(5, 15)]
	[SerializeField] private int distance;

	[Range(30, 60)]
	[SerializeField] private int angle;

	[SerializeField] private GameObject target;

    private Transform targetTransform;

    private Vector3 offset;

	private int version = 1;

	private void Start()
	{
		targetTransform = target.GetComponent<Transform>();

		offset = new Vector3(0, distance * Mathf.Cos(angle * Mathf.Deg2Rad), -distance * Mathf.Sin(angle * Mathf.Deg2Rad));
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			version = 1;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			version = 2;
		}
		SetOffset();


        SetPosition();
	}
	private void SetOffset()
	{
		offset = new Vector3(0, distance * version * Mathf.Cos(angle * Mathf.Deg2Rad), -distance * version * Mathf.Sin(angle * Mathf.Deg2Rad));
	}
	private void SetPosition()
    {
		this.transform.position = targetTransform.position + offset;

		transform.rotation = Quaternion.Euler(new Vector3(90 - angle, 0, 0));
    }
}
