using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Body : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {


		var minDist = 0.5f;
		var maxDist = 3f;

		var minScale = 0.5f;
		var maxScale = 4f;

		//var scale = Mathf.Lerp(minScale, maxScale, Mathf.InverseLerp(minDist, maxDist, mdist));
		//transform.localScale = new Vector3(scale, scale, scale);
	}
}
