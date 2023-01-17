using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour, IDataProvider {

    static public GameMenu instance;

    Canvas canvas;

    RectTransform rectTransformImageBackground;
    RectTransform rectTransformImageOwlBackground;
    Image imageOwlBackground; // A bagoly szinének beállításához kell

    RectTransform rectTransformExitButton;    // Ha nincs következő tanegység akkor az exit gombot középre kell helyezni
    float rectTransformExitButtonDefaultXPos;

    GameObject previousButton;
    Text textButtonExit;
    GameObject nextButton;
    Text textButtonNext;

    GameObject infoButton;
    GameObject popUp;
    GameObject popUpInfo;
    GameObject popUpBugReport;

    InputField errorDescriptionInputField;

    public JSONNode gameDataJson;
    Common.CallBack_In_String buttonClick;

    public bool menuShow { get; private set; } // A menüt lenyitották-e
    int initialCanvasOrderInLayer;
    public bool isNextButtonActive; // Be van-e kapcsolva a "Következő tanegység" gomb

    public string selectedButton { get; private set; } // Melyik gombot nyomták meg a menüben
    public bool showInfoPanel { get; private set; } // A PopUp-ban az infopanel vagy a BugReport panel látszik

    Texture2D textureScreenShoot;   // A menü lenyitásakor készül egy screenshoot amit a BugReport-ban elküldhetünk

    void Awake() {
        instance = this;

        canvas = gameObject.SearchChild("Canvas").GetComponent<Canvas>();

        rectTransformImageBackground = gameObject.SearchChild("ImageBackground").GetComponent<RectTransform>();
        rectTransformImageOwlBackground = gameObject.SearchChild("ImageOwlBackground").GetComponent<RectTransform>();
        imageOwlBackground = rectTransformImageOwlBackground.GetComponent<Image>();

        rectTransformExitButton = gameObject.SearchChild("ButtonExit").GetComponent<RectTransform>();
        rectTransformExitButtonDefaultXPos = rectTransformExitButton.anchoredPosition.x;

        previousButton = gameObject.SearchChild("ButtonPrevious").gameObject;
        textButtonNext = gameObject.SearchChild("TextNext").GetComponent<Text>();
        nextButton = gameObject.SearchChild("ButtonNext").gameObject;
        textButtonExit = gameObject.SearchChild("TextExit").GetComponent<Text>();

        infoButton = gameObject.SearchChild("ImageInfoButton").gameObject;
        popUp = gameObject.SearchChild("InfoPanelBorder").gameObject;
        popUpInfo  = gameObject.SearchChild("InfoPanel").gameObject;
        popUpBugReport = gameObject.SearchChild("BugReportPanel").gameObject;

        errorDescriptionInputField = gameObject.SearchChild("ErrorDescriptionInputField").GetComponent<InputField>();

        initialCanvasOrderInLayer = canvas.sortingOrder;

        transform.position = Vector3.zero;
        menuShow = false;
    }

    // Use this for initialization
    void Start () {
        Enabled(false);
    }

    public void Initialize(Common.CallBack_In_String buttonClick, Color owlColor) {
        selectedButton = "";
        this.buttonClick = buttonClick;
        imageOwlBackground.color = owlColor;
        errorDescriptionInputField.text = "";
    }

    public void SetCanvasOrderInLayer(int order) {
        canvas.sortingOrder = order;
    }

    public void SetPreviousButton(bool enabled) {
        previousButton.SetActive(enabled);
    }

    /// <summary>
    /// Látható legyen-e a következő tanegység gomb. Projekt termék lejátszásánál nem kell, ott elég a kilépés gomb.
    /// </summary>
    /// <param name="enabled"></param>
    public void SetNextButton(bool enabled)
    {
        isNextButtonActive = enabled;
        nextButton.SetActive(enabled);
        rectTransformExitButton.anchoredPosition = new Vector2(enabled ? rectTransformExitButtonDefaultXPos : 0, 0);
    }

    // Engedélyezzük vagy nem a game menüt
    public void Enabled(bool enabled) {
        gameObject.SetActive(enabled);
        menuShow = false;

        if (enabled)
        {
            iTween.Stop(gameObject);

            // Bekapcsoljuk az info gombot ha rendelkezésre áll info a játékról
            infoButton.SetActive(gameDataJson != null);
            popUp.SetActive(false);
            SetPopUpContent(true);

            //Common.languageController.Refresh();
            SetCanvasOrderInLayer(initialCanvasOrderInLayer);
            UpdateBackgroundPos(rectTransformImageBackground.sizeDelta.y);
            UpdateOwlRotation(-180);
            Common.fade.HideImmediatelly();
        }
        else {
            Common.fade.HideImmediatelly();
        }
    }

    /// <summary>
    /// Megmutatjuk a Game menüt.
    /// </summary>
    void MenuShow() 
    {
        /*
        iTween.Stop(gameObject); // Leállítjuk a korábbi animációkat, ha még mennének

        // Kiírjuk a szövegeket
        textButtonNext.text = Common.languageController.Translate(C.Texts.nextGame);
        textButtonExit.text = Common.languageController.Translate(C.Texts.exit);

        // Elindítjuk az animációkat
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformImageBackground.anchoredPosition.y, "to", 0, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "UpdateBackgroundPos", "onupdatetarget", gameObject));
        iTween.ValueTo(gameObject, iTween.Hash("from", -180, "to", 0, "easetype", iTween.EaseType.easeOutBack, "onupdate", "UpdateOwlRotation", "onupdatetarget", gameObject));
        Common.fade.Show(Color.white, 0.4f, 1, ButtonClick);

        menuShow = true;
        */

        StartCoroutine(MenuShowCoroutine());
    }

    IEnumerator MenuShowCoroutine()
    {
        // Készítünk egy screenShoot-ot
        yield return new WaitForEndOfFrame();
        textureScreenShoot = ScreenCapture.CaptureScreenshotAsTexture();

        /*
        // do something with texture
        byte[] bytes = textureScreenShoot.EncodeToPNG();
        string base64 = "data:image/png;base64," + System.Convert.ToBase64String(bytes);

        File.WriteAllBytes("D:/screenShoot.png", bytes);
        File.WriteAllText("D:/screenshootBase64.txt", base64);

        // cleanup
        Object.Destroy(textureScreenShoot);
        */


        iTween.Stop(gameObject); // Leállítjuk a korábbi animációkat, ha még mennének

        // Kiírjuk a szövegeket
        textButtonNext.text = Common.languageController.Translate(C.Texts.nextGame);
        textButtonExit.text = Common.languageController.Translate(C.Texts.exit);

        // Elindítjuk az animációkat
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformImageBackground.anchoredPosition.y, "to", 0, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "UpdateBackgroundPos", "onupdatetarget", gameObject));
        iTween.ValueTo(gameObject, iTween.Hash("from", -180, "to", 0, "easetype", iTween.EaseType.easeOutBack, "onupdate", "UpdateOwlRotation", "onupdatetarget", gameObject));
        Common.fade.Show(Color.white, 0.4f, 1, ButtonClick);

        menuShow = true;
    }

    void UpdateBackgroundPos(float pos) {
        rectTransformImageBackground.anchoredPosition = new Vector2(rectTransformImageBackground.anchoredPosition.x, pos);
    }

    void UpdateOwlRotation(float rotation) {
        rectTransformImageOwlBackground.transform.eulerAngles = new Vector3(0, 0, rotation);
    }

    void MenuHide() {
        iTween.Stop(gameObject); // Leállítjuk a korábbi animációkat, ha még mennének

        // Elindítjuk az animációkat
        iTween.ValueTo(gameObject, iTween.Hash("from", rectTransformImageBackground.anchoredPosition.y, "to", rectTransformImageBackground.sizeDelta.y, "time", 0.5f, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "UpdateBackgroundPos", "onupdatetarget", gameObject));
        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", -180, "time", 0.5f, "easetype", iTween.EaseType.easeOutBack, "onupdate", "UpdateOwlRotation", "onupdatetarget", gameObject));
        Common.fade.Hide(0.5f);

        popUp.SetActive(false);
        menuShow = false;
    }

    void SetPopUpContent(bool showInfoPanel)
    {
        this.showInfoPanel = showInfoPanel;
        popUpInfo.SetActive(showInfoPanel);
        popUpBugReport.SetActive(!showInfoPanel);
    }


    public void ButtonClick(string buttonName)
    {
        Debug.Log(buttonName);

        switch (buttonName)
        {
            case C.Program.GameMenuPrevious:
            case C.Program.GameMenuNext:
            case C.Program.GameMenuExit:
                selectedButton = buttonName;
                if (buttonClick != null)
                    buttonClick(buttonName);

                if (menuShow)
                    MenuHide();
                break;

            case "GameMenuOwl":
                if (menuShow)
                    MenuHide();
                else
                    MenuShow();
                break;

            case "FadeClick":
                MenuHide();
                break;

            case "Info":
                popUp.SetActive(!popUp.activeSelf);
                break;

            case "Cancel":
                SetPopUpContent(true);
                break;

            case "Send":
                byte[] bytes = textureScreenShoot.EncodeToPNG();
                string base64 = "data:image/png;base64," + System.Convert.ToBase64String(bytes);

                //File.WriteAllBytes("D:/screenShoot.png", bytes);
                //File.WriteAllText("D:/screenshootBase64.txt", base64);

                ClassYServerCommunication.instance.sendBugReport(
                    errorDescriptionInputField.text,
                    base64,
                    gameDataJson,
                    (bool success, JSONNode response) =>
                    {
                        // Ha a hibaküldés sikeres volt erről értesítjük a felhasználót
                        if (success)
                        {
                            ErrorPanel.instance.Show(Common.languageController.Translate(C.Texts.BugSendSuccess), Common.languageController.Translate(C.Texts.Ok), callBack: (string buttonName) =>
                            {
                                ErrorPanel.instance.Hide(() =>
                                {
                                    SetPopUpContent(true);
                                    errorDescriptionInputField.text = "";
                                });
                            });
                        }
                    }
                    );

                break;

            case "BugReport":
                SetPopUpContent(false);
                break;
        }
    }

    public void ButtonClick2(Button button) {

    }

    public string DataProvider(string token)
    {
        if (gameDataJson != null)
        {
            switch (token)
            {
                case "OwnerValue" : return gameDataJson[C.JSONKeys.owner][C.JSONKeys.userName].Value; break;
                case "SubjectValue": return (gameDataJson[C.JSONKeys.curriculums] is JSONArray) ? gameDataJson[C.JSONKeys.curriculums][0][C.JSONKeys.courses][0][C.JSONKeys.topic][C.JSONKeys.subject][C.JSONKeys.name].Value : "-"; break;
                case "TopicValue": return (gameDataJson[C.JSONKeys.curriculums] is JSONArray) ? gameDataJson[C.JSONKeys.curriculums][0][C.JSONKeys.courses][0][C.JSONKeys.topic][C.JSONKeys.name].Value : "-"; break;
                case "CoursesValue": return (gameDataJson[C.JSONKeys.curriculums] is JSONArray) ? gameDataJson[C.JSONKeys.curriculums][0][C.JSONKeys.courses][0][C.JSONKeys.name].Value : "-"; break;
                case "CurriculumsValue": return (gameDataJson[C.JSONKeys.curriculums] is JSONArray) ? gameDataJson[C.JSONKeys.curriculums][0][C.JSONKeys.name].Value : "-"; break;
                case "GameNameValue": return gameDataJson[C.JSONKeys.name].Value; break;
            }
        }

        return $"({token}) Missing token";
    }
}
