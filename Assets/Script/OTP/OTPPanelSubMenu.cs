using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nem csak egy subMenüt tartalmaz, hanem egy tananyag listázót is a hozzá tartozó sortSelectorral.
/// </summary>
public class OTPPanelSubMenu : MonoBehaviour
{
    public PanelSubMenu panelSubMenu;
    public PanelSortSelector sortSelector;
    public OTPCurriculumListHorizontal curriculumList;

    Common.CallBack_In_String buttonClick;

    List<CurriculumItemDriveData> listOfCurriculumItem;

    // Use this for initialization
    void Awake ()
    {
        panelSubMenu = gameObject.SearchChild("PanelSubMenu").GetComponent<PanelSubMenu>();
        sortSelector = gameObject.SearchChild("PanelSortSelector").GetComponent<PanelSortSelector>();
        curriculumList = gameObject.SearchChild("CurriculumList").GetComponent<OTPCurriculumListHorizontal>();
	}

    public void Initialize(Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        sortSelector.Initialize(ButtonClick);
        curriculumList.Initialize(null, null);
    }

    public void DrawCurriculumItems(List<CurriculumItemDriveData> list)
    {
        listOfCurriculumItem = list;
        DrawCurriculumItems();
    }

    void DrawCurriculumItems()
    {
        curriculumList.Initialize(sortSelector.Sort(listOfCurriculumItem), ButtonClick);
        sortSelector.SetButtonVisibility(listOfCurriculumItem != null && listOfCurriculumItem.Count != 0);
    }

    public void ButtonClick(string buttonName)
    {
        // Ha megváltoztatták a listázási szempontot, akkor újra kirajzoljuk
        if (buttonName == C.Program.SortChange)
            DrawCurriculumItems();
        else if (buttonClick != null) // Egyébként a gombnyomást vissza adjuk, ha van feldolgozó beállítva
            buttonClick(buttonName);
    }
}
