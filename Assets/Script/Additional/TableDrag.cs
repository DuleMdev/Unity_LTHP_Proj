using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TableDrag : DragItem
{

    [Tooltip("A szöveg hány pixelel kezdődjön beljebb a tábla szélétől függőlegesen")]
    public int textMarginY;

    [Tooltip("A kéz mozgásának amplitudója")]
    public float amplitude;
    [Tooltip("A kéz mozgásának minimális sebessége")]
    public float moveSpeedMin;
    [Tooltip("A kéz mozgásának maximális sebessége")]
    public float moveSpeedMax;

    SpriteRenderer armImage;
    SpriteRenderer armShadowImage;

    Image tableImage;
    RectTransform tableTransform;

    Image tableShadowImage;
    RectTransform tableShadowTransform;

    Image tableGlowImage;

    BoxCollider2D boxCollider;
    Text text;                              // Szöveg megjelenítéséhez
    TEXDraw texDraw;                        // Matematikai szövegek megjelenítéséhez

    [HideInInspector]
    public Transform armMove;               // A kar beúszását és kiúszását végrehajtó komponens
    Transform armAnimMove;                  // A kar imbolygását végrehajtó transform komponens

    float tableSizeDiff;                    // A különbség a szöveg mérete és a tábla képének mérete között
    float tableShadowDiff;                  // A különbség a szöveg mérete és a tábla árnyékának mérete között
    float tableGlowDiff;                    // A különbség a szöveg mérete és a tábla glow képének mérete között
    float tableColliderDiff;                // A különbség a szöveg mérete és a táblán levő collider mérete között

    float wobbleTime;                       // Az imbolygás frekvenciája
    float aktWobble;                        // Az imbolygás aktuális értéke

    Color flashingColor;                    // Milyen szinnel villogjon a szöveg a FlashingCoroutine eljárásban

    public override void Awake()
    {
        base.Awake();

        // Megkeressük a szükséges komponenseket
        armImage = Common.SearchGameObject(gameObject, "ArmAnimMove").GetComponent<SpriteRenderer>();
        armShadowImage = Common.SearchGameObject(gameObject, "ArmShadow").GetComponent<SpriteRenderer>();

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

        armMove = Common.SearchGameObject(gameObject, "ArmMove").transform;
        armAnimMove = Common.SearchGameObject(gameObject, "ArmAnimMove").transform;

        //GameObject go = Common.FindGameObject(gameObject, "TableBase");
        //BaseTransform = go.transform;
        BaseTransform = Common.SearchGameObject(gameObject, "TableBase").transform;
        MoveTransform = Common.SearchGameObject(gameObject, "TableMove").transform;

        // Megvizsgáljuk, hogy az aktuális szöveg méret és az egyébb komponensek mérete, hogy viszonyulnak egymáshoz
        tableSizeDiff = tableTransform.sizeDelta.x - text.preferredWidth;
        tableShadowDiff = tableShadowTransform.sizeDelta.x - text.preferredWidth;
        tableGlowDiff = tableGlowImage.rectTransform.sizeDelta.x - text.preferredWidth;
        tableColliderDiff = boxCollider.size.x - text.preferredWidth;

        // Kitaláljuk az imbolygásnak a paramétereit
        wobbleTime = (float)(Common.random.NextDouble() * (moveSpeedMax - moveSpeedMin) - moveSpeedMax);
    }

    // text a táblán megjelenő szöveg
    // goodItems - Azok az elemek, amelyeket elfogadhat a célpont
    public IEnumerator InitializeTable(string text, string text2)
    {
        itemName = text;

        if (text2 != null)
            text = text2;

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

                Debug.LogWarning(" Módosítva " + textWidth);
            }
            */

            yield return null;
        }

        tableTransform.sizeDelta = new Vector2(textWidth + tableSizeDiff, tableTransform.sizeDelta.y);
        tableShadowTransform.sizeDelta = new Vector2(textWidth + tableShadowDiff, tableShadowTransform.sizeDelta.y);
        tableGlowImage.rectTransform.sizeDelta = new Vector2(textWidth + tableGlowDiff, tableGlowImage.rectTransform.sizeDelta.y);
        boxCollider.size = new Vector2(textWidth + tableColliderDiff, boxCollider.size.y);
        boxCollider.offset = new Vector2(boxCollider.size.x / -2, boxCollider.offset.y);
    }

    /// <summary>
    /// Beállítja a layoutnak megfelelő képeket a táblán.
    /// </summary>
    /// <param name="table">A tábla képe.</param>
    /// <param name="shadow">A tábla árnyékának képe.</param>
    /// <param name="glow">A tábla körül megjelenő (jó vagy rossz válasz esetén) ragyogás képe.</param>
    public void SetPictures(Sprite table, Color tableColor, Sprite shadow, Color shadowColor, Sprite glow, Color glowColor, Sprite arm, Color armColor, Sprite armShadow, Color armShadowColor) {
        tableImage.sprite = table;
        tableImage.color = tableColor;

        tableShadowImage.sprite = shadow;
        tableShadowImage.color = shadowColor;

        tableGlowImage.sprite = glow;
        tableGlowImage.color = glowColor;

        armImage.sprite = arm;
        armImage.color = armColor;

        armShadowImage.sprite = armShadow;
        armShadowImage.color = armShadowColor;
    }

    /// <summary>
    /// Beállítja a táblán megjelenő szöveg szinét.
    /// </summary>
    /// <param name="color">A szöveg színe.</param>
    public void SetTextColor(Color color) {
        text.color = color;
        texDraw.color = color;
    }

    void Update() {
        // kiszámoljuk az aktuális lebegés értéket
        base.Update();

        if (!grabbed && Common.configurationController.playAnimation) { // Ha nincs megfogva az elem, akkor mozgatjuk a macska kezét
            aktWobble += Time.deltaTime / wobbleTime * Mathf.PI * 2;
            float xPos = Mathf.Sin(aktWobble);
            armAnimMove.transform.localPosition = new Vector2(xPos * amplitude + amplitude, armAnimMove.transform.localPosition.y);
        }
    }

    // Beállítjuk a rétegsorrendjét az elemnek
    public override void SetOrderInLayer(int order)
    {
        BaseTransform.GetComponent<Canvas>().sortingOrder = order;
    }

    // Vissza adja, hogy a macska kezét hova kell pozícionálni, hogy ne látszódjon a képen
    // Ezt a pozíciót a macska kezében levő tábla széllességének figyelembe vételével kell kiszámolni
    public Vector3 GetGlobalHideArmMovePos() {
        /*

        1. táblaszélesség   - GetWidth()
        2. a kar pozíciója a képernyő szélétől:  Camera.main.aspect - transform.position.x
        3. a kar imbolygásának aktuális mértéke: armAnimMove.position.x - armMove.position.x
        4. a tábla pozíciója a kartól           :  

    */

        return new Vector3(Camera.main.aspect + transform.position.x - BaseTransform.position.x + GetWidth() + amplitude * 2, armMove.position.y);
    }

    public void HideArm()
    {
        iTween.MoveTo(armMove.gameObject, iTween.Hash("position", GetGlobalHideArmMovePos(), "time", animSpeed, "easeType", iTween.EaseType.easeInCirc));
    }

    // vissza adja, hogy melyik pontot kell figyelni a célpontba dobásnál
    // Nem biztos, hogy a megfogott elem root GameObject pozíciója határozza meg az elem helyét. lásd Toldalékos játéknál például a tábla bal szélének csúcsa.
    public override Vector3 GetDropPos()
    {
        return MoveTransform.position - new Vector3(GetWidth(), 0);
    }

    // Villogtatja az elemet
    public override void FlashingPositive()
    {
        flashingColor = Color.green;
        base.FlashingPositive();

        HideArm();
    }

    public override void FlashingNegative()
    {
        flashingColor = Color.red;
        base.FlashingNegative();
    }

    public override IEnumerator FlashingCoroutine()
    {
        Color textOriginalColor = text.color;
        Color glowColor;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);
            glowColor = flashingColor * 0.8f;
            SetTextColor(flashingColor);
            //text.color = flashingColor;
            //texDraw.color = flashingColor;
            tableGlowImage.color = glowColor;
            // Ha adott a célpont, akkor azt is villogtatjuk, (igazából mindig adottnak kellene lennie)
            if (dragTarget != null)
            {
                ((TableDragTarget)dragTarget).ChangeTextColor(flashingColor);
                ((TableDragTarget)dragTarget).ChangeGlowColor(glowColor);
            }

            yield return new WaitForSeconds(0.2f);

            SetTextColor(textOriginalColor);
            //text.color = textOriginalColor;
            //texDraw.color = textOriginalColor;
            tableGlowImage.color = Color.white * 0f;
            // Ha adott a célpont, akkor azt is visszaállítjuk, (igazából mindig adottnak kellene lennie)
            if (dragTarget != null)
            {
                ((TableDragTarget)dragTarget).ChangeTextColor(textOriginalColor);
                ((TableDragTarget)dragTarget).ChangeGlowColor(Color.white * 0);
            }
        }

        animRun = false;
    }

    public override float GetWidth()
    {
        return tableTransform.sizeDelta.x * Mathf.Abs(tableTransform.lossyScale.x);
    }

}
