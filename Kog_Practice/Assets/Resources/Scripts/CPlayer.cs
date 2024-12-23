using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class CPlayer : MonoBehaviour
{
	[Range(0, 10)]
	public float distance = 5;

	private Collider[] hitColliders;

	private int speed = 5;

    private int hitBush = 0;

	private void Update()
	{
		Move();

		SearchBush();
	}
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Bush"))
        {
            hitBush++;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Bush"))
        {
            hitBush--;
        }
    }
    private void Move()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position -= Vector3.right * Time.deltaTime * speed;

        if (Input.GetKey(KeyCode.RightArrow))
            transform.position += Vector3.right * Time.deltaTime * speed;

        if (Input.GetKey(KeyCode.UpArrow))
            transform.position += Vector3.forward * Time.deltaTime * speed;

        if (Input.GetKey(KeyCode.DownArrow))
            transform.position -= Vector3.forward * Time.deltaTime * speed;
    }
	private void SearchBush()
    {
        if (hitBush > 0)
        {
            hitColliders = Physics.OverlapSphere(transform.position, distance);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].gameObject.CompareTag("Bush"))
                {
                    Debug.Log("<color=red>Tag : Sphere</color> " + hitColliders[i].gameObject.name + " Hit!!! ");
                    hitColliders[i].gameObject.GetComponent<Grass>().ChangeMaterial();
                }
            }
        }
    }
}