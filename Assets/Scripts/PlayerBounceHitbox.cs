using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBounceHitbox : MonoBehaviour {

	public bool can_atk = false;

	PlayerMovement parento;

	void Start() {
		parento = GetComponentInParent<PlayerMovement>();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag ("Enemy") && can_atk) {
			other.GetComponent<BaseEnemy> ().Damage (1);

			Vector2 contactPoint = other.bounds.ClosestPoint (transform.position);

			parento.EnemRebound (contactPoint);
		} else if (other.gameObject.GetComponent<BreakableWall> () != null && can_atk) {
			other.GetComponent<BreakableWall> ().Break ();
			Vector2 contactPoint = other.bounds.ClosestPoint (transform.position);

			parento.EnemRebound (contactPoint);
		} else if (other.GetComponent<Bullet> () && can_atk) {
			Destroy (other.gameObject);
		} else if (other.CompareTag ("Boss") && can_atk) {
			other.GetComponent<BaseBoss> ().Damage (1);

			Vector2 contactPoint = other.bounds.ClosestPoint (transform.position);

			parento.EnemRebound (contactPoint);
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Enemy")) {
			
		}
	}
}