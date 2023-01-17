using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;

public class CanvasClassYLogin : HHHScreen
{
    Text textFacebookLogin;
    Text textEmailLogin;
    Text textEmailRegistration;

    // Use this for initialization
    void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textFacebookLogin = gameObject.SearchChild("TextFacebookLogin").GetComponent<Text>();
        textEmailLogin = gameObject.SearchChild("TextEmailLogin").GetComponent<Text>();
        textEmailRegistration = gameObject.SearchChild("TextEmailRegistration").GetComponent<Text>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(2);
        Common.menuStripe.SetItem();

        textFacebookLogin.text = Common.languageController.Translate(C.Texts.FacebookLogin);
        textEmailLogin.text = Common.languageController.Translate(C.Texts.EmailLogin);
        textEmailRegistration.text = Common.languageController.Translate(C.Texts.EmailRegistration);

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
                Common.screenController.ChangeScreen(C.Screens.MenuClassYEmailLogin);
                break;

            case "EmailRegistration":
                //StartCoroutine(Communication());
                //Common.configurationController.CheckUpdate();
                Common.screenController.ChangeScreen(C.Screens.MenuClassYEmailRegistration);
                break;
        }
    }


    /*
    coreData: {
        command: 'userLogin', 
        loginName: loginname, 
        loginPassword: loginpassword
     }
    */

    IEnumerator Communication() {
        // Bejelentkezünk a szerveren
        JSONClass node = new JSONClass();
        node[C.JSONKeys.thisIsTablet] = "dcf5de6bef6d4ff370f98300b224f253abdf682cd72be29d59f7495cce4d0183";
        node[C.JSONKeys.command] = C.JSONValues.userLogin;
        node[C.JSONKeys.userName] = "kacsa";
        node[C.JSONKeys.loginPassword] = Common.GetMd5Hash("kicsikacsa");

        Debug.Log(node.ToString(" "));

        WWWForm form = new WWWForm();
        form.AddField("coreDataStringify", node.ToString());

        WWW www = new WWW("http://test.classyedu.com/Wheatley/Core.php", form);

        yield return www; // Várunk amíg befejeződik a letöltés

        if (www.text != null)
            Debug.Log("Response : \n" + www.text);
        if (www.error != null)
            Debug.Log("Error : \n" + www.error);
    }
}