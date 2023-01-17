using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ez az objektum gombokat készít a scrollRect komponenshez.
/// Lesz egy fel és egy le gomb amivel a scrollRect tartalmát tudjuk mozgatni.
/// 
/// Használat:
/// Tegyük valahová a scrollRect mellé.
/// Állítsuk be a scrollRect tulajdonságát arra a scrollRect-re amit mozgatni szeretnénk a segítségével.
/// Az amount az egy kattintásra az elmozdulás mértéke. Ezt tapasztalati uton kell meghatározni.
/// </summary>

public class UIScrollButtons : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform scrollContent;

    public float amount;

    public bool invert = true;  // Megfordítja az irányokat

    public int upButtonMove;
    public int downButtonMove;  

    RectTransform scrollRectTransform;
    RectTransform contentRectTransform;

    bool upEnabled;
    bool downEnabled;

    CanvasGroup upButtonCanvasGroup;
    CanvasGroup downButtonCanvasGroup;

    RectTransform rectUpButton;
    RectTransform rectDownButton;

    float upButtonInitialPosition;
    float downButtonInitialPosition;

    float fadeTime = 0.5f;

    void Awake()
    {
        Debug.Log(Common.GetGameObjectHierarchy(gameObject));

        scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        contentRectTransform = scrollContent != null ? scrollContent : scrollRect.gameObject.SearchChild("Content").GetComponent<RectTransform>();
        //contentRectTransform = scrollRect.gameObject.SearchChild("Content").GetComponent<RectTransform>();

        upButtonCanvasGroup = gameObject.SearchChild("ImageBackgroundUp").GetComponent<CanvasGroup>();
        downButtonCanvasGroup = gameObject.SearchChild("ImageBackgroundDown").GetComponent<CanvasGroup>();

        rectUpButton = upButtonCanvasGroup.gameObject.GetComponent<RectTransform>();
        rectDownButton = downButtonCanvasGroup.gameObject.GetComponent<RectTransform>();

        //upButtonInitialPosition = rectUpButton.anchoredPosition.y;
        downButtonInitialPosition = rectDownButton.localPosition.y;
    }

    public void ButtonClick(string buttonName)
    {
        float amount = 0;
        switch (buttonName)
        {
            case "Up":
                if (upEnabled)
                    amount = invert ? -this.amount : this.amount;
                break;
            case "Down":
                if (downEnabled)
                    amount = invert ? this.amount : -this.amount;
                break;
        }

        scrollRect.verticalNormalizedPosition = Mathf.Clamp(scrollRect.verticalNormalizedPosition, 0.0001f, 0.9999f); //Clamping between 0 and 1 just didn't do...
        scrollRect.velocity = new Vector2(scrollRect.velocity.x, scrollRect.velocity.y + amount);
    }

    void Update()
    {
        upEnabled = true;
        downEnabled = true;
        // Ha a tartalom mérete kisebb mint a scrollRect mérete, akkor nem kell mozgatni a tartalmat
        if (Common.GetRectTransformPixelSize(scrollRectTransform, xNeed: false).y >= Common.GetRectTransformPixelSize(contentRectTransform, xNeed: false).y)
        {
            upEnabled = false;
            downEnabled = false;
        }

        if (scrollRect.verticalNormalizedPosition > 0.9999f)
        {
            if (invert)
                upEnabled = false;
            else
                downEnabled = false;
        }

        if (scrollRect.verticalNormalizedPosition < 0.0001f)
        {
            if (invert)
                downEnabled = false;
            else
                upEnabled = false;
        }

        upButtonCanvasGroup.alpha = upButtonCanvasGroup.alpha + Time.deltaTime / fadeTime * (upEnabled ? 1 : -1);
        downButtonCanvasGroup.alpha = downButtonCanvasGroup.alpha + Time.deltaTime / fadeTime * (downEnabled ? 1 : -1);
    }

    void LateUpdate()
    {
        if (downButtonMove == 0) return;

        float windowSize = Common.GetRectTransformPixelSize(scrollRectTransform, xNeed: false).y;
        float fullSize = Common.GetRectTransformPixelSize(contentRectTransform, xNeed: false).y;
        float different = fullSize - windowSize;

        if (different <= 0) return; // Ha nincs mit mozgatni a scroll ablakban, mert az ablak nagyobb mint a tartalom, akkor kilépünk
        
        // mekkora terület van még az ablak alatt (pixelben)
        float underwindowSize = different * scrollRect.verticalNormalizedPosition;

        if (underwindowSize < downButtonMove)
            //rectDownButton.anchoredPosition.Set(rectDownButton.anchoredPosition.x, downButtonInitialPosition + downButtonMove - underwindowSize);
            rectDownButton.localPosition = rectDownButton.localPosition.SetY(downButtonInitialPosition + downButtonMove - underwindowSize);
        else 
            rectDownButton.localPosition = rectDownButton.localPosition.SetY(downButtonInitialPosition);
    }
}
