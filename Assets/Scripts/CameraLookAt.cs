using UnityEngine;

public class CameraLookAt : MonoBehaviour
{
    public Transform target;

    //private

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 mDelta = Input.mousePositionDelta;

        //mDelta;

        transform.LookAt(target, Vector3.up);
    }
}
