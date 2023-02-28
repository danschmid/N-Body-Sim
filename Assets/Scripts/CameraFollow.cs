using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
	public GameObject ObjToFollow = null;
    public bool IsActive = false;
    public Camera mainCam;
    public Canvas UI;

    public Camera followCam;
    public float followSpeed = 5f;
    public Camera currentCam;
    public Transform followTarget;
    private bool followingObject = false;

    // Use this for initialization
    void Start () 
	{
        this.GetComponent<Camera>().enabled = false;  //this camera should always be inactive from the start
        currentCam = mainCam;
    }
	
	// Update is called once per frame
	void Update () 
	{
        // Check for input to switch between cameras
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = currentCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == followTarget && followingObject)
                {
                    // Clicked on the follow target, switch back to main camera
                    Debug.Log("Switching to main camera");
                    followingObject = false;
                    followTarget = null;
                    SwitchCurrentCamera();
                }
                else
                {
                    // Clicked on a new object, switch to follow camera
                    Debug.Log("Switching to follow camera");
                    followTarget = hit.transform;
                    SwitchCurrentCamera();
                    Vector3 targetPosition = followTarget.position;
                    followCam.transform.position = new Vector3(transform.position.x, transform.position.y, targetPosition.z - 200);
                    followingObject = true;
                }
            }
        }

        // If following an object, move the follow camera to keep it in view
        if (followingObject)
        {
            Vector3 targetPosition = followTarget.position;
            targetPosition.z = followCam.transform.position.z;
            followCam.transform.position = Vector3.Lerp(followCam.transform.position, targetPosition, followSpeed * Time.deltaTime);
        }

        if(IsActive)  //control object selection and camera switching from the editor
        {
            if (followingObject)
            {
                // Clicked on the follow target, switch back to main camera
                Debug.Log("Switching to main camera");
                followingObject = false;
                followTarget = null;
                SwitchCurrentCamera();
            }
            else
            {
                // Clicked on a new object, switch to follow camera
                Debug.Log("Switching to follow camera");
                followTarget = ObjToFollow.transform;
                SwitchCurrentCamera();
                Vector3 targetPosition = followTarget.position;
                followCam.transform.position = new Vector3(transform.position.x, transform.position.y, targetPosition.z - 200);
                followingObject = true;
            }

            IsActive = false;
        }
    }


    private void SwitchCurrentCamera()
    {
        if (followCam.enabled == true)
        {
            Debug.Log("setting current camera to disabled and IsActive to " + !IsActive);
            followCam.enabled = false;
            mainCam.enabled = true;
            currentCam = mainCam;
        }
        else
        {
            Debug.Log("enabling follow camera...");
            followCam.enabled = true;
            mainCam.enabled = false;
            currentCam = followCam;
        }
    }
}
