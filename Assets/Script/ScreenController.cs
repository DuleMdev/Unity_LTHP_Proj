using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Ez az objektum kezeli a képernyők közötti váltásokat.

Van egy alapértelmezett képernyő, amivel a játék kezdődik.
A továbbiakban az aktív képernyő utasítja a képernyő kezelőt a képernyő váltásokra.

A képernyő váltások idején nem kezeljük a felhasználói input-ot.

A képernyő váltásokat különböző effektekkel lehet végrehajtani.
Ha nem adunk meg effektet, akkor az alapértelmezett effektel fog a képernyőváltás megtörténni, amit az inspector ablakban adtunk meg.
*/

public class ScreenController : MonoBehaviour
{

    // Nincs használatban *****************************************************
    [System.Serializable]
    public class ScreenData
    {
        public string name = "Screen Name"; // A képernyő neve vagy ha úgy jobban tetszik az azonosítója (Ezt a string-et kell megadni a képernyő váltásnál)
        public HHHScreen screen;            // A képernyőhöz tartozó vezérlő szkript
    } // **********************************************************************



    public enum ScreenTransition // A képernyő átmenetekhez használható effektek
    {
        FadeColor,              // Az előző képernyőt kitakarja a beállított szín, majd a szín eltűnik és így előbukkan az új képernyő
        FadePicture,            // Az előző képernyőt egy beállított kép kitakarja, majd a kép eltűnik és így előbukkan az új képernyő
        Slide,                  // Az előző képernyő kicsúszik a képből
        Scale,                  // Az előző képernyő a képernyő közepébe zsugorodik, majd a következő képernyő a képernyő közepéből kinagyítódik
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    // A képernyő váltásnál milyen effectet használjon
    [System.Serializable]
    public class TransitionData
    {
        [Tooltip("Mi legyen az alapértelmezett effect képernyő váltásoknál")]
        public ScreenTransition screenTransition;   // A képernyőváltás effectje
        [Tooltip("Az alapértelmezett fade effekt színe")]
        public Color fadeColor;                     // Ha FadeColor effectet választottunk ki, akkor a színt ezzel állíthatjuk be
        [Tooltip("Az alapértelmezett kép index fade effektnél")]
        public int pictureIndex;                    // Ha FadePicture effectet, akkor a kép indexét itt adhatjuk meg
        [Tooltip("Milyik irányba csússzon ki a képernyő")]
        public Direction direction;                 // Slide effect esetén az irányt állíthatjuk be itt
        [Tooltip("Csúszó effektnél használandó függvény megadása")]
        public iTween.EaseType easeType;            // Ha SlideXXX effectet, akkor a csúsztatást végző függvényt adhatjuk meg itt
                                                    // Scale effectnél nincs további beállítási lehetőség
        [Tooltip("Milyen gyors legyen az effect (másodpercben)")]
        public float effectSpeed;                   // Az effect sebessége
    }

    public HHHScreen[] prefabScreens;



    [HideInInspector] // Egyelőre nem ez alapján dönti el a StartScreen a kezdő képernyőt, hanem a ConfigurationControl-ban található appID alapján
    [Tooltip("Melyik képernyővel kezdődjön az alkalmazás")]
    public HHHScreen startScreen;                   // Az inspector ablakban itt megadott képernyővel fog az alkalmazás indulni

    [Space(20)]
    [Tooltip("Mi legyen az alapértelmezett effect képernyő váltásoknál")]
    public TransitionData transitionData;         // Az alapértelmezett átmenet beállítások

    [HideInInspector]
    public HHHScreen actScreen;        // Az aktuális képernyőt vezérlő szkript
    [HideInInspector]
    public bool actScreenIsPermanent;   // Az aktuális képernyő állandó vagy létrehozott
    HHHScreen[] screens;        // A scene-n levő összes képernyő
    List<GameObject> gameScreens;        // A scene-n levő összes game képernyőről egy másolat (az előnézethez)

    public bool changeScreenInProgress { get; private set; } // A képernyő váltás történik ha ez true (ilyenkor a játékostól nem fogadunk inputot

    bool next;                  // Képernyő átmenet több lépésből van megvalósítva (1. régi képernyő eltüntetése 2. új képernyő megmutatása), ez a változó mutatja, hogy mehet-e a következő lépésre.

