using UnityEngine;
using System.Collections;


public class MoveCameraInertia : MonoBehaviour 
{
    public float turnSpeed = 4.0f;
    public float panSpeed = 4.0f;
    public float zoomSpeed = 4.0f;

    private Vector3 mouseOrigin;
    private bool isPanning;
    private bool isRotating;
    private bool isZooming;

    Camera self;

    void Start()
    {
        self = this.GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            mouseOrigin = Input.mousePosition;
        }

        isRotating = Input.GetMouseButton(1);
        isPanning = Input.GetMouseButton(0);
        isZooming = Input.GetMouseButton(2);

        Vector3 pos = self.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
        if (isRotating)
        {
            /*GameObject sob = GameObject.Find("Sun");
            transform.RotateAround(sob.transform.position, Vector3.right, -pos.y * turnSpeed);
            transform.RotateAround(sob.transform.position, Vector3.up, pos.x * turnSpeed);*/

            float speed = 5;
            GameObject obj = GameObject.Find("Sun");
            //transform.LookAt(obj.transform);
            transform.RotateAround(obj.transform.position, Vector3.up, Input.GetAxis("Mouse X") * speed);
            transform.RotateAround(obj.transform.position, Vector3.right, Input.GetAxis("Mouse Y") * speed);
        }

        if (isPanning)
        {
            Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
            transform.Translate(move, Space.Self);
        }

        if (isZooming)
        {
            Vector3 move = pos.y * zoomSpeed * transform.forward;
            transform.Translate(move, Space.World);
        }
    }
}

