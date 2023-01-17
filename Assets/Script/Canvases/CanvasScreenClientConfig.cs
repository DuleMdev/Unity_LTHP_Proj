using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasScreenClientConfig : HHHScreen
{

    GameObject canvas;

    // Szövegek kiírásához
    Text textServerAddress;         // A szerver címe szöveg  kiírásához
    Text textPortNumber;            // A PortNumber szöveg kiírásához
    Text textTabletID;              // A tablet azonosító szöveg kiírásához
    Text textOrderNumber;           // A sorszám szöveg kiírásához
    Text textButtonSave;            // A Save szöveg kiírásához
    Text textButtonCancel;          // A Cancel szöveg kiírásához

    // Beírt input adatok megszerzéséhez
    InputField inputFieldServerAddress;     // A szerver cím beállításához
    InputField inputFieldPortNumber;        // A port szám beállításához
    InputField inputFieldTabletID;          // A tablet azonosító beállításához
    InputField inputFieldOrderNumber;       // A tablet sorszámának beállításához

    GameObject goCancel;            // A Cancel gomb eltüntetéséhez


    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        textServerAddress = Common.SearchGameObject(gameObject, "TextServerAddress").GetComponent<Text>();
        textPortNumber = Common.SearchGameObject(gameObject, "TextPortNumber").GetComponent<Text>();
        textTabletID = Common.SearchGameObject(gameObject, "TextTabletID").GetComponent<Text>();
        textOrderNumber = Common.SearchGameObject(gameObject, "TextOrderNumber").GetComponent<Text>();
        textButtonSave = Common.SearchGameObject(gameObject, "TextButtonSave").GetComponent<Text>();
        textButtonCancel = Common.SearchGameObject(gameObject, "TextButtonCancel").GetComponent<Text>();

        inputFieldServerAddress = Common.SearchGameObject(gameObject, "InputFieldServerAddress").GetComponent<InputField>();
        inputFieldPortNumber = Common.SearchGameObject(gameObject, "InputFieldPortNumber").GetComponent<InputField>();
        inputFieldTabletID = Common.SearchGameObject(gameObject, "InputFieldTabletID").GetComponent<InputField>();
        inputFieldOrderNumber = Common.SearchGameObject(gameObject, "InputFieldOrderNumber").GetComponent<InputField>();

        goCancel = Common.SearchGameObject(gameObject, "ButtonCancel").gameObject;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a kiválasztott nyelvnek megfelelően a szövegeket
        textServerAddress.text = Common.languageController.Translate("Server address");
        textPortNumber.text = Common.languageController.Translate("Port number");
        textTabletID.text = Common.languageController.Translate("Tablet ID");
        textOrderNumber.text = Common.languageController.Translate("Order number");
        textButtonSave.text = Common.languageController.Translate("Save");
        textButtonCancel.text = Common.languageController.Translate("Cancel");

        inputFieldServerAddress.text = Common.configurationController.serverAddress.ToString();
        inputFieldPortNumber.text = Common.configurationController.portNumber.ToString();
        inputFieldTabletID.text = Common.configurationController.tabletID.ToString();
        inputFieldOrderNumber.text = Common.configurationController.clientID.ToString();

        goCancel.SetActive(true);
        //goCancel.SetActive(Common.configurationController.tabletType != ConfigurationController.TabletType.Undefined);

        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        canvas.SetActive(true);

        yield return null;
    }

    void OnDisable()
    {
        canvas.SetActive(false);
    }

    // A mentés gombra kattintottak
    public void ButtonClickSave()
    {
        // Hiba ellenőrzés  ************************************************************

        Debug.Log("Port number : " + inputFieldPortNumber.text);
        Debug.Log("Order number : " + inputFieldOrderNumber.text);

        int portNumber = System.Convert.ToInt32(inputFieldPortNumber.text);
        int orderNumber = System.Convert.ToInt32(inputFieldOrderNumber.text);

        if (portNumber > 65535)
        {
            Common.canvasInformation.ShowError("PortNumberError", () => {
                inputFieldPortNumber.Select();
                inputFieldPortNumber.ActivateInputField();
            });

            return;
        }

        if (orderNumber > 99)
        {
            Common.canvasInformation.ShowError("OrderNumberError", () => {
                inputFieldOrderNumber.Select();
                inputFieldOrderNumber.ActivateInputField();
            });

            return;
        }

        Common.configurationController.SetClientData(inputFieldServerAddress.text, portNumber, inputFieldTabletID.text, orderNumber);

        //Common.configurationController.portNumber = System.Convert.ToInt32(textSetPortNumber.text);
        //Common.configurationController.maxConnectNumber = System.Convert.ToInt32(textSetMaxClientNumber.text);

        Debug.Log("Save client setup!");
        Common.screenController.ChangeScreen("CanvasScreenClientMenu");
    }

    // A mégsem gombra kattintottak
    public void ButtonClickCancel()
    {
        Debug.Log("Cancel client setup!");
        Common.screenController.ChangeScreen("CanvasScreenClientMenu");
    }

    public void ChangePortNumber(string text)
    {
        string number = Common.StringToIntToString(inputFieldPortNumber.text).ToString();

        Debug.Log("Port : " + inputFieldPortNumber.text);

        if (inputFieldPortNumber.text != number)
            inputFieldPortNumber.text = number;
    }

    public void ChangeClientNumber(string text)
    {
        string number = Common.StringToIntToString(inputFieldOrderNumber.text).ToString();

        Debug.Log("Client : " + inputFieldOrderNumber.text);

        if (inputFieldOrderNumber.text != number)
            inputFieldOrderNumber.text = number;
    }



}
