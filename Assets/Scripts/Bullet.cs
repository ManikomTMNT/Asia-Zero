using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	public float speed = 2f;
	public float lifeTime = 3f;
	public int damage = 1;
	public Vector2 dir = new Vector2(1, 0);
	public float acc = 0f;

	Rigidbody2D rb;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.velocity = dir * speed;

		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0, 0, angle-90);

		Destroy(gameObject, lifeTime);
	}

	void Update() {
		speed += acc;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player")) {
			other.GetComponent<PlayerMovement>().GetHit(damage);
			Destroy(gameObject);
		}
	}
}
