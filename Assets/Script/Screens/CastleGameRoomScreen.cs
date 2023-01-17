using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameRoomScreen : HHHScreen , IDataProvider
{
    static public JSONNode jsonUserInventoryData;
    static JSONNode statusData;
    static Common.CallBack callBack;

    public TextAsset statusDataAsset;

    PolyNav2D polyNav2D;
    PolyNavAgent polyNavAgent;
    PolyNavObstacle polyNavObstacle;

    Hero hero;

    GameObject notification;    //

    Animator chestAnimator;     //
    Image chestLight;           // A láda kivilágosításához
    Text chestPercent;          // A ládán az aktuális érték kiírásához

    GameObject gift;

    const int doorCount = 5;

    List<GameObject> ListOfOpenDoor;
    List<GameObject> ListOfCloseDoor;

    List<CastleGameInventoryScreen.GiftData> giftsInChest;

    ClickTarget clickTarget;    // Hova kattintott a felhasználó

    bool userInputEnabled = true;      // Csak egy bizonyos ideig engedjük a user inputot feldolgozni, az után automatikussá válik a hős mozgatása
    bool chestFull = false;     // A ládában ajándék található
    bool chestOpen = false;     // Kinyitották-e a ládát már ha van

    bool firstRun = true;  // A képernyőt most hoztuk létre és a ScreenShowStartCoroutine és a ScreenShowFinishCoroutine először fog futni

    // Start is called before the first frame update
    void Start()
    {
        polyNav2D = GetComponentInChildren<PolyNav2D>();
        polyNavAgent = GetComponentInChildren<PolyNavAgent>();
        polyNavObstacle = GetComponentInChildren<PolyNavObstacle>(true);

        hero = GetComponentInChildren<Hero>();

        notification = gameObject.SearchChild("Notification").gameObject;

        chestAnimator = gameObject.SearchChild("Chest").GetComponent<Animator>();
        chestLight = gameObject.SearchChild("Light").GetComponent<Image>();
        chestPercent = gameObject.SearchChild("ChestPercent").GetComponent<Text>();

        gift = gameObject.SearchChild("Reward").gameObject;

        // Összegyűjtjük a nyitott és csukott ajtókat
        ListOfCloseDoor = new List<GameObject>();
        ListOfOpenDoor = new List<GameObject>();
        for (int i = 1; i <= doorCount; i++)
        {
            ListOfCloseDoor.Add(gameObject.SearchChild($"DoorClose/DoorClose{i}"));
            ListOfOpenDoor.Add(gameObject.SearchChild($"DoorOpen/Door{i}"));
        }
    }

    override public IEnumerator ScreenShowStartCoroutine()
    {
        // Másodjára már nem kell futtatni ezt a metódust
        if (!firstRun)
        {
            polyNav2D.enabled = true;
            polyNavAgent.enabled = true;
            if (chestFull && !chestOpen)
                chestAnimator.Play("ChestIsFull");
            //ShowInventory();
            yield break;
        }

        if (statusData == null)
        {
            statusData = JSON.Parse(statusDataAsset.text);
        }

        // StatusData-ból kigyűjtjük a ládában található dolgokat egy GiftData tömbbe
        giftsInChest = new List<CastleGameInventoryScreen.GiftData>();
        giftsInChest.AddRange(CastleGameInventoryScreen.GiftData.GetList(statusData[C.JSONKeys.collectedFrameGameChests][C.JSONKeys.characters], CastleGameInventoryScreen.GiftType.Character, true));
        giftsInChest.AddRange(CastleGameInventoryScreen.GiftData.GetList(statusData[C.JSONKeys.collectedFrameGameChests][C.JSONKeys.treasures], CastleGameInventoryScreen.GiftType.UselessThing, true));

        // Kivonjuk az inventory-ból a ládában található ajándékokat
        for (int i = 0; i < giftsInChest.Count; i++)
            CastleGameInventoryScreen.inventoryData.RemoveGift(giftsInChest[i]);

        polyNav2D.enabled = true;
        yield return null;
        polyNavAgent.enabled = true;
        yield return null;

        // Meghatározzuk, hogy a ládában van-e ajándék
        chestFull = giftsInChest.Count > 0;
            //_statusData[C.JSONKeys.collectedFrameGameChests][C.JSONKeys.characters].Count +
            //_statusData[C.JSONKeys.collectedFrameGameChests][C.JSONKeys.treasures].Count > 0;

        if (chestFull)
        {
            chestLight.fillAmount = 0;
            chestPercent.transform.parent.gameObject.SetActive(false);
            polyNavObstacle.gameObject.SetActive(true); // enabled = true;
            chestAnimator.Play("ChestIsFull");
        }
        // Meghatározzuk, hogy mutatni kell-e a ládát
        else if (statusData[C.JSONKeys.nextChestValue].AsInt > 0)
        {
            polyNavObstacle.gameObject.SetActive(true); // enabled = true;

            int actValue = statusData[C.JSONKeys.disposableChestScore].AsInt;
            int maxValue = statusData[C.JSONKeys.nextChestValue].AsInt;

            chestLight.fillAmount = 1 - (actValue * 1f / maxValue);
            chestPercent.text = $"{actValue}/{maxValue}";
        }

        // Beállítjuk, hogy melyik metódust hívja meg ha a hős elérte a célját
        hero.Initialize(HeroInTheTarget);

        // Meghatározzuk a nyitott és csukott ajtókat
        int[] randomNumbers = Common.GetRandomNumbers(doorCount);
        int openedDoors = Common.random.Next(2) + 1;
        for (int i = 0; i < doorCount; i++)
        {
            // Az aktuális ajtó nyitva vagy zárva legyen
            bool open = openedDoors >= i;
            ListOfOpenDoor[randomNumbers[i]].SetActive(open);
            ListOfCloseDoor[randomNumbers[i]].SetActive(!open);
        }
    }

    override public IEnumerator ScreenShowFinishCoroutine()
    {
        // Másodjára már nem kell futtatni ezt a metódust
        if (firstRun)
            polyNavAgent.SetDestination(gameObject.SearchChild("StartPos").transform.position);

        firstRun = false;

        yield return null;
    }

    override public IEnumerator ScreenHideFinish()
    {
        //CastleGameInventoryScreen.instance.Hide();
        polyNav2D.enabled = false;
        polyNavAgent.enabled = false;
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && userInputEnabled)
        {
            // Megnézzük, hol nyomták le az egeret
            Transform t = (Transform)Common.GetComponentInPos(Camera.main.ScreenToWorldPoint(Input.mousePosition), "Transform");

            if (t)
            {
                Debug.Log(t.name);
                switch (t.name)
                {
                    case "Door1": 
                    case "Door2": 
                    case "Door3": 
                    case "Door4": 
                    case "Door5": 
                        clickTarget = ClickTarget.Door;
                        // Meghatározzuk az ajtóhoz tartozó célt
                        polyNavAgent.SetDestination(t.Find("Target").position);
                        break;

                    case "ChestCollider": 
                        clickTarget = ClickTarget.Chest;
                        // Meghatározzuk az ajtóhoz tartozó célt
                        polyNavAgent.SetDestination(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        break;

                    case "Inventory":
                        ShowInventory();
                        break;

                    default:
                        clickTarget = ClickTarget.Floor;
                        polyNavAgent.SetDestination(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                        break;
                }
            }
            else
            {
                clickTarget = ClickTarget.Floor;
                polyNavAgent.SetDestination(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
    }

    void ShowInventory()
    {
        clickTarget = ClickTarget.Inventory;

        userInputEnabled = false;

        Common.screenController.holdingActScreen = true;

        CastleGameInventoryScreen.Load(
            () => { 
                Common.screenController.ChangeScreen(C.Screens.CastleGameRoomScreen);
                userInputEnabled = true;
            } 
        );

        /*
        CastleGameInventoryScreen.instance.Show(() =>
        {
            CastleGameInventoryScreen.instance.Hide();
            userInputEnabled = true;
        });
        */
        // Kinyítjuk az inventory-t

        // Az időt leállítjuk

        // Várunk amíg az inventory-t bezárják
    }


    /// <summary>
    /// Ez lesz meghívva, ha a hős elérte a célt és megállt.
    /// </summary>
    /// <param name=""></param>
    public void HeroInTheTarget ()
    {
        // Megérkezett a célbe a hős
        switch (clickTarget)
        {
            case ClickTarget.Floor:
                break;
            case ClickTarget.Door:
                userInputEnabled = false;
                if (callBack != null)
                    callBack();
                break;
            case ClickTarget.Chest:
                if (chestFull && !chestOpen)
                    StartCoroutine(OpenChest());
                break;
        }
    }

    IEnumerator OpenChest()
    {
        chestOpen = true;
        userInputEnabled = false;

        // Lejátszuk a láda nyítás animációt
        chestAnimator.Play("Chest");
        // Várunk egy keveset amíg a láda kinyílik
        yield return new WaitForSeconds(1.1f);

        // A kapott tárgyakat a inventory-ba reptetjük
        for (int i = 0; i < giftsInChest.Count; i++)
        {
            // Létrehozunk egy ajándékot
            GameObject newGift = Instantiate(gift, gift.transform.parent);

            bool ready = false;
            // Megszerezzük az ajándék képét (minden ajándékhoz tartozik egy kép)
            EmailGroupPictureController.instance.GetPictureFromUploadsDir(giftsInChest[i].image,
                (Sprite sprite) =>
                {
                    newGift.GetComponent<Image>().sprite = sprite;
                    ready = true;
                }
            );

            while (!ready) { yield return null; }

            newGift.SetActive(true);
            StartCoroutine(WaitForGiftFlyFinish(giftsInChest[i]));

            // Várunk egy keveset, hogy az ajándékok egymás után jöjjenek
            yield return new WaitForSeconds(1.3f);
        }

        // Várunk amíg az utolsó ajándék is berepül az inventory-ba
        yield return new WaitForSeconds(1.5f);

        userInputEnabled = true;
    }

    // Ha az ajándék repülő animációja véget ért csinálunk egy-két dolgot
    IEnumerator WaitForGiftFlyFinish(CastleGameInventoryScreen.GiftData giftData)
    {
        yield return new WaitForSeconds(2.8f);

        // Hozzáadjuk az inventory-hoz az ajándékot
        CastleGameInventoryScreen.inventoryData.AddGift(giftData);

        // Az értesítés ikont bekapcsoljuk
        // Először kikapcsoljuk, mert lehet, hogy már más ajándék bekapcsolta, majd újra bekapcsoljuk, hogy újra lejátsza az előugrást
        notification.SetActive(false);
        yield return null;
        notification.SetActive(true);
    }


    // Ha másodjára mutatjuk meg a már korábban létrehozott CastleGameRoomScreen-t
    // A sima Load metódust a ServerPlay hívja meg ha az útvonal lejátszás közben meg kell mutatni a szobát
    // Ha a szobából elindítottunk egy játékot, akkor utána a Reload metódust kell meghívni, hogy a játék
    // után visszakerüljünk a szobába. Ilyenkor már nem kell megadni bizonyos adatokat, mert már meg vannak adva.
    static public void ReLoad()
    {
        Common.screenController.ChangeScreen(C.Screens.CastleGameRoomScreen);
        //Load(_statusData, _callBack);
    }

    static public void Load(JSONNode statusData, Common.CallBack callBack)
    {
        CastleGameRoomScreen.statusData = statusData;
        CastleGameRoomScreen.callBack = callBack;

        CastleGameInventoryScreen.DownloadInventoryData(
            (bool success) => 
            {
                // Átváltunk a kastély játék szoba képernyőjére
                Common.screenController.ChangeScreen(C.Screens.CastleGameRoomScreen);
            }
        );

        /*
        // Lekérdezzük a játék adatait
        ClassYServerCommunication.instance.GetUserInventory(
            (bool success, JSONNode response) =>
            {
                if (success)
                {
                    // Ha megjöttek az adatok, akkor inicializáljuk a játékot
                    jsonUserInventoryData = response[C.JSONKeys.answer];

                    // Átváltunk a kastély játék instrukciós képernyőjére
                    Common.screenController.ChangeScreen(C.Screens.CastleGameRoomScreen);
                }
            }
        );
        */
    }

    public void ButtonClick()
    {
        // Tovább gombra kattintottak
        userInputEnabled = false;
        if (callBack != null)
            callBack();
    }

    public string DataProvider(string token)
    {
        if (statusData == null)
        {
            statusData = JSON.Parse(statusDataAsset.text);
        }

        string result = null;

        switch (token)
        {
            case "pathScoreValue": result = Common.GroupingNumber(statusData[C.JSONKeys.learnRoutePathScore].AsInt); break;
            case "actLevelValue": result = statusData[C.JSONKeys.currentLevelNumber]; break;
            case "maxLevelValue": result = statusData[C.JSONKeys.possibleMaxLevelNumber]; break;
        }

        return result;
    }

    enum ClickTarget
    {
        Floor,
        Door,
        Chest,
        Inventory,
    }
}
