using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ennek a szkriptnek csak az a feladata, hogy a tartalmazó GameObject-et a nulla pozícióba mozgassa indításkor
/// </summary>

public class SetCenterPos : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        transform.position = Vector3.zero;
	}
}
