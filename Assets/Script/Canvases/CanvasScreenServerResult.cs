using UnityEngine;
using System.Collections;

using System.Collections.Generic;

using SimpleJSON;

/// <summary>
/// Ez az osztály mutatja a játékosok és a csapatok által elért eredményeket
/// és itt lehet az aktuális flow-t leállítani.
/// </summary>

public class CanvasScreenServerResult : HHHScreen {

    /// <summary>
    /// Egy játékos adatait tárolja. ( név, játékok eredményei )
    /// </summary>
    public class PlayerResult {
        public string name;         // A játékos neve
        public int[] result;        // A flow-on belüli játékok eredményei
        public int connectionID;    // A játékos kapcsolati azonosítója

        /// <summary>
        /// Létrehozza a játékos különböző játékokban elért eredményeinek tárolására való objektumokat.
        /// </summary>
        /// <param name="gameCount"></param>
        public PlayerResult(int gameCount) {
            result = new int[gameCount];
        }

        /// <summary>
        /// Kiszámolja, hogy összesen a játékos hány pontot gyűjtött a játékokban.
        /// </summary>
        /// <returns>A játékos által gyűjtött összes pont.</returns>
        public int GetSum() {
            int sum = 0;
            foreach (int point in result)
                sum += point;

            return sum;
        }
    }

    /// <summary>
    /// Egy adott csoport eredményét tárolja a játékos adatokkal együtt
    /// </summary>
    public class GroupResult {
        List<PlayerResult> playersResult;   // A játékosok eredményei a különböző játékokban

        int[] result;   // A játékosok játékonként elért össz pontszáma
        public int sum { get; private set; }        // Az összes elért pont a csoportban

        /// <summary>
        /// Létrehoz egy csoport objektumot. Létrehozáskor meg kell adni, hogy hány játék lesz összesen.
        /// </summary>
        /// <param name="gameCount">A játékok száma</param>
        public GroupResult(int gameCount) {
            playersResult = new List<PlayerResult>();
            result = new int[gameCount];
        }

        /// <summary>
        /// Összeadja a játékosok által elért pontszámokat játékonként.
        /// </summary>
        public void Sum() {
            sum = 0;
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = 0;
                foreach (PlayerResult playerResult in playersResult)
                {
                    result[i] += playerResult.result[i];
                    sum += playerResult.result[i];
                }
            }
        }

        /// <summary>
        /// Vissza adja, hogy a flow-ban a megadott indexű játékban a csoport mennyi pontot gyűjtöt
        /// </summary>
        /// <param name="gameIndex">Melyik játékban gyűjtött össz pontszámra vagyun kiváncsiak.</param>
        /// <returns>A vissza adott érték a megadott indexű játékban a csoport össz pontszáma.</returns>
        public int GetGameResult(int gameIndex) {
            int sum = 0;
            foreach (PlayerResult playerResult in playersResult)
            {
                sum += playerResult.result[gameIndex];
            }

            return sum;
        }

        /// <summary>
        /// Vissza adja az összes játékos által elért összes pontot.
        /// </summary>
        /// <returns>A játékosok össz pontszáma.</returns>
        public int GetSum() {
            int sum = 0;
            foreach (PlayerResult playerResult in playersResult)
            {
                sum += playerResult.GetSum();
            }

            return sum;
        }

        /// <summary>
        /// A megadott játékost a csoportba teszi és a pontszámai meg fognak jelenni a csoport adatokban.
        /// </summary>
        /// <param name="playerResult">Melyik játékos kerüljön a csoportba.</param>
        public void AddPlayer(PlayerResult playerResult) {
            playersResult.Add(playerResult);
        }

