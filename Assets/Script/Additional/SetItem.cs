using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
Ez az objektum tárolja a halmazos játékban egy darab kép elemet

Az initialize metódusban beállíthatjuk a képet és a keret színét, illetve a elem nevét
Az utóbbi akkor fontos, amikor valamelyik 


*/

public class SetItem : DragItem {

    enum ItemType
    {
        Picture,    // Kép típusú halmaz elem
        Text,       // Szöveg típusú halmaz elem
    }


    [Tooltip("A szöveg hány pixelel kezdődjön beljebb a buborék szélétől vízszintesen")]
    public int textMarginX;
    [Tooltip("A szöveg hány pixelel kezdődjön beljebb a buborék szélétől függőlegesen")]
    public int textMarginY;

    [Tooltip("Mennyi lehet egy szöveg maximális szélessége")]
    public int textMaxWidth;
    //public string itemName { private set; get; }

    [HideInInspector]
    //public bool enabledGrab;

    Transform move;                         // Ezt mozgatjuk míg a bázis változatlan marad, így tudjuk hova kell visszarakni az elemet

    RectTransform rectTransformPictureCanvas;     // Ezen az elemen vannak a képek
    Image picture;                          // A megjelenítendő kép
    public Image border;                    // A kép kerete
    public Image borderShadow;              // A kép keretének árnyéka

    RectTransform rectTransformTextCanvas;  // Ezen az elemen van a szöveg
    public Image textBubble;                // Szöveg háttere ami egy szöveg buborék
    public Image textBubbleShadow;          // Szöveg buborék árnyéka
    Text text;                              // Szöveg megjelenítéséhez
    TEXDraw texDraw;                        // Szöveg megjelenítéséhez

    ItemType itemType;
    /// <summary>
    /// Képet tartalmaz a halmaz elem?
    /// </summary>
    public bool isPicture { get { return itemType == ItemType.Picture; } }

    Color textBubbleColor;
    Color borderColor;

    Color flashingColor;                    // Milyen szinnel történjen a villogás a FlashingCoroutine eljárásban


    public Vector3 GetMovePos { get { return move.position; } } // Vissza adja, hogy éppen hol van a move transform elem 



	// Use this for initialization
	public override void Awake () {
        base.Awake();

        move = Common.SearchGameObject(gameObject, "move").transform;

        rectTransformPictureCanvas = Common.SearchGameObject(gameObject, "PictureCanvas").GetComponent<RectTransform>();
        picture = Common.SearchGameObject(gameObject, "Picture").GetComponent<Image>();
        border = Common.SearchGameObject(gameObject, "Border").GetComponent<Image>();
        borderShadow = Common.SearchGameObject(gameObject, "BorderShadow").GetComponent<Image>();

        rectTransformTextCanvas = Common.SearchGameObject(gameObject, "TextCanvas").GetComponent<RectTransform>();
        textBubble = Common.SearchGameObject(gameObject, "TextBubble").GetComponent<Image>();
        textBubbleShadow = Common.SearchGameObject(gameObject, "TextBubbleShadow").GetComponent<Image>();
        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
        texDraw = Common.SearchGameObject(gameObject, "TEXDraw").GetComponent<TEXDraw>();

        BaseTransform = transform;
        MoveTransform = move; //   Common.SearchGameObject(gameObject, "FishMove").transform;
    }

    public void Initialize(TaskSetsData task, string itemName, Color color)
    {
        StartCoroutine(InitializeCoroutine(task, itemName, color));
    }

