using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform[] povs;
    [SerializeField] float speed;
    private float offset=-40;

    private float lerpcounter = 0;
    private int index = 1;
    private int count = 0;
    private Vector3 target;
    private Quaternion randomrotation;
    [SerializeField] Rigidbody rb;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) index = 3;
    }
    private void FixedUpdate()
    {
        if(count==75)
        {
            offset = 40;
        }
        else if(count==150)
        {
            offset = -40;
            count = 0;
        }
        else
        {
            count++;
            offset *= 0.9f;
        }
        lerpcounter += 0.1f;
        if (lerpcounter > 1)
            lerpcounter = 0;
        randomrotation.z = povs[index].rotation.z + (offset*0.01f);
        transform.position = povs[index].position;
        transform.forward = Vector3.MoveTowards(transform.forward, povs[index].forward, Time.deltaTime * 5); ;
        transform.rotation = Quaternion.Lerp(transform.rotation, randomrotation,lerpcounter);

    }
}
