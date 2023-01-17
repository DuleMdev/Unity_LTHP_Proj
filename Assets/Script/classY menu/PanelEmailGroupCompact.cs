using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelEmailGroupCompact : MonoBehaviour
{
    Text text;
    Image image;

    GameObject buttonDefaultContainer;
    GameObject buttonUnsubscribe;
    GameObject buttonSubscribe;
    GameObject buttonInvitedAcceptance;

    string buttonName;
    Common.CallBack_In_String callBack;

    // Start is called before the first frame update
    void Awake()
    {
        text = gameObject.SearchChild("Text").GetComponent<Text>();
        image = GetComponent<Image>();

        buttonDefaultContainer = gameObject.SearchChild("ButtonDefaultContainer");
        buttonSubscribe = gameObject.SearchChild("ButtonSubscribe");
        buttonUnsubscribe = gameObject.SearchChild("ButtonUnsubscribe");
        buttonInvitedAcceptance = gameObject.SearchChild("ButtonInvitedAcceptance");
    }

    public void Initialize(EmailGroup emailGroup, string buttonName, bool even, Common.CallBack_In_String callBack)
    {
        text.text = emailGroup.name;
        image.enabled = even;
        this.buttonName = buttonName + ":" + emailGroup.id;
        this.callBack = callBack;

        // Ha van gomb konténer, akkor beállítjuk a gombok láthatóságát
        if (buttonDefaultContainer)
        {
            buttonSubscribe.SetActive(emailGroup.IsSubscribePossible());
            buttonUnsubscribe.SetActive(emailGroup.IsUnsubscribePossible());
            buttonInvitedAcceptance.SetActive(emailGroup.IsAcceptancePossible());

            // Ha valamelyik be van kapcsolva, akkor látszik a gomb
            buttonDefaultContainer.SetActive(emailGroup.IsSubscribePossible() || emailGroup.IsUnsubscribePossible() || emailGroup.IsAcceptancePossible());
        }
    }

    public void ButtonClick(string buttonName)
    {
        if (callBack != null)
            callBack(this.buttonName + ":" + buttonName);
    }
}