    // Beállítja a halmaz elemen megjelenő képet és a keret színét
    // A itemName a halmaz elem neve, amivel be tudjuk majd azonosítani
    public IEnumerator InitializeCoroutine(TaskSetsData task, string itemName, Color color)
    {
        this.itemName = itemName;
        textBubbleColor = textBubble.color; // color;
        //borderColor = border.color; // color;

        if (Common.configurationController.isServer2020)
        {
            TaskSetsData.Set.ItemData itemData = task.GetItemDataFromAnswerID(itemName);
            itemName = (itemData.isImage ? "#" : "") + itemData.answer;
        }

        if (itemName[0] == '#') {
            // A halmaz elem kép típusú lesz
            itemType = ItemType.Picture;
            itemRenderer = picture.GetComponent<Renderer>();

            Sprite sprite = task.gameData.GetSprite(itemName.Substring(1));



            // Később át lehetne térni sima eljárásra

            // yield return null;

            //yield return Common.pictureController.LoadSpriteFromFileSystemCoroutine(itemName.Substring(1));
            //Sprite sprite = Common.pictureController.resultSprite;

            if (sprite != null) {
                // Ha sikeres volt a kép beolvasás
                picture.sprite = sprite;
                //border.color = color;

                // Méretezzük a Canvast, hogy a kép méretarányos legyen
                float ratio = picture.sprite.texture.width * 1.0f / picture.sprite.texture.height;
                rectTransformPictureCanvas.sizeDelta = new Vector2(rectTransformPictureCanvas.sizeDelta.y * ratio, rectTransformPictureCanvas.sizeDelta.y);

                rectTransformPictureCanvas.GetComponent<BoxCollider2D>().size = rectTransformPictureCanvas.sizeDelta; // Beállítjuk a BoxCollider2D-t a canvas méretére
            }
            else {
                // Sikertelen volt a képbeolvasás
                itemName = itemName.Substring(1);
            }
        }

        // Ha nem képről van szó, vagy előzőleg nem sikerült a kép beolvasás, akkor az első karaktere nem kettős kereszt
        if (itemName[0] != '#') {
            // A halmaz elem szöveg típusú lesz
            itemType = ItemType.Text;
            itemRenderer = textBubble.GetComponent<Renderer>();

            //textBubble.color = color;
            texDraw.text = itemName;
            text.text = itemName;
            text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
            texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

            // TEXDraw komponessel méretezés
            // Méretezzük a Canvast, hogy a szöveg jól kitöltse
            // A margó méretével csökkentjük a maximális lehetséges méreteket
            float canvasMaxHeight = rectTransformPictureCanvas.sizeDelta.y - textMarginY * 2;
            float canvasMaxWidth = textMaxWidth - textMarginX * 2;

            RectTransform textRectTransform = texDraw.GetComponent<RectTransform>();
            // Várunk amíg lesz mérete a TEXDraw komponensnek

            Vector2 textSizeDelta;

            if (Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text)
            {
                textSizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
            }
            else
            {
                textSizeDelta = new Vector2(texDraw.preferredWidth + 1, texDraw.preferredHeight + 1);
                textRectTransform.sizeDelta = textSizeDelta;
                /*
                do
                {
                    yield return null;
                    textSizeDelta = textRectTransform.sizeDelta; // Ez tartalmazza fogja a text végleges méretét
                } while (textSizeDelta.x == 0);
                */
            }

            float xRatio = textSizeDelta.x / canvasMaxWidth;
            float yRatio = textSizeDelta.y / canvasMaxHeight;

            float maxRatio = Mathf.Max(xRatio, yRatio);

            // Ha a maxRatio nagyobb mint 1, akkor zsugorítani kell a text-et
            if (maxRatio > 1)
            {
                if (Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text)
                    textRectTransform = text.GetComponent<RectTransform>();

                textRectTransform.localScale = textRectTransform.localScale / maxRatio;

                //textRectTransform.sizeDelta = textRectTransform.sizeDelta / maxRatio;
                textSizeDelta = textSizeDelta / maxRatio;
            }

            textSizeDelta.x += textMarginX * 2;
            textSizeDelta.y += textMarginY * 2;

            // Ha túl kicsi lenne a szöveg, akkor beállítjuk a minimum méretet
            if (textSizeDelta.y < rectTransformTextCanvas.sizeDelta.y)
                textSizeDelta.y = rectTransformTextCanvas.sizeDelta.y;
            // A szöveg szélessége nem lehet kisebb a szöveg magasságánál (Így legalább egy kockát fogunk kapni, vagy egy laposabb valamit)
            if (textSizeDelta.x < rectTransformTextCanvas.sizeDelta.y)
                textSizeDelta.x = rectTransformTextCanvas.sizeDelta.y;

            rectTransformTextCanvas.sizeDelta = textSizeDelta;
            rectTransformTextCanvas.GetComponent<BoxCollider2D>().size = textSizeDelta; // Beállítjuk a BoxCollider2D-t a canvas méretére

            // *** Sima text méretezése
            // Méretezzük a Canvast, hogy a szöveg jól kitöltse
            /*
            float textWidth = text.preferredWidth * text.transform.localScale.x + textMargin * 2;
            if (textWidth < textRectTransform.sizeDelta.y)
                textWidth = textRectTransform.sizeDelta.y;
            
            Vector2 canvasSize = new Vector2(textWidth, textRectTransform.sizeDelta.y);
            
            textRectTransform.sizeDelta = canvasSize;
            textRectTransform.GetComponent<BoxCollider2D>().size = canvasSize; // Beállítjuk a BoxCollider2D-t a canvas méretére
            */



            //textRectTransform.sizeDelta = new Vector2(textWidth, textRectTransform.sizeDelta.y);
            //textRectTransform.GetComponent<BoxCollider2D>().size = Vector2()
            //textRectTransform.sizeDelta = new Vector2(text.preferredWidth * text.transform.localScale.x + textMargin * 2, textRectTransform.sizeDelta.y);
        }

        // A típusnak megfelelő canvast bekapcsoljuk a másikat kikapcsoljuk
        rectTransformPictureCanvas.gameObject.SetActive(itemType == ItemType.Picture);
        rectTransformTextCanvas.gameObject.SetActive(itemType == ItemType.Text);

        yield return null;
    }

