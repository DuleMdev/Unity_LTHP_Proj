using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassYCurriculumListItem : MonoBehaviour {

    Image imageShadow;
    ClassYIconControl iconControl;
    Notice notice;
    Text textMadeBy;    // Készítő nevének kiírásához 

    public string buttonName { get { return iconControl.buttonName; } }

    // Use this for initialization
    void Awake()
    {
        imageShadow = gameObject.SearchChild("Shadow").GetComponent<Image>();
        iconControl = gameObject.SearchChild("ClassYIconControl").GetComponent<ClassYIconControl>();
        notice = gameObject.SearchChild("Notice").GetComponent<Notice>();
        textMadeBy = gameObject.SearchChild("TextMadeBy").GetComponent<Text>();
    }

    public void Initialize(bool backgroundDark, string iconText, string madeBy, string notice, bool isCheck, bool isSync, string buttonName, Common.CallBack_In_String buttonClick, Color? diamondColor = null, bool? showIcon = null)
    {
        imageShadow.enabled = backgroundDark;

        iconControl.Initialize(iconText, buttonName, buttonClick, diamondColor, showIcon);
        this.notice.Initialize(notice);
        textMadeBy.text = madeBy;
    }

    public void Empty(bool backgroundDark)
    {
        imageShadow.enabled = backgroundDark;

        iconControl.gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        iconControl.SetActive(active);
    }
}
