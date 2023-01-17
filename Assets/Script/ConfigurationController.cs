using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System;

/*
Ez az objektum a következő dolgokkal foglalkozik:

1. Alkalmazás frissítésével
Ha van letölti a frissebb verziót és meghívja az alkalmazást frissítő képernyőt (CanvasUpdateController)

2. Eldönti, hogy szerver vagy kliens módban kell futnia az alkalmazásnak és ettől függően a MenuLogin vagy a 
ClientStartScreen hívódik meg.

3. Tárolja az osztályok névsorát és adatait.

4. Tárolja az óraterveket


- Menti és betölti az adatokat.

*/

public class ConfigurationController : MonoBehaviour
{
    public enum TabletType
    {
        Undefined,
        Server,
        Client,
    }

    public enum AppID
    {
        unknown,

        mInspire,
        ClassY,
        OTPFIA,
        LearnThenPlay,
        HelloMentor,
        Provocent,
        Tanlet,
        Entrepreneur,
        Lingland,
        SzekelyTermelok,
        Storie,
        SmartEmaths,
        StartUp,
        Market,
    }

    public enum AppMode
    {
        Full,   // Teljes alkalmazás
        WebGLGamePlay,  // Csak egy játékot játszik le
    }

    public enum Link
    {
        TestLink,
        LiveLink,
        Server2020DuckLink,
        Server2020LiveLink,
        Server2020LiveLinkWegGL,
    }

    public enum User
    {
        None,
        lastUser,
        ZsoltPtrvari,
        ZsoltPtrvariNewServer,
        test,
        test2,
        pista,
        contactClassyedu,
        helloMentor,
        provocentClassyedu,
        szekelytermelokClassyedu,
        bgaClassyedu,
        storieClassyedu,
        tipcike,
        storie_eng,
        domo001,
        domo002,
        domo003,
        domo004,
        domo005,
        domo006,
        domo007,
        domo008,
        domo009,
    }

    public enum TextComponentType
    {
        Text,
        TEXDraw
    }

    public enum DesignType
    {
        Design1,
        Design2,
        Design3,
    }

    public enum AnswerFeedback
    {
        Immediately,        // Exercise - A válasz ellenőrzése azonnal
        ShelfCheckButton,   // Selftest - A válasz ellenőrzése az ellenőrzés gomb megnyomására
        NoFeedback,         // Test - Nincs visszajelzés egyáltalán. A Tovább gombra megy tovább a program
        PerfectPlay,        // Csak hiba nélküli megoldásra megy tovább
    }

    public class BallGameDatas
    {
        int[] columnNumber = { 15, 25, 50, 75, 100, 150, 200, 300, 400, 600, 800, 1000 };
        // 15 25 50 75 100 150 200 250 400 600 1000

        int level = 0;

        int _bestScore;

        public BallGameDatas(int level, int bestScore)
        {
            this.level = level - 1;
            if (this.level < 0)
                this.level = 0;

            this._bestScore = bestScore;
        }


        public int bestScore {
            get { return _bestScore; }
            set { _bestScore = Mathf.Min(value, getMaxCloumnInLevel); }
        }

        public int getLevel
        {
            get { return level + 1; }
        }

        public int getMaxCloumnInLevel {
            get { return columnNumber[level]; }
        }

        public void LevelIncrease()
        {
            if (level < columnNumber.Length - 1)
                level++;
        }
    }

    public BallGameDatas ballGameDatas = new BallGameDatas(0, 0);

    [Space]
    /// <summary>
    /// A játékokban mozoghatnak-e a szövegek
    /// </summary>
    [Tooltip("A játékokban mozoghatnak-e a szövegek")]
    public bool playAnimation;

    /// <summary>
    /// Melyik komponensel jelenítsük meg a játékokban a szövegeket
    /// </summary>
    [Tooltip("Melyik komponensel jelenítsük meg a játékokban a szövegeket")]
    public TextComponentType textComponentType;

    /// <summary>
    /// Melyik design-t használja a játékokhoz az alkalmazás
    /// </summary>
    [Tooltip("Melyik design-t használja a játékokhoz az alkalmazás")]
    public DesignType designType;

    /// <summary>
    /// A válasz ellenőrzése mikor történjen
    /// </summary>
    [Tooltip("A válasz ellenőrzése mikor történjen")]
    public AnswerFeedback answerFeedback;

    /// <summary>
    /// Tökéletes válasz kell a tovább lépéshez.
    /// </summary>
    [Tooltip("Tökéletes válasz kell a tovább lépéshez.")]
    public bool perfectAnswer;

    [HideInInspector]
    public string loginEmailAddress; // Mit írtak be a inputField mezőbe loginnél (nem biztos, hogy megfelelő)

    /// <summary>
    /// Ha true, akkor a nem lejátszott tananyagokon egy lakat jelenik meg a tananyagok listázásánál
    /// </summary>
    [Tooltip("A tantárgyak listájából nem lehet olyan tananyagot indítani, amit még nem játszottak le az útvonalnál.")]
    public bool curriculumLock;

    [Space]
    /// <summary>
    /// A teszt szerver elérési linkje
    /// </summary>
    public string testLink;
    /// <summary>
    /// Az éles szerver elérési linkje
    /// </summary>
    public string liveLink;
    /// <summary>
    /// Zsolt saját test szervere
    /// </summary>
    public string server2020DuckLink;
    /// <summary>
    /// A 2020-as éles szerver elérési linkje
    /// </summary>
    public string server2020LiveLink;
    /// <summary>
    /// WebGL esetén a szerver kommunikációja
    /// </summary>
    [Tooltip("A program automatikusan tölti ki.")]
    public string server2020LiveLinkWebGL;

    [Tooltip("Hol kommunikáljon a szerverrel a fenti linkek közül.")]
    public Link link;

    [Tooltip("WebGL platformon editorban mi legyen az alkalmazás linkje. (Mivel abból veszi az alkalmazás azonosítóját)")]
    public string WebGLTestLink;

    // Ha nincs megadva fentebb a WebGLTestLink, akkor készít egyet és az appID a beállított appID lesz
    public string getWebGLTestLink {
        get {
            if (string.IsNullOrEmpty(WebGLTestLink))
            {
                WebGLTestLink = string.Format("https://www.{0}.classyedu.eu/app/", appID);
            }

            return WebGLTestLink;
        }
    }



    /// <summary>
    /// Az alkalmazás az új szervernek megfelelően kommunikál vagy a réginek megfelelően
    /// </summary>
    public bool isServer2020 { get { return link.ToString().Contains("2020"); } }

    /// <summary>
    /// Legyen-e az alkalmazásban osztálytermi játékra lehetőség
    /// </summary>
    [Tooltip("Legyen-e az alkalmazásban osztálytermi játékra lehetőség.")]
    public bool sideMenu;

    /// <summary>
    /// Látható legyen-e a kérdőjel a menüsávban. Ha látható, akkor a felhasználó törölni tudja az összes korábbi eredményét
    /// </summary>
    [Tooltip("Látható legyen-e a kérdőjel a menüsávban? A kérdőjellel törölni tudjuk a felhasználó korábbi eredményeit.")]
    public bool questionMarkVisible;

    /// <summary>
    /// Legyen-e intro képernyő
    /// </summary>
    [Tooltip("Legyen-e intro képernyő.")]
    public bool introScreenEnabled;

    /// <summary>
    /// Minden videó játékban a Youtube Test Linket próbálja betölteni
    /// </summary>
    [Space]
    [Tooltip("Minden videó játékban a Youtube Test Linket próbálja betölteni.")]
    public bool YoutubeTest;

    /// <summary>
    /// Ezt a Youtube linket próbálja betölteni a Youtube játék ha a YoutubeTest változó igaz
    /// </summary>
    [Tooltip("Ezt a Youtube linket próbálja betölteni ha a fenti Youtube Test ki van jelölve.")]
    public string YoutubeTestLink = "https://www.youtube.com/watch?v=SqBP0SDtwvg";

    [Space]
    [Tooltip("ShareToken-t tesztel editorban.")]
    public bool shareTokenTest;

