using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour {

	[SerializeField] bool debuggo = false;

	// Use this for initialization
	void Start () {
		GetComponent<SpriteRenderer> ().enabled = debuggo;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
