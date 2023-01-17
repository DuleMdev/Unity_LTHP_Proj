using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassYMainMenuItem : MonoBehaviour {

    ClassYIconControl iconControl;
    Notice notice;

    public string buttonName { get { return iconControl.buttonName; } }

	// Use this for initialization
	void Awake () {
        iconControl = gameObject.GetComponent<ClassYIconControl>();
        notice = gameObject.SearchChild("Notice").GetComponent<Notice>();
	}

    public void Initialize(string iconText, string notice, string buttonName, Common.CallBack_In_String buttonClick, Color? diamondColor = null, bool? showIcon = null, Sprite icon = null) {
        iconControl.Initialize(iconText, buttonName, buttonClick, diamondColor, showIcon, icon);
        this.notice.Initialize(notice);
    }

    public void SetActive(bool active) {
        iconControl.SetActive(active);
    } 
}
