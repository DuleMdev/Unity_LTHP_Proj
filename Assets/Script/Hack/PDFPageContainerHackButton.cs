using Paroxe.PdfRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PDFPageContainerHackButton : MonoBehaviour
{
    RectTransform rectTransformPageContainer;
    RectTransform rectTransform;

    Text textButtonNext;

    void Awake()
    {
        rectTransformPageContainer = GetComponentsInParent<PDFViewer>(true)[0].m_Internal.m_PageContainer;
        rectTransform = GetComponent<RectTransform>();

        textButtonNext = gameObject.SearchChild("TextNext").GetComponent<Text>();
    }

    void LateUpdate()
    {
        rectTransform.sizeDelta = rectTransformPageContainer.sizeDelta;
        rectTransform.anchoredPosition = rectTransformPageContainer.anchoredPosition;
    }

    void OnEnable()
    {
        // Az OnEnable nagyon hamar meghívódik, lehetséges, hogy megelőzi más szkriptek Awake metódusát
        // így nem lesz még a Common.languageController változó kitöltve
        try
        {
            textButtonNext.text = Common.languageController.Translate((GameMenu.instance.isNextButtonActive) ? C.Texts.nextGame : C.Texts.exit);
        }
        catch (System.Exception)
        {
        }
    }

    public void ButtonClick(string buttonName)
    {
        GameMenu.instance.ButtonClick(buttonName);
    }
}
