using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIButtonStudentName : MonoBehaviour {

    public enum ButtonStatus
    {
        Selectable,
        Selected,
        Actual
    }

    GameObject button;          // Választható gomb
    GameObject buttonSelected;  // Már kiválasztották, nem választható
    GameObject buttonActual;    // Az aktuálisan kiválasztott gomb

    Text textStudentName;

    public int id { get; private set; }
    public ButtonStatus buttonStatus { get; private set; }
    Common.CallBack_In_String callBack;

	// Use this for initialization
	void Awake () {
        button = Common.SearchGameObject(gameObject, "Button").gameObject;
        buttonSelected = Common.SearchGameObject(gameObject, "ButtonSelected").gameObject;
        buttonActual = Common.SearchGameObject(gameObject, "ButtonActual").gameObject;

        textStudentName = Common.SearchGameObject(gameObject, "TextStudentName").GetComponent<Text>();
	}

    public void Initialize(int id, string studentName, ButtonStatus buttonStatus, Common.CallBack_In_String callBack)
    {
        this.id = id;
        this.callBack = callBack;

        textStudentName.text = studentName;

        SetButtonState(buttonStatus);
    }

    public void SetButtonState(ButtonStatus buttonStatus) {
        this.buttonStatus = buttonStatus;
        button.SetActive(ButtonStatus.Selectable == buttonStatus);
        buttonSelected.SetActive(ButtonStatus.Selected == buttonStatus);
        buttonActual.SetActive(ButtonStatus.Actual == buttonStatus);
    }

    public void ButtonClick(string buttonName) { 
        if (callBack != null)
            callBack(id.ToString());
    }
}
