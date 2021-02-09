using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour {

	public Vector2 circle_A = new Vector2(0, -2);
	public Vector2 circle_B = new Vector2(0, 2);
	private Vector2 circ_v;
	private Vector2 midpoint;
	private Vector2 virtual_circle1;
	private Vector2 virtual_circle2;

	const float around_unit_circle = 0.75f * (2.0f * Mathf.PI);
	const float total_dist = 4.0f + 2.0f * around_unit_circle;
	private float r;
	private Vector2 path_down;
	private Vector2 path_up;

	// Use this for initialization
	void Start ()
	{
		init();
	}

	private float ccw(float signed_angle)
	{
		if (signed_angle < 0)
		{ signed_angle = 180.0f + (180.0f + signed_angle); }
		signed_angle = 360.0f - signed_angle;
		return signed_angle;
	}

	private Vector2 normalize(Vector2 input)
	{
		return input / Mathf.Sqrt(Vector2.Dot(input, input));
	}

	public void p_u_on_a(Vector3 pos, float last_u, ref Vector2 p_on_a, ref Vector2 r_on_a, ref float u_on_a)
	{
		Vector2 pos2 = new Vector2(pos.x, pos.z);
		float dist_a = Vector2.Distance(pos2, circle_A);
		float dist_b = Vector2.Distance(pos2, circle_B);

		if (dist_a < dist_b) //test lower circle
		{
			Vector2 norm_diff = normalize(pos2 - circle_A); ;
			float angle = ccw(Vector2.SignedAngle(circ_v, pos2 - circle_A));

			if (angle > 45.0f && angle < 315.0f)
			{
				float u_d = 1.0f + ((angle - 45.0f) / 270.0f) * around_unit_circle;
				p_on_a = circle_A + r * norm_diff;
				u_on_a = u_d / total_dist;
				r_on_a = Quaternion.Euler(0, 0, -(angle - 45.0f)) * path_down;
				return;
			}
		}
		else //test upper circle
		{
			Vector2 norm_diff = normalize(pos2 - circle_B);
			float angle = ccw(Vector2.SignedAngle(-circ_v, pos2 - circle_B));

			if (angle > 45.0f && angle < 315.0f)
			{
				float u_d = 3.0f + around_unit_circle + ((360.0f - angle - 45.0f) / 270.0f) * around_unit_circle;
				p_on_a = circle_B + r * norm_diff;
				u_on_a = u_d / total_dist;
				r_on_a = Quaternion.Euler(0, 0, (360.0f - angle - 45.0f)) * path_up;
				return;
			}
		}

		Vector2 local_pos = pos2 - midpoint;
		float on_path_down = Vector2.Dot(local_pos, path_down);
		float on_path_up = Vector2.Dot(local_pos, path_up);

		float last_ref = last_u * total_dist;

		if(last_ref < (1.0f + 0.5f*around_unit_circle) || last_ref > 3.0f + 1.5f*around_unit_circle) // on path down
		{
			p_on_a = midpoint + on_path_down * path_down;
			on_path_down = on_path_down < 0 ? (total_dist * r) + on_path_down : on_path_down;
			u_on_a = (on_path_down / r) / total_dist;
			r_on_a = path_down;
			return;
		}
		else
		{
			p_on_a = midpoint + on_path_up * path_up;
			u_on_a = (2.0f + around_unit_circle + (on_path_up / r)) / total_dist;
			r_on_a = path_up;
			return;
		}
	}

	public void p_on_b(float u, ref Vector2 p_on_b, ref Vector2 r_on_b)
	{
		float u_f = u * total_dist;

		if(u_f < 1.0f)
		{
			p_on_b = (path_down * u_f) * r;
			r_on_b = path_down;
		}
		else if(u_f < (1.0f + around_unit_circle))
		{
			float sped = (u_f - 1.0f) / around_unit_circle;
			Quaternion rot = Quaternion.Euler(0, 0, -90.0f * sped);
			Vector2 from_circle = rot * (3.0f * path_up * r);
			p_on_b = virtual_circle1 + from_circle;
			r_on_b = rot * path_down;
		}
		else if (u_f < (3.0f + around_unit_circle))
		{
			float sped = (u_f - 1.0f - around_unit_circle);
			Vector2 after_circle = virtual_circle1 + 3.0f * path_down * r;
			p_on_b = after_circle - sped * path_up * r;
			r_on_b = -path_up;
		}
		else if (u_f < (3.0f + 2.0f * around_unit_circle))
		{
			float sped = (u_f - 3.0f - around_unit_circle) / around_unit_circle;
			Quaternion rot = Quaternion.Euler(0, 0, 90.0f * sped);
			Vector2 from_circle = rot * (3.0f * -path_down * r);
			p_on_b = virtual_circle2 + from_circle;
			r_on_b = rot * -path_up;
		}
		else
		{
			float sped = (u_f - 3.0f - 2.0f * around_unit_circle);
			Vector2 after_circle = virtual_circle2 - 3.0f * path_up * r;
			p_on_b = after_circle + sped * path_down * r;
			r_on_b = path_down;
		}
	}

	private void init()
	{
		circ_v = circle_B - circle_A;

		midpoint = (circle_B + circle_A) / 2.0f;

		r = Mathf.Sqrt(0.5f * Vector2.Dot(0.5f * circ_v, 0.5f * circ_v));

		Vector2 down = circle_A - midpoint;
		Vector2 right = new Vector2(-down.y, down.x);

		path_down = ((down + right) / 2.0f);
		path_down = path_down / Mathf.Sqrt(Vector2.Dot(path_down, path_down));
		path_up = new Vector2(-path_down.y, path_down.x);

		virtual_circle1 = (path_down - 3.0f * path_up) * r;
		virtual_circle2 = virtual_circle1 + 3.0f * path_down * r - 2.0f * path_up * r + 3.0f * path_down * r;
	}
	
	// Update is called once per frame
	void Update ()
	{
		init();
	}
}
