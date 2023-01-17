using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class TextyGameTextAndPictureInScroll : MonoBehaviour
{
    class Item
    {
        public RectTransform rectTransform;
        public ILayoutElement layoutElement;
        public bool isPicture;

        public Item(RectTransform rectTransform = null, ILayoutElement layoutElement = null, bool isPicture = false)
        {
            this.rectTransform = rectTransform;
            this.layoutElement = layoutElement;
            this.isPicture = isPicture;
        }
    }

    //public delegate Sprite CallBack_In_String_Out_Sprite(string x);

    GameObject prefabText;
    GameObject prefabTEXDraw;
    GameObject prefabPicture;

    RectTransform content;

    [Tooltip("Hézag a scrollBox tetején és alján")]
    public float gapTopAndBottom;
    [Tooltip("A szöveg és kép elemek közötti hézag")]
    public float gapBetweenItems;

    Zoomer zoomer;

    List<Item> listOfTextsAndPictures;

    void Awake()
    {
        // Megkeressük a klónozható gameObject-eket
        prefabText = gameObject.SearchChild("Text").gameObject;
        prefabTEXDraw = gameObject.SearchChild("TEXDraw").gameObject; 
        prefabPicture = gameObject.SearchChild("PictureStripe").gameObject;

        // Kikapcsoljuk a prefab-okat
        prefabText.SetActive(false);
        prefabTEXDraw.SetActive(false);
        prefabPicture.SetActive(false);

        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();

        listOfTextsAndPictures = new List<Item>();
    }

    public void Initialize(string text, Common.CallBack_In_String_Out_Sprite getSprite, Zoomer zoomer)
    {
        this.zoomer = zoomer;

        float contentWidth = Common.GetRectTransformPixelSize(content, yNeed: false).x;

        // Felbontjuk a megadott szöveget szöveg és kép részekre
        List<string> texts = SplitText(text);

        //Debug.LogError(Common.GetGameObjectHierarchy(gameObject));

        // Töröljük a korábbi gameObjecteket ha vannak
        for (int i = listOfTextsAndPictures.Count - 1; i >= 0; i--)
            Destroy(listOfTextsAndPictures[i].rectTransform.gameObject);

        // Létrehozzuk a szöveg és kép részleteket
        listOfTextsAndPictures = new List<Item>();
        for (int i = 0; i < texts.Count; i++)
        {
            Sprite sprite = getSprite(texts[i]);

            if (sprite != null)
            {
                // Képet kell létrehozni
                GameObject go = Instantiate(prefabPicture, content.transform, false);
                go.SetActive(true);
                Image image = go.GetComponentInChildren<Image>();
                image.sprite = sprite;

                // Ki kell számolni a méretét
                Vector2 spriteSize = new Vector2(sprite.rect.width, sprite.rect.height);
                float spriteRatio = spriteSize.x / spriteSize.y;
                float imageHeight = contentWidth / spriteRatio;
                Vector2 resultSize = imageHeight < spriteSize.y ? new Vector2(contentWidth, imageHeight) : spriteSize;

                // Beállítjuk a méretét
                RectTransform pictureStripeRectTransform = go.GetComponent<RectTransform>();
                pictureStripeRectTransform.sizeDelta = new Vector2(pictureStripeRectTransform.sizeDelta.x, resultSize.y);

                RectTransform imageRectTransform = image.GetComponent<RectTransform>();
                imageRectTransform.sizeDelta = resultSize;

                listOfTextsAndPictures.Add(new Item(pictureStripeRectTransform, isPicture: true));

                // Beállítjuk a képre kattintás eseményét
                UIButtonHelper buttonHelper = go.GetComponentInChildren<UIButtonHelper>();
                buttonHelper.i = i;
                buttonHelper.callBackInteger = ButtonClick;

                //UnityEngine.UI.Button tempButton = go.GetComponentInChildren<UnityEngine.UI.Button>();
                //tempButton.onClick.AddListener(() => ButtonClick(i));

                //go.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() => ButtonClick(i));
                //go.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener( ( (delegate { ButtonClick(i); });


                //string debugString = "";
                //debugString += "Sprite size : " + spriteSize + "\n";
                //debugString += "Image height : " + imageHeight;
                //
                //Debug.LogError(debugString);
            }
            else
            {
                if (Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text)
                {
                    // Sima szöveges komponenst kell létrehozni
                    GameObject go = Instantiate(prefabText, content.transform, false);
                    go.SetActive(true);
                    Text newText = go.GetComponent<Text>();
                    newText.text = texts[i];

                    listOfTextsAndPictures.Add(new Item(go.GetComponent<RectTransform>(), newText));
                }
                else
                {
                    // TEXDraw komponenst kell létrehozni
                    GameObject go = Instantiate(prefabTEXDraw, content.transform, false);
                    go.SetActive(true);
                    TEXDraw texDraw = go.GetComponent<TEXDraw>();
                    texDraw.text = texts[i];

                    listOfTextsAndPictures.Add(new Item(go.GetComponent<RectTransform>(), texDraw));
                }
            }
        }
    }

    public void Clear()
    {

    }

    /// <summary>
    /// A kérdés szövegét szavakra bontja. Figyel a képletek körül levő [#block] k é p l e t [#/block] szerkezetre
    /// </summary>
    /// <returns></returns>
    List<string> SplitText(string text)
    {
        string blockStart = "[#image]";
        string blockEnd = "[#/image]";

        blockStart = Common.ConvertStringToRegEx(blockStart);
        blockEnd = Common.ConvertStringToRegEx(blockEnd);

        // Megkeressük a szövegben található képeket
        MatchCollection matches = Regex.Matches(text, @"(" + blockStart + @"(.*?)" + blockEnd + @")");

        int index = 0;
        List<string> texts = new List<string>();
        for (int i = 0; i < matches.Count; i++)
        {
            if (matches[i].Index >= index)
            {
                // A kép előtti szöveget ha van hozzáadjuk
                if (matches[i].Index > index)
                    texts.Add(text.Substring(index, matches[i].Index - index)); 

                // Hozzáadjuk a képet
                texts.Add(matches[i].Groups[2].Captures[0].Value);

                // Beállítjuk az indexet
                index = matches[i].Index + matches[i].Length;
            }
        }

        // Ha az utolsó kép után még van szöveg, akkor azt is hozzáadjuk
        if (text.Length > index)
            texts.Add(text.SubstringSafe(index, text.Length));


        //string debugString = "";
        //for (int i = 0; i < texts.Count; i++)
        //{
        //    debugString += texts[i] + "\n";
        //}
        //
        //Debug.LogError(debugString);


        //matches[0].
        //if (matches.Success)
        //    for (int i = 0; i < matches.Groups[0].Captures.Count; i++)
        //    {
        //        Debug.LogError(matches.Groups[0].Captures[i].Value);
        //
        //        //Capture subCapture = Common.GetFirstSubCapture(match, match.Groups[1].Captures[i], 3);
        //        //texts.Add(subCapture != null ? subCapture.Value : match.Groups[1].Captures[i].Value);
        //    }

        return texts;
    }

    // Update is called once per frame
    void Update()
    {
        // Beállítjuk a szöveg és kép részletek pozícióját
        // Végig megyünk a listán és beállítjuk 

        //Debug.LogError(Common.GetGameObjectHierarchy(gameObject));
        //string debugString = "";

        float pos = gapTopAndBottom;
        for (int i = 0; i < listOfTextsAndPictures.Count; i++)
        {
            if (i != 0)
                pos += gapBetweenItems;

            //debugString += i + " Pos : " + pos + "\n";
            //debugString += "PreferredHeight : " + listOfTextsAndPictures[i].layoutElement.preferredHeight + "\n";

            listOfTextsAndPictures[i].rectTransform.anchoredPosition = new Vector2(0, -pos);

            pos += listOfTextsAndPictures[i].isPicture ? listOfTextsAndPictures[i].rectTransform.sizeDelta.y : listOfTextsAndPictures[i].layoutElement.preferredHeight;
        }

        pos += gapTopAndBottom;

        //Debug.LogError(debugString);

        // Beállítjuk a tartalom méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, pos);
    }

    public void ButtonClick(int index)
    {
        Debug.Log(index);
        //Image image = listOfTextsAndPictures[index].rectTransform.GetComponentInChildren<Image>();
        //zoomer.Zoom(image.gameObject, moveable: true);

        zoomer.Zoom(listOfTextsAndPictures[index].rectTransform.GetComponentInChildren<Image>().gameObject, moveable: true);
    }
}
