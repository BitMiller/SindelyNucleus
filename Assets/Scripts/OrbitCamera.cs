
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Orbit")]
    public float yaw = 0f;
    public float pitch = 20f;
    public float distance = 10f;

    [Header("Controls")]
    public float rotationSpeed = 3f;
    public float zoomSpeed = 5f;
    public float minDistance = 2f;
    public float maxDistance = 30f;

    void Start() {
    }

    void Update()
    {
        // Rotate while holding the right mouse button.
        //if (Input.GetMouseButton(1))
        if (Keyboard.current.spaceKey.isPressed)
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;

            pitch = Mathf.Clamp(pitch, -80f, 80f);
        }

        // Zoom with the mouse wheel.
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        Vector3 pos = target != null ? target.position : Vector3.zero;

        // Calculate the orbit rotation.
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Place the camera on the orbit sphere.
        transform.position =
            pos +
            rotation * new Vector3(0f, 0f, -distance);

        // Point the camera toward the world origin.
        transform.LookAt(pos, Vector3.up);

        //Debug.Log($"Pitch: {pitch}, Yaw: {yaw}");
    }

    /*void LateUpdate()
    {
        // Calculate the orbit rotation.
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Place the camera on the orbit sphere.
        transform.position =
            Vector3.zero +
            rotation * new Vector3(0f, 0f, -distance);

        // Point the camera toward the world origin.
        transform.LookAt(Vector3.zero, Vector3.up);
    }*/

}
