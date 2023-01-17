using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System;
using System.Collections.Generic;

using System.Text.RegularExpressions;

/// <summary>
/// A számok háromkarakterenkénti tagolása:
/// Ha a begépelt szöveg szám mondjuk 1234.1234 akkor azt három karakterenként tagolja 1 234.123 4
/// A szóközöket nem tartalmazza az eredmény szöveg mivel az csak formázási célt szolgál.
/// 
/// Hogyan?
/// Különbséget kell tenni a között, hogy töröltünk a szövegből vagy hozzáadtunk egy új karaktert.
/// Törlés:
/// 1. Ha az előző szöveg szám volt, akkor töröljük az összes szóközt.
/// 2. Megvizsgáljuk, hogy a kapott szöveg számot tartalmaz-e.
/// 3. Ha szám, akkor hármassával tagoljuk.
/// 
/// Hozzáadás/beszúrás:
/// 1. Carret position-ból tudjuk, hogy hová történt az új karakter beszúrása.
/// Ha az előző szöveg szám volt, akkor a karakter előtti és utáni szövegből töröljük a szóközöket.
/// Az új szöveg karakter előtti szöveg szóközmentesítve + új karakter + karakter utáni szöveg szóközmentesítve.
/// 2. Megvizsgáljuk, hogy a kapott szöveg számot tartalmaz-e.
/// 3. Ha szám, akkor hármassával tagoljuk.
/// 
/// A műveletek közben figyelni kell, hogy a caret pos, hogyan változik.
/// Mikor vissza írjuk az inputField-be a módosított szöveget, utána a karakter pozíciót is megfelelően be kell állítani.
/// 
/// 
/// </summary>



public class UITextyInputField : MonoBehaviour, ITextyInput, IWidthHeight {

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

    float textDiff;             // Mekkora a különbség a szöveg mérete és a komponens mérete között, tehát mennyivel kell nagyobnak lennie a komponensnek a szöveg méretéhez képest

    List<string> answers;       // Az elfogadható válaszok

    string previousEditText;    // Az inputFieldben található editálás megkezdése előtti szöveg, hogy a szöveg változást érzékelni tudjuk

    string inputText;           // Milyen szöveg van az inputField-ben. (A nyers szöveg ami még nincs szóközökkel tagolva)
    string inputTextWithSpace;  // Milyen szöveg van az inputField-ben esetleg az automatikusan beírt szóközökkel együtt.
    char? pressedChar;          // Milyen karaktert nyomtak le az inputField-ben
    bool previousTextWasNumber; // inputField módosítása előtt szám volt benne
    bool valueChangedInside;    // Az inputFieldben található szöveget belsőleg módosítottuk

    // Ki kell-e értékelni a választ
    // Előfordulhat, hogy az inputField-ből kitörölték a szöveget, tehát nem írtak bele újat, ezért nem kell kiértékelni a választ
    // viszont igazítani szükséges az elemeket hiszen az inputField keskenyebb lett
    // Persze az is előfordulhat, hogy megváltoztatták a szöveget és ki kell értékelni, ilyenkor az alábbi változó igaz értéket tartalmaz
    [HideInInspector]
    public bool wasAnswer;      // Ki kell-e értékelni a választ

    // Use this for initialization
    void Awake ()
    {
        Debug.Log(Common.GetGameObjectHierarchy(gameObject));

        rectTransform = GetComponent<RectTransform>();
        textTest = Common.SearchGameObject(gameObject, "TextTest").GetComponent<Text>();
        inputField = Common.SearchGameObject(gameObject, "InputField").GetComponent<InputField>();

        //rectTransformButton = Common.SearchGameObject(gameObject, "Button").GetComponent<RectTransform>();
        imageBackground = Common.SearchGameObject(gameObject, "Background").GetComponent<Image>();
        defaultBackgroundColor = imageBackground.color;

        textDiff = rectTransform.sizeDelta.x - textTest.preferredWidth;
	}

    // Létrehozunk egy kérdés gombot
    public void Init(Common.CallBack_In_Object callBack, int questionIndex, int subQuestionIndex, float maxWidth)
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

