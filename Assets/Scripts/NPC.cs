using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {

	[SerializeField] GameObject dia_object;
	public string[] lines;
	public Sprite[] imgs;
	Dialogue dia;

	public bool player_in_range = false;

	// Use this for initialization
	void Start () {
		dia = dia_object.GetComponent<Dialogue>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Action() {
		if (!dia_object.activeSelf) {
			dia.lines = lines;
			dia.imgs = imgs;
			dia.StartDialogue ();
		}
	}

	public IEnumerator IAction() {
		if (!dia_object.activeSelf) {
			dia.lines = lines;
			dia.imgs = imgs;
			dia.StartDialogue ();
			yield return new WaitUntil (() => !dia_object.activeSelf);
		}
	}
}
