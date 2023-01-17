using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuClassYMenuStripe : MonoBehaviour {

    Image imageCountryFlag;
    GameObject gameObjectFlag;  // A zászló eltüntetéséhez

    Common.CallBack_In_String buttonClick;

	// Use this for initialization
	void Awake ()
    {
        imageCountryFlag = gameObject.SearchChild("ImageCountryFlag").GetComponent<Image>();
        gameObjectFlag = gameObject.SearchChild("ButtonCountryFlag").gameObject;

        gameObjectFlag.SetActive(false); // Kikapcsoljuk a zászlót Nem fogjuk használni többé
    }

    void Start()
    {
        gameObject.SearchChild("ButtonQuestionMark").gameObject.SetActive(Common.configurationController.questionMarkVisible);
    }

    public void SetCountryFlag(Sprite sprite)
    {
        imageCountryFlag.sprite = sprite;
    }

    public void SetCountryFlagVisible(bool visible) {
        //gameObjectFlag.SetActive(visible);
    }

    public void Initialize(Common.CallBack_In_String buttonClick) {
        this.buttonClick = buttonClick;
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
