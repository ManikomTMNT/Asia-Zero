
using UnityEngine;
using System.Collections;

public class TestBoss : BaseBoss
{
	Rigidbody2D rb;

	float walk_speed = 4f;
	float jump_force = 14f;
	float hp = 220;

	BoxCollider2D boxcollider;
	bool ground_touch = false;

	bool has = false;

	EnemyAtkHitbox atk_hitbox;
	SpriteRenderer spr;

	Animator anim;
	Transform player;
	float jump_direction;

	[SerializeField] GameObject bullet_prefab;

	[SerializeField] GameObject shockwave_prefab;

	float slam_hover_time = 0.6f;
	float slam_fall_speed = 20f;

	bool active = false;

	[SerializeField] GameObject wall1;
	[SerializeField] GameObject wall2;

	[SerializeField] GameObject TNPC;

	public enum BossState {
		Walk,
		Jump,
		Slash,
		Idle,
		BulletShot, Slam
	}

	public BossState current_state;

	float timer;

	void Awake() {
		atk_hitbox = transform.Find("AttackHitbox").GetComponent<EnemyAtkHitbox>();
		anim = GetComponent<Animator> ();
		boxcollider = GetComponent<BoxCollider2D>();
	}

	void SwitchState(BossState state) {
		timer = 0;
		has = false;
		current_state = state;
	}

	void FixedUpdate() {
		CheckGround();
	}

	void Start() {
		rb = GetComponent<Rigidbody2D>();
		player = GameObject.FindGameObjectWithTag("Player").transform;

		current_state = BossState.Idle;
		timer = 0;
		spr = GetComponent<SpriteRenderer>();
	}

	public void IntroCutscene() {
		StartCoroutine (IIntroCutscene ());
	}

	IEnumerator IIntroCutscene() {
		GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerMovement> ().can_move = false;
		wall1.SetActive(true);
		wall2.SetActive(true);

		yield return new WaitForSeconds (1f);

		yield return StartCoroutine(TNPC.GetComponent<NPC> ().IAction());
		GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerMovement> ().can_move = true;

		active = true;
	}

	void Update()
	{
		switch (current_state) {
		case BossState.Walk:
			Walk();
			break;

		case BossState.Jump:
			Jump();
			break;

		case BossState.Idle:
			Idle();
			break;

		case BossState.Slash:
			Slash();
			break;

		case BossState.BulletShot:
			BulletShot();
			break;

		case BossState.Slam:
			Slam();
			break;
		}

		anim.SetFloat ("yvel", rb.velocity.y);
	}

	void Walk() {
		float direction = Mathf.Sign(player.position.x - transform.position.x);

		rb.velocity = new Vector2(walk_speed * direction, rb.velocity.y);

		spr.flipX = direction < 0;

		anim.SetBool("walking", true);

		timer += Time.deltaTime;

		if (timer >= 0.85f || Mathf.Abs(player.position.x - transform.position.x) < 0.2) {
			SwitchState(Crap.Choose<BossState>(
				BossState.Slash,
				BossState.Jump,
				BossState.BulletShot
			));
			anim.SetBool("walking", false);
		}
	}

	void EnterJump() {
		SwitchState(BossState.Jump);

		jump_direction = Mathf.Sign(player.position.x - transform.position.x);

		rb.velocity = new Vector2(rb.velocity.x, 0);

		anim.SetBool("in_air", true);

		rb.AddForce(
			Vector2.up * Random.Range(jump_force - 3, jump_force + 3),
			ForceMode2D.Impulse
		);
	}


	void Jump() {
		if (timer == 0) {
			EnterJump ();
		}

		timer += Time.deltaTime;
		rb.velocity = new Vector2(walk_speed * jump_direction, rb.velocity.y);

		spr.flipX = jump_direction < 0;

		if (ground_touch && rb.velocity.y <= 0) {
			anim.SetBool("in_air", false);

			SwitchState(Crap.Choose<BossState>(
				BossState.Walk,
				BossState.Slash,
				BossState.BulletShot,
				BossState.Idle,
				BossState.Jump
			));
		}
	}

	void Idle()	{
		rb.velocity = Vector2.zero;

		if (active) {
			timer += Time.deltaTime;
		}

		if (timer >= 1f) {
			SwitchState(Crap.Choose<BossState> (BossState.Walk, BossState.Slash, BossState.BulletShot, BossState.Slam));
		}
	}

