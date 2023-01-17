using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Beállítja a Text componens aligment tulajdonságát.
Ha a Text compoensbe levő szöveg hosszabb mint a Text komponens szélessége, tehát nem férne bele, akkor 
Balra fogja igazítani, A maradék ki fog lógni, de egy Mask komponensel levághatjuk.
Ha viszotn a szöveg kisebb mint a text komponens szélessége, akkor középre lesz igazítva a szöveg.
*/

public class UITextAligmentChange : MonoBehaviour
{
    Text text;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 v = Common.GetRectTransformPixelSize(GetComponent<RectTransform>(), true);

        if (text.preferredWidth > v.x)
            text.alignment = TextAnchor.UpperLeft;
        else
            text.alignment = TextAnchor.UpperCenter;
    }
}
