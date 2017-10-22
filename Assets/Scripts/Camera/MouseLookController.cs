using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add a rigid body to the capsule
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSWalker script to the capsule

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLookController : MonoBehaviour
{

    public bool online;
    public float mouseSensitivity = 1000.0f;
    public float clampAngle = 80.0f;
    public Vector3 target = new Vector3(0.0f, 0.0f, 0.0f);
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis
    public float maxX;
    public float maxY;
    void Start()
    {
        online = false;
        //  Vector3 rot = transform.localRotation.eulerAngles;
        //  rotY = rot.y;
        // rotX = rot.x;

       
    }

    void Update()
    {
        if (online)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");
           
            rotY += mouseX * mouseSensitivity * Time.deltaTime;
           
                rotX += mouseY * mouseSensitivity * Time.deltaTime;
      //      Debug.Log(rotX + "   " + rotY);

        //    if (rotX > -50 && rotX < 250 && rotY > -200 & rotY < 250)
         //   {

                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

                Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
                transform.rotation = localRotation;
          //  }
        }
    }
    public void Online(bool _on)
    {
        if (_on)
        {
            transform.LookAt(target);
            rotY = transform.rotation.y;
            rotX = transform.rotation.x;
        }
        online = _on;
    }
}