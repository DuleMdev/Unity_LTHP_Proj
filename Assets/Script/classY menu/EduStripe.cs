using UnityEngine;
using System.Collections;
using UnityEngine.UI;


/// <summary>
/// Ez az objektum a classY fő menüben található.
/// Ez tartalmazza az EDU store és az EDU drive között váltó gombot.
/// </summary>
public class EduStripe : MonoBehaviour {

    Notice notice;
    Text text;

    Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake () {
        notice = transform.Find("Notice").GetComponent<Notice>();
        text = transform.Find("Text").GetComponent<Text>();
    }

    public void Initialize(string EDUstring, int noticeCount, Common.CallBack_In_String buttonClick) {
        text.text = "EDU\n" + EDUstring;
        notice.Initialize("" + noticeCount);
        this.buttonClick = buttonClick;
    }

    public void ButtonClick() {
        if (buttonClick != null) {
            buttonClick("EduStripe");
        }
    }
}
