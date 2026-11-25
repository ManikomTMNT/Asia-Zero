using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDoor : MonoBehaviour {

	public string sceneo;
	public string spawno = "PlayerSpawn";

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D collision) {
		if (collision.CompareTag("Player")) {
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.LoadScene(sceneo);
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		GameObject spawn = GameObject.Find(spawno);

		if (spawn != null) {
			GameObject player = GameObject.FindWithTag("Player");

			if (player != null) {
				player.transform.position = spawn.transform.position;
			}
		}

		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
