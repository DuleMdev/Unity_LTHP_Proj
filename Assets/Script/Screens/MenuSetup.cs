using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class MenuSetup : HHHScreen {

    Text textLabelServerIPAddress;
    InputField inputFieldServerIPAddress;

    Text textLabelServerPortNumber;
    InputField inputFieldServerPortNumber;

    Text textOkButton;
    Text textCancelButton;

    Text textLabelDeviceUniqueIdentifier;

    // Use this for initialization
    new void Awake()
    {
        // Bekapcsoljuk a Canvas-t mert az Editorban való átláthatóbb munka miatt ki vannak kapcsolva
        Common.SearchGameObject(gameObject, "Canvas").gameObject.SetActive(true);

        // Összeszedjük az állítandó componensekre a referenciákat
        textLabelServerIPAddress = Common.SearchGameObject(gameObject, "TextLabelServerIPAddress").GetComponent<Text>();
        inputFieldServerIPAddress = Common.SearchGameObject(gameObject, "InputFieldServerIPAddress").GetComponent<InputField>();

        textLabelServerPortNumber = Common.SearchGameObject(gameObject, "TextLabelServerPortNumber").GetComponent<Text>();
        inputFieldServerPortNumber = Common.SearchGameObject(gameObject, "InputFieldServerPortNumber").GetComponent<InputField>();

        textOkButton = Common.SearchGameObject(gameObject, "TextOkButton").GetComponent<Text>();
        textCancelButton = Common.SearchGameObject(gameObject, "TextCancelButton").GetComponent<Text>();

        textLabelDeviceUniqueIdentifier = Common.SearchGameObject(gameObject, "TextLabelDeviceUniqueIdentifier").GetComponent<Text>();
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator ScreenShowStartCoroutine()
    {
        Common.menuBackground.ChangeBackgroundIndex(0);

        Common.menuStripe.SetItem();

        textLabelServerIPAddress.text = Common.languageController.Translate("ServerIPaddress");
        textLabelServerPortNumber.text = Common.languageController.Translate("ServerPortNumber");
        textOkButton.text = Common.languageController.Translate("Ok");
        textCancelButton.text = Common.languageController.Translate("Cancel");

        // Beállítjuk az input mezők tartalmát
        inputFieldServerIPAddress.text = Common.configurationController.serverAddress;
        inputFieldServerPortNumber.text = Common.configurationController.portNumber.ToString();

        textLabelDeviceUniqueIdentifier.text = Common.configurationController.DeviceUID;

        yield return null;
    }

    /// <summary>
    /// A UI felületen lévő Button komponens hívja meg ha rákattintottak
    /// </summary>
    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Ok":
                Common.configurationController.serverAddress = inputFieldServerIPAddress.text;
                Common.configurationController.portNumber = System.Int32.Parse(inputFieldServerPortNumber.text);

                Common.configurationController.Save();

                Common.configurationController.DecideServerOrClient();
                break;
            case "Cancel":
                Common.configurationController.DecideServerOrClient();
                break;
        }
    }
}
