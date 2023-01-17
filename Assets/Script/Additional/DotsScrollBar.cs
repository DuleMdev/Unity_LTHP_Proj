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

        // Kit�r�lj�k a kor�bban l�trehozott tartalmakat
        for (int i = 0; i < listOfMadeDots.Count; i++)
            Destroy(listOfMadeDots[i].gameObject);

        listOfMadeDots.Clear();

        // Kisz�moljuk az elemek sz�m�t
        viewPort_height = scrollRect.viewport.rect.height;
        content_height = scrollRect.content.rect.height;

        count = Mathf.CeilToInt(content_height / viewPort_height);

        // L�trehozzuk az elemeket
        for(int i = 0; i < count; i++)
        {
            Transform newDot = Instantiate(dotPrefab, dotPrefab.transform.parent).GetComponent<Transform>();
            newDot.gameObject.SetActive(true);

            listOfMadeDots.Add(newDot);
        }

        // Kisz�moljuk az elemek k�zti t�vols�got
        float dotScrollBar_width = ((RectTransform)transform).rect.width;

        // Kisz�moljuk a pontok k�zti t�vols�got (A Clamp az�rt kell, hogy ne osszunk null�val ha csak egy oldal van)
        float calculatedDistanceBetweenDots = dotScrollBar_width / Mathf.Clamp((count - 1), 1, float.MaxValue);

        // Be�ll�tjuk a kisz�molt �rt�kre a horizontal layout-ot
        horizontalLayoutGroup.spacing = Mathf.Min(distanceBetweenDots, calculatedDistanceBetweenDots);
    }

    // Update is called once per frame
    void Update()
    {
        // Ha v�ltoz�s t�rt�nt �jra l�trehozzuk a pontokat
        if (lastDistanceBetweenDots != distanceBetweenDots ||
            viewPort_height != scrollRect.viewport.rect.height ||
            content_height != scrollRect.content.rect.height)
        {
            MakeItems();
        }

        // Be�ll�tjuk a Marker poz�ci�j�t
        if (count > 0)
        {
            int pos = (int)Mathf.Clamp(count - scrollRect.verticalNormalizedPosition * count, 0, count - 1);
            marker.position = listOfMadeDots[pos].position;
        }

        marker.gameObject.SetActive(count > 0);
    }
}
