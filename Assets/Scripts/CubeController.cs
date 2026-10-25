using System.ComponentModel;
using UnityEngine;

public class CubeController : MonoBehaviour
{

    //public GameObject test;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        /*transform.position.x = Input.mousePosition.x;
        transform.position.z = Input.mousePosition.y;*/

        Vector3 mMove = Input.mousePositionDelta;
        Vector3 curPos = transform.position;

        curPos.x += mMove.x/100;
        curPos.z += mMove.y/100;

        transform.position = curPos;
    }
}
