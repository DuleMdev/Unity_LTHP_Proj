using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CurriculumItemDrive : MonoBehaviour
{
    GameObject imageDark;
    Notice notice;
    Text text;
    GameObject twoTriangle;
    GameObject imageCheckSignal;
    GameObject imageSyncOn;
    GameObject imageSyncOff;

    string buttonName;
    Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake () {
        imageDark = gameObject.SearchChild("ImageDark").gameObject;
        notice = gameObject.SearchChild("Notice").GetComponent<Notice>();
        text = transform.Find("Text").GetComponent<Text>();
        twoTriangle = gameObject.SearchChild("TwoTriangle").gameObject;
        imageCheckSignal = gameObject.SearchChild("ImageCheckSignal").gameObject;
        imageSyncOn = gameObject.SearchChild("ImageSyncOn").gameObject;
        imageSyncOff = gameObject.SearchChild("ImageSyncOff").gameObject;
    }

    public void Initialize(bool backgroundDark, string notice, string text, bool isCheck, bool SyncOn, string buttonName, Common.CallBack_In_String buttonClick)
    {
        this.buttonName = buttonName;
        this.buttonClick = buttonClick;

        imageDark.SetActive(backgroundDark);
        this.notice.Initialize(notice);
        this.text.text = text;
        twoTriangle.SetActive(true);
        imageCheckSignal.SetActive(isCheck);
        imageSyncOn.SetActive(SyncOn);
        imageSyncOff.SetActive(!SyncOn);
    }

    public void EmptyLine(bool backgroundDark) {
        Initialize(backgroundDark, "0", "", false, false, "", null);
        imageSyncOff.SetActive(false);
        twoTriangle.SetActive(false);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null) {
            switch (buttonName)
            {
                case "Panel":
                    buttonClick(this.buttonName);
                    break;
                case "Sync":
                    buttonClick(this.buttonName + ":" + buttonName);
                    break;
            }
        }
    }
}
