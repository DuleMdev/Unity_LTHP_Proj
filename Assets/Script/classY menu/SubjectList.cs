using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SubjectList : MonoBehaviour {

    RectTransform content;
    RectTransform prefabListItem; // Minden tantárgyhoz létrehozok ebből az elemeből egyet

    List<GameObject> createdListItems = new List<GameObject>();

	// Use this for initialization
	void Awake ()
    {
        content = transform.Find("Viewport/Content").GetComponent<RectTransform>();
        prefabListItem = transform.Find("Viewport/Content/PanelSubjectItem").GetComponent<RectTransform>();

        prefabListItem.gameObject.SetActive(false); // Kikapcsoljuk a prefab elemet
    }

    public void Initialize(List<string> subjectList, List<Sprite> subjectIconList)
    {
        // Az esetleg korábban létrehozott listItem-eket eltávolítjuk
        foreach (GameObject item in createdListItems)
        {
            Destroy(item);
        }

        // Létrehozunk egy új listát
        int maxItemInRow = 6;
        for (int i = 0; i < subjectList.Count; i++)
        {
            // Létrehozzuk az elemet
            GameObject item = Instantiate(prefabListItem.gameObject);

            // Elhelyezzük a content transformon
            item.transform.SetParent(content);

            // Kiszámoljuk az elem pozícioját
            item.transform.position = prefabListItem.position + new Vector3(i % maxItemInRow * prefabListItem.sizeDelta.x, i / maxItemInRow * prefabListItem.sizeDelta.y);

            // Beállítjuk a tartalmát
            (item.GetComponent<SubjectItem>()).Initialize(subjectIconList[i], subjectList[i]);
        }

        // Beállítjuk a content magasságát
        content.sizeDelta = new Vector2(0, ((subjectList.Count + maxItemInRow - 1) / maxItemInRow) * prefabListItem.sizeDelta.y);
    }
}
