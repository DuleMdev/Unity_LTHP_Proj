using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OTPPanelCurriculumList : MonoBehaviour {

    PanelSortSelector sortSelector;
    OTPCurriculumListVertical curriculumList;

    Common.CallBack_In_String buttonClick;

    List<CurriculumItemDriveData> listOfCurriculumItems;

    string buttonNamePrefix;

    // Use this for initialization
    void Awake () {
        sortSelector = gameObject.SearchChild("PanelSortSelector").GetComponent<PanelSortSelector>();
        curriculumList = gameObject.SearchChild("CurriculumList").GetComponent<OTPCurriculumListVertical>();
	}

    public void Initialize(List<CurriculumItemDriveData> list, string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        listOfCurriculumItems = list;
        this.buttonNamePrefix = buttonNamePrefix;
        this.buttonClick = buttonClick;

        sortSelector.Initialize(ButtonClick);
        DrawCurriculumItems();
    }

    public void DrawCurriculumItems()
    {
        curriculumList.Initialize(sortSelector.Sort(listOfCurriculumItems), buttonNamePrefix, ButtonClick);
        // Ha üres lista esetén elszeretnénk tüntetni sort gombokat
        //sortSelector.SetButtonVisibility(listOfCurriculumItems != null && listOfCurriculumItems.Count != 0);
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
