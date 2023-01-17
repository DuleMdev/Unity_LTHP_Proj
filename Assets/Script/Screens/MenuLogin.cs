using UnityEngine;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.UI;

using SimpleJSON;

public class MenuLogin : HHHScreen {

    Text textLabelUserName;
    InputField inputFieldUserName;

    Text textLabelPassword;
    InputField inputFieldPassword;

    Text textLoginButton;

    Text textVersionNumber;

    // Use this for initialization
    new void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textLabelUserName = Common.SearchGameObject(gameObject, "TextLabelUserName").GetComponent<Text>();
        inputFieldUserName = Common.SearchGameObject(gameObject, "InputFieldUserName").GetComponent<InputField>();

        textLabelPassword = Common.SearchGameObject(gameObject, "TextLabelPassword").GetComponent<Text>();
        inputFieldPassword = Common.SearchGameObject(gameObject, "InputFieldPassword").GetComponent<InputField>();

        textLoginButton = Common.SearchGameObject(gameObject, "TextLoginButton").GetComponent<Text>();

        textVersionNumber = gameObject.SearchChild("TextVersionNumber").GetComponent<Text>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    override public IEnumerator ScreenShowStartCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(0);

        Common.menuStripe.SetItem();

        textLabelUserName.text = Common.languageController.Translate("User name");
        textLabelPassword.text = Common.languageController.Translate("Password");
        textLoginButton.text = Common.languageController.Translate("Login");

        // Töröljük az input mezők tartalmát
        inputFieldUserName.text = Common.configurationController.userName;
        inputFieldPassword.text = Common.configurationController.password;

        textVersionNumber.text = Path.GetFileNameWithoutExtension(Common.configurationController.versionNumber);

        Common.canvasNetworkHUD.SetText("");

