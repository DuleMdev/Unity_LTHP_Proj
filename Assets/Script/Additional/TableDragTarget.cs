using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TableDragTarget : DragTarget {

    [Tooltip("A szöveg hány pixelel kezdődjön beljebb a tábla szélétől függőlegesen")]
    public int textMarginY;

    Canvas canvas;

    RectTransform joinPointRectTransform;

    Image tableImage;
    RectTransform tableTransform;

    Image tableShadowImage;
    RectTransform tableShadowTransform;

    Image tableGlowImage;

    BoxCollider2D boxCollider;
    Text text;                              // Szöveg megjelenítéséhez
    TEXDraw texDraw;                        // Matematikai szövegek megjelenítéséhez

    [HideInInspector]
    public Transform tableMove;             // A tábla skálázásához

    float tableSizeDiff;                    // A különbség a szöveg mérete és a tábla képének mérete között
    float tableShadowDiff;                  // A különbség a szöveg mérete és a tábla árnyékának mérete között
    float tableGlowDiff;                    // A különbség a szöveg mérete és a Glow árnyékának mérete között
    float tableColliderDiff;                // A különbség a szöveg mérete és a táblán levő collider mérete között

    public override void Awake()
    {
        base.Awake();

        // Megkeressük a szükséges komponenseket
        canvas = GetComponent<Canvas>();

        joinPointRectTransform = gameObject.SearchChild("JoinPoint").GetComponent<RectTransform>();

        tableImage = Common.SearchGameObject(gameObject, "Picture").GetComponent<Image>();
        tableTransform = tableImage.GetComponent<RectTransform>();
        //tableTransform = Common.SearchGameObject(gameObject, "Picture").GetComponent<RectTransform>();

        tableShadowImage = Common.SearchGameObject(gameObject, "Shadow").GetComponent<Image>();
        tableShadowTransform = tableShadowImage.GetComponent<RectTransform>();
        //tableShadowTransform = Common.SearchGameObject(gameObject, "Shadow").GetComponent<RectTransform>();

        tableGlowImage = Common.SearchGameObject(gameObject, "Glow").GetComponent<Image>();

        boxCollider = Common.SearchGameObject(gameObject, "Picture").GetComponent<BoxCollider2D>();
        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
        texDraw = Common.SearchGameObject(gameObject, "TEXDraw").GetComponent<TEXDraw>();

        tableMove = Common.SearchGameObject(gameObject, "TableMove").transform;

        // Megvizsgáljuk, hogy az aktuális szöveg méret és az egyébb komponensek mérete, hogy viszonyul egymáshoz
        tableSizeDiff = tableTransform.sizeDelta.x - text.preferredWidth;
        tableShadowDiff = tableShadowTransform.sizeDelta.x - text.preferredWidth;
        tableGlowDiff = tableGlowImage.rectTransform.sizeDelta.x - text.preferredWidth;
        tableColliderDiff = boxCollider.size.x - text.preferredWidth;

        // Teszt
        //InitializeTable("Valami Amerika", new List<string> { "izé" } );
    }

    // text a táblán megjelenő szöveg
    // goodItems - Azok az elemek, amelyeket elfogadhat a célpont
    public IEnumerator InitializeTable(string text, List<string> goodItems) {
        Initialize(goodItems);

        RectTransform rectTransformTEXDraw = texDraw.GetComponent<RectTransform>();

        // Beállítjuk a táblán a megadott szöveget
        this.text.text = text;
        texDraw.text = text;
        this.text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

        // Meghatározzuk a szöveg szélességét
        float textWidth = 0;
        if (Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text)
        {
            textWidth = this.text.preferredWidth;
        }
        else
        {
            textWidth = texDraw.preferredWidth;

            /*
            do
            {
                yield return null;
                textWidth = rectTransformTEXDraw.sizeDelta.x; // Ez tartalmazza fogja a text végleges méretét
            } while (textWidth == 0);
            */

            float canvasMaxHeight = tableTransform.sizeDelta.y - textMarginY * 2 + 40;

            rectTransformTEXDraw.sizeDelta = new Vector2(texDraw.preferredWidth + 1, texDraw.preferredHeight + 1);

            if (texDraw.preferredHeight > canvasMaxHeight)
            {
                float ratioY = canvasMaxHeight / texDraw.preferredHeight;
                rectTransformTEXDraw.localScale = Vector3.one * ratioY;
                textWidth *= ratioY;
            }
            /*
            if (rectTransformTEXDraw.sizeDelta.y > canvasMaxHeight)
            {
                float ratioY = canvasMaxHeight / rectTransformTEXDraw.sizeDelta.y;
                rectTransformTEXDraw.localScale = Vector3.one * ratioY;
                textWidth *= ratioY;
            }
            */
        }

        tableTransform.sizeDelta = new Vector2(textWidth + tableSizeDiff, tableTransform.sizeDelta.y);
        tableShadowTransform.sizeDelta = new Vector2(textWidth + tableShadowDiff, tableShadowTransform.sizeDelta.y);
        tableGlowImage.rectTransform.sizeDelta = new Vector2(textWidth + tableGlowDiff, tableGlowImage.rectTransform.sizeDelta.y);
        boxCollider.size = new Vector2(textWidth + tableColliderDiff, boxCollider.size.y);
        //boxCollider.offset = new Vector2(boxCollider.size.x / 2, boxCollider.offset.y);
        yield return null;
    }

    /// <summary>
    /// Beállítja a layoutnak megfelelő képeket a táblán.
    /// </summary>
    /// <param name="table">A tábla képe.</param>
    /// <param name="shadow">A tábla árnyékának képe.</param>
    /// <param name="glow">A tábla körül megjelenő (jó vagy rossz válasz esetén) ragyogás képe.</param>
    public void SetPictures(Sprite table, Color tableColor, Sprite shadow, Color shadowColor, Sprite glow, Color glowColor)
    {
        tableImage.sprite = table;
        tableImage.color = tableColor;

        tableShadowImage.sprite = shadow;
        tableShadowImage.color = shadowColor;

        tableGlowImage.sprite = glow;
        tableGlowImage.color = glowColor;
    }

    void Update() {
        if (!canvas.isRootCanvas && !canvas.overrideSorting)
            canvas.overrideSorting = true;
    }

    public void ChangeTextColor(Color color) {
        text.color = color;
        texDraw.color = color;
    }

    public void ChangeGlowColor(Color color) {
        tableGlowImage.color = color;
    }

    // Vissza adja, hogy a bedobott elemet hova kell a térben elhelyezni
    public override Vector3 GetDropPos(DragItem dragItem)
    {
        // A két táblát nem csak egymás mellé kell tenni, hanem egymásra is kell csúsztatni őket bizonyos mértékben

        //float offset = 0.0453f + 0.03592187f; // * Camera.main.aspect;
        //
        //return transform.position + new Vector3(GetWidth() / 2 + dragItem.GetWidth() - offset, 0);

        return joinPointRectTransform.position.AddX(dragItem.GetWidth());
    }

    // Vissza adja, hogy a bedobott elemet mekkorára kell skálázni
    public override Vector3 GetDropScale(DragItem dragItem)
    {
        return dragItem.BaseTransform.localScale;
    }

    public override float GetWidth()
    {
        return tableTransform.sizeDelta.x * Mathf.Abs(tableTransform.lossyScale.x);
    }
}
