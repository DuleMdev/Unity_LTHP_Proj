using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasBorder_16_9 : MonoBehaviour
{
    static public CanvasBorder_16_9 instance;

    /// <summary>
    /// Mi legyen az alap szine a keretnek
    /// </summary>
    public Color defaultColor;
    /// <summary>
    /// Meg lehet-e változtatni a keret háttérszínét
    /// </summary>
    public bool overWritetable;

    /// <summary>
    /// Ezekből az Image componensekből (4 darab) van elkészítve a keret
    /// </summary>
    Image[] borderImages;

    Image image_Up;
    Image image_Up_Transparent;
    Image image_Up_Watermark;
    Image image_Down;
    Image image_Down_Transparent;
    Image image_Down_Watermark;
    Image image_Left;
    Image image_Left_Transparent;
    Image image_Left_Watermark;
    Image image_Right;
    Image image_Right_Transparent;
    Image image_Right_Watermark;

    Canvas canvas;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        borderImages = transform.GetComponentsInChildren<Image>(true);

        image_Up = gameObject.SearchChild("Image_Up").GetComponent<Image>();
        image_Up_Transparent = gameObject.SearchChild("Image_Up_Transparent").GetComponent<Image>();
        image_Up_Watermark = gameObject.SearchChild("Image_Up_Watermark").GetComponent<Image>();
        image_Down = gameObject.SearchChild("Image_Down").GetComponent<Image>();
        image_Down_Transparent = gameObject.SearchChild("Image_Down_Transparent").GetComponent<Image>();
        image_Down_Watermark = gameObject.SearchChild("Image_Down_Watermark").GetComponent<Image>();
        image_Left = gameObject.SearchChild("Image_Left").GetComponent<Image>();
        image_Left_Transparent = gameObject.SearchChild("Image_Left_Transparent").GetComponent<Image>();
        image_Left_Watermark = gameObject.SearchChild("Image_Left_Watermark").GetComponent<Image>();
        image_Right = gameObject.SearchChild("Image_Right").GetComponent<Image>();
        image_Right_Transparent = gameObject.SearchChild("Image_Right_Transparent").GetComponent<Image>();
        image_Right_Watermark = gameObject.SearchChild("Image_Right_Watermark").GetComponent<Image>();

        canvas = GetComponent<Canvas>();

        // Beállítjuk az alap értelmezett háttérszínt
        _SetBorderColor(defaultColor);

        Enabled(false);
    }

    void Start()
    {

    }


    public void Enabled(bool enabled)
    {
        gameObject.SetActive(enabled);
    }

    /// <summary>
    /// Beállítja a keret háttérszínét, ha engedélyezve van a felűlírása
    /// </summary>
    /// <param name="color"></param>
    public void SetBorderColor(Color color)
    {
        if (overWritetable)
            _SetBorderColor(color);
    }

    /// <summary>
    /// Beállítja a háttérszínt a megadottra
    /// </summary>
    /// <param name="color"></param>
    void _SetBorderColor(Color color)
    {
        foreach (Image image in borderImages)
            if (!image.gameObject.name.Contains("_"))
                image.color = color;
    }

    public void SetBorderPictures(HHHScreen newScreen)
    {
        // Lekérdezzük, hogy az új képernyőn van-e kiegészítő háttérkép információ
        ExtendedBackgroundDatas extendedBackgroundDatas = newScreen.GetComponent<ExtendedBackgroundDatas>();

        List<ExtendedBackgroundDatas.ExtendedBackgroundData> extendedBackgroundDataList = new List<ExtendedBackgroundDatas.ExtendedBackgroundData>();
        // Ha vannak kiterjesztett adatok, akkor azt átmásoljuk egy másik tömbbe
        if (extendedBackgroundDatas)
            foreach (var item in extendedBackgroundDatas.extendedBackgroundDataList)
                extendedBackgroundDataList.Add(item);

        // Ha kevesebb background adat van mint kettő, akkor csinálunk még, hogy meg legyen a kettő
        while (extendedBackgroundDataList.Count < 3)
            extendedBackgroundDataList.Add(new ExtendedBackgroundDatas.ExtendedBackgroundData());

        // Most már biztos, hogy van három adat

        // betöltjük az alsó rétegre a képeket
        ExtendedBackgroundDatas.ExtendedBackgroundData extendedBackgroundData = extendedBackgroundDataList[0];

        SetImageSprite(image_Up, extendedBackgroundData.up, extendedBackgroundData.color, setHeight: true);
        SetImageSprite(image_Down, extendedBackgroundData.down, extendedBackgroundData.color, setHeight: true);
        SetImageSprite(image_Left, extendedBackgroundData.left, extendedBackgroundData.color, setHeight: false);
        SetImageSprite(image_Right, extendedBackgroundData.right, extendedBackgroundData.color, setHeight: false);

        // Betöltjük a transparent rétegre a képeket
        extendedBackgroundData = extendedBackgroundDataList[1];

        SetImageSprite(image_Up_Transparent, extendedBackgroundData.up, extendedBackgroundData.color, setHeight: true);
        SetImageSprite(image_Down_Transparent, extendedBackgroundData.down, extendedBackgroundData.color, setHeight: true);
        SetImageSprite(image_Left_Transparent, extendedBackgroundData.left, extendedBackgroundData.color, setHeight: false);
        SetImageSprite(image_Right_Transparent, extendedBackgroundData.right, extendedBackgroundData.color, setHeight: false);

        // Betöltjük a watermark rétegre a képeket
        extendedBackgroundData = extendedBackgroundDataList[2];

        SetImageSprite(image_Up_Watermark, extendedBackgroundData.up, extendedBackgroundData.color, setHeight: true);
        SetImageSprite(image_Down_Watermark, extendedBackgroundData.down, extendedBackgroundData.color, setHeight: true);
        SetImageSprite(image_Left_Watermark, extendedBackgroundData.left, extendedBackgroundData.color, setHeight: false);
        SetImageSprite(image_Right_Watermark, extendedBackgroundData.right, extendedBackgroundData.color, setHeight: false);

    }

    /// <summary>
    /// Beállítja a megadott Image- méretét, hogy a mérete megfeleljen a kép képarányának. (Tehát, hogy teljesen kitöltse az Image méretét torzítás mentesen)
    /// </summary>
    void SetImageSprite(Image image, Sprite sprite, Color color, bool setHeight)
    {
        image.sprite = sprite;
        image.color = color;

        // Ha létező a Sprite, akkor az Image komponens méretét is beállítjuk
        if (sprite)
            if (setHeight) // Az Image komponens magasságát vagy a szélességét kell beállítani a betöltött kép függvényében?
                ((RectTransform)image.transform).sizeDelta = new Vector2(((RectTransform)image.transform).sizeDelta.x, ((RectTransform)canvas.transform).sizeDelta.x / sprite.texture.width * sprite.texture.height);
            else
                ((RectTransform)image.transform).sizeDelta = new Vector2(((RectTransform)canvas.transform).sizeDelta.y / sprite.texture.height * sprite.texture.width, ((RectTransform)image.transform).sizeDelta.y);
    }
}
