using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

/*




*/

public class MenuLessonStudentList : MonoBehaviour {

    // Milyen szempont szerint legyen a lista rendezve
    /*
    public enum SortBy
    {
        Undefined,      // Nem meghatározott
        ID,             // Sorbarendezés azonosító alapján
        StudentName,    // Diák neve alapján
        Stars,          // Csillagok száma szerint
        Rank,           // Rangsor szerint
        Point,          // Pontszám szerint
        Progress        // teljesítés szerint
    }
    */

    [Tooltip("Sorok bázisának Y távolsága.")]
    public float distanceY;

    GameObject studentBarHeaderPrefab;    // A listában mindig jelen levő fejléc
    RectTransform content;

    //GameObject studentBarPrefab;        // 

    Text textWaiting;

    MenuStudentBar.SortBy sortBy = MenuStudentBar.SortBy.ID;                  // Melyik érték alapján rendezzük a student adatokat
    bool sortInAscendant = true;           // A sorbarendezés növekvően történjen

    List<MenuStudentBar> listOfMenuStudentBar = new List<MenuStudentBar>();

	// Use this for initialization
	void Awake () {

        content = (RectTransform)Common.SearchGameObject(gameObject, "Content").GetComponent<Transform>();

        studentBarHeaderPrefab = Common.SearchGameObject(gameObject, "StudentBar").gameObject;
        //listOfMenuStudentBar.Add(studentBarHeaderPrefab.GetComponent<MenuStudentBar>());

        textWaiting = Common.SearchGameObject(gameObject, "TextWaiting").GetComponent<Text>();
	}

    public void Initialize() {
        //studentBarHeader.InitializeHeader();

        textWaiting.text = Common.languageController.Translate("WaitStudentTabletConnect");

    }

    /// <summary>
    /// Beállítja az indexedik elemet a studentBarStatus által meghatározott típusura. Ha nincs indexedik, 
    /// akkor létrehozza azt. 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="studentBarStatus"></param>
    /// <param name="data"></param>
    public MenuStudentBar SetRow(int index) {

        MenuStudentBar studentBar;
        while (index > listOfMenuStudentBar.Count - 1) {
            // Egy új sort kell létrehozni
            studentBar = Instantiate(studentBarHeaderPrefab).GetComponent<MenuStudentBar>();
            studentBar.gameObject.SetActive(true);
            studentBar.transform.SetParent(content, false);
            studentBar.transform.localScale = Vector3.one;

            //studentBar.Initialize(Common.configurationController.classRoster[i].name, UIButtonStudentName.ButtonStatus.Selectable, ButtonClick);

            // Meghatározzuk a sor pozícióját
            studentBar.transform.localPosition = new Vector3(0, (listOfMenuStudentBar.Count + 1) * distanceY - 20);

            // A létrehozott új sort hozzá adjuk a listához
            listOfMenuStudentBar.Add(studentBar);
        }

        listOfMenuStudentBar[index].gameObject.SetActive(true);

        // Lekérdezzük az indexedik elemet és beállítjuk a típusát
        return listOfMenuStudentBar[index];
        //studentBar.Initialize (index, studentBarStatus, data, ButtonClick);
    }

    /// <summary>
    /// Kikapcsolja azokat a sorokat amelyekre nincs szükség.
    /// </summary>
    /// <param name="firstIndex">Az első sor amelyre nincs szükség.</param>
    public void DisableRows(int firstIndex) {

    }

