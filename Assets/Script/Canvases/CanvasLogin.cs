using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class CanvasLogin : HHHScreen {

    Text textLabelUserName;
    InputField inputFieldUserName;

    Text textLabelPassword;
    InputField inputFieldPassword;

    Text textLoginButton;

    // Use this for initialization
    new void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textLabelUserName = Common.SearchGameObject(gameObject, "TextLabelUserName").GetComponent<Text>();
        inputFieldUserName = Common.SearchGameObject(gameObject, "InputFieldUserName").GetComponent<InputField>();

        textLabelPassword = Common.SearchGameObject(gameObject, "TextLabelUserPassword").GetComponent<Text>();
        inputFieldPassword = Common.SearchGameObject(gameObject, "InputFieldUserPassword").GetComponent<InputField>();

        textLoginButton = Common.SearchGameObject(gameObject, "TextLoginButton").GetComponent<Text>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        Common.menuStripe.SetItem();

        textLabelUserName.text = Common.languageController.Translate("User name");
        textLabelPassword.text = Common.languageController.Translate("Password");
        textLoginButton.text = Common.languageController.Translate("Login");

        yield return null;
    }

    /// <summary>
    /// A UI felületen lévő Button komponens hívja meg ha rákattintottak
    /// </summary>
    public void ButtonClickLogin()
    {
        // Felvesszük a szerverrel a kapcsolatot és leellenőrízzük a felhasználó név és jelszó helyességét


        // Ha nem helyes hiba üzenet feldobása és vissza



        // Ha helyes tovább megyünk a következő képernyőre
        Common.screenController.ChangeScreen("CanvasScreenServerConfig");
    }
}
