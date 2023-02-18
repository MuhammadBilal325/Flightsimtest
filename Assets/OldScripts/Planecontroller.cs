using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Planecontroller : MonoBehaviour
{
    public float ThrottleIncrement = 0.1f;
    public float Maxthrust = 100f;
    public float responsiveness = 10f;
    public float lift = 135f;

    private float throttle;  //Percentage of maximum engine thrust being used
    private float roll;      //Tilting left/right
    private float pitch;     //Tilting upwards/downwards
    private float yaw;       //Turning left to right
    private float pitchdown;
    private Vector3 Crossprod;

    private float responseModifier //Value used to tweak responsiveness to suit plane's mass
    {
        get
        {
            return (rb.mass / 10f) * responsiveness;
        }
    }
    Rigidbody rb;
    [SerializeField]TextMeshProUGUI hud;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInputs()
    {
        //Set rotational values from our axis inputs.
        roll = Input.GetAxis("Roll");
        pitch = Input.GetAxis("Pitch");
        yaw = Input.GetAxis("Yaw");

        //Handle throttle value being sure to clamp it between 0 and 100
        if (Input.GetKey(KeyCode.Space)) throttle += ThrottleIncrement;
        else if (Input.GetKey(KeyCode.LeftControl)) throttle -= ThrottleIncrement;
        throttle = Mathf.Clamp(throttle, 0f, 100f);
    }
    private void Update()
    {
        HandleInputs();
        UpdateHUD();
    }
    private void FixedUpdate()
    {
        //Apply forces to our plane
        rb.AddForce(transform.forward * Maxthrust * throttle);
        rb.AddTorque(transform.up * yaw * responseModifier);
        rb.AddTorque(transform.right * pitch * responseModifier);
        pitchdown = Vector3.Dot(transform.forward, Vector3.up);
        rb.AddTorque(transform.right *Mathf.Abs(pitchdown)*200);
        rb.AddTorque(-transform.forward * roll * responseModifier);
        rb.AddForce(transform.up*rb.velocity.magnitude*lift);
    }
    private void UpdateHUD()
    {
        hud.text = "Throttle: " + throttle.ToString("F0")+ "%\n";
        hud.text += "Airspeed: " + (rb.velocity.magnitude * 3.6f).ToString("F0") + "km/h\n";
        hud.text += "Altitude: " + transform.position.y.ToString("F0") + " m\n";;
    }
}