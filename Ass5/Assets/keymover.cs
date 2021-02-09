using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keymover : MonoBehaviour {

	private Vector3 start;
	private PathFinder finder;
	private float last_u;
    private VRVUPoseClient script;
    public GameObject control;
	public Camera virtual_player;
    private Vector2 offset_cum;

    // Use this for initialization
    void Start ()
	{
		last_u = 0;
		start = transform.position;
		finder = gameObject.GetComponent<PathFinder>();
        script = gameObject.GetComponent<VRVUPoseClient>();
    }
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKey(KeyCode.UpArrow)|| Input.GetKey(KeyCode.W))
		{
			gameObject.transform.position += new Vector3(0, 0, 0.02f);
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			gameObject.transform.position += new Vector3(0, 0, -0.02f);
		}
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			gameObject.transform.position += new Vector3(0.02f, 0, 0);
		}
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			gameObject.transform.position += new Vector3(-0.02f, 0, 0);
		}

        if (Input.GetKey(KeyCode.E))
        {
            Quaternion test = Quaternion.Euler(0, 5, 0);
            gameObject.transform.rotation = gameObject.transform.rotation * test;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            Quaternion test = Quaternion.Euler(0, -5, 0);
            gameObject.transform.rotation = gameObject.transform.rotation * test;
        }
        /*  Quaternion curr_rot = script.getRotation();
          Quaternion rotation = Quaternion.Inverse(new Quaternion(curr_rot.x, curr_rot.y, curr_rot.z, curr_rot.w));
          gameObject.transform.localRotation = rotation;*/


        float u = 0;
		Vector2 ideal_pos = new Vector2();
		Vector2 ideal_rot = new Vector2();

		finder.p_u_on_a(gameObject.transform.position, last_u, ref ideal_pos, ref ideal_rot, ref u);

		control.transform.position = new Vector3(ideal_pos.x, 0, ideal_pos.y);
		control.transform.rotation = Quaternion.LookRotation(new Vector3(ideal_rot.x, 0, ideal_rot.y));

		Vector2 virtual_ideal_pos = new Vector2();
		Vector2 virtual_ideal_rot = new Vector2();
		finder.p_on_b(u, ref virtual_ideal_pos, ref virtual_ideal_rot);

        Quaternion qr = Quaternion.LookRotation(new Vector3(ideal_pos.x, 0, ideal_pos.y));
        Quaternion qv = Quaternion.LookRotation(new Vector3(virtual_ideal_rot.x,0, virtual_ideal_rot.y));
        Quaternion qr_inv = Quaternion.Inverse(qr);
        Quaternion qc = qv * qr_inv;//korrektur qr* measurement

        virtual_player.transform.rotation = qc * gameObject.transform.rotation;

        //infite walk
        Vector2 offset = new Vector2();
        Vector2 rota = new Vector2(1.0f, 2.0f);
        finder.p_on_b(1, ref offset, ref rota);
        if (u < 0.1f && last_u > 0.9f)
            offset_cum += offset;

        //position
        Vector3 ideal = new Vector3(ideal_pos.x, 0, ideal_pos.y);
        Vector3 delta = gameObject.transform.localPosition - ideal;
        Vector3 ideal_v = new Vector3(virtual_ideal_pos.x+offset_cum.x,0,virtual_ideal_pos.y + offset_cum.y);
        virtual_player.transform.position = ideal_v + qc * delta;



        //      virtual_player.transform.position  = new Vector3(virtual_ideal_pos.x, 0, virtual_ideal_pos.y);
        //Vector3 tmp /*virtual_player.transform.position*/ = new Vector3(virtual_ideal_pos.x, 0, virtual_ideal_pos.y);
        //    virtual_player.transform.rotation = Quaternion.LookRotation(new Vector3(virtual_ideal_rot.x, 0, virtual_ideal_rot.y));

        //      // Δ = p0 - pos_on_a(p0)             virtual_pos(u) = pos_on_b(u) + qc * Δ
        //      Vector2 delta = new Vector2(virtual_player.transform.position.x, virtual_player.transform.position.z);
        //      delta = delta - virtual_ideal_pos;
        //      virtual_player.transform.position = tmp + virtual_player.transform.rotation * delta;

        last_u = u;
	}
}
