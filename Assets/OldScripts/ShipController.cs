using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public float forwardSpeed = 50f, strafeSpeed = 12.5f, hoverSpeed = 10f;
    private float activeForwardSpeed, activeStrafeSpeed, activeHoverSpeed;
    private float forwardAcceleration = 10f, strafeAcceleration = 7f, hoveracceleration = 7f;
    public float lookrotatespeed = 90f;
    private Vector2 Lookinput, ScreenCentre, MouseDistance;
    private float rollinput;
    public float rollspeed = 90f, rollAcceleration = 10f;
    private float deceleration=0.1f;
    // Start is called before the first frame update
    void Start()
    {
        ScreenCentre.x = Screen.width * 0.5f;
        ScreenCentre.y = Screen.height * 0.5f;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        Lookinput.x = Input.mousePosition.x;
        Lookinput.y = Input.mousePosition.y;

        MouseDistance.x = (Lookinput.x - ScreenCentre.x) / ScreenCentre.y;
        MouseDistance.y = (Lookinput.y - ScreenCentre.y) / ScreenCentre.y;

        MouseDistance = Vector2.ClampMagnitude(MouseDistance,1f);
        rollinput = Mathf.Lerp(rollinput, Input.GetAxisRaw("Roll"), rollAcceleration * Time.deltaTime);
       
        transform.Rotate(-MouseDistance.y * lookrotatespeed * Time.deltaTime, MouseDistance.x * lookrotatespeed * Time.deltaTime,rollinput*rollspeed*Time.deltaTime,Space.Self);
        if (Input.GetAxisRaw("Vertical")==1)
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, Input.GetAxisRaw("Vertical") * forwardSpeed, forwardAcceleration * Time.deltaTime);
        else if(activeForwardSpeed > deceleration)
            activeForwardSpeed -= deceleration;
        activeStrafeSpeed = Mathf.Lerp(activeStrafeSpeed, Input.GetAxisRaw("Horizontal") * strafeSpeed, strafeAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, Input.GetAxisRaw("Hover") * hoverSpeed, hoveracceleration * Time.deltaTime);

        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;
        transform.position += transform.right * activeStrafeSpeed * Time.deltaTime;
        transform.position += transform.up * activeHoverSpeed * Time.deltaTime;
    }
}
