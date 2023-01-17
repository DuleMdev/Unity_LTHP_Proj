using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;
using SimpleJSON;

public class MenuLessonPlanList : HHHScreen
{

    public List<TextAsset> lessonPlanAssets; // Az óra terveket tartalmazó lista

    Text textLabelUserName;     // A bejelentkezett felhasználó nevének kiírásához
    GameObject menuLessonPlanRowPrefab; // Előregyártott
    RectTransform content;
    
    //List<string> lessonPlanIDList;
    //List<string> lessonPlanLabelsList;



    List<MenuLessonPlanRow> menuLessonPlanRowList;

    TextAsset selectedAsset;    // Melyik óratervet választották ki.

    // Use this for initialization
    new void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textLabelUserName = Common.SearchGameObject(gameObject, "TextLabelUserName").GetComponent<Text>();

        content = (RectTransform)Common.SearchGameObject(gameObject, "Content").GetComponent<Transform>();

        menuLessonPlanRowPrefab = Common.SearchGameObject(gameObject, "LessonPlanRow").gameObject;
        menuLessonPlanRowPrefab.SetActive(false); // Kikapcsoljuk a prefab láthatóságát
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        textLabelUserName.text = Common.configurationController.userName;

        // Töröljük az esetleg korábban létező lecke terv sorokat
        if (menuLessonPlanRowList != null)
            foreach (MenuLessonPlanRow item in menuLessonPlanRowList)
                Destroy(item.gameObject);

        menuLessonPlanRowList = new List<MenuLessonPlanRow>();

        // Lekérdezzük a létező lecke terveket
        //FillLessonPlanLists();

        float rowHeight = ((RectTransform)menuLessonPlanRowPrefab.transform).sizeDelta.y;

        for (int i = 0; i < Common.configurationController.teacherConfig.lessonPlanList.Count; i++)
        {
            MenuLessonPlanRow menuLessonPlanRow = Instantiate(menuLessonPlanRowPrefab).GetComponent<MenuLessonPlanRow>();
            menuLessonPlanRow.gameObject.SetActive(true);
            menuLessonPlanRow.transform.SetParent(content, false);
            menuLessonPlanRow.transform.localScale = Vector3.one;
            menuLessonPlanRow.Initialize(
                Common.configurationController.teacherConfig.lessonPlanList[i].id,
                Common.configurationController.teacherConfig.lessonPlanList[i].name,
                Common.configurationController.teacherConfig.lessonPlanList[i].labels,
                LessonPlanRowClick);

            menuLessonPlanRow.transform.localPosition = new Vector3(0, -i * rowHeight);

            menuLessonPlanRowList.Add(menuLessonPlanRow);
        }

        // Beállítjuk a tartalmazó panel méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, rowHeight * Common.configurationController.teacherConfig.lessonPlanList.Count);