    /// <summary>
    /// Frissíti a studentLista tartalmát
    /// </summary>
    void Refresh() {
        // Frissítjük a fejlécet
        studentBarHeaderPrefab.GetComponent<MenuStudentBar>().InitializeAsHeader(sortBy, sortInAscendant, ButtonClick);

        int index = 1;

        // Ha létre van már hozva a gameMaster objektum
        // Listázzuk a tanulókat
        switch (Common.gameMaster.gameMode)
        {
            // Egyszemélyes üzemmódok
            case GameMaster.GameMode.WarmUp:
            case GameMaster.GameMode.Single:
                // Sorbarendezzük a tanulókat a beállított szempont szerint
                List<int> indexes = Common.GetIntList(Common.gameMaster.listOfClients.Count);
                indexes = Sort(indexes, Common.gameMaster.listOfClients);

                // Beállítjuk a sorbarendezés szerint a megjelenítést
                for (int i = 0; i < indexes.Count; i++)
                {
                    MenuStudentBar studentBar = SetRow(i);
                    
                    studentBar.InitializeAsStudentData(Common.gameMaster.listOfClients[indexes[i]].tabletID , Common.gameMaster.listOfClients[indexes[i]], ButtonClick);
                    index++;
                }

                break;

            // Többszemélyes üzemmódok
            case GameMaster.GameMode.Multi_OneAfterAnother:
            case GameMaster.GameMode.Multi_Single:

                List<ClientData> listOfClients = new List<ClientData>();

                // Létrehozunk minden CliensGroup-hoz egy ClientData objektumot
                foreach (ClientGroup clientGroup in Common.gameMaster.listOfClientGroup)
                {
                    int tabletID = clientGroup.listOfClientData[SearchIndex(null, clientGroup.listOfClientData, MenuStudentBar.SortBy.ID, true)].tabletID;

                    ClientData clientData = new ClientData(0, tabletID, "");
                    clientData.name = Common.languageController.Translate(Common.configurationController.groupColorsName[clientGroup.groupNumber]) +
                        " " + Common.languageController.Translate(C.Texts.group);
                    clientData.starNumber = (int)Sum(clientGroup.listOfClientData, MenuStudentBar.SortBy.Stars);
                    clientData.rank = clientGroup.listOfClientData[SearchIndex(null, clientGroup.listOfClientData, MenuStudentBar.SortBy.Rank, sortInAscendant)].rank;
                    clientData.point = (int)Sum(clientGroup.listOfClientData, MenuStudentBar.SortBy.Point);

                    if (clientGroup.listOfClientData.Count > 0) {
                        clientData.mosaic = clientGroup.listOfClientData[0].mosaic;
                        clientData.game = clientGroup.listOfClientData[0].game;
                        clientData.screen = clientGroup.listOfClientData[0].screen;
                    }

                    clientData.groupID = clientGroup.groupNumber;

                    listOfClients.Add(clientData);
                }

                // sorba rendezzük a csoport fejléceket
                List<int> sorted = Sort(null, listOfClients);

                // A sorba rendezésnek megfelelően létrehozzuk a csoportokat és a csoportokon belüli pedig a klienseket
                for (int i = 0; i < sorted.Count; i++)
                {
                    ClientData clientData = listOfClients[sorted[i]];

                    ClientGroup clientGroup = Common.gameMaster.listOfClientGroup[sorted[i]];

                    // Létrehozzuk a csoport fejlécet
                    MenuStudentBar studentBar = SetRow(index - 1);

                    studentBar.InitializeAsGroupHeader(clientData);
                    index++;

                    // A csoportba tartozó klienseket sorba rendezzük
                    List<int> sorted2 = Sort(null, clientGroup.listOfClientData);

                    // Létrehozzuk a csoportba tartozó klienseket
                    for (int j = 0; j < sorted2.Count; j++)
                    {
                        studentBar = SetRow(index - 1);

                        studentBar.InitializeAsStudentData(clientGroup.listOfClientData[sorted2[j]].tabletID, clientGroup.listOfClientData[sorted2[j]], ButtonClick);
                        index++;
                    }
                }

                break;
        }

        // Beállítjuk a content méretét a használt studentBar-ok alapján
        content.sizeDelta = new Vector2(content.sizeDelta.x, -distanceY * index + 20);
        index--;

        // Eltüntetjük a listából a nem használt studentBar-okat
        while (index < listOfMenuStudentBar.Count)
        {
            listOfMenuStudentBar[index].gameObject.SetActive(false);
            index++;
        }



    }

    // Update is called once per frame
    void Update () {
        // A várakozás a tanulók csatlakozására szöveg csak akkor látszik ha még egy tanuló sem csatlakozott

        textWaiting.enabled = true;
        if (Common.gameMaster != null)
        {
            textWaiting.enabled = Common.gameMaster.listOfClients.Count == 0;

            Refresh();
        }
    }