        inputField.onValidateInput += Validate;

        previousEditText = "";
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
    public void SetWidth()
    {
        string debugString = "SetWidth";
        debugString += "\ntext : " + inputField.text;
        debugString += "\nsize delta : " + rectTransform.sizeDelta;

        textTest.text = inputField.text; // ConvertText(inputField.text);
        rectTransform.sizeDelta = new Vector2(Mathf.Clamp(textTest.preferredWidth + textDiff, 120, maxWidth), rectTransform.sizeDelta.y);

        debugString += "\nsize delta2 : " + rectTransform.sizeDelta;
        Debug.Log(debugString);
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
        return inputText;
        return inputField.text;
    }

    public void SetText(string text)
    {
        inputField.text = text;

        //inputText = text;
        //ValueChanged();
    }

    //    public void ButtonClick() {
    //        if (callBack != null)
    //            callBack(this);
    //    }

    public float GetHeight()
    {
        return rectTransform.sizeDelta.y;
    }

    public float GetWidth()
    {
        return rectTransform.sizeDelta.x;
    }

    public void ValueChanged()
    {
        string debugString = "ValueChanged\n";
        debugString += "\ntext : " + inputField.text;
        debugString += "\ntext length : " + inputField.text.Length;
        debugString += "\ncaret position : " + inputField.caretPosition;
        Debug.Log(debugString);

        // Ha belsőleg változtattuk meg az inputField tartalmát, akkor nem kell ezen mégegyszer végig menni
        if (valueChangedInside)
        {
            valueChangedInside = false;
            Debug.Log("ValueChanged exit becose value changed inside");
            return;
        }

        if (Input.GetKey(KeyCode.Delete))
            Debug.Log("Delete is pressed");
        if (Input.GetKey(KeyCode.Backspace))
            Debug.Log("BackSpace is pressed");

        int newCaretPosition = inputField.caretPosition;
        string newInputText = inputField.text;

        // Ha előzőleg számként azonosított szöveg volt az InputField-ben. Akkor ki kell törölni az esetleg bele írt szóközöket
        if (previousTextWasNumber)
        {
            if (pressedChar != null)
            {
                // Ha volt leütött karakter, akkor külön ki kell törölni az új karakter előtti és utáni szöveg részből is a szóközöket
                string charBefore = inputField.text.Substring(0, inputField.caretPosition - 1);
                string charBeforeSpaceless = charBefore.Replace(" ", "");
                string charAfterSpaceless = inputField.text.SubstringSafe(inputField.caretPosition, inputField.text.Length).Replace(" ", "");

                // Új szöveg
                newInputText = charBeforeSpaceless + pressedChar + charAfterSpaceless;

                // Ahány szóközt kitöröltünk az új karakter előtti részből, annyit le kell vonni a caret pozícióból
                newCaretPosition -= charBefore.Length - charBeforeSpaceless.Length;
            }
            else
            {
                // Ha nem volt leütött karakter, akkor a módosítás csak törlés lehetett, így az egész szövegből eltávolítjuk a szóközöket, hogy aztán újra bele tehessük őket
                // Új szöveg
                newInputText = inputField.text.Replace(" ", "");

                string charBefore = inputField.text.Substring(0, inputField.caretPosition);
                string charBeforeSpaceless = charBefore.Replace(" ", "");

                // Ahány szóközt kitöröltünk az új karakter előtti részből, annyit le kell vonni a caret pozícióból
                newCaretPosition -= charBefore.Length - charBeforeSpaceless.Length;

                // Ha csak egy karaktert töröltek ...
                if (inputTextWithSpace.Length - 1 == inputField.text.Length)
                {
                    // ... akkor megnézzük, hogy szóközt töröltek-e
                    if (inputTextWithSpace[inputField.caretPosition] == ' ')
                    {
                        // Ha szóközt töröltek, akkor megnézzük, hogy BackSpace vagy Delete gombbal
                        // Ha BackSpace-val, akkor a kurzor előtti karaktert kell eltávolítani pluszban ha Delete-el, akkor a kurzor utánit
                        if (Input.GetKey(KeyCode.Delete))
                        {
                            if (newInputText.Length > newCaretPosition)
                                newInputText = newInputText.Remove(newCaretPosition, 1);
                        }

                        if (Input.GetKey(KeyCode.Backspace))
                        {
                            if (newCaretPosition > 0)
                            {
                                newInputText = newInputText.Remove(newCaretPosition - 1, 1);
                                newCaretPosition--;
                            }
                        }
                    }
                }
            }
        }

        inputText = newInputText;

        // Megvizsgáljuk, hogy az új szöveg számot tartalmaz-e
        previousTextWasNumber = false;
        string s = ConvertText(newInputText);

        // Ha konvertálás után a szöveg megváltozott, akkor a szöveg számot tartalmaz
        if (s != newInputText)
        {
            previousTextWasNumber = true;
            newInputText = s;

            // Ahány szóközt beszúrtunk a kurzor pozíció elé, annyival kell növelni a kurzor pozícióját
            int notSpace = 0; // A nem space karakterek száma
            for (int i = 0; i < newInputText.Length; i++)
            {
                if (newInputText[i] != ' ')
                {
                    notSpace++;
                }
                else
                {
                    newCaretPosition++;
                }

                if (i == newCaretPosition)
                    break;
            }
        }

        inputTextWithSpace = newInputText;

        // Az új inputot rögzítjük a inputField-ben.
        //valueChangedInside = true;
        inputField.text = newInputText;
        inputField.caretPosition = newCaretPosition;

        pressedChar = null;

        SetWidth(); // Beállítjuk az inputField szélességét a benne levő szövegnek megfelelően

        wasAnswer = false; // Nem kell kiértékelni csak igazítani

        if (callBack != null)
            callBack(this);
    }

