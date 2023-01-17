using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class UIPanelUserName : MonoBehaviour {

    Image imageColor;
    Text text;

    // Use this for initialization
    void Awake()
    {
        imageColor = GetComponent<Image>();
        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetColor(Color color)
    {
        imageColor.color = color;
    }
}
