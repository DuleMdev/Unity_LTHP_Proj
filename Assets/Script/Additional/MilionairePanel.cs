using UnityEngine;
using System.Collections;

using UnityEngine.UI;

/*
    - 2019.07.12
    Képletek megjelenítéséhez (gyökvonás stb.) a Text Unity UI komponenséről egy TEXDraw nevű AssetStore-os komponensre álltunk át.
*/
public class MilionairePanel : MonoBehaviour
{
    public Color markedColor;

    Text text;          // A panelen megjelenő szövegnek
    TEXDraw texDraw;    // A panelen megjelenő szövegnek
    Image image;        // A panel átszínezéséhez
    GameObject move;    // A panel mozgatásához

    Color defaultColor; // A panel alap vagy beállított színe

    bool disabled;      // Ha jó válasz történik, akkor a továbbiakban már nem lehet megnyomni ezt a panelt, mivel letiltásra kerül

    public delegate void PanelClick(MilionairePanel panel);
    public PanelClick panelClick;

    public string textID;

	// Use this for initialization
	void Awake () {
        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
        texDraw = Common.SearchGameObject(gameObject, "TEXDraw").GetComponent<TEXDraw>();
        image = Common.SearchGameObject(gameObject, "Image").GetComponent<Image>();
        move = Common.SearchGameObject(gameObject, "CanvasMove").gameObject;

        //GetComponentInChildren<Button>().buttonClick = ButtonClick; // Beállítjuk a Button objektumra kattintást feldolgozó metódust

        defaultColor = image.color;
        panelClick = null;
    }

    // A régi szerveren a text van használva kizárólag
    // A Server2020-an a text-ben az ID van, míg a reallyTextInServer2020-ban van az igazi szöveg
    public void SetText(string text, string reallyTextInServer2020)
    {
        if (Common.configurationController.isServer2020)
        {
            textID = text;
            text = reallyTextInServer2020;
        }

        this.text.text = text;
        texDraw.text = text;
        this.text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;

        //this.text.text = text; // Common.TextUIHelper(text);
    }

    public string GetText() {
        if (Common.configurationController.isServer2020)
            return textID;
        else
            return text.text;
    }

    public void SetPicture(Sprite sprite) {
        image.sprite = sprite;
    }

    // Több layout-os játék használja
    public void SetColor(Color color) {
        defaultColor = color;
        image.color = color;
    }

    // Több layout-os játék használja
    public void SetTextColor(Color color) {
        text.color = color;
    }

    public void SetGlobalMovePos(Vector3 pos) {
        move.transform.position = pos;
    }

    public void SetLocalMovePos(Vector3 pos) {
        move.transform.localPosition = pos;
    }

    public void Move(Vector3 pos, float time, iTween.EaseType easeType) {
        iTween.MoveTo(move, iTween.Hash("islocal", true, "position", pos, "time", time, "easetype", easeType));
    }

    // Megjelölve - Ha nincs azonnali visszajelzés, akkor ezzel jelölhetjük a kiválasztást
    public void Marking()
    {
        disabled = true;
        image.color = markedColor;
    }

    // Villogtatja a panelt
    public float Flashing(bool goodAnswer)
    {
        if (goodAnswer)
        {
            disabled = true;

            StartCoroutine(FlashingCoroutine(Color.green, defaultColor));
            defaultColor = Color.green;

        }
        else
        {
            StartCoroutine(FlashingCoroutine(Color.red, defaultColor));
        }

        return 1.2f;
    }

    IEnumerator FlashingCoroutine(Color color1, Color color2)
    {
        for (int i = 0; i < 3; i++)
        {
            image.color = color1;
            yield return new WaitForSeconds(0.2f);

            image.color = color2;
            yield return new WaitForSeconds(0.2f);
        }

        image.color = defaultColor;
    }

    void ButtonClick(Button button)
    {

        //if (!disabled) {
        //    if (panelClick != null)
        //        panelClick(this);
        //}
    }

    public void ButtonClick()
    {
        if (!disabled)
        {
            if (panelClick != null)
                panelClick(this);
        }
    }
}
