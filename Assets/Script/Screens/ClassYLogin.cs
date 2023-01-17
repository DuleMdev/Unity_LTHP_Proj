using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassYLogin : HHHScreen
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
    ClassYIconControl iconControlFacebookLogin;
    ClassYIconControl iconControlEmailLogin;
    ClassYIconControl iconControlEmailRegistration;

    // Üdvözlő képernyő
    Text textGrettings;
    Text textInfo;
    Text textInfo2;

    // Email belépés
    Text textLoginUserName;
    Text textLoginPassword;
    InputField inputFieldLoginUserName;
    InputField inputFieldLoginPassword;

    Text textEmailLoginButton;

    // Email regisztráció
    Text textRegistrationUserName;
    Text textRegistrationPassword;
    Text textRegistrationPasswordAgain;
    Text textRegistrationEmailAddress;
    InputField inputFieldRegistrationUserName;
    InputField inputFieldRegistrationPassword;
    InputField inputFieldRegistrationPasswordAgain;
    InputField inputFieldRegistrationEmailAddress;

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
        iconControlFacebookLogin = gameObject.SearchChild("FacebookIcon").GetComponent<ClassYIconControl>();
        iconControlEmailLogin = gameObject.SearchChild("EmailLoginIcon").GetComponent<ClassYIconControl>();
        iconControlEmailRegistration = gameObject.SearchChild("EmailRegistrationIcon").GetComponent<ClassYIconControl>();

        // Üdvözlő képernyő
        textGrettings = gameObject.SearchChild("TextGrettings").GetComponent<Text>();
        textInfo = gameObject.SearchChild("TextInfo").GetComponent<Text>();
        textInfo2 = gameObject.SearchChild("TextInfo2").GetComponent<Text>();

        // Email belépés
        textLoginUserName = gameObject.SearchChild("TextLoginUserName").GetComponent<Text>();
        textLoginPassword = gameObject.SearchChild("TextLoginPassword").GetComponent<Text>();
        inputFieldLoginUserName = gameObject.SearchChild("InputFieldLoginUserName").GetComponent<InputField>();
        inputFieldLoginPassword = gameObject.SearchChild("InputFieldLoginPassword").GetComponent<InputField>();

        textEmailLoginButton = gameObject.SearchChild("TextEmailLoginButton").GetComponent<Text>();

        // Email regisztráció
        textRegistrationUserName = gameObject.SearchChild("TextRegistrationUserName").GetComponent<Text>();
        textRegistrationPassword = gameObject.SearchChild("TextRegistrationPassword").GetComponent<Text>();
        textRegistrationPasswordAgain = gameObject.SearchChild("TextRegistrationPasswordAgain").GetComponent<Text>();
        textRegistrationEmailAddress = gameObject.SearchChild("TextRegistrationEmailAddress").GetComponent<Text>();
        inputFieldRegistrationUserName = gameObject.SearchChild("InputFieldRegistrationUserName").GetComponent<InputField>();
        inputFieldRegistrationPassword = gameObject.SearchChild("InputFieldRegistrationPassword").GetComponent<InputField>();
        inputFieldRegistrationPasswordAgain = gameObject.SearchChild("InputFieldRegistrationPasswordAgain").GetComponent<InputField>();
        inputFieldRegistrationEmailAddress = gameObject.SearchChild("InputFieldRegistrationEmailAddress").GetComponent<InputField>();

        textEmailRegistrationButton = gameObject.SearchChild("TextEmailRegistrationButton").GetComponent<Text>();

        // Alképernyők rectTransform komponensének megkeresése
        rectTransformGrettings = gameObject.SearchChild("Grettings").GetComponent<RectTransform>();
        rectTransformEmailLogin = gameObject.SearchChild("EmailLogin").GetComponent<RectTransform>();
        rectTransformEmailRegistration = gameObject.SearchChild("EmailRegistration").GetComponent<RectTransform>();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Common.configurationController.editorUser != editorUser)
        {
            inputFieldLoginUserName.text = Common.configurationController.GetUserName();
            inputFieldLoginPassword.text = Common.configurationController.GetUserPassword();
            editorUser = Common.configurationController.editorUser;
        }
