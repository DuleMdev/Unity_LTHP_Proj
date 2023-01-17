using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using SimpleJSON;

public class MenuSynchronize : HHHScreen {

    Text textLabelUserName;     // A bejelentkezett felhasználó nevének kiírásához
    Text textLogoutButton;      // A kijelentkező gombon található szöveg

    Transform imageArrows;      // A szinkronizáló nyilak forgatásához
    Text textLabelSyncronize;   // Szinkronizálás szöveg kiírásához
    Text textLabelPercent;      // A szinkronizálási folyamat állásának százalékos kiírásához

    Image imageSyncronize;      // Szinkronizálás gomb elhalványításához, ha nincs internet kapcsolat
    Image imageSyncronizeArrow; // A szinkronizálás nyíl elhalványításához, ha nincs internet kapcsolat
    Image imageLessonPlan;      // Az óraterv gomb elhalványításához, amikor a szinkronizálás folyik
    Text textLabelLessonPlan;   // Óraterv szöveg kiírásához

    bool synchronizeIsPossibility;  // Lehetséges a szinkronizálás? Van Wi-Fi kapcsolat?
    bool synchronize;

    // Use this for initialization
    new void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textLabelUserName = Common.SearchGameObject(gameObject, "TextLabelUserName").GetComponent<Text>();
        textLogoutButton = Common.SearchGameObject(gameObject, "TextLogoutButton").GetComponent<Text>();

        imageArrows = Common.SearchGameObject(gameObject, "ImageArrows").transform;
        textLabelSyncronize = Common.SearchGameObject(gameObject, "TextLabelSynchronize").GetComponent<Text>();
        textLabelPercent = Common.SearchGameObject(gameObject, "TextLabelPercent").GetComponent<Text>();

        imageSyncronize = Common.SearchGameObject(gameObject, "ButtonSynchronize").GetComponent<Image>();
        imageSyncronizeArrow = Common.SearchGameObject(gameObject, "ImageArrows").GetComponent<Image>();
        imageLessonPlan = Common.SearchGameObject(gameObject, "ButtonLessonPlan").GetComponent<Image>();
        textLabelLessonPlan = Common.SearchGameObject(gameObject, "TextLabelLessonPlan").GetComponent<Text>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator ScreenShowStartCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(0);

        Common.menuStripe.SetItem(); // Eltüntetjük a menüsávon az egyébb elemeket

        textLabelUserName.text = Common.configurationController.userName;
        textLogoutButton.text = Common.languageController.Translate("Logout");

        textLabelSyncronize.text = Common.languageController.Translate("Synchronize");
        textLabelPercent.text = "";

        textLabelLessonPlan.text = Common.languageController.Translate("Lesson plan");

        synchronize = false;


