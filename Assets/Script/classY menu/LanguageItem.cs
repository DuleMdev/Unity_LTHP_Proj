using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageItem : MonoBehaviour {

    Image imageBackground;
    Image imageFlag;
    Text textLanguageName;

    public string buttonName { get; private set; }
    Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake ()
    {
        imageBackground = GetComponent<Image>();
        imageFlag = gameObject.SearchChild("ImageFlag").GetComponent<Image>();
        textLanguageName = gameObject.SearchChild("TextLanguageName").GetComponent<Text>();
	}

    public void Initialize(Sprite flag, string languageName, string buttonName, Common.CallBack_In_String buttonClick)
    {
        // Ha nincs zászló kikapcsoljuk az image komponenst
        imageFlag.sprite = flag;
        if (!flag)
            imageFlag.gameObject.SetActive(false);

        textLanguageName.text = languageName;

        this.buttonName = buttonName;
        this.buttonClick = buttonClick;
    }

    public void SetSelected(bool active)
    {
        // Beállítjuk a háttérkép láthatóságát
        imageBackground.enabled = active;
    }


    public void ButtonClick()
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