    Coroutine coroutine;        // A képernyő váltó coroutine

    Dictionary<string, HHHScreen> holdingScreens = new Dictionary<string, HHHScreen>(); // Átmenetileg megtartja a képerenyőt későbbi használatra pl. MarketPlace2021
    public bool holdingActScreen; // Mutatja, hogy az aktuális képernyőt elkell-e tárolni, ha nem akkor törölve lesz

    // Use this for initialization
    void Awake()
    {
        Common.screenController = this;

        screens = FindObjectsOfType<HHHScreen>(); // Program indulásánál összegyűjtjük a HHHScreen szkripteket

        // A játék képernyőkről készítünk egy másolatot, az előnézeti képernyőkhöz
        gameScreens = new List<GameObject>();
        foreach (HHHScreen screen in screens)
            if (screen.name.Contains("Game"))
                gameScreens.Add(Instantiate(screen.gameObject));

        // Kikapcsoljuk a másolatokat
        foreach (GameObject go in gameScreens)
            go.SetActive(false);
    }

    void Start()
    {
        Debug.Log("ScreenController.Start");
        AllScreenOff(); // Majd mindet kikapcsoljuk

        LoadStartScreen();

        /*
        // Létrehozzuk a json-t amit elküldünk a kliensnek.
        JSONClass jsonData = new JSONClass();
        
        jsonData[C.JSONKeys.dataContent] = C.JSONValues.EvaluationScreen;

        jsonData[C.JSONKeys.multi].AsBool = true;

        jsonData[C.JSONKeys.onlyBadge].AsBool = false;
        jsonData[C.JSONKeys.evaluate].AsBool = false;
        jsonData[C.JSONKeys.allStar].AsInt = 91;

        jsonData[C.JSONKeys.groupStar].AsInt = 30;
        jsonData[C.JSONKeys.groupStarNew].AsInt = 30;

        jsonData[C.JSONKeys.allGroupStar].AsInt = 53;
        jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(StudentData.GetMonsterOrder());
        jsonData[C.JSONKeys.levelBorder].AsInt = 90;

        // Elindítjuk a single értékelő képernyőt
        Common.evaluationScreenSingle.jsonData = jsonData;
        Common.screenController.ChangeScreen(C.JSONValues.EvaluationScreenSingle);

        Common.configurationController.WaitTime(5.1f, () =>
        
        {
            jsonData[C.JSONKeys.dataContent] = C.JSONValues.EvaluationScreen;

            jsonData[C.JSONKeys.multi].AsBool = true;

            jsonData[C.JSONKeys.onlyBadge].AsBool = false;
            jsonData[C.JSONKeys.allStar].AsInt = 91;
            jsonData[C.JSONKeys.evaluate].AsBool = true;

            jsonData[C.JSONKeys.result].AsInt = 0;
            jsonData[C.JSONKeys.resultNew].AsInt = 3;
            jsonData[C.JSONKeys.groupStar].AsInt = 30;
            jsonData[C.JSONKeys.groupStarNew].AsInt = 30;
            jsonData[C.JSONKeys.itWasLastGame].AsBool = true;

            jsonData[C.JSONKeys.allGroupStar].AsInt = 53;
            jsonData[C.JSONKeys.monsterOrder] = Common.ArrayToJSON(StudentData.GetMonsterOrder());
            jsonData[C.JSONKeys.bestTeamMember].AsBool = false;
            jsonData[C.JSONKeys.cleverestStudent].AsBool = false;
            jsonData[C.JSONKeys.fastestStudent].AsBool = false;
            jsonData[C.JSONKeys.longest3StarSeries].AsInt = 4;
            jsonData[C.JSONKeys.showTime].AsInt = 30;
            jsonData[C.JSONKeys.levelBorder].AsInt = 90;

            jsonData[C.JSONKeys.lessonPlanEnd].AsBool = true;

            // Elindítjuk a single értékelő képernyőt

            /*
            Common.evaluationScreenSingle.jsonData = jsonData;
            Common.screenController.ChangeScreen(C.JSONValues.EvaluationScreenSingle);
            */

        /*
            Common.evaluationScreenSingle.UpdateData(jsonData);
        });
        */

        //#if UNITY_WEBGL || UNITY_EDITOR
        //      ChangeScreen("WebGLScreen"); // Bekapcsoljuk a WebGL képernyőt
        //#else



        /*
        if (startScreen != null)
            ChangeScreen(startScreen.name, effectSpeed: 1); // A megadott képernyőt bekapcsoljuk, amivel a program indulni fog
        else
            Debug.Log("Nincs start képernyő megadva a ScreenController-ben.");
            */
        //#endif
    }

