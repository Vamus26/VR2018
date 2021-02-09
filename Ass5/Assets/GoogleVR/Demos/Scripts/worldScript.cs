using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class worldScript : MonoBehaviour {
    private VRVUPoseClient script;
    public GameObject world;
    // Use this for initialization
    void Start () {
        script = gameObject.GetComponent<VRVUPoseClient>();
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 curr_pose = script.getPosition();
        Quaternion curr_rot = script.getRotation();
        Debug.Log(curr_pose);
        world.transform.localPosition = new Vector3(curr_pose.x, curr_pose.y, curr_pose.z);
        Vector3 invPos = new Vector3(-curr_pose.x, -curr_pose.y, -curr_pose.z);
        Quaternion rotation = Quaternion.Inverse(new Quaternion(curr_rot.x, curr_rot.y, curr_rot.z, curr_rot.w));
        Vector3 result = rotation * invPos;
        world.transform.localRotation = rotation;
        world.transform.localPosition = result;
    }
}
