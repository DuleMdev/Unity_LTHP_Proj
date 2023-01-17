using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPEmailGroupJumpingPanel : MonoBehaviour
{
    public static OTPEmailGroupJumpingPanel instance;

    Text textEmailGroupName;
    Text textNameOfGroupOwner;
    Text textDescriptionOfEmailGroup;
    SetLanguageText setLanguageTextTypeOfSubscription;

    GameObject buttonUnsubscribe;
    GameObject buttonSubscribe;
    GameObject buttonInvitedAcceptance;
    GameObject buttonInvitedRejection;
    GameObject buttonPlay;
    GameObject textWaitForConfirmation;

    Image imageGroupPicture;

    RectTransform rectTransformPanel;

    public EmailGroup emailGroup;
    Common.CallBack_In_String buttonCallBack; // Gomb lenyomásnál mit hívjon vissza ha szükéges
    bool oneShoot;
    bool playEnabled;

    Common.CallBack hideCallBack;   // Mit kell meghívni, ha eltünt a Panel

    bool panelIsVisible;

    string confirmationAnswer;        // Biztonsági kérdésre a válasz (igen, vagy mégsem)

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        textEmailGroupName = gameObject.SearchChild("TextEmailGroupName").GetComponent<Text>();
        textNameOfGroupOwner = gameObject.SearchChild("TextNameOfGroupOwner").GetComponent<Text>();
        textDescriptionOfEmailGroup = gameObject.SearchChild("TextDescriptionOfEmailGroup").GetComponent<Text>();
        setLanguageTextTypeOfSubscription = gameObject.SearchChild("TextTypeOfSubscription").GetComponent<SetLanguageText>();

        buttonUnsubscribe = gameObject.SearchChild("ButtonUnsubscribe").gameObject;
        buttonSubscribe = gameObject.SearchChild("ButtonSubscribe").gameObject;
        buttonInvitedAcceptance = gameObject.SearchChild("ButtonInvitedAcceptance").gameObject;
        buttonInvitedRejection = gameObject.SearchChild("ButtonInvitedRejection").gameObject;
        buttonPlay = gameObject.SearchChild("ButtonPlay").gameObject;
        textWaitForConfirmation = gameObject.SearchChild("TextWaitForConfirmation").gameObject;

        imageGroupPicture = gameObject.SearchChild("ImageGroupPicture").GetComponent<Image>();

        rectTransformPanel = gameObject.SearchChild("Panel").GetComponent<RectTransform>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="emailGroup">A megmutatni kívánt email csoport adatai.</param>
    /// <param name="buttonCallBack">Milyen függvényt hívjon meg gombnyomáskor</param>
    /// <param name="oneShoot">Ha megnyomtak egy gombot, akkor a panel becsukódik</param>
    /// <param name="playEnabled">Engedélyezve van-e a lejátszás gomb megjelenése. (Ha az útvonal lejátszásnál jön elő az ablak, akkor nem kell play gomb)</param>
    public void Show(EmailGroup emailGroup, Common.CallBack_In_String buttonCallBack, bool oneShoot = false, bool playEnabled = true)
    {
        panelIsVisible = true;

        this.emailGroup = emailGroup;
        this.buttonCallBack = buttonCallBack;
        this.oneShoot = oneShoot;
        this.playEnabled = playEnabled;

        // Beállítjuk a szövegeket
        textEmailGroupName.text = emailGroup.name;
        textNameOfGroupOwner.text = Common.languageController.Translate(C.Texts.maker) + emailGroup.ownerName;
        textDescriptionOfEmailGroup.text = emailGroup.description;
        setLanguageTextTypeOfSubscription.AddParams("type", "#" + emailGroup.joinLevel);
        setLanguageTextTypeOfSubscription.gameObject.SetActive(emailGroup.isPublic && !emailGroup.IsPlayEnabled());

        SetButtons();

        EmailGroupPictureController.instance.GetPictureFromUploadsDir(emailGroup.pictureName, (Sprite sprite) =>
        {
            imageGroupPicture.sprite = sprite;
        });

        // Elindítjuk az animációt
        float showAnimTime = 1;
        Common.fadeOTPMain.Show(Color.white, 0.4f, showAnimTime, ButtonClick); // ButtonClick);
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", 0,
            "to", rectTransformPanel.sizeDelta.y,
            "time", showAnimTime,
            "easetype", iTween.EaseType.easeOutCubic,
            "onupdate", "UpdatePanelPos", "onupdatetarget", gameObject));
    }

    /// <summary>
    /// Az email csoport alapján meghatározza, hogy mely gomb(ok)nak kellene látszódnia.
    /// </summary>
    void SetButtons()
    {
        buttonSubscribe.SetActive(emailGroup.IsSubscribePossible());
        buttonUnsubscribe.SetActive(emailGroup.IsUnsubscribePossible());
        buttonInvitedAcceptance.SetActive(emailGroup.IsAcceptancePossible());
        buttonInvitedRejection.SetActive(emailGroup.IsAcceptancePossible());
        buttonPlay.SetActive(emailGroup.IsPlayEnabled() && playEnabled);
        textWaitForConfirmation.SetActive(emailGroup.IsWaitForConfirmation());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hideCallBack">Milyen függvényt hívjon meg ha befejezte az errorPanel elrejtését.</param>
    /// <param name="fadeHide">A háttér elfédelése is legyen (Ha az egyik hibát egy másik követi, akkor nem kell)</param>
    public void Hide(Common.CallBack hideCallBack = null)
    {
        this.hideCallBack = hideCallBack;

        float hideAnimTime = 0.5f;

        iTween.Stop(gameObject);

        Common.fadeOTPMain.Hide(hideAnimTime);
        iTween.ValueTo(gameObject, iTween.Hash(
            "from", rectTransformPanel.anchoredPosition.y,
            "to", 0,
            "time", hideAnimTime,
            "easetype", iTween.EaseType.easeOutCubic,
            "onupdate", "UpdatePanelPos", "onupdatetarget", gameObject,
            "oncomplete", "HideCompleted", "oncompletetarget", gameObject)
            );
    }

    void UpdatePanelPos(float value)
    {
        rectTransformPanel.anchoredPosition = new Vector2(rectTransformPanel.anchoredPosition.x, value);
    }

    void HideCompleted()
    {
        panelIsVisible = false;

        if (hideCallBack != null)
        {
            hideCallBack();
            hideCallBack = null;
        }
    }

    /// <summary>
    /// A felugró ablak hajtja végre a listában levő elemeken történő felíratkozás, stb parancsokat, csak itt az ablak láthatatlan marad.
    /// </summary>
    /// <param name="emailGroup"></param>
    /// <param name="buttonCallBack">Megadja a hívónak, hogy a kommunikáció siekeres volt-e.</param>
    public void DefaultButtonClick (EmailGroup emailGroup, Common.CallBack_In_Bool buttonCallBack)
    {
        this.emailGroup = emailGroup;

        string buttonName = "";
        switch (emailGroup.GetButtonState())
        {
            case EmailGroup.ButtonState.subscribe:   buttonName = C.Program.Subscribe;         break;
            case EmailGroup.ButtonState.unsubscribe: buttonName = C.Program.Unsubscribe;       break;
            case EmailGroup.ButtonState.acceptance:  buttonName = C.Program.InvitedAcceptance; break;
        }

        StartCoroutine(ServerCommunicationCoroutine(buttonName, buttonCallBack));
    }

    public void ButtonClick(string buttonName)
    {
        StartCoroutine(ButtonClickCoroutine(buttonName));
    }

    IEnumerator ServerCommunicationCoroutine (string action, Common.CallBack_In_Bool callBack)
    {
        // Biztonsági kérdések
        yield return StartCoroutine(Confirmation(action));

        // Ha a biztonsági kérdésre nemmel válaszoltak, akkor kilépünk
        Debug.Log(confirmationAnswer);
        if (confirmationAnswer == "cancel") yield break;

        //yield break;

        bool ready = false;
        bool success = false;

        Common.CallBack_In_Bool_JSONNode func = (bool succ, JSONNode response) =>
        {
            success = succ;
            ready = true;
        };

        switch (action)
        {
            case C.Program.Subscribe: ClassYServerCommunication.instance.SubToMailList(emailGroup.id, null, func); break;
            case C.Program.Unsubscribe: ClassYServerCommunication.instance.UnsubFromMailList(emailGroup.id, func); break;
            case C.Program.InvitedAcceptance:
            case C.Program.InvitedRejection: ClassYServerCommunication.instance.ForceAnswerInvitation(emailGroup.invitationID, action == C.Program.InvitedAcceptance, func); break;

            default:
                ready = true;
                break;
        }

        // Várunk amíg a szerver kommunikáció lezajlik
        while (!ready) { yield return null; }

        // Sikerese akcióról visszajelzések
        if (success)
        {
            yield return StartCoroutine(SuccessAction(action));
        }

        callBack(success);
    }

    
    IEnumerator Confirmation(string action)
    {
        string question = "";
        confirmationAnswer = "ok";

        switch (action)
        {
            case C.Program.Unsubscribe : question = C.Texts.confirmationUnsubscribe; break;
        }

        if (!string.IsNullOrEmpty(question))
        {
            confirmationAnswer = "";
            ErrorPanel.instance.Show(Common.languageController.Translate(question, '#', "groupName", emailGroup.name), Common.languageController.Translate(C.Texts.Ok), button2Text: Common.languageController.Translate(C.Texts.Cancel), callBack: (string s) =>
            {
                ErrorPanel.instance.Hide(() => {
                    switch (s)
                    {
                        case "button1": confirmationAnswer = "ok"; break;
                        case "button2": confirmationAnswer = "cancel"; break;
                    }
                });
            });
        }

        // Ha volt kérdés, akkor addig várunk amíg nem jön válasz
        while (string.IsNullOrEmpty(confirmationAnswer)) { yield return null; }
    }


    IEnumerator SuccessAction(string action)
    {
        string feedbackText = "";
        bool ready = true;

        switch (action)
        {
            case C.Program.Subscribe:
                if (emailGroup.joinLevel == "free")
                    feedbackText = C.Texts.successSubscribe;
                else
                    feedbackText = C.Texts.successSubscribeConfirmation;
                break;
            case C.Program.Unsubscribe:       feedbackText = C.Texts.successUnsubscribe; break;
            case C.Program.InvitedAcceptance: feedbackText = C.Texts.successInvitedAcceptance; break;
            case C.Program.InvitedRejection:  feedbackText = C.Texts.successInvitedRejection; break;
        }

        if (!string.IsNullOrEmpty(feedbackText))
        {
            ready = false;
            ErrorPanel.instance.Show(Common.languageController.Translate(feedbackText, '#', "groupName", emailGroup.name), Common.languageController.Translate(C.Texts.Ok), callBack: (string s) =>
            {
                ErrorPanel.instance.Hide(() => { ready = true; } );
            });
        }

        // Várunk amíg bezárják a panelt
        while (!ready) { yield return null; }
    }


    public IEnumerator ButtonClickCoroutine(string buttonName)
    {
        bool ready = false;
        bool success = false;
        bool processed = true; // A gomb nyomás fel lett-e dolgozva

        switch (buttonName)
        {
            case C.Program.Subscribe: 
            case C.Program.Unsubscribe: 
            case C.Program.InvitedAcceptance:
            case C.Program.InvitedRejection:
                StartCoroutine(ServerCommunicationCoroutine(buttonName, (bool succ) =>
                {
                    success = succ;
                    ready = true;
                } ) );
                break;

            default:
                processed = false;
                ready = true;
                break;
        }

        // Várunk amíg a szerver kommunikáció lezajlik
        while (!ready) { yield return null; }

        // Ha sikeres volt az előző kommunikáció a szerverrel és látható a panel és nem kell bezárni egy gombnyomás után az ablakot
        if (success && panelIsVisible && !oneShoot)
        {
            // Lekérdezzük a megváltozott csoport adatokat
            ready = false;
            ClassYServerCommunication.instance.GetUserMailListInfo(emailGroup.id,
                (bool succ, JSONNode response) =>
                {
                    // Ha sikeres volt, akkor feldolgozzuk a választ
                    if (succ)
                        emailGroup = new EmailGroup(response[C.JSONKeys.answer]);

                    success = succ;
                    ready = true;
                });

            // Várunk amíg a szerver kommunikáció lezajlik
            while (!ready) { yield return null; }

            if (success)
                // Beállítjuk a gombokat
                SetButtons();
            //selse
            //s    processed = false;
        }

        // Ha egy kattintás engedélyezett, akkor visszahívjuk a hívót
        if (oneShoot)
        {
            buttonCallBack(buttonName);
            yield break;
        }

        if (!processed)
            buttonCallBack(buttonName);
    }
}
