using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class UIReadTextQuestionNumber : MonoBehaviour {

    Text text;

	// Use this for initialization
	void Awake () {
        text = GetComponent<Text>();
	}

    public void Init(string text) {
        this.text.text = "<b>" + text + ". </b>";
    }

    public void ChangeTextColor(Color color)
    {
        text.color = color;
    }
}
