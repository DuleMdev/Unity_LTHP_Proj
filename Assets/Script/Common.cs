using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

using SimpleJSON;
using System.Security.Cryptography;
using System.Text;
//using System;

/*
Ezen az objektumon keresztűl tartják a szkriptek egymással a kapcsolatot.

Minden fontosabb szkript a létrejövésekor az Awake metódusában beállítja a hozzátartozó itt felsorolt statikus változót saját magára.
Ezek után a többi szkript eltudja ezen az objektumon keresztűl érni és a matódusait meghívni.

*/

static public class Common {

    public delegate void CallBack();   // Egy paramétert nem váró és vissza nem adó függvényt deklarálunk
    public delegate void CallBack_In_Object(Object o);  // Egy objektumot váró függvény
    public delegate void CallBack_In_String(string s); // Egy stringet váró függvény
    public delegate void CallBack_In_Int(int i); // Egy intet váró függvény
    public delegate void CallBack_In_Bool(bool b); // Egy bool-t váró függvény
    public delegate void CallBack_In_ByteArray(byte[] byteArray);   // Egy bájt tömböt váró függvény
    public delegate void CallBack_In_JSONNode(JSONNode jsonNode);   // Egy JSONNode objektumot váró függvény
    public delegate void CallBack_In_Bool_String(bool b, string s);   // Egy bool és egy string-et váró függvény
    public delegate void CallBack_In_Int_JSONNode(int i, JSONNode jsonNode);   // Egy int-et és egy JSONNode objektumot váró függvény
    public delegate void CallBack_In_Bool_JSONNode(bool b, JSONNode jsonNode);   // Egy bool és egy JSONNode objektumot váró függvény
    public delegate void CallBack_In_String_String(string s1, string s2); // két stringet váró függvény
    public delegate void CallBack_In_Int_String(int i, string s); // Egy intet és egy stringet váró függvény
    public delegate void CallBack_In_Int_Int(int i1, int i2); // Két intet váró függvény
    public delegate void CallBack_In_NetworkEventType_Int_JSONNode(NetworkEventType net, int i, JSONNode jsonNode);
    public delegate string CallBack_In_String_Out_String(string s); // Egy stringet váró függvény
    public delegate int CallBack_In_Int_Int_String_Out_Int(int a, int b, string c);
    public delegate Sprite CallBack_In_String_Out_Sprite(string x);
    public delegate void CallBack_In_Sprite(Sprite sprite);
    public delegate void CallBack_In_ListLanguageData(List<LanguageData> list);

    static public Stopper stopper;

    static public CanvasScreenController canvasScreenController;    // A canvas típusú képernyő váltásokat kezeli
    static public ScreenController screenController;    // A gameObject típusú képernyő váltásokat kezeli
    //static public ScreenControllerOld screenController;    // A gameObject típusú képernyő váltásokat kezeli
    //static public FadeEffect fadeEffect;                // A fadeEffect-et megvalósító szkript (az egyébb képernyőváltás effecteket a sceneController valósítja meg)
    static public FadeEffectCanvas fadeEffect;                // A fadeEffect-et megvalósító szkript (az egyébb képernyőváltás effecteket a sceneController valósítja meg)
    static public AudioController audioController;      // A hangok lejátszásához

    static public TaskControllerOld taskControllerOld;    // Feladatokat tartalmazza és menedzseli
    static public TaskController taskController;    // Feladatokat tartalmazza és menedzseli
    static public LanguageController languageController; // Az elérhető nyelveket és fordításokat tartalmazza
    static public PictureController pictureControllerr;  // A képeket tölti be a háttértárról
    static public PictureRepository pictureRepository;  // WebGL esetben a háttértárról nem elérhetőek a képek, ezért ebben az objektumban tárolom őket

    //static public NetworkTest HHHnetwork;
    static public MenuClientGrouping menuClientGrouping;

    static public HHHNetwork HHHnetwork;
    static public CanvasNetworkHUD canvasNetworkHUD;
    static public ConfigurationController configurationController;

    static public CanvasBackground canvasBackground;
    static public CanvasInformation canvasInformation;          // Információk, illetve hibaüzenetek megjelenítésére
    static public CanvasDark canvasDark;                // Besötétíti a képernyőt ha nem a játékos következik lépéssel
    static public CanvasUpdateController canvasUpdateController;    // Update Infók kiírását és a frissítés lebonyolítását végző objektum

    static public Background menuBackground;    // A menürendszer háttérképét szolgáltató szkript
    //static public CanvasMenuStripe canvasMenuStripe;// A menü felső sávját adja
    static public MenuStripe menuStripe;            // A menü felső sávját adja