    // Melyik képernyőre váltsunk, a beállított alapértelmezett effectel fog végrehajtódni
    public void ChangeScreen(string screenName, ScreenTransition? screenTransition = null, float? effectSpeed = null)
    {
        ChangeScreenTry(screenName, screenTransition, effectSpeed: effectSpeed);
    }

    // Önkényesen megadhatunk minden paramétert
    public void ChangeScreenFull(string screenName, ScreenTransition? screenTransition = null, Color? color = null, int? pictureIndex = null, Direction? direction = null, iTween.EaseType? easeType = null, float? effectSpeed = null)
    {
        ChangeScreenTry(screenName, screenTransition, color, pictureIndex, direction, easeType, effectSpeed);
    }

    /// <summary>
    /// A képernyő váltás alatt a képrnyőt eltakarjuk a megadott színnel, ami lehet fekete vagy fehér vagy tetszőleges egyébb szín.
    /// </summary>
    public void ChangeScreenColor(string screenName, Color? color = null, float? effectSpeed = null)
    {
        ChangeScreenTry(screenName, ScreenTransition.FadeColor, color, effectSpeed: effectSpeed);
    }

    // A képernyő váltás úgy hajtja végre, hogy egy képpel kitakarja a képernyőt, a kitakaráshoz 
    //használt képeket a CanvasFade objektumon levő FadeEffect szkriptben a Transition picture 
    //tulajdonság beállításával lehet megadni. Itt csak a kép indexét kell megadni.
    // A képernyőváltáshoz használt kép lehet akár egy logó is természetesen
    public void ChangeScreenPicture(string screenName, int? pictureIndex = null, float? effectSpeed = null)
    {
        ChangeScreenTry(screenName, ScreenTransition.FadePicture, pictureIndex: pictureIndex, effectSpeed: effectSpeed);
    }

    // A képernyőt lekicsinítjük, majd a következő képernyő a nullából jön elő
    public void ChangeScreenScale(string screenName, float? effectSpeed = null)
    {
        ChangeScreenTry(screenName, ScreenTransition.Scale, effectSpeed: effectSpeed);
    }

    // Az aktív képernyőt az új kívánt képernyő kitolja a megadott irányba.
    public void ChangeScreenSlide(string screenName, Direction? direction = null, iTween.EaseType? easeType = null, float? effectSpeed = null)
    {
        ChangeScreenTry(screenName, ScreenTransition.Slide, direction: direction, easeType: easeType, effectSpeed: effectSpeed);
    }

    void ChangeScreenTry(string screenName, ScreenTransition? screenTransition = null, Color? color = null, int? pictureIndex = null, Direction? direction = null, iTween.EaseType? easeType = null, float? effectSpeed = null)
    {
        Debug.Log("A kívánt képrnyő név : " + screenName);

        if (actScreen != null && actScreen.name == screenName)
        {
            Debug.Log("A kért képernyő jelenleg is aktív : " + screenName);

            return;
        }

        HHHScreen newScreen = GetScreenByName(screenName); // A képernyő név alapján megkeressük a kívánt képernyő szkriptet
        if (newScreen == null)
        {
            Debug.LogError("Nem találtam a megadott képernyő nevet : " + screenName);
            return;
        }

        /*
        if (newScreen == actScreen)
        {
            Debug.Log("A kért képernyő jelenleg is aktív : " + screenName);

            return;
        }
        */

        // A létrehozott képernyő nevét felülírjuk, mivel az új képernyő végén szerepel a (Clone) szöveg
        newScreen.name = screenName;

        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(ChangeScreenCoroutine(newScreen, screenName, screenTransition, color, pictureIndex, direction, easeType, effectSpeed));
    }

