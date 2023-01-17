using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System;
using System.Collections.Generic;

using System.Text.RegularExpressions;

public class UIRead2InputField : MonoBehaviour, IWidthHeight  {

    enum ButtonColorType
    {
        selected, deselected, answer,
    }

    public Color selectedColor;
    public Color deselectedColor;
    public Color answerColor;

    ButtonColorType buttonColorType;

    RectTransform rectTransform;    // A szkriptet tartalmazó object RectTransformja, méretezni kell a szöveghez
    Text textTest;                  // Ennek a komponensnek a segítségével tudjuk meg, hogy mennyi helyre van szüksége a szövegnek
    InputField inputField;          // Ide írjuk a választ

    //RectTransform rectTransformButton;  // Ez az objektum adja a gomb valódi szélességét
    Image imageBackground;          // A gomb képe, időnként át kell színezni, ezér van erre szükség
    Color defaultBackgroundColor;   // Az alapértelmezett háttérszin

    Common.CallBack_In_Object callBack; // Ha megnyomták a gombot meghívjuk az ebbe a változóba megadott függvényt

    [HideInInspector]
    public int questionIndex;      // Melyik kérdéshez tartozik a gomb
    [HideInInspector]
    public int subQuestionIndex;   // Kérdésen belül melyik alkérdés

    float maxWidth;               // Mennyi lehet a szöveg szélessége maximum

    float textDiff;             // Mekkora a különbség a szöveg mérete és a komponens mérete között, tehát mennyivel kell nagyobnak lennie a komponensnek a szöveg méretéhez képest

    List<string> answers;       // Az elfogadható válaszok

    string previousText;        // Az inputFieldben található korábbi szöveg, hogy a szöveg változást érzékelni tudjuk

    // Ki kell-e értékelni a választ
    // Előfordulhat, hogy az inputField-ből kitörölték a szöveget, tehát nem írtak bele újat, ezért nem kell kiértékelni a választ
    // viszont igazítani szükséges az elemeket hiszen az inputField keskenyebb lett
    // Persze az is előfordulhat, hogy megváltoztatták a szöveget és ki kell értékelni, ilyenkor az alábbi változó igaz értéket tartaalmaz
    [HideInInspector]
    public bool wasAnswer;      // Ki kell-e értékelni a választ

    // Use this for initialization
    void Awake () {
        textTest = Common.SearchGameObject(gameObject, "TextTest").GetComponent<Text>();
        inputField = Common.SearchGameObject(gameObject, "InputField").GetComponent<InputField>();
        rectTransform = GetComponent<RectTransform>();

        //rectTransformButton = Common.SearchGameObject(gameObject, "Button").GetComponent<RectTransform>();
        imageBackground = Common.SearchGameObject(gameObject, "Background").GetComponent<Image>();
        defaultBackgroundColor = imageBackground.color;

        textDiff = rectTransform.sizeDelta.x - textTest.preferredWidth;
	}

    // Létrehozunk egy kérdés gombot
    public void Init(List<string> texts, Common.CallBack_In_Object callBack, int questionIndex, int subQuestionIndex, float maxWidth)
    {
        // Előfordul, hogy amikor az Instantiate utasítással létrehozzuk az input mezőt nem hívódik meg az Awake metódusa
        // Ezért megvizsgáljuk az egyik komponens referenciáját, hogy meg van-e már, ha nincs, akkor elvileg nem volt még 
        // meghívva az Awake, ezért meghívjuk
        if (textTest == null)
            Awake();

        this.callBack = callBack;
        this.questionIndex = questionIndex;
        this.subQuestionIndex = subQuestionIndex;
        this.maxWidth = maxWidth;

        previousText = "";
        SetWidth();

//        // Megnézzük, hogy melyik szöveg a legszélesebb és ahhoz állítjuk be a gomb szélességét
//        TextGenerationSettings settings = new TextGenerationSettings();
//        settings.richText = text.supportRichText;
//        settings.font = text.font;
//        settings.fontSize = text.fontSize;
//        settings.fontStyle = text.fontStyle;
//        // settings.generationExtents = new Vector2(500.0F, 200.0F);
//        TextGenerator generator = new TextGenerator();
//
//        float maxWidth = float.MinValue;
//        string longestText = "";
//        foreach (string textItem in texts)
//        {
//            float actWidth = generator.GetPreferredWidth(textItem, settings);
//
//            if (actWidth > maxWidth) { // Ha a mostani szöveg szélessége nagyobb mint a korábbiak
//                maxWidth = actWidth;
//                longestText = textItem;
//            }
//        }
    }