#endif
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A képenyőnek alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(2);
        Common.menuStripe.SetItem();

        // Menü ikonok inicializálása
        iconControlFacebookLogin.Initialize(Common.languageController.Translate(C.Texts.FacebookLogin), "MenuFacebookLogin", ButtonClick);
        iconControlEmailLogin.Initialize(Common.languageController.Translate(C.Texts.EmailLogin), "MenuEmailLogin", ButtonClick);
        iconControlEmailRegistration.Initialize(Common.languageController.Translate(C.Texts.EmailRegistration), "MenuEmailRegistration", ButtonClick);

        // Üdvözlő képernyő
        textGrettings.text = Common.languageController.Translate(C.Texts.Grettings);
        textInfo.text = Common.languageController.Translate(C.Texts.GrettingsInfo);
        textInfo2.text = Common.languageController.Translate(C.Texts.GrettingsInfo2);

        // Email login
        textLoginUserName.text = Common.languageController.Translate(C.Texts.UserName);
        textLoginPassword.text = Common.languageController.Translate(C.Texts.Password);
        inputFieldLoginUserName.text = "";
        inputFieldLoginPassword.text = "";

#if UNITY_EDITOR
        inputFieldLoginUserName.text = "zsolt.ptrvari@gmail.com";
        inputFieldLoginPassword.text = "kicsikacsa";
#endif

        textEmailLoginButton.text = Common.languageController.Translate(C.Texts.EmailLogin);

        // Email regisztráció
        textRegistrationUserName.text = Common.languageController.Translate(C.Texts.UserName);
        textRegistrationPassword.text = Common.languageController.Translate(C.Texts.Password);
        textRegistrationPasswordAgain.text = Common.languageController.Translate(C.Texts.PasswordAgain);
        textRegistrationEmailAddress.text = Common.languageController.Translate(C.Texts.EmailAddress);
        inputFieldRegistrationUserName.text = "";
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

    void SubScreenMove(string callBackFinish, float movePos, float moveTime) {
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

    void UpdateGrettingsSubScreenPos(float pos) {
        rectTransformGrettings.anchoredPosition = new Vector2(pos, 0);
    }

    void UpdateEmailLoginSubScreenPos(float pos) {
        rectTransformEmailLogin.anchoredPosition = new Vector2(pos, 0);
    }

    void UpdateEmailRegistrationSubScreenPos(float pos) {
        rectTransformEmailRegistration.anchoredPosition = new Vector2(pos, 0);
    }

    void FinishHideSubScreen() {
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
        iconControlFacebookLogin.SetActive(activeMenu == ActiveMenu.FacebookLogin);
        iconControlEmailLogin.SetActive(activeMenu == ActiveMenu.EmailLogin);
        iconControlEmailRegistration.SetActive(activeMenu == ActiveMenu.EmailRegistration);
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

                ClassYServerCommunication.instance.LoginEmail(inputFieldLoginUserName.text, inputFieldLoginPassword.text, true,
                    (bool success, JSONNode response) =>
                    {
                    if (success)
                        Common.screenController.ChangeScreen(C.Screens.OTPMain); // ClassYEDUDrive);
                        //else
                        //    buttonForgotPass.gameObject.SetActive(response[C.JSONKeys.answer][0].Value == C.Texts.wrongPassword);
                    }
                );
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
                        inputFieldRegistrationUserName.text,
                        inputFieldRegistrationPassword.text,
                        inputFieldRegistrationPasswordAgain.text,
                        inputFieldRegistrationEmailAddress.text,
                        true,
                        (bool success) =>
                        {
                            if (success)
                                Common.infoPanelInformation.Show(Common.languageController.Translate(C.Texts.RegistrationSuccess), true,
                                    (string s) => {
                                        Common.menuInformation.Hide(() => {
                                            Common.screenController.ChangeScreen(C.Screens.MenuClassYLogin);
                                        });
                                    });

                            Debug.Log("Email regisztráció : " + ((success) ? "sikeres" : "sikertelen"));
                        }
                    );
                else
                    Common.infoPanelInformation.Show(Common.languageController.Translate(errorText), true,
                        (string s) => {
                            Common.menuInformation.Hide();
                        });

                break;

            case "ForgotPassword":
                ClassYServerCommunication.instance.ForgotPassword(inputFieldLoginUserName.text, true,
                    (bool success, JSONNode response) =>
                    {
                        if (success)
                            Common.infoPanelInformation.Show(Common.languageController.Translate(C.Texts.ForgotPasswordSuccess), true,
                                (string s) => {
                                    Common.menuInformation.Hide();

                                });
                        //else
                        //    buttonForgotPass.gameObject.SetActive(response[C.JSONKeys.answer][0].Value == C.Texts.wrongPassword);
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
