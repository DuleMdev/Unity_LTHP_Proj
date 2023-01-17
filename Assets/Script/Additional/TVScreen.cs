using UnityEngine;
using System.Collections;
using System;

using UnityEngine.UI;

/*
A boom játékban a képernyőn megjelenő tartalom kezelésére és a válaszok és a helyüknek a megjelenítését 
kezelő objektum.










*/
public class TVScreen : MonoBehaviour, IWidthHeight {

    [HideInInspector]
    public GameObject tvScreenMove; // A TV képernyőjét skálázó gameObject (A válaszok előugrásához kell)

    RectTransform canvas;       // 
    GameObject questionMark;    // A kérdőjelet tartalmazó kép gameObject-je

    Image imageTextBackground;  // A szöveget tartalmazó képenyő alap képe
    Text text;                  // A felírat szövegének megjelenéséhez
    TEXDraw texDraw;            // A felírat szövegének megjelenéséhez
    Image imagePictureBackground; // A kérdéshez tartozó kép
    Image imagePicture;         // A kérdéshez tartozó kép
    Image imageQuestionMark;    // A kérdés képe
    Image imageChannelError;    // A képhibát tartalmazó gameObject
    Image imageWhiteScreen;     // Fehér képenyőhöz

    int lastError;                  // Az utolsó hiba képet hogyan tükröztük (kétszer egymás után ne használjuk ugyan azt a tükrözést, mert abból állókép lesz)
    float lastErrorPictureChangeTime;   // Mikor változtattuk meg utoljára a hiba kép tükrözését

    // Use this for initialization
    void Awake() {
        tvScreenMove = Common.SearchGameObject(gameObject, "TVscreenMove").gameObject;

        canvas = Common.SearchGameObject(gameObject, "Canvas").GetComponent<RectTransform>();

        questionMark = Common.SearchGameObject(gameObject, "ImageQuestionMark").gameObject;
        imageTextBackground = Common.SearchGameObject(gameObject, "ImageText").GetComponent<Image>();
        text = Common.SearchGameObject(gameObject, "questionText").GetComponent<Text>();
        texDraw = Common.SearchGameObject(gameObject, "questionTEXDraw").GetComponent<TEXDraw>();
        imagePictureBackground = Common.SearchGameObject(gameObject, "ImagePictureBackground").GetComponent<Image>();
        imagePicture = Common.SearchGameObject(gameObject, "ImagePicture").GetComponent<Image>();
        imageQuestionMark = Common.SearchGameObject(gameObject, "ImageQuestionMark").GetComponent<Image>();
        imageChannelError = Common.SearchGameObject(gameObject, "ImageChannelError").GetComponent<Image>();
        imageWhiteScreen = Common.SearchGameObject(gameObject, "ImageWhiteScreen").GetComponent<Image>();
    }

    /// <summary>
    /// Beállítjuk a képernyőt felépítő képeket.
    /// </summary>
    /// <param name="background">Ha a képernyőn szöveg jelenik meg, akkor ez a kép lesz a szöveg háttérképe.</param>
    /// <param name="questionMark">A kérdés képe.</param>
    /// <param name="channelError">A csatorna hiba képe.</param>
    /// <param name="textColor">Ha a kérdés szöveg, akkor az ezzel a szinnel fog megjelenni.</param>
    public void Set(Sprite background, Sprite questionMark, Sprite channelError, Color textColor)
    {
        imageTextBackground.sprite = background;
        imageQuestionMark.sprite = questionMark;
        imageChannelError.sprite = channelError;

        text.color = textColor;
        texDraw.color = textColor;
    }

    // Update is called once per frame
    void Update() {
        // A csatorna hiba kép tükrözése
        lastErrorPictureChangeTime -= Time.deltaTime;
        if (lastErrorPictureChangeTime < 0)
        {
            // Kitaláljuk az új tükrözést, ami nem egyezhet a korábbi tükrözéssel
            while (true)
            {
                int newError = Common.random.Next(4);
                if (newError != lastError)
                {
                    lastError = newError;
                    break;
                }
            }

            // Az új értéknek megfelelően tükrözzük az error képet
            imageChannelError.transform.localScale = new Vector3(((lastError & 1) == 0) ? 1 : -1, ((lastError & 2) == 0) ? 1 : -1);
            lastErrorPictureChangeTime = 0.05f; // Másodpercenként 20-szor tükrözünk
        }
    }

    public void Reset() {
        questionMark.SetActive(false);
        ShowQuestion(false);
        //imageTextBackground.gameObject.SetActive(false);
        ChangeChannelErrorVisibility(0);
        ChangeWhiteScreenVisibility(0);
    }

