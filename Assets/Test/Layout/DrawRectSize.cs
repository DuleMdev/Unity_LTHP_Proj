using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawRectSize : MonoBehaviour
{
    RectTransform parent;
    Text text;

    LayoutElement layoutElement;

    // Start is called before the first frame update
    void Awake()
    {
        parent = transform.parent.GetComponent<RectTransform>();
        text = GetComponent<Text>();

        layoutElement = parent.GetComponent<LayoutElement>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = parent.sizeDelta.x + "\n" +
            "\n min : " + layoutElement.minWidth +
            "\n pref : " + layoutElement.preferredWidth +
            "\n flex : " + layoutElement.flexibleWidth;
    }
}
