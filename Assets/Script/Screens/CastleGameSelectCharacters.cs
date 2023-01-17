using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameSelectCharacters : HHHScreen
{
    public Sprite[] sprites;

    public Color inactiveButtonColor;
    Color activeButtonColor;

    Text textMaxLevel;

    Image button;

    Transform hold; // Erre a transformra tesszük az éppen megfogott elemet, hogy minden fölött lehessen
    RectTransform prefab;  // Ezt a prefabot kell sokszorosítani, hogy a szereplőket megjelenítsük

    ScrollRect scrollRect;  // Ide tesszük a szereplőket

    Transform place;    // Ez tartalmazza a kiválasztott figurákat

    CharacterSelectDragAndDropControl characterSelectDragAndDropControl;

    int lastCharacterPage;
    int actCharacterPage;

    List<CharacterSelectDrag> madeCharacterSelectDrag;

    // Start is called before the first frame update
    void Awake()
    {
        textMaxLevel = gameObject.SearchChild("TextMaxLevel").GetComponent<Text>();

        button = gameObject.SearchChild("ButtonWithArrow/Image").GetComponent<Image>();

        hold = gameObject.SearchChild("Hold").transform;
        prefab = gameObject.SearchChild("Prefab").GetComponent<RectTransform>();
        scrollRect = gameObject.SearchChild("Scroll View").GetComponent<ScrollRect>();

        place = gameObject.SearchChild("Place").GetComponent<RectTransform>();

        characterSelectDragAndDropControl = GetComponent<CharacterSelectDragAndDropControl>();

        activeButtonColor = button.color;
    }

    /// <summary>
    /// Mielőtt a képernyő láthatóvá válna a ScreenController meghívja ezt a metódust, hogy inicializája magát. 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator InitCoroutine()
    {
        madeCharacterSelectDrag = new List<CharacterSelectDrag>();

        List<CharacterData> ListOfCharacterData = CharacterData.GetListOFCharacterData(CastleGameInstructionScreen.jsonUserInventoryData[C.JSONKeys.availableFrameGameCharacters]);

        //JSONNode characters = CastleGameInstructionScreen.jsonUserInventoryData[C.JSONKeys.availableFrameGameCharacters];

        // Kirajzoljuk a szereplőket
        float pos = 0;
        for (int i = 0; i < ListOfCharacterData.Count; i++)
        {
            RectTransform rectTransform = Instantiate(prefab, scrollRect.content);
            rectTransform.gameObject.SetActive(true);

            rectTransform.localPosition = rectTransform.localPosition.SetX(pos);
            pos += rectTransform.sizeDelta.x;

            CharacterSelectDrag characterSelectDrag = rectTransform.GetComponentInChildren<CharacterSelectDrag>();

            characterSelectDrag.Initialize(ListOfCharacterData[i], hold);

            madeCharacterSelectDrag.Add(characterSelectDrag);
        }

        // Kiszámoljuk a lapok számát
        //lastCharacterPage = (sprites.Length - 1) / 5;
        lastCharacterPage = (ListOfCharacterData.Count - 1) / 5;
        actCharacterPage = 0;

        EnabledCharacters();

        // Beállítjuk a Content méretét
        scrollRect.content.sizeDelta = new Vector2(1100 * (lastCharacterPage + 1), 0);

        textMaxLevel.text = CastleGameInstructionScreen.curriculumPathData.possibleMaxLevelNumber;

        yield break;
    }

    /*
    override public IEnumerator InitCoroutine()
    {
        madeCharacterSelectDrag = new List<CharacterSelectDrag>();

        // Kirajzoljuk a szereplőket
        float pos = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            RectTransform rectTransform = Instantiate(prefab, scrollRect.content);
            rectTransform.gameObject.SetActive(true);

            rectTransform.localPosition = rectTransform.localPosition.SetX(pos);
            pos += rectTransform.sizeDelta.x;

            CharacterSelectDrag characterSelectDrag = rectTransform.GetComponentInChildren<CharacterSelectDrag>();

            characterSelectDrag.Initialize(i.ToString(), hold, sprites[i]);

            madeCharacterSelectDrag.Add(characterSelectDrag);
        }

        // Kiszámoljuk a lapok számát
        lastCharacterPage = (sprites.Length - 1) / 5;
        actCharacterPage = 0;

        EnabledCharacters();

        // Beállítjuk a Content méretét
        scrollRect.content.sizeDelta = new Vector2(1100 * (lastCharacterPage + 1), 0);

        yield break;
    }
    */

    bool SelectedThreeCharactersAlready()
    {
        for (int i = 0; i < place.childCount; i++)
        {
            if (place.GetChild(i).Find("FixPos").childCount == 0)
                return false;
        }

        return true;
    }

    void EnabledCharacters()
    {
        for (int i = 0; i < madeCharacterSelectDrag.Count; i++)
        {
            madeCharacterSelectDrag[i].dragEnabled = i / 5 == actCharacterPage;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Megvizsgáljuk, hogy ki választottak-e már három karaktert, ennek megfelelően
        button.color = SelectedThreeCharactersAlready() ? activeButtonColor : inactiveButtonColor;

        // Beállítjuk a scrollRect pozícióját, hogy az aktuális lap látszódjon ha több lap van mint egy
        if (lastCharacterPage > 0)
        {
            // Egy lap milyen hosszú
            float onePageLength = 1.0f / lastCharacterPage;

            float targetPos = onePageLength * actCharacterPage;

            float actPos = scrollRect.horizontalNormalizedPosition;

            float diff = targetPos - actPos;

            characterSelectDragAndDropControl.dragAndDropEnabled = onePageLength / 20 > Mathf.Abs(diff);

            scrollRect.horizontalNormalizedPosition = actPos + diff * 0.1f;
        }
    }

    public void ButtonClick(string buttonName)
    {
        Debug.Log(buttonName);

        switch (buttonName)
        {
            case "Next":
                if (SelectedThreeCharactersAlready())
                {
                    // Rögzítjük a válaszott karaktereket a curriculumPathData-ban
                    CharacterData heroData = place.GetChild(0).GetComponentInChildren<CharacterSelectDrag>().characterData;
                    CharacterData monsterData = place.GetChild(1).GetComponentInChildren<CharacterSelectDrag>().characterData;
                    CharacterData victimData = place.GetChild(2).GetComponentInChildren<CharacterSelectDrag>().characterData;
                    CastleGameInstructionScreen.curriculumPathData.SetFrameGameCharacters(heroData, monsterData, victimData);

                    //Elküldjük a választott karaktereket a szervernek is, hogy tudjon róla
                    ClassYServerCommunication.instance.SetFrameGameCharacters(
                        CastleGameInstructionScreen.curriculumPathData.ID,
                        CastleGameInstructionScreen.curriculumPathData.mailListID,
                        heroData.id,
                        monsterData.id,
                        victimData.id,
                        (bool success, JSONNode response) =>
                        {
                            if (success)
                            {
                                // Ha sikeres volt a mentés visszatérünk a hívás helyére
                                CastleGameInstructionScreen.callBack();
                            }
                        }
                    );
                }
                break;
            case "Left":
                if (actCharacterPage > 0)
                {
                    actCharacterPage--;
                    EnabledCharacters();
                }
                break;
            case "Right":
                if (actCharacterPage < lastCharacterPage)
                {
                    actCharacterPage++;
                    EnabledCharacters();
                }
                break;


        }
    }
}
