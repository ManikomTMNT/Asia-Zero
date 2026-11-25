using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour {

	[SerializeField] private float bounceForce = 20f;

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!collision.gameObject.CompareTag("Player")) return;

		Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D> ();
		rb.velocity = new Vector2(rb.velocity.x, bounceForce);
	}
}
