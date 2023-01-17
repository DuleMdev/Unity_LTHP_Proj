using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;

public class CanvasClassYEmailLogin : HHHScreen
{

    Text textFacebookLogin;
    Text textEmailLogin;
    Text textEmailRegistration;

    Text textUserName;
    Text textPassword;

    InputField inputFieldUserName;
    InputField inputFieldPassword;

    PrefabButton buttonLogin;
    PrefabButton buttonForgotPass;

    // Use this for initialization
    void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textFacebookLogin = gameObject.SearchChild("TextFacebookLogin").GetComponent<Text>();
        textEmailLogin = gameObject.SearchChild("TextEmailLogin").GetComponent<Text>();
        textEmailRegistration = gameObject.SearchChild("TextEmailRegistration").GetComponent<Text>();

        textUserName = gameObject.SearchChild("TextUserName").GetComponent<Text>();
        textPassword = gameObject.SearchChild("TextUserPassword").GetComponent<Text>();

        inputFieldUserName = gameObject.SearchChild("InputFieldUserName").GetComponent<InputField>();
        inputFieldPassword = gameObject.SearchChild("InputFieldPassword").GetComponent<InputField>();

        buttonLogin = gameObject.SearchChild("ButtonLogin").GetComponent<PrefabButton>();
        buttonForgotPass = gameObject.SearchChild("ButtonForgotPass").GetComponent<PrefabButton>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    override public IEnumerator InitCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(2);
        Common.menuStripe.SetItem();

        textFacebookLogin.text = Common.languageController.Translate(C.Texts.FacebookLogin);
        textEmailLogin.text = Common.languageController.Translate(C.Texts.EmailLogin);
        textEmailRegistration.text = Common.languageController.Translate(C.Texts.EmailRegistration);

        textUserName.text = Common.languageController.Translate(C.Texts.UserName);
        textPassword.text = Common.languageController.Translate(C.Texts.Password);

        inputFieldUserName.text = "kacsa";
        inputFieldPassword.text = "kicsikacsa";

        buttonLogin.Initialize(Common.languageController.Translate(C.Texts.EmailLogin));
        buttonForgotPass.Initialize(Common.languageController.Translate(C.Texts.ForgotPass));
        buttonForgotPass.gameObject.SetActive(false);

        yield return null;
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
                //Common.screenController.ChangeScreen(C.Screens.MenuClassYMain);

                
                ClassYServerCommunication.instance.LoginEmail(inputFieldUserName.text, inputFieldPassword.text, true,
                    (bool success, JSONNode response) =>
                    {
                        if (success)
                            Common.screenController.ChangeScreen(C.Screens.MenuClassYMain);
                        else
                            buttonForgotPass.gameObject.SetActive(response[C.JSONKeys.answer][0].Value == C.Texts.wrongPassword);
                    }
                );
                
                break;

            case "EmailRegistration":
                Common.screenController.ChangeScreen(C.Screens.MenuClassYEmailRegistration);

                break;

            case "ForgotPassword":
                ClassYServerCommunication.instance.ForgotPassword(inputFieldUserName.text, true,
                    (bool success, JSONNode response) =>
                    {
                        if (success)
                            Common.infoPanelInformation.Show(Common.languageController.Translate(C.Texts.ForgotPasswordSuccess), true,
                                (string s) => {
                                    Common.menuInformation.Hide();
                                    
                                });
                        else
                            buttonForgotPass.gameObject.SetActive(response[C.JSONKeys.answer][0].Value == C.Texts.wrongPassword);
                    }
                );

                break;
        }
    }
}
