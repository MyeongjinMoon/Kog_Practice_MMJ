using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Myeongjin
{
    public class PlayerSight : MonoBehaviour
    {
        private List<Collider> colls;
        private float sightRange;
        private RaycastHit hit;

        void Start()
        {
            colls = new List<Collider>();
            sightRange = this.transform.parent.GetComponent<FowUnit>().sightRange;
        }
        void Update()
        {
            Sight();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("TeamB"))
            {
                colls.Add(other);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < colls.Count; i++)
            {
                if (colls[i] == other)
                {
                    colls[i].GetComponent<MeshRenderer>().enabled = false;

                    colls.RemoveAt(i);
                }
            }
        }
        private void Sight()
        {
            foreach (var coll in colls)
            {
                //Debug.DrawRay(transform.position, coll.transform.position - transform.position, Color.red, (coll.transform.position - transform.position).magnitude);
                if (Physics.Raycast(transform.position, coll.transform.position - transform.position, out hit, (coll.transform.position - transform.position).magnitude))
                {
                    if (hit.collider.gameObject != coll.gameObject)
                        coll.gameObject.GetComponent<MeshRenderer>().enabled = false;
                    else if (hit.collider.gameObject == coll.gameObject && !hit.collider.gameObject.GetComponent<MeshRenderer>().enabled)
                        coll.gameObject.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
    }
}