using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class InfoPanelClassSelector : InfoPanelBase
{
    [Tooltip("A gombok vízszintes távolsága")]
    public float distanceWidth;
    [Tooltip("A gombok függőleges távolsága")]
    public float distanceHeight;

    Text textInformation;
    RectTransform content;
    Text textSelectionButton;
    Text textCancelButton;

    GameObject buttonNamePrefab; // Előregyártott gomb komponens

    List<UIButtonStudentName> listOfButtonName = new List<UIButtonStudentName>();

    public int selectedClassID;         // A kiválasztott osztály

    override protected void SearchComponents()
    {
        Common.infoPanelClassSelector = this;

        textInformation = Common.SearchGameObject(gameObject, "TextLabelInformation").GetComponent<Text>();

        content = (RectTransform)Common.SearchGameObject(gameObject, "Content").GetComponent<Transform>();

        textSelectionButton = Common.SearchGameObject(gameObject, "TextSelection").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancel").GetComponent<Text>();

        buttonNamePrefab = Common.SearchGameObject(gameObject, "ButtonStudentName").gameObject;
        buttonNamePrefab.SetActive(false); // Kikapcsoljuk a prefab láthatóságát
    }

    /*
    public void Show(string information, bool okButton, Common.CallBack_In_String callBack)
    {

        Show(callBack);
    }
    */

    /// <summary>
    /// Eltávolítja a korábban létrehozott gombokat.
    /// </summary>
    public void RemoveButtons()
    {
        foreach (UIButtonStudentName buttonStudentName in listOfButtonName)
            Destroy(buttonStudentName.gameObject);

        listOfButtonName.Clear();
    }

    /// <summary>
    /// Létrehozzuk az osztálynévsorban található nevek számára a gombokat.
    /// </summary>
    /// <param name="selectedClassID">Melyik osztály legyen alapból kiválasztot.</param>
    public void MakeButtons(int selectedClassID)
    {
        RemoveButtons();

        SetSelectedClassID(-1);

        // Lekérdezzük az osztály listát
        ClassList classList = Common.configurationController.teacherConfig.classList;

        float baseY = -distanceHeight + (classList.Count - 1) / 3 * (distanceHeight / 2);
        if (baseY > 0)
            baseY = 0;

        // Létrehozzuk a szükséges számú gombot ha még nincs létrehozva
        int i;
        for (i = 0; i < classList.Count; i++)
        {
            UIButtonStudentName button = Instantiate(buttonNamePrefab).GetComponent<UIButtonStudentName>();
            button.gameObject.SetActive(true);
            button.transform.SetParent(content, false);
            button.transform.localScale = Vector3.one;
            button.Initialize(classList[i].id, classList[i].name, (classList[i].id == selectedClassID) ? UIButtonStudentName.ButtonStatus.Selected : UIButtonStudentName.ButtonStatus.Selectable, ButtonClick);
            if (classList[i].id == selectedClassID)
                SetSelectedClassID(selectedClassID);

            // Kiszámoljuk az X koordinátát
            float baseX = 7;

            int maxRow = (classList.Count + 2) / 3;
            int actRow = (i + 3) / 3;
            if (maxRow == actRow) {
                switch (classList.Count % 3)
                {
                    case 1:
                        baseX += distanceWidth;
                        break;
                    case 2:
                        baseX += distanceWidth / 2;
                        break;
                    default:
                        break;
                }
            }

            button.transform.localPosition = new Vector3(baseX + i % 3 * distanceWidth, baseY - i / 3 * distanceHeight);

            listOfButtonName.Add(button);
        }

        // Beállítjuk a tartalmazó panel méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, baseY + ((i - 1) / 3 + 1) * distanceHeight);

        // Meghatározzuk, hogy a tanárnak melyik osztállyal van órája az órarend szerint
        SetSelectedClassID(Common.configurationController.teacherConfig.timeList.GetNextClassID());

        // Ha csak egy osztály van, akkor azzal lesz a tanárnak órája
        if (classList.Count == 1)
            SetSelectedClassID(classList[0].id);

        SetButtonsColor();
    }

    /// <summary>
    /// Beállítja a kiválasztott osztályt, a kiválasztás gombot ki/be kapcsolja attól függően, hogy van-e
    /// legalább 1 osztály kiválasztva.
    /// </summary>
    /// <param name="selectedClassID">A kiválasztott osztály azonosítója.</param>
    void SetSelectedClassID(int selectedClassID) {
        this.selectedClassID = selectedClassID;

        textSelectionButton.transform.parent.gameObject.SetActive(selectedClassID != -1);
    }

    /// <summary>
    /// Beállítja a név gombok szinét a kiválasztottságuknak megfelelően.
    /// </summary>
    void SetButtonsColor() {
        // Beállítjuk a gombok kiválasztottságát
        // Lekérdezzük az osztály listát
        ClassList classList = Common.configurationController.teacherConfig.classList;

        for (int i = 0; i < classList.Count; i++)
        {
            listOfButtonName[i].SetButtonState((classList[i].id == selectedClassID) ? UIButtonStudentName.ButtonStatus.Actual : UIButtonStudentName.ButtonStatus.Selectable);
        }
    }

    /// <summary>
    /// Melyik gombot nyomták meg. A gombok ezt a metódust hívják ha rájuk kattintottak.
    /// </summary>
    /// <param name="buttonName">A megnyomott gomb neve.</param>
    new public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Selection":
                // Ha megnyomták a kiválasztás gombot, akkor megnézzük, hogy van-e osztály kiválasztva
                if (selectedClassID != -1) {
                    base.ButtonClick(buttonName);
                }
                break;

            case "Cancel":
                base.ButtonClick(buttonName);
                break;

            default:
                // Megkeressük a kattintott osztály azonosítóját
                SetSelectedClassID(int.Parse(buttonName));
                SetButtonsColor();
                break;
        }
    }

    override protected void ShowComponents()
    {
        textInformation.text = Common.languageController.Translate(C.Texts.SelectClassToLesson);

        textSelectionButton.text = Common.languageController.Translate(C.Texts.Selection);
        textCancelButton.text = Common.languageController.Translate(C.Texts.Cancel);
    }

}