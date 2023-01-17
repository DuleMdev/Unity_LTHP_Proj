using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundFade : MonoBehaviour {

    public enum FadeType
    {
        Fade,
        FadeError,
        FadeOTPMain,
    }

    public FadeType fadeType;

    Image imageBackground;
    Common.CallBack_In_String buttonClick;

    public bool fadeActive { get; private set; } // Vissza jelez, hogy a fade aktív-e

    // Use this for initialization
    void Awake () {
        switch (fadeType)
        {
            case FadeType.Fade:
                Common.fade = this;
                break;

            case FadeType.FadeError:
                Common.fadeError = this;
                break;

            case FadeType.FadeOTPMain:
                Common.fadeOTPMain = this;
                break;
        }

        imageBackground = gameObject.SearchChild("Canvas").GetComponent<Image>();
        imageBackground.enabled = false;

        //transform.position = Vector3.zero;
    }

    public void Show(Color color, float visibility, float time, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        // A megadott színbe másoljuk az aktuális átlátszóságot
        color.a = imageBackground.color.a;

        // Beállítjuk a megadott színt az image komponensbe
        imageBackground.color = color;

        iTween.ValueTo(gameObject, iTween.Hash("from", imageBackground.color.a, "to", visibility, "time", time, "easetype", iTween.EaseType.linear, "onupdate", "UpdateBackgroundColor", "onupdatetarget", gameObject));
    }

    public void Hide(float time) {
        iTween.Stop(gameObject);
        iTween.ValueTo(gameObject, iTween.Hash("from", imageBackground.color.a, "to", 0, "time", time, "easetype", iTween.EaseType.linear, "onupdate", "UpdateBackgroundColor", "onupdatetarget", gameObject, "oncomplete", "FinishHideBackground", "oncompletetarget", gameObject));
    }

    public void HideImmediatelly() {
        iTween.Stop(gameObject);
        UpdateBackgroundColor(0);
    }

    void UpdateBackgroundColor(float value)
    {
        imageBackground.color = imageBackground.color.SetA(value);

        fadeActive = value != 0;
        imageBackground.enabled = fadeActive;
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
