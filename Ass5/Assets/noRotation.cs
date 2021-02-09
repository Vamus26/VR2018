using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class noRotation : MonoBehaviour {
    Quaternion tmp;
    Vector3 tmp2;
    public GameObject cam;
    Vector3 diff;
    public Transform[] wayPointList;

    public int currentWayPoint = 0;
    Transform targetWayPoint;

    public float speed = 0.1f;
    // Use this for initialization
    void Start () {
       tmp =  this.gameObject.transform.localRotation;
        tmp2 = this.gameObject.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
  //      this.gameObject.transform.localRotation = tmp;
     //   this.gameObject.transform.localPosition.y = tmp2.y;
   //     this.gameObject.transform.localPosition = tmp2;

        if (currentWayPoint < this.wayPointList.Length)
        {
            if (targetWayPoint == null)
                targetWayPoint = wayPointList[currentWayPoint];

            diff = transform.position - cam.transform.position;
            diff.y = 0.0f;
            float dist = diff.magnitude;
            this.gameObject.GetComponent<Animator>().enabled = false;
            if (dist < 5.0f)
            {
                this.gameObject.GetComponent<Animator>().enabled = true;
                walk();
            }
        }
        Vector3 dist2 = wayPointList[4].transform.position - cam.transform.position;
        dist2.y = 0.0f;
        float fdist2 = dist2.magnitude;
        if (fdist2 <2.0f)
        {
          //  Debug.Log("here");
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void walk()
    {
        // rotate towards the target
        transform.forward = Vector3.RotateTowards(transform.forward, targetWayPoint.position - transform.position, speed * Time.deltaTime, 0.0f);

        // move towards the target
        transform.position = Vector3.MoveTowards(transform.position, targetWayPoint.position, speed * Time.deltaTime);

        if (transform.position == targetWayPoint.position)
        {
            currentWayPoint++;
            targetWayPoint = wayPointList[currentWayPoint];
        }
    }
}
