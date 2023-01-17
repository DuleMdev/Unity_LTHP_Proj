using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;

public class CanvasClassYEmailRegistration : HHHScreen
{

    Text textFacebookLogin;
    Text textEmailLogin;
    Text textEmailRegistration;

    Text textUserName;
    Text textPassword;
    Text textPasswordAgain;
    Text textEmailAddress;
    Text textPrivacyPolicy;

    InputField inputFieldUserName;
    InputField inputFieldPassword;
    InputField inputFieldPasswordAgain;
    InputField inputFieldEmailAddress;

    PrefabButton buttonRegistration;

    // Use this for initialization
    void Awake ()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textFacebookLogin = gameObject.SearchChild("TextFacebookLogin").GetComponent<Text>();
        textEmailLogin = gameObject.SearchChild("TextEmailLogin").GetComponent<Text>();
        textEmailRegistration = gameObject.SearchChild("TextEmailRegistration").GetComponent<Text>();

        textUserName = gameObject.SearchChild("TextUserName").GetComponent<Text>();
        textPassword = gameObject.SearchChild("TextUserPassword").GetComponent<Text>();
        textPasswordAgain = gameObject.SearchChild("TextUserPasswordAgain").GetComponent<Text>();
        textEmailAddress = gameObject.SearchChild("TextUserEmailAddress").GetComponent<Text>();
        textPrivacyPolicy = gameObject.SearchChild("TextUserPrivacyPolicy").GetComponent<Text>();

        inputFieldUserName = gameObject.SearchChild("InputFieldUserName").GetComponent<InputField>();
        inputFieldPassword = gameObject.SearchChild("InputFieldPassword").GetComponent<InputField>();
        inputFieldPasswordAgain = gameObject.SearchChild("InputFieldPasswordAgain").GetComponent<InputField>();
        inputFieldEmailAddress = gameObject.SearchChild("InputFieldEmailAddress").GetComponent<InputField>();

        buttonRegistration = gameObject.SearchChild("ButtonRegistration").GetComponent<PrefabButton>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A képenyőnek alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(2);
        Common.menuStripe.SetItem();

        textFacebookLogin.text = Common.languageController.Translate(C.Texts.FacebookLogin);
        textEmailLogin.text = Common.languageController.Translate(C.Texts.EmailLogin);
        textEmailRegistration.text = Common.languageController.Translate(C.Texts.EmailRegistration);

        textUserName.text = Common.languageController.Translate(C.Texts.UserName);
        textPassword.text = Common.languageController.Translate(C.Texts.Password);
        textPasswordAgain.text = Common.languageController.Translate(C.Texts.PasswordAgain);
        textEmailAddress.text = Common.languageController.Translate(C.Texts.EmailAddress);
        textPrivacyPolicy.text = Common.languageController.Translate(C.Texts.PrivacyPolicy);

        inputFieldUserName.text = "";
        inputFieldPassword.text = "";
        inputFieldPasswordAgain.text = "";
        inputFieldEmailAddress.text = "";

        buttonRegistration.Initialize(Common.languageController.Translate(C.Texts.Registration));

        TouchScreenKeyboard.hideInput = true;
        TouchScreenKeyboard.Open("Szöveg", TouchScreenKeyboardType.Default, false, false, false, false, "");

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        TouchScreenKeyboard.hideInput = true;
    }


    /// <summary>
    /// A UI felületen lévő Button komponens hívja meg ha rákattintottak
    /// </summary>
    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "FacebookLogin":

                break;

            case "EmailLogin":
                Common.screenController.ChangeScreen(C.Screens.MenuClassYEmailLogin);
                break;

            case "EmailRegistration":
                
                string errorText = "";
                // Felhasználói név ellenőrzése
                // Szerveren ellenőrizzük
                /*
                if (string.IsNullOrEmpty(inputFieldUserName.text)) // Ha üres
                    errorText = C.Texts.emptyUserName;

                if (inputFieldUserName.text.Length < 3) // Ha kisebb mint 3 karakter
                    errorText = C.Texts.lengthUserName;
                */

                // Jelszó mező nincs kitöltve
                if (string.IsNullOrEmpty(inputFieldPassword.text))
                    errorText = C.Texts.emptyPass;

                // Jelszó bonyolultságának ellenőrzése (min. 6 karakter)
                if (errorText == "" && inputFieldPassword.text.Length < 6)
                    errorText = C.Texts.lengthPass;

                // A két jelszó azonosságának ellenőrzése
                // Szerveren ellenőrizzük
                /*
                if (inputFieldPassword.text != inputFieldPasswordAgain.text)
                    errorText = C.Texts.matchPass;

                // email cím szintaxisának ellenőrzése
                if (!EmailIsValid(inputFieldEmailAddress.text))
                    errorText = C.Texts.wrongEmail;
                */

                if (string.IsNullOrEmpty(errorText))
                    ClassYServerCommunication.instance.EmailRegistration(
                        inputFieldUserName.text,
                        inputFieldPassword.text,
                        inputFieldPasswordAgain.text,
                        inputFieldEmailAddress.text,
                        true,
                        (bool success) =>
                        {
                            if (success)
                                Common.infoPanelInformation.Show(Common.languageController.Translate(C.Texts.RegistrationSuccess), true,
                                    (string s) => {
                                    Common.menuInformation.Hide( () => {
                                        Common.screenController.ChangeScreen(C.Screens.MenuClassYLogin);
                                    } );  
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
