using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

public class MenuLessonMosaic : MonoBehaviour {

    public enum PlayStatus
    {
        NotPlayedYet,
        Playing,
        Played,
    }

    [Tooltip("Mennyivel legyen villágosabb minden második sor. 0 nem lesz világosabb, 1 teljesen fehér lesz.")]
    public float lightRate;

    public Color notPlayedColor;
    public Color playingColor;
    public Color playedColor;

    RectTransform transformLessonMosaic;    // Ezt fogjuk méretezni, hogy elférjenek a játék ikonok
    RectTransform transformBackground;      // Ebbe a komponensbe fogjuk a játék ikonokat elhelyezni
    Image imageBackground;                  // Megváltoztathatjuk a háttér színét a háttér image komponense segítségével
    Image imageLight;                       // A háttér világosságát tudjuk ezzel állítani ki-be kapcsolással
    GameObject autoContinuationOn;          // Ez a kép látszik ha az automatikus folytatás be van kapcsolva
    GameObject autoContinuationOff;         // Ez a kép látszik ha az automatikus folytatás ki van kapcsolva
    Text textLessonMosaicName;              // Az óra mozaik nevét tartalmazó elem

    GameObject foreground;                  // Az előtér ki-be kapcsolásához
    Image imageForeground;                  // Az előtér színének beállításához
    GameObject buttonAutoContinuationOn;    // Az automatikus folytatás kikapcsolásához
    GameObject buttonAutoContinuationOff;   // Az automatikus folytatás bekapcsolásához

    RectTransform gameIconPrefab;           // Egy játék ikon prefabja amit majd sokszorosítunk

    List<MenuGameButton> gameButtonList;

    LessonMosaicData lessonMosaicData;      // Az óramozaik adatait tároló objektum
    int mosaicIndex;                        // Hányadik óramozaik az óra tervben

    Common.CallBack_In_Int_Int callBack; // A gombnyomásokkor meghívandó függvény

    public PlayStatus playStatus { get; protected set; } // Milyen állapotban van a játszás szempontjából az óramozaik

	// Use this for initialization
	void Awake () {
        transformLessonMosaic = (RectTransform)transform;
        transformBackground = (RectTransform)Common.SearchGameObject(gameObject, "Background").transform;
        imageBackground = transformBackground.GetComponent<Image>();
        imageLight = Common.SearchGameObject(gameObject, "Light").GetComponent<Image>();
        autoContinuationOn = Common.SearchGameObject(gameObject, "ImageAutoContinuationOn").gameObject;
        autoContinuationOff = Common.SearchGameObject(gameObject, "ImageAutoContinuationOff").gameObject;
        textLessonMosaicName = Common.SearchGameObject(gameObject, "TextLessonMosaicName").GetComponent<Text>();

        foreground = Common.SearchGameObject(gameObject, "Foreground").gameObject;
        imageForeground = foreground.GetComponent<Image>();

        buttonAutoContinuationOn = Common.SearchGameObject(gameObject, "ButtonAutoContinuationOn").gameObject;
        buttonAutoContinuationOff = Common.SearchGameObject(gameObject, "ButtonAutoContinuationOff").gameObject;

        gameIconPrefab = (RectTransform)Common.SearchGameObject(gameObject, "GameButton").transform;
        gameIconPrefab.gameObject.SetActive(false);
    }

    /// <summary>
    /// Létrehozza az óra mozaikban található játékokhoz az ikonokat.
    /// </summary>
    /// <param name="lessonMosaicData">Óra mozaik adatok, amikben a játékok adatai találhatóak.</param>
    /// <returns></returns>
    public float Initialize(LessonMosaicData lessonMosaicData, int mosaicIndex, Common.CallBack_In_Int_Int callBack) {
        this.lessonMosaicData = lessonMosaicData;
        this.mosaicIndex = mosaicIndex;
        this.callBack = callBack;

        SetPlayStatus(PlayStatus.NotPlayedYet);
        //imageLight.enabled = mosaicIndex % 2 == 1; // Minden második sor világosabb szinű

        SetAutoContinuation();
        textLessonMosaicName.text = (mosaicIndex+1).ToString() + ". " + lessonMosaicData.name;

        float buttonWidth = gameIconPrefab.sizeDelta.x;

        // Létrehozunk minden játékhoz egy játék ikont
        gameButtonList = new List<MenuGameButton>();
        for (int i = 0; i < lessonMosaicData.listOfGames.Count; i++)
        {
            MenuGameButton newGameButton = Instantiate(gameIconPrefab).GetComponent<MenuGameButton>();
            newGameButton.gameObject.SetActive(true);
            newGameButton.transform.SetParent(transformBackground, false);
            newGameButton.transform.localScale = Vector3.one;
            newGameButton.Initialize(lessonMosaicData.listOfGames[i], i, ClickGameButton);

            newGameButton.transform.localPosition =
                new Vector3(3 + (i % 4) * gameIconPrefab.sizeDelta.x, -25 + (i / 4) * -gameIconPrefab.sizeDelta.y);

            gameButtonList.Add(newGameButton);
        }

        // Kiszámoljuk az óra mozaiknak a teljes magasságát
        float allHeight = (lessonMosaicData.listOfGames.Count + 3) / 4 * gameIconPrefab.sizeDelta.y + 30;

        // Beállítjuk az óra mozaik komponens magasságát a kiszámolt értékre
        transformLessonMosaic.sizeDelta = new Vector2(transformLessonMosaic.sizeDelta.x, allHeight);

        return allHeight;
    }