    [Tooltip("A megnyítandó ShareToken.")]
    public string shareToken;

    [Space]
    [Tooltip("testToken-t tesztel editorban.")]
    public bool testTokenTest;

    [Tooltip("A megnyítandó testToken.")]
    public string testToken;

    [Space]
    [Tooltip("ReplayToken-t tesztel editorban.")]
    public bool replayTokenTest;

    [Tooltip("A megnyítandó replayToken.")]
    public string replayToken;

    [Space]
    [Tooltip("Ha tesztelni akarunk bizonyos lekérdezéseket. A lekérdezések json válaszait a ClassyServerCommunication scriptbe kell elhelyezni.")]
    public bool offlineWork;

    [Tooltip("A PDF játékokban található PDF-et elmenti a háttértárra. (Csak editorban)")]
    public bool savePDF;

    [Tooltip("Ezzel le tudjuk tiltani, hogy logot küldjön az alkalmazás.")]
    public bool gameLogDisabled;

    [Tooltip("Eltávolítja a szerver json válaszából a szerver debug információit.")]
    public bool removeDebugInfoFromJson;

    [Tooltip("A tesztelni kívánt tananyag json formában. Ilyenkor minden tananyag letöltésnél ez lesz a válasz.")]
    public TextAsset CurriculumTestData;

    [Space]
    [Tooltip("Editorban megkönnyíti a felhasználó kiválsztását.")]
    public User editorUser;

    public AppID appID; // Melyik alkalmazás
    public AppMode appMode;

    //public string mInspireVersionCode;
    //public string classyVersionCode;
    //public string otpfiaVersionCode;
    //public string hellomentorVersionCode;
    //public string provocentVersionCode;
    //public string tanletVersionCode;
    //public string entrepreneurVersionCode;
    //public string linglandVersionCode;
    //public string szekelyTermelokVersionCode;
    //public string storieVersionCode;
    //public string SmartEmathsVersionCode;

    [HideInInspector]
    public string versionCodeFromURLParameter;
    [HideInInspector]
    public bool WebGL = false;  // A platform WebGL ?
    [HideInInspector]
    public bool editor = false; // Editorban vagyunk ?
    [HideInInspector]
    public string appName = ""; // URL-ből vagy az URL paraméterből beolvasott alkalmazás azonosító (WebGL esetén)

    public string getVersionCode {
        get {
            string versionCode = "";

            switch (link)
            {
                case Link.TestLink:                versionCode = setupData.TestServerOld; break;
                case Link.LiveLink:                versionCode = setupData.LiveServerOld; break;
                case Link.Server2020DuckLink:      versionCode = setupData.TestServer2020; break;
                case Link.Server2020LiveLink: 
                case Link.Server2020LiveLinkWegGL: versionCode = setupData.LiveServer2020; break;
            }

            Debug.Log("getVersionCode : " + versionCode);
            if (!string.IsNullOrEmpty(versionCodeFromURLParameter))
            {
                versionCode = versionCodeFromURLParameter;
                Debug.Log("Verziószám felülírása az URL paraméterben találtal : " + versionCode);
            }

            //switch (appID)
            //{
            //    case AppID.mInspire:        versionCode = mInspireVersionCode;          break;
            //    case AppID.classY:          versionCode = classyVersionCode;            break;
            //    case AppID.OTPFIA:          versionCode = otpfiaVersionCode;            break;
            //    case AppID.HELLOMENTOR:     versionCode = hellomentorVersionCode;       break;
            //    case AppID.PROVOCENT:       versionCode = provocentVersionCode;         break;
            //    case AppID.TANLET:          versionCode = tanletVersionCode;            break;
            //    case AppID.ENTREPRENEUR:    versionCode = entrepreneurVersionCode;      break;
            //    case AppID.Lingland:        versionCode = linglandVersionCode;          break;
            //    case AppID.SZEKELYTERMELOK: versionCode = szekelyTermelokVersionCode;   break;
            //    case AppID.Storie:          versionCode = storieVersionCode;            break;
            //    case AppID.SMARTEMATHS:     versionCode = SmartEmathsVersionCode;       break;
            //}

            return versionCode;
        }
    }

    [HideInInspector]
    public AppSetup setupData;

    public TextAsset tasksInJSON;    // A feladatokat tartalmazó JSON fájl

    public TextAsset hangmanKeySet; // A hangman játékban a nyelvtől függően jelennek meg a betűk a választható táblán

    [Tooltip("Mi legyen a konfigurációs fájl neve. Ezen a néven fogja az adatokat a háttértárra menteni.")]
    public string configurationFileName;

    [Tooltip("Ha engedélyezve van, akkor a program logolni fog.")]
    public bool logIntoFile;

    [Header("M'Inspire")]
    [Tooltip("A különböző csoportoknak milyen szinüeknek kell lennie.")]
    public Color[] groupColors;

    [Tooltip("A különböző csoportok szineinek mi a neve.")]
    public string[] groupColorsName;

    //[Tooltip("Mi a szerver IP címe.")]
    //public string serverIP;

//    [HideInInspector]
    public string logFileName = "Log dummy.txt"; // A log fájl neve

    // Közös adatok
    //public bool server { get; private set; }          // Az eszköz szerver vagy kliens?
    [HideInInspector]
    public int portNumber { get; set; }                 // Port szám (szerver esetén a megnyitott port, cliens esetén amire kapcsolódnia kell)

    // szerver adatok
    [HideInInspector]
    public int maxConnectNumber { get; private set; }   // Maximális kapcsolatok száma

    // kliens adatok
    [HideInInspector]
    public string serverAddress { get; set; }           // A szerver címe
    [HideInInspector]
    public string tabletID { get; private set; }        // Tablet beazonosítására (valamilyen hosszú számsor)
    [HideInInspector]
    public int clientID; //{ get; private set; }        // Kliens beazonosítására (A szerver adja, egy egytől növekvő szám)
    [HideInInspector]
    public string userName;                             // A felhasználó neve
    [HideInInspector]
    public List<String> recentUserNames;                // A belépett felhasználók listája
    [HideInInspector]
    public string userID;                               // A felhasználó azonosítója
    [HideInInspector]
    public string password;                             // A felhasználó jelszava

    [HideInInspector]
    public bool playAnimations = true;                     // Keretjáték animációk engedélyezettek (szint fel/le és a szoba)
    [HideInInspector]
    public bool statusTableInSuper = true;        // Kiértékelő táblák megjelenhetnek a szuper egységen belül
    [HideInInspector]
    public bool statusTableBetweenSuper = true;  // Kiértékelő táblák megjelenhetnek a szuper egységek között

    [HideInInspector]
    public int userGroup;                               // A felhasználó csoportja

    public string className;                            // A tanuló osztályának a neve

    public string studentName;                          // A tanuló neve

    [HideInInspector]
    public LanguageData publicEmailGroupsLang;          // Milyen nyelvű publikus csoportokat listázzunk (Főoldal csoport böngésző / publikus csoportok)

    [HideInInspector]
    public LanguageData curriculumLang;                 // A listázandó email csoportok nyelve (Útvonalak lejátszása / email csoportok)

    [HideInInspector]
    public int curriculumLangCount;                     // Hány különböző nyelven vannak a tananyagok

    [HideInInspector]
    public string applicationLangName;                  // Az alkalmazásban megjelenő szövegek nyelve

    //[HideInInspector]
    //public TabletType tabletType { get; private set; }// A tablet szervernek vagy kliensnek lett definiálva

    [HideInInspector]
    public bool deviceIsServer;                         // A tablet szerver vagy kliens

    [HideInInspector]
    public int selectedFlow;                            // Melyik flow-t választották a CanvasScreenServerCollectClient képernyőn


    [HideInInspector]
    public JSONNode lessonPlanJSON;                     // Az óraterv adatai JSON formában

    [HideInInspector]
    public LessonPlanData lessonPlanData;               // Az óraterv adatai LessonPlanData objektumban



    // Az alábbi két változót áthelyeztem a MenuLessonPlan objektumba
    //[HideInInspector]
    // public string selectedLessonPlan;                // Melyik lecke tervet válaszották a lecketerv listázó képernyőn