        /// <summary>
        /// Frissíti a megadott kapcsolat azonosítójú játákos elért pontszámát a megadott játékban
        /// </summary>
        /// <param name="connectionID">A játékos kapcsolat azonosítója.</param>
        /// <param name="gameIndex">A játék indexe.</param>
        /// <param name="point">A játékban elért új pontszám.</param>
        public void ReFreshPoint(int connectionID, int gameIndex, int point) {
            foreach (PlayerResult playerResult in playersResult)
            {
                if (playerResult.connectionID == connectionID)
                    playerResult.result[gameIndex] = point; 
            }
        }
    }

    /// <summary>
    /// Egy adott flow eredményeit tárolja a csoport adatokkal együtt
    /// </summary>
    public class FlowResult {

        List<GroupResult> groupList = new List<GroupResult>();

        /// <summary>
        /// Egy új csoportot ad a flow-hoz.
        /// </summary>
        /// <param name="groupResult">A csoport adatai.</param>
        public void Add(GroupResult groupResult) {
            groupList.Add(groupResult);
        }

        /// <summary>
        /// Frissíti a megadott kapcsolat azonosítójú játákos elért pontszámát.
        /// </summary>
        /// <param name="connectionID">A játékos kapcsolat azonosítója.</param>
        /// <param name="gameIndex">A játék indexe.</param>
        /// <param name="point">A játékban elért új pontszám.</param>
        public void ReFreshPoint(int connectionID, int gameIndex, int point)
        {
            foreach (GroupResult playerResult in groupList)
            {
                playerResult.ReFreshPoint(connectionID, gameIndex, point);
            }
        }
    }

    [Tooltip("A felhasználó nevének megjelenítését végző prefab")]
    public GameObject userNamePrefab;
    [Tooltip("A felhasználó eredményének megjelenítését végző prefab")]
    public GameObject userResultPrefab;
    [Tooltip("Egy csoportba tartozó emberek nevének megjelenítését végző prefab")]
    public GameObject groupUsersNamePrefab;
    [Tooltip("Egy csoportba tartozó emberek eredményének megjelenítését végző prefab")]
    public GameObject groupUsersResultPrefab;



    GameObject collectGroupNames;   // A gameObject ami a csoport neveit tartalmazza
    GameObject collectGroupResults; // A gameObject ami a csoport eredményeit tartalmazza

    GameObject canvas;

    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;

        collectGroupNames = Common.SearchGameObject(gameObject, "VLayoutGroupsName").gameObject;
        collectGroupResults = Common.SearchGameObject(gameObject, "VLayoutGroupsResult").gameObject;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően


        // Más szükséges értékeket is beállítunk alapértékekre



        // Létrehozzuk a játékosok csoportjait a játékosokkal együtt
        JSONNode node = JSONNode.Parse(Common.configurationController.tasksInJSON.text);
        node = node["flows"][Common.configurationController.selectedFlow];
        int questionNumber = node["task"].Count;

        FlowResult flowResult = new FlowResult();

        for (int i = 0; i < Server_Logic.groups.Count; i++)
        {
            GroupResult groupResult = new GroupResult(101);

            ClientGroup group = Server_Logic.groups[i];

            GameObject groupOfName = Instantiate(groupUsersNamePrefab);
            groupOfName.transform.SetParent(collectGroupNames.transform, false);
            groupOfName.transform.localScale = Vector3.one;

            GameObject groupOfResult = Instantiate(groupUsersResultPrefab);
            groupOfResult.transform.SetParent(collectGroupNames.transform, false);
            groupOfResult.transform.localScale = Vector3.one;

            foreach (ClientData clientData in group.listOfClientData)
            {
                UIPanelUserName userName = Instantiate(groupUsersNamePrefab).GetComponent<UIPanelUserName>();
                groupOfName.transform.SetParent(collectGroupNames.transform, false);
                groupOfName.transform.localScale = Vector3.one;

                UIHLayoutUserResult userResult = Instantiate(groupUsersResultPrefab).GetComponent<UIHLayoutUserResult>();
                groupOfName.transform.SetParent(collectGroupNames.transform, false);
                groupOfName.transform.localScale = Vector3.one;

                //userName.SetText(clientData.userName);
                if (clientData.groupID != -1)
                    userName.SetColor(Common.configurationController.groupColors[clientData.groupID]);




            }

        }

        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        canvas.SetActive(true);

        yield return null;
    }

    // Ha letiltják a gameObject-et, akkor letiltjuk a Canvast, hogy ha engedélyezik, akkor a Canvas ne legyen látható azonnal
    void OnDisable()
    {
        canvas.SetActive(false);
    }


    // A mentés gombra kattintottak
    public void ButtonClickSomething()
    {
        // Hiba ellenőrzés  ************************************************************



        Debug.Log("Megnyomták a ... gombot!");
        Common.screenController.ChangeScreen("CanvasScreenServerMenu");
    }
}

