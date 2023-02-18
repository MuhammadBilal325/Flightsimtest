using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Planecontrollernew : MonoBehaviour
{
    public float ThrottleIncrement = 0.1f;
    public float Maxthrust = 100f;
    public float liftpower = 135;
    public Vector3 turnSpeed;
    public Vector3 turnacceleration;
    public AnimationCurve steeringcurve;
    private float throttle;  //Percentage of maximum engine thrust being used
    private float roll;      //Tilting left/right
    private float pitch;     //Tilting upwards/downwards
    private float yaw;       //Turning left to right
    private float pitchdown;
    private Vector3 Crossprod;

    public AnimationCurve AoaCurve;
    public float InducedDragMultiplier;
    [SerializeField] AnimationCurve dragRight, dragLeft, dragTop, dragBottom, dragForward, dragBack;
    Rigidbody rb;
    [SerializeField] TextMeshProUGUI hud;
    [SerializeField] float AngleofAttack;
    [SerializeField] float AngleofAttackYaw;
    [SerializeField] Vector3 LocalAngularVelocity;
    [SerializeField] Vector3 LocalVelocity;
    [SerializeField] Vector3 lastVelocity;
    [SerializeField] Vector3 LocalGForce;
    private Vector3 controlinput;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInputs()
    {
        //Set rotational values from our axis inputs.
        controlinput.x = Input.GetAxisRaw("Pitch");
        controlinput.z= Input.GetAxisRaw("Roll");
        controlinput.y = Input.GetAxisRaw("Yaw");

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
    public static Vector3 Scale6(
     Vector3 value,
     float posX, float negX,
     float posY, float negY,
     float posZ, float negZ
     )
    {
        Vector3 result = value;
        if (result.x > 0)
        {
            result.x *= posX;
        } else if (result.x < 0)
        {
            result.x *= negX;
        }

        if (result.y > 0)
        {
            result.y *= posY;
        }
        else if (result.y < 0)
        {
            result.y *= negY;
        }

        if (result.z > 0)
        {
            result.z *= posZ;
        }
        else if (result.z < 0)
        {
            result.z *= negZ;
        }

        return result;
    }
    void CalculateAngleOfAttack()
    {
        if (LocalVelocity.sqrMagnitude < 0.1f)
        {
            AngleofAttack = 0;
            AngleofAttackYaw = 0;
            return;
        }
        AngleofAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z);
        AngleofAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z);
    }
    void CalculateState(float dt)
    {
        var invRotation = Quaternion.Inverse(rb.rotation);
        LocalVelocity = invRotation * rb.velocity;
        LocalAngularVelocity = invRotation * rb.angularVelocity;
    }
    void CalculateGForce(float dt)
    {
        var invRotation = Quaternion.Inverse(rb.rotation);
        var acceleration = (rb.velocity - lastVelocity) / dt;
        LocalGForce = invRotation * acceleration;
        lastVelocity = rb.velocity;
    }
    void UpdateDrag()
    {
        var lv = LocalVelocity;
        var lv2 = lv.sqrMagnitude;

        var coefficient = Scale6(
            lv.normalized,
            dragRight.Evaluate(Mathf.Abs(lv.x)), dragLeft.Evaluate(Mathf.Abs(lv.x)),
            dragTop.Evaluate(Mathf.Abs(lv.y)), dragBottom.Evaluate(Mathf.Abs(lv.y)),
            dragForward.Evaluate(Mathf.Abs(lv.z)), dragBack.Evaluate(Mathf.Abs(lv.z))
            );

      var drag = coefficient.magnitude * lv2 * -lv.normalized;
        rb.AddRelativeForce(drag);
    }
    void UpdateThrust() 
    {
        rb.AddRelativeForce(throttle * Maxthrust * transform.forward);
    }
    Vector3 CalculateLift(float angleOfAttack, Vector3 rightaxis, float lifePower, AnimationCurve aoaCurve)
    {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightaxis);
        var v2 = liftVelocity.sqrMagnitude;

        var liftCoefficient = aoaCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
        var liftForce = v2 * liftCoefficient * liftpower;

        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightaxis);
        var lift = liftDirection * liftForce;

        var dragForce = liftCoefficient * liftCoefficient * InducedDragMultiplier;
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag = dragDirection * v2 * dragForce;
        return lift + inducedDrag;
    }
    void UpdateLift()
    {
        if (LocalVelocity.sqrMagnitude < 1f) return;

      var liftForce = CalculateLift(AngleofAttack, transform.right, liftpower, AoaCurve);

        rb.AddRelativeForce(liftForce);
    }
    float CalculateSteering(float dt, float angularVelocity, float targetVelocity,float acceleration)
    {
        var error=targetVelocity- angularVelocity;
        var accel = acceleration - angularVelocity;
        return Mathf.Clamp(error, -accel, accel);
    }
    void UpdateSteering(float dt)
    {
        var speed = Mathf.Max(0, LocalVelocity.z);
        var steeringPower = steeringcurve.Evaluate(speed);

        var targetAV = Vector3.Scale(controlinput, turnSpeed * steeringPower);
        hud.text = "target AV.x: " + targetAV.x +"\n";
        hud.text += "target AV.y: " + targetAV.y +"\n";
        hud.text += "target AV.z: " + targetAV.z +"\n";
        var av = LocalAngularVelocity * Mathf.Rad2Deg;

        var correction = new Vector3(
            CalculateSteering(dt, av.x, targetAV.x, turnacceleration.x * steeringPower),
            CalculateSteering(dt, av.y, targetAV.y, turnacceleration.y * steeringPower),
            CalculateSteering(dt, av.z, targetAV.z, turnacceleration.z * steeringPower)
            );
            rb.AddRelativeTorque(correction * Mathf.Deg2Rad,ForceMode.VelocityChange);
    }
    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
       
        CalculateState(dt);
        CalculateAngleOfAttack();
        CalculateGForce(dt);
     
        UpdateThrust();
        UpdateDrag();
        UpdateLift();
        UpdateSteering(dt);
        //Apply forces to our plane
        //rb.AddForce(transform.forward * Maxthrust * throttle);
        //rb.AddTorque(transform.up * yaw );
        //rb.AddTorque(transform.right * pitch );
        //pitchdown = Vector3.Dot(transform.forward, Vector3.up);
        //rb.AddTorque(transform.right * Mathf.Abs(pitchdown) * 0.5f);
        //rb.AddTorque(-transform.forward * roll );
        //rb.AddForce(transform.up * rb.velocity.magnitude * lift);
    }
    private void UpdateHUD()
    {
        //hud.text = "Throttle: " + throttle.ToString("F0") + "%\n";
        //hud.text += "Airspeed: " + (rb.velocity.magnitude * 3.6f).ToString("F0") + "km/h\n";
        //hud.text += "Altitude: " + transform.position.y.ToString("F0") + " m\n";
        //hud.text += "Controlinput.x " + controlinput.x + "\n";
        //hud.text += "Controlinput.y " + controlinput.y + "\n";
        //hud.text += "Controlinput.z " + controlinput.z + "\n";
    }
}