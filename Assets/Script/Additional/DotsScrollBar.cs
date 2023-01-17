using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DotsScrollBar : MonoBehaviour
{
    public float distanceBetweenDots;

    ScrollRect scrollRect;

    GameObject dotPrefab;
    Transform marker;
    HorizontalLayoutGroup horizontalLayoutGroup;

    List<Transform> listOfMadeDots = new List<Transform>();

    float lastDistanceBetweenDots;
    float viewPort_height;
    float content_height;
    int count;


    // Start is called before the first frame update
    void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponentInParent<ScrollRect>(true);

        dotPrefab = gameObject.SearchChild("DotPrefab").gameObject;
        marker = gameObject.SearchChild("Marker").GetComponent<Transform>();
        horizontalLayoutGroup = gameObject.SearchChild("Dots").GetComponent<HorizontalLayoutGroup>();
    }

    void MakeItems()
    {
        lastDistanceBetweenDots = distanceBetweenDots;

        // Kitöröljük a korábban létrehozott tartalmakat
        for (int i = 0; i < listOfMadeDots.Count; i++)
            Destroy(listOfMadeDots[i].gameObject);

        listOfMadeDots.Clear();

        // Kiszámoljuk az elemek számát
        viewPort_height = scrollRect.viewport.rect.height;
        content_height = scrollRect.content.rect.height;

        count = Mathf.CeilToInt(content_height / viewPort_height);

        // Létrehozzuk az elemeket
        for(int i = 0; i < count; i++)
        {
            Transform newDot = Instantiate(dotPrefab, dotPrefab.transform.parent).GetComponent<Transform>();
            newDot.gameObject.SetActive(true);

            listOfMadeDots.Add(newDot);
        }

        // Kiszámoljuk az elemek közti távolságot
        float dotScrollBar_width = ((RectTransform)transform).rect.width;

        // Kiszámoljuk a pontok közti távolságot (A Clamp azért kell, hogy ne osszunk nullával ha csak egy oldal van)
        float calculatedDistanceBetweenDots = dotScrollBar_width / Mathf.Clamp((count - 1), 1, float.MaxValue);

        // Beállítjuk a kiszámolt értékre a horizontal layout-ot
        horizontalLayoutGroup.spacing = Mathf.Min(distanceBetweenDots, calculatedDistanceBetweenDots);
    }

    // Update is called once per frame
    void Update()
    {
        // Ha változás történt újra létrehozzuk a pontokat
        if (lastDistanceBetweenDots != distanceBetweenDots ||
            viewPort_height != scrollRect.viewport.rect.height ||
            content_height != scrollRect.content.rect.height)
        {
            MakeItems();
        }

        // Beállítjuk a Marker pozícióját
        if (count > 0)
        {
            int pos = (int)Mathf.Clamp(count - scrollRect.verticalNormalizedPosition * count, 0, count - 1);
            marker.position = listOfMadeDots[pos].position;
        }

        marker.gameObject.SetActive(count > 0);
    }
}