    //[HideInInspector]
    // public bool lessonPlanView;                      // A lecke tervet view üzemmódba kell mutatni vagy nem

    string classes;



    // **********************************************************************************************************

    public TeacherList teacherList = new TeacherList(); // A tabletten legalább egyszer bejelentkezett tanárok listája és adatai
    public TeacherData actTeacher;                      // A bejelentkezett tanár adatai

    public TeacherConfig teacherConfig;
   

    public string lastTeacherUserName;                  // Az utoljára bejelentkezett tanár neve (A Login megkönnyítéséhez)

    public bool offlineMode;                            // Az eszköz offlineMód-ban fut-e

    public ClassRoster classRoster;  // A kiválasztott osztály névsora

    public ReportLessonPlan reportLessonPlan;   // A tanulók játékonkénti eredményei az óratervben

    public string DeviceUID;    // Mi a készülék vip azonosítója, újabb verzió telepítésénél fontos, mert előfordulhat, hogy csak vip tagoknak szánjuk az új verziót

    public int levelBorder = 90;    // Hány csillagonként jöjjön a következő szörny

    // **********************************************************************************************************

    [Tooltip("Melyik képernyő jöjjön az UpdateScreen után")]
    public HHHScreen nextScreen;

    [Tooltip("Mi az alkalmazás aktuális verzió száma.")]
    public string versionNumber = "2016.05.04v1.apk";   // Mi az aktuális verzió

    public string server = "http://bonis.me/minspire/";
    public string updateJSON = "update.json";

    //public string newVersionFile;   // Mi az új verziónak a neve (a JSON fájlból lesz kiszedve)
    public string cacheMappa { get { return Application.temporaryCachePath; } }

    [HideInInspector]
    public bool deployTestVersion = true; // Teszt verziókat is telepíteni kell az eszközre ha ez true

    [HideInInspector]
    string downloadedUpdateJSON;

    [HideInInspector]
    public string buildTime;


    public JSONNode loadNode;  // Ebbe töltjük a config json adatokat (szükséges megtartani, mivel a languageController ebből olvassa ki az adatait)

    public Dictionary<string, float> playRoutesEmailGroupScrollPos;

