using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour, IBoolProvider
{
    public static Settings instance;

    Canvas canvas;
    RectTransform rectTransformSettingsPanel;

    void Awake()
    {
        instance = this;

        canvas = GetComponentInChildren<Canvas>();
        rectTransformSettingsPanel = gameObject.SearchChild("Panel").GetComponent<RectTransform>();
    }

    public void Show()
    {
        Refresh();

        float showAnimTime = 1f;
        Common.fade.Show(Color.white, 0.4f,showAnimTime, null);

        Tween.TweenAnimation animation = new Tween.TweenAnimation(
            startPos: -2000f,
            endPos: 0f,
            easeType: Tween.EaseType.easeOutQuint,
            time: showAnimTime,
            onUpdate: UpdateBackgroundPos
        );

        Tween.StartAnimation(animation);
    }

    public void Hide()
    {
        float hideAnimTime = 0.5f;
        Common.fade.Hide(hideAnimTime);

        Tween.TweenAnimation animation = new Tween.TweenAnimation(
            startPos: 0f,
            endPos: -2000f,
            easeType: Tween.EaseType.easeInSine,
            time: hideAnimTime,
            onUpdate: UpdateBackgroundPos
        );

        Tween.StartAnimation(animation);
    }

    void UpdateBackgroundPos(object o) // float pos)
    {
        float pos = (float)o;
        rectTransformSettingsPanel.anchoredPosition = new Vector2(rectTransformSettingsPanel.anchoredPosition.x, pos);
    }

    void Refresh()
    {
        // Ki/be kapcsolgatjuk a gameObject-et, hogy kikényszerítsünk a hierarchy-ába levő komponensek frissítését
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case C.JSONKeys.playAnimations: Common.configurationController.playAnimations = !Common.configurationController.playAnimations; break;
            case C.JSONKeys.statusTableInSuper: Common.configurationController.statusTableInSuper = !Common.configurationController.statusTableInSuper; break;
            case C.JSONKeys.statusTableBetweenSuper: Common.configurationController.statusTableBetweenSuper = !Common.configurationController.statusTableBetweenSuper; break;

            case "Exit":

                ClassYServerCommunication.instance.saveAppSettings(
                    (bool success, JSONNode response) => {
                        Hide();
                    }
                    );

                //Common.configurationController.Save();
                //Hide();

                break;
        }

        Refresh();
    }

    public bool BoolProvider(string token)
    {
        switch (token)
        {
            case C.JSONKeys.playAnimations: return Common.configurationController.playAnimations; break;
            case C.JSONKeys.statusTableInSuper: return Common.configurationController.statusTableInSuper; break;
            case C.JSONKeys.statusTableBetweenSuper: return Common.configurationController.statusTableBetweenSuper; break;
        }

        return false;
    }
}