    // Melyik képernyőre váltsunk, fade effectel fog végrehajtódni a megadott színnel
    IEnumerator ChangeScreenCoroutine(HHHScreen newScreen, string screenName, ScreenTransition? screenTransition = null, Color? color = null, int? pictureIndex = null, Direction? direction = null, iTween.EaseType? easeType = null, float? effectSpeed = null)
    {
        changeScreenInProgress = true;
        Common.HHHnetwork.messageProcessingEnabled = false; // A képernyő váltás idejére letiltjuk a hálózati kommunikációt

        bool nextScreenInitReady = false;
        next = false;
        newScreen.transform.position = new Vector3(0, 10); // kipozícionáljuk a képernyőből az új képernyőt ha esetleg ott lenne
        newScreen.transform.localScale = Vector3.one;
        newScreen.gameObject.SetActive(true);              // majd bekapcsoljuk (Coroutine csak aktív gameObject-en tud futni)

        StartCoroutine(HelperCoroutine(newScreen.InitCoroutine(), () => { nextScreenInitReady = true; })); // Majd inicializáljuk. A képenyőt vezérlő szkript visszaszól ha az inicializálás befejeződött és meg lehet mutatni a képernyőt
        //StartCoroutine(newScreen.InitCoroutine( () => { nextScreenInitReady = true; })); // Majd inicializáljuk. A képenyőt vezérlő szkript visszaszól ha az inicializálás befejeződött és meg lehet mutatni a képernyőt

        // Ha valamelyik érték nincs megadva, akkor az alapértelmezetten beállított értékket veszi fel
        if (screenTransition == null)
            screenTransition = this.transitionData.screenTransition;
        if (color == null)
            color = this.transitionData.fadeColor;
        if (pictureIndex == null)
            pictureIndex = this.transitionData.pictureIndex;
        if (direction == null)
            direction = this.transitionData.direction;
        if (easeType == null)
            easeType = this.transitionData.easeType;
        if (effectSpeed == null)
            effectSpeed = this.transitionData.effectSpeed;

        // Eltüntetjük a korábbi képernyőt
        switch (screenTransition.Value)
        {
            case ScreenTransition.FadeColor:
                Common.fadeEffect.FadeInColor(color, (actScreen != null) ? effectSpeed : 0, () => { next = true; });
                break;
            case ScreenTransition.FadePicture:
                Common.fadeEffect.FadeInPicture(pictureIndex.Value, effectSpeed, () => { next = true; });
                break;
            case ScreenTransition.Slide:
                next = true; // Csúszó effect csak egy lépésből áll (nem kell eltüntetni semmit, mivel a következő képernyő becsúszásával párhuzamosan fog a korábbi képenyő kicsúszni
                break;
            case ScreenTransition.Scale:
                if (actScreen != null)
                    iTween.ScaleTo(actScreen.gameObject, iTween.Hash("scale", Vector3.zero, "time", effectSpeed, "easetype", iTween.EaseType.linear, "oncomplete", "iTweenAnimEnd", "oncompletetarget", gameObject));
                else
                    next = true;
                break;
        }

        // Elhalkítjuk a háttérzenét
        Common.audioController.MuteBackgroundMusic(effectSpeed.Value);

        // Értesítjük a régi képernyőt, hogy az eltüntetése elkezdődött
        if (actScreen != null)
            StartCoroutine(actScreen.ScreenHideStart());

        // várunk amíg a Fade effect és a következő képernyő inicializálása befejeződik
        while (!next || !nextScreenInitReady) { yield return null; }

        // Értesítjük a régi képernyőt, hogy az eltüntetése befejeződött
        if (actScreen != null)
            StartCoroutine(actScreen.ScreenHideFinish());

        // A memóriából eltávolítjuk a nem használt asseteket
        Resources.UnloadUnusedAssets();

        // Canvas típusú képernyőknél bekapcsoljuk a hátteret
        // A canvas típusú képernyőket onnan ismerhetjük fel, hogy Canvas karakterekkel kezdődik a nevük
        // Common.canvasBackground.Show(screenName.StartsWith("Canvas"));

        // Ha valamelyik játék képernyő következik, akkor kikapcsoljuk a MenuSystem hátterét és a menüsávot.
        if (screenName.Contains("Game"))
        {
            /*
            Common.menuBackground.ChangeBackground(null); // Kikapcsoljuk a hátteret
            Common.menuStripe.SetItem(); // Eltüntetjük az összes tartalmat a menüsávról
            */
        }
        else
        {
            // Ha nem Game képernyő következik, akkor kikapcsoljuk a fogaskerekeket
            Common.canvasDark.Dark(false);
        }

        // Ha olyan képernyő következik ami responsive, akkor nem kell a 16:9-es keret
        // Ha van a képernyő GameObject-én BackgroundRectTransform script és az extension be van kapcsolva, akkor nem kell keret
        BackgroundRectTransform backgroundRectTransform = newScreen.GetComponentInChildren<BackgroundRectTransform>();
        if (backgroundRectTransform && backgroundRectTransform.extension)
            CanvasBorder_16_9.instance.Enabled(false);
        else
        {
            CanvasBorder_16_9.instance.Enabled(true);
            CanvasBorder_16_9.instance.SetBorderPictures(newScreen);
        }

        // Az új képernyőt megjelenítjük
        next = false;
        switch (screenTransition.Value)
        {
            case ScreenTransition.FadeColor:
            case ScreenTransition.FadePicture:
                // AllScreenOff(); // Az összes képernyőt kikapcsoljuk // Ez hiba, mivel akkor az új képernyő is ki lesz kapcsolva és nem tudnak futni rajta a coroutinok, vagy megszakad a futásuk
                // Az aktív képernyőt kikapcsoljuk, mivel már eltünt
                if (actScreen != null)
                    actScreen.gameObject.SetActive(false);

                newScreen.transform.position = Vector3.zero; // középre tesszük
                //newScreen.gameObject.SetActive(true); // majd bekapcsoljuk // Már korábban bekapcsoltuk az InitCoroutine meghívása miatt

                Common.fadeEffect.FadeOut(effectSpeed, () => { next = true; });
                break;

            case ScreenTransition.Slide:
                // Kiszámoljuk, hogy merről kell a következő képenyőnek beúsznia
                Vector3 newScreenStartPos = Vector3.zero;
                switch (direction.Value)
                {
                    case Direction.Up:
                        newScreenStartPos = new Vector3(0, -Camera.main.orthographicSize * 2);
                        break;
                    case Direction.Down:
                        newScreenStartPos = new Vector3(0, Camera.main.orthographicSize * 2);
                        break;
                    case Direction.Left:
                        newScreenStartPos = new Vector3(Camera.main.aspect * 2, 0);
                        break;
                    case Direction.Right:
                        newScreenStartPos = new Vector3(-Camera.main.aspect * 2, 0);
                        break;
                }
                Vector3 actScreenEndPos = new Vector3(-newScreenStartPos.x, -newScreenStartPos.y); // kiszámoljuk, hogy merre kell az aktuális képernyőnek kiúsznia

                newScreen.transform.localPosition = newScreenStartPos; // A következő képernyőt a start pozícióba helyezzük

                // elindítjuk a két képernyőn a csúszó effect-et
                if (actScreen != null)
                    iTween.MoveTo(actScreen.gameObject, iTween.Hash("position", actScreenEndPos, "time", effectSpeed, "easetype", easeType.Value));
                iTween.MoveTo(newScreen.gameObject, iTween.Hash("position", Vector3.zero, "time", effectSpeed, "easetype", easeType.Value, "oncomplete", "iTweenAnimEnd", "oncompletetarget", gameObject));
                break;

            case ScreenTransition.Scale:
                AllScreenOff(); // Az összes képernyőt kikapcsoljuk
                newScreen.transform.position = Vector3.zero; // középre tesszük az új képernyőt
                newScreen.transform.localScale = Vector3.zero; // A méretét nullára állítjuk
                newScreen.gameObject.SetActive(true); // majd bekapcsoljuk

                // Elindítjuk a skálázás effect-et
                iTween.ScaleTo(newScreen.gameObject, iTween.Hash("scale", Vector3.one, "time", effectSpeed, "easetype", iTween.EaseType.linear, "oncomplete", "iTweenAnimEnd", "oncompletetarget", gameObject));
                break;
        }

        StartCoroutine(newScreen.ScreenShowStartCoroutine()); // Értesítjük az új képernyőt, hogy a megjelenése elkezdődött

        // Lejátszuk a képernyő megjelenési hangot, de csak azoknál amelyeknek a nevében benne van a Game szöveg
        //if (screenName.Contains("Game")) {
        //    Common.audioController.SFXPlay("gameStart");
        //}

        // várunk amíg az effect befejeződik
        while (!next) yield return null;

        if (actScreen != null)
        { // A korábbi képernyőt kikapcsoljuk ha volt korábbi képernyő
            iTween.Stop(actScreen.gameObject); // Leállítjuk az összes iTween animációt
            actScreen.gameObject.SetActive(false);







            /*
            // Ha nem állandó képernyőről van szó, akkor töröljük
            if (holdingActScreen)
            {
                holdingScreens.Add(actScreen.name, actScreen);
                holdingActScreen = false;
            }
            else if (!actScreenIsPermanent)
                Destroy(actScreen.gameObject);
            */



            // Ha nem állandó képernyőről van szó, akkor töröljük
            if (!actScreenIsPermanent)
            {
                if (holdingActScreen)
                {
                    holdingScreens.Add(actScreen.name, actScreen);
                    holdingActScreen = false;
                }
                else
                    Destroy(actScreen.gameObject);
            }





        }

        actScreen = newScreen;         // Az új képernyő lesz mostantól az aktuális
        actScreenIsPermanent = GetScreenIsPermanent(screenName);

        changeScreenInProgress = false;
        StartCoroutine(newScreen.ScreenShowFinishCoroutine()); // Értesítjük az új képernyőt, hogy a megjelenése befejeződött
    }

