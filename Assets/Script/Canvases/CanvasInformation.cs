using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasInformation : MonoBehaviour {

    Canvas canvas;

    Image panelImage;           // A panel átszínezéséhez

    Text textInformation;       // Az információs vagy a hibaüzenet megjelenítéséhez
    Text textButtonOk;          // Az Ok gomb szövegének beállításához

    Button buttonOk;            // Kikapcsoljuk, ha nem kell az ok gomb
    
    Common.CallBack callBack;   // Ha megnyomták az Ok gombot meghívja a megadott függvényt

    // Use this for initialization
    void Awake()
    {
        Common.canvasInformation = this;

        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "CanvasInformation").GetComponent<Canvas>();

        panelImage = Common.SearchGameObject(gameObject, "PanelText").GetComponent<Image>();

        textInformation = Common.SearchGameObject(gameObject, "TextInformation").GetComponent<Text>();
        textButtonOk = Common.SearchGameObject(gameObject, "TextButtonOk").GetComponent<Text>();

        buttonOk = Common.SearchGameObject(gameObject, "ButtonOk").GetComponent<Button>();

        Hide();
    }

    void Start() {
        textButtonOk.text = Common.languageController.Translate("Ok");
    }

    public void ShowInformation(string text, Common.CallBack callBack = null, bool buttonOkEnabled = true) {

    }

    // Kiír egy hiba üzenetet
    // Opcionálisan meg lehet adni, hogy az Ok gomb látható legyen-e vagy sem
    public void ShowError(string text, Common.CallBack callBack = null, bool buttonOkEnabled = true)
    {
        this.callBack = callBack;

        panelImage.color = Color.red;
        textInformation.text = Common.languageController.Translate(text);
        //buttonOk.enabled = buttonOkEnabled;
        canvas.enabled = true;
    }

    // Eltünteti az information képernyőt
    public void Hide() {
        canvas.enabled = false;
    }

    // Az ok gombra kattintottak
    public void ButtonClickOk()
    {
        // Eltüntetjük a képernyőt
        Hide();

        if (callBack != null)
            callBack();
    }
}
