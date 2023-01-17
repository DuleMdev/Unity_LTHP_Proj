using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuLessonMosaicList : MonoBehaviour {

    GameObject lessonMosaicPrefab;    // A lecke mozaik prefabja amit majd sokszorosítunk
    RectTransform content;

    List<MenuLessonMosaic> lessonMosaicList;

    Common.CallBack_In_Int_Int callBack; // A gombnyomásokkor meghívandó függvény

    [HideInInspector]
    public bool showPlayButton; // Valamelyik óramozaikon látható a play gomb

    // Use this for initialization
    void Awake () {

        // Összeszedjük az állítandó componensekre a referenciákat
        content = (RectTransform)Common.SearchGameObject(gameObject, "Content").GetComponent<Transform>();

        lessonMosaicPrefab = Common.SearchGameObject(gameObject, "LessonMosaic").gameObject;
        lessonMosaicPrefab.SetActive(false);

        lessonMosaicList = new List<MenuLessonMosaic>();
	}

    public void Initialize(LessonPlanData lessonPlanData, Common.CallBack_In_Int_Int callBack) {
        this.callBack = callBack;

        // A korábban létrehozott óramozaikokat kitöröljük
        foreach (MenuLessonMosaic lessonMosaic in lessonMosaicList)
            Destroy(lessonMosaic.gameObject);
        lessonMosaicList.Clear();

        // Létrehozzuk a lecketervben levő óramozaikokat
        float posY = 0; // Hova kerüljön az óra mozaik
        lessonMosaicList = new List<MenuLessonMosaic>();
        for (int i = 0; i < lessonPlanData.lessonMosaicsList.Count; i++)
        {
            MenuLessonMosaic newLessonMosaic = Instantiate(lessonMosaicPrefab).GetComponent<MenuLessonMosaic>();
            newLessonMosaic.gameObject.SetActive(true);
            newLessonMosaic.transform.SetParent(content, false);
            newLessonMosaic.transform.localScale = Vector3.one;
            float height = newLessonMosaic.Initialize(lessonPlanData.lessonMosaicsList[i], i, ClickButton);

            newLessonMosaic.transform.localPosition = new Vector3(0, -posY);
            posY += height;

            lessonMosaicList.Add(newLessonMosaic);
        }

        content.sizeDelta = new Vector2(content.sizeDelta.x, posY);
    }

    /// <summary>
    /// Megadjuk, hogy aktuálisan melyik óra-mozaik van lejátszás alatt.
    /// </summary>
    /// <param name="lessonMosaicIndex">A lejátszás alatt levő óra-mozaik indexe</param>
    public void SetPlay(int lessonMosaicIndex) {
        for (int i = 0; i < lessonMosaicList.Count; i++)
        {
            if (i == lessonMosaicIndex) {
                lessonMosaicList[i].SetPlayStatus(MenuLessonMosaic.PlayStatus.Playing);
            } else {
                if (lessonMosaicList[i].playStatus == MenuLessonMosaic.PlayStatus.Playing)
                    lessonMosaicList[i].SetPlayStatus(MenuLessonMosaic.PlayStatus.Played);
            }
        }
    }

    /// <summary>
    /// Melyik óramozaik és azon belül melyik játék legyen megjelölve.
    /// </summary>
    /// <param name="lessonMosaicIndex">A kiválasztott óramozaik indexe.</param>
    /// <param name="gameIndex">A kiválasztott óramozaikon belül a kiválasztott játék.</param>
    public void Selected(int lessonMosaicIndex, int gameIndex) {
        for (int i = 0; i < lessonMosaicList.Count; i++)
        {
            lessonMosaicList[i].Selected((i == lessonMosaicIndex) ? gameIndex : -1);
        }
    }

    /// <summary>
    /// A megadott óramozaikon bekapcsoljuk a play gomb láthatóságát.
    /// </summary>
    /// <param name="lessonMosaicIndex">Melyik óramozaikon kapcsoljuk be a play gombot. Ha -1 -et adunk meg, akkor az összesen kikapcsolja.</param>
    public void ShowPlayButton(int lessonMosaicIndex) {
        for (int i = 0; i < lessonMosaicList.Count; i++)
            lessonMosaicList[i].ShowForeground(i == lessonMosaicIndex);

        showPlayButton = lessonMosaicIndex >= 0;
    }

    public void ClickButton(int lessonMosaicIndex, int buttonIndex)
    {
        if (Common.menuLessonPlan.status == MenuLessonPlan.Status.Preview)
        {
            callBack(lessonMosaicIndex, buttonIndex);
        }
        else {
            if (buttonIndex == -1) { // A Play gombot nyomták meg az egyik óramozaikon
                callBack(lessonMosaicIndex, -1);
            }
            else { // A panelt vagy valamelyik game button nyomták meg
                // A play gombot meg kell mutatni a lessonMosaicIndex-edik elemen a többin pedig eltüntetni
                ShowPlayButton(lessonMosaicIndex);
            }
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
