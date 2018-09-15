using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * create based upon Windexglow script
 * + converted to C# 
 * + Right click binding
 * 
 * Modified by Vincenzo Rinaldi
 */
public class CameraMovements : MonoBehaviour {
    float mainSpeed = 2.0f; //speed
    float shiftAdd = 25.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 10.0f; //maximum speed when holding shift
    float camSens = 0.2f; //how sensitive it with mouse
    Vector3 lastMouse = new Vector3(255f, 255f, 255f);
    float totalRun  = 1.0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            lastMouse = Input.mousePosition;
        } else if (Input.GetMouseButton(1))
        {
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;
        }


        //Keyboard commands
        float f = 0.0f;
        var p = GetKeyboardInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = p * totalRun * shiftAdd;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000);
            p = p * mainSpeed;
        }

        p = p * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))
        { //If player wants to move on X and Z axis only
            f = transform.position.y;
            transform.Translate(p);
            transform.position = new Vector3(transform.position.x, f, transform.position.z);
        }
        else
        {
            transform.Translate(p);
        }

    }

    Vector3  GetKeyboardInput(){ //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = Vector3.zero;
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += new Vector3(0, 0 , 1);
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity += new Vector3(0, 0 , -1);
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity += new Vector3(-1, 0 , 0);
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += new Vector3(1, 0 , 0);
        }
        return p_Velocity;
    }
}
