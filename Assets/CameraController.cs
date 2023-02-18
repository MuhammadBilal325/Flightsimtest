using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform[] povs;
    [SerializeField] float speed;

    private int index = 1;
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
        randomrotation = povs[index].rotation;
        if (rb.velocity.magnitude * 3.6f > 80)
        {
            randomrotation.x += (Random.Range(-1, 1) * 0.01f);
            randomrotation.y += (Random.Range(-1, 1) * 0.01f);
            randomrotation.z += (Random.Range(-1, 1) * 0.01f);
        }
        transform.position = povs[index].position;
        transform.forward = Vector3.MoveTowards(transform.forward, povs[index].forward, Time.deltaTime * 5); ;
        transform.rotation = Quaternion.Lerp(transform.rotation, randomrotation, Time.deltaTime * 5);

    }
}
