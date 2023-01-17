using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ez az objektum beállítja a megadott RectTranform szélességét a text objektumnak megfelelően.
/// Csak akkor működik megfelelően, ha a RectTransform mérete a width és height vagyis a sizeDelta határozza meg.
/// Ha viszont a szülő transform méretéhez képest van meghatározva a mérete, akkor nem használható.
/// 
/// A tolerancia az egy érdekes jószág.
/// Annyival nagyobb lehet a méret amitt ott megadtunk. Tehát ha a tolerancia = 50 a maxWidth = 800, akkor a méret
/// 850 is lehet. Viszont ha már a méret nagyobb mint 850, akkor a méret 800 lesz.
/// Ez azért van, hogy egy pixel kilógás miatt ne csináljunk drámát.
/// 
/// Viszont ha nagyobb akkor van lehetőség egy gameObject bekapcsolására is.
/// Ez igazán akkor jó, ha mondjuk a szövegnél ha hosszú, akkor meg akarunk jeleníteni egy ... szöveget.
/// Ezt a ... szöveget egy másik gameObject-en hozzuk létre egy másik Text komponensben, amit rá tehetünk a fő szövegre
/// amit majd eltakar.
/// 
/// 
/// 
/// </summary>
public class TextParentWidthSetter : MonoBehaviour
{
    [Tooltip("A text komponens által állítandő rectTransform. Ha nem adjuk meg, akkor a text komponenset tartalmazó gameObject rectTransformja lesz állítva")]
    public RectTransform rectTransform;
    [Tooltip("Mennyi legyen minimum a rectTransform szélessége")]
    public float minWidth = float.MinValue;
    [Tooltip("Mennyi lehet maximum a rectTransform szélessége")]
    public float maxWidth = float.MaxValue;
    [Tooltip("Mennyi lehet a maximum tolerancia")]
    public float maxTolerancia = 0;
    [Tooltip("Ha korlátozni kellett a szélességet a maxWidth miatt, akkor ezt a RectTransformot bekapcsoljuk")]
    public GameObject gameObjectIfTooWidth;

    //[Tooltip("Ha engedélyezve van, akkor minden Update-ben újra kalkulálja a méretet")]
    //public bool updateEnabled;

    Text text;

    float different;    // A text komponens és a rectTransform szélességének differenciája kezdetben

    float previousPreferedWidth;

    // Use this for initialization
    void Awake()
    {
        // Ha nincs megadva rectTransform, akkor a text komponenset tartalmazó gameObject rectTransformját fogja állítani.
        if (!rectTransform)
            rectTransform = gameObject.GetComponent<RectTransform>();
        text = GetComponent<Text>();

        // Kiszámoljuk, hogy kezdetben mennyi a differencia
        different = rectTransform.sizeDelta.x - text.preferredWidth;
    }

    //public void SetText(string text)
    //{
    //    this.text.text = text;
    //    CalculateWidth();
    //}

    /// <summary>
    /// Kiszámolja a szükséges szélességet.
    /// </summary>
    /// <returns>Igaz értéket ad vissza ha változott a szélesség.</returns>
    bool CalculateWidth()
    {

        //if (text.transform.parent.gameObject.name == "Mask")
        //    Debug.Log("Mask");

        if (previousPreferedWidth != text.preferredWidth)
        {
            rectTransform.sizeDelta = new Vector2(Mathf.Clamp(text.preferredWidth + different, minWidth, text.preferredWidth + different <= maxWidth + maxTolerancia ? maxWidth + maxTolerancia : maxWidth), rectTransform.sizeDelta.y);
            previousPreferedWidth = text.preferredWidth;
             
            if (gameObjectIfTooWidth != null)
                gameObjectIfTooWidth.SetActive(text.preferredWidth + different > maxWidth + maxTolerancia);

            return true;
        }

        return false;
    }

    void Update()
    {
        // if volt változás a szélességben, akkor újra számoljuk az elhelyezkedést (HorizontalLayoutGroup komponens miatt kell)
        if (CalculateWidth())
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }
}
