using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Myeongjin
{
	public class PlayerController : MonoBehaviour
    {
        [Range(0, 10)]
        public float distance;
        public float speed;

        private Collider[] grassColliders;
        private List<Collider> colls;

        private RaycastHit hit;
        private int hitBush = 0;

        void Start()
		{
			colls = new List<Collider>();
		}
		void Update()
		{
			Move();
			SearchBush();
        }
		private void Move()
		{
			if (Input.GetKey(KeyCode.W))
			{
				this.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, speed));
				//transform.position += new Vector3(0, 0, speed * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.S))
			{
				this.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, -speed));
				//transform.position += new Vector3(0, 0, speed * -Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.A))
			{
				this.GetComponent<Rigidbody>().AddForce(new Vector3(-speed, 0, 0));
				//transform.position += new Vector3(speed * -Time.deltaTime, 0, 0);
			}
			if (Input.GetKey(KeyCode.D))
			{
				this.GetComponent<Rigidbody>().AddForce(new Vector3(speed, 0, 0));
				//transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
			}
		}
		private void OnTriggerEnter(Collider other)
		{
            if (other.gameObject.CompareTag("Bush"))
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
        private void SearchBush()
        {
            if (hitBush > 0)
            {
                grassColliders = Physics.OverlapSphere(transform.position, distance);

                for (int i = 0; i < grassColliders.Length; i++)
                {
                    if (grassColliders[i].gameObject.CompareTag("Bush"))
                    {
                        Debug.Log("<color=red>Tag : Sphere</color> " + grassColliders[i].gameObject.name + " Hit!!! ");
                        grassColliders[i].gameObject.GetComponent<Grass>().ChangeMaterial();
                    }
                }
            }
        }
    }
}