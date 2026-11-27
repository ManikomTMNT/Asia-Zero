using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BaseEnemy : MonoBehaviour {

	float hp = 5;

	//float bullet_delay  = 0;

	//[SerializeField] GameObject bullet_prefab;

	SpriteRenderer sprite;
	Color original_color;

	void Start() {
		sprite = GetComponent<SpriteRenderer>();
		original_color = sprite.color;
	}

	void Update() {
		/*bullet_delay--;
		if (bullet_delay <= 0) {
			bullet_delay = 30;
			GameObject bull = Instantiate (bullet_prefab);
			bull.transform.position = transform.position;

			bull.GetComponent<Bullet> ().dir = new Vector2 ((Random.value*2)-1, (Random.value*2)-1);
		}*/
	}

	public void Damage(float _hp) {
		hp -= _hp;

		StartCoroutine(Flash());

		if (hp <= 0) {
			Destroy(gameObject);
		}
	}

	IEnumerator Flash() {
		sprite.color = Color.gray;

		yield return new WaitForSeconds(0.1f);

		sprite.color = original_color;
	}
}