    // Beállítjuk a rétegsorrendjét a halmaz elemnek
    override public void SetOrderInLayer(int order) {
        rectTransformPictureCanvas.GetComponent<Canvas>().sortingOrder = order;
        rectTransformTextCanvas.GetComponent<Canvas>().sortingOrder = order;
    }

    /*
    // Beállítja az elem bázis pozícióját
    // newPos       - A halmaz elem új pozíciója
    // delay        - mennyit várakozzon míg elindul az új pozícióba
    // Igaz értéket ad vissza ha a pozíció egy új pozíció, hamisat ha már a megadott pozícióban volt
    public bool SetBasePos(Vector3 newPos, float delay = 0) {
        bool returnValue = transform.position != newPos;

        Vector3 originalMovePos = move.position;
        transform.position = newPos;
        move.position = originalMovePos;

        MoveBasePos(delay);

        return returnValue;
    }
    */

    /*
    // Az elemet a bázis pozíciójába mozgatja a megadott idő letelte után
    public void MoveBasePos(float delay = 0) {
        if (delay == 0) delay = 0.001f; // Ha leállítjuk az iTween animációt egy gameObjecten, akkor rögtön nem tudunk egy másikat elindítani rajta, ezért van itt ez a minimális késleltetés
        // iTween animációval az új pozícióba mozgatjuk az elemet
        iTween.Stop(move.gameObject); // Leállítjuk az esetlegesen már működő iTween animációkat
        iTween.MoveTo(move.gameObject, iTween.Hash("position", Vector3.zero, "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "time", 1, "delay", delay, "oncompletetarget", gameObject, "oncomplete", "MoveBasePosEnd"));
    }
    */

    /*
    // A bázis pozícióra mozgatta az iTween a move gameObject-et
    void MoveBasePosEnd() {
        if (enabledGrab)
            SetOrderInLayer(1);
    }
    */

    /*
    // Ha megfogták az elemet, akkor ezzel az eljárással lehet mozgatni
    // dragPos      - word koordináta
    public void DragPos(Vector3 grabWorldPos) {
        iTween.Stop(move.gameObject); // Leállítjuk az esetlegesen már működő iTween animációt

        // Villogás miatt visszaállítjuk a színt
        textBubble.color = textColor;
        border.color = borderColor;

        move.position = grabWorldPos;
    }
    */

    
    // Villogtatjuk az elemet majd a megadott pozícióba megy, majd vissza szól, hogy elkészült
    public void FlashingAndGoOut(Vector3 newPos) {
        StartCoroutine(FlashingAndGoOutCoroutine(newPos));
    }
    
