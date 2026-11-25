using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAtkHitbox : MonoBehaviour {
	public bool can_atk = false;

	float atk_timer = 0;
	float default_atk_timer = 0.6f;

	bool player_overlapping = false;
	PlayerMovement player;

	void Update() {
		if (can_atk) {
			atk_timer -= Time.deltaTime;

			if (atk_timer <= 0) {
				can_atk = false;
			}
		}
	}

	void FixedUpdate() {
		if (can_atk && player_overlapping && player != null) {
			player.GetHit(1);
		}
	}

	public void Attack() {
		can_atk = true;
		atk_timer = default_atk_timer;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			player = other.GetComponentInParent<PlayerMovement>();
			player_overlapping = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other) {
		if (other.CompareTag("Player")) {
			player_overlapping = false;
			player = null;
		}
	}
}