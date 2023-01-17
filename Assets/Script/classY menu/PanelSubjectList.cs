using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Tantárgyakat listázza a classYStore, illetve az OTPMain képernyőn.
*/
public class PanelSubjectList : MonoBehaviour {

    RectTransform content;  // A subjectItem-eket tartalmazó rectTransform
    ClassYMainMenuItem subjectItem;

    Common.CallBack_In_String buttonClick;

    List<ClassYMainMenuItem> listOfSubjectItems = new List<ClassYMainMenuItem>(); // A létrehozott SubjectItems, a törlésükhöz kell

    void Awake() {
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        subjectItem = gameObject.SearchChild("SubjectItem").GetComponent<ClassYMainMenuItem>();
    }

    // Use this for initialization
    void Start () {
        // Eltüntetjük a másolandó tantárgy elemet
        subjectItem.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="back">Legyen-e vissza gomb.</param>
    /// <param name="list">Az elemek listája amit meg kell mutatni.</param>
    /// <param name="buttonClick">A kattintás eseményt hová küldjük.</param>
    public void Initialize(List<SubFolderDatas> list, string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        // A már létező menü elemeket töröljük
        for (int i = 0; i < listOfSubjectItems.Count; i++)
        {
            Destroy(listOfSubjectItems[i].gameObject);
        }
        listOfSubjectItems.Clear();

        // Létrehozzuk az új menü elemeket
        for (int i = 0; i < list.Count; i++)
        {
            ClassYMainMenuItem newSubjectItem = Instantiate(subjectItem);
            newSubjectItem.gameObject.SetActive(true);
            newSubjectItem.transform.SetParent(content.transform);
            newSubjectItem.transform.localScale = Vector3.one;

            newSubjectItem.Initialize(list[i].name, list[i].notice.ToString(), buttonNamePrefix + ":" + list[i].ID, ButtonClick, ColorBuilder.GetColor(list[i].name));
            newSubjectItem.SetActive(false);

            RectTransform recTranform = newSubjectItem.GetComponent<RectTransform>();
            recTranform.localPosition = recTranform.localPosition.SetX(recTranform.sizeDelta.x * i);
            recTranform.sizeDelta = new Vector2(recTranform.sizeDelta.x, 0);
            recTranform.anchoredPosition = new Vector2(recTranform.anchoredPosition.x, 0);

            listOfSubjectItems.Add(newSubjectItem);
        }

        // Beállítjuk a tartalom méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, subjectItem.GetComponent<RectTransform>().sizeDelta.y * (listOfSubjectItems.Count + 4) / 5);
    }

    /// <summary>
    /// Kiválasztottá teszi a megadott ID-jű elemet.
    /// </summary>
    /// <param name="ID"></param>
    public void SetSelected(string ID)
    {
        for (int i = 0; i < listOfSubjectItems.Count; i++)
            listOfSubjectItems[i].SetActive(listOfSubjectItems[i].buttonName == ID);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }

}
