using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SetItemCollector : DragTarget {

    public float shrinkingSize;             // A zsugorítás mérete (minimum ennyit kell zsugorodnia egy halmaz elemnek, maximum annyit, hogy beleférjen a halmazba)

    [HideInInspector]
    public RectTransform canvasRectTransform; // Ezen az elemen vannak a képek
    Image picture;                          // A megjelenítendő kép
    public Image border;                    // A keret
    public Image borderShadow;
    Text text;                              // Szöveg megjelenítéséhez
    TEXDraw texDraw;                        // Szöveg megjelenítéséhez

    RectTransform upperLeft;                // A halmaz elemek tárolásának bal felső koordinátája
    RectTransform bottomRight;              // A halmaz elemek tárolásának jobb alsó koordinátája

    //List<string> items;                     // Ehhez a halmazhoz milyen nevű halmaz elemek tartoznak
    int itemsOrder = 10;                    // Halmaz elemek a halmazba kerülésnél kapnak egy növekvő rétegsorrendet

    // Use this for initialization
    override public void Awake()
    {
        base.Awake();

        canvasRectTransform = Common.SearchGameObject(gameObject, "Canvas").GetComponent<RectTransform>();
        picture = Common.SearchGameObject(gameObject, "Picture").GetComponent<Image>();
        border = Common.SearchGameObject(gameObject, "Border").GetComponent<Image>();
        borderShadow = Common.SearchGameObject(gameObject, "Shadow").GetComponent<Image>();
        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
        texDraw = Common.SearchGameObject(gameObject, "TEXDraw").GetComponent<TEXDraw>();

        upperLeft = Common.SearchGameObject(gameObject, "UpperLeft").GetComponent<RectTransform>();
        bottomRight = Common.SearchGameObject(gameObject, "BottomRight").GetComponent<RectTransform>();
    }

    // Beállítja a halmazt
    // setItem      - A halmaz neve
    // pictureName  - A halmazon látható kép neve
    // borderColor  - A halmaz keretének színe
    // items        - A halmazba tehető elemek nevének felsorolása
    // canvasSize   - A halmaz össz mérete
    public void InitializeCoroutine(string setItemName, Sprite picture, Color borderColor, List<string> items, Vector2 canvasSize)
    {
        Debug.Log("original canvas size : " + canvasSize.x + ":" + canvasSize.y);

        //canvasSize = new Vector2(canvasSize.x / canvasRectTransform.localScale.x, canvasSize.y / canvasRectTransform.localScale.y);
        canvasSize = new Vector2(canvasSize.x / canvasRectTransform.lossyScale.x, canvasSize.y / canvasRectTransform.lossyScale.y);

        Debug.Log("canvas lossyScale : " + canvasRectTransform.lossyScale.x + ":" + canvasRectTransform.lossyScale.y);

        Debug.Log("original canvas size after : " + canvasSize.x + ":" + canvasSize.y);


        this.picture.sprite = picture; // Common.menuLessonPlan.GetPicture(picture);



        // Később át lehetne térni sima eljárásra

        // yield return null;

        //yield return Common.pictureController.LoadSpriteFromFileSystemCoroutine(pictureName);
        //picture.sprite = Common.pictureController.resultSprite;
        //picture.sprite = Common.pictureController.LoadSpriteFromFileSystemCoroutine();

        //border.color = borderColor;   // Nem állítjuk a border colort, az marad az alapértelmezett
        texDraw.text = setItemName;
        text.text = setItemName;
        text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

        this.items = items;

        // Beállítjuk a canvas méretét a megadott értékre
        canvasRectTransform.sizeDelta = canvasSize;
        canvasRectTransform.GetComponent<BoxCollider2D>().size = canvasSize; // Beállítjuk a BoxCollider2D-t a canvas méretére

        // Ha szükséges összébb nyomjuk a Text componenst, hogy kiférjen a teljes szöveg
        //if (texDraw.rectTransform.rect.width / texDraw.rectTransform.localScale.x < texDraw.preferredWidth) {
        //    texDraw.rectTransform.localScale = new Vector2(texDraw.rectTransform.rect.width / texDraw.preferredWidth, texDraw.rectTransform.localScale.y);
        //}

        // Méretezzük a képet, hogy a kép méretarányos legyen
        if (this.picture.sprite != null)
        {
            float pictureXRatio = canvasSize.x / this.picture.sprite.texture.width;
            //float pictureYRatio = (canvasSize.y - text.rectTransform.rect.height) / this.picture.sprite.texture.height;
            float pictureYRatio = (canvasSize.y - bottomRight.anchoredPosition.y) / this.picture.sprite.texture.height;

            float pictureRatio = Mathf.Min(pictureXRatio, pictureYRatio);

            this.picture.rectTransform.sizeDelta = new Vector2(this.picture.sprite.texture.width * pictureRatio, this.picture.sprite.texture.height * pictureRatio);
        }

        this.picture.enabled = (this.picture.sprite != null); // A képet bekapcsoljuk vagy ki annak megfelelően, hogy van-e kép benne
    }

    /*
    // Megviszgálja, hogy a megadott halmaz elem ebbe a halmazba tartozik-e
    // True értéket ad vissza ha igen
    public bool IsItemInSet(SetItem setItem) {
        return items.Contains(setItem.itemName);
    }
    */

    // A megadott halmaz elemet beteszi a halmazba
    public override void PutItemInTarget(DragItem dragItem)
    {
        dragItem.itemInPlace = true;       // Az elem a helyén van
        dragItem.enabledGrab = false;      // Az elem a helyén van, letiltjuk a mozgatását

        // Kiszámoljuk, hogy mennyire kell lekicsinyíteni a halmaz elemet, hogy beleférjen a halmazba
        float ratioX = (bottomRight.position.x - upperLeft.position.x) / dragItem.GetWidth();
        float ratioY = (upperLeft.position.y - bottomRight.position.y) / dragItem.GetHeight();
        //float ratioX = GetGlobalWidth() / setItem.GetGlobalWidth();
        //float ratioY = GetGlobalHeight() / setItem.GetGlobalHeight();
        float ratio = Mathf.Min(ratioX, ratioY, shrinkingSize);

        float setItemSizeX = dragItem.GetWidth() * ratio;
        float setItemSizeY = dragItem.GetHeight() * ratio;

        // Meghatározzuk a halmaz elem pozícióját
        float setItemPosX = dragItem.MoveTransform.position.x;
        float setItemPosY = dragItem.MoveTransform.position.y;

        if (upperLeft.transform.position.x > setItemPosX - setItemSizeX / 2) setItemPosX = upperLeft.transform.position.x + setItemSizeX / 2;
        if (bottomRight.transform.position.x < setItemPosX + setItemSizeX / 2) setItemPosX = bottomRight.transform.position.x - setItemSizeX / 2;
        if (upperLeft.transform.position.y < setItemPosY + setItemSizeY / 2) setItemPosY = upperLeft.transform.position.y - setItemSizeY / 2;
        if (bottomRight.transform.position.y > setItemPosY - setItemSizeY / 2) setItemPosY = bottomRight.transform.position.y + setItemSizeY / 2;

        // Elhelyezzük az elemet a kiszámolt pozícióban
        dragItem.SetBasePos(new Vector3(setItemPosX, setItemPosY));

        // Betesszük az elemet a halmazba
        //Vector3 t = setItem.transform.localScale;
        dragItem.transform.parent = transform;
        //setItem.transform.localScale = t;

        // Zsugorítjuk az elemet
        iTween.ScaleTo(dragItem.gameObject, iTween.Hash("scale", new Vector3(dragItem.transform.localScale.x * ratio, dragItem.transform.localScale.y * ratio, dragItem.transform.localScale.z * ratio), "easeType", iTween.EaseType.easeInOutCubic));

        dragItem.SetOrderInLayer(itemsOrder++);
    }

    // Vissza adja, hogy a bedobott elemet hova kell a térben elhelyezni
    public override Vector3 GetDropPos(DragItem dragItem)
    {
        float sizeX = bottomRight.position.x - upperLeft.position.x;
        float sizeY = upperLeft.position.y - bottomRight.position.y;

        return new Vector3(
            (float)(upperLeft.position.x + sizeX * Common.random.NextDouble()),
            (float)(bottomRight.position.y + sizeY * Common.random.NextDouble()),
            0);
    }

    // Vissza adja az elem globális szélességét
    public override float GetWidth()
    {
        return canvasRectTransform.sizeDelta.x * canvasRectTransform.lossyScale.x;
    }

    // Vissza adja az elem globális szélességét
    public override float GetHeight()
    {
        return canvasRectTransform.sizeDelta.y * canvasRectTransform.lossyScale.y;
    }
}
