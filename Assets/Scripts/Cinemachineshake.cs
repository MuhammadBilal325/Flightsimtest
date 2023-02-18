using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

public class Cinemachineshake : MonoBehaviour
{
    public CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin noise;
    public float Amplitude=0f;

    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void Update()
    {
        noise.m_AmplitudeGain = Mathf.Clamp(( rb.velocity.magnitude * 3.6f)/300,0,1);

    }
}
