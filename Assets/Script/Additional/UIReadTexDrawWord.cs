using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReadTexDrawWord : MonoBehaviour, IWidthHeight
{

    RectTransform rectTransform;    // A szkriptet tartalmazó object RectTransformja, méretezni kell a szöveghez
    TEXDraw text;                      // A szöveget megjelenítő komponens

    // Use this for initialization
    void Awake()
    {
        text = GetComponent<TEXDraw>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Init(string text)
    {
        // Előfordul, hogy amikor az Instantiate utasítással létrehozzuk egy gameObject-et nem hívódik meg az Awake metódusa
        // Ezért megvizsgáljuk az egyik komponens referenciáját, hogy meg van-e már, ha nincs, akkor elvileg nem volt még 
        // meghívva az Awake, ezért meghívjuk
        if (this.text == null)
            Awake();

        this.text.text = text;

        rectTransform.sizeDelta = new Vector2(this.text.preferredWidth + 1, this.text.preferredHeight + 1);
    }

    public void ChangeTextColor(Color color)
    {
        text.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        //rectTransform.sizeDelta = new Vector2(this.text.preferredWidth, text.preferredHeight);
    }

    public float GetHeight()
    {
        return ((RectTransform)transform).sizeDelta.y;
    }

    public float GetWidth()
    {
        return ((RectTransform)transform).sizeDelta.x;
    }
}
