
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	BoxCollider2D boxcollider;
	Rigidbody2D rb;
	Animator animator;
	SpriteRenderer sprite;

	PlayerAttackHitbox atk_hitbox;

	public bool can_move = true;

	int hp = 5;
	float invulnera_time = 0.0f;

	float hmovement;
	float atk_range = 0.55f;

	[SerializeField] float movement_spd = 6;
	[SerializeField] float jump_force = 14;

	bool is_bouncing = false;
	bool is_charging_bounce = false;
	Vector2 bounce_direction;
	[SerializeField] float bounce_force = 15f;
	PlayerBounceHitbox bounce_hitbox;

	[SerializeField] float bounce_decay = 0.15f;
	[SerializeField] float bounce_decay_increase = 0.08f;
	float bounce_min_speed = 5f;
	[SerializeField] float bounce_rebound_force = 15f;

	float current_bounce_decay;

	[SerializeField] float coyote_time = 0.2f;
	[SerializeField] float jump_buffer_time = 0.15f;

	float coyote_timer;
	float jump_buffer_timer;

	bool ground_touch = false;
	bool has_air_dashed = false;

	GameObject bouncernouncer;


	int dir = 1;

	float dash_force = 12f;
	float dash_duration = 0.225f;
	float dash_cooldown = 0.5f;

	bool is_dashing = false;
	float dash_timer = 0f;
	float dash_cooldown_timer = 0f;

	Vector2 dash_direction;

	static PlayerMovement instance;

	void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		} else if (instance != this) {
			Destroy(gameObject);
		}
	}

	void Start() {
		boxcollider = GetComponent<BoxCollider2D>();
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator> ();
		sprite = GetComponent<SpriteRenderer> ();

		atk_hitbox = transform.Find("AttackHitbox").GetComponent<PlayerAttackHitbox>();
		bouncernouncer = transform.Find("BounceArrowDisp").gameObject;
		bounce_hitbox =  transform.Find("BounceHitbox").GetComponent<PlayerBounceHitbox>();
		bouncernouncer.SetActive (false);
	}

	public void GetHit(int dmg) {
		if (invulnera_time <= 0 && !is_bouncing && !is_dashing) {
			invulnera_time = 3.0f;
			hp -= dmg;
		}
		Debug.Log (hp);
	}

	void Update() {
		invulnera_time -= Time.deltaTime;

		if (invulnera_time >= 0) {
			sprite.color = new Color (1f, 1f, 1f, 0.5f);
		} else {
			sprite.color = new Color (1f, 1f, 1f, 1f);
		}

		if (Input.GetButtonDown("Jump") && !is_bouncing) {
			jump_buffer_timer = jump_buffer_time;
		} else {
			jump_buffer_timer -= Time.deltaTime;
		}

		if (Input.GetButtonDown("Attack") && !is_bouncing) {
			atk_hitbox.Attack ();
			animator.SetTrigger ("atk");
		}

		if (Input.GetButtonDown("Dash") && dash_cooldown_timer <= 0 && !is_dashing && !is_bouncing) {
			if (ground_touch || !has_air_dashed) {
				StartDash();
			}
		}

		if (Input.GetButtonDown("Bounce") && !is_bouncing) {
			is_charging_bounce = true;
		}

		if (Input.GetButton("Bounce") && is_charging_bounce) {
			rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.065f);
			bouncernouncer.SetActive(true);

			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");

			if (h == 0 && v == 0) {
				h = dir;
			}

			float angle = Mathf.Atan2(v, h) * Mathf.Rad2Deg;
			bouncernouncer.transform.rotation = Quaternion.Lerp (bouncernouncer.transform.rotation, Quaternion.Euler (0, 0, angle), 0.065f);
		}

		if (Input.GetButtonUp("Bounce") && is_charging_bounce) {
			is_charging_bounce = false;
			is_bouncing = true;

			bounce_direction = bouncernouncer.transform.right;
			rb.velocity = bounce_direction * bounce_force;

			current_bounce_decay = bounce_decay;
			bounce_hitbox.can_atk = true;

			bouncernouncer.SetActive(false);
		}

		if (Input.GetButtonDown("Bounce") && is_bouncing) {
			bounce_hitbox.can_atk = false;
			is_bouncing = false;
		}
	}

	void FixedUpdate() {
		if (is_bouncing) {
			float speed = rb.velocity.magnitude;

			// stop if too slow
			if (speed <= bounce_min_speed) {
				is_bouncing = false;
				bounce_hitbox.can_atk = false;
			} else {
				// reduce velocity
				rb.velocity = Vector2.MoveTowards(
					rb.velocity,
					Vector2.zero,
					current_bounce_decay * Time.fixedDeltaTime
				);

				// mooore decay over time
				current_bounce_decay += bounce_decay_increase * Time.fixedDeltaTime;

				return;
			}
		}

		dash_cooldown_timer -= Time.fixedDeltaTime;

		if (is_dashing) {
			Dash();
			return;
		}

		hmovement = Input.GetAxis("Horizontal");
		dir = (hmovement > 0) ? 1 : (hmovement < 0) ? -1 : dir;
		sprite.flipX = (dir == -1);

		if (can_move && !is_bouncing && !is_charging_bounce) {
			rb.velocity = new Vector2 (
				hmovement * movement_spd,
				rb.velocity.y
			);
		} else {
			rb.velocity = new Vector2 (
				0,
				rb.velocity.y
			);
		}

		// floor crap
		RaycastHit2D raycast_hit = Physics2D.BoxCast(
			boxcollider.bounds.center,
			boxcollider.bounds.size,
			0f,
			Vector2.down,
			0.1f
		);

		ground_touch = raycast_hit.collider != null;
		ground_touch = ground_touch && (!raycast_hit.collider.CompareTag("NPC")) && (!raycast_hit.collider.CompareTag("Enemy")) && (!raycast_hit.collider.CompareTag("Boss")); // make sure you cant jump on npc's!

		if (ground_touch) {
			has_air_dashed = false;
		}

		// Jump
		if (ground_touch) {
			coyote_timer = coyote_time;
		} else {
			coyote_timer -= Time.fixedDeltaTime;
		}

		if (jump_buffer_timer > 0 && coyote_timer > 0 && can_move && !is_bouncing) {
			rb.velocity = new Vector2(rb.velocity.x, jump_force);

			jump_buffer_timer = 0;
			coyote_timer = 0;
		}

		// Atk box
		float py = atk_hitbox.transform.localPosition.y;
		if (dir == 1) {
			atk_hitbox.transform.localPosition = new Vector2(atk_range, py);
		} else if (dir == -1) {
			atk_hitbox.transform.localPosition = new Vector2(-atk_range, py);
		}

		animator.SetFloat("vel", Mathf.Abs(rb.velocity.x));
		animator.SetBool("grounded", ground_touch);
		animator.SetFloat("yvel", Mathf.Round(rb.velocity.y * 80f) / 80f);
	}

	void StartDash() {
		if (can_move) {
			if (!ground_touch) {
				has_air_dashed = true;
			}

			float x = Input.GetAxisRaw("Horizontal");
			float y = Input.GetAxisRaw("Vertical");

			if (x == 0 && y == 0) {
				x = dir;
			}

			dash_direction = new Vector2(x, y).normalized;

			is_dashing = true;
			dash_timer = dash_duration;
			dash_cooldown_timer = dash_cooldown;

			rb.velocity = dash_direction * dash_force;
		}
	}

	void Dash() {
		if (!is_dashing) return;

		dash_timer -= Time.fixedDeltaTime;

		rb.velocity = dash_direction * dash_force;

		if (dash_timer <= 0) {
			is_dashing = false;
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (is_bouncing) {
			ContactPoint2D contact = collision.contacts[0];

			bounce_direction = Vector2.Reflect(
				bounce_direction,
				contact.normal
			).normalized;

			rb.velocity = bounce_direction * bounce_rebound_force;
			current_bounce_decay -= bounce_decay;
		}

		if (collision.gameObject.CompareTag ("Enemy") || collision.gameObject.CompareTag ("Boss")) {
			GetHit (1);
		}
	}	

	public void EnemRebound(Vector2 contact) {
		Vector2 normal = (contact - (Vector2)transform.position).normalized;

		bounce_direction = Vector2.Reflect(
			bounce_direction,
			normal
		).normalized;

		rb.velocity = bounce_direction * bounce_rebound_force;
		current_bounce_decay -= bounce_decay;
	}
}