    /// <summary>
    /// Megkeresi a gameMaster.listOfClient listában a magadott indexek közül a sortBy és sortInAscendant szerinti következőt.
    /// </summary>
    /// <param name="indexes">Az indexek listája.</param>
    /// <returns>A vissza adott érték a következő index</returns>
    int SearchIndex(List<int> indexes, List<ClientData> listOfClients, MenuStudentBar.SortBy sortBy, bool sortInAscendant)
    {
        if (indexes == null)
            indexes = Common.GetIntList(listOfClients.Count);

        int bestIndex = 0;

        for (int i = 1; i < indexes.Count; i++)
        {
            bool actIndexIsBetter = false;

            switch (sortBy)
            {
                case MenuStudentBar.SortBy.ID:
                    actIndexIsBetter = (sortInAscendant) ? listOfClients[indexes[i]].tabletID < listOfClients[indexes[bestIndex]].tabletID : listOfClients[indexes[i]].tabletID > listOfClients[indexes[bestIndex]].tabletID;
                    break;
                case MenuStudentBar.SortBy.StudentName:
                    actIndexIsBetter = (sortInAscendant) ? string.Compare(listOfClients[indexes[i]].name, listOfClients[indexes[bestIndex]].name) == -1 : string.Compare(listOfClients[indexes[i]].name, listOfClients[indexes[bestIndex]].name) == 1;
                    break;
                case MenuStudentBar.SortBy.Stars:
                    actIndexIsBetter = (sortInAscendant) ? listOfClients[indexes[i]].starNumber < listOfClients[indexes[bestIndex]].starNumber : listOfClients[indexes[i]].starNumber > listOfClients[indexes[bestIndex]].starNumber;
                    break;
                case MenuStudentBar.SortBy.Rank:
                    actIndexIsBetter = (sortInAscendant) ? listOfClients[indexes[i]].rank < listOfClients[indexes[bestIndex]].rank : listOfClients[indexes[i]].rank > listOfClients[indexes[bestIndex]].rank;
                    break;
                case MenuStudentBar.SortBy.Point:
                    actIndexIsBetter = (sortInAscendant) ? listOfClients[indexes[i]].point < listOfClients[indexes[bestIndex]].point : listOfClients[indexes[i]].point > listOfClients[indexes[bestIndex]].point;
                    break;
                case MenuStudentBar.SortBy.Progress:
                    if (listOfClients[indexes[i]].mosaic == listOfClients[indexes[bestIndex]].mosaic)
                        actIndexIsBetter = (sortInAscendant) ? listOfClients[indexes[i]].mosaic < listOfClients[indexes[bestIndex]].mosaic : listOfClients[indexes[i]].mosaic > listOfClients[indexes[bestIndex]].mosaic;
                    else
                        actIndexIsBetter = (sortInAscendant) ? listOfClients[indexes[i]].game < listOfClients[indexes[bestIndex]].game : listOfClients[indexes[i]].game > listOfClients[indexes[bestIndex]].game;
                    break;
            }
            
            if (actIndexIsBetter)
                bestIndex = i;
        }

        return bestIndex;
    }

    float Sum(List<ClientData> listOfClients, MenuStudentBar.SortBy sortBy) {

        float result = 0;

        for (int i = 0; i < listOfClients.Count; i++)
        {
            switch (sortBy)
            {
                case MenuStudentBar.SortBy.ID:
                    result += listOfClients[i].tabletID;
                    break;
                case MenuStudentBar.SortBy.Stars:
                    result += listOfClients[i].starNumber;
                    break;
                case MenuStudentBar.SortBy.Rank:
                    result += listOfClients[i].rank;
                    break;
                case MenuStudentBar.SortBy.Point:
                    result += listOfClients[i].point;
                    break;
            }
        }

        return result;
    }

    float Avarage(List<ClientData> listOfClients, MenuStudentBar.SortBy sortBy) {
        return Sum(listOfClients, sortBy) / listOfClients.Count;
    }

