using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour {
	[SerializeField] float smoothSpeed = 5f;
	[SerializeField] Vector3 offset = new Vector3(0, 0, -10);

	[Header("Camera Limits")]
	public float minX = -10f;
	public float maxX = 10f;
	public float minY = -5f;
	public float maxY = 5f;

	private Transform player;

	void Start()
	{
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

		if (playerObject != null) {
			player = playerObject.transform;
		}
	}

	void LateUpdate()
	{
		if (player == null) return;

		Vector3 targetPosition = player.position + offset;

		float clampedX = Mathf.Clamp(targetPosition.x, minX, maxX);
		float clampedY = Mathf.Clamp(targetPosition.y, minY, maxY);

		Vector3 targ = new Vector3 (clampedX, clampedY, offset.z);


		transform.position = Vector3.Lerp(
			transform.position,
			targ,
			smoothSpeed * Time.deltaTime
		);
	}

}
