using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPCurriculumItem : MonoBehaviour
{
    public enum SignalType
    {
        nothing,
        newCurriculum,
        curriculumCompleted,
    }

    Image imageShadow;
    ClassYIconControl iconControl;
    Notice notice;
    Text textMadeBy;    // Készítő nevének kiírásához 
    GameObject newCurriculumSignal;         // Új tananyag jelző
    //Text textNewCurriculum; // Új szöveg  // Leváltotta a SetLanguageText komponens
    GameObject curriculumCompletedSignal;   // Teljesített tananyag jelző
    //Text textCurriculumCompleted;   // Teljesítve szöveg      // Leváltotta a SetLanguageText komponens
    Text textPercent;   // Hány százalékra lett megoldva

    GameObject cover;   // Ha nem lejátszható a tananyag, akkor ezzel letakarjuk (lakatot tartalmaz)

    public string buttonName { get { return iconControl.buttonName; } }

    // Use this for initialization
    void Awake()
    {
        imageShadow = gameObject.SearchChild("Shadow").GetComponent<Image>();

        newCurriculumSignal = gameObject.SearchChild("NewCurriculum").gameObject;
        curriculumCompletedSignal = gameObject.SearchChild("CurriculumCompleted").gameObject;
        //textNewCurriculum = gameObject.SearchChild("TextNewCurriculum").GetComponent<Text>();
        //textCurriculumCompleted = gameObject.SearchChild("TextCurriculumCompleted").GetComponent<Text>();

        iconControl = gameObject.SearchChild("ClassYIconControl").GetComponent<ClassYIconControl>();
        notice = gameObject.SearchChild("Notice").GetComponent<Notice>();

        textPercent = gameObject.SearchChild("TextPercent").GetComponent<Text>();
        textMadeBy = gameObject.SearchChild("TextMadeBy").GetComponent<Text>();

        cover = gameObject.SearchChild("Cover");
    }

    public void Initialize(bool backgroundDark, CurriculumItemDriveData itemData, string buttonName, Common.CallBack_In_String buttonClick, bool? showIcon = null)
    {
        imageShadow.enabled = backgroundDark;

        //textNewCurriculum.text = Common.languageController.Translate(C.Texts.New);
        //textCurriculumCompleted.text = Common.languageController.Translate(C.Texts.Completed);

        newCurriculumSignal.SetActive(itemData.maxCurriculumProgress == 0);
        curriculumCompletedSignal.SetActive(itemData.maxCurriculumProgress == 100);

        iconControl.Initialize(itemData.name, buttonName, buttonClick, ColorBuilder.GetColor(itemData.name), showIcon);
        //this.notice.Initialize(notice);
        textPercent.text = ((int)itemData.scorePercent).ToString() + "%";
        //textMadeBy.text = madeBy;

        if (cover != null)
            cover.SetActive(itemData.maxCurriculumProgress == 0 && Common.configurationController.curriculumLock);
    }

    public void Initialize(bool backgroundDark, SignalType signalType, string iconText, string percent, string madeBy, string notice, bool isCheck, bool isSync, string buttonName, Common.CallBack_In_String buttonClick, Color? diamondColor = null, bool? showIcon = null)
    {
        imageShadow.enabled = backgroundDark;

        //textNewCurriculum.text = Common.languageController.Translate(C.Texts.New);
        //textCurriculumCompleted.text = Common.languageController.Translate(C.Texts.Completed);

        newCurriculumSignal.SetActive(signalType == SignalType.newCurriculum);
        curriculumCompletedSignal.SetActive(signalType == SignalType.curriculumCompleted);

        iconControl.Initialize(iconText, buttonName, buttonClick, diamondColor, showIcon);
        //this.notice.Initialize(notice);
        textPercent.text = percent;
        //textMadeBy.text = madeBy;

        if (cover != null) 
            cover.SetActive(signalType == SignalType.newCurriculum && Common.configurationController.curriculumLock);
    }

    public void Empty(bool backgroundDark)
    {
        imageShadow.enabled = backgroundDark;

        iconControl.gameObject.SetActive(false);
        newCurriculumSignal.SetActive(false);
        curriculumCompletedSignal.SetActive(false);

        if (cover != null)
            cover.SetActive(false);
    }

    public void SetActive(bool active)
    {
        iconControl.SetActive(active);
    }
}