    // Use this for initialization
    void Awake ()
    {
        string version = "Build time : " + buildTime + "\n" +
            "2022.07.04 - TrueOrFalse, ReadGame, Boom, Bubble, hangman játékok tesz üzemmódjának és visszajátszhatóságának elkészítése. \n" +
            "2022.06.14 - Affix, Fish, MathMonster, Millionaire, Sets játék visszajátszhatóságának elkészítése. \n" +
            "2022.05.30 - Texty játék visszajátszhatóságának elkészítése. \n" +
            "";

        // Az android manifest fájlban az android:host alatti internet cím megváltoztasásának tesztelése

        /*
        string manifestFileName = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
        string manifest = File.ReadAllText(manifestFileName);
        */

        /*
        //var m = Regex.Match(manifest, "android:host=\"([^\"]+)\"");
        Match m = Regex.Match(manifest, "android:host=\"([^\\.]*)([^\"]*)\"");


        Debug.LogError(m.Groups[1].Value);
        Debug.LogError(m.Groups[2].Value);

        Debug.LogError(appID.ToString().ToLower());

        manifest = manifest.Replace(m.Groups[1].Value + m.Groups[2].Value, appID.ToString().ToLower() + m.Groups[2].Value);
        File.WriteAllText(manifestFileName, manifest);
        */


        /*
        Match m = Regex.Match(manifest, "android:scheme=\"([^\"]*)\"");

        Debug.LogError(m.Groups[1].Value);
        Debug.LogError(appID.ToString().ToLower());

        manifest = manifest.Replace(m.Groups[1].Value, appID.ToString().ToLower());
        File.WriteAllText(manifestFileName, manifest);
        */




        string s = "";
        Debug.Log(s.Remove(" "));

        Debug.Log(version);

        Common.configurationController = this;

        Debug.Log("ConfigurationController.Awake");

        Debug.Log("System language : " + Application.systemLanguage);


        JSONNode jsonNode = new JSONClass();

        //jsonNode[C.JSONKeys.previousLogID] = "-1";

        string previousLogID = jsonNode[C.JSONKeys.previousLogID];



        Debug.Log("previousLogID : " + previousLogID);

        Debug.Log(jsonNode[C.JSONKeys.previousLogID] == "");

        Debug.Log(jsonNode[C.JSONKeys.previousLogID].Value == "");

        /*
        string date = Common.TimeStamp();
        Debug.Log("Most1 : " + date);

        DateTime dateTime = Common.StringToDateTime(date);
        Debug.Log("Most2 : " + dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        */


        /*
        DateTime d1 = new DateTime(2016, 11, 17, 22, 27, 12);
        Debug.Log("Dátum 1 : " + d1.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        Debug.Log("Dátum most : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        TimeSpan t1 = DateTime.Now.Subtract(d1);

        Debug.Log("Idő eltérés : " + DateTime.Now.AddTicks(t1.Ticks).ToString("yyyy-MM-dd HH:mm:ss.fff"));
        */

        //Common.deltaTimeSpan = new TimeSpan(t2.Ticks + t1.Ticks / 2);


        // Létrehozzuk a szükséges csatornát
        //ConnectionConfig config = new ConnectionConfig();
        //
        //Debug.Log("ConnectTimeOut : " + config.ConnectTimeout);
        //Debug.Log("DisconnectTimeOut : " + config.DisconnectTimeout);
        //Debug.Log("MaxConnectionAttempt : " + config.MaxConnectionAttempt);
        //Debug.Log("MaxSentMessageQueueSize : " + config.MaxSentMessageQueueSize);
        //Debug.Log("MinUpdateTimeout : " + config.MinUpdateTimeout);
        //Debug.Log("PingTimeOut : " + config.PingTimeout);
        //Debug.Log("ResendTimeOut : " + config.ResendTimeout);



        Application.logMessageReceived += HandleLog;
        Application.targetFrameRate = 30;

        DeviceUID = SystemInfo.deviceUniqueIdentifier;

        clientID = -1;
        userGroup = -1;

        GlobalConfig gc = new GlobalConfig();
        Debug.Log(gc.MaxPacketSize);
        
        gc.MaxPacketSize = 2000;                        // Egy csomag maximális mérete     
        gc.ReactorMaximumReceivedMessages = 1024;       // Maximum hány csomagot tud tárolni a csomagokat fogadó sor
        gc.ReactorMaximumSentMessages = 1024;           // Maximum hány csomagot tud tárolni a csomogokat küldő sor
        gc.ReactorModel = ReactorModel.FixRateReactor;  // Milyen módot használjon a csomag kezelésre
        gc.ThreadAwakeTimeout = 10;

        //Debug.Log(System.DateTime.Now.ToString("HH:mm:ss.fff"));

        //JSONClass node = new JSONClass();
        //
        //Debug.Log(node.ContainsKey("Valami"));
        //
        //Debug.Log("json teszt : " + node["valami"]);

        //Debug.Log("deviceModel : " + SystemInfo.deviceModel);

        //Debug.Log("graphicsDeviceName : " + SystemInfo.graphicsDeviceName);



        /*

        int[] array = new int[] { 5, 3, 2, 9 };

        JSONNode node2 = Common.ArrayToJSON(array);

        Debug.Log(node2.ToString(" "));

        array = Common.JSONToArray(node2);

        Debug.Log(array[1]);
        */


        /*
        Debug.Log("deviceUniqueIdentifier : " + SystemInfo.deviceUniqueIdentifier);

        for (int i = 0; i < 20; i++)
        {
            string result = "";
            foreach (var item in Common.GetRandomNumbersWithFix(2, 0))
            {
                result += item.ToString() + ", ";
            }
            Debug.Log(result);
        }
        */




        /*

        Common.CallBack cc = Start;

        Object o = (object)Start;


        Debug.Log("Start ConfigurationController");
        Debug.Log(((object)cc).GetType());
        */



        //GZipStream zipStream;

        /*
        ZipFile zFile = new ZipFile(@"C:\lessonPlan1.zip");


        foreach (ZipEntry zEntry in zFile.Entries)
        {
            Debug.Log(zEntry.FileName);
            Debug.Log(zEntry.CompressedSize);
            Debug.Log(zEntry.UncompressedSize);

            MemoryStream ms = new MemoryStream();

            zEntry.Extract(ms);

            Debug.Log(ms.Length);
        }

        */
        //ZipEntry zEntry = zFile.Entries;

        /*
        // Egy zip fájlt letöltünk az internetről
        WWW www = new WWW(server + "lessonPlan1.zip");

        // Várunk amíg a letöltés befejeződik
        while (!www.isDone) { }

        // Ha nem volt hiba a letöltés közben
        if (www.error == null) {

            // A letöltött fájlt betöltjük egy memoryStream-be
            MemoryStream m = new MemoryStream(www.bytes);

            // A memoryStream tartalmát betöltjük egy ZipInputStream-be
            ZipInputStream zInput = new ZipInputStream(m);

            // Lekérdezzük az első bejegyzést
            ZipEntry zEntry = zInput.GetNextEntry();

            // Ha a bejegyzés nem könyvtár, akkor ...
            if (!zEntry.IsDirectory) {
                // Kiírjuk a fájlt nevét a tömörített és a tömörítetlen méretét
                Debug.Log(zEntry.FileName);
                Debug.Log(zEntry.CompressedSize);
                Debug.Log(zEntry.UncompressedSize);

                // Létrehozunk egy másik memoryStream-et és kitömörítjük bele a fájlt
                MemoryStream ms = new MemoryStream();

                // Átámásoljuk a ZipInputStream-ből a fájl a memoryStream-be
                byte[] buffer = new byte[32768];
                int read;
                while ((read = zInput.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                // Kiírjuk a keletkezet méretet
                Debug.Log(ms.Length);

                // A bájt tömböt stringé konvertáljuk
                string str = System.Text.Encoding.Default.GetString(ms.ToArray());

                // Kiírjuk a sztringet
                Debug.Log(str);
            }
        }
        */



        //zEntry.Extract()


        // Teszt DataPacket

        /*
        byte[] b = new byte[1000];
        DataPacket dp = new DataPacket(127, 13, 0, b);

        byte[] serialized = dp.Serialize();

        Debug.Log("Serialized Length : " + serialized.Length);

        DataPacket dp2 = new DataPacket(serialized);

        Debug.Log("dataLength : " + dp2.dataLength);
        Debug.Log("packID : " + dp2.packID);
        Debug.Log("packOrder : " + dp2.packOrder);
        Debug.Log("pack.Length : " + dp2.pack.Length);
        */



#if UNITY_WEBGL
        WebGL = true;
#endif

#if UNITY_EDITOR
        editor = true;
#endif

        // Ha editorban vagyunk és valamelyik TokenTest be van állítva az inspector ablakban, akkor úgy csinálunk mintha WebGL platformon lennénk
        if (editor)
            WebGL = testTokenTest || replayTokenTest;

        bool versionNumberIsTestServer = false;
        // Ha az alkalmazást WebGL platformról futtatjuk és full módban vagyunk, azaz nem csak egy játékot szeretnénk lejátszani,
        // akkor az alkalmazás nevét és a kommunikációs url-t meg kell határozni
        if (WebGL)
        {
            Debug.Log("Alkalmazás nevének meghatározása az URL-ből");
            string url = Application.absoluteURL;
            if (editor) // Ha editorban vagyunk, akkor csinálunk egy kamu url-t
            {
                // Ha az inspectorban be van állítva a test vagy a replay token, akkor bele tesszük a kamu url-be (a testToken elsőbbséget élvez)
                string urlParameter = "";
                if (testTokenTest)
                    urlParameter = C.JSONKeys.testToken + "=" + testToken;
                else if (replayTokenTest)
                    urlParameter = C.JSONKeys.replayToken + "=" + replayToken;

                //url = getServerCommunicationLink();
                switch (link)
                {
                    case Link.Server2020DuckLink:
                        //url = @"https://duck23.ddns.net/classy/app/?appID=" + appID + "&" + urlParameter; // TesztSzerver
                        url = @"https://" + appName + ".test.classyedu.eu/app/" + (string.IsNullOrEmpty(urlParameter) ? "" : "?" + urlParameter);
                        break;
                    case Link.Server2020LiveLink:
                        //url = @"https://" + appID + ".classyedu.eu/app/" + (string.IsNullOrEmpty(urlParameter) ? "" : "?" + urlParameter);
                        url = @"https://" + appName + ".classyedu.eu/app/" + (string.IsNullOrEmpty(urlParameter) ? "" : "?" + urlParameter);
                        break;
                }

                URLParameterReader.AddURL(url, true);

                //URLParameterReader.AddURL("https://duck23.ddns.net/classy/app/?appID=tanlet", true);
                //URLParameterReader.AddURL("https://tanlet.classyedu.eu/Wheatley/Core.php", true);

                Debug.LogError("We are in Editor - Hoax link created : " + url);
            }

            // Kibányásszuk az url-ből az alkalmazás nevét
            Match match = Regex.Match(url, @"((https?://)?(www\.)?([^\.]*).*)/app/");
            if (match.Success)
            {
                Debug.Log("match.Groups[0].Value = " + match.Groups[0].Value);
                Debug.Log("match.Groups[1].Value = " + match.Groups[1].Value);
                Debug.Log("match.Groups[2].Value = " + match.Groups[2].Value);
                Debug.Log("match.Groups[3].Value = " + match.Groups[3].Value);
                Debug.Log("match.Groups[4].Value = " + match.Groups[4].Value);

                appName = match.Groups[4].Value;
                Debug.Log("AppName from link : " + appName);
                Common.configurationController.server2020LiveLinkWebGL = match.Groups[1].Value + "/Wheatley/Core.php";
                Debug.Log("Communication link created : " + Common.configurationController.server2020LiveLinkWebGL);
                Common.configurationController.link = ConfigurationController.Link.Server2020LiveLinkWegGL;
            }

            if (editor)
            {
                //URLParameterReader.AddURL("https://duck23.ddns.net/classy/app/?appID=provoizé&version=v2.4", true);
                //URLParameterReader.AddURL("https://duck23.ddns.net/classy/app/?replayToken=KaC8M3t0rYx234wrXKXo7kFSLWDeRNMB&appID=CLASSY", true);
            }

            // Ha a linkbe van testToken vagy replayToken paraméter, akkor nem full módban indul az alkalmazás
            if (URLParameterReader.KeyIsExists(C.JSONKeys.testToken) ||
                URLParameterReader.KeyIsExists(C.JSONKeys.replayToken))
                Common.configurationController.appMode = ConfigurationController.AppMode.WebGLGamePlay;

            // Ha az appMode full, akkor meg kell határozni, hogy melyik alkalmazásként működjön a program
            if (Common.configurationController.appMode == ConfigurationController.AppMode.Full)
            {
                Debug.Log("appMode full");
                appID = GetAppIDFromString(appName);

                if (appID == AppID.unknown)
                {
                    Debug.Log("appName is unknown : " + appName);
                    if (URLParameterReader.KeyIsExists(C.JSONKeys.appID))
                    {
                        // Ha nem lehetett meghatározni az URL-ből az alkalmazást, akkor biztos, hogy a teszt szerveren vagyunk, mert ott nem lehet aldomain-t vagy mit készíteni
                        versionNumberIsTestServer = true;
                        Debug.Log("appName from URL parameters");
                        appName = URLParameterReader.GetParameter(C.JSONKeys.appID);
                        appID = GetAppIDFromString(appName);
                        Debug.Log("Find appName from URL parameters : " + appName);
                    }
                }

                //if (appID == AppID.unknown)
                //{
                //    // Ha nem megfelelő az alkalmazásnév, akkor feljön egy hiba ablak és semmi több nem fog történni a továbbiakban
                //    ErrorPanel.instance.Show(
                //        Common.languageController.Translate(C.Texts.UnknownApp) + "\n\"" + appName + "\"",
                //        null);
                //
                //    return;
                //}

                // Meghatározzuk az alkalmazás verziószámát ha tudjuk
                if (URLParameterReader.KeyIsExists(C.JSONKeys.version))
                {
                    Debug.Log("Verziószám felülírása az URL paraméterben levővel.");
                    versionCodeFromURLParameter = URLParameterReader.GetParameter(C.JSONKeys.version);
                    Debug.Log("versionCodeFromURLParameter : " + versionCodeFromURLParameter);
                }


                /*
                try
                {
                    // Ez elavult mivel inkább az URL-ből határozzuk meg, lásd fentebb
                    // Az alkalmazás nevét az URL-ben található appName paraméterből határozzuk meg
                    // test
                    //URLParameterReader.AddURL("http:\\google.com\valami.html?appName=Provocentt&param2=másodikParaméter", true);
                    //if (URLParameterReader.KeyIsExists("appName"))
                    //    appName = URLParameterReader.GetParameter("appName");

                    // Megnézzük, hogy a meghatározott alkalmazásnév létezik-e
                    ConfigurationController.AppID urlApp = (ConfigurationController.AppID)System.Enum.Parse(typeof(ConfigurationController.AppID), appName, true);
                    // Az alkalmazásnév létezik rögzítjük
                    Common.configurationController.appID = urlApp;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());

                    if (URLParameterReader.KeyIsExists(C.JSONKeys.testToken))
                        Common.configurationController.appMode = ConfigurationController.AppMode.WebGLGamePlay;



                    Common.configurationController.appID = ConfigurationController.AppID.unknown;

                    // Ha nem megfelelő az alkalmazásnév, akkor feljön egy hiba ablak és semmi több nem fog történni a továbbiakban
                    ErrorPanel.instance.Show(
                        Common.languageController.Translate(C.Texts.UnknownApp) + "\n\"" + appName + "\"",
                        null);

                    return;
                }
                */
            }
        }

        // WebGL esetén meghaatározzuk az alkalmazás nevét az URL-ből

        //https://cm.classyedu.eu/Wheatley/Core.php

        // Ha megvan az alkalmazás neve, akkor meghatározzuk a kommunikációs URL-t





        Common.configurationController = this;

        serverAddress = "";
        tabletID = "";
        userName = "";

        // Betöltjük a korábbi konfigurációt
        Load();


        string setupDataFileName;

        foreach (var name in Enum.GetNames(typeof(AppID)))
        {
            setupDataFileName = "AppSetup/" + name.ToString();
            setupData = Resources.Load<AppSetup>(setupDataFileName);
            Debug.Log("SetupData betöltése  (" + name.ToString() + " ): " + (setupData != null));
        }

        if (appID != AppID.unknown)
        {
            setupDataFileName = "AppSetup/" + appID.ToString();
            Debug.Log("Megkiséreljük betölteni az " + setupDataFileName + " helyen található adatokat.");

            setupData = Resources.Load<AppSetup>(setupDataFileName);
            Debug.Log("SetupData betöltése : " + (setupData != null));

            if (setupData == null)
                setupData = new AppSetup();

            Debug.Log("Setup data-ban beállított nyelv : " + setupData.defaultLanguage);
        }

        // Ha az alkalmazás azonosítóját az URL paraméterből szedtük, akkor teszt szervert verziószámát állítjuk be, ha még nincs meghatározva a verziószám
        if (versionNumberIsTestServer)
        {
            if (string.IsNullOrEmpty(versionCodeFromURLParameter))
            {
                Debug.Log("Alkalmazás verziószáma a teszt szerver verziószáma lesz : " + setupData.TestServer2020);
                versionCodeFromURLParameter = setupData.TestServer2020;
            }
            else
            {
                Debug.Log("A verziószám már meg lett határozva az URL paraméterekből : " + versionCodeFromURLParameter);
            }
        }
    }