    // Kérdőjelet megjelenítjük
    public void ShowQuestionMark() {
        questionMark.SetActive(true);
    }

    // TV bekapcsolása
    // A channelError képernyő fokozatosan láthatóvá válik, mint amikor a TV bemelegszik
    // A fadeTime mutatja, hogy mennyi idő alatt lesz teljesen látható
    public void TVOn(float fadeTime) {
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", fadeTime, "easetype", iTween.EaseType.linear, "onupdate", "ChangeChannelErrorVisibility", "onupdatetarget", gameObject));
    }

    // A TVOn metódusban található iTween ValueTo hívja ezt a metódust
    // Beállítjuk a csatorna hiba láthatóságát (A játék kezdésénél ez fokozatosan jelenik meg)
    void ChangeChannelErrorVisibility(float visibility) {
        imageChannelError.color = new Color(1, 1, 1, visibility);
    }

    // Megjelenik a válasz
    public void ShowAnswer(string answer, Sprite picture, float fadeIn = 0.2f, float fadeOut = 0.5f) {
        StartCoroutine(ShowAnswerCoroutine(answer, picture, fadeIn, fadeOut));
    }

    /// <summary>
    /// A TV képernyőn megjelenik a válasz. A válasz lehet egy kép, akkor a picture paraméter van megadva, vagy szöveg, ekkor az answer paraméter.
    /// A válasz úgy jelenik meg mint egy polaroid kép.
    /// Először gyorsan kifehéredik a képernyő aminek a gyorsaságát a fadeIn paraméter adja meg.
    /// Majd miután megjelent a megadott tartalom a fehér kép alatt a képet eltüntetjük a fadeOut
    /// paraméterben megadott idő alatt.
    /// </summary>
    /// <param name="answer">Ha a megjelenítendő tartalom szöveg, akkor az itt van megadva.</param>
    /// <param name="picture">Ha a megjelenítendő tartalom kép, akkor az itt van megadva.</param>
    /// <param name="fadeIn">A fehér kép mennyi idő alatt jelenjen meg.</param>
    /// <param name="fadeOut">A fehér kép mennyi idő alatt tünjön el.</param>
    /// <returns></returns>
    IEnumerator ShowAnswerCoroutine(string answer, Sprite picture, float fadeIn, float fadeOut) {
        //float fadeIn = 0.2f;
        //float fadeOut = 0.5f;
        // Egy fehér kép feltűnik
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", fadeIn, "easetype", iTween.EaseType.linear, "onupdate", "ChangeWhiteScreenVisibility", "onupdatetarget", gameObject));
        yield return new WaitForSeconds(fadeIn);

        // Alatta megjelenik a tartalom
        if (picture != null)
            SetPicture(picture);
        else
            SetText(answer);

        ShowQuestion(true);
        questionMark.SetActive(false);

        // A fehér kép eltűnik
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", fadeOut, "easetype", iTween.EaseType.linear, "onupdate", "ChangeWhiteScreenVisibility", "onupdatetarget", gameObject));
        yield return new WaitForSeconds(fadeOut);
    }

    void ChangeWhiteScreenVisibility(float visibility) {
        imageWhiteScreen.color = new Color(1, 1, 1, visibility);
    }

    public void BackgroundDefault() {
        ChangeBackgroundColor(new Color(65 / 255f, 195 / 255f, 184 / 255f));
    }

    public void BackgroundRed() {
        ChangeBackgroundColor(new Color(203 / 255f, 112 / 255f, 71 / 255f));
    }

    public void BackgroundGreen() {
        ChangeBackgroundColor(new Color(51 / 255f, 185 / 255f, 152 / 255f));
    }

    void ChangeBackgroundColor(Color color)
    {
        imageTextBackground.color = color;
    }

    // Ha a text egy kép név, akkor képet cserélünk
    public void SetText(string text) {
        this.text.text = text;
        texDraw.text = text;

        imagePictureBackground.enabled = false;
        imagePicture.enabled = false;
    }

    public void SetPicture(Sprite picture) {
        imagePicture.sprite = picture;
        imagePictureBackground.enabled = true;
        imagePicture.enabled = true;
    }

    public string GetText() {
        return text.text;
    }

    public void ShowQuestion(bool show) {
        imageTextBackground.gameObject.SetActive(show);
        text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

        imagePictureBackground.gameObject.SetActive(show);
        imageChannelError.gameObject.SetActive(!show);
    }

    public float GetHeight() {
        // return imageTextBackground.GetComponent<Renderer>().bounds.size.y;
        return canvas.rect.height * canvas.lossyScale.y;
    }

    public float GetWidth() {
        // return imageTextBackground.GetComponent<Renderer>().bounds.size.x;
        return canvas.rect.width * canvas.lossyScale.x;
    }
}
