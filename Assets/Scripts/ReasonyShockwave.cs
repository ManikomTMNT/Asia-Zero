
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReasonyShockwave : MonoBehaviour {

	public float dir = 1;
	float speed = 8;
	// Use this for initialization
	void Start () {
		Destroy(gameObject, 3f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += new Vector3 (dir * speed, 0, 0) * Time.deltaTime;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			other.GetComponentInParent<PlayerMovement>().GetHit(1);
		}
	}
}
