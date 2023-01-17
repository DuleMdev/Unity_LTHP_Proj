using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortButton : MonoBehaviour {

    public Color activeColor;
    public Color inactiveColor;

    RectTransform transformSortButton;  // A komponens méretezéséhez
    Text text;                          // A gomb nevének szinének beállításához
    SetLanguageText setLanguageText;    // A gomb nevének kiírásához
    Image imageBorder;                  // A keret színezéséhez
    Image imageTriangle;                // A háromszög színezéséhez

    public string buttonClickName { get; private set; }
    Common.CallBack_In_String buttonClick;

    //float textAndImageDiff;

    [HideInInspector]
    public bool isActive;
    [HideInInspector]
    public bool isAscendant = true;

    void Awake()
    {
        transformSortButton = gameObject.GetComponent<RectTransform>();
        text = gameObject.SearchChild("Text").GetComponent<Text>();
        setLanguageText = gameObject.SearchChild("Text").GetComponent<SetLanguageText>();
        imageBorder = gameObject.SearchChild("ImageBorder").GetComponent<Image>();
        imageTriangle = gameObject.SearchChild("ImageArrow").GetComponent<Image>();

        //textAndImageDiff = transformSortButton.sizeDelta.x - text.preferredWidth;
    }

    /// <summary>
    /// Inicializálja a sort gombot.
    /// </summary>
    /// <param name="buttonTextID">A gombon megjelenő név megadása.</param>
    /// <param name="buttonClickName">Kattintás esetén milyen nevet adjon vissza.</param>
    /// <param name="buttonClick">Melyik függvényt hívja meg kattintás esetén.</param>
    public void Initialize(string buttonTextID, string buttonClickName, Common.CallBack_In_String buttonClick)
    {
        //text.text = buttonText;
        setLanguageText.SetTextID(buttonTextID);
        this.buttonClickName = buttonClickName;
        this.buttonClick = buttonClick;

        //transformSortButton.sizeDelta = new Vector2(text.preferredWidth + textAndImageDiff, transformSortButton.sizeDelta.y);
    }

    /// <summary>
    /// Beállítjuk a sort gomb színét az actív változónak megfelelően.
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active)
    {
        // Ha úgy nyomták meg, hogy már aktív volt a gomb, akkor megváltoztatjuk a sorbarendezés irányát
        if (isActive && active)
            isAscendant = !isAscendant;

        text.color = (active) ? activeColor : inactiveColor;
        imageBorder.color = (active) ? activeColor : inactiveColor;
        imageTriangle.color = (active) ? activeColor : inactiveColor;

        // Beállítjuk a gombon levő nyilat annak megfelelően, hogy növekvően vagy csökkenően kell listázni az elemeket
        imageTriangle.transform.localScale = imageTriangle.transform.localScale.SetX(Mathf.Abs(imageTriangle.transform.localScale.x) * ((isAscendant) ? 1 : -1));

        isActive = active;
    }

    public void ButtonClick()
    {
        if (buttonClick != null)
            buttonClick(buttonClickName);

        //SetActive(true);
    }
}