    AppID GetAppIDFromString(string appID)
    {
        try
        {
            return (AppID)System.Enum.Parse(typeof(AppID), appID, true);
        }
        catch (System.Exception e)
        {
            return AppID.unknown;
        }
    }

    IEnumerator Start()
    {
        Debug.Log("ConfigurationController.Start");

        // Common.canvasNetworkHUD.SetText(SystemInfo.deviceUniqueIdentifier);
        Common.canvasNetworkHUD.SetText("");





        // Mindig szerverként induljon a tablet
        serverAddress = Common.HHHnetwork.ipAddress;

        // Fix IP cím beállítása
        //serverAddress = "192.168.1.255";






        deviceIsServer = serverAddress == Common.HHHnetwork.ipAddress; // A DecideServerOrClient-ben van ez beállítva, remélem nem késő akkor már!?

        // Törlünk mindent a Cache mappából
        Common.DeleteDirectoryContent(Application.temporaryCachePath);

        // Letöltjük az internetről a frissítési információkat tartalmazó json fájlt
        /*
#if UNITY_ANDROID
        WWW www = new WWW(server + updateJSON);
        yield return www;

        if (www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            downloadedUpdateJSON = www.text;
        }
#endif
*/
        /*
        // Töröljük az esetleg korábban letöltött verziót a háttértárról, hogy ne foglalja a helyet
        string filePath = System.IO.Path.Combine(cacheMappa, versionNumber);

        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
        */

        //StartCoroutine(LoadUpdateJSON());
        // Common.screenController.ChangeScreen("CanvasScreenSetupStart");

        /*
        // Indításnál eldöntjük, hogy melyik képernyőnek kellene megjelennie
        switch (tabletType)
        {
            case TabletType.Undefined:  // Ha még nem volt setup, akkor kliens / szerver választás
                Common.screenController.ChangeScreen("CanvasScreenSetupStart");
                break;
            case TabletType.Server:     // Ha korábban szervert választottunk, akkor a szerver menü
                Common.screenController.ChangeScreen("CanvasScreenServerMenu");
                break;
            case TabletType.Client:     // Ha korábban klienst választottunk, akkor a kliens menü
                Common.screenController.ChangeScreen("CanvasScreenClientMenu");
                break;
        }
        */

        yield break;
    }

