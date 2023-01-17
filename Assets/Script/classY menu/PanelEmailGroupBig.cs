using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelEmailGroupBig : MonoBehaviour
{
    Text text;
    Image image;

    GameObject buttonUnsubscribe;
    GameObject buttonSubscribe;
    GameObject buttonInvitedAcceptance;
    GameObject buttonPlay;

    string buttonName;
    Common.CallBack_In_String callBack;

    string emailGroupID;

    // Start is called before the first frame update
    void Awake()
    {
        text = gameObject.SearchChild("Text").GetComponent<Text>();
        image = gameObject.SearchChild("Image").GetComponent<Image>();

        buttonUnsubscribe = gameObject.SearchChild("ButtonUnsubscribe").gameObject;
        buttonSubscribe = gameObject.SearchChild("ButtonSubscribe").gameObject;
        buttonInvitedAcceptance = gameObject.SearchChild("ButtonInvitedAcceptance").gameObject;
        buttonPlay = gameObject.SearchChild("ButtonPlay").gameObject;
    }

    public void Initialize(EmailGroup emailGroup, string buttonNamePrefix, Common.CallBack_In_String callBack)
    {
        text.text = emailGroup.name;
        this.buttonName = buttonNamePrefix + ":" + emailGroup.id;
        this.callBack = callBack;

        emailGroupID = emailGroup.id;

        EmailGroupPictureController.instance.GetPictureFromUploadsDir(emailGroup.pictureName, (Sprite sprite) =>
        {
            image.sprite = sprite;
        });

        // Gombok beállítása
        buttonSubscribe.SetActive(emailGroup.IsSubscribePossible());
        buttonUnsubscribe.SetActive(false); //buttonUnsubscribe.SetActive(emailGroup.IsUnsubscribePossible()); // Helyette a play van
        buttonUnsubscribe.SetActive(emailGroup.IsUnsubscribePossible());
        buttonInvitedAcceptance.SetActive(emailGroup.IsAcceptancePossible());
        buttonPlay.SetActive(emailGroup.IsPlayEnabled());
    }

    public void ButtonClick(string buttonName)
    {
        if (callBack != null)
            callBack(this.buttonName + ":" + buttonName);
    }
}