    // Beállítja a gombot a inputField-ben található szöveg szélességére megadott szöveg szélességűre
    public void SetWidth() {
        textTest.text = inputField.text;
        rectTransform.sizeDelta = new Vector2(Mathf.Clamp(textTest.preferredWidth + textDiff, 120, maxWidth), rectTransform.sizeDelta.y);
    }

    /// <summary>
    /// Kiszínezi a gombot a ButtonColorType szerinti színre.
    /// </summary>
    void Coloring()
    {
//        switch (buttonColorType)
//        {
//            case ButtonColorType.selected:
//                imageButton.color = selectedColor;
//                break;
//
//            case ButtonColorType.deselected:
//                imageButton.color = deselectedColor;
//                break;
//
//            case ButtonColorType.answer:
//                imageButton.color = answerColor;
//                break;
//        }
    }

    /// <summary>
    /// Question button esetében két állapot van egy kiválasztott és egy nem kiválasztott, mikor is más a színe
    /// a gombnak.
    /// </summary>
    /// <param name="select">Ki van választva a gomb, true ha igen.</param>
    public void Selected(bool select)
    {
        buttonColorType = (select) ? ButtonColorType.selected : ButtonColorType.deselected;
        Coloring();
    }

//    /// <summary>
//    /// A gomb szövegét megjeleníti Question button esetében van erre szükség.
//    /// </summary>
//    public void ShowText() {
//        text.enabled = true;
//
//        buttonColorType = ButtonColorType.answer;
//        Coloring();
//        //imageButton.color = answerColor;
//    }


    public float Flashing(bool positive)
    {
        // Ha pozitív a villogás, akkor letiltjuk az inputField-et, hogy újra ne lehessen beírni a jó megoldást
        if (positive)
            inputField.interactable = false;

        StartCoroutine(FlashingCoroutine((positive) ? Common.MakeColor("#00A400") : Common.MakeColor("#D40000")));

        return 1.2f;
    }

    public void Interactable(bool interactable)
    {
        inputField.interactable = interactable;
    }

    IEnumerator FlashingCoroutine(Color color)
    {
        for (int i = 0; i < 3; i++)
        {
            imageBackground.color = color;
            yield return new WaitForSeconds(0.2f);

            Coloring();
            imageBackground.color = defaultBackgroundColor;
            yield return new WaitForSeconds(0.2f);
        }

        imageBackground.color = color;
    }

    /// <summary>
    /// Vissza adja a gombon található szöveget.
    /// </summary>
    /// <returns>A gombon található szöveg</returns>
    public string GetText() {
        return inputField.text;
    }

//    public void ButtonClick() {
//        if (callBack != null)
//            callBack(this);
//    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.x;
    }

    public float GetWidth()
    {
        return rectTransform.sizeDelta.x;
    }

    public void EndEdit()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
            inputField.text = "";

        SetWidth(); // Beállítjuk az inputField szélességét a benne levő szövegnek megfelelően

        // Ha nem változtatták meg a szöveget vagy a szöveg üres stringet tartalmaz, akkor nem kell kiértékelni az eredményt
        wasAnswer = inputField.text != previousText && !string.IsNullOrWhiteSpace(inputField.text);

        previousText = inputField.text;

        if (callBack != null)
            callBack(this);
    }


}
