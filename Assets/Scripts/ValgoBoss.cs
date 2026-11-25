
using UnityEngine;
using System.Collections;

public class ValgoBoss : BaseBoss
{
	Rigidbody2D rb;

	float walk_speed = 2.7f;
	float jump_force = 12f;
	float hp = 95;

	int shots = 0;
	float shotWait = 0.35f;
	bool shooting = false;

	BoxCollider2D boxcollider;
	bool ground_touch = false;

	bool has = false;

	EnemyAtkHitbox atk_hitbox;
	SpriteRenderer spr;

	public Sprite[] bull_sprs;

	float dash_speed = 7f;
	float dash_duration = 1.9f;
	float dash_direction;
	bool dashing = false;

	int sky_phase = 0;

	float sky_dash_speed = 8f;
	float sky_dash_duration = 0.3f;
	float sky_dash_wait = 0.1f;

	float sky_direction;

	Animator anim;
	Transform player;
	float jump_direction;

	int slash_count = 0;
	float slash_move_speed = 5f;

	[SerializeField] GameObject bullet_prefab;

	public enum BossState {
		Walk,
		Jump,
		Idle,
		BulletThrow, SkyDash,
		Dash,
		Slash
	}

	public BossState current_state;
	Transform hitbox_transform;

	float timer;

	void Awake() {
		atk_hitbox = transform.Find("AttackHitbox").GetComponent<EnemyAtkHitbox>();
		anim = GetComponent<Animator> ();
		Transform hitbox_transform = atk_hitbox.transform;
		boxcollider = GetComponent<BoxCollider2D>();
	}

