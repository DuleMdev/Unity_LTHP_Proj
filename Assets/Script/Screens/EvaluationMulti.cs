using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;

public class EvaluationMulti : MonoBehaviour {

    GameObject background;

    GameObject gameObjectAllStars;
    Text textAllStars;

    Text textWaitForGroupmate;  // Várakozás a csoport társaidra szöveg kiírásához

    GameObject[] stars;         // Csillagok gameObject-je
    Vector3[] starsPos;         // A csillagok pozíciója

    GameObject cupStar;         // A kupából az összes csillagba szállító csillag

    Text textCupStars;         // A kupán levő szám kiírására

    Text textCountDown;
    GameObject gameObjectPlayButton;

    UIProgressBarEvaluation progressBar;
    UIEvaluationMonster monster;

    public JSONNode jsonData;   // Az adatok amiket meg kell jeleníteni az értékelő képernyőn

    int levelBorder;            // Hány csillagonként van szint ugrás
    int level;                  // Hányadik szinten tart a játékos

    int startStarNumber;        // Hány csillaggal kezdődik az animáció
    int startProgressBar;       // Hol áll a progressBar indításnál

    bool evaluate;              // Van információ a kiértékelésről, vagy kiértékelés nélkül kell megmutatni a képrnyőt

    int result;                 // Hány csillagot szerzett korábban
    int resultNew;              // Hány csillagot szerzett most
    int groupStar;              // Az aktuális csoport óramozaikban mennyi csillagot szerzett
    int groupStarNew;           // Az aktuális óramozaikban, hány új csillagot szerzett
    bool itWasLastGame;         // Az utolsó játék volt az óramozaikban?

    bool lessonPlanEnd;         // Vége van a lecketervnek (ki kell írni, hogy ügyes voltál)

    bool startCountDown;        // Megy-e a visszaszámláló
    float remainTime;           // A maradék idő amíg még lehet nézegetni az értékelő képernyőt
    public bool paused;         // A játék le van állítva, ekkor nem megy a visszaszámlálás

    float animSpeed = 1;

    Common.CallBack_In_String buttonClick;  // Mit hívjon meg az objektum ha lejárt az idő

    IEnumerator show;           // A Show metódus ebben fog futni.

    // Use this for initialization
    void Awake()
    {
        background = gameObject.SearchChild("background").gameObject;

        gameObjectAllStars = gameObject.SearchChild("allStars").gameObject;
        textAllStars = gameObject.SearchChild("textAllStars").GetComponent<Text>();

        textWaitForGroupmate = gameObject.SearchChild("TextWaitforGroupmate").GetComponent<Text>();

        // Beolvassuk a csillagok gameObject-ját és meghatározzuk az alap pozíciójukat
        stars = new GameObject[3];
        starsPos = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            stars[i] = gameObject.SearchChild("star" + (i + 1)).gameObject;
            starsPos[i] = stars[i].transform.localPosition;
        }

        cupStar = gameObject.SearchChild("cupStar").gameObject;

        textCupStars = gameObject.SearchChild("textCupStars").GetComponent<Text>();

        textCountDown = gameObject.SearchChild("textCountDown").GetComponent<Text>();
        gameObjectPlayButton = gameObject.SearchChild("playButton").gameObject;

