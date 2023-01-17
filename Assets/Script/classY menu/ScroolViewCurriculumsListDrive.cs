using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScroolViewCurriculumsListDrive : MonoBehaviour
{
    RectTransform content;  // A tananyagokat tartalmazó rectTransform
    ClassYCurriculumListItem curriculumListItem;

    Common.CallBack_In_String buttonClick;

    List<GameObject> listOfCurriculums = new List<GameObject>(); // A létrehozott tananyag sávok, a törlésükhöz kell

    // Use this for initialization
    void Awake () {
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        curriculumListItem = gameObject.SearchChild("CurriculumItemDrive").GetComponent<ClassYCurriculumListItem>();
    }

    void Start() {
        // Eltüntetjük a másolandó tananyag sávot
        curriculumListItem.gameObject.SetActive(false);
    }

    public void Initialize(List<CurriculumItemDriveData> list, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        // A már létező menü elemeket töröljük
        for (int i = 0; i < listOfCurriculums.Count; i++)
        {
            Destroy(listOfCurriculums[i]);
        }
        listOfCurriculums.Clear();

        // Létrehozzuk az új menü elemeket
        int listCount = (list != null) ? list.Count : 0;
        int piece = (listCount < 3) ? 3 : listCount;
        for (int i = 0; i < piece; i++)
        {
            ClassYCurriculumListItem curriculum = Instantiate(curriculumListItem);
            curriculum.gameObject.SetActive(true);
            curriculum.transform.SetParent(content.transform);
            curriculum.transform.localScale = Vector3.one;

            if (i < listCount)
                curriculum.Initialize(
                    i % 2 == 0,
                    list[i].name,
                    Common.languageController.Translate(C.Texts.MadeBy) + list[i].madeBy,
                    list[i].notice,
                    list[i].check,
                    list[i].sync,
                    C.Program.Curriculum + ":" + i, // list[i].id, 
                    ButtonClick,
                    ColorBuilder.GetColor(list[i].name),
                    false
                    );
            else
                curriculum.Empty(i % 2 == 0);

            RectTransform recTranform = curriculum.GetComponent<RectTransform>();
            recTranform.localPosition = recTranform.localPosition.SetX(recTranform.sizeDelta.x * i);
            recTranform.sizeDelta = new Vector2(recTranform.sizeDelta.x, 0);
            recTranform.anchoredPosition = new Vector2(recTranform.anchoredPosition.x, 0);

            listOfCurriculums.Add(curriculum.gameObject);
        }

        // Beállítjuk a tartalom méretét
        content.sizeDelta = new Vector2(curriculumListItem.GetComponent<RectTransform>().sizeDelta.x * listCount, content.sizeDelta.y);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
