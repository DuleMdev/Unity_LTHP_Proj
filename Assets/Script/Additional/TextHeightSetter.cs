using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Beállítja a text komponens magasságát a preferált méretnek megfelelően.
/// Plusz hozzáad egy megadott értéket ha szükséges.
/// Ha megadjuk a parent RectTransformot, akkor a megadott RectTransform méretét fogja beállítani.
/// A minHeight paraméter megasásával a minimális méretet tudjuk meghatározni.
/// </summary>

public class TextHeightSetter : MonoBehaviour
{
    public float margo;
    public float minHeight;
    public RectTransform parent;

    Text text;
    RectTransform rectTransform;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();

        if (parent != null)
            rectTransform = parent;
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Max(text.preferredHeight + margo, minHeight));
    }
}
