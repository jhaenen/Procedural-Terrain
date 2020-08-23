using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform viewCamera;
    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float lookSpeed = 3;

    bool lockMouse = false;

    void Update() {
        if(Input.GetMouseButtonDown(0)) {
            lockMouse = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if(Input.GetKeyDown(KeyCode.Escape)) {
            lockMouse = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if(lockMouse) {
            
            transform.Translate(Input.GetAxis("Horizontal") * ((Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed)) * Time.deltaTime, 0, Input.GetAxis("Vertical") * ((Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed)) * Time.deltaTime);
            transform.Rotate(0, Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime, 0);
            viewCamera.Rotate(-Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime, 0, 0);
        }
    }
}
