using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScrollView componens Content RectTransform méretét állítja be a content tartalmának megfelelően.
/// 
/// Ki lehet választani, hogy vízszintesen vagy függőlegesen állítsa be a méretet.
/// Majd a script megvizsgálja, minden Update-ben a content tartalmát és megnézi, hogy melyik Recttranform fejeződik be legbalrább 
/// vagy leglejjebb és annak megfelelően állítja be a content méretét miután hozzáadott egy esetleges margót is.
/// </summary>
public class UIScrollViewContentSizeSetter : MonoBehaviour
{
    public bool setX = true;
    public float minSize;
    public float margoX;
    public float margoY;

    ScrollRect scrollRect;

    // Start is called before the first frame update
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    void ResizeContentSize()
    {
        float xMax = 0;
        float yMin = 0;

        // Végig megyünk a Content
        if (scrollRect.content.childCount > 0)
        {
            RectTransform lastTransform = (RectTransform)scrollRect.content.GetChild(scrollRect.content.childCount - 1);

            xMax = lastTransform.anchoredPosition.x + lastTransform.rect.width + margoX;
            yMin = -lastTransform.anchoredPosition.y + lastTransform.rect.height + margoY;
        }

        if (setX)
            scrollRect.content.sizeDelta = new Vector2(xMax < minSize ? minSize : xMax, scrollRect.content.sizeDelta.y);
        else
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, yMin < minSize ? minSize : yMin);
    }

    // Update is called once per frame
    void Update()
    {
        ResizeContentSize();
    }
}
