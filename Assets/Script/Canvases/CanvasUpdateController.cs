using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using SimpleJSON;

public class CanvasUpdateController : HHHScreen {

    [Tooltip("A dátumot kiíró prefab")]
    public GameObject dateTextPrefab;
    [Tooltip("Az információs szöveget kiíró prefab")]
    public GameObject infoTextPrefab;

    GameObject canvasUpdateCheck;
    GameObject canvasUpdateDownload;

    RectTransform content;  // A ScrollView tartalmi része
    RectTransform vLayout;  // Az a konténer ahová a dátumokat és az info szöveget dobni kell.

    Text textUpdateDownload;    // Az Update Download szöveg kiírásához
    UIProgressBar progressBar;  // A letöltési folyamat mutatásához

    Text textUpdate;            // 
    Text textIgnoreUpdate;      // 

    /*
    [Tooltip("Mi az alkalmazás aktuális verzió száma.")]
    public string versionNumber = "2016.05.04v1.apk";          // Mi az aktuális verzió

    public string server = "http://bonis.me/minspire/";

    public string updateJSON = "update.json";
    */
    [HideInInspector]
    public string latestVersionFile;   // Mi az új verziónak a neve (a JSON fájlból lesz kiszedve)

    bool init;
    int counter = 0;

    // Use this for initialization
    new public void Awake () {
        Common.canvasUpdateController = this;

        canvasUpdateCheck = Common.SearchGameObject(gameObject, "CanvasUpdateCheck").gameObject;
        canvasUpdateDownload = Common.SearchGameObject(gameObject, "CanvasUpdateDownload").gameObject;

        content = Common.SearchGameObject(gameObject, "Content").GetComponent<RectTransform>();
        vLayout = Common.SearchGameObject(gameObject, "VLayout").GetComponent<RectTransform>();

        textUpdate = Common.SearchGameObject(gameObject, "TextUpdate").GetComponent<Text>();
        textIgnoreUpdate = Common.SearchGameObject(gameObject, "TextIgnoreUpdate").GetComponent<Text>();

        textUpdateDownload = Common.SearchGameObject(gameObject, "TextUpdateDownload").GetComponent<Text>();
        progressBar = Common.SearchGameObject(gameObject, "ProgressBar").GetComponent<UIProgressBar>();
    }