    static public MenuInformation menuInformation;  // Információs megjelenítő tábla alap
    static public InfoPanelSelectGroupNumber infoPanelSelectGroupNumber;    // Egy csoportba szervezendő tanulók számának beállításához
    static public InfoPanelSureStartLessonPlan infoPanelSureStartLessonPlan;    // Óraterv indításának megerősítéséhez
    static public InfoPanelExitFromLessonPlan infoPanelExitFromLessonPlan;  // Az óratervből való kilépés megerősítéséhez
    static public InfoPanelPauseLessonPlan infoPanelPauseLessonPlan;    // Az óraterv szüneteltetéséhez
    static public InfoPanelAutoGroup infoPanelAutoGroup;    // Óra-mozaik automatikus csoportbeosztáshoz
    static public InfoPanelAutoStartNextLessonMosaic infoPanelAutoStartNextLessonMosaic;    // A következő óra-mozaik automatikus inditásához
    static public InfoPanelPassword infoPanelPassword;  // A jelszó bekérő infopanel
    static public InfoPanelAfterGrouping infoPanelAfterGrouping;    // Csoportosítás utáni diák rendeződés képernyője
    static public InfoPanelSinglePlayerStart infoPanelSinglePlayerStart;    // Egyéni óra-mozaik indítása
    static public InfoPanelInformationWithOkButton infoPanelInformationWithOkButton;
    static public InfoPanelInformationWithoutButtons infoPanelInformationWithoutButtons;
    static public InfoPanelInformation infoPanelInformation;
    static public InfoPanelInformationOkCancel infoPanelInformationOkCancel;
    static public InfoPanelProgressBar infoPanelProgressBar;
    static public InfoPanelClassRoster infoPanelClassRoster;
    static public InfoPanelClassSelector infoPanelClassSelector;

    static public EvaluationScreenSingle evaluationScreenSingle;

    static public PDFController pdfController;

    static public GameMaster gameMaster;

    static public MenuLessonPlan menuLessonPlan;

    static public Game_Bubble_Old game_Bubble;

    static public BackgroundFade fade;          // Ha el kell takarni a dolgokat egy panel megjelenésénél
    static public BackgroundFade fadeError;     // Ha el kell takarni a dolgokat hiba üzenet megjelenésénél
    static public BackgroundFade fadeOTPMain;   // Ha el kell takarni a dolgokat az OTPMain képernyőn (Project termék lejátszásánál felbukkanó panel esetén)

    static public bool fadeActive {
        get {
            return fade.fadeActive || fadeError.fadeActive || Zoomer.instance.active || GamePlusQuestionInfo.instance.show;
        }
    }

    static public System.Random random = new System.Random();

    static public System.TimeSpan deltaTimeSpan = new System.TimeSpan(0);

    /// <summary>
    /// A megadott listában található elemeket véletlenszerűen megkeveri.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">A lista amit meg kell keverni.</param>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// A megadott listában található elemek sorrendjét megváltoztatja az index tömben magadottak szerint.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">A lista amelynek a sorrendjét meg kell változtatni.</param>
    /// <param name="indexArray">Az új sorrendet tartalmazó tömb.</param>
    public static void ShuffleByIndexArray<T>(this IList<T> list, int[] indexArray)
    {
        if (list.Count != indexArray.Length)
            return;

        // Lemásoljuk az eredeti listát
        List<T> newList = new List<T>();
        foreach (var item in list)
            newList.Add(item);

        // Az eredeti listában a sorrendet az index tömb által megadottra változtatjuk
        for (int i = 0; i < indexArray.Length; i++)
            list[i] = newList[indexArray[i]];
    }


    // A megadott gameObject-nek az elhelyezkedését adja vissza
    public static string GetGameObjectHierarchy(GameObject go)
    {
        string path = "";
        Transform transform = go.transform;

        do
        {
            path = "/" + transform.name + path;
            transform = transform.parent;
        } while (transform != null);

        return path;
    }

    /// <summary>
    /// Kilistázza a megadott gameObject és a szülői aktív állapotát.
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static string GetGameObjectActive(GameObject go)
    {
        string path = "";
        Transform transform = go.transform;

        do
        {
            path += transform.name + " - active : " + transform.gameObject.activeSelf + " activeInHierarchy : " + transform.gameObject.activeInHierarchy + "\n";
            transform = transform.parent;
        } while (transform != null);

        return path;

    }

    // Megvizsgálja, hogy a megadott child GameObject az valóban a parent leszármazotja
    // Ha igen, akkor true értéket ad vissza
    public static bool IsDescendant(Transform parent, Transform child) {
        do {
            if (child == parent) return true;
            child = child.parent;
        } while (child != null);

        return false;
    }

    // Megkeresi az adott nevű GameObject-et
    // Ha az adott GameObjectből kiíndulva nem találja meg, akkor null értéket ad vissza
    public static GameObject SearchGameObject(GameObject parent, string name, params GameObject[] ignoreGameObjectList)
    {
        /*
        string[] strings = name.Split('/');

        foreach (string s in strings)
        {

        }
        */
        Transform[] transforms = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform transform in transforms) {

            // Megvizsgáljuk, hogy az aktuális transform az ignoráltGameObject-ek gyereke-e

            bool isDescendant = false;
            foreach (GameObject gameObject in ignoreGameObjectList)
            {
                isDescendant = IsDescendant(gameObject.transform, transform);
                if (isDescendant) break;
            }

            if (!isDescendant && transform.name == name)
                return transform.gameObject;
        }