        yield return null;
    }

    /// <summary>
    /// Figyeli, hogy van-e internet kapcsolat és annak megfelelően engedélyezi vagy tiltja az update gombot.
    /// </summary>
    void Update()
    {
        // Meghatározzuk, hogy van-e internet kapcsolat azaz lehet-e szinkronizálni
        synchronizeIsPossibility = Application.internetReachability != NetworkReachability.NotReachable && !Common.configurationController.offlineMode;

        //synchronizeIsPossibility = false;

        // A szinkronizálás lehetőségének megfelelően beállítjuk a szinkronizálás gomb és a rajta levő nyíl átlátszóságát
        imageSyncronize.color = imageSyncronize.color.SetA((synchronizeIsPossibility) ? 1 : 0.2f);
        imageSyncronizeArrow.color = (synchronizeIsPossibility) ? Common.MakeColor("#cde2da") : Common.MakeColor("#c8e6f4");
        //imageSyncronizeArrow.color.SetA((synchronizeIsPossibility) ? 1 : 0.8f);
    }

    IEnumerator Synchronize() {
        // Lecketerv gombot elhalványítjuk
        imageLessonPlan.color = new Color(1, 1, 1, 0.2f);

        bool error = false;
        string errorText = "";

        // Feltöltjük a korábbi adatokat
        ReportFull reportFull = new ReportFull();
        JSONNode upLoadJSON = reportFull.GetReportDatas(Common.configurationController.GetTeacherDirectory());

        if (upLoadJSON != null) { // Ha van mit feltölteni, akkor feltöltjük
            WWWForm upLoadForm = new WWWForm();
            upLoadForm.AddField("json", upLoadJSON.ToString());

            WWW upLoadWWW = new WWW("http://minspire.eu/reportcomm.php", upLoadForm);

            // Várunk amíg a feltöltés befejeződik
            while (!upLoadWWW.isDone)
            {
                // pörgetjük a nyilt másodpercenként egy kör sebességgel
                imageArrows.Rotate(Vector3.forward * 360 * Time.deltaTime);

                // frissítjük a progressBar-t
                textLabelPercent.text = ((int)(upLoadWWW.progress * 100)).ToString() + "%";

                Debug.Log(upLoadWWW.progress);

                yield return null;
            }

            // Ha a feltöltés sikeres volt, akkor töröljük a korábbi adatokat
            if (upLoadWWW.error == null)
            {
                Debug.Log(upLoadWWW.text);
                if (upLoadWWW.text == "OK")
                {
                    // Report mappa tartalmának törlése
                    Common.DeleteDirectoryContent(Common.configurationController.GetReportDirectory());
                }
            }
            else {
                error = true;
                errorText = upLoadWWW.error;
            }
        }


        // Ha a feltöltésnél nem volt hiba, akkor folytatjuk a letöltéssel
        if (!error) {
            // Lekérjük a szervertől a tanár adatait
            JSONClass node = new JSONClass();
            node[C.JSONKeys.task] = C.JSONValues.getalldata;
            node[C.JSONKeys.username] = Common.configurationController.actTeacher.userName;
            node[C.JSONKeys.password] = Common.configurationController.actTeacher.passMD5;

            Debug.Log(node.ToString());

            WWWForm form = new WWWForm();
            form.AddField("json", node.ToString());

            WWW www = new WWW("http://minspire.eu/tabletcomm.php", form);

            // Várunk amíg a letöltés befejeződik
            while (!www.isDone)
            {
                // pörgetjük a nyilt másodpercenként egy kör sebességgel
                imageArrows.Rotate(Vector3.back * 360 * Time.deltaTime);

                // frissítjük a progressBar-t
                textLabelPercent.text = ((int)(www.progress * 100)).ToString() + "%";

                Debug.Log(www.progress);

                yield return null;
            }

            Debug.Log(Common.Now() + " - Download finish.");

            // frissítjük a progressBar-t
            textLabelPercent.text = ((int)(www.progress * 100)).ToString() + "%";

            Debug.Log(www.progress);

            Debug.Log("Downloaded text size : " + www.text.Length);

            //Debug.Log(www.text.Substring(0, 1000));
            // Hibás bejelentkező adatoknál a válasz
            // {"response":"Login failed"}

            // Hiba ellenőrzések
            // check for errors
            if (www != null && www.error == null)
            {   // Megjött a válasz megpróbáljuk feldolgozni a jsont-t
                JSONNode answer = null;

                Debug.Log(Common.Now() + " - Start json parse.");

                try
                {
                    answer = JSON.Parse(www.text);
                }
                catch (System.Exception)
                {
                }
                Debug.Log(Common.Now() + " - Finish json parse.");

                if (answer != null)
                {
                    // Ha nem tartalmazza a response kulcsot, akkor hozzá tesszük.
                    if (!answer.ContainsKey(C.JSONKeys.response))
                    {
                        answer[C.JSONKeys.response] = C.JSONValues.Success;
                    }

                    if (answer.ContainsKey(C.JSONKeys.response))
                    {
                        if (answer[C.JSONKeys.response].Value == C.JSONValues.Success)
                        {
                            Debug.Log(Common.Now() + " - Start AllDataProcessor.");

                            Common.configurationController.teacherConfig.AllDataProcessor(answer);
                            Common.configurationController.teacherConfig.Save();

                            Debug.Log(Common.Now() + " - Finish AllDataProcessor.");

                            if (Common.configurationController.teacherConfig.error)
                            {
                                Common.infoPanelInformation.Show(
                                    Common.languageController.Translate(C.Texts.ErrorUnderProcessing),
                                    true,
                                    (string buttonName) =>
                                    {
                                        Common.menuInformation.Hide();
                                    });
                            }
                            else {
                                Common.menuInformation.Hide();
                            }

                            error = false;  // Nem történt hiba
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

                                error = false;  // A hiba feldolgozás már megtörtént fentebb
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
        }



        // Hiba történt
        if (error)
        {
            Common.infoPanelInformation.Show(
                Common.languageController.Translate(C.Texts.ConnectToServerUnsuccessful) + "\n" +
                Common.languageController.Translate(errorText),
                true,
                (string buttonName) =>
                {
                    // Nyugtázták a hibaüzenetet megnyomták az Ok gombot
                    Common.menuInformation.Hide();
                });
        }

        //yield return null;
        imageLessonPlan.color = Color.white;
        synchronize = false;
    }

    /// <summary>
    /// A UI felületen lévő button komponensek hívják meg ha rákattintottak.
    /// </summary>
    /// <param name="buttonName">Melyik gombra kattintottak.</param>
    public void ButtonClick(string buttonName) {
        Debug.Log(buttonName);

        if (!synchronize) {
            switch (buttonName)
            {
                case "Logout":
                    Common.screenController.ChangeScreen("MenuLogin");
                    break;
                case "LessonPlan":
                    Common.screenController.ChangeScreen("MenuLessonPlanList");
                    break;
                case "Synchronize":
                    if (synchronizeIsPossibility) {
                        synchronize = true;
                        StartCoroutine(Synchronize());
                    }
                    break;
            }
        }
    } 
}
