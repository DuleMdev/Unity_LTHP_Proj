using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PanelSubMenu : MonoBehaviour
{
    GameObject backStripe;  // Vissza gomb
    //Text textBack;    // Kiváltotta a SetLanguageText komponens
    RectTransform content;  // A subMenuItem-eket tartalmazó rectTransform
    ClassYMainMenuItem mainMenuItem; 

    Common.CallBack_In_String buttonClick;

    List<ClassYMainMenuItem> listOfMainMenuItems = new List<ClassYMainMenuItem>(); // A létrehozott SubMenuItems, a törlésükhöz kell

	// Use this for initialization
	void Awake() {
        // Megkeressük a szükséges referenciákat
        backStripe = gameObject.SearchChild("BackStripe").gameObject;
        //textBack = gameObject.SearchChild("TextBack").GetComponent<Text>();
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        mainMenuItem = gameObject.SearchChild("ClassYSubMenuIconControl").GetComponent<ClassYMainMenuItem>();
    }

    void Start() {
        // Kikapcsoljuk a vissza gombot
        backStripe.SetActive(false);

        // Eltüntetjük a másolandó almenü elemet
        mainMenuItem.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="back">Legyen-e vissza gomb.</param>
    /// <param name="list">Az elemek listája amit meg kell mutatni.</param>
    /// <param name="buttonClick">A kattintás eseményt hová küldjük.</param>
    public void Initialize(bool back, List<SubFolderDatas> list, string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        backStripe.SetActive(back);
        //textBack.text = Common.languageController.Translate(C.Texts.Back);
        this.buttonClick = buttonClick;

        // A már létező menü elemeket töröljük
        for (int i = 0; i < listOfMainMenuItems.Count; i++)
        {
            Destroy(listOfMainMenuItems[i].gameObject);
        }
        listOfMainMenuItems.Clear();

        // Létrehozzuk az új menü elemeket
        for (int i = 0; i < list.Count; i++)
        {
            ClassYMainMenuItem newMainMenuItem = Instantiate(mainMenuItem);
            newMainMenuItem.gameObject.SetActive(true);
            newMainMenuItem.transform.SetParent(content.transform);
            newMainMenuItem.transform.localScale = Vector3.one;

            newMainMenuItem.Initialize(list[i].name, list[i].notice.ToString(), buttonNamePrefix + ":" + list[i].ID, ButtonClick, ColorBuilder.GetColor(list[i].name));
            newMainMenuItem.SetActive(false);

            RectTransform recTranform = newMainMenuItem.GetComponent<RectTransform>();
            recTranform.localPosition = recTranform.localPosition.SetX(recTranform.sizeDelta.x * i);
            recTranform.sizeDelta = new Vector2(recTranform.sizeDelta.x, 0);
            recTranform.anchoredPosition = new Vector2(recTranform.anchoredPosition.x, 0);

            listOfMainMenuItems.Add(newMainMenuItem);
        }

        // Beállítjuk a tartalom méretét
        content.sizeDelta = new Vector2(mainMenuItem.GetComponent<RectTransform>().sizeDelta.x * list.Count, content.sizeDelta.y);
    }

    /// <summary>
    /// Kiválasztottá teszi a megadott ID-jű elemet.
    /// </summary>
    /// <param name="ID"></param>
    public void SetSelected(string ID) {
        for (int i = 0; i < listOfMainMenuItems.Count; i++)
            listOfMainMenuItems[i].SetActive(listOfMainMenuItems[i].buttonName == ID);
    }

    public void ButtonClick(string buttonName) {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