    /// <summary>
    /// Beállítja az óra-mozaik automatikus folytatás ikonjának színét
    /// </summary>
    void SetAutoContinuation() {
        // A preview nézetben nem látathó az automatikus folytatás gomb
        autoContinuationOn.SetActive(lessonMosaicData.autoContinuation && Common.menuLessonPlan.status != MenuLessonPlan.Status.Preview);
        autoContinuationOff.SetActive(!lessonMosaicData.autoContinuation && Common.menuLessonPlan.status != MenuLessonPlan.Status.Preview);

        buttonAutoContinuationOn.SetActive(lessonMosaicData.autoContinuation && Common.menuLessonPlan.status != MenuLessonPlan.Status.Preview);
        buttonAutoContinuationOff.SetActive(!lessonMosaicData.autoContinuation && Common.menuLessonPlan.status != MenuLessonPlan.Status.Preview);
    }

    /// <summary>
    /// Beállítja a megadott playStatus-nek megfelelően a háttér színt.
    /// </summary>
    /// <param name="playStatus"></param>
    public void SetPlayStatus(PlayStatus playStatus) {
        Color color = Color.white;
        switch (playStatus)
        {
            case PlayStatus.NotPlayedYet:
                color = notPlayedColor;
                break;
            case PlayStatus.Playing:
                color = playingColor;
                break;
            case PlayStatus.Played:
                color = playedColor;
                break;
        }

        this.playStatus = playStatus;

        //imageLight.enabled = mosaicIndex % 2 == 1; // Minden második sor világosabb szinű

        if (mosaicIndex % 2 == 1)
            color = new Color(
                Mathf.Lerp(color.r, 1, lightRate),
                Mathf.Lerp(color.g, 1, lightRate),
                Mathf.Lerp(color.b, 1, lightRate)
                );

        imageBackground.color = color;
        imageForeground.color = color.SetA(0.7f);
    }

    /// <summary>
    /// Beállítja az óra mozaik hátterének a színét a magodott színre.
    /// </summary>
    /// <param name="color">Milyen színű legyen az óra mozaik háttere.</param>
    public void SetBackgroundColor(Color color) {
        imageBackground.color = color;
    }

    /// <summary>
    /// Beállítja a kiválasztást a megadott elemen.
    /// </summary>
    /// <param name="index">Ha van kiválasztott elem, akkor az melyik, -1 ha nincs elem kiválasztva.</param>
    public void Selected(int index) {
        for (int i = 0; i < gameButtonList.Count; i++)
            gameButtonList[i].Selected(i == index);
    }

    /// <summary>
    /// Megmutatja vagy elrejti az óramozaik előterét amin a play gomb megtalálható.
    /// </summary>
    /// <param name="show">true ha meg kell mutatni és false ha elrejteni kell az előteret.</param>
    public void ShowForeground(bool show) {
        foreground.SetActive(show);
    }

    // 
    public void ClickGameButton(int i) {
        Debug.Log("GameButton : " + i);

        if (Common.menuLessonPlan.status == MenuLessonPlan.Status.Preview)
        {
            // Ha megnyomták valamelyik játék gombját és előnézetben vagyunk, akkor le kell játszani a játékot
            callBack(mosaicIndex, i);
        }
        else {
            // Ha nem előnézetben vagyunk és megnyomták valamelyik játék gombját az olyan mintha a panelt nyomták volna meg
            ClickButton("Panel");
        }
    }

    public void ClickButton(string buttonName) {
        Debug.Log("Panel : " + buttonName);

        if (Common.menuLessonPlan.status != MenuLessonPlan.Status.Preview) {
            switch (buttonName)
            {
                case "Panel":
                    if (foreground.activeInHierarchy)
                        callBack(-1, -2); // Eltüntetjük a kijelölést
                    else
                        callBack(mosaicIndex, -2);
                    break;
                case "AutoContinuationOff":
                case "AutoContinuationOn":
                    lessonMosaicData.autoContinuation = !lessonMosaicData.autoContinuation;
                    SetAutoContinuation();
                    break;
                case "Play":
                    callBack(mosaicIndex, -1);
                    break;
            }
        }
    }

	// Update is called once per frame
	void Update () {
	
	}
}