        progressBar = gameObject.SearchChild("ProgressBarEvaluation").GetComponent<UIProgressBarEvaluation>();
        monster = gameObject.SearchChild("EvaluationMonster").GetComponent<UIEvaluationMonster>();


    }

    /// <summary>
    /// Inicializáljuk az értékelő képernyőt
    /// </summary>
    public void InitInit(JSONNode jsonData, Common.CallBack_In_String buttonClick)
    {
        textWaitForGroupmate.text = Common.languageController.Translate(C.Texts.WaitForGroupmates);
        textWaitForGroupmate.transform.localScale = Vector3.one * 0.0001f;

        //Init(jsonData, buttonClick);
    }

    public void Init(JSONNode jsonData, Common.CallBack_In_String buttonClick) {
        this.jsonData = jsonData;
        this.buttonClick = buttonClick;

        levelBorder = jsonData[C.JSONKeys.levelBorder].AsInt;

        evaluate = jsonData[C.JSONKeys.evaluate].AsBool;

        // Eltüntetjük a csillagokat
        foreach (GameObject go in stars)
        {
            go.transform.localScale = Vector3.zero;
            iTween.Stop(go);
        }

        // Eltüntetjük a play gombot
        gameObjectPlayButton.transform.localScale = Vector3.one * 0.0001f;

        groupStar = jsonData[C.JSONKeys.groupStar].AsInt;
        groupStarNew = jsonData[C.JSONKeys.groupStarNew].AsInt;
        itWasLastGame = jsonData[C.JSONKeys.itWasLastGame].AsBool;

        lessonPlanEnd = jsonData[C.JSONKeys.lessonPlanEnd].AsBool;

        // Kiszámoljuk, hány csillagja van a most megszerzetteken kívűl
        result = jsonData[C.JSONKeys.result].AsInt;
        resultNew = jsonData[C.JSONKeys.resultNew].AsInt;
        startStarNumber = jsonData[C.JSONKeys.allStar].AsInt - groupStar + ((resultNew > result) ? result : resultNew);
        //if (resultNew > result)
        //startStarNumber -= resultNew - result;

        // Beleírjuk az összes csillagba
        //textAllStars.text = startStarNumber.ToString();

        // Kiszámoljuk a csoport kupába mennyi csillag van
        groupStar -= resultNew;

        // Beleírjuk a kupa csillag számlálójába
        //textCupStars.text = groupStar.ToString();

        // Kiszámoljuk hányadik szinten tart a játékos
        level = startStarNumber / levelBorder;
        if (level > 9)
            level = 9;

        // Beállítjuk a progressBar-t
        startProgressBar = jsonData[C.JSONKeys.allStar].AsInt - groupStarNew;
        progressBar.SetProgressBar(CalculateProgressBarValue(startProgressBar));

        // Kiválasztjuk az aktuálisan megjelenítendő szörnyet
        monster.SetMonster(GetMonsterSprite());

        // Beállítjuk a maradék időt
        remainTime = jsonData[C.JSONKeys.showTime].AsInt + 0.9f;
        startCountDown = false;

        show = null;
    }

    public void Show() {
        if (show != null)
            StopCoroutine(show);

        iTween.Stop(textWaitForGroupmate.gameObject);

        show = ShowCoroutine();
        StartCoroutine(show);
    }

    /// <summary>
    /// Lejátszuk az animációkat
    /// </summary>
    /// <returns></returns>
    public IEnumerator ShowCoroutine()
    {
        if (!evaluate)
        {
            // Csak a várakozás a csoporttársaidra szöveget mutatjuk meg
            iTween.ScaleTo(textWaitForGroupmate.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
        }
        else {
            // Össze zsugorítjuk a várakozás a csoporttársaidra szöveget
            iTween.ScaleTo(textWaitForGroupmate.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one * 0.0001f, "time", animSpeed / 2, "delay", 0.001f, "easeType", iTween.EaseType.linear));

            // Várakozunk egy keveset
            yield return new WaitForSeconds(animSpeed / 2);

            if (resultNew > 0)
            {
                // Megjelennek a csillagok egyessével
                for (int i = 0; i < resultNew; i++)
                {
                    // Ha ez jobb eredmény mint a korábbi, akkor egy új csillag jelenik meg
                    if (i + 1 > result)
                    {
                        Common.audioController.SFXPlay("positive");
                        // Megjelenik a soron következő csillag
                        stars[i].transform.localPosition = starsPos[i];
                        iTween.ScaleTo(stars[i], iTween.Hash("islocal", true, "scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
                    }
                    else { // Ha nem jobb a korábbinál, akkor az összes csillagszámlálóból kirepül egy csillag
                           // A soron következő csillagot az össszes csillag számláló alá pozíciónáljuk
                        stars[i].transform.localScale = Vector3.one;
                        stars[i].transform.position = gameObjectAllStars.transform.position;

                        // A helyére mozgatjuk
                        stars[i].GetComponent<iTweenMove>().MoveLocal(starsPos[i], iTween.EaseType.easeOutCirc, iTween.EaseType.easeInOutCubic, animSpeed, null);

                        startStarNumber--;
                    }

                    // Várakozunk egy keveset
                    yield return new WaitForSeconds(animSpeed);

                    // Jöhet a következő csillag
                }

                // Várakozunk egy keveset
                yield return new WaitForSeconds(animSpeed);

                // A csillagok egyessével beleesnek a kupába
                for (int i = resultNew - 1; i >= 0; i--)
                {
                    // A kupába mozgatjuk
                    stars[i].GetComponent<iTweenMove>().MoveLocal(cupStar.gameObject.transform.localPosition, iTween.EaseType.easeOutCirc, iTween.EaseType.easeInOutCubic, animSpeed, null);

                    // Várakozunk egy keveset
                    yield return new WaitForSeconds(animSpeed / 2);

                    groupStar++;
                    yield return new WaitForSeconds(animSpeed / 2);

                    // Jöhet a következő csillag
                }

                // Várakozunk egy keveset
                yield return new WaitForSeconds(animSpeed);
            }

            // Ha az utolsó játék volt az óramozaikban, akkor a csillagok berepülnek a közösbe
            if (itWasLastGame)
            {

                for (int i = groupStar; i > 0; i--)
                {
                    // Leklónozzuk a kupa csillagot
                    GameObject newCupStar = Instantiate(cupStar);
                    newCupStar.transform.SetParent(cupStar.transform.parent, false);

                    // elindítjuk az összes csillag számlálóba
                    newCupStar.GetComponent<iTweenMove>().MoveLocal(gameObjectAllStars.transform.localPosition, GetRandomEaseType(), GetRandomEaseType(), animSpeed, (Object o) =>
                    {
                        Destroy(o);
                        startStarNumber++;
                        Common.audioController.SFXPlay("positive");

                        // Mozgatjuk a progressBar-t ha szükséges
                        if (startStarNumber > startProgressBar)
                            progressBar.SetProgressBarAnim(CalculateProgressBarValue(startStarNumber), 1);

                    });

                    groupStar--;

                    //iTween.MoveTo(newCupStar, iTween.Hash("x", gameObjectAllStars.transform.position.x, "time", animSpeed, "easeType", GetRandomEaseType(), 
                    //"oncomplete", "CupStarMoveComplete", "oncompletetarget", this.gameObject, "oncompleteparams", newCupStar));
                    //iTween.MoveTo(newCupStar, iTween.Hash("y", gameObjectAllStars.transform.position.y, "time", animSpeed, "easeType", GetRandomEaseType()));

                    // Várakozunk egy keveset
                    yield return new WaitForSeconds(0.1f);
                }

                // Várakozunk egy keveset
                yield return new WaitForSeconds(animSpeed * 3);
            }

            // Megjelenik a play gomb
            iTween.ScaleTo(gameObjectPlayButton, iTween.Hash("islocal", true, "scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
            Common.audioController.SFXPlay("boing");

            startCountDown = true;
        }

    }

    void ChangeXValue(float value) {
        stars[0].transform.localPosition = stars[0].transform.localPosition.SetX(value);

        //((GameObject)o).transform.position = ((GameObject)o).transform.position.SetX(value);
    }

    void ChangeYValue(float value) {
        stars[0].transform.localPosition = stars[0].transform.localPosition.SetY(value);

        //((GameObject)o).transform.position = ((GameObject)o).transform.position.SetX(value);
    }

    /// <summary>
    /// Egy csillag a kupából az összes csillag számlálóba ért
    /// </summary>
    void CupStarMoveComplete(object o) {
        // Elpusztítjuk a csillagot
        Destroy((GameObject)o);

        // Számlál egyet az összes csillag számláló
        textAllStars.text = (++startStarNumber).ToString();

        // Mozgatjuk a progressBar-t ha szükséges
        if (startStarNumber > startProgressBar)
            progressBar.SetProgressBarAnim(CalculateProgressBarValue(startStarNumber), 1);
    }

    iTween.EaseType GetRandomEaseType() {
        int r = Random.Range(0, 14);

        switch (r)
        {
            case 0: return iTween.EaseType.easeInSine;
            case 1: return iTween.EaseType.easeInQuad;
            case 2: return iTween.EaseType.easeInCubic;
            case 3: return iTween.EaseType.easeInQuart;
            case 4: return iTween.EaseType.easeInQuint;
            case 5: return iTween.EaseType.easeInExpo;
            case 6: return iTween.EaseType.easeInCirc;

            case 7: return iTween.EaseType.easeOutSine;
            case 8: return iTween.EaseType.easeOutQuad;
            case 9: return iTween.EaseType.easeOutCubic;
            case 10: return iTween.EaseType.easeOutQuart;
            case 11: return iTween.EaseType.easeOutQuint;
            case 12: return iTween.EaseType.easeOutExpo;
            case 13: return iTween.EaseType.easeOutCirc;
        }

        return iTween.EaseType.easeInCirc;
    }

    public void SetActive(bool active)
    {
        background.SetActive(active);
    }

    Sprite GetMonsterSprite()
    {
        return (level == 0) ? null : Badges.instance.GetMonster(level - 1);
    }

    /// <summary>
    /// Kiszámolja, hogy hol kellene állnia a progressBar-nak a csillagszámnak megfelelően.
    /// </summary>
    /// <param name="starNumber"></param>
    float CalculateProgressBarValue(int starNumber)
    {
        return (starNumber - level * levelBorder) / (float)levelBorder;
    }

    /// <summary>
    /// Eltüntetjük a play gombot.
    /// </summary>
    public void HidePlayButton()
    {
        iTween.ScaleTo(gameObjectPlayButton, iTween.Hash("islocal", true, "scale", Vector3.one * 0.001f, "time", 0.2f, "easeType", iTween.EaseType.linear));
    }

    /// <summary>
    /// Egy újabb csillagot ad az összes csillag gyűjtőbe, vagyis számlál egyet miközben úgrik egyet.
    /// </summary>
    void AddAllStars()
    {

    }

    public void StopCountDown() {
        startCountDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Kiírjuk az össz csillag számot
        textAllStars.text = startStarNumber.ToString();

        // Kiírjuk a kupában található csillagok számát
        textCupStars.text = groupStar.ToString();

        // Ha még nem az utolsó szinten vagyunk.
        if (level < 9)
        {
            // Megnézzük, hogy végig ért-e a progressBar
            if (progressBar.progressBarIsEnd)
            {
                // Ha végig ért előugrik az új figura
                level++;

                monster.SetMonsterAnim(GetMonsterSprite(), animSpeed);

                // A progressBar vissza megy nullába
                progressBar.SetProgressBarAnim(CalculateProgressBarValue(startStarNumber), animSpeed);

                // Várakozunk egy keveset
                //yield return new WaitForSeconds(animSpeed);
            }
        }

        if (startCountDown && !paused)
        {
            remainTime -= Time.deltaTime;

            textCountDown.text = ((int)remainTime).ToString() + " " + Common.languageController.Translate(C.Texts.sec);

            if (remainTime <= 0)
            {
                if (buttonClick != null)
                    buttonClick("Play");

                startCountDown = false;
            }
        }
    }

}