    public IEnumerator FlashingAndGoOutCoroutine(Vector3 newPos) {
        FlashingNegative();
        yield return new WaitForSeconds(1.2f);

        // Ha az adagoló már beadagolta, akkor nem kell kivinni a képernyőből
        if (!enabledGrab) {
            SetBasePos(newPos);
            yield return new WaitForSeconds(1); // Egy másodperc alatt ér a helyére
            gameObject.SetActive(false); // Ha kiért a képből, akkor kikapcsoljuk a láthatóságát
        }
    }

    // Villogtatja az elemet
    public override void FlashingPositive()
    {
        flashingColor = Color.green;
        base.FlashingPositive();
    }

    public override void FlashingNegative()
    {
        flashingColor = Color.red;
        base.FlashingNegative();
    }

    /*
    // Háromszor villog a menüelem a megadott színnel
    public void Flashing(Color color) {
        StartCoroutine(FlashingCoroutine(color));
    }
    */
    public override IEnumerator FlashingCoroutine()
    {
        // A képek eredeti szineit megjegyezzük
        textBubbleColor = textBubble.color; 
        borderColor = border.color;

        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);
            textBubble.color = flashingColor;
            border.color = flashingColor;

            yield return new WaitForSeconds(0.2f);
            textBubble.color = textBubbleColor;
            border.color = borderColor;
        }

        animRun = false;
        yield return null;
    }
    

    /*
    // Vissza adja az elem globális szélességét
    public float GetGlobalWidth() {
        //float canvasWidth = (itemType == ItemType.Picture) ? pictureRectTransform.sizeDelta.x * pictureRectTransform.localScale.x : textRectTransform.sizeDelta.x * textRectTransform.localScale.x;
        float canvasWidth = (itemType == ItemType.Picture) ? pictureRectTransform.sizeDelta.x * pictureRectTransform.lossyScale.x : textRectTransform.sizeDelta.x * textRectTransform.lossyScale.x;

        return canvasWidth; // / transform.localScale.x;
    }
    */

    /*
    // Vissza adja az elem globális szélességét
    public float GetGlobalHeight()
    {
        //float canvasWidth = (itemType == ItemType.Picture) ? pictureRectTransform.sizeDelta.x * pictureRectTransform.localScale.x : textRectTransform.sizeDelta.x * textRectTransform.localScale.x;
        float canvasHeight = (itemType == ItemType.Picture) ? pictureRectTransform.sizeDelta.y * pictureRectTransform.lossyScale.y : textRectTransform.sizeDelta.y * textRectTransform.lossyScale.y;

        return canvasHeight; // / transform.localScale.x;
    }
    */

    public override float GetHeight()
    {
        //float canvasWidth = (itemType == ItemType.Picture) ? pictureRectTransform.sizeDelta.x * pictureRectTransform.localScale.x : textRectTransform.sizeDelta.x * textRectTransform.localScale.x;
        float canvasHeight = (itemType == ItemType.Picture) ? rectTransformPictureCanvas.sizeDelta.y * rectTransformPictureCanvas.lossyScale.y : rectTransformTextCanvas.sizeDelta.y * rectTransformTextCanvas.lossyScale.y;

        return canvasHeight; // / transform.localScale.x;
    }

    public override float GetWidth()
    {
        //float canvasWidth = (itemType == ItemType.Picture) ? pictureRectTransform.sizeDelta.x * pictureRectTransform.localScale.x : textRectTransform.sizeDelta.x * textRectTransform.localScale.x;
        float canvasWidth = (itemType == ItemType.Picture) ? rectTransformPictureCanvas.sizeDelta.x * rectTransformPictureCanvas.lossyScale.x : rectTransformTextCanvas.sizeDelta.x * rectTransformTextCanvas.lossyScale.x;

        return canvasWidth; // / transform.localScale.x;
    }

}
