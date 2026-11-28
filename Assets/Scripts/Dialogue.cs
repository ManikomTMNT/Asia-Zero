using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour {
	public Text txt;
	public string[] lines;
	public Sprite[] imgs;
	public Sprite[] imgs_r;
	public UnityEngine.UI.Image imger;
	public UnityEngine.UI.Image imger_r;
	float text_speed = 0.04f;

	GameObject player;

	int index;

	// Use this for initialization
	void Start () {
		gameObject.SetActive (false);
		imger.gameObject.SetActive (false);
		imger_r.gameObject.SetActive (false);
		player = GameObject.FindGameObjectWithTag ("Player");
		// StartDialogue ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Confirm")) {
			if (txt.text == lines [index]) {
				NextLine ();
			} else {
				StopAllCoroutines ();
				txt.text = lines [index];
			}
		}
	}

	public void StartDialogue() {
		gameObject.SetActive (true);
		imger.gameObject.SetActive (true);
		imger_r.gameObject.SetActive (true);
		player.GetComponent<PlayerMovement> ().can_move = false;
		txt.text = "";
		index = -1;
		NextLine ();
	}

	IEnumerator TypeLine() {
		foreach (char c in lines[index].ToCharArray()) {
			txt.text += c;
			yield return new WaitForSeconds (text_speed);
		}
	}

	void NextLine() {
		if (index < lines.Length - 1) {
			index++;

			if (imgs [index] != imger.sprite) {
				imger.sprite = imgs [index];
				imger.GetComponent<Animator> ().Play ("DialogueSlideIn", -1, 0f);
			}
			if (imgs_r [index] != imger_r.sprite) {
				imger_r.sprite = imgs_r [index];
				imger_r.GetComponent<Animator> ().Play ("DialogueSlideInR", -1, 0f);
			}

			txt.text = "";
			StartCoroutine (TypeLine ());
		} else {
			gameObject.SetActive(false);
			imger.gameObject.SetActive (false);
			imger_r.gameObject.SetActive (false);
			player.GetComponent<PlayerMovement> ().can_move = true;
		}
	}
}