    public void EndEdit()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
            inputField.text = "";

        SetWidth(); // Beállítjuk az inputField szélességét a benne levő szövegnek megfelelően

        // Ha nem változtatták meg a szöveget vagy a szöveg üres stringet tartalmaz, akkor nem kell kiértékelni az eredményt
        wasAnswer = inputField.text != previousEditText && !string.IsNullOrWhiteSpace(inputField.text);

        previousEditText = inputField.text;

        if (callBack != null)
            callBack(this);
    }

    char Validate(string input, int charIndex, char addedChar)
    {
        string debugString = "Validate";
        debugString += "\ninput : " + input;
        debugString += "\ncharIndex : " + charIndex;
        debugString += "\naddedChar : " + addedChar;
        Debug.Log(debugString);

        pressedChar = addedChar;

        //Checks if a dollar sign is entered....
        if (addedChar == '$')
        {
            // ... if it is change it to an empty character.
            addedChar = '\0';
        }

        return addedChar;
    }

    /// <summary>
    /// Megvizsgálja, hogyg a text változó számot tartalmaz-e, ha igen, akkor három karakterenként tagolja.
    /// pl. -12478,12548 -> -12 478,125 48
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    string ConvertText(string text)
    {
        string regex = @"^([+-]?)(\d*)(([.,])(\d*))?$";

        Match match = Regex.Match(text, regex);
        if (match.Success && (match.Groups[2].Length > 0 || match.Groups[5].Length > 0))
        {
            return match.Groups[1].Value + Separate(match.Groups[2].Value, match.Groups[2].Length % 3) + match.Groups[4].Value + Separate(match.Groups[5].Value, 3);
        }

        return text;
    }

    /// <summary>
    /// Hármas csoportokra bontja és szóközökkel tölti meg az átadott stringet
    /// </summary>
    /// <param name="s"></param>
    /// <param name="nextSeparate">Hány karakterből álljon az első csoport</param>
    /// <returns></returns>
    string Separate(string s, int nextSeparate)
    {
        if (nextSeparate < 1)
            nextSeparate = 3;

        string result = "";
        int triple = 0;
        for (int i = 0; i < s.Length; i++)
        {
            triple++;
            if (triple > nextSeparate)
            {
                result += " ";
                triple = 1;
                nextSeparate = 3;
            }

            result += s[i];
        }

        return result;
    }

    public bool WasAnswer()
    {
        return wasAnswer;
    }
}