        yield return null;
    }

    IEnumerator ConnectToServer() {
        
        Common.infoPanelInformation.Show(C.Texts.ConnectToServer, false, null);

        // Megvárjuk amíg teljesen megjelneik az információs panel
        yield return StartCoroutine(Common.menuInformation.WaitShowFinish());

        // Bejelentkezünk a szerveren
        JSONClass node = new JSONClass();
        node[C.JSONKeys.task] = C.JSONValues.login;
        node[C.JSONKeys.username] = inputFieldUserName.text;
        node[C.JSONKeys.password] = Common.GetMd5Hash(inputFieldPassword.text);

        if (string.IsNullOrEmpty(inputFieldUserName.text) && string.IsNullOrEmpty(inputFieldPassword.text))
        {
            //node[C.JSONKeys.userName] = "Pubi";
            //node[C.JSONKeys.password] = Common.GetMd5Hash("pubi12345");

            //node[C.JSONKeys.userName] = "bonis";
            //node[C.JSONKeys.password] = "5ccfbabb8e0d32e3343ec6bca656b6fa";
        }

        Debug.Log(node.ToString());

        WWWForm form = new WWWForm();
        form.AddField("json", node.ToString());

        WWW www = new WWW("http://minspire.eu/tabletcomm.php", form);

        yield return www; // Várunk amíg befejeződik a letöltés

        Debug.Log(www.text);

        string errorText = "";
         // check for errors
        if (www.error == null)
        {   // Megjött a válasz megpróbáljuk feldolgozni a jsont-t
            JSONNode answer = null;
            try
            {
                answer = JSON.Parse(www.text);
            }
            catch (System.Exception)
            {
            }

            if (answer != null) {
                if (answer.ContainsKey(C.JSONKeys.response)) {
                    if (answer[C.JSONKeys.response].Value == C.JSONValues.Success)
                    {
                        Common.configurationController.userName = inputFieldUserName.text;
                        Common.configurationController.password = inputFieldPassword.text;

                        answer[C.JSONKeys.username] = node[C.JSONKeys.username].Value;
                        answer[C.JSONKeys.passMD5] = node[C.JSONKeys.password].Value;

                        Common.configurationController.teacherList.Add(answer);
                        Common.configurationController.actTeacher = Common.configurationController.teacherList.GetTeacherByID(answer[C.JSONKeys.userID].AsInt);
                        Common.configurationController.Save();

                        Common.configurationController.MakeTeacherDirectories(); // Létrehozzuk az aktuális tanárnak a directory-jait
                        Common.configurationController.teacherConfig = new TeacherConfig(Common.configurationController.actTeacher); // Betöltjük a tanár configurációját

                        Common.menuInformation.Hide( () => {
                            Common.configurationController.offlineMode = false;
                            Common.screenController.ChangeScreen(C.Screens.MenuSynchronize);
                        });

                        yield break;
                    }
                    else { // A response kulcs nem a Success szöveget tartalmazza
                        if (answer[C.JSONKeys.response].Value == C.JSONValues.Loginfailed)
                        {   // response kulcs a "Login failed" szöveget tartalmazza (Rossz név vagy jelszó)
                            Common.infoPanelInformation.Show(
                                Common.languageController.Translate(C.Texts.WrongUserNameOrPassword),
                                true,
                                (string buttonName) => {
                                    Common.menuInformation.Hide();
                                });

                            yield break;
                        }
                        else { // A response kulcs nem is a failed szöveget, hanem valami mást
                            errorText = Common.languageController.Translate(C.Texts.ServerResponse) + answer[C.JSONKeys.response].Value;
                        } 
                    }
                }
                else { // A válasz JSON nem tartalmaz response nevű kulcsot, így nem tudom kiolvasni, hogy sikerült-e a letöltés, de ezek szerint nem
                    errorText = C.Texts.ServerError;
                }
            }
            else { // A JSON.Parse nem tudta átkonvertálni a fogadott adatokat
                errorText = "json.parser error.";
            }
        }
        else { // Hiba történt a kommunikáció során
            errorText = www.error;
        }

        // Hiba történt
        Common.infoPanelInformation.Show(
            Common.languageController.Translate(C.Texts.ConnectToServerUnsuccessful) + "\n" + 
            Common.languageController.Translate(errorText), 
            true, 
            (string buttonName) => {
                // Nyugtázták a hibaüzenetet megnyomták az Ok gombot

                // Ellenőrzés, hogy a felhasználói név és a jelszó helyes-e, mert ebben az esetben beengedhetjük a felhasználót offline módban
                TeacherData teacherData = Common.configurationController.teacherList.GetTeacherByNameAndPass(node[C.JSONKeys.username].Value, node[C.JSONKeys.password].Value);

                if (teacherData != null)
                {
                    Common.infoPanelInformationOkCancel.Show(C.Texts.SwitchOfflineMode, (string bName) =>
                    {
                        switch (bName)
                        {
                            case "Ok":
                                Common.configurationController.actTeacher = teacherData;
                                Common.configurationController.teacherConfig = new TeacherConfig(Common.configurationController.actTeacher); // Betöltjük a tanár configurációját
                                Common.menuInformation.Hide( () => {
                                    Common.configurationController.offlineMode = true;
                                    Common.screenController.ChangeScreen(C.Screens.MenuSynchronize);
                                });

                                break;
                            default:
                                Common.menuInformation.Hide();
                                break;
                        }
                    });
                }
                else {
                    Common.menuInformation.Hide();
                }
        });
    }

    /// <summary>
    /// A UI felületen lévő Button komponens hívja meg ha rákattintottak
    /// </summary>
    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Setup":
                Common.screenController.ChangeScreen("MenuSetup");
                break;
            case "Login":
                // Felvesszük a szerverrel a kapcsolatot és leellenőrízzük a felhasználó név és jelszó helyességét


                // Ha nem helyes hiba üzenet feldobása és vissza
                //inputFieldUserName.text = GetMd5Hash(inputFieldPassword.text);

                StartCoroutine(ConnectToServer());


                // Ha helyes tovább megyünk a következő képernyőre
                //Common.configurationController.userName = inputFieldUserName.text;

                //Common.screenController.ChangeScreen("MenuSynchronize");
                break;
        }



    }
}
