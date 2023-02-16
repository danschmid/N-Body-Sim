using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
	public GameObject ObjToFollow = null;
    public bool IsActive = false;
    public Camera MainCam;

    // Use this for initialization
    void Start () 
	{
        this.GetComponent<Camera>().enabled = false;  //this camera should always be inactive from the start
    }
	
	// Update is called once per frame
	void Update () 
	{
        if (IsActive && ObjToFollow != null)
        {
            transform.position = new Vector3(ObjToFollow.transform.position.x, ObjToFollow.transform.position.y, transform.position.z);
        }
        else if (IsActive && ObjToFollow == null)
        {
            Debug.Log("No object to follow");
        }
	}
}
