using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class UIText : MonoBehaviour {

    Text text;

	// Use this for initialization
	void Awake () {
        text = GetComponentInChildren<Text>(true);
	}

    public void SetText(string text) {
        this.text.text = text;
    }
}
