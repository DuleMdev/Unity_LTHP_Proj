using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpecialInput : MonoBehaviour, ITextyInput, IWidthHeight
{
    RectTransform rectTransform;
    TEXDraw texDraw;

    Image imageBackground;          // A gomb képe, időnként át kell színezni, ezért van erre szükség
    Color defaultBackgroundColor;   // Az alapértelmezett háttérszin

    Common.CallBack_In_Object callBack; // Ha megnyomták a gombot meghívjuk az ebbe a változóba megadott függvényt

    [HideInInspector]
    int questionIndex;      // Melyik kérdéshez tartozik a gomb
    [HideInInspector]
    int subQuestionIndex;   // Kérdésen belül melyik alkérdés

    public int GetQuestionIndex() { return questionIndex; }
    public int GetSubQuestionIndex() { return subQuestionIndex; }

    float maxWidth;               // Mennyi lehet a szöveg szélessége maximum

    SpecialInputPopUp specialInputPopUp;




    float textDiffX;             // Mekkora a különbség a szöveg mérete és a komponens mérete között, tehát mennyivel kell nagyobnak lennie a komponensnek a szöveg méretéhez képest
    float textDiffY;

    string initText;

    bool interactable = true;

    // Start is called before the first frame update
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        texDraw = gameObject.SearchChild("TEXDraw").GetComponent<TEXDraw>();

        imageBackground = Common.SearchGameObject(gameObject, "Background").GetComponent<Image>();
        defaultBackgroundColor = imageBackground.color;

        textDiffX = 21; // rectTransform.sizeDelta.x - texDraw.preferredWidth;
        textDiffY = 11; // rectTransform.sizeDelta.y - texDraw.preferredHeight;
    }

    public float Flashing(bool positive)
    {
        // Ha pozitív a villogás, akkor letiltjuk az inputField-et, hogy újra ne lehessen beírni a jó megoldást
        if (positive)
            interactable = false;

        StartCoroutine(FlashingCoroutine((positive) ? Common.MakeColor("#00A400") : Common.MakeColor("#D40000")));

        return 1.2f;
    }

    IEnumerator FlashingCoroutine(Color color)
    {
        for (int i = 0; i < 3; i++)
        {
            imageBackground.color = color;
            yield return new WaitForSeconds(0.2f);

            imageBackground.color = defaultBackgroundColor;
            yield return new WaitForSeconds(0.2f);
        }

        imageBackground.color = color;
    }

    public string GetText()
    {
        return texDraw.text;
    }

    public void SetText(string text)
    {
        texDraw.text = text;
    }

    public void Init(Common.CallBack_In_Object callBack, int questionIndex, int subQuestionIndex, float maxWidth, string initText, SpecialInputPopUp specialInputPopUp)
    {
        this.callBack = callBack;
        this.questionIndex = questionIndex;
        this.subQuestionIndex = subQuestionIndex;
        this.maxWidth = maxWidth;
        this.initText = initText;
        this.specialInputPopUp = specialInputPopUp;

        rectTransform.sizeDelta = new Vector2(2000, rectTransform.sizeDelta.y);

        texDraw.text = specialInputPopUp.GetInitText(initText);

        SetSize();
    }

    public void Interactable(bool interactable)
    {
        this.interactable = interactable;
    }

    public float GetWidth()
    {
        return rectTransform.sizeDelta.x;
    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.y;
    }

    void SetSize()
    {
        rectTransform.sizeDelta = new Vector2(Mathf.Clamp(texDraw.preferredWidth + textDiffX + 1, 50, maxWidth), texDraw.preferredHeight + textDiffY + 1);
    }

    /// <summary>
    /// Rákattintottak az objektumra
    /// </summary>
    public void Click()
    {
        if (interactable)
        {
            specialInputPopUp.Show(initText, GetText(), EndEdit);
        }
    }

    public void EndEdit(string newText)
    {
        texDraw.text = newText;
        SetSize();

        if (callBack != null)
            callBack(this);
    }

    public bool WasAnswer()
    {
        return true;
    }
}
