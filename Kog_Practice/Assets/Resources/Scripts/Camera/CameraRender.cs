using Myeongjin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRender : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float offset_x;
    [SerializeField] private float offset_y;
    [SerializeField] private float offset_z;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = target.transform.position + new Vector3(offset_x, offset_y, offset_z);
    }
}