    public HHHScreen GetScreenByName(string screenName)
    {
        // Először megnézzük, hogy el van-e tárolva a keresett képernyő már (tehát korábban már létre lett hozva és nem lett törölve pl. MarketPlace2021 )
        if (holdingScreens.ContainsKey(screenName))
        {
            // Ha el volt tárolva, akkor vissza adjuk és töröljük a tárolóból.
            HHHScreen screen = holdingScreens[screenName];
            holdingScreens.Remove(screenName);

            return screen;
        }

        // Először a screen gameObject alatt keresi
        foreach (HHHScreen screen in screens)
        {
            if (screen.name == screenName)
                return screen;
        }



        // Ha nincs meg akkor tovább keresi a megadott képernyők között
        foreach (HHHScreen screen in prefabScreens)
        {
            if (screen != null && screen.name == screenName)
            {
                // Létrehozunk egy egyedet belőle
                return (HHHScreen)Instantiate(screen);
            }
        }

        // Ha még mindig nincs meg, akkor megnézi az erőforrás mappában is
        //return Resources.Load<HHHScreen>("Screens/" + screenName);

        HHHScreen hhhScreen = Resources.Load<HHHScreen>("Screens/" + screenName + "_" + (((int)Common.configurationController.designType) + 1));
        if (hhhScreen)
            return (HHHScreen)Instantiate(hhhScreen);

        return null;
    }