    // Mielőtt a képernyő láthatóvá válna a ScreenController meghívja ezt a metódust, hogy inicializája magát.
    // Meglehet adni neki egy callBack függvényt, amit akkor hív meg ha befejezte az inicializálást, mivel akár ez sokáig is eltarthat és addig a képernyőt nem kéne megmutatni.
    override public IEnumerator InitCoroutine()
    {
        textUpdateDownload.text = Common.languageController.Translate("UpdateDownload");
        textUpdate.text = Common.languageController.Translate("Update");
        textIgnoreUpdate.text = Common.languageController.Translate("IgnoreUpdate");

        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowStartCoroutine()
    {
        ShowUpdateCheck(true);
        yield break;
    }




    /*
    void Start() {
        init = false;

        StartCoroutine(InitCoroutine());
    }*/

    /*
    IEnumerator InitCoroutine() {

        

        //yield return null;

        
        // Letöltjük az update információt
        WWW www = new WWW(server + updateJSON);
        yield return www;

        if (www.error != null)
        {
            Debug.Log(www.error);

            // Valahova megyünk itt


        }
        else {

            //JSONNode node = JSON.Parse(www.text);
            if (BuildUI(www.text))
            {
                ShowUpdateCheck(false);
            }
            else {

            }

            ShowUpdateCheck(true);
        }
        

    }*/

    void ShowUpdateCheck(bool show) {
        canvasUpdateCheck.SetActive(show);
        canvasUpdateDownload.SetActive(!show);
    }

    /// <summary>
    /// Felépíti a JSON fájl alapján az UI-t, ha szükséges
    /// </summary>
    /// <param name="json">Az json fájl amiben az új verziók megtalálhatóak.</param>
    /// <returns>A visszatérési érték mutatja, hogy van-e újabb verzió, true ha van.</returns>
    public bool BuildUI(JSONNode node) {// string json) {
        // JSONNode node = JSON.Parse(json);

        // A copyNode ba tesszük azokat a frissítési információkat amik relevánsak az adott verzióhoz
        JSONNode copyNode = new JSONClass();

        // Átmásoljuk a frissíteni való node-okat
        for (int i = 0; i < node["versions"].Count; i++)
        {
            JSONNode version = node["versions"][i];

            if (!version["vipUpdate"].AsBool) {
                // Ha nem vip frissítés, akkor mindenkinek szól
                copyNode["versions"][int.MaxValue] = version;
            } else {
                for (int j = 0; j < version["vipMembers"].Count; j++)
                {
                    // Ha a vip tömbben megtaláljuk a saját vip azonosítónkat, akkor nekünk is telepíteni kell
                    if (version["vipMembers"][j].Value == Common.configurationController.DeviceUID) {
                        copyNode["versions"][int.MaxValue] = version;
                        break;
                    }
                }
            }

            if (version["fileName"].Value == Common.configurationController.versionNumber)
                copyNode = new JSONClass();
        };

        Debug.Log("new nodes\n" + copyNode.ToString(" "));

        /*
        // Megkeressük azt a bejegyzést, aminek a verzió száma megegyezik a programéval
        int i = copyNode["versions"].Count;
        while (i > 0 && copyNode["versions"][i - 1]["fileName"].Value != Common.configurationController.versionNumber)
        {
            string s = copyNode["versions"][i - 1]["fileName"].Value;

            Debug.Log(s);

            // Eltároljuk a hátulról az első olyan indexet ami nem teszt verziót tartalmaz


            i--;
        }
        */

        bool isUpdate = false;  // Van-e frissítés
        bool mustUpdate = false; // Ha van frissítés, akkor muszáj az Update-et megcsinálni vagy választható
        int siblingIndex = 0;

        for (int i = 0; i < copyNode["versions"].Count; i++)
        //while (i < copyNode["versions"].Count)
        {
            isUpdate = true;    // Van frissíteni való

            // Létrehozzuk a tartalmakat
            JSONNode version = copyNode["versions"][i];

            if (version["mustUpdate"].AsBool)
                mustUpdate = true;

            latestVersionFile = version["fileName"];

            // Létrehozzuk a dátumot tartalmazó UI elemet
            GameObject go = Instantiate(dateTextPrefab);
            go.transform.SetParent(vLayout, false);
            //go.transform.SetAsFirstSibling();
            go.transform.SetSiblingIndex(siblingIndex++);
            go.GetComponentInChildren<Text>(true).text = version["date"];
            Debug.Log("Dátum létrehozva : " + version["date"]);

            // Létrehozzuk az infót tartalmazó UI elemeket
            for (int j = 0; j < version["info"].Count; j++)
            {
                go = Instantiate(infoTextPrefab);
                go.transform.SetParent(vLayout, false);
                go.transform.SetSiblingIndex(siblingIndex++);
                go.GetComponentInChildren<Text>(true).text = version["info"][j];

                Debug.Log("Dátum létrehozva : " + version["info"][j]);
            }

            //i++;
        }

        // Ha muszáj frissíteni, akkor kikapcsoljuk az ignoreUpdate gombot
        textIgnoreUpdate.transform.parent.gameObject.SetActive(!mustUpdate);

        return isUpdate;
    }

    IEnumerator UpdateDownload(string fileName) {

        ShowUpdateCheck(false);

        WWW www = new WWW(System.IO.Path.Combine(Common.configurationController.server, fileName));

        while (!www.isDone) {
            // frissítjük a progressBar-t
            progressBar.SetProgressValue(www.progress);
            yield return null;
        }

        // Ha a letöltés befejeződött megnézzük, hogy volt-e hiba
        if (www.error != null)
        {
            Debug.Log(www.error);

            // Valahova megyünk itt
            Common.configurationController.DecideServerOrClient();
            //Common.screenController.ChangeScreen(Common.configurationController.nextScreen.name);
        }
        else {
            // Ha a letöltés sikerült mentjük a letöltött fájlt
            string fullPath = System.IO.Path.Combine(Common.configurationController.cacheMappa, fileName);
            System.IO.File.WriteAllBytes(fullPath, www.bytes);

            // Majd elindítjuk a telepítést
            Application.OpenURL(fullPath);

            // Majd kilépünk az alkalmazásból
            Application.Quit();
        }

        yield return null;
    }

    public void ButtonClick(string buttonName) {
        switch (buttonName)
        {
            case "Update":
                Debug.Log("Megnyomták az Update gombot!");

                StartCoroutine(UpdateDownload(latestVersionFile)); // "2016-05-09v02.apk"));

                break;
            case "IgnoreUpdate":
                Debug.Log("Megnyomták az IgnoreUpdate gombot!");
                Common.configurationController.DecideServerOrClient();
                //Common.screenController.ChangeScreen(Common.configurationController.nextScreen.name);
                break;
            default:
                Debug.Log("CanvasUpdateController.ButtonClick - Hibás button név : " + buttonName);

                break;
        }
    }

    // Update is called once per frame
    void Update () {

        // Beállítjuk a magasságát a content-nek
        // Beállítjuk a tartalom méretét, hogy pontosan beleférjen
        // Lekérdezzük a Viewport magasságat, mert annál kisebbnek nem kell lennie a content méretének
        float minHeight = ((RectTransform)content.parent).rect.height;
        float prefHeight = LayoutUtility.GetPreferredHeight(vLayout) + 1;
        if (prefHeight < minHeight)
            prefHeight = minHeight;

        content.sizeDelta = new Vector2(
            content.sizeDelta.x,
            prefHeight
            );

        /*
        counter++;
        if (counter == 10) {
            //StartCoroutine(InitCoroutine());
            init = true;
        }*/
    }
}
