using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPLogin : HHHScreen
{
    enum SubScreen
    {
        Nothing,            // Egyik sem látszik

        Grettings,
        EmailLogin,
        EmailRegistration,
    }

    enum ActiveMenu
    {
        Nothing,

        FacebookLogin,
        EmailLogin,
        EmailRegistration,
    }

    RectTransform rectTransformCanvas;

    ConfigurationController.User editorUser; // Ha a ConfigurationController-ben megváltoztatják a user nevét, akkor az Update metódusban erre reagálunk

    // Menü
    ClassYIconControl iconControlEmailLogin;
    ClassYIconControl iconControlEmailRegistration;

    // Üdvözlő képernyő
    Text textGrettings;
    Text textInfo;

    // Email belépés
    Text textLoginEmailAddress;
    Text textLoginPassword;
    InputField inputFieldLoginEmailAddress;
    InputField inputFieldLoginPassword;

    GameObject panelUsers;

    Text textEmailLoginButton;
    Text textForgotPasswordButton;
    GameObject gameObjectForgotPasswordButton;  // Az elfelejtett jelszó gomb aktíválásához

    // Email regisztráció
    Text textRegistrationEmailAddress;
    Text textRegistrationPassword;
    Text textRegistrationPasswordAgain;
    InputField inputFieldRegistrationEmailAddress;
    InputField inputFieldRegistrationPassword;
    InputField inputFieldRegistrationPasswordAgain;

    Text textEmailRegistrationButton;

    // Alképernyők mozgatásához
    RectTransform rectTransformGrettings;
    RectTransform rectTransformEmailLogin;
    RectTransform rectTransformEmailRegistration;

    SubScreen actSubScreen;
    RectTransform rectTransformActSubScreen;    // Az aktív alképernyő rectTransformja iTween animációhoz

    Common.CallBack callBackFinishHide;
    Common.CallBack callBackFinishShow;

    bool buttonEnabled;

    void Awake()
    {
        rectTransformCanvas = gameObject.SearchChild("Canvas").GetComponent<RectTransform>();

        // Menü ikonok
        iconControlEmailLogin = gameObject.SearchChild("EmailLoginIcon").GetComponent<ClassYIconControl>();
        iconControlEmailRegistration = gameObject.SearchChild("EmailRegistrationIcon").GetComponent<ClassYIconControl>();

        // Üdvözlő képernyő
        textGrettings = gameObject.SearchChild("TextGrettings").GetComponent<Text>();
        textInfo = gameObject.SearchChild("TextInfo").GetComponent<Text>();

        // Email belépés
        textLoginEmailAddress = gameObject.SearchChild("TextLoginEmailAddress").GetComponent<Text>();
        textLoginPassword = gameObject.SearchChild("TextLoginPassword").GetComponent<Text>();
        inputFieldLoginEmailAddress = gameObject.SearchChild("InputFieldLoginEmailAddress").GetComponent<InputField>();
        inputFieldLoginPassword = gameObject.SearchChild("InputFieldLoginPassword").GetComponent<InputField>();

        gameObject.SearchChild("ImageShowUsers").GetComponent<UIButtonHelper>().SetString("ShowUsers", ButtonClick);
        if (Common.configurationController.recentUserNames.Count == 0)
            gameObject.SearchChild("ImageShowUsers").SetActive(false);

        panelUsers = gameObject.SearchChild("PanelUsers").gameObject;

        textEmailLoginButton = gameObject.SearchChild("TextEmailLoginButton").GetComponent<Text>();
        textForgotPasswordButton = gameObject.SearchChild("TextForgotPasswordButton").GetComponent<Text>();
        gameObjectForgotPasswordButton = gameObject.SearchChild("ButtonForgotPassword").gameObject;

        // Email regisztráció
        textRegistrationPassword = gameObject.SearchChild("TextRegistrationPassword").GetComponent<Text>();
        textRegistrationPasswordAgain = gameObject.SearchChild("TextRegistrationPasswordAgain").GetComponent<Text>();
        textRegistrationEmailAddress = gameObject.SearchChild("TextRegistrationEmailAddress").GetComponent<Text>();
        inputFieldRegistrationPassword = gameObject.SearchChild("InputFieldRegistrationPassword").GetComponent<InputField>();
        inputFieldRegistrationPasswordAgain = gameObject.SearchChild("InputFieldRegistrationPasswordAgain").GetComponent<InputField>();
        inputFieldRegistrationEmailAddress = gameObject.SearchChild("InputFieldRegistrationEmailAddress").GetComponent<InputField>();

        textEmailRegistrationButton = gameObject.SearchChild("TextEmailRegistrationButton").GetComponent<Text>();

        // Alképernyők rectTransform komponensének megkeresése
        rectTransformGrettings = gameObject.SearchChild("Grettings").GetComponent<RectTransform>();
        rectTransformEmailLogin = gameObject.SearchChild("EmailLogin").GetComponent<RectTransform>();
        rectTransformEmailRegistration = gameObject.SearchChild("EmailRegistration").GetComponent<RectTransform>();

        // Megkeressük a UIButtonHelper komponenseket és megadjuk a Click metódust
        foreach (var item in GetComponentsInChildren<UIButtonHelper>(true))
            item.callBackString = ButtonClick;
    }

    void Update()
    {
#if UNITY_EDITOR
        // Ha Unity editorban vagyunk és megváltoztatjuk az editorUser-t, akkor automatikusan azonnal kitöltésre kerül a 
        // név és a jelszó
        if (Common.configurationController.editorUser != editorUser)
        {
            inputFieldLoginEmailAddress.text = Common.configurationController.GetUserName();
            inputFieldLoginPassword.text = Common.configurationController.GetUserPassword();
            editorUser = Common.configurationController.editorUser;
        }
#endif
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A képenyőnek alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Menü ikonok inicializálása
        iconControlEmailLogin.Initialize(C.Texts.EmailLogin, "MenuEmailLogin", ButtonClick);
        iconControlEmailRegistration.Initialize(C.Texts.EmailRegistration, "MenuEmailRegistration", ButtonClick);

        // Üdvözlő képernyő
        textGrettings.text = Common.languageController.Translate(C.Texts.OTPGrettings);
        textInfo.text = Common.languageController.Translate(C.Texts.OTPGrettingsInfo);

        // Email login
        textLoginEmailAddress.text = Common.languageController.Translate(C.Texts.EmailAddress);
        textLoginPassword.text = Common.languageController.Translate(C.Texts.Password);
        //inputFieldLoginEmailAddress.text = "";
        //inputFieldLoginPassword.text = "";
        inputFieldLoginEmailAddress.text = Common.configurationController.userName;
        inputFieldLoginPassword.text = Common.configurationController.password;

        textEmailLoginButton.text = Common.languageController.Translate(C.Texts.EmailLogin);
        textForgotPasswordButton.text = Common.languageController.Translate(C.Texts.ForgotPass);
        gameObjectForgotPasswordButton.SetActive(false);

        // Email regisztráció
        textRegistrationPassword.text = Common.languageController.Translate(C.Texts.Password);
        textRegistrationPasswordAgain.text = Common.languageController.Translate(C.Texts.PasswordAgain);
        textRegistrationEmailAddress.text = Common.languageController.Translate(C.Texts.EmailAddress);
        inputFieldRegistrationPassword.text = "";
        inputFieldRegistrationPasswordAgain.text = "";
        inputFieldRegistrationEmailAddress.text = "";

        textEmailRegistrationButton.text = Common.languageController.Translate(C.Texts.EmailRegistration);

        // Alképernyőket eltüntetjük a képernyőről
        rectTransformGrettings.anchoredPosition = new Vector2(rectTransformCanvas.sizeDelta.x, 0);
        rectTransformEmailLogin.anchoredPosition = new Vector2(rectTransformCanvas.sizeDelta.x, 0);
        rectTransformEmailRegistration.anchoredPosition = new Vector2(rectTransformCanvas.sizeDelta.x, 0);

        rectTransformGrettings.gameObject.SetActive(true);
        rectTransformEmailLogin.gameObject.SetActive(true);
        rectTransformEmailRegistration.gameObject.SetActive(true);

        buttonEnabled = false;

        // Feltöltjük a felhasználó listát a korábban belépett felhasználókkal
        GameObject prefab = gameObject.SearchChild("UserPrefab").gameObject;
        for (int i = 0; i < Common.configurationController.recentUserNames.Count; i++)
        {
            GameObject newPrefab = Instantiate(prefab, prefab.transform.parent);
            newPrefab.SetActive(true);
            newPrefab.SearchChild("Text").GetComponent<Text>().text = Common.configurationController.recentUserNames[i];
            if (i % 2 == 0)
                newPrefab.SearchChild("ImageDarkness").GetComponent<Image>().enabled = false;

            newPrefab.GetComponent<UIButtonHelper>().SetInteger(i, RecentUserNamesButtonClick);
        }


        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowStartCoroutine()
    {
        CanvasBorder_16_9.instance.SetBorderColor(Common.MakeColor("#F6F6F6"));

        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        ChangeSubScreen(SubScreen.Grettings);

        yield return null;
    }

    void ChangeSubScreen(SubScreen requestedSubScreen)
    {
        if (requestedSubScreen != actSubScreen)
        {
            buttonEnabled = false;

            HideSubScreen(() => {
                actSubScreen = requestedSubScreen;
                ShowSubScreen(() => {
                    buttonEnabled = true;
                });
            });
        }
    }

    void SubScreenMove(string callBackFinish, float movePos, float moveTime)
    {
        switch (actSubScreen)
        {
            case SubScreen.Nothing:
                FinishHideSubScreen();
                break;
            case SubScreen.Grettings:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformGrettings.anchoredPosition.x, "to", movePos, "time", moveTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateGrettingsSubScreenPos", "onupdatetarget", gameObject, "oncomplete", callBackFinish, "oncompletetarget", gameObject));
                break;
            case SubScreen.EmailLogin:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformEmailLogin.anchoredPosition.x, "to", movePos, "time", moveTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateEmailLoginSubScreenPos", "onupdatetarget", gameObject, "oncomplete", callBackFinish, "oncompletetarget", gameObject));
                break;
            case SubScreen.EmailRegistration:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformEmailRegistration.anchoredPosition.x, "to", movePos, "time", moveTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateEmailRegistrationSubScreenPos", "onupdatetarget", gameObject, "oncomplete", callBackFinish, "oncompletetarget", gameObject));
                break;
        }
    }

    void ShowSubScreen(Common.CallBack callBackFinishShow)
    {
        this.callBackFinishShow = callBackFinishShow;

        float showPos = 0;
        float hideTime = 1f;
        switch (actSubScreen)
        {
            case SubScreen.Nothing:
                FinishShowSubScreen();
                break;
            case SubScreen.Grettings:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformGrettings.anchoredPosition.x, "to", showPos, "time", hideTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateGrettingsSubScreenPos", "onupdatetarget", gameObject, "oncomplete", "FinishShowSubScreen", "oncompletetarget", gameObject));
                break;
            case SubScreen.EmailLogin:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformEmailLogin.anchoredPosition.x, "to", showPos, "time", hideTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateEmailLoginSubScreenPos", "onupdatetarget", gameObject, "oncomplete", "FinishShowSubScreen", "oncompletetarget", gameObject));
                break;
            case SubScreen.EmailRegistration:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformEmailRegistration.anchoredPosition.x, "to", showPos, "time", hideTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateEmailRegistrationSubScreenPos", "onupdatetarget", gameObject, "oncomplete", "FinishShowSubScreen", "oncompletetarget", gameObject));
                break;
        }
    }

    /// <summary>
    /// Eltünteti az aktív alképernyőt a képernyőről (kimegy jobbra).
    /// Ha befejeződött meghívja a callBack függvényt
    /// </summary>
    /// <param name="callBackFinish"></param>
    void HideSubScreen(Common.CallBack callBackFinishHide)
    {
        this.callBackFinishHide = callBackFinishHide;

        float hidePos = rectTransformCanvas.sizeDelta.x;
        float hideTime = 0.5f;
        switch (actSubScreen)
        {
            case SubScreen.Nothing:
                FinishHideSubScreen();
                break;
            case SubScreen.Grettings:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformGrettings.anchoredPosition.x, "to", hidePos, "time", hideTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateGrettingsSubScreenPos", "onupdatetarget", gameObject, "oncomplete", "FinishHideSubScreen", "oncompletetarget", gameObject));
                break;
            case SubScreen.EmailLogin:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformEmailLogin.anchoredPosition.x, "to", hidePos, "time", hideTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateEmailLoginSubScreenPos", "onupdatetarget", gameObject, "oncomplete", "FinishHideSubScreen", "oncompletetarget", gameObject));
                break;
            case SubScreen.EmailRegistration:
                iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformEmailRegistration.anchoredPosition.x, "to", hidePos, "time", hideTime, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "UpdateEmailRegistrationSubScreenPos", "onupdatetarget", gameObject, "oncomplete", "FinishHideSubScreen", "oncompletetarget", gameObject));
                break;
        }
    }

    void UpdateGrettingsSubScreenPos(float pos)
    {
        rectTransformGrettings.anchoredPosition = new Vector2(pos, 0);
    }

    void UpdateEmailLoginSubScreenPos(float pos)
    {
        rectTransformEmailLogin.anchoredPosition = new Vector2(pos, 0);
    }

    void UpdateEmailRegistrationSubScreenPos(float pos)
    {
        rectTransformEmailRegistration.anchoredPosition = new Vector2(pos, 0);
    }

    void FinishHideSubScreen()
    {
        if (callBackFinishHide != null)
            callBackFinishHide();
    }

    void FinishShowSubScreen()
    {
        if (callBackFinishShow != null)
            callBackFinishShow();
    }

    void SetActiveMenu(ActiveMenu activeMenu)
    {
        iconControlEmailLogin.SetActive(activeMenu == ActiveMenu.EmailLogin);
        iconControlEmailRegistration.SetActive(activeMenu == ActiveMenu.EmailRegistration);
    }

    public void RecentUserNamesButtonClick(int buttonName)
    {
        inputFieldLoginEmailAddress.text = Common.configurationController.recentUserNames[buttonName];
        inputFieldLoginPassword.text = "";
        panelUsers.SetActive(false);
    }

    public void ButtonClick(string buttonName)
    {
        // Ha nincs engedélyezve a gombnyomás, akkor kilépünk
        Debug.Log(((buttonEnabled) ? "enabled" : "disabled") + " : " + buttonName);

        if (!buttonEnabled)
            return;

        switch (buttonName)
        {
            case "MenuFacebookLogin":
                SetActiveMenu(ActiveMenu.FacebookLogin);
                break;

            case "MenuEmailLogin":
                SetActiveMenu(ActiveMenu.EmailLogin);
                ChangeSubScreen(SubScreen.EmailLogin);
                break;

            case "MenuEmailRegistration":
                SetActiveMenu(ActiveMenu.EmailRegistration);
                ChangeSubScreen(SubScreen.EmailRegistration);
                break;

            case "EmailLogin":
                Common.configurationController.loginEmailAddress = inputFieldLoginEmailAddress.text;

                ClassYServerCommunication.instance.LoginEmail(inputFieldLoginEmailAddress.text, inputFieldLoginPassword.text, true,
                    (bool success, JSONNode response) =>
                    {
                        if (success)
                        {
                            Common.configurationController.password = inputFieldLoginPassword.text;
                            Common.configurationController.userName = inputFieldLoginEmailAddress.text;

                            Common.configurationController.recentUserNames.Remove(Common.configurationController.userName);
                            Common.configurationController.recentUserNames.Insert(0, Common.configurationController.userName);

                            Common.configurationController.Save();

                            // Ha belépünk az OTPMain képernyőre, akkor alapból a tananyagok lejátszása lesz kiválasztva
                            OTPMain.instance.actVisiblePanel = OTPMain.Panels.empty;

                            Common.screenController.LoadScreenAfterLogin();
                        }
                        else // Elfelejtett jelszó
                            gameObjectForgotPasswordButton.SetActive(response[C.JSONKeys.answer][0].Value == C.Texts.wrongPasswordError);
                    }
                );
                break;

            case "ShowUsers":
                panelUsers.SetActive(!panelUsers.activeInHierarchy);
                break;

            case "ShowPassword":
                if (inputFieldLoginPassword.contentType == InputField.ContentType.Password)
                    inputFieldLoginPassword.contentType = InputField.ContentType.Standard;
                else
                    inputFieldLoginPassword.contentType = InputField.ContentType.Password;

                inputFieldLoginPassword.ForceLabelUpdate();

                break;

            case "ShowPasswordReg":
                if (inputFieldRegistrationPassword.contentType == InputField.ContentType.Password)
                {
                    inputFieldRegistrationPassword.contentType = InputField.ContentType.Standard;
                    inputFieldRegistrationPasswordAgain.contentType = InputField.ContentType.Standard;

                }
                else
                {
                    inputFieldRegistrationPassword.contentType = InputField.ContentType.Password;
                    inputFieldRegistrationPasswordAgain.contentType = InputField.ContentType.Password;
                }

                inputFieldRegistrationPassword.ForceLabelUpdate();
                inputFieldRegistrationPasswordAgain.ForceLabelUpdate();

                break;

            case "EmailRegistration":

                string errorText = "";
                // Felhasználói név ellenőrzése
                // Szerveren ellenőrizzük
                /*
                if (string.IsNullOrEmpty(inputFieldRegistrationUserName.text)) // Ha üres
                    errorText = C.Texts.emptyUserName;

                if (inputFieldRegistrationUserName.text.Length < 3) // Ha kisebb mint 3 karakter
                    errorText = C.Texts.lengthUserName;
                */

                // Jelszó mező nincs kitöltve
                if (string.IsNullOrEmpty(inputFieldRegistrationPassword.text))
                    errorText = C.Texts.emptyPass;

                // Jelszó bonyolultságának ellenőrzése (min. 6 karakter)
                if (errorText == "" && inputFieldRegistrationPassword.text.Length < 6)
                    errorText = C.Texts.lengthPass;

                // A két jelszó azonosságának ellenőrzése
                // Szerveren ellenőrizzük
                /*           
                if (inputFieldRegistrationPassword.text != inputFieldRegistrationPasswordAgain.text)
                    errorText = C.Texts.matchPass;

                // email cím szintaxisának ellenőrzése
                if (!EmailIsValid(inputFieldRegistrationEmailAddress.text))
                    errorText = C.Texts.wrongEmail;
                */

                if (string.IsNullOrEmpty(errorText))
                    ClassYServerCommunication.instance.EmailRegistration(
                        inputFieldRegistrationEmailAddress.text,
                        inputFieldRegistrationPassword.text,
                        inputFieldRegistrationPasswordAgain.text,
                        inputFieldRegistrationEmailAddress.text,
                        true,
                        (bool success) =>
                        {
                            if (success)
                                ErrorPanel.instance.Show(Common.languageController.Translate(C.Texts.RegistrationSuccess), Common.languageController.Translate(C.Texts.Ok), callBack: (string s) =>
                                {
                                    ErrorPanel.instance.Hide(() => {
                                        // Átváltunk a belépés képernyőre
                                        ButtonClick("MenuEmailLogin");
                                    });
                                });

                            Debug.Log("Email regisztráció : " + ((success) ? "sikeres" : "sikertelen"));
                        }
                    );
                else
                    ErrorPanel.instance.Show(Common.languageController.Translate(errorText), Common.languageController.Translate(C.Texts.Ok), callBack: (string s) =>
                    {
                        ErrorPanel.instance.Hide(() => {
                        });
                    });

                break;

            case "ForgotPassword":
                ClassYServerCommunication.instance.ForgotPassword(inputFieldLoginEmailAddress.text, true,
                    (bool success, JSONNode response) =>
                    {
                        if (success)
                            ErrorPanel.instance.Show(Common.languageController.Translate(C.Texts.ForgotPasswordSuccess), Common.languageController.Translate(C.Texts.Ok), callBack: (string s) =>
                            {
                                gameObjectForgotPasswordButton.SetActive(false);
                                ErrorPanel.instance.Hide();
                            });
                    }
                );

                break;
        }
    }

    bool EmailIsValid(string emailAddress)
    {
        int[,] D = { { 8, 8, 2 }, { 4, 3, 2 }, { 8, 8, 2 }, { 8, 8, 5 }, { 8, 6, 5 }, { 8, 8, 7 }, { 8, 6, 7 }, { 8, 8, 8 } };

        int a = 1;
        for (int i = 0; i < emailAddress.Length; i++)
        {
            char c = emailAddress[i];
            byte b = 3;
            if (c == '@') b = 1;
            if (c == '.') b = 2;

            a = D[a - 1, b - 1];

        }
        return a == 7;
    }

}
