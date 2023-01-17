using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasScreenServerConfig : HHHScreen {

    GameObject canvas;

    Text textPortNumber;            // A PortNumber szöveg kiírásához
    Text textMaxClientNumber;       // A Max Client Number szöveg kiírásához

    InputField inputFieldMaxClientNumber;    // A maximál kliens szám beállításához
    InputField inputFieldPortNumber;         // A port szám beállításához

    Text textButtonSave;            // A Save szöveg kiírásához
    Text textButtonCancel;          // A Cancel szöveg kiírásához

    GameObject goCancel;            // A Cancel gomb eltüntetéséhez


	// Use this for initialization
	new void Awake ()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        textPortNumber = Common.SearchGameObject(gameObject, "TextPortNumber").GetComponent<Text>();
        textMaxClientNumber = Common.SearchGameObject(gameObject, "TextMaxClientNumber").GetComponent<Text>();

        inputFieldMaxClientNumber = Common.SearchGameObject(gameObject, "InputFieldMaxClientNumber").GetComponent<InputField>();
        inputFieldPortNumber = Common.SearchGameObject(gameObject, "InputFieldPortNumber").GetComponent<InputField>();   

        textButtonSave = Common.SearchGameObject(gameObject, "TextButtonSave").GetComponent<Text>();
        textButtonCancel = Common.SearchGameObject(gameObject, "TextButtonCancel").GetComponent<Text>();   

        goCancel = Common.SearchGameObject(gameObject, "ButtonCancel").gameObject;  
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        textPortNumber.text = Common.languageController.Translate("Port number");
        textMaxClientNumber.text = Common.languageController.Translate("Maximum client number");

        inputFieldPortNumber.text = Common.configurationController.portNumber.ToString();
        inputFieldMaxClientNumber.text = Common.configurationController.maxConnectNumber.ToString();

        textButtonSave.text = Common.languageController.Translate("Save");
        textButtonCancel.text = Common.languageController.Translate("Cancel");

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
        Debug.Log("Client number : " + inputFieldMaxClientNumber.text);


        int portNumber = System.Convert.ToInt32(inputFieldPortNumber.text);
        int maxConnectNumber = System.Convert.ToInt32(inputFieldMaxClientNumber.text);

        if (portNumber > 65535)
        {
            Common.canvasInformation.ShowError("PortNumberError", () => {
                inputFieldPortNumber.Select();
                inputFieldPortNumber.ActivateInputField();
            });

            return;
        }

        if (maxConnectNumber > 99)
        {
            Common.canvasInformation.ShowError("ConnectNumberError", () => {
                inputFieldMaxClientNumber.Select();
                inputFieldMaxClientNumber.ActivateInputField();
            });

            return;
        }

        Common.configurationController.SetServerData(portNumber, maxConnectNumber);



        //Common.configurationController.portNumber = System.Convert.ToInt32(textSetPortNumber.text);
        //Common.configurationController.maxConnectNumber = System.Convert.ToInt32(textSetMaxClientNumber.text);

        Debug.Log("Save server setup!");
        Common.screenController.ChangeScreen("CanvasScreenServerMenu");
    }

    // A mégsem gombra kattintottak
    public void ButtonClickCancel()
    {
        Debug.Log("Cancel server setup!");
        Common.screenController.ChangeScreen("CanvasScreenServerMenu");
    }

    public void ChangePortNumber(string text) {
        string number = Common.StringToIntToString(inputFieldPortNumber.text).ToString();

        Debug.Log("Port : " + inputFieldPortNumber.text);

        if (inputFieldPortNumber.text != number) 
            inputFieldPortNumber.text = number;
    }

    public void ChangeClientNumber(string text) {
        string number = Common.StringToIntToString(inputFieldMaxClientNumber.text).ToString();

        Debug.Log("Client : " + inputFieldMaxClientNumber.text);

        if (inputFieldMaxClientNumber.text != number)
            inputFieldMaxClientNumber.text = number;
    }



}
