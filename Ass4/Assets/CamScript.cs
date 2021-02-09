using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour {

    public Camera main;
    public GameObject birdi;
    public GameObject world;
    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {

            main.gameObject.SetActive(!main.gameObject.activeInHierarchy);
            //  yield return new WaitForEndOfFrame();

            birdi.gameObject.SetActive(!birdi.gameObject.activeInHierarchy);
            if (birdi.gameObject.activeInHierarchy)
            {
                world.transform.SetParent(birdi.transform);
            }
            else
            {
                world.transform.SetParent(main.transform);
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            //Debug.Log("hello");
            world.gameObject.transform.Translate(0, 0, -1.0f);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            //Debug.Log("hello");
            world.gameObject.transform.Translate(1.0f, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            //Debug.Log("hello");
            world.gameObject.transform.Translate(0, 0, 1.0f);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            //  Debug.Log("hello");
            world.gameObject.transform.Translate(-1.0f, 0, 0);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            world.gameObject.transform.Rotate(-Vector3.down, 10.0f);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            world.gameObject.transform.Rotate(Vector3.down, 10.0f);
        }
    }
}
