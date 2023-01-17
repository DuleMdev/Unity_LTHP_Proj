using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;
using System;

/*

Két okból jelenik meg ez a képernyő.
Ahhoz, hogy tudassuk az objektummal, hogy melyik okból kell megjelennie a képernyőnek, be kell állítani a firstStart változót.

    
1. Mikor csatlakoztunk a szerverre, akkor azért indul el, hogy letöltse az óramozaikot.
firstStart = true

2. Mikor csoportosítás történt és a tanulónak meg kell keresnie a csoporttársait.
firstStart = false



*/

public class MenuClientWaitStart : HHHScreen
{
    GameObject canvas;

    Text textWaitToStart;           // Várakozás az indításra szöveg kiírásához
    Text textSearchGroupmates;      // Keresd meg a csoport társaidat szöveg kiírásához

    Image imageBackground;          // A háttérkép, ami egyszínű

    Color defaultBackgroundColor;   // Mi a háttér alapszíne

    public bool firstStart;         // Ha true, akkor azonosítót kérünk a szervertől, ha false, akkor csak megmutatjuk a képernyőt

    bool downloading;               // Megy-e az óraterv letöltése
    bool clientIDOk;                // Érkezett már kliens azonosító és elindítottuk az óraterv letöltését, tehát ha közben érkezik egy újabb kliens azonosító nem kell újra elindítani a letöltést

    DateTime idRequestStartTime;

    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        textWaitToStart = Common.SearchGameObject(gameObject, "TextWaitToStart").GetComponent<Text>();
        textSearchGroupmates = Common.SearchGameObject(gameObject, "TextSearchGroupmates").GetComponent<Text>();

        imageBackground = Common.SearchGameObject(gameObject, "ImageBackground").GetComponent<Image>();

        defaultBackgroundColor = imageBackground.color;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően
        textWaitToStart.text = Common.languageController.Translate("Connect success");
        textSearchGroupmates.text = Common.languageController.Translate("SearchGroupmates");

        // Beállítjuk, hogy a szervertől kapjuk meg az adatokat
        Common.HHHnetwork.callBackNetworkEvent = ReceivedNetworkEvent;

        clientIDOk = false; // Még nem kaptunk kliens azonosítót

        //Common.HHHnetwork.SendClientIdentity(); // Elküldjük a kliens adatokat a szervernek

        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        canvas.SetActive(true);

        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        SendIDRequest(); // Azonosító kérést küldünk a szervernek

