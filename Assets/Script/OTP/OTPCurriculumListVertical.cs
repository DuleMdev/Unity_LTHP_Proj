using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OTPCurriculumListVertical : MonoBehaviour
{
    RectTransform content;  // A tananyagokat tartalmazó rectTransform
    OTPCurriculumItem curriculumItem;   // Sokszorosítandó elem
    RectTransform rectTransformCurriculumItem; // Sokszorosítandó elem pozíciójának lekérdezéséhez
    Text textNotCurriculum; // "nincs megjelenítendő tananyag" szöveg kiírásához

    Common.CallBack_In_String buttonClick;

    List<GameObject> listOfCurriculums = new List<GameObject>(); // A létrehozott tananyag sávok, a törlésükhöz kell

    // Use this for initialization
    void Awake()
    {
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        curriculumItem = gameObject.SearchChild("CurriculumItem").GetComponent<OTPCurriculumItem>();
        rectTransformCurriculumItem = curriculumItem.GetComponent<RectTransform>();
        textNotCurriculum = gameObject.SearchChild("TextNotCurriculum").GetComponent<Text>();
    }

    void Start()
    {
        // Eltüntetjük a másolandó tananyag sávot
        curriculumItem.gameObject.SetActive(false);
    }

    public void Initialize(List<CurriculumItemDriveData> list, string buttonNamePrefix, Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;

        textNotCurriculum.text = Common.languageController.Translate(C.Texts.NotCurriculum);
        textNotCurriculum.enabled = (list == null || list.Count == 0);

        // A már létező tananyag elemeket töröljük
        for (int i = 0; i < listOfCurriculums.Count; i++)
        {
            Destroy(listOfCurriculums[i]);
        }
        listOfCurriculums.Clear();

        // Létrehozzuk az új tananyag elemeket
        int listCount = (list != null) ? list.Count : 0;
        int piece = (listCount < 10) ? 10 : listCount;
        piece = listCount; // Ne legyenek üres elemek
        for (int i = 0; i < piece; i++)
        {
            OTPCurriculumItem curriculum = Instantiate(curriculumItem, content.transform);
            curriculum.gameObject.SetActive(true);

            bool backgroundShouldBeDark = i % 4 == 0 || i % 4 == 3; // (((i + 1) % 4) & 2) > 0;

            if (i < listCount)
                /*
                curriculum.Initialize(
                    backgroundShouldBeDark,
                    OTPCurriculumItem.SignalType.curriculumCompleted,
                    list[i].name,
                    ((int)list[i].scorePercent).ToString() + "%",
                    Common.languageController.Translate(C.Texts.MadeBy) + list[i].madeBy,
                    list[i].notice,
                    list[i].check,
                    list[i].sync,
                    buttonNamePrefix + ":" + list[i].curriculumID, 
                    ButtonClick,
                    ColorBuilder.GetColor(list[i].name),
                    false
                );
                */
                curriculum.Initialize(
                    backgroundShouldBeDark, // i % 2 == 0,
                    list[i],
                    buttonNamePrefix + ":" + list[i].curriculumID,
                    ButtonClick
                );
            else
                curriculum.Empty(backgroundShouldBeDark);

            RectTransform recTranform = curriculum.GetComponent<RectTransform>();
            recTranform.localPosition = rectTransformCurriculumItem.localPosition.SetX(content.rect.width / 2 - rectTransformCurriculumItem.sizeDelta.x + (i % 2) * rectTransformCurriculumItem.sizeDelta.x).AddY((i / 2) * -rectTransformCurriculumItem.sizeDelta.y);
            //recTranform.sizeDelta = new Vector2(recTranform.sizeDelta.x, 0);
            //recTranform.anchoredPosition = new Vector2(recTranform.anchoredPosition.x, 0);

            listOfCurriculums.Add(curriculum.gameObject);
        }

        // Beállítjuk a tartalom méretét

        //content.sizeDelta = new Vector2(curriculumItem.GetComponent<RectTransform>().sizeDelta.x * listCount, content.sizeDelta.y);

        content.sizeDelta = new Vector2(content.sizeDelta.x, curriculumItem.GetComponent<RectTransform>().sizeDelta.y * ((listCount + 1) / 2));
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