        yield return null;
    }

    override public IEnumerator ScreenShowStartCoroutine()
    {


        Common.menuBackground.ChangeBackgroundIndex(1);

        Common.menuStripe.SetItem(backButton: true); // Eltüntetjük a menüsávon az egyébb elemeket
        Common.menuStripe.buttonClick = ButtonClick;



        yield return null;
    }

    /*
    /// <summary>
    /// A létező lecke terveket betölti a listába.
    /// </summary>
    void FillLessonPlanLists()
    {
        lessonPlanIDList = new List<string>();
        lessonPlanLabelsList = new List<string>();

        lessonPlanIDList.Add("Óraterv 1. - Szepsi");
        lessonPlanLabelsList.Add("IR, mese, jellemző, szövegértés, szerző");

        lessonPlanIDList.Add("Óraterv 2. - Magyar ");
        lessonPlanLabelsList.Add("IR, mese, jellemző, szereplő, szövegértés, népmese");
    }
    */


    
    public void LessonPlanRowClick(int lessonPlanID, string buttonName) {
        Debug.Log("Óraterv : " + lessonPlanID + " - " + buttonName);

        /*
        Common.configurationController.selectedLessonPlan = lessonPlanID;
        Common.configurationController.lessonPlanView = buttonName == "Preview";
        */



        //Common.menuLessonPlan.taskJSON = lessonPlanAssets[lessonPlanIDList.IndexOf(lessonPlanID)];
        //selectedAsset = lessonPlanAssets[lessonPlanIDList.IndexOf(lessonPlanID)];

        LessonPlanData lessonPlanData = Common.configurationController.teacherConfig.lessonPlanList.GetLessonPlanByName(lessonPlanID);

        Common.menuLessonPlan.selectedLessonPlanName = lessonPlanData.name;
        Common.menuLessonPlan.status = (buttonName == "Preview") ? MenuLessonPlan.Status.Preview : MenuLessonPlan.Status.StudentList;

        Common.menuLessonPlan.lessonMosaicIndex = 0;
        Common.menuLessonPlan.gameIndex = 0;
        Common.menuLessonPlan.screenIndex = 0;

        if (buttonName == "Preview")
        {
            StartCoroutine(LoadLessonPlan(lessonPlanID));



            //Common.screenController.ChangeScreen("MenuLessonPlan");
        }
        else
        {   // Megkérdezzük, hogy melyik osztállyal szeretné elindítani az órát
            Common.infoPanelClassSelector.MakeButtons(-1);
            Common.infoPanelClassSelector.Show((string bName) =>
            {
                switch (bName)
                {
                    case "Selection":
                        // Ha kiválasztottak egy osztályt
                        // Betöltjük az osztályban található gyerekek adatait
                        Common.configurationController.classRoster = new ClassRoster();
                        Common.configurationController.classRoster.LoadFromFile(
                            System.IO.Path.Combine(
                                Common.configurationController.GetClassDirectory(), 
                                Common.infoPanelClassSelector.selectedClassID.ToString()));

                        // Megkérdezzük, hogy biztosan akarja-e, hogy elindítsuk az óratervet
                        Common.infoPanelSureStartLessonPlan.Show((string bName2) =>
                        {
                            switch (bName2)
                            {
                                case "Ok":
                                    // Ha az óraterv elindítására Ok gombra kattintva válaszolt, akkor elindítjuk az óratervet
                                    StartCoroutine(LoadLessonPlan(lessonPlanID));
                                    break;
                                case "Cancel":
                                    Common.menuInformation.Hide();
                                    break;
                            }
                        });

                        break;
                    case "Cancel":
                        Common.menuInformation.Hide();
                        break;
                }
            });
        }
    }




    IEnumerator LoadLessonPlan(int lessonPlanID) {
        Common.infoPanelInformation.Show(C.Texts.LoadLessonPlan, false, null);

        // Megvárjuk amíg teljesen megjelneik az információs panel
        while (!Common.menuInformation.show)
            yield return null;





        try
        {
            Debug.Log(System.DateTime.Now.ToString("HH:mm:ss.fff"));

            // Beolvassuk a háttértárról a lecketervet
            string fileName = System.IO.Path.Combine(Common.configurationController.GetLessonDirectory(), lessonPlanID.ToString());
            Debug.Log("Selected lesson plan : " + fileName);
            string lessonPlanString = System.IO.File.ReadAllText(fileName + ".json");

            Common.menuLessonPlan.lessonPlanData = new LessonPlanData();
            Common.menuLessonPlan.lessonPlanData.LoadDataFromString(lessonPlanString);
            //Common.menuLessonPlan.lessonPlanData = new LessonPlanData("}"); // Egy hibás json, tesztelés miatt
            if (Common.menuLessonPlan.lessonPlanData.error) // Ha gond volt a feldolgozás közben kilépünk
                throw new System.Exception();

            Common.configurationController.lessonPlanData = Common.menuLessonPlan.lessonPlanData;
            Common.configurationController.lessonPlanJSON = JSON.Parse(lessonPlanString);
            //Common.configurationController.lessonPlanJSON = JSON.Parse("}"); // Egy hibás json, tesztelés miatt

            Debug.Log(System.DateTime.Now.ToString("HH:mm:ss.fff"));
        }
        catch (System.Exception)
        {
            string errorString = C.Texts.LessonPlanError + "\n";

            if (Common.menuLessonPlan.lessonPlanData != null)
                errorString += Common.menuLessonPlan.lessonPlanData.errorMessage;

            Common.infoPanelInformation.Show(errorString, true, (string buttonName) => {
                Common.menuInformation.Hide();
            });

            yield break;
        }



        /*
        // Ha játékot indítunk akkor el kell távolítani azokat a játékokat amik még nincsenek kész
        if (Common.menuLessonPlan.status != MenuLessonPlan.Status.Preview)
        {
            // A beolvasott lessonPlan adatokból eltávolítjuk azokat a játékokat, amelyeket nem lehet még játszani
            LessonPlanData lessonPlanData = Common.menuLessonPlan.lessonPlanData;
            if (!lessonPlanData.error)
            {
                for (int i = lessonPlanData.lessonMosaicsList.Count - 1; i >= 0; i--)
                {
                    LessonMosaicData lessonMosaicData = lessonPlanData.lessonMosaicsList[i];

                    for (int j = lessonMosaicData.listOfGames.Count - 1; j >= 0; j--)
                    {
                        GameData gameData = lessonMosaicData.listOfGames[j];

                        switch (gameData.gameType)
                        {
                            case GameData.GameType.TrueOrFalse:
                            case GameData.GameType.Bubble:
                            case GameData.GameType.Sets:
                            case GameData.GameType.MathMonster:
                            case GameData.GameType.Affix:
                            case GameData.GameType.Boom:
                            case GameData.GameType.Fish:
                            case GameData.GameType.Hangman:
                            case GameData.GameType.Read:
                                lessonMosaicData.listOfGames.Remove(gameData);
                                break;
                            case GameData.GameType.Millionaire:
                                break;
                        }
                    }

                    if (lessonMosaicData.listOfGames.Count == 0)
                        lessonPlanData.lessonMosaicsList.Remove(lessonMosaicData);
                }

                if (lessonPlanData.lessonMosaicsList.Count == 0)
                {
                    Common.infoPanelInformation.Show(C.Texts.LessonPlanError + "\n" + Common.menuLessonPlan.lessonPlanData.errorMessage, true, (string buttonName2) =>
                    {
                        Common.menuInformation.Hide();
                    });

                    yield break;
                }
                else
                {
                    for (int i = 0; i < lessonPlanData.lessonMosaicsList.Count; i++)
                    {
                        for (int j = 0; j < lessonPlanData.lessonMosaicsList[i].listOfGames.Count; j++)
                        {
                            lessonPlanData.lessonMosaicsList[i].listOfGames[j].lessonMosaicIndex = i;
                            lessonPlanData.lessonMosaicsList[i].listOfGames[j].gameIndex = j;
                        }
                    }
                }
            }
        }
        */




        //StartCoroutine(Common.menuLessonPlan.lessonPlanData.InitDatas());

        //yield return new WaitForSeconds(2);

        /*
        while (!Common.menuLessonPlan.lessonPlanData.finish) {
            Debug.Log("Percent : " + Common.menuLessonPlan.lessonPlanData.progressPercent);
            Common.infoPanelProgressBar.SetProgressBarValue(Common.menuLessonPlan.lessonPlanData.progressPercent);
            yield return null;
        }
        */

        // Ha rendben megtörtént a beolvasás megyünk a lecketerv képernyőre
        Common.menuInformation.Hide( () => {
            Common.screenController.ChangeScreen("MenuLessonPlan");
        });
      
    }

    /// <summary>
    /// A UI felületen lévő button komponensek hívják meg ha rákattintottak.
    /// </summary>
    /// <param name="buttonName">Melyik gombra kattintottak.</param>
    public void ButtonClick(string buttonName)
    {
        Debug.Log(buttonName);

        switch (buttonName)
        {
            //case "Home":
            case "Back":
                Common.screenController.ChangeScreen("MenuSynchronize");
                break;
        }
    }
}