        Common.HHHnetwork.messageProcessingEnabled = true;
        yield break;
    }

    /// <summary>
    /// Azonosító kérést küldönk a szervernek.
    /// </summary>
    void SendIDRequest()
    {
        idRequestStartTime = DateTime.Now;

        // Kérünk azonosítót a szervertől
        JSONClass node = new JSONClass();
        node[C.JSONKeys.dataContent] = C.JSONValues.IDRequest;
        node[C.JSONKeys.clientUniqueIdentifier] = Common.configurationController.DeviceUID;

        // Elküldjük a szervernek a kérést
        Common.HHHnetwork.SendMessageClientToServer(node);
    }

    /// <summary>
    /// Küldünk a szervernek egy lecketerv kérést.
    /// </summary>
    IEnumerator SendLessonPlanRequest() {
        downloading = true;

        // Lekérjük a szervertől az óratervet
        JSONClass node = new JSONClass();
        node[C.JSONKeys.dataContent] = C.JSONValues.lessonPlanRequest;

        // Elküldjük a szervernek a kérést
        Common.HHHnetwork.SendMessageClientToServer(node);

        // Feldobunk egy progressBar ablakot ahol a letöltés folyamatát mutatjuk
        Common.infoPanelProgressBar.Show(C.Texts.LoadLessonPlan);

        // Amíg nem feleződött be a letöltés, addig frissítjük a progressBar-t
        while (downloading) {
            Common.infoPanelProgressBar.SetProgressBarValue(Common.HHHnetwork.GetClientReceivedPercent());
            yield return null;
        }

        // Common.menuInformation.Hide();
    }

    /// <summary>
    /// Elküldjük a szervernek, hogy a lecketerv sikeresen megérkezett.
    /// </summary>
    void SendLessonPlanTransferOk()
    {
        // Tudatjuk a szerverrel, hogy a letöltés sikerült.
        JSONClass node = new JSONClass();
        node[C.JSONKeys.dataContent] = C.JSONValues.lessonPlanTransferOk;

        // Elküldjük a szervernek az üzenetet
        Common.HHHnetwork.SendMessageClientToServer(node);
    }


    /// <summary>
    /// Feldolgozzuk a lecketervet, azaz átdolgozzuk LessonPlanData objektumba a json-t.
    /// </summary>
    /// <param name="lessonPlanJSON"></param>
    /// <returns></returns>
    IEnumerator LessonPlanProcessing(JSONNode lessonPlanJSON)
    {
        // Mutatjuk a lecketerv feldolgozás infoPanel-t
        Common.infoPanelInformation.Show(C.Texts.ProcessingLessonPlan, false, null);
                                          
        // Várunk amíg az előzőekben kért panel meg nem jelenik teljes egészében
        while (!Common.menuInformation.show)
            yield return null;

        Debug.Log(System.DateTime.Now.Ticks);

        // Feldolgozzuk a lecketervet
        Common.configurationController.lessonPlanData = new LessonPlanData();
        Common.configurationController.lessonPlanData.LoadDataFromString(lessonPlanJSON.ToString());

        //Common.configurationController.lessonPlanData = new LessonPlanData(lessonPlanJSON.ToString());

        Debug.Log(System.DateTime.Now.Ticks);

        if (Common.configurationController.lessonPlanData.error)
        {
            // Ha hiba történt a feldolgozás során tudatjuk a felhasználót, majd kilépünk a clientStartScreen képernyőre
            Common.infoPanelInformation.Show(C.Texts.LessonPlanError + "\n" + Common.configurationController.lessonPlanData.errorMessage, true, (string buttonName) => {
                Common.menuInformation.Hide(() => {
                    Common.HHHnetwork.StopHost();
                    Common.screenController.ChangeScreen(C.Screens.ClientStartScreen);
                });
            } );

        }
        else {
            // Elmentjük az aktuális óratervet, hogy ne kelljen újból letölteni, ha ugyan erre lesz szükség
            string fileName = System.IO.Path.Combine(Common.GetDocumentsDir(), C.DirFileNames.lastLessonPlanName);
            System.IO.File.WriteAllText(fileName, lessonPlanJSON.ToString(" "));

            // Ha hibátlanul lefutott minden
            Common.menuInformation.Hide();

            // Az adathalmaz sikeres átvételéről küldünk a szervernek információt, azaz készek vagyunk a játékok futtatására
            SendLessonPlanTransferOk();
        }
    }

    // Ha letiltják a gameObject-et, akkor letiltjuk a Canvast, hogy ha engedélyezik, akkor a Canvas ne legyen látható azonnal
    void OnDisable()
    {
        canvas.SetActive(false);
    }

    // Network objektum hívja meg ha érkezett valamilyen esemény
    void ReceivedNetworkEvent(NetworkEventType networkEventType, int connectionID, JSONNode receivedData)
    {
        switch (networkEventType)
        {
            case NetworkEventType.DataEvent:
                switch (receivedData[C.JSONKeys.dataContent])
                {
                    
                    case C.JSONValues.clientID:

                        // Kiszámoljuk szerver és a kliens óra közötti eltérést
                        TimeSpan t1 = DateTime.Now.Subtract(idRequestStartTime); 
                        //TimeSpan t2 = DateTime.Now.Subtract(Common.StringToDateTime(receivedData[C.JSONKeys.serverDateTime]));
                        TimeSpan t2 = Common.StringToDateTime(receivedData[C.JSONKeys.serverDateTime]).Subtract(DateTime.Now);
                        Common.deltaTimeSpan = new TimeSpan(t2.Ticks + t1.Ticks / 2);
                        Common.configurationController.Log("Kliens idő korrigálása a szerver időhöz : " + Common.deltaTimeSpan.ToString());

                        if (!clientIDOk)
                        {
                            // Betöltjük az utolsó óratervet amit a kliens használt
                            bool lessonPlanIsPresent = true; // Van-e használható lecketerv a kliens gépen
                            try
                            {
                                string fileName = System.IO.Path.Combine(Common.GetDocumentsDir(), C.DirFileNames.lastLessonPlanName);
                                Common.configurationController.lessonPlanData = new LessonPlanData();
                                Common.configurationController.lessonPlanData.LoadDataFromString(System.IO.File.ReadAllText(fileName));
                                if (Common.configurationController.lessonPlanData.error)
                                    throw new System.Exception();
                            }
                            catch (System.Exception)
                            {
                                lessonPlanIsPresent = false;
                            }

                            // Ha van óraterv és az ugyan az mint amit már korábban letöltött
                            if (lessonPlanIsPresent &&
                                receivedData[C.JSONKeys.lessonid].AsInt == Common.configurationController.lessonPlanData.id &&
                                receivedData[C.JSONKeys.lessonSynchronizeTime].Value == Common.configurationController.lessonPlanData.lessonSynchronizeTime)
                            {
                                // elküldünk a szervernek, hogy készek vagyunk a játékok futtatására
                                SendLessonPlanTransferOk();
                            }
                            else
                            {
                                // Letöltjük a szervertől az óratervet
                                StartCoroutine(SendLessonPlanRequest());
                            }
                        }

                        clientIDOk = true;
                        // Feldobunk egy progressBar-t, ami majd mutatja a letöltési folyamatot

                        break;

                    case C.JSONValues.lessonPlan:

                        downloading = false; // Megérkezett az óraterv

                        StartCoroutine(LessonPlanProcessing(receivedData[C.JSONKeys.lessonPlan]));
                        // JSONNode node = JSONNode.Parse(Common.configurationController.tasksInJSON.text);
                        // Common.taskController.PlayQuestionList(node["flows"][flowIndex].ToString(), false, () => { Common.screenController.ChangeScreen(gameObject.name); });


                        break;
                }

                break;
        }
    }

    // Beállítja a háttérszínt a klinens csoport számának megfelelően
    void SetColor()
    {

        imageBackground.color = (Common.configurationController.userGroup == -1) ? defaultBackgroundColor : Common.configurationController.groupColors[Common.configurationController.userGroup];
        //imageBackground.color = Common.configurationController.groupColors[Common.taskController.groupID];
    }

    /*
    // Az Update figyeli, hogy ha megszünt a kapcsolat a szerverrel, akkor menjünk a kliens menübe, hogy újra tudjunk csatlakozni
    void Update()
    {
        // Ha a kapcsolat megszünt a szerverrel és ez a képernyő aktív és az u, akkor kilépünk a kliens menübe
        if (!Common.HHHnetwork.clientIsConnected && // Ha a kapcsolat megszünt a szerverrel 
            Common.screenController.actScreen == this && // és ez a képernyő az aktív
            Mathf.Abs(lastGoMenu - Time.time) > 5) // és 5 másodperc már eltelt az előző menjünk a menübe kérés óta
        {
            lastGoMenu = Time.time;
            Common.screenController.ChangeScreen("CanvasScreenClientMenu");
        }
    }
    */
}