    public bool GetScreenIsPermanent(string screenName)
    {
        foreach (HHHScreen screen in screens)
        {
            if (screen.name == screenName)
                return true;
        }

        return false;
    }


    /// <summary>
    /// Vissza ad egy másolatot a megadott nevű játék képernyőről.
    /// </summary>
    /// <param name="screenName">Melyik játékról szeretnénk egy másolatot.</param>
    /// <returns>Másolat a megadott játék képernyőről. Null érték ha nincs megadott nevű játék képernyő.</returns>
    public GameObject GetGameScreenCopyByName(string screenName)
    {
        foreach (GameObject gameScreen in gameScreens)
        {
            if (gameScreen.name.Contains(screenName))
                return Instantiate(gameScreen);
        }

        return null;
    }

    // Kikapcsolja az összes képernyőt
    void AllScreenOff()
    {
        foreach (HHHScreen screen in screens)
            screen.gameObject.SetActive(false);
    }

    // iTween hívja meg, amikor befejezte az animációt
    void iTweenAnimEnd()
    {
        next = true;
    }

    IEnumerator HelperCoroutine(IEnumerator enumerator, Common.CallBack callBack)
    {
        yield return StartCoroutine(enumerator);

        callBack();
    }

    public void LoadStartScreen()
    {
        switch (Common.configurationController.appID)
        {
            case ConfigurationController.AppID.mInspire:
                Common.configurationController.CheckUpdate();
                break;

            case ConfigurationController.AppID.ClassY:
            case ConfigurationController.AppID.OTPFIA:
            case ConfigurationController.AppID.LearnThenPlay:
            case ConfigurationController.AppID.HelloMentor:
            case ConfigurationController.AppID.Provocent:
            case ConfigurationController.AppID.Tanlet:
            case ConfigurationController.AppID.Entrepreneur:
            case ConfigurationController.AppID.Lingland:
            case ConfigurationController.AppID.SzekelyTermelok:
            case ConfigurationController.AppID.Storie:
            case ConfigurationController.AppID.SmartEmaths:
            case ConfigurationController.AppID.StartUp:
            case ConfigurationController.AppID.Market:
                if (Common.configurationController.appMode == ConfigurationController.AppMode.Full)
                {
                    if (Common.configurationController.introScreenEnabled)
                        ChangeScreen(C.Screens.ClassYIntroScreen, effectSpeed: 1);
                    else
                        LoadScreenAfterIntro();
                }
                else
                {
                    if (Common.configurationController.appMode == ConfigurationController.AppMode.WebGLGamePlay &&
                        (URLParameterReader.KeyIsExists(C.JSONKeys.testToken) || URLParameterReader.KeyIsExists(C.JSONKeys.replayToken)))
                            ChangeScreen(C.Screens.WebGLScreen, effectSpeed: 1);
                    else
                        LoadScreenAfterIntro();
                }
                
                break;

            default:
                Debug.Log("Nem sikerült a start képernyőt meghatározni.");
                // Ha nem megfelelő az alkalmazásnév, akkor feljön egy hiba ablak és semmi több nem fog történni a továbbiakban
                ErrorPanel.instance.Show(
                    Common.languageController.Translate(C.Texts.UnknownApp) + "\n\"" + Common.configurationController.appName + "\"",
                    null);
                break;
        }
    }

