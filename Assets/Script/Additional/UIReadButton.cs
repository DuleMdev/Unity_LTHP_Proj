using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System;
using System.Collections.Generic;

using System.Text.RegularExpressions;

public class UIReadButton : MonoBehaviour, IWidthHeight  {

    enum ButtonColorType
    {
        selected, deselected, answer,
    }

    public Color selectedColor;
    public Color deselectedColor;
    public Color answerColor;

    ButtonColorType buttonColorType;

    RectTransform rectTransform;    // A szkriptet tartalmazó object RectTransformja, méretezni kell a szöveghez
    Text text;                      // A szöveget megjelenítő komponens

    RectTransform rectTransformButton;  // Ez az objektum adja a gomb valódi szélességét
    Image imageButton;          // A gomb képe, időnként át kell színezni, ezér van erre szükség
    Image imageShadow;          // A gomb árnyékát tartalmazó image, kérdés gombnál nem kell árnyék

    bool isQuestion;            // A gomb kérdés vagy válasz?

    Common.CallBack_In_Object callBack; // Ha megnyomták a gombot meghívjuk az ebbe a változóba megadott függvényt

    [HideInInspector]
    public int questionIndex;      // Melyik kérdéshez tartozik a gomb
    [HideInInspector]
    public int subQuestionIndex;   // Kérdésen belül melyik alkérdés

    float textDiff;     // Mekkora a különbség a szöveg mérete és a komponens mérete között, tehát mennyivel kell nagyobnak lennie a komponensnek a szöveg méretéhez képest

    string goodAnswer;          // A helyes válasz
    //List<string> answers;       // A lehetséges válaszok, az első a helyes
    List<string> answersMixed;  // A lehetséges válaszok megkeverve (csak egyszer keverjük meg a továbbiakban azt használjuk)

    // Use this for initialization
    void Awake () {
        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();

        rectTransformButton = Common.SearchGameObject(gameObject, "Button").GetComponent<RectTransform>();
        imageButton = Common.SearchGameObject(gameObject, "Button").GetComponent<Image>();
        imageShadow = Common.SearchGameObject(gameObject, "Shadow").GetComponent<Image>();

        textDiff = rectTransform.sizeDelta.x - text.preferredWidth;

        //Init("?(mogyorósövény:várárok|tujasor|virágágyás)");
	}

    // Létrehozunk egy kérdés gombot
    public void Init(List<string> texts, Common.CallBack_In_Object callBack, int questionIndex, int subQuestionIndex)
    {
        this.callBack = callBack;
        this.questionIndex = questionIndex;
        this.subQuestionIndex = subQuestionIndex;
        isQuestion = true;

        // Ha kérdésgombról van szó, akkor kikapcsoljuk az árnyékot
        imageShadow.enabled = false;

        // Válaszok meghatározása
        goodAnswer = texts[0]; // A válaszok közül az első a helyes
        answersMixed = new List<string>(texts);
        answersMixed.Shuffle(); // Megkeverjük a válaszokat

        // Megnézzük, hogy melyik szöveg a legszélesebb és ahhoz állítjuk be a gomb szélességét
        TextGenerationSettings settings = new TextGenerationSettings();
        settings.richText = text.supportRichText;
        settings.font = text.font;
        settings.fontSize = text.fontSize;
        settings.fontStyle = text.fontStyle;
        // settings.generationExtents = new Vector2(500.0F, 200.0F);
        TextGenerator generator = new TextGenerator();

        float maxWidth = float.MinValue;
        string longestText = "";
        foreach (string textItem in texts)
        {
            float actWidth = generator.GetPreferredWidth(textItem, settings);

            if (actWidth > maxWidth) { // Ha a mostani szöveg szélessége nagyobb mint a korábbiak
                maxWidth = actWidth;
                longestText = textItem;
            }
        }

        CreateButton(longestText);

        text.text = texts[0]; // Beállítjuk a helyes szöveget a gombon
        text.enabled = false; // Eltüntetjük a szöveget

        Selected(false);
    }

    // Létrehozunk egy válasz gombot
    public void Init(string text, Common.CallBack_In_Object callBack)
    {
        this.callBack = callBack;
        isQuestion = false;

        CreateButton(text);

        buttonColorType = ButtonColorType.answer;
        Coloring();
    }

    // Beállítja a gombot a megadott szöveg szélességűre
    void CreateButton(string text) {
        this.text.text = text;
        rectTransform.sizeDelta = new Vector2(this.text.preferredWidth + textDiff, rectTransform.sizeDelta.y);
    }

    public void Set(Sprite shadow, Color shadowColor, Sprite button, Color selectedColor, Color deselectedColor, Color answerColor, Color textColor) {
        imageShadow.sprite = shadow;
        imageShadow.color = shadowColor;
        imageButton.sprite = button;
        this.selectedColor = selectedColor;
        this.deselectedColor = deselectedColor;
        this.answerColor = answerColor;
        text.color = textColor;

        Coloring();
    }

    /// <summary>
    /// Kiszínezi a gombot a ButtonColorType szerinti színre.
    /// </summary>
    void Coloring() {
        switch (buttonColorType)
        {
            case ButtonColorType.selected:
                imageButton.color = selectedColor;
                break;

            case ButtonColorType.deselected:
                imageButton.color = deselectedColor;
                break;

            case ButtonColorType.answer:
                imageButton.color = answerColor;
                break;
        }
    }

    /// <summary>
    /// Question button esetében két állapot van egy kiválasztott és egy nem kiválasztott, mikor is más a színe
    /// a gombnak.
    /// </summary>
    /// <param name="select">Ki van választva a gomb, true ha igen.</param>
    public void Selected(bool select)
    {
        // Ha már a szöveg megjelent, akkor answerColor színünek kell maradnia
        if (!text.enabled)
        {
            buttonColorType = (select) ? ButtonColorType.selected : ButtonColorType.deselected;
            Coloring();
        }
    }

    /// <summary>
    /// A gomb szövegét megjeleníti Question button esetében van erre szükség.
    /// </summary>
    public void ShowText(string t = null) {
        if (t != null)
            text.text = t;
        text.enabled = true;

        buttonColorType = ButtonColorType.answer;
        Coloring();
        //imageButton.color = answerColor;
    }


    public float Flashing(bool positive)
    {
        StartCoroutine(FlashingCoroutine((positive) ? Color.green : Color.red));

        return 1.2f;
    }

    IEnumerator FlashingCoroutine(Color color)
    {
        //Color defaultColor = imageButton.color;

        for (int i = 0; i < 3; i++)
        {
            imageButton.color = color;
            yield return new WaitForSeconds(0.2f);

            Coloring();
            //imageButton.color = defaultColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// Vissza adja a gombon található szöveget.
    /// </summary>
    /// <returns>A gombon található szöveg</returns>
    public string GetText() {
        return text.text;
    }

    public List<string> GetAnswers()
    {
        return new List<string>(answersMixed);
    }

    public void ButtonClick() {
        if (callBack != null && (!isQuestion || !text.enabled))
            callBack(this);
    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.x + rectTransformButton.sizeDelta.y;
    }

    public float GetWidth()
    {
        return rectTransform.sizeDelta.x + rectTransformButton.sizeDelta.x;
    }
}