    /// <summary>
    /// Megvizsgálja, hogy van-e a neten újabb verzió a programból
    /// </summary>
    public void CheckUpdate() {

        Debug.Log("Update json downloaded " + ((string.IsNullOrEmpty(downloadedUpdateJSON)) ? "not yet ready" : "success"));

        if (!string.IsNullOrEmpty(downloadedUpdateJSON))
        {
            try
            {
                JSONNode node = JSON.Parse(downloadedUpdateJSON);

                if (Common.canvasUpdateController.BuildUI(node))
                {
                    // Van frissíteni való
                    Common.screenController.ChangeScreen("CanvasUpdateController");
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Hiba az UpdateJSON fájl feldolgozása közben!\n" + e.Message);
            }
        }

        DecideServerOrClient();

        //StartCoroutine(LoadUpdateJSON());
    }

    /*
    IEnumerator LoadUpdateJSON() {
        WWW www = new WWW(server + updateJSON);
        yield return www;

        if (www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            try
            {
                JSONNode node = JSON.Parse(www.text);

                if (Common.canvasUpdateController.BuildUI(node))
                {
                    // Van frissíteni való
                    Common.screenController.ChangeScreen("CanvasUpdateController");
                    yield break;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Hiba az UpdateJSON fájl feldolgozása közben!\n" + e.Message);
            }
        }

        DecideServerOrClient();
    }*/

    /// <summary>
    /// Megvizsgálja, hogy az eszköz szerver vagy kliens és annak megfelelően tölti be a kezdő képernyőt
    /// </summary>
    public void DecideServerOrClient() {

        // A készülék szerver módban indul.
        // serverAddress = Common.HHHnetwork.ipAddress;

        deviceIsServer = serverAddress == Common.HHHnetwork.ipAddress;

        if (deviceIsServer) {
            // Szerver
            Common.screenController.ChangeScreen("MenuLogin");
        } else {
            // Kliens
            Common.screenController.ChangeScreen("ClientStartScreen");
        }
    }

    /// <summary>
    /// Ha nem kell frissítést letölteni, akkor melyik képernyő legyen a következő.
    /// </summary>
    /// <remarks>
    /// Ha a készülék IP címe megegyezik a beállított szerver IP címével, akkor szerver képernyő jön.
    /// Ha az IP cím nem egyezik, akkor kliens start képernyő.
    /// </remarks>
    public void NextScreen() {

    }

    public void SetServerData(int portNumber, int maxConnectNumber) {
        //tabletType = TabletType.Server;
        //server = true;
        this.portNumber = portNumber;
        this.maxConnectNumber = maxConnectNumber;

        Save();
    }

    public void SetClientData(string serverAddress, int portNumber, string tabletID, int clientID) {
        //tabletType = TabletType.Client;
        //server = false;
        this.serverAddress = serverAddress;
        this.portNumber = portNumber;
        this.tabletID = tabletID;
        this.clientID = clientID;

        Save();
    }

    /*
    /// <summary>
    /// Feltölti az osztály névsort (classRoster) osztály a megadott json-ben található nevek alapján.
    /// </summary>
    /// <param name="node">Az osztály névsor json formában.</param>
    public void FillClassRoster(int classID)
    {
        classRoster = ClassRoster(  new List<StudentData>();
    }
    */

    /*
    
    /// <summary>
    /// Feldolgozza a megadott json objektumot.
    /// </summary>
    /// <remarks>
    /// Kiolvassa a json objektumból a tanárhoz rendelt osztályokat illetve leckéket és elmenti a háttértárra.
    /// </remarks>
    /// <param name="jsonData">A feldolgozandó json.</param>
    public void AllDataProcessor(JSONNode jsonData) {

        // Osztályok feldolgozása
        if (jsonData.ContainsKey(C.JSONKeys.classes))
            for (int i = 0; i < jsonData[C.JSONKeys.classes].Count; i++) 

                studentList.Add(new StudentData(jsonData[C.JSONKeys.classStudents][i]));

        // Lecke tervek foldolgozása

    }
    */

    /// <summary>
    /// Vissza adja a Log directory elérési útját.
    /// </summary>
    /// <returns>A log directory elérési útja.</returns>
    public string GetLogDirectory(TeacherData teacherData = null)
    {
        //Debug.Log("Log directory : " + Path.Combine(Common.GetDocumentsDir(), C.DirFileNames.logDirName));

        return Path.Combine(Common.GetDocumentsDir(), C.DirFileNames.logDirName);
    }

    /// <summary>
    /// Vissza adja az aktuális tanárhoz tartozó lecke terveket tároló directory elérési útját.
    /// </summary>
    /// <returns>A lecketerveket tartalmazó directory elérési útja.</returns>
    public string GetLessonDirectory(TeacherData teacherData = null)
    {
        //Debug.Log("Lesson directory : " + Path.Combine(GetTeacherDirectory(teacherData), C.DirFileNames.lessonsDirName));

        return Path.Combine(GetTeacherDirectory(teacherData), C.DirFileNames.lessonsDirName);
    }

    /// <summary>
    /// Vissza adja az aktuális tanárhoz tartozó osztályokat tároló directory elérési útját.
    /// </summary>
    /// <returns>A lecketerveket tartalmazó directory elérési útja.</returns>
    public string GetClassDirectory(TeacherData teacherData = null)
    {
        //Debug.Log("Class directory : " + Path.Combine(GetTeacherDirectory(teacherData), C.DirFileNames.classesDirName));

        return Path.Combine(GetTeacherDirectory(teacherData), C.DirFileNames.classesDirName);
    }

    /// <summary>
    /// Vissza adja az aktuális tanárhoz tartozó lecke terveket tároló directory elérési útját.
    /// </summary>
    /// <returns>A lecketerveket tartalmazó directory elérési útja.</returns>
    public string GetReportDirectory(TeacherData teacherData = null)
    {
        //Debug.Log("Report directory : " + Path.Combine(GetTeacherDirectory(teacherData), C.DirFileNames.reportsDirName));

        return Path.Combine(GetTeacherDirectory(teacherData), C.DirFileNames.reportsDirName);
    }


    /// <summary>
    /// Vissza adja a megadott tanárhoz tartozó directory elérését.
    /// Ha nincs megadott tanár, akkor az aktuális tanár directory-ját adja vissza.
    /// </summary>
    /// <param name="teacherData">Melyik tanárnak keressük a directory-ját.</param>
    /// <returns>Az aktuális tanár vagy a megadott tanár gyökér directory-ja.</returns>
    public string GetTeacherDirectory(TeacherData teacherData = null) {
        if (teacherData == null)
            teacherData = actTeacher;

        return (teacherData != null) ? Path.Combine(Common.GetDocumentsDir(), teacherData.userID.ToString()) : null;
    }

    /// <summary>
    /// Létrehozza az aktuális tanárnak vagy a megadott tanárnak a directory szerkezetét.
    /// </summary>
    /// <param name="teacherData">Melyik tanárnak hozza létre a directory szerkezetet, ha nincs megadva, akkor az aktuális tanárnak fogja.</param>
    public void MakeTeacherDirectories(TeacherData teacherData = null) {
        // Ha nincs megadva tanár, akkor az aktuális tanárral fog dolgozni
        if (teacherData == null)
            teacherData = actTeacher;

        // Ha van tanár akkor létrehozza számára a directory szerkezetet
        if (teacherData != null) {
            Directory.CreateDirectory(GetLessonDirectory(teacherData)); // Létrehozzuk az óratervek számára fenntartott directory-t 
            Directory.CreateDirectory(GetClassDirectory(teacherData));  // Létrehozzuk az osztályok számára fenntartott directory-t
            Directory.CreateDirectory(GetReportDirectory(teacherData)); // Létrehozzuk a reportok számára fenntartott directory-t
        }
    }

    /// <summary>
    /// Elmenti a konfigurációs beállításokat egy json fájlba
    /// </summary>
    public void Save() {
        //firstSetup = false;
        string fileName = System.IO.Path.Combine(Common.GetDocumentsDir(), configurationFileName);

        JSONClass saveJson = new JSONClass();

        // Configurációs beállítások elmentése
        saveJson[C.JSONKeys.sessionToken] = ClassYServerCommunication.instance.sessionToken;

        saveJson[C.JSONKeys.serverAddress] = serverAddress;
        saveJson[C.JSONKeys.portNumber].AsInt = portNumber;
        saveJson[C.JSONKeys.maxConnectionNumber].AsInt = maxConnectNumber;

        saveJson[C.JSONKeys.username] = userName;
        //saveJson[C.JSONKeys.password] = password;

        saveJson[C.JSONKeys.playAnimations].AsBool = playAnimations;
        saveJson[C.JSONKeys.statusTableInSuper].AsBool = statusTableInSuper;
        saveJson[C.JSONKeys.statusTableBetweenSuper].AsBool = statusTableBetweenSuper;

        JSONArray jsonArray = new JSONArray();
        foreach (var item in recentUserNames)
            jsonArray.Add(item);

        saveJson[C.JSONKeys.recentUserNames] = jsonArray;

        saveJson[C.JSONKeys.studentName] = studentName;

        // Tanári adatok json-ba másolása
        saveJson[C.JSONKeys.teachers] = teacherList.SaveDataToJSON();

        saveJson[C.JSONKeys.appLanguages] = Common.languageController.GetLanguageDataToSave();

        saveJson[C.JSONKeys.appLanguage] = applicationLangName;

        if (curriculumLang != null)
            saveJson[C.JSONKeys.curriculumLanguage] = curriculumLang.SaveDataToJson();

        if (publicEmailGroupsLang != null)
            saveJson[C.JSONKeys.publicEmailGroupsLanguage] = publicEmailGroupsLang.SaveDataToJson();

        // Elmentjük az email csoportokhoz tartozó scroll pozíciókat
        if (playRoutesEmailGroupScrollPos != null && playRoutesEmailGroupScrollPos.Count > 0)
        {
            JSONClass jsonClass = new JSONClass();

            foreach (KeyValuePair<string, float> kvp in playRoutesEmailGroupScrollPos)
            {
                jsonClass[kvp.Key].AsFloat = kvp.Value;
            }

            saveJson[C.JSONKeys.playRoutes_ScrollPosOfEmailGroup] = jsonClass;
        }

        System.IO.File.WriteAllText(fileName, saveJson.ToString(" "));

        // Ha IOS platformon vagyunk, akkor beállítjuk, hogy a fájlt ne szinkronizálja a felhőbe
#if UNITY_IOS
		UnityEngine.iOS.Device.SetNoBackupFlag (fileName);
#endif
    }

    // Betölti a konfigurációs beállításokat
    public void Load() {
        string fileName = System.IO.Path.Combine(Common.GetDocumentsDir(), configurationFileName);
        Debug.Log("configuration location : " + fileName);

        loadNode = null;

        curriculumLang = new LanguageData();
        publicEmailGroupsLang = new LanguageData();
        playRoutesEmailGroupScrollPos = new Dictionary<string, float>();

        // Ha a file létezik, akkor betöltjük a tartalmát
        if (File.Exists(fileName))
        {
            loadNode = JSON.Parse(System.IO.File.ReadAllText(fileName));

            if (loadNode != null)
            {
                Debug.Log("JSON FROM FILE\n" + loadNode.ToString(" "));

                // Konfigurációs beállítások betöltése
                ClassYServerCommunication.instance.sessionToken = loadNode[C.JSONKeys.sessionToken].Value;

                serverAddress = loadNode[C.JSONKeys.serverAddress].Value;
                portNumber = loadNode[C.JSONKeys.portNumber].AsInt;
                maxConnectNumber = loadNode[C.JSONKeys.maxConnectionNumber].AsInt;

                userName = loadNode.ContainsKey(C.JSONKeys.username) ? loadNode[C.JSONKeys.username].Value : "";
                //password = loadNode.ContainsKey(C.JSONKeys.password) ? loadNode[C.JSONKeys.password].Value : "";

                playAnimations = loadNode.ContainsKey(C.JSONKeys.playAnimations) ? loadNode[C.JSONKeys.playAnimations].AsBool : true;
                statusTableInSuper = loadNode.ContainsKey(C.JSONKeys.statusTableInSuper) ? loadNode[C.JSONKeys.statusTableInSuper].AsBool : true;
                statusTableBetweenSuper = loadNode.ContainsKey(C.JSONKeys.statusTableBetweenSuper) ? loadNode[C.JSONKeys.statusTableBetweenSuper].AsBool : true;

                recentUserNames = new List<string>();
                if (loadNode.ContainsKey(C.JSONKeys.recentUserNames))
                {
                    for (int i = 0; i < loadNode[C.JSONKeys.recentUserNames].Count; i++)
                    {
                        recentUserNames.Add(loadNode[C.JSONKeys.recentUserNames][i].Value);
                    }
                }

                studentName = loadNode.ContainsKey(C.JSONKeys.studentName) ? loadNode[C.JSONKeys.studentName].Value : "";

                // Tanári adatok betöltése
                teacherList.LoadDataFromJSON(loadNode[C.JSONKeys.teachers]);

                applicationLangName = loadNode.ContainsKey(C.JSONKeys.appLanguage) ? loadNode[C.JSONKeys.appLanguage].Value : "";

                curriculumLang = loadNode.ContainsKey(C.JSONKeys.curriculumLanguage) ? new LanguageData(loadNode[C.JSONKeys.curriculumLanguage]) : new LanguageData();

                publicEmailGroupsLang = loadNode.ContainsKey(C.JSONKeys.publicEmailGroupsLanguage) ? new LanguageData(loadNode[C.JSONKeys.publicEmailGroupsLanguage]) : new LanguageData();

                // Beolvassuk az email csoportokhoz tartozó scroll pozíciót
                if (loadNode.ContainsKey(C.JSONKeys.playRoutes_ScrollPosOfEmailGroup))
                {
                    JSONClass scrollPosClass = loadNode[C.JSONKeys.playRoutes_ScrollPosOfEmailGroup].AsObject;

                    foreach (string item in scrollPosClass.Keys)
                    {
                        if (playRoutesEmailGroupScrollPos.ContainsKey(item))
                            playRoutesEmailGroupScrollPos[item] = scrollPosClass[item].AsFloat;
                        else
                            playRoutesEmailGroupScrollPos.Add(item, scrollPosClass[item].AsFloat);
                    }
                }
            }
        }

        // Ha WebGL platformon vagyunk, akkor a sessionToken jöhet a böngésző localStorage-éből is
#if UNITY_WEBGL

        //if (Common.configurationController.appMode == AppMode.Full)
        {
            if (LocalStorageWebGL.HasKey(C.JSONKeys.sessionToken))
            {
                string token = LocalStorageWebGL.GetString(C.JSONKeys.sessionToken);
                if (!string.IsNullOrEmpty(token))
                {
                    ClassYServerCommunication.instance.sessionToken = token;
                    userName = "";
                }
            }
        }
#endif
    }

    void OnGUI()
    {
        /*
        GUI.Label(new Rect(10, 10, 1500, 20), "streamingAssetsPath : " + Application.streamingAssetsPath);
        GUI.Label(new Rect(10, 30, 1500, 20), "version : " + Application.version);
        GUI.Label(new Rect(10, 40, 1500, 20), "Unity version : " + Application.unityVersion);
        GUI.Label(new Rect(10, 50, 1500, 20), "data path : " + Application.dataPath);
        GUI.Label(new Rect(10, 70, 1500, 20), "persistentDataPath : " + Application.persistentDataPath);
        GUI.Label(new Rect(10, 90, 1500, 20), "temporaryCachePath : " + Application.temporaryCachePath);
        GUI.Label(new Rect(10, 110, 1500, 20), "companyName : " + Application.companyName);
        GUI.Label(new Rect(10, 130, 1500, 20), "product name : " + Application.productName);
        GUI.Label(new Rect(10, 150, 1500, 20), "bundle identifier : " + Application.bundleIdentifier);
        */
    }

    void OnApplicationQuit()
    {
        Save();
    }


    /// <summary>
    /// Elindítja a megadott Coroutinet és vissza jelez ha végzet.
    /// </summary>
    /// <param name="enumerator">A futtatandó Coroutine.</param>
    /// <param name="callBack">Mit hívjon meg a befejezésekor.</param>
    public void WaitCoroutine(IEnumerator enumerator, Common.CallBack callBack)
    {
        StartCoroutine(WaitCoroutineHelper(enumerator, callBack));
    }

    IEnumerator WaitCoroutineHelper(IEnumerator enumerator, Common.CallBack callBack)
    {
        yield return StartCoroutine(enumerator);
        callBack();
    }

    /// <summary>
    /// Vár az f változóban megadott ideig majd meghívja a callBack függvényt.
    /// </summary>
    /// <param name="time">Hány másodpercig várjon.</param>
    /// <param name="callBack">Mit hívjon meg ha letelt az idő.</param>
    public void WaitTime(float time, Common.CallBack callBack)
    {
        // Ha nem adtak meg várokozási időt, akkor azonnal meghívjuk a callBack függvényt
        if (time <= 0)
        {
            callBack();
        }
        else {
            StartCoroutine(WaitTimeHelper(time, callBack));
        }
    }

    /// <summary>
    /// Vár az f változóban megadott ideig majd meghívja a callBack függvényt.
    /// </summary>
    /// <param name="f">Hány másodpercig várjon.</param>
    /// <param name="callBack">Mit hívjon meg ha letelt az idő.</param>
    /// <returns></returns>
    IEnumerator WaitTimeHelper(float f, Common.CallBack callBack)
    {
        Debug.Log(Common.Now() + " - waitTimeHelper - Start - " + f);
        Debug.Log(Time.time + " - waitTimeHelper - Start - " + f);

        yield return new WaitForSeconds(f);

        Debug.Log(Common.Now() + " - waitTimeHelper - End");
        Debug.Log(Time.time + " - waitTimeHelper - End");

        callBack();
    }

    public void CoroutineStartHelper(IEnumerator routine) {
        StartCoroutine(routine);
    }

    // Kovertálás text -> enum
    public object ConvertTextToEnumValue<T>(string text, T defaultValue) // where T : System.Enum // C# 7.3 and above
    {
        try
        {
            return System.Enum.Parse(typeof(T), text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Nincs a " + typeof(T).ToString() + " felsoroló típusnak " + text + " nevű értéke.\n" + e.ToString());
                
            return defaultValue;
        }
    }

    public IEnumerator KamuPlay()
    {

        bool next = false;

        for (int i = 0; i < lessonPlanData.lessonMosaicsList.Count; i++)
        {
            LessonMosaicData lessonMosaicData = lessonPlanData.lessonMosaicsList[i];

            for (int j = 0; j < lessonMosaicData.listOfGames.Count; j++)
            {
                GameData gameData = lessonMosaicData.listOfGames[j];

                next = false;

                switch (gameData.gameEngine)
                {
                    case GameData.GameEngine.Bubble:
                    case GameData.GameEngine.Sets:
                    case GameData.GameEngine.MathMonster:
                    case GameData.GameEngine.Affix:
                    case GameData.GameEngine.Boom:
                    case GameData.GameEngine.Fish:
                    case GameData.GameEngine.Hangman:
                    case GameData.GameEngine.Read:
                        Common.taskControllerOld.PlayQuestionList(gameData, 0, true, () => {
                            next = true;
                        });

                        break;

                    case GameData.GameEngine.TrueOrFalse:
                    case GameData.GameEngine.Millionaire:
                        Common.taskController.PlayGameInServer(gameData, 0, () => {
                            next = true;
                        });

                        break;
                }

                while (!next)
                    yield return null;


            }
        }

        Common.screenController.ChangeScreen("MenuLessonPlan");

        yield return null;
    }

    public void Log(string message , LogType logType = LogType.Log, JSONNode json = null) {
        if (!logIntoFile) return;

#if UNITY_WEBGL
        //Application.ExternalCall("AddLog", message, logType.ToString());
#else
        Directory.CreateDirectory(GetLogDirectory());

        using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(GetLogDirectory(), logFileName), true))
        {
            file.WriteLine(Common.Now() + " - " + message);
            if (json != null)
                file.WriteLine(Common.GetStringPart(json.ToString(" "), 1000));
        }
#endif
    }