	void SwitchState(BossState state) {
		timer = 0;
		has = false;
		current_state = state;
		dashing = false;
		sky_phase = 0;
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

		case BossState.BulletThrow:
			BulletThrow();
			break;

		case BossState.Dash:
			Dash();
			break;

		case BossState.SkyDash:
			SkyDash();
			break;

		case BossState.Slash:
			Slash();
			break;
		}

	}

	void Slash() {
		timer += Time.deltaTime;

		if (timer == Time.deltaTime) {
			rb.velocity = Vector2.zero;
			if (slash_count == 0) {
				anim.SetBool ("slashwindup", true);
			}
		}

		// Start 
		if (timer >= 0.6f && !has) {
			if (slash_count == 0) {
				anim.SetBool ("slashwindup", false);
				anim.SetBool ("slash1", true);
			} else {
				anim.SetBool ("slash1", false);
				anim.SetBool ("slash2", true);
			}
			has = true;
			atk_hitbox.Attack();
		}

		// Move
		if (timer >= 0.6f && timer < 1.0f) {
			float direction = Mathf.Sign(player.position.x - transform.position.x);

			spr.flipX = direction < 0;

			Vector3 scale = atk_hitbox.transform.localScale;
			scale.x = Mathf.Abs(scale.x) * (direction < 0 ? -1 : 1);
			atk_hitbox.transform.localScale = scale;

			rb.velocity = new Vector2(direction * slash_move_speed, rb.velocity.y);
		}

		// d slash
		if (timer >= 1.0f) {
			rb.velocity = Vector2.zero;

			slash_count++;

			if (slash_count >= 2) {
				anim.SetBool ("slash2", false);
				slash_count = 0;

				SwitchState(Crap.Choose<BossState>(
					BossState.Walk,
					BossState.Jump,
					BossState.Idle,
					BossState.BulletThrow,
					BossState.Dash, BossState.Slash
				));
			}
			else {
				timer = 0;
				has = false;
			}
		}
	}

	void Walk() {
		anim.SetBool("walk", true);
		float direction = Mathf.Sign(player.position.x - transform.position.x);

		rb.velocity = new Vector2(walk_speed * direction, rb.velocity.y);

		spr.flipX = direction < 0;

		timer += Time.deltaTime;

		if (timer >= 0.85f || Mathf.Abs(player.position.x - transform.position.x) < 0.2) {
			SwitchState(Crap.Choose<BossState>(BossState.Jump, BossState.BulletThrow, BossState.Dash, BossState.SkyDash));
			anim.SetBool("walk", false);
		}
	}

	void EnterJump() {
		SwitchState(BossState.Jump);

		jump_direction = Mathf.Sign(player.position.x - transform.position.x);

		rb.velocity = new Vector2(rb.velocity.x, 0);

		rb.AddForce(
			Vector2.up * Random.Range(jump_force - 3, jump_force + 3),
			ForceMode2D.Impulse
		);
	}


	void Jump() {
		if (timer == 0) {
			EnterJump ();
			anim.SetBool("jump", true);
		}

		timer += Time.deltaTime;
		rb.velocity = new Vector2(walk_speed * jump_direction, rb.velocity.y);

		spr.flipX = jump_direction < 0;

		if (rb.velocity.y <= 0) {
			anim.SetBool("jump", false);
			anim.SetBool("fall", true);
		}

		if (ground_touch && rb.velocity.y <= 0) {
			anim.SetBool("fall", false);

			SwitchState(Crap.Choose<BossState>(
				BossState.Walk,
				BossState.Idle,
				BossState.Jump,
				BossState.BulletThrow, BossState.Dash, BossState.SkyDash, BossState.Slash
			));
		}
	}

	void ResetX() {
		rb.velocity = new Vector2(0, rb.velocity.y);
	}

	void Idle()	{
		ResetX ();

		timer += Time.deltaTime;

		if (timer >= 1f) {
			SwitchState(Crap.Choose<BossState> (BossState.BulletThrow, BossState.Dash, BossState.Jump, BossState.Walk, BossState.SkyDash, BossState.Slash));
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

	void BulletThrow() {
		ResetX ();

		timer += Time.deltaTime;
		anim.SetBool ("bulletwindup", true);

		if (timer >= 0.9f && !shooting) {
			anim.SetBool ("bulletwindup", false);
			shooting = true;
			timer = 0;
			shots = 0;
		}

		if (shooting && timer >= shotWait) {
			anim.SetBool ("bulletwindup", false);
			timer = 0;
			shots++;

			BulletShot();

			if (shots >= 6) {
				shooting = false;
				anim.SetBool("bullet", false);
				anim.StopPlayback ();
				current_state = Crap.Choose<BossState> (BossState.Jump, BossState.Idle, BossState.BulletThrow,
					BossState.Dash, BossState.SkyDash, BossState.Slash);
			}
		}
	}

	void BulletShot() {
		anim.SetBool ("bullet", true);
		anim.Play("ValgoBullet", -1, 0f);
		GameObject player = GameObject.FindGameObjectWithTag("Player");

		if (player == null) return;

		Vector2 targetDir = (player.transform.position - transform.position).normalized;

		float baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

		int bulletCount = 3;
		float arc = 85f;
		Sprite b = Crap.Choose<Sprite>(bull_sprs[0], bull_sprs[1], bull_sprs[2], bull_sprs[3], bull_sprs[4]);
		for (int i = 0; i < bulletCount; i++) {
			float angle = baseAngle - arc / 2f + i * (arc / (bulletCount - 1)) + ((Random.value*10) - (1*10/2));

			float radian = angle * Mathf.Deg2Rad;

			Vector2 direction = new Vector2(
				Mathf.Cos(radian),
				Mathf.Sin(radian)
			);

			GameObject bull = Instantiate(bullet_prefab);
			bull.transform.position = transform.position;

			bull.GetComponent<Bullet>().dir = direction;
			bull.GetComponent<Bullet>().speed = 4f;
			bull.GetComponent<SpriteRenderer> ().sprite = b;
		}

		bulletCount = 4;
		arc = 125f;
		b = Crap.Choose<Sprite>(bull_sprs[0], bull_sprs[1], bull_sprs[2], bull_sprs[3], bull_sprs[4]);

		for (int i = 0; i < bulletCount; i++) {
			float angle = baseAngle - arc / 2f + i * (arc / (bulletCount - 1)) + ((Random.value*10) - (1*10/2));

			float radian = angle * Mathf.Deg2Rad;

			Vector2 direction = new Vector2(
				Mathf.Cos(radian),
				Mathf.Sin(radian)
			);

			GameObject bull = Instantiate(bullet_prefab);
			bull.transform.position = transform.position;

			bull.GetComponent<Bullet>().dir = direction;
			bull.GetComponent<SpriteRenderer> ().sprite = b;
			bull.GetComponent<Bullet>().speed = 6f;
		}
	}

	void Dash() {
		ResetX ();

		timer += Time.deltaTime;

		if (timer < 0.7f) {
			if (!has) {
				dash_direction = Mathf.Sign(player.position.x - transform.position.x);
				spr.flipX = dash_direction < 0;
				has = true;
			}
			anim.SetBool ("dashwindup", true);
			return;
		}

		if (!dashing) {
			anim.SetBool ("dashwindup", false);
			anim.SetBool ("dash", true);
			dashing = true;
			timer = 0;
		}

		rb.velocity = new Vector2(dash_direction * dash_speed, 0);

		if (timer >= dash_duration) {
			dashing = false;
			ResetX ();
			anim.SetBool("dash", false);
			anim.SetBool("dashwindup", false);

			SwitchState(Crap.Choose<BossState>(
				BossState.BulletThrow,
				BossState.Idle,
				BossState.Jump, BossState.Slash
			));
		}
	}

	void SkyDash() {
		timer += Time.deltaTime;
		anim.SetBool ("airdash", true);

		switch (sky_phase) {

		// First dash
		case 0:
			if (timer == Time.deltaTime) {
				sky_direction = Mathf.Sign(player.position.x - transform.position.x);
				spr.flipX = sky_direction < 0;
			}

			rb.velocity = new Vector2(
				sky_direction * sky_dash_speed,
				sky_dash_speed
			);

			if (timer >= sky_dash_duration) {
				sky_phase = 1;
				timer = 0;
				rb.velocity = Vector2.zero;
			}
			break;

			// do nnnothig
		case 1:
			rb.velocity = Vector2.zero;

			if (timer >= sky_dash_wait) {
				sky_phase = 2;
				timer = 0;

				sky_direction = Mathf.Sign(player.position.x - transform.position.x);
				spr.flipX = sky_direction < 0;
			}
			break;

			// other dash
		case 2:
			rb.velocity = new Vector2(
				sky_direction * sky_dash_speed,
				sky_dash_speed
			);

			if (timer >= sky_dash_duration) {
				sky_phase = 3;
				timer = 0;
				rb.velocity = Vector2.zero;
			}
			break;

			// fire prep
		case 3:
			rb.velocity = Vector2.zero;

			if (timer >= 0.1f) {
				CircleShot();
				sky_phase = 4;
				timer = 0;
			}
			break;

			// Fall
		case 4:
			rb.velocity = new Vector2(0, -sky_dash_speed);

			if (ground_touch && rb.velocity.y <= 0) {
				rb.velocity = Vector2.zero;
				sky_phase = 0;
				anim.SetBool ("airdash", false);

				SwitchState(Crap.Choose<BossState>(
					BossState.Walk,
					BossState.Jump,
					BossState.Idle,
					BossState.BulletThrow, BossState.Slash
				));
			}
			break;
		}
	}

	void CircleShot() {
		int bulletCount = 12;

		Sprite b = Crap.Choose<Sprite>(
			bull_sprs[0], bull_sprs[1],
			bull_sprs[2], bull_sprs[3],
			bull_sprs[4]
		);

		for (int i = 0; i < bulletCount; i++) {
			float angle = i * (360f / bulletCount);

			float radian = angle * Mathf.Deg2Rad;

			Vector2 direction = new Vector2(
				Mathf.Cos(radian),
				Mathf.Sin(radian)
			);

			GameObject bull = Instantiate(bullet_prefab);
			bull.transform.position = transform.position;

			bull.GetComponent<Bullet>().dir = direction;
			bull.GetComponent<Bullet>().speed = 5f;
			bull.GetComponent<SpriteRenderer>().sprite = b;
		}
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
