using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class InfoPanelClassRoster : InfoPanelBase
{
    [Tooltip("A gombok vízszintes távolsága")]
    public float distanceWidth;
    [Tooltip("A gombok függőleges távolsága")]
    public float distanceHeight;

    GameObject buttonNamePrefab; // Előregyártott gomb komponens
    RectTransform content;

    List<UIButtonStudentName> listOfButtonName = new List<UIButtonStudentName>();

    public int selectedStudentID;   // A kiválasztott tanuló azonosítója

    int tabletID;       // Mi a tablet azonosítója amihez tanulót akarunk rendelni
    ClientData clientData;   // Melyik klienshez akarunk nevet rendelni

    override protected void SearchComponents()
    {
        Common.infoPanelClassRoster = this;

        content = (RectTransform)Common.SearchGameObject(gameObject, "Content").GetComponent<Transform>();

        buttonNamePrefab = Common.SearchGameObject(gameObject, "ButtonStudentName").gameObject;
        buttonNamePrefab.SetActive(false); // Kikapcsoljuk a prefab láthatóságát
    }

    
    public void Show(int tabletID, Common.CallBack_In_String callBack)
    {
        this.tabletID = tabletID;

        // Megkeressük az azonosító alapján a klienst adatait (Ennek muszáj léteznie, hiszen ehhez akarunk tanulót rendelni)
        clientData = Common.gameMaster.GetClientDataBytabletID(tabletID);

        Show(callBack);
    }
    

    /// <summary>
    /// Eltávolítja a korábban létrehozott gombokat.
    /// </summary>
    public void RemoveButtons()
    {
        foreach (UIButtonStudentName item in listOfButtonName)
            Destroy(item.gameObject);

        listOfButtonName.Clear();
    }

    /// <summary>
    /// Létrehozzuk az osztálynévsorban található nevek számára a gombokat.
    /// </summary>
    public void MakeButtons()
    {
        RemoveButtons();

        // Létrehozzuk a szükséges számú gombot
        int i;
        for (i = 0; i < Common.configurationController.classRoster.Count; i++)
        {
            UIButtonStudentName button = Instantiate(buttonNamePrefab).GetComponent<UIButtonStudentName>();
            button.gameObject.SetActive(true);
            button.transform.SetParent(content, false);
            button.transform.localScale = Vector3.one;
            button.Initialize(Common.configurationController.classRoster[i].id, Common.configurationController.classRoster[i].name, UIButtonStudentName.ButtonStatus.Selectable, ButtonClick);

            button.transform.localPosition = new Vector3(7 + i % 3 * distanceWidth, -7 - i / 3 * distanceHeight);

            listOfButtonName.Add(button);
        }

        // Beállítjuk a tartalmazó panel méretét
        content.sizeDelta = new Vector2(content.sizeDelta.x, ((i - 1) / 3 + 1) * distanceHeight);

        SetButtonsColor();
    }

    /// <summary>
    /// Beállítja a gombok szinét a kiválasztott osztálynak megfelelően.
    /// </summary>
    void SetButtonsColor()
    {
        // Beállítjuk a gombok kiválasztottságát
        // Lekérdezzük az osztály listát

        // Minden gombra beállítjuk, hogy választható
        foreach (UIButtonStudentName button in listOfButtonName)
            button.SetButtonState(UIButtonStudentName.ButtonStatus.Selectable);

        // Végig megyünk a kliensek listáján és amelyik klienshez már van név rendelve azt beállítjuk választottá
        foreach (ClientData clientData in Common.gameMaster.listOfClients)
        {
            if (clientData.studentData != null) {
                UIButtonStudentName button = GetButtonByStudentID(clientData.studentData.id);
                button.SetButtonState((this.clientData == clientData) ? UIButtonStudentName.ButtonStatus.Actual : UIButtonStudentName.ButtonStatus.Selected);
            }
        }
    }

    /// <summary>
    /// Vissza adja a megadott tanulóID-hoz tartozó gombot.
    /// </summary>
    /// <param name="studentID">Ezzel az azonosítóval rendelkező tanulóhoz tartozó gombot keressük.</param>
    /// <returns>A tanulóID-hez tartozó gomb.</returns>
    UIButtonStudentName GetButtonByStudentID(int studentID) {
        foreach (UIButtonStudentName button in listOfButtonName)
            if (button.id == studentID)
                return button;

        return null;
    }

    void SendNameToClient() {
        JSONClass answerNode = new JSONClass();
        answerNode[C.JSONKeys.dataContent] = C.JSONValues.clientID;
        answerNode[C.JSONKeys.clientID].AsInt = clientData.tabletID;
        answerNode[C.JSONKeys.clientName] = (clientData.studentData != null) ? clientData.studentData.name : "";

        // Elküldjük a kliensnek az adatokat
        Common.HHHnetwork.SendMessage(clientData.connectionID, answerNode);
    }


    /// <summary>
    /// Melyik gombot nyomták meg. A gombok ezt a metódust hívják ha rájuk kattintottak.
    /// </summary>
    /// <param name="buttonName">A megnyomott gomb neve.</param>
    new public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Exit":
                base.ButtonClick(buttonName);

                break;

            default:
                // Megkeressük a kattintott osztály azonosítóját
                UIButtonStudentName button = GetButtonByStudentID(int.Parse(buttonName));

                switch (button.buttonStatus)
                {
                    case UIButtonStudentName.ButtonStatus.Selectable:
                        this.clientData.studentData = Common.configurationController.classRoster.GetStudentDataByStudentID(int.Parse(buttonName));
                        this.clientData.studentData.tabletUniqueIdentifier = this.clientData.uniqueIdentifier;
                        SetButtonsColor();
                        SendNameToClient();

                        break;

                    case UIButtonStudentName.ButtonStatus.Selected:
                        break;

                    case UIButtonStudentName.ButtonStatus.Actual:
                        this.clientData.studentData.tabletUniqueIdentifier = "";
                        this.clientData.studentData = null;
                        /*
                        Server_ClientData clientData = Common.gameMaster.GetClientDataByStudentID(int.Parse(buttonName));
                        if (clientData != null)
                            clientData.studentData = null;
                            */
                        SetButtonsColor();
                        SendNameToClient();

                        break;
                }

                //SetSelectedClassID(int.Parse(buttonName));
                //SetButtonsColor();
                break;
        }
    }



    override protected void ShowComponents()
    {
        SetButtonsColor();





        /*

        textInformation.text = Common.languageController.Translate(information);


        textOkButton.text = Common.languageController.Translate("Ok");
        textOkButton.transform.parent.gameObject.SetActive(okButton); // Be / Ki kapcsoljuk az Ok gombot attól függően, hogy kell vagy nem kell
        */
    }

}