using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PrefabClassYMenuItem : MonoBehaviour {

    Image imageSelected;
    GameObject info;
    PrefabTwoTriangle twoTriangle;
    Notice notice;
    Text text;

    Common.CallBack_In_String buttonClick;
    public string buttonName { get; private set; }

	// Use this for initialization
	void Awake () {
        imageSelected = gameObject.SearchChild("ImageSelected").GetComponent<Image>();
        twoTriangle = gameObject.SearchChild("PrefabTwoTriangle").GetComponent<PrefabTwoTriangle>();
        notice = gameObject.SearchChild("PrefabNotice").GetComponent<Notice>();
        //text = gameObject.SearchChild("Text").GetComponent<Text>(); // Máshol is van Text nevű gameObject
        text = transform.Find("Text").GetComponent<Text>();
        info = gameObject.SearchChild("ImageInfo").gameObject;
    }

    public void Initialize(Sprite selectedSprite, Color selectedColor, Color leftTriangleColor, Color rightTriangleColor, bool infoSignal, string noticeText, string menuItemName, Color menuItemNameColor, string buttonName, Common.CallBack_In_String buttonClick)
    {
        imageSelected.sprite = selectedSprite;
        imageSelected.color = selectedColor;

        twoTriangle.SetTriangleColor(leftTriangleColor, rightTriangleColor);

        info.SetActive(infoSignal);

        notice.Initialize(noticeText);

        text.text = menuItemName;
        text.color = menuItemNameColor;

        this.buttonName = buttonName;
        this.buttonClick = buttonClick;
    }

    public void Initialize(bool infoSignal, string noticeText, string menuItemName, string buttonName, Common.CallBack_In_String buttonClick)
    {
        info.SetActive(infoSignal);

        notice.Initialize(noticeText);

        text.text = menuItemName;

        this.buttonName = buttonName;
        this.buttonClick = buttonClick;
    }

    public void SetSelected(bool selected) {
        imageSelected.enabled = selected;
    }

    public void ButtonClick() {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
