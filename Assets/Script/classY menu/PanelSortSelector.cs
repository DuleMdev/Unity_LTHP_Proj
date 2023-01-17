using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSortSelector : MonoBehaviour
{
    List<SortButton> listOfSortButton = new List<SortButton>();

    Common.CallBack_In_String buttonClick;

    string activeButtonName;
    bool activeButtonIsAscendant;

    // Use this for initialization
    void Awake ()
    {
        // Összeszedjük a sort gombokat
        int index = 1;
        while (true)
        {
            Transform t = transform.Find("SortButton" + index);
            if (t == null) break;

            listOfSortButton.Add(t.GetComponent<SortButton>());
            index++;
        }
    }

    public void Initialize(Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        listOfSortButton[0].Initialize(C.Texts.SortByName, C.Texts.SortByName, ButtonClick);
        listOfSortButton[1].Initialize(C.Texts.SortByResult, C.Texts.SortByResult, ButtonClick);
        listOfSortButton[2].Initialize(C.Texts.SortByProgress, C.Texts.SortByProgress, ButtonClick);

        // Beállítjuk az elsőt kiválasztottnak
        listOfSortButton[0].SetActive(true);

        // A gombok hogy ne legyenek egymáson ki kell kapcsolni a GameObject-et majd vissza,
        // ekkor a HorizontalLayoutGroup újra rendezi az elemeket és nem lesznek egymáson a gombok
        gameObject.SetActive(false); 
        gameObject.SetActive(true);
        // Talán ez is megtenné
        // LayoutRebuilder.MarkLayoutForRebuild(gameObject.GetComponent<RectTransform>());
    }

    public List<CurriculumItemDriveData> Sort(List<CurriculumItemDriveData> list)
    {
        // Rendezzük a listát a beállított szempont szerint
        switch (activeButtonName)
        {
            case C.Texts.SortByName:
                list.Sort(
                    delegate(CurriculumItemDriveData a, CurriculumItemDriveData b) {
                        int result = a.name.CompareTo(b.name);
                        return activeButtonIsAscendant ? result : result * -1;
                    }
                );
                break;

            case C.Texts.SortByResult:
                list.Sort(
                    delegate (CurriculumItemDriveData a, CurriculumItemDriveData b) {
                        int result = a.scorePercent.CompareTo(b.scorePercent);
                        return activeButtonIsAscendant ? result : result * -1;
                    }
                );
                break;

            case C.Texts.SortByProgress:
                list.Sort(
                    delegate (CurriculumItemDriveData a, CurriculumItemDriveData b) {
                        int result = a.maxCurriculumProgress.CompareTo(b.maxCurriculumProgress);
                        return activeButtonIsAscendant ? result : result * -1;
                    }
                );
                break;
        }

        return list;
    }

    public void SetButtonVisibility(bool visible)
    {
        foreach (SortButton sortButton in listOfSortButton)
        {
            sortButton.gameObject.SetActive(visible);
        }
    }

    public void ButtonClick(string buttonName)
    {
        // Aktíváljuk a megnyomott gombot a többit inaktíváljuk
        foreach (SortButton sortButton in listOfSortButton)
        {
            sortButton.SetActive(sortButton.buttonClickName == buttonName);
            if (sortButton.buttonClickName == buttonName) {
                activeButtonName = sortButton.buttonClickName;
                activeButtonIsAscendant = sortButton.isAscendant;
            }
        }

        if (buttonClick != null)
            buttonClick(C.Program.SortChange); // + ":" + buttonName);
    }
}