	void Slash() {
		rb.velocity = Vector2.zero;
		if (timer == 0) {
			anim.SetTrigger ("slashwindup");
		}

		timer += Time.deltaTime;

		if (timer >= 1.1f && !has) {
			anim.SetBool ("slashing", true);
			has = true;
			atk_hitbox.Attack ();
		}

		if (timer >= 1.7f) {
			anim.SetBool ("slashing", false);
			SwitchState(Crap.Choose<BossState> (BossState.Walk, BossState.BulletShot, BossState.Idle, BossState.Jump, BossState.Slam));

			if (Mathf.Abs (player.position.x - transform.position.x) < 0.6f) {
				SwitchState (BossState.Walk);
			}
		}
	}

	void EnterSlam() {
		SwitchState(BossState.Slam);

		rb.velocity = Vector2.zero;

		anim.SetBool("in_air", true);

		rb.AddForce(
			Vector2.up * Random.Range(jump_force - 3, jump_force + 3),
			ForceMode2D.Impulse
		);
	}

	void Slam() {
		if (timer == 0) {
			EnterSlam();
		}

		timer += Time.deltaTime;


		if (timer < 0.5f) {
			rb.velocity = new Vector2(0, rb.velocity.y);
		}
		else if (timer < 0.5f + slam_hover_time) {
			rb.velocity = Vector2.zero;
		}

		else {
			rb.velocity = new Vector2(0, -slam_fall_speed);

			if (ground_touch) {
				rb.velocity = Vector2.zero;

				anim.SetBool("in_air", false);

				SpawnShockwaves();

				SwitchState(Crap.Choose<BossState>(
					BossState.Walk,
					BossState.Slash,
					BossState.BulletShot
				));
			}
		}
	}

	void SpawnShockwaves() {
		GameObject left = Instantiate(
			shockwave_prefab,
			transform.position,
			Quaternion.identity
		);

		left.GetComponent<ReasonyShockwave>().dir = 1;

		GameObject right = Instantiate(
			shockwave_prefab,
			transform.position,
			Quaternion.identity
		);

		right.GetComponent<ReasonyShockwave>().dir = -1;
	}

	void BulletShot() {
		rb.velocity = Vector2.zero;

		timer += Time.deltaTime;
		anim.SetBool ("bull", true);

		if (timer >= 0.9f && !has) {
			has = true;
			timer = 0;

			int bulletCount = 7;

			for (int i = 0; i < bulletCount; i++) {
				float angle = i * (360f / bulletCount);

				float radian = angle * Mathf.Deg2Rad;

				Vector2 direction = new Vector2(
					Mathf.Cos(radian),
					Mathf.Sin(radian)
				);

				GameObject bull = Instantiate(bullet_prefab);
				bull.transform.position = transform.position;

				bull.GetComponent<Bullet>().dir = direction + new Vector2(Random.value / 3, Random.value / 3);
				bull.GetComponent<Bullet> ().speed = 4f;
			}

			bulletCount = 5;

			for (int i = 0; i < bulletCount; i++) {
				float angle = i * (360f / bulletCount);

				float radian = angle * Mathf.Deg2Rad;

				Vector2 direction = new Vector2(
					Mathf.Cos(radian),
					Mathf.Sin(radian)
				);

				GameObject bull = Instantiate(bullet_prefab);
				bull.transform.position = transform.position;

				bull.GetComponent<Bullet>().dir = direction + new Vector2(Random.value / 3, Random.value / 3);
				bull.GetComponent<Bullet> ().speed = 6f;
			}
		}

		if (timer >= 1.6f) {
			anim.SetBool ("bull", false);
			SwitchState(Crap.Choose<BossState> (BossState.Jump, BossState.Walk, BossState.Slash, BossState.Slam));
		}
	}

	public override void Damage(float _hp) {
		hp -= _hp;

		StartCoroutine(Flash());

		if (hp <= 0) {
			Destroy(gameObject);
		}
	}

	IEnumerator Flash() {
		spr.color = Color.gray;

		yield return new WaitForSeconds(0.1f);

		spr.color = Color.white;
	}

	void CheckGround() {
		RaycastHit2D raycast_hit = Physics2D.BoxCast(
			boxcollider.bounds.center,
			boxcollider.bounds.size,
			0f,
			Vector2.down,
			0.1f
		);

		ground_touch = raycast_hit.collider != null;
	}
}
