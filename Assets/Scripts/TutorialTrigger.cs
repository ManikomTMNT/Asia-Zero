using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTrigger : MonoBehaviour {
	
	public Text tutorialText;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player"))
			return;

		tutorialText.gameObject.SetActive(true);
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (!other.CompareTag("Player"))
			return;

		tutorialText.gameObject.SetActive(false);
	}

}
