using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MainMenuClassY : MonoBehaviour
{
    public GameObject menuItemPrefab;

    public List<MainMenuItemsData> listOfIcons;

    [HideInInspector]
    public MainMenuClassYMenuStripe menuStripe;
    //EduStripe eduStripe;

    RectTransform rectTransformContent; // A menü elemeket ebbe a GameObject-be tesszük
    RectTransform scrollRect;

    List<ClassYMainMenuItem> listOfMainMenuItems = new List<ClassYMainMenuItem>();

    Common.CallBack_In_String buttonClick;

    // Use this for initialization
    void Awake()
    {
        menuStripe = transform.Find("MenuStripe").GetComponent<MainMenuClassYMenuStripe>();
        //eduStripe = transform.Find("EduStripe").GetComponent<EduStripe>();

        rectTransformContent = gameObject.SearchChild("Content").GetComponent<RectTransform>(); //  (RectTransform)transform.Find("Content");
        scrollRect = (RectTransform)rectTransformContent.parent.parent;
    }

    public void Initialize(bool dark, List<string> menuNames, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        menuStripe.Initialize(ButtonClick);
        /*
        menuStripe.SetColor(
            (dark) ? darkBackgroundColor : lightBackgroundColor,
            (!dark) ? darkBackgroundColor : lightBackgroundColor);
            */

        // Töröljük az esetlegesen már létező menü elemeket
        ClassYMainMenuItem[] existsItem = GetComponentsInChildren<ClassYMainMenuItem>(true);
        for (int i = 0; i < existsItem.Length; i++)
            Destroy(existsItem[i].gameObject);

        // Létrehozzuk a megadott számú és típusú menü elemeket
        listOfMainMenuItems = new List<ClassYMainMenuItem>();
        Debug.Log("HELLOHELLOHELLO");
        for (int i = 0; i < menuNames.Count; i++)
        {
            MainMenuItemsData itemsData = MainMenuItemsData.GetByName(menuNames[i], listOfIcons);
            Debug.Log(menuNames[i]);

            ClassYMainMenuItem newItem = Instantiate(menuItemPrefab, rectTransformContent).GetComponent<ClassYMainMenuItem>();
            newItem.Initialize(itemsData.getText, "0", "MainMenu:" + menuNames[i], ButtonClick, itemsData.color, true, itemsData.icon);
            newItem.SetActive(false); // Az ikon nem lesz kiválasztott (nincs körülötte keret, és a szöveg színe is fekete marad nem narancssárga)

            listOfMainMenuItems.Add(newItem);
        }

        // Vízszintesen Középre igazítjuk az ikonokat
        float itemWidth = ((RectTransform)menuItemPrefab.transform).sizeDelta.x;
        float allItemWidth = itemWidth * menuNames.Count;

        float scrollWidth = scrollRect.rect.width + scrollRect.sizeDelta.x;

        // Ha a létrehozott elemek össz szélessége kisebb mint a rendelkezésre álló hely, akkor középre igazítja az elemeket
        if (scrollWidth > allItemWidth)
        {
            scrollRect.GetComponent<ScrollRect>().horizontal = false;
            rectTransformContent.anchoredPosition = new Vector2((scrollWidth - allItemWidth) / 2, 0);
        }
        else
        {
            rectTransformContent.sizeDelta = new Vector2(allItemWidth, 0);
        }

        /*
        for (int i = 0; i < listOfMainMenuItems.Count; i++) { 
            listOfMainMenuItems[i].Initialize(menuNames[i], "0", "MainMenu:" + i, ButtonClick);
            listOfMainMenuItems[i].SetActive(false);
        }
        */

        /*
        eduStripe.Initialize(
            (dark) ? "drive" : "store",
            0,
            ButtonClick
            );
            */
    }

    /// <summary>
    /// Kiválasztottá teszi a megadott ID-jű elemet.
    /// </summary>
    /// <param name="ID"></param>
    public void SetSelected(string ID)
    {
        for (int i = 0; i < listOfMainMenuItems.Count; i++)
            listOfMainMenuItems[i].SetActive(listOfMainMenuItems[i].buttonName == ID);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }

    [System.Serializable]
    public class MainMenuItemsData {
        public string name;
        public string text;
        public Color color;
        public Sprite icon;

        public string getText {
            get {
                return string.IsNullOrWhiteSpace(text) ? name : text;
            }
        }

        static public MainMenuItemsData GetByName(string name, List<MainMenuItemsData> list) {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].name == name)
                    return list[i];
            }

            return null;
        }
    }
}
