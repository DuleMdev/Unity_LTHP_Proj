using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassYCurriculumInfo : MonoBehaviour
{
    static public ClassYCurriculumInfo instance;

    RectTransform rectTransformCurriculumInfo;

    StarProgressBar starProgressBar;

    Text textCurriculumName;
    Text textCurriculumOtherName;
    Text textPoints;

    Text textShareWithStudentsForLearn;
    Text textShareInCommonWork;
    Text textShareOnFacebook;
    Text textShareOnClassYStore;

    Text textPlay;

    Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake ()
    {
        instance = this;

        rectTransformCurriculumInfo = gameObject.SearchChild("Image").GetComponent<RectTransform>();

        starProgressBar = gameObject.SearchChild("StarProgressBar").GetComponent<StarProgressBar>();

        textCurriculumName = gameObject.SearchChild("TextCurriculumName").GetComponent<Text>();
        textCurriculumOtherName = gameObject.SearchChild("TextCurriculumOtherDatas").GetComponent<Text>();
        textPoints = gameObject.SearchChild("TextPoints").GetComponent<Text>();

        textShareWithStudentsForLearn = gameObject.SearchChild("TextShareWithStudentsForLearn").GetComponent<Text>();
        textShareInCommonWork = gameObject.SearchChild("TextShareInCommonWork").GetComponent<Text>();
        textShareOnFacebook = gameObject.SearchChild("TextShareOnFacebook").GetComponent<Text>();
        textShareOnClassYStore = gameObject.SearchChild("TextShareOnClassYStore").GetComponent<Text>();

        textPlay = gameObject.SearchChild("TextPlay").GetComponent<Text>();

        transform.position = Vector3.zero;

        UpdateCurriculumInfoPos(0);
    }

    public void Initialize(CurriculumItemDriveData curriculumItemDriveData, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        textShareWithStudentsForLearn.text = Common.languageController.Translate(C.Texts.ShareWithStudentsForLearn);
        textShareInCommonWork.text = Common.languageController.Translate(C.Texts.ShareInCommonWork);
        textShareOnFacebook.text = Common.languageController.Translate(C.Texts.ShareOnFacebook);
        textShareOnClassYStore.text = Common.languageController.Translate(C.Texts.ShareOnClassYStore);

        textPlay.text = Common.languageController.Translate(C.Texts.Play);

        // Tananyag nevének kiírása
        textCurriculumName.text = curriculumItemDriveData.name;

        // Csillagok beállítása

        // pontszám kiírása
        textPoints.text = curriculumItemDriveData.points + " " + Common.languageController.Translate(C.Texts.Points);

        // Egyébb információk kiírása
        textCurriculumOtherName.text =
            Common.languageController.Translate(C.Texts.MadeBy) + curriculumItemDriveData.madeBy + "\n" +
            Common.languageController.Translate(C.Texts.Date) + curriculumItemDriveData.date + "\n" +
            "\n" +
            Common.languageController.Translate(C.Texts.Subject) + "subject is here" + "\n" +
            Common.languageController.Translate(C.Texts.SearchWords) + curriculumItemDriveData.searchWords;
    }

    public void Show() {
        float showAnimTime = 1;
        Common.fade.Show(Color.white, 0.4f, showAnimTime, buttonClick);
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", rectTransformCurriculumInfo.sizeDelta.y, "time", showAnimTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateCurriculumInfoPos", "onupdatetarget", gameObject));
    }

    public void Hide() {
        float hideAnimTime = 0.5f;
        Common.fade.Hide(hideAnimTime);
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformCurriculumInfo.anchoredPosition.y, "to", 0, "time", hideAnimTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateCurriculumInfoPos", "onupdatetarget", gameObject));
    }

    public void HideImmediatelly() {
        Common.fade.HideImmediatelly();
        UpdateCurriculumInfoPos(0);
    }

    void UpdateCurriculumInfoPos(float value)
    {
        rectTransformCurriculumInfo.anchoredPosition = new Vector2(rectTransformCurriculumInfo.anchoredPosition.x, value);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
