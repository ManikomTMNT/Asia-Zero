using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour {

	public bool can_atk = false;

	float atk_timer = 0;
	float default_atk_timer = 0.1f;

	HashSet<GameObject> hit_enemies = new HashSet<GameObject>();

	SpriteRenderer sprite;

	void Start() {
		sprite = GetComponent<SpriteRenderer>();
		sprite.enabled = false;
	}

	void Update() {
		if (can_atk) {
			atk_timer -= Time.deltaTime;

			if (atk_timer <= 0) {
				can_atk = false;
				sprite.enabled = false;
			}
		}
	}

	public void Attack() {
		can_atk = true;
		atk_timer = default_atk_timer;

		hit_enemies.Clear();

		//sprite.enabled = true;

		Collider2D[] hits = Physics2D.OverlapBoxAll(
			GetComponent<BoxCollider2D>().bounds.center,
			GetComponent<BoxCollider2D>().bounds.size,
			0f
		);

		foreach (Collider2D hit in hits) {
			if (hit.CompareTag("Enemy")) {
				if (hit_enemies.Add(hit.gameObject)) {
					hit.GetComponent<BaseEnemy>().Damage(1);
				}
			} else if (hit.CompareTag("Boss")) {
				if (hit_enemies.Add(hit.gameObject)) {
					hit.GetComponent<BaseBoss>().Damage(1);  // TODO: this crap
				}
			}
		}
	}

	/*private void OnTriggerStay2D(Collider2D other) {
		if (other.CompareTag("Enemy") && can_atk) {
			if (hit_enemies.Add(other.gameObject)) {
				other.GetComponent<BaseEnemy>().Damage(1);
			}
		}
	}*/
}