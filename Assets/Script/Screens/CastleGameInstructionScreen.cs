using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameInstructionScreen : HHHScreen
{
    static public JSONNode jsonUserInventoryData;
    static public CurriculumPathData curriculumPathData;
    static public Common.CallBack callBack; // Ha befejeztük a játékot mit hívjon meg

    public TextAsset curriculumPathDataJson;

    Animator pergamentAnimator;

    GameObject coinDescription;
    GameObject chestDescription;

    GameObject pergament;
    bool pergamentIsOpen;

    //Text textDescription;
    //RectTransform content;

    // Start is called before the first frame update
    void Awake()
    {
        pergament = gameObject.SearchChild("Pergament").gameObject;
        //textDescription = gameObject.SearchChild("TextGameDescription").GetComponent<Text>();
        //content = gameObject.SearchChild("Content").GetComponent<RectTransform>();

        pergamentAnimator = gameObject.SearchChild("Pergament").GetComponent<Animator>();


        coinDescription = gameObject.SearchChild("CoinDescription").gameObject;
        chestDescription = gameObject.SearchChild("ChestDescription").gameObject;

        if (curriculumPathData == null)
            curriculumPathData = new CurriculumPathData(JSON.Parse(curriculumPathDataJson.text)[C.JSONKeys.answer][0]);
    }

    override public IEnumerator InitCoroutine()
    {
        Initialize();

        yield return null;
    }

    public void Initialize()
    {
        //pergament.SetActive(false);

        /*
        int fontSize = 35;
        string text =
            // Cím kiírása
            $"<b>\n{Common.languageController.Translate(C.Texts.FrameGameInstructionScreenTitle)}</b>\n" +
            // Alcím kiírása he lenne, de nincs
            //$"<size=30>\n{data.subtitle}</size></b>\n" +
            // Kastély játék leírása
            $"<size={fontSize}>\n{Common.languageController.Translate(C.Texts.FrameGameInstructionScreenDescription)}\n</size>";
        
        // Láda szövegének kiírása ha van láda
        if (_curriculumPathData.chestExists)
            text += $"<size={fontSize}>\n{Common.languageController.Translate(C.Texts.FrameGameInstructionScreenChestDescription)}\n</size>";
        // Zseton játék szövegének kiírása ha van zseton játék
        if (_curriculumPathData.coinGameExists)
            text += $"<size={fontSize}>\n{Common.languageController.Translate(C.Texts.FrameGameInstructionScreenCoinDescription)}\n</size>";
        
        textDescription.text = text;

        float min = ((RectTransform)content.parent).sizeDelta.y;

        // Beállítjuk a content méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Max(min, textDescription.preferredHeight + 80));
        */

        chestDescription.SetActive(curriculumPathData.chestExists);
        coinDescription.SetActive(curriculumPathData.coinGameExists);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Next":

                Common.screenController.ChangeScreen(C.Screens.CastleGameSelectCharacters);
                //_callBack();

                //Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;

            case "Info":
                // Ki/be kapcsolgatjuk a pergament
                pergamentIsOpen = !pergamentIsOpen;

                pergamentAnimator.Play(pergamentIsOpen ? "PergamentOpen" : "PergamentClose");

                //pergament.SetActive(!pergament.activeSelf);
                break;
        }
    }


    static public void Load(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        CastleGameInstructionScreen.curriculumPathData = curriculumPathData;
        CastleGameInstructionScreen.callBack = callBack;

        // Lekérdezzük a játék adatait
        ClassYServerCommunication.instance.GetUserInventory(
            (bool success, JSONNode response) =>
            {
                if (success)
                {
                    // Ha megjöttek az adatok, akkor inicializáljuk a játékot
                    jsonUserInventoryData = response[C.JSONKeys.answer];

                    // Átváltunk a kastély játék instrukciós képernyőjére
                    Common.screenController.ChangeScreen(C.Screens.CastleGameInstructionScreen);
                }
            }
        );
    }
}
