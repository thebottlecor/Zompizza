using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadTest : MonoBehaviour
{
    // Prints a joystick name if movement is detected.

    void Update()
    {
        // requires you to set up axes "Joy0X" - "Joy3X" and "Joy0Y" - "Joy3Y" in the Input Manager
        if (Input.GetAxis("Horizontal") != 0)
        {
            Debug.Log(Input.GetAxis("Horizontal"));
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            Debug.Log(Input.GetAxis("Vertical"));
        }
    }
}
