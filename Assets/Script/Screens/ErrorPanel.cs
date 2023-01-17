using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPanel : MonoBehaviour
{
    public static ErrorPanel instance;

    RectTransform rectTransformCanvas;
    RectTransform rectTransformImageBackground;
    Text textErrorMessage;

    GameObject button1;
    GameObject button2;

    Common.CallBack_In_String callBack;

    bool visible = false; // Éppen látszik az error panel?

    Common.CallBack hideCallBack;   // Mit kell meghívni, ha eltünt az errorPanel

	// Use this for initialization
	void Awake () {
        instance = this;

        rectTransformCanvas = gameObject.SearchChild("Canvas").GetComponent<RectTransform>();
        rectTransformImageBackground = gameObject.SearchChild("Image").GetComponent<RectTransform>();
        textErrorMessage = gameObject.SearchChild("TextErrorMessage").GetComponent<Text>();

        button1 = gameObject.SearchChild("Button1").gameObject;
        button2 = gameObject.SearchChild("Button2").gameObject;

        // Gombokat alaphelyzetbe hozzuk
        ButtonInitialize(button1, "Button1", Color.yellow);
        ButtonInitialize(button2, "Button2", Color.yellow);

        transform.position = Vector3.zero;
    }

    void ButtonInitialize(GameObject buttonGameObject, string buttonText, Color? buttonColor = null)
    {
        buttonGameObject.SetActive(buttonText != null);

        buttonGameObject.SearchChild("TextButton").GetComponent<Text>().text = buttonText;

        if (buttonColor == null)
            buttonColor = Common.MakeColor("#539B1EFF"); // Zöld

        buttonGameObject.SearchChild("ImageBackground").GetComponent<Image>().color = buttonColor.Value;
    }

    public void Show(string errorMessage, string button1Text, Color? button1Color = null, string button2Text = null, Color? button2Color = null, Common.CallBack_In_String callBack = null)
    {
        this.callBack = callBack;
        StartCoroutine(ShowCoroutine(errorMessage, button1Text, button1Color, button2Text, button2Color));
    }

    IEnumerator ShowCoroutine(string errorMessage, string button1Text, Color? button1Color = null, string button2Text = null, Color? button2Color = null)
    {
        // Ha látszik a panel, akkor eltüntetjük
        if (visible)
            Hide(null, false);

        while (visible) { yield return null; }

        // Beállítjuk a megadott szöveghez az ablak méretét
        textErrorMessage.text = errorMessage;

        // Beállítjuk a gombokat
        ButtonInitialize(button1, button1Text, button1Color);
        ButtonInitialize(button2, button2Text, button2Color);

        // Elindítjuk az animációt
        float showAnimTime = 1;
        Common.fadeError.Show(Color.white, 0.4f, showAnimTime, null); // ButtonClick);
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", 0, 
            "to", rectTransformImageBackground.sizeDelta.y, 
            "time", showAnimTime, 
            "easetype", iTween.EaseType.easeOutCubic, 
            "onupdate", "UpdateImageBackgroundPos", "onupdatetarget", gameObject));

        visible = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callBack">Milyen függvényt hívjon meg ha befejezte az errorPanel elrejtését.</param>
    /// <param name="fadeHide">A háttér elfédelése is legyen (Ha az egyik hibát egy másik követi, akkor nem kell)</param>
    public  void Hide(Common.CallBack callBack = null, bool fadeHide = true)
    {
        hideCallBack = callBack;

        float hideAnimTime = 0.5f;

        iTween.Stop(gameObject);

        if (fadeHide)
            Common.fadeError.Hide(hideAnimTime);

        iTween.ValueTo(gameObject, iTween.Hash(
            "from", rectTransformImageBackground.anchoredPosition.y, 
            "to", 0, 
            "time", hideAnimTime, 
            "easetype", iTween.EaseType.easeOutCubic, 
            "onupdate", "UpdateImageBackgroundPos", "onupdatetarget", gameObject, 
            "oncomplete", "HideCompleted", "oncompletetarget", gameObject)
            );
    }

    void UpdateImageBackgroundPos(float value)
    {
        rectTransformImageBackground.anchoredPosition = new Vector2(rectTransformImageBackground.anchoredPosition.x, value);
    }

    void HideCompleted() {
        if (hideCallBack != null)
        {
            hideCallBack();
            hideCallBack = null;
        }

        visible = false;
    }

    public void ButtonClick(string buttonName)
    {
        callBack(buttonName);
        //if (callBack != null)
        //    callBack(buttonName);
    }
}