    public void LoadScreenAfterIntro()
    {
        // Megvizsgáljuk, hogy van-e már sessionToken-ünk, mert ha nincs, akkor be kell jelentkezni, ha van, akkor jöhet a main képernyő
        if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
            Common.screenController.ChangeScreen(Common.configurationController.appID + "Login");
        else
            LoadScreenAfterLogin();

        /*
        switch (Common.configurationController.appID)
        {
            case ConfigurationController.AppID.classY:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.ClassYLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                    //Common.screenController.ChangeScreen(C.Screens.MenuClassYMain);
                break;

            case ConfigurationController.AppID.OTPFIA:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.OTPLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;

            case ConfigurationController.AppID.HELLOMENTOR:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.HelloMentorLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;

            case ConfigurationController.AppID.PROVOCENT:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.ProvocentLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;

            case ConfigurationController.AppID.TANLET:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.TanletLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;

            case ConfigurationController.AppID.ENTREPRENEUR:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.entrepreneurLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;

            case ConfigurationController.AppID.Lingland:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.LinglandLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;
            case ConfigurationController.AppID.SZEKELYTERMELOK:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.SzekelyTermelokLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;
            case ConfigurationController.AppID.Storie:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.StorieLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;
            case ConfigurationController.AppID.SMARTEMATHS:
                if (string.IsNullOrEmpty(ClassYServerCommunication.instance.sessionToken))
                    Common.screenController.ChangeScreen(C.Screens.SmartEmathsLogin);
                else
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;

        }*/
    }

    public void LoadScreenAfterLogin()
    {
        // Teszt
        if (Common.configurationController.editor && Common.configurationController.shareTokenTest)
        {
            string url = $"https://{Common.configurationController.appName}.test.classyedu.eu/?shareToken={Common.configurationController.shareToken}";
            //string url = $"https://duck23.ddns.net/classy/app/?shareToken={Common.configurationController.shareToken}&appID={Common.configurationController.appID.ToString()}";

            Debug.Log("Az editorban előállított shareToken-es url\n" + url);

            URLParameterReader.AddURL(url, true);
        }

        if (URLParameterReader.KeyIsExists(C.JSONKeys.shareToken))
        {
            ServerPlay.instance.PlayRouteLink(
                URLParameterReader.GetParameter(C.JSONKeys.shareToken),
                () =>
                {
                    LoadScreenAfterRouteLink();
                }
            );
        }
        else
            LoadScreenAfterRouteLink();
    }

    public void LoadScreenAfterRouteLink()
    {
        switch (Common.configurationController.appMode)
        {
            case ConfigurationController.AppMode.Full:
                Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;
            case ConfigurationController.AppMode.WebGLGamePlay:
                ChangeScreen(C.Screens.WebGLScreen);
                break;
        }
    }
}
