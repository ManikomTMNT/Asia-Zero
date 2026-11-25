using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
	public string targetTag = "Player";
	public bool triggerOnce = false;

	public UnityEvent onTriggerEnter;
	public UnityEvent onTriggerExit;

	private bool hasTriggered = false;

	private void Reset()
	{
		GetComponent<Collider2D>().isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (triggerOnce && hasTriggered) return;

		if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
		{
			hasTriggered = true;
			onTriggerEnter.Invoke();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
		{
			onTriggerExit.Invoke();
		}
	}
}