        return null;
    }

    public static GameObject SearchChild(this GameObject g, string childName, params GameObject[] ignoreGameObjectList)
    {
        string[] buttonNameSplitted = childName.Split('/');

        Transform[] transforms = g.GetComponentsInChildren<Transform>(true);

        foreach (Transform transform in transforms) {
            // Saját magát ne tuddja megtalálni
            if (transform == g.transform) 
                continue;

            bool isDescendant = false;
            foreach (GameObject gameObject in ignoreGameObjectList)
            {
                isDescendant = IsDescendant(gameObject.transform, transform);
                if (isDescendant) break;
            }

            if (!isDescendant && transform.name == buttonNameSplitted[0])
            {
                if (buttonNameSplitted.Length == 1)
                    return transform.gameObject;
                else
                    return transform.gameObject.SearchChild(childName.Remove(0, buttonNameSplitted[0].Length + 1), ignoreGameObjectList);
            }
        }

        return null;
    }

    /// <summary>
    /// A metódus vissza ad egy kevert egészekből álló listát, ahol a számok a startIndex -től kezdődően count-1 elemig vannak. 
    /// például ha a count = 5 a startIndex = 0, akkor 0 - 4 -ig a számokat megkeveri és visszaadja egy öt elemű tömbben pl. [3, 2, 4, 1, 0]
    /// </summary>
    /// <param name="count">Hány darab egymást követő számot adjon vissza keverve.</param>
    /// <param name="startIndex">Melyik számtól kezdődjön a lista.</param>
    /// <returns></returns>
    public static int[] GetRandomNumbers(int count, int startIndex = 0)
    {
        if (count < 0)
            count = 0;

        int[] list = new int[count];

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, i + 1);
            list[i] = list[randomIndex];
            list[randomIndex] = i + startIndex;
        }

        return list;
    }

    public static int[] GetRandomNumbersWithFix(int count, int fix) {
        // Létrehozzuk a szükséges méretű tömböt a kevert indexek tárolására
        int[] order = new int[count];
        // A nem fix értékek indexeire kérünk egy kevert listát
        int[] randomized = Common.GetRandomNumbers(count - fix, fix);

        // Feltöltjük a sorrend tömböt
        for (int i = 0; i < count; i++)
        {
            if (i < fix)
                // Amíg fix a sorrend addig sorba tesszük az indexeket a tömbbe
                order[i] = i;
            else
                // Hamár nem fix a sorrend, akkor a kevert sorrendet másoljuk át
                order[i] = randomized[i - fix];
        }

        return order;
    }

    /// <summary>
    /// Vissza ad egy int listát aminek az elemei nullától egyesével növekednek.
    /// Tehát ha az érték 5, akkor a vissza adott érték [0, 1, 2, 3, 4] lesz.
    /// </summary>
    /// <param name="count">Milyen hosszú legyen az int lista.</param>
    /// <returns></returns>
    public static List<int> GetIntList(int count) {
        List<int> list = new List<int>();

        for (int i = 0; i < count; i++)
            list.Add(i);

        return list;
    }

    /// <summary>
    /// Vissza ad egy int tömböt aminek az elemei nullától egyesével növekednek.
    /// Tehát ha az érték 5, akkor a vissza adott érték [0, 1, 2, 3, 4] lesz.
    /// </summary>
    /// <param name="count">Milyen hosszú legyen az int tömb.</param>
    /// <returns></returns>
    public static int[] GetIntArray(int count) {
        return GetIntList(count).ToArray();
    }

    /// <summary>
    /// Egy listába beteszi a megadott helyre a megadott elemet. Ha a lista rövidebb, akkor kibővíti a listát a megadott elemmel.
    /// Ha a listába már van olyan indexű elem, akkor azt felülírja.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">A lista amibe be kell tenni az új elemet.</param>
    /// <param name="index">A lista melyik indexébe kell tenni az elemet.</param>
    /// <param name="item">A berakandó elem.</param>
    /// <param name="defaultValue">Ha bővíteni kell a listát milyen értékket tegyen a közbenső elemekbe.</param>
    public static void ListAdd<T>(List<T> list, int index, T item, T defaultValue)
    {
        while (index >= list.Count)
            list.Add(defaultValue);

        list[index] = item;
    }

    // newV3 = Common.ChangeVector(V3, z: 3);

    public static Vector3 ChangeVector3(Vector3 vector, float? x = null, float? y = null, float? z = null) {
        return new Vector3(
            (x == null) ? vector.x : x.Value,
            (y == null) ? vector.y : y.Value,
            (z == null) ? vector.z : z.Value
            );
    }

    // newV3 = V3.Set(x: 3);

    /// <summary>
    /// Megváltoztatja a vektor komponenseinek értékét, ha megadjuk.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x">Az új X érték, vagy null ha nem akarjuk megváltoztatni.</param>
    /// <param name="y">Az új Y érték, vagy null ha nem akarjuk megváltoztatni.</param>
    /// <param name="z">Az új Z érték, vagy null ha nem akarjuk megváltoztatni.</param>
    /// <returns></returns>
    public static Vector3 Set(this Vector3 v, float? x = null, float? y = null, float? z = null) {
        return new Vector3(
            (x == null) ? v.x : x.Value,
            (y == null) ? v.y : y.Value,
            (z == null) ? v.z : z.Value
            );
    }

    /// <summary>
    /// Megváltoztatja a vektor X komponensének az értékét a megadottra.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x">Az új X érték.</param>
    /// <returns></returns>
    public static Vector3 SetX(this Vector3 v, float x)
    {
        return new Vector3(x, v.y, v.z);
    }

    /// <summary>
    /// Megváltoztatja a vektor Y komponensének értékét a megadottra.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="y">Az új Y érték.</param>
    /// <returns></returns>
    public static Vector3 SetY(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }

    /// <summary>
    /// Megváltoztatja a vektor Z komponensének értékét a megadottra.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="z">Az új Z érték.</param>
    /// <returns></returns>
    public static Vector3 SetZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }

    /// <summary>
    /// A vektor megadott komponenseihez hozzáadja a megadott értéket.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x">A vektor X irányú változása, vagy nulla ha nem akarjuk megváltoztatni.</param>
    /// <param name="y">A vektor Y irányú változása, vagy nulla ha nem akarjuk megváltoztatni.</param>
    /// <param name="z">A vektor Z irányú változása, vagy nulla ha nem akarjuk megváltoztatni.</param>
    /// <returns></returns>
    public static Vector3 Add(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(
            (x == null) ? v.x : v.x + x.Value,
            (y == null) ? v.y : v.y + y.Value,
            (z == null) ? v.z : v.z + z.Value
            );
    }

    /// <summary>
    /// A vektor X komponenséhez hozzáadja a megadott értéket.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x">X értékhez hozzáadandó érték.</param>
    /// <returns></returns>
    public static Vector3 AddX(this Vector3 v, float x)
    {
        return new Vector3(v.x + x, v.y, v.z);
    }

    /// <summary>
    /// A vektor Y komponenséhez hozzáadja a megadott értéket.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="y">X értékhez hozzáadandó érték.</param>
    /// <returns></returns>
    public static Vector3 AddY(this Vector3 v, float y)
    {
        return new Vector3(v.x, v.y + y, v.z);
    }

    /// <summary>
    /// A vektor Z komponenséhez hozzáadja a megadott értéket.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="z">Z értékhez hozzáadandó érték.</param>
    /// <returns></returns>
    public static Vector3 AddZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, v.z + z);
    }

    /// <summary>
    /// A vektor megadott komponenseit megszorozza a megadott értékkel.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x">A vektor X komponensének szorzója, vagy nulla ha nem akarjuk megváltoztatni.</param>
    /// <param name="y">A vektor Y komponensének szorzója, vagy nulla ha nem akarjuk megváltoztatni.</param>
    /// <param name="z">A vektor Z komponensének szorzója, vagy nulla ha nem akarjuk megváltoztatni.</param>
    /// <returns></returns>
    public static Vector3 Mul(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(
            (x == null) ? v.x : v.x * x.Value,
            (y == null) ? v.y : v.y * y.Value,
            (z == null) ? v.z : v.z * z.Value
            );
    }

    /// <summary>
    /// A vektor X kompnensét megszorozza a megadott értékkel.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x">X érték szorzója.</param>
    /// <returns></returns>
    public static Vector3 MulX(this Vector3 v, float x)
    {
        return new Vector3(v.x * x, v.y, v.z);
    }

    /// <summary>
    /// A vektor Y komponensét megszorozza a megadott értékkel.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="y">Y érték szorzója.</param>
    /// <returns></returns>
    public static Vector3 MulY(this Vector3 v, float y)
    {
        return new Vector3(v.x, v.y * y, v.z);
    }

    /// <summary>
    /// A vektor Z kompnensét megszorozza a megadott értékkel.
    /// </summary>
    /// <param name="v"></param>
    /// <param name="z">Z érték szorzója.</param>
    /// <returns></returns>
    public static Vector3 MulZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, v.z * z);
    }

    /// <summary>
    /// Megváltoztatja a szín komponenseinek azon értékét amit megadtunk.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="r">Az új piros érték, vagy null ha nem akarjuk megváltoztatni.</param>
    /// <param name="g">Az új zöld érték, vagy null ha nem akarjuk megváltoztatni.</param>
    /// <param name="b">Az új kék érték, vagy null ha nem akarjuk megváltoztatni.</param>
    /// <param name="a">Az új átlátszósági érték, vagy null ha nem akarjuk megváltoztatni.</param>
    /// <returns></returns>
    public static Color Set(this Color c, float? r = null, float? g = null, float? b = null, float? a = null)
    {
        return new Color(
            (r == null) ? c.r : r.Value,
            (g == null) ? c.g : g.Value,
            (b == null) ? c.b : b.Value,
            (a == null) ? c.a : a.Value
            );
    }

    /// <summary>
    /// Megváltoztatja a szín piros komponensének értékét.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="r">A piros komponens új értéke.</param>
    /// <returns></returns>
    public static Color SetR(this Color c, float r) {
        return new Color(r, c.g, c.b, c.a);
    }

    /// <summary>
    /// Megváltoztatja a szín zöld komponensének értékét.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="g">A zöld komponens új értéke.</param>
    /// <returns></returns>
    public static Color SetG(this Color c, float g)
    {
        return new Color(c.r, g, c.b, c.a);
    }

    /// <summary>
    /// Megváltoztatja a szín kék komponensének értékét.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="b">A kék komponens új értéke.</param>
    /// <returns></returns>
    public static Color SetB(this Color c, float b)
    {
        return new Color(c.r, c.g, b, c.a);
    }

    /// <summary>
    /// Megváltoztatja a szín átlátszósági komponensének értékét.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="a">Az átlátszósági komponens új értéke.</param>
    /// <returns></returns>
    public static Color SetA(this Color c, float a)
    {
        return new Color(c.r, c.g, c.b, a);
    }

    // Beállítja a megadott objektumok pozícióit úgy, hogy a baseGameObject pozíciója középen legyen az objektumok által lefedett területnek
    // Vissza adja az elemek teljes szélességét
    public static float PositionToBase(GameObject baseGameObject, List<MonoBehaviour> listItems, float spaceBetweenItems) {

        // Kiszámoljuk a teljes szélességet
        float fullWidth = 0;

        foreach (MonoBehaviour item in listItems) 
            fullWidth += ((IWidthHeight)item).GetWidth();

        if (listItems.Count > 0)
            fullWidth += (listItems.Count - 1) * spaceBetweenItems;

        // Elhelyezzük az objektumokat
        float posX = fullWidth / -2;

        for (int i = 0; i < listItems.Count; i++)
        {
            // Ha nem az első elemnél tartunk, akkor hozzáadjuk az elemek közti távolságot is
            if (i != 0) posX += spaceBetweenItems; 

            // Elhelyezzük az aktuális elemet
            float width = ((IWidthHeight)listItems[i]).GetWidth();
            listItems[i].transform.position = new Vector3(baseGameObject.transform.position.x + posX + width / 2, baseGameObject.transform.position.y, baseGameObject.transform.position.z);

            // Kiszámoljuk a következő bal szélének pozícióját
            posX += width;
        }

        return fullWidth;
    }

    // A megadott értékből annyit konvertál át számmá, amennyi számként felismerhető
    static public string StringToIntToString(string input) {

        string result = "";

        if (input != "")
        {
            Match match = Regex.Match(input, @"\A\s*\d*");

            int i;
            int.TryParse(match.Value, out i);

            result = i.ToString();

            //Debug.Log("StringToInt : (" + match.Value + ") " + input + " -> " + result);
        }

        return result;
    }

    /// <summary>
    /// A megadott capture-n belül a megadott csoportindexből az első hozzá tartozó capture-t adja vissza.
    /// </summary>
    static public Capture GetFirstSubCapture(Match match, Capture capture, int groupIndex)
    {
        List<Capture> captures = GetSubCaptures(match, capture, groupIndex);

        return captures.Count > 0 ? captures[0] : null;
    }

    /// <summary>
    /// A megadott capture-n belül a megadott csoportindexből az összes hozzá tartozó capture-t vissza adja.
    /// </summary>
    static public List<Capture> GetSubCaptures(Match match, Capture capture, int groupIndex)
    {
        List<Capture> captures = new List<Capture>();
        if (capture == null || match == null)
            return captures;

        int minIndex = capture.Index;
        int maxIndex = capture.Index + capture.Length;

        if (groupIndex >= 0 && groupIndex < match.Groups.Count)
        {
            for (int i = 0; i < match.Groups[groupIndex].Captures.Count; i++)
            {
                Capture cap = match.Groups[groupIndex].Captures[i];
                if (cap.Index >= minIndex && cap.Index + cap.Length <= maxIndex)
                    captures.Add(cap);
            }
        }

        return captures;
    }

    /// <summary>
    /// A szöveget átkonvertálja regex-ben használható karakterekké. (A regex-ben bizonyos karaktereknek speciális jelentése van, azok elé egy \ jelet tesz, hogy semlegesítse a speciális jelentését)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    static public string ConvertStringToRegEx(string s)
    {
        StringBuilder sb = new StringBuilder();
        string forbiddenChars = @".$^{[(|)]}*+?\";

        foreach (var item in s)
        {
            if (forbiddenChars.Contains(item.ToString()))
                sb.Append(@"\");

            sb.Append(item);
        }

        return sb.ToString();
    }

    static public string GetDocumentsDir()
    {
        return Application.persistentDataPath;

        /*
        #if UNITY_EDITOR
        return Application.streamingAssetsPath;
        #endif

        #if UNITY_IOS
		return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        #endif

        #if UNITY_ANDROID
        return Application.persistentDataPath;
        #endif

        #if UNITY_STANDALONE_OSX
		return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        #endif
        */
    }

    public static void DeleteDirectoryContent(string directory)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = System.IO.Directory.GetFiles(directory);
        foreach (string fileName in fileEntries)
            System.IO.File.Delete(fileName);

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = System.IO.Directory.GetDirectories(directory);
        foreach (string subdirectory in subdirectoryEntries) {
            DeleteDirectoryContent(subdirectory);
            System.IO.Directory.Delete(subdirectory);
        }
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    /// <summary>
    /// A megadott hexadecimális formában megadott szint vissza adja Color objektumként
    /// </summary>
    /// <param name="hexa">Hexadecimális formában megadott szín pl. (#RGB, #RRGGBB, #RGBA, #RRGGBBAA, black, yellow)</param>
    /// <returns></returns>
    public static Color MakeColor(string hexa) {
        Color mewColor = new Color();
        ColorUtility.TryParseHtmlString(hexa, out mewColor);

        return mewColor;
    }

    /// <summary>
    /// Átmásolja egy stream tartalmát egy másikba
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    public static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[32768];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }

    /// <summary>
    /// Egy int típusú tömböt JSONArray objektummá konvertál.
    /// </summary>
    /// <param name="array">A konvertálandó int tömb.</param>
    /// <returns></returns>
    public static JSONArray ArrayToJSON(int[] array) {
        JSONArray jsonArray = new JSONArray();

        for (int i = 0; i < array.Length; i++)
        {
            jsonArray[i].AsInt = array[i];
        }

        return jsonArray;
    }

    /// <summary>
    /// Egy JSONArray típusú tömböt int tömbbé konvertál.
    /// </summary>
    /// <param name="jsonArray">A konvertálandó JSONArray objektum.</param>
    /// <returns></returns>
    public static int[] JSONToArray(JSONNode jsonArray) {
        int[] array = new int[jsonArray.Count];

        for (int i = 0; i < jsonArray.Count; i++)
        {
            array[i] = jsonArray[i].AsInt;
        }

        return array;
    }

    public static string Now()
    {
        return System.DateTime.Now.AddTicks(deltaTimeSpan.Ticks).ToString("HH:mm:ss.fff");
    }

    public static string TimeStamp()
    {
        return TimeStamp(System.DateTime.Now);
    }

    public static string TimeStamp(System.DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    public static string TimeStampWithSpace()
    {
        return System.DateTime.Now.ToString("yyyy MM dd HH mm ss fff");
    }

    public static System.DateTime StringToDateTime(string dateTime, string format = "yyyy-MM-dd HH:mm:ss.fff") {
        string year = "";
        string mouth = "";
        string day = "";
        string hour = "";
        string minute = "";
        string second = "";
        string msecond = "";

        for (int i = 0; i < Mathf.Min(dateTime.Length, format.Length); i++)
        {
            char f = format[i];
            char c = dateTime[i];

            switch (f)
            {
                case 'y': year += c; break;
                case 'M': mouth += c; break;
                case 'd': day += c; break;
                case 'H': hour += c; break;
                case 'm': minute += c; break;
                case 's': second += c; break;
                case 'f': msecond += c; break;
            }
        }

        if (string.IsNullOrWhiteSpace(year)) year = "1";
        if (string.IsNullOrWhiteSpace(mouth)) mouth = "1";
        if (string.IsNullOrWhiteSpace(day)) day = "1";
        if (string.IsNullOrWhiteSpace(hour)) hour = "0";
        if (string.IsNullOrWhiteSpace(minute)) minute = "0";
        if (string.IsNullOrWhiteSpace(second)) second = "0";
        if (string.IsNullOrWhiteSpace(msecond)) msecond = "0";

        return new System.DateTime(System.Int32.Parse(year), System.Int32.Parse(mouth), System.Int32.Parse(day), System.Int32.Parse(hour), System.Int32.Parse(minute), System.Int32.Parse(second), System.Int32.Parse(msecond));
    }

    /// <summary>
    /// A megadott stringből vissza ad egy rész stringet.
    /// Ha a kívánt rész kisebb mint a string teljes hossza, akkor a teljes stringet fogja visszaadni.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string GetStringPart(string s, int count) {
        if (s.Length < count)
            count = s.Length;

        return s.Substring(0, count);
    }

    public static string SubstringSafe(this string s, int startIndex)
    {
        return s.SubstringSafe(startIndex, int.MaxValue);
    }

    public static string SubstringSafe(this string s, int startIndex, int length)
    {
        int startIndexPos = startIndex;
        if (startIndexPos < 0)
            startIndexPos = 0;

        if (startIndexPos > s.Length - 1)
            return "";

        length = Mathf.Min(s.Length, length);

        int lengthIndedxPos = startIndex + length;
        if (lengthIndedxPos > s.Length)
            lengthIndedxPos = s.Length;

        if (startIndexPos >= lengthIndedxPos)
            return "";

        return s.Substring(startIndexPos, lengthIndedxPos - startIndexPos);

        /*
        if (s.Length < startIndex + length)
            length = s.Length - startIndex;

        if (length < 0)
            return "";

        return s.Substring(startIndex, length);
        */
    }

    /// <summary>
    /// Átalakítja a megadott szöveget úgy, hogy megfelelően jelenjen meg a TextUI componensben ha be van kapcsolva
    /// a BestFit.
    /// 
    /// A probléma az, hogy ha van egy sortörés, akkor a következő sor csak akkor jelenik meg ha van a végén annak is sortörés.
    /// </summary>
    /// <param name="text">Az átalakítandó szöveg.</param>
    /// <returns></returns>
    public static string TextUIHelper(string text) {
        // A szöveg végéről leszedjük a sortörtést ha van (ha csak egy sor van, akkor nem kell sortörés a végére)
        while (text.Length > 0 && char.IsWhiteSpace(text[text.Length - 1])) {
            // Ha whiteSpace akkor eltávolítjuk a szöveg végéről
            text = text.Substring(0, text.Length - 1);
        }

        // Megnézzük, hogy van-e a szövegben sortörés
        bool lineBreakIsPresent = false;
        foreach (char c in text) {
            if (c == '\n') {
                lineBreakIsPresent = true;
                break;
            }
        }

        // Ha van sörtörés, akkor a végére is teszünk egyet (enélkül az utolsó sor nem jelenik meg)
        if (lineBreakIsPresent)
            text = text + '\n';

        return text;
    }

    /// <summary>
    /// A megadott méretet átalakítja olyan formán, hogy 16:9 legyen az aránya és beférjen az eredeti méretbe.
    /// </summary>
    /// <param name="v"></param>
    /// <returns>Az új 16:9-es méret</returns>
    public static Vector3 fitSize_16_9(Vector3? v = null)
    {
        if (v == null) {
            v = ScreenSizeInUnit(); // new Vector3(Camera.main.aspect * 2, Camera.main.orthographicSize * 2);
        }

        // Kiszámoljuk a magassághoz tartozó 16:9 -es szélességet
        Vector3 vector = new Vector3(v.Value.y / 9 * 16, v.Value.y);

        // Ha a magassághoz kiszámolt szélesség túl széles, tehát kilóg a képernyőből
        if (vector.x > v.Value.x)
        {
            // akkor inkább a szélességhez számoljuk ki a magasságot
            vector = new Vector3(v.Value.x, v.Value.x / 16 * 9);
        }

        return vector;
    }

    /// <summary>
    /// Vissza adja a képernyő méretét Unit-okban.
    /// </summary>
    /// <returns></returns>
    public static Vector2 ScreenSizeInUnit()
    {
        return new Vector2(Camera.main.orthographicSize * 2 * Camera.main.aspect, Camera.main.orthographicSize * 2);
    }

    public static string GetMd5Hash(string input)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }

    /// <summary>
    /// Kiszámolja, hogy a megadott rectTransform-nak hány pixeles a mérete
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="xNeed"></param>
    /// <param name="yNeed"></param>
    /// <returns></returns>
    public static Vector2 GetRectTransformPixelSize(RectTransform rectTransform, bool xNeed = true, bool yNeed = true)
    {
        if (rectTransform == null)
            return Vector2.zero;

        float xSize = -1;
        float ySize = -1;

        if (xNeed && rectTransform.anchorMin.x == rectTransform.anchorMax.x)
        {
            xSize = rectTransform.sizeDelta.x;
            xNeed = false;
        }

        if (yNeed && rectTransform.anchorMin.y == rectTransform.anchorMax.y)
        {
            ySize = rectTransform.sizeDelta.y;
            yNeed = false;
        }

        Vector2 ancestorSize = Vector2.zero;
        if (xNeed || yNeed)
            ancestorSize = GetRectTransformPixelSize(rectTransform.parent.GetComponent<RectTransform>(), xNeed, yNeed);

        if (xNeed)
            xSize = ancestorSize.x * (rectTransform.anchorMax.x - rectTransform.anchorMin.x) + rectTransform.sizeDelta.x;

        if (yNeed)
            ySize = ancestorSize.y * (rectTransform.anchorMax.y - rectTransform.anchorMin.y) + rectTransform.sizeDelta.y;

        return new Vector2(xSize, ySize);
    }

    /// <summary>
    /// Az eljárás kiszámolja, hogy mennyivel kell növelni az original méretét x és y irányokba, hogy a required méretét kapjuk
    /// </summary>
    /// <param name="original"></param>
    /// <param name="required"></param>
    /// <returns></returns>
    public static Vector2 GetScaleFactor(Vector2 original, Vector2 required)
    {
        return new Vector2(required.x / original.x, required.y / original.y);
    }

    /// <summary>
    /// Kiszámolja, hogy mekkora lehet a ratio-val megadott terület, hogy maximálisan kitöltse a place területét, a méret arányok megtartásával
    /// </summary>
    /// <param name="place"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector2 GetMaxSize(Vector2 place, Vector2 ratio)
    {
        Vector2 scale = GetScaleFactor(ratio, place);
        float minSize = Mathf.Min(scale.x, scale.y);

        return ratio * minSize;
    }

    /// <summary>
    /// Mekkorára kell nagyítani a ratio által megadott tarületet, hogy teljesen kitöltse a place területét.
    /// </summary>
    /// <param name="place"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    public static Vector2 GetFullSize(Vector2 place, Vector2 ratio)
    {
        Vector2 scale = GetScaleFactor(ratio, place);
        float maxSize = Mathf.Max(scale.x, scale.y);

        return ratio * maxSize;
    }

    public static Sprite MakeSpriteFromByteArray(byte[] byteArray)
    {
        Debug.Log("Byte Array Size : " + byteArray.Length);

        if (byteArray.Length == 1093)
            Debug.Log(System.Text.Encoding.UTF8.GetString(byteArray));

        // A végén a false azt jelenti, hogy ne legyen mipmap generálva, mivel WebGL-en rossz szint van kiválasztva és így homályos a kép
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(byteArray);

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// Eltávolítja a jsonból a paraméter listában megadott kulcs neveket bármilyen mélységben is vannak.
    /// </summary>
    /// <param name="jsonnode"></param>
    /// <param name="keys">Eltávolítandó kulcs nevek</param>
    public static void RemoveKeysFromJson(JSONNode jsonnode, params string[] keys)
    {
        // Ha a json egy tömböt tartalmaz
        if (jsonnode is JSONArray)
        {
            // Végig megyünk a tömb elemein és megnézzük, hogy melyik osztály vagy tömb
            for (int i = 0; i < jsonnode.Count; i++)
            {
                if (jsonnode[i] is JSONArray || jsonnode[i] is JSONClass)
                {
                    RemoveKeysFromJson(jsonnode[i], keys);
                }
            }
        }

        // Ha a json egy osztályt tartalmaz
        if (jsonnode is JSONClass)
        {
            // Eltávolítjuk az osztályból a megadott kulcsu bejegyzéseket
            for (int i = 0; i < keys.Length; i++)
            {
                if (jsonnode.ContainsKey(keys[i]))
                    jsonnode.Remove(keys[i]);
            }

            // Végig megyünk a kulcsokon, ha valamelyik tömb vagy osztály, akkor azt is megvizsgáljuk
            List<string> jsonKeys = new List<string>(jsonnode.Keys);

            for (int i = 0; i < jsonKeys.Count; i++)
            {
                if (jsonnode[jsonKeys[i]] is JSONArray || jsonnode[jsonKeys[i]] is JSONClass)
                    RemoveKeysFromJson(jsonnode[jsonKeys[i]], keys);
            }
        }
    }

    /// <summary>
    /// Hármas csoportokra bontja a megadott számot, hogy mit írjon közéjük az is megadható.
    /// </summary>
    /// <param name="number"></param>
    /// <param name="groupingSignal"></param>
    /// <returns></returns>
    public static string GroupingNumber(int number, string groupingSignal = " ")
    {
        string sNumber = number.ToString();
        string result = "";

        int amount = sNumber.Length % 3;
        for (int i = 0; i < sNumber.Length;)
        {
            if (result.Length != 0)
                result += groupingSignal;

            result += sNumber.Substring(i, amount);
            i += amount;

            amount = 3;
        }

        return result;
    }


    /// <summary>
    /// Annyival tud többet a float.tryParse utasításnál, hogy a tizedes vessző lehet pont és tizedes vessző is
    /// </summary>
    /// <param name="s"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static bool tryNumberParse(string s, out float x)
    {
        if (float.TryParse(s.Replace(',', '.'), out x))
        {
            return true;
        }

        return false;
    }

    public static T SearchClass<T>(Transform tr) where T : class
    {
        do
        {
            Component[] components = tr.GetComponents<Component>();

            foreach (Component component in components)
            {
                if (component is T)
                    return component as T;
            }

            tr = tr.parent;

        } while (tr != null);

        return null;
    }

    public static IDataProvider SearchDataProvider(GameObject go)
    {
        Component[] components = go.transform.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component is IDataProvider)
                return component as IDataProvider;
        }

        return null;
    }

    // Vissza adja a megadott componenst, ha a megadott pozícióban levő collider gameObject-jén megtalálható vagy a szülőjén valahol
    // Null értéket ad vissza ha nincs ott semmi
    public static Component GetComponentInPos(Vector3 pos, string componentName)
    {
        // Egy sugarat kell kibocsájtani a kamerából
        //Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(pos);
        // A sugár által eltalált collidereket egy tömbben tároljuk
        //RaycastHit2D[] hits = Physics2D.LinecastAll(mousePositionInWorld, mousePositionInWorld);
        RaycastHit2D[] hits = Physics2D.LinecastAll(pos, pos);

        foreach (RaycastHit2D raycastHit in hits)
        {
            Transform tr = raycastHit.collider.transform;
            Component component;
            while (tr != null)
            {
                component = tr.gameObject.GetComponent(componentName);
                if (component != null)
                    return component;

                tr = tr.parent;
            }

            /*
            Component component = raycastHit.collider.gameObject.GetComponentInParent(componentName);

            if (component != null)
                return component;
                */
        }

        return null;
    }

    public static string Remove(this string originalString, string remove)
    {
        StringBuilder sb = new StringBuilder();

        int removeindex = originalString.IndexOf(remove);
        if (removeindex == -1)
            return originalString;

        int index = 0;

        while (index < originalString.Length)
        {
            if (index > removeindex)
                removeindex = originalString.IndexOf(remove, index);

            if (index == removeindex)
            {
                index += remove.Length;
                continue;
            }

            sb.Append(originalString[index]);
            index++;
        }

        return sb.ToString();
    }

}
