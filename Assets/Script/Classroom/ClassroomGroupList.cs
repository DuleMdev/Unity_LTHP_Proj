using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassroomGroupList : MonoBehaviour
{

    RectTransform content; // Mibe kell tenni a létrehozott elemeket
    ClassroomGroupListItem listItem; // A sokszorosítandó elem
    //RectTransform rectTransformListItem; //  A sokszorosítandó elem pozíciójának lekérdezéséhez

    bool multiSelect;

    List<ClassroomGroupListItem> listOfItems = new List<ClassroomGroupListItem>(); // A létrehozott lista elemek, a törlésükhöz kell többek között
    public bool[] selecteditems;

    void Awake()
    {
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();
        listItem = gameObject.SearchChild("ListItem").GetComponent<ClassroomGroupListItem>();
    }

    // Use this for initialization
    void Start()
    {
        listItem.gameObject.SetActive(false);
    }

    public void Initialize(string[] list, bool multiSelect = true)
    {
        this.multiSelect = multiSelect;

        // Létrehozzuk a kiválasztottságot tartalmazó tömböt
        selecteditems = new bool[list.Length];

        // Kitöröljük a korábbi elemeket a listából
        for (int i = 0; i < listOfItems.Count; i++)
        {
            Destroy(listOfItems[i].gameObject);
        }
        listOfItems.Clear();

        // Létrehozzuk az új elemeket
        for (int i = 0; i < list.Length; i++)
        {
            ClassroomGroupListItem newItem = Instantiate(listItem, content.transform);
            newItem.gameObject.SetActive(true);

            newItem.Initialize(i, list[i], ButtonClick);

            // Beállítjuk a létrehozott elem pozícióját
            RectTransform rectTransform = newItem.GetComponent<RectTransform>();
            rectTransform.localPosition = rectTransform.localPosition.SetY(-i * rectTransform.sizeDelta.y);

            // Beállítjuk a content méretét, elég egyszer
            if (i == 0)
                content.sizeDelta = new Vector2(content.sizeDelta.x, list.Length * rectTransform.sizeDelta.y);

            // Tároljuk a létrehozott lista elemet
            listOfItems.Add(newItem);
        }
    }

    public void ButtonClick(int listItemIndex)
    {
        if (multiSelect)
        {
            // Többet is ki lehet választani
            selecteditems[listItemIndex] = !selecteditems[listItemIndex];
            listOfItems[listItemIndex].SetSelected(selecteditems[listItemIndex]);
        }
        else
        {
            // Csak egyet lehet kiválasztani
            // A korábbi kiválasztottakat töröljük
            for (int i = 0; i < listOfItems.Count; i++)
            {
                listOfItems[i].SetSelected(false);
                selecteditems[i] = false;
            }

            // Bejelöljük amit most választottak ki
            selecteditems[listItemIndex] = true;
            listOfItems[listItemIndex].SetSelected(true);
        }
    }
}
