using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noRotation : MonoBehaviour {
    Quaternion tmp;
    Vector3 tmp2;
	// Use this for initialization
	void Start () {
       tmp =  this.gameObject.transform.localRotation;
        tmp2 = this.gameObject.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        this.gameObject.transform.localRotation = tmp;
        this.gameObject.transform.localPosition = tmp2;

    }
}
