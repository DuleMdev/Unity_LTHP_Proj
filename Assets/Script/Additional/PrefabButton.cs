using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PrefabButton : MonoBehaviour {

    Text text;
    RectTransform transformImageBorder;
    Image imageBorder;

    float textAndImageDiff;

	// Use this for initialization
	void Awake ()
    {
        text = transform.Find("Text").GetComponent<Text>();
        transformImageBorder = transform.Find("ImageBorder").GetComponent<RectTransform>();
        imageBorder = transformImageBorder.GetComponent<Image>();

        textAndImageDiff = transformImageBorder.sizeDelta.x - this.text.preferredWidth;
    }

    public void Initialize(string text) {
        Initialize(text, Color.black, Color.black);
    }

    public void Initialize(string text, Color textColor, Color borderColor) {
        this.text.text = text;
        this.text.color = textColor;
        imageBorder.color = borderColor;

        transformImageBorder.sizeDelta = new Vector2(this.text.preferredWidth + textAndImageDiff, transformImageBorder.sizeDelta.y);
    }
}
