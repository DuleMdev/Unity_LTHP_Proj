using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON;

/*





*/
public class EvaluationSingle : MonoBehaviour {

    GameObject gameObjectShow;

    GameObject gameObjectAllStars;
    Text textAllStars;

    GameObject[] stars;

    Text textCountDown;
    GameObject gameObjectPlayButton;
    GameObject gameObjectBadgeButton;

    UIProgressBarEvaluation progressBar;
    UIEvaluationMonster monster;

    public JSONNode jsonData;   // Az adatok amiket meg kell jeleníteni az értékelő képernyőn

    int levelBorder;            // Hány csillagonként van szint ugrás
    int level;                  // Hányadik szinten tart a játékos

    int startStarNumber;        // Hány csillaggal kezdődik az animáció
    int result;                 // Hány csillagot szerzett korábban
    int resultNew;              // Hány csillagot szerzett most

    bool startCountDown;        // Megy-e a visszaszámláló
    float remainTime;           // A maradék idő amíg még lehet nézegetni az értékelő képernyőt
    public bool paused;                // A játék le van állítva, ekkor nem megy a visszaszámlálás

    Common.CallBack_In_String buttonClick;  // Mit hívjon meg az objektum ha lejárt az idő

    // Use this for initialization
    void Awake () {
        gameObjectShow = gameObject.SearchChild("Show").gameObject;

        gameObjectAllStars = gameObject.SearchChild("allStars").gameObject;
        textAllStars = gameObject.SearchChild("textAllStars").GetComponent<Text>();

        stars = new GameObject[3];
        stars[0] = gameObject.SearchChild("star1").gameObject;
        stars[1] = gameObject.SearchChild("star2").gameObject;
        stars[2] = gameObject.SearchChild("star3").gameObject;

        textCountDown = gameObject.SearchChild("textCountDown").GetComponent<Text>();
        gameObjectPlayButton = gameObject.SearchChild("playButton").gameObject;
        gameObjectBadgeButton = gameObject.SearchChild("badgeButton").gameObject;

        progressBar = gameObject.SearchChild("ProgressBarEvaluation").GetComponent<UIProgressBarEvaluation>();
        monster = gameObject.SearchChild("EvaluationMonster").GetComponent<UIEvaluationMonster>();
    }

    /// <summary>
    /// Inicializáljuk az értékelő képernyőt
    /// </summary>
    public void Init(JSONNode jsonData, Common.CallBack_In_String buttonClick)
    {
        this.jsonData = jsonData;
        this.buttonClick = buttonClick;

        levelBorder = jsonData[C.JSONKeys.levelBorder].AsInt;

        // Eltüntetjük a csillagokat
        foreach (GameObject go in stars)
        {
            go.transform.localScale = Vector3.zero;
            iTween.Stop(go);
        }

        // Eltüntetjük a jelvény gombot
        gameObjectBadgeButton.transform.localScale = Vector3.one * 0.0001f;

        // Eltüntetjük a play gombot
        gameObjectPlayButton.transform.localScale = Vector3.one * 0.0001f;

        // Kiszámoljuk, hány csillagja van a most megszerzetteken kívűl
        startStarNumber = jsonData[C.JSONKeys.allStar].AsInt;
        result = jsonData[C.JSONKeys.result].AsInt;
        resultNew = jsonData[C.JSONKeys.resultNew].AsInt;
        if (resultNew > result)
            startStarNumber -= resultNew - result;

        // Beleírjuk az összes csillagba
        textAllStars.text = startStarNumber.ToString();

        // Kiszámoljuk hányadik szinten tart a játékos
        level = startStarNumber / levelBorder;
        if (level > 9)
            level = 9;

        // Beállítjuk a progressBar-t
        progressBar.SetProgressBar(CalculateProgressBarValue(startStarNumber));

        // Kiválasztjuk az aktuálisan megjelenítendő szörnyet
        monster.SetMonster(GetMonsterSprite());

        // Beállítjuk a maradék időt
        remainTime = jsonData[C.JSONKeys.showTime].AsInt + 0.9f;
        startCountDown = false;
    }

    /// <summary>
    /// Lejátszuk az animációkat
    /// </summary>
    /// <returns></returns>
    public IEnumerator Show()
    {
        float animSpeed = 1;

        // Várakozunk egy keveset
        yield return new WaitForSeconds(animSpeed);

        // Megjelennek a csillagok egyessével
        for (int i = 0; i < resultNew; i++)
        {
            Common.audioController.SFXPlay("positive");
            // Megjelenik a soron következő csillag
            iTween.ScaleTo(stars[i], iTween.Hash("islocal", true, "scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));

            // Ha ez jobb eredmény mint a korábbi, akkor az összes csillag számláló ugrik egyet
            if (i + 1 > result)
            {
                gameObjectAllStars.transform.localScale = Vector3.one * 0.5f;
                iTween.Stop(gameObjectAllStars); // Leállítjuk a korábbi animációt
                iTween.ScaleTo(gameObjectAllStars, iTween.Hash("islocal", true, "scale", Vector3.one, "delay", 0.001f, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
                textAllStars.text = (++startStarNumber).ToString();

                // A progressBar odébb csúszik
                progressBar.SetProgressBarAnim(CalculateProgressBarValue(startStarNumber), animSpeed);
            }

            // Várakozunk egy keveset
            yield return new WaitForSeconds(animSpeed);

            // Ha még nem az utolsó szinten vagyunk.
            if (level < 9) {
                // Megnézzük, hogy végig ért-e a progressBar
                if (progressBar.progressBarIsEnd) {
                    // Ha végig ért előugrik az új figura
                    level++;

                    monster.SetMonsterAnim(GetMonsterSprite(), animSpeed);

                    // A progressBar vissza megy nullába
                    progressBar.SetProgressBarAnim(CalculateProgressBarValue(startStarNumber), animSpeed);

                    // Várakozunk egy keveset
                    yield return new WaitForSeconds(animSpeed);
                }
            }

            // Jöhet a következő csillag
        }

        yield return new WaitForSeconds(animSpeed);

        // Megjelenik a jelvény gomb
        iTween.ScaleTo(gameObjectBadgeButton, iTween.Hash("islocal", true, "scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
        Common.audioController.SFXPlay("boing");
        yield return new WaitForSeconds(animSpeed);

        // Megjelenik a play gomb
        iTween.ScaleTo(gameObjectPlayButton, iTween.Hash("islocal", true, "scale", Vector3.one, "time", animSpeed, "easeType", iTween.EaseType.easeOutElastic));
        Common.audioController.SFXPlay("boing");

        startCountDown = true;
    }

    public void SetActive(bool active) {
        gameObjectShow.SetActive(active);
    }

    Sprite GetMonsterSprite()
    {
        return (level == 0) ? null : Badges.instance.GetMonster(level-1);
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
    public void HidePlayButton() {
        iTween.ScaleTo(gameObjectPlayButton, iTween.Hash("islocal", true, "scale", Vector3.one * 0.001f, "time", 0.2f, "easeType", iTween.EaseType.linear));
    }

    /// <summary>
    /// Egy újabb csillagot ad az összes csillag gyűjtőbe, vagyis számlál egyet miközben úgrik egyet.
    /// </summary>
    void AddAllStars() {

    }

    public void StopCountDown()
    {
        startCountDown = false;
    }

    // Update is called once per frame
    void Update () {
        if (startCountDown && !paused) {
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
