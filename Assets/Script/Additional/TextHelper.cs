using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ez az objektum beállítja a text componens szélességét a belehelyezett szövegnek megfelelően.
/// Azért van erre szükség, hogy a Horizontal Layout eltudja megfelelően helyezni.
/// </summary>
public class TextHelper : MonoBehaviour
{
    RectTransform rectTransform;
    Text text;

	// Use this for initialization
	void Awake ()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        text = GetComponent<Text>();
	}

    public void SetText(string text) {
        this.text.text = text;
        rectTransform.sizeDelta = new Vector2(this.text.preferredWidth, rectTransform.sizeDelta.y);
    }
}
