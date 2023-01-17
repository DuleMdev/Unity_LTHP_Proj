using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PopUpInfo : MonoBehaviour
{
    Image imageBackground;

    PanelCurriculumInfo curriculumInfo;
    RectTransform rectTransformCurriculumInfo;
    PanelTeacherInfo teacherInfo;
    RectTransform rectTransformTeacherInfo;

    float initialCurriculumInfoPanelPosY;
    string lastButtonName;
    bool buttonPressEnabled;

    Common.CallBack_In_String buttonClick;
    // Use this for initialization
    void Awake()
    {
        imageBackground = gameObject.GetComponent<Image>();
        ImageBackgroundEnabled(false);

        curriculumInfo = gameObject.SearchChild("PanelCurriculumInfo").GetComponent<PanelCurriculumInfo>();
        rectTransformCurriculumInfo = curriculumInfo.GetComponent<RectTransform>();
        teacherInfo = gameObject.SearchChild("PanelTeacherInfo").GetComponent<PanelTeacherInfo>();
        rectTransformTeacherInfo = teacherInfo.GetComponent<RectTransform>();

        initialCurriculumInfoPanelPosY = rectTransformCurriculumInfo.position.y;

        buttonPressEnabled = false;
    }

    public void Initialize(Common.CallBack_In_String buttonClick) {
        this.buttonClick = buttonClick;
    }

    public void ShowTeacherInfo()
    {
        teacherInfo.Initialize(ButtonClick);
        ShowBackground();
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformTeacherInfo.position.y, "to", 0, "easetype", iTween.EaseType.easeOutBounce, "onupdate", "UpdateTeacherInfoPos", "onupdatetarget", gameObject));
        buttonPressEnabled = true;
    }

    void UpdateTeacherInfoPos(float value)
    {
        rectTransformTeacherInfo.position = new Vector2(rectTransformTeacherInfo.position.x, value);
    }

    public void ShowCurriculumInfo()
    {
        curriculumInfo.Initialize(ButtonClick);
        ShowBackground();
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformCurriculumInfo.position.y, "to", 0, "easetype", iTween.EaseType.easeOutBack, "onupdate", "UpdateCurriculumInfoPos", "onupdatetarget", gameObject));
        buttonPressEnabled = true;
    }

    void UpdateCurriculumInfoPos(float value)
    {
        rectTransformCurriculumInfo.position = new Vector2(rectTransformCurriculumInfo.position.x, value);
    }

    void ShowBackground()
    {
        ImageBackgroundEnabled(true);
        iTween.ValueTo(gameObject, iTween.Hash("from", imageBackground.color.a, "to", 0.25, "easetype", iTween.EaseType.linear, "onupdate", "UpdateBackgroundColor", "onupdatetarget", gameObject));
    }

    void UpdateBackgroundColor(float value)
    {
        imageBackground.color = imageBackground.color.SetA(value);
    }

    /// <summary>
    /// Eltünteti a Teacher és a Curriculum info panelt is.
    /// </summary>
    public void HideInfoPanels()
    {
        buttonPressEnabled = false;
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformTeacherInfo.position.y, "to", rectTransformTeacherInfo.sizeDelta.y, "time", 0.5f, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "UpdateTeacherInfoPos", "onupdatetarget", gameObject));
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformCurriculumInfo.position.y, "to", initialCurriculumInfoPanelPosY, "time", 0.5f, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "UpdateCurriculumInfoPos", "onupdatetarget", gameObject));
        iTween.ValueTo(gameObject, iTween.Hash("from", imageBackground.color.a, "to", 0, "time", 0.5f, "easetype", iTween.EaseType.linear, "onupdate", "UpdateBackgroundColor", "onupdatetarget", gameObject, "oncomplete", "FinishHideBackground", "oncompletetarget", gameObject));
    }

    void FinishHideBackground()
    {
        ImageBackgroundEnabled(false);

        if (buttonClick != null)
            buttonClick(lastButtonName);
    }

    public void ImageBackgroundEnabled(bool enabled) {
        imageBackground.enabled = enabled;
    }

    public void ButtonClick(string buttonName)
    {
        if (!buttonPressEnabled)
            return;

        lastButtonName = buttonName;

        switch (buttonName)
        {
            case "ClosePopUpInfo":
            case "Play":
                HideInfoPanels();
                break;

            default:
                break;
        }
    }
}
