using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crap : MonoBehaviour {
	public static T Choose<T>(params T[] options) {
		return options [Random.Range (0, options.Length)];
	}
}