    /// <summary>
    /// Sorbarendezi az indexes listában megadott indexű clientData objektumokat a listOfClients listában.
    /// </summary>
    List<int> Sort(List<int> indexes, List<ClientData> listOfClients)
    {
        if (indexes == null)
            indexes = Common.GetIntList(listOfClients.Count);

        List<int> result = new List<int>();

        for (int i = indexes.Count - 1; i >= 0; i--)
        {
            int bestIndex = SearchIndex(indexes, listOfClients, sortBy, sortInAscendant);
            result.Add(indexes[bestIndex]);
            indexes.RemoveAt(bestIndex);
        }

        return result;
    }

    /// <summary>
    /// A listOfMenuStudentBar listában található MenuStudentBar objektumok hívják ezt ha rájuk kattintottak.
    /// </summary>
    /// <param name="index">Mi a tabletID-ja a kattintott studentBar-nak.</param>
    /// <param name="buttonName">A student báron megnyomott gomb neve.</param>
    public void ButtonClick(int index, string buttonName)
    {
        Debug.Log(index.ToString() + " : " + buttonName);

        // Ha -1 az index, akkor Header
        if (index == -1)
        {
            switch (buttonName)
            {
                case "Disconnect":
                    break;
                case "Progress":
                    if (sortBy == MenuStudentBar.SortBy.Progress) {
                        sortInAscendant = !sortInAscendant;
                    } else {
                        sortBy = MenuStudentBar.SortBy.Progress;
                        sortInAscendant = true;
                    }
                    break;

                case "Point":
                    if (sortBy == MenuStudentBar.SortBy.Point) {
                        sortInAscendant = !sortInAscendant;
                    } else {
                        sortBy = MenuStudentBar.SortBy.Point;
                        sortInAscendant = true;
                    }
                    break;

                case "Rank":
                    if (sortBy == MenuStudentBar.SortBy.Rank) {
                        sortInAscendant = !sortInAscendant;
                    } else {
                        sortBy = MenuStudentBar.SortBy.Rank;
                        sortInAscendant = true;
                    }
                    break;

                case "Stars":
                    if (sortBy == MenuStudentBar.SortBy.Stars) {
                        sortInAscendant = !sortInAscendant;
                    } else {
                        sortBy = MenuStudentBar.SortBy.Stars;
                        sortInAscendant = true;
                    }
                    break;

                case "StudentName":
                    if (sortBy == MenuStudentBar.SortBy.StudentName) {
                        sortInAscendant = !sortInAscendant;
                    } else {
                        sortBy = MenuStudentBar.SortBy.StudentName;
                        sortInAscendant = true;
                    }
                    break;

                case "ID":
                    if (sortBy == MenuStudentBar.SortBy.ID) {
                        sortInAscendant = !sortInAscendant;
                    } else {
                        sortBy = MenuStudentBar.SortBy.ID;
                        sortInAscendant = true;
                    }
                    break;
            }
        }
        else
        {
            // Egy tanuló során kattintottak
            //Server_ClientData clientData = Common.gameMaster.listOfClients[index];

            if (buttonName == "StudentName") {


            }

            switch (buttonName)
            {
                case "Disconnect":
                    ClientData clientData = Common.gameMaster.GetClientDataBytabletID(index);
                    Common.gameMaster.RemoveClient(clientData);
                    //Common.gameMaster.listOfClients.Remove(clientData);
                    Common.HHHnetwork.DisconnectClient(clientData.connectionID);
                    break;
                case "Progress":
                    break;
                case "Point":
                    break;
                case "Rank":
                    break;
                case "Stars":
                    break;
                case "StudentName":
                    Common.infoPanelClassRoster.MakeButtons();
                    //Common.infoPanelClassRoster.Show(listOfMenuStudentBar[index].clientData.tabletID, (string bName) => {
                    Common.infoPanelClassRoster.Show(index, (string bName) => {
                        switch (bName)
                        {
                            case "Exit":
                                Common.menuInformation.Hide();
                                break;
                            default:
                                break;
                        }
                    });
                    break;
                case "ID":
                    break;
            }
        }
    }
}