    /// <summary>
    /// Elkészítjük a log fájl nevét
    /// </summary>
    public void StartLog() {
        logFileName = "Log " + Common.TimeStampWithSpace() + ".txt";
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.Length > 1000)
            logString = logString.Substring(0, 999) + "\n... (full length : " + logString.Length + " )";

        // If an exception or error, then log to file
        //if (type == LogType.Exception || type == LogType.Error)
        {
            Log("Debug." + type.ToString() + "\nMessage: " + logString + ((string.IsNullOrEmpty(stackTrace)) ? "" : "\nStackTrace" + stackTrace), type);
        }
    }

    public string getServerCommunicationLink()
    {
        string link = "";
        switch (Common.configurationController.link)
        {
            case ConfigurationController.Link.TestLink:
                link = Common.configurationController.testLink;
                break;
            case ConfigurationController.Link.LiveLink:
                link = Common.configurationController.liveLink;
                break;
            case ConfigurationController.Link.Server2020DuckLink:
                link = Common.configurationController.server2020DuckLink;
                break;
            case ConfigurationController.Link.Server2020LiveLink:
                link = Common.configurationController.server2020LiveLink;
                break;
            case ConfigurationController.Link.Server2020LiveLinkWegGL:
                link = Common.configurationController.server2020LiveLinkWebGL;
                break;
        }

#if UNITY_IOS // Az Apple alkalmazás tesztelése miatt a test@classyedu.com felhasználó mindig a teszt adatbázist használja.
        if (Common.configurationController.loginEmailAddress == "test@classyedu.com")
        {
            if (Common.configurationController.isServer2020)
                link = Common.configurationController.server2020DuckLink;
            else
                link = Common.configurationController.testLink;
        }
#endif

        link = link.Replace("[appName]", Common.configurationController.appID.ToString().ToLower());

        return link;
    }

    public string GetUserName()
    {
#if UNITY_EDITOR
        switch (Common.configurationController.editorUser)
        {
            case ConfigurationController.User.lastUser:                 return userName;
            case ConfigurationController.User.ZsoltPtrvari:             return "zsolt.ptrvari@gmail.com";
            case ConfigurationController.User.ZsoltPtrvariNewServer:    return "zsolt.ptrvari@gmail.com";
            case ConfigurationController.User.test:                     return "test@classyedu.com";
            case ConfigurationController.User.test2:                    return "test2@classyedu.com";
            case ConfigurationController.User.pista:                    return "pista@gmail.com";
            case ConfigurationController.User.contactClassyedu:         return "contact@classyedu.com";
            case ConfigurationController.User.helloMentor:              return "hellomentor@classyedu.com";
            case ConfigurationController.User.provocentClassyedu:       return "provocent@classyedu.com";
            case ConfigurationController.User.szekelytermelokClassyedu: return "szekelytermelok@classyedu.com";
            case ConfigurationController.User.bgaClassyedu:             return "bga@classyedu.com";
            case ConfigurationController.User.storieClassyedu:          return "storie@classyedu.com";
            case ConfigurationController.User.tipcike:                  return "tipcike@gmail.com";
            case ConfigurationController.User.storie_eng:               return "storie.eng@classyedu.com";
            case ConfigurationController.User.domo001:                  return "domo001@erettsegi.ro";
            case ConfigurationController.User.domo002:                  return "domo002@erettsegi.ro";
            case ConfigurationController.User.domo003:                  return "domo003@erettsegi.ro";
            case ConfigurationController.User.domo004:                  return "domo004@erettsegi.ro";
            case ConfigurationController.User.domo005:                  return "domo005@erettsegi.ro";
            case ConfigurationController.User.domo006:                  return "domo006@erettsegi.ro";
            case ConfigurationController.User.domo007:                  return "domo007@erettsegi.ro";
            case ConfigurationController.User.domo008:                  return "domo008@erettsegi.ro";
            case ConfigurationController.User.domo009:                  return "domo009@erettsegi.ro";
        }
#endif
        return "";
    }

    public string GetUserPassword()
    {
#if UNITY_EDITOR
        switch (Common.configurationController.editorUser)
        {
            case ConfigurationController.User.lastUser:                 return password;
            case ConfigurationController.User.ZsoltPtrvari:             return "kicsikacsa";
            case ConfigurationController.User.ZsoltPtrvariNewServer:    return "Of$yPFkRXI@l";
            case ConfigurationController.User.test:                     return "classy";
            case ConfigurationController.User.test2:                    return "classy";
            case ConfigurationController.User.pista:                    return "pista1";
            case ConfigurationController.User.contactClassyedu:         return "otpfia12345";
            case ConfigurationController.User.helloMentor:              return "hellomentor";
            case ConfigurationController.User.provocentClassyedu:       return "provocent12345";
            case ConfigurationController.User.szekelytermelokClassyedu: return "szekely12345";
            case ConfigurationController.User.bgaClassyedu:             return "bga12345";
            case ConfigurationController.User.storieClassyedu:          return "storie12345";
            case ConfigurationController.User.tipcike:                  return "123456";
            case ConfigurationController.User.storie_eng:               return "storie2020";
            case ConfigurationController.User.domo001:                  return "erettsegi12345";
            case ConfigurationController.User.domo002:                  return "erettsegi12345";
            case ConfigurationController.User.domo003:                  return "erettsegi12345";
            case ConfigurationController.User.domo004:                  return "erettsegi12345";
            case ConfigurationController.User.domo005:                  return "erettsegi12345";
            case ConfigurationController.User.domo006:                  return "erettsegi12345";
            case ConfigurationController.User.domo007:                  return "erettsegi12345";
            case ConfigurationController.User.domo008:                  return "erettsegi12345";
            case ConfigurationController.User.domo009:                  return "erettsegi12345";
        }
#endif
        return "";
    }
}



