using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPCurriculumListHorizontal : MonoBehaviour
{
    RectTransform content;  // A tananyagokat tartalmazó rectTransform
    OTPCurriculumItem curriculumItem; // Sokszorosítandó elem
    Text textNotCurriculum; // "nincs megjelenítendő tananyag" szöveg kiírásához

    Common.CallBack_In_String buttonClick;

    List<GameObject> listOfCurriculums = new List<GameObject>(); // A létrehozott tananyag sávok, a törlésükhöz kell

    // Use this for initialization
    void Awake()
    {
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        curriculumItem = gameObject.SearchChild("CurriculumItem").GetComponent<OTPCurriculumItem>();
        textNotCurriculum = gameObject.SearchChild("TextNotCurriculum").GetComponent<Text>();
    }

    void Start()
    {
        // Eltüntetjük a másolandó tananyag sávot
        curriculumItem.gameObject.SetActive(false);
    }

    public void Initialize(List<CurriculumItemDriveData> list, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;


        //textNotCurriculum.text = Common.languageController.Translate(C.Texts.NotCurriculum); // A SetLanguageText komponens írja ki
        textNotCurriculum.enabled = (list != null && list.Count == 0);

        // A már létező tananyag elemeket töröljük
        for (int i = 0; i < listOfCurriculums.Count; i++)
        {
            Destroy(listOfCurriculums[i]);
        }
        listOfCurriculums.Clear();

        // Létrehozzuk az új tananyag elemeket
        int listCount = (list != null) ? list.Count : 0;
        int piece = (listCount < 3) ? 3 : listCount;
        piece = listCount; // Ne legyenek üres elemek
        for (int i = 0; i < piece; i++)
        {
            OTPCurriculumItem curriculum = Instantiate(curriculumItem);
            curriculum.gameObject.SetActive(true);
            curriculum.transform.SetParent(content.transform);
            curriculum.transform.localScale = Vector3.one;

            if (i < listCount)
                /*
                curriculum.Initialize(
                    i % 2 == 0,
                    OTPCurriculumItem.SignalType.curriculumCompleted,
                    list[i].name,
                    ((int)list[i].scorePercent).ToString() + "%",
                    Common.languageController.Translate(C.Texts.MadeBy) + list[i].madeBy,
                    list[i].notice,
                    list[i].check,
                    list[i].sync,
                    C.Program.Curriculum + ":" + i, // list[i].id, 
                    ButtonClick,
                    ColorBuilder.GetColor(list[i].name),
                    false
                    );
                    */
                curriculum.Initialize(
                    i % 2 == 0,
                    list[i],
                    C.Program.Curriculum + ":" + i,
                    ButtonClick
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
        content.sizeDelta = new Vector2(curriculumItem.GetComponent<RectTransform>().sizeDelta.x * listCount, content.sizeDelta.y);
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
