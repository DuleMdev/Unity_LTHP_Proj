using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Beállítja a rectTransform méretét a benne található elemek méretéhez.
/// 
/// Ezt úgy teszi, hogy megnézi az utolsó elemet a hierarhiában és azt tekinti a leghátsónak.
/// </summary>

public class UIRectTransformSizeSetter : MonoBehaviour
{
    public bool setX = true;
    public float minSize;
    public float margoX;
    public float margoY;

    RectTransform rectTransform;

    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    void ResizeContentSize()
    {
        // Végig megyünk a Content
        RectTransform lastTransform = (RectTransform)rectTransform.GetChild(rectTransform.childCount - 1);

        float xMax = lastTransform.anchoredPosition.x + lastTransform.rect.width + margoX;
        float yMin = -lastTransform.anchoredPosition.y + lastTransform.rect.height + margoY;
        if (setX)
            rectTransform.sizeDelta = new Vector2(xMax < minSize ? minSize : xMax, rectTransform.sizeDelta.y);
        else
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, yMin < minSize ? minSize : yMin);

        //if (setX)
        //    scrollRect.content.sizeDelta = new Vector2(lastTransform.anchoredPosition.x + lastTransform.rect.width + margoX, scrollRect.content.sizeDelta.y);
        //else
        //    scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, -lastTransform.anchoredPosition.y + lastTransform.rect.height + margoY);
    }

    // Update is called once per frame
    void Update()
    {
        ResizeContentSize();
    }
}
