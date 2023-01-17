using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//public class CastleGameInventory : MonoBehaviour , IDataProvider
//{
//    public static CastleGameInventory instance;
//
//    public Sprite[] uselessThings;
//
//    public Sprite[] characters;
//
//    Common.CallBack callBack;
//
//    static public InventoryData inventoryData;
//
//    ShowCoinsPiece showCoinsPiece;
//
//    GameObject giftPrefab;
//    GameObject characterPrefab;
//    GameObject gameKindPrefab;
//
//    GameObject contentCaharacters;
//    GameObject contentUselessThings;
//    GameObject contentGameKind;
//
//    InventoryRayPopUp rayPopUp;
//
//    List<InventoryGiftItem> listOfMadeCharacters = new List<InventoryGiftItem>();
//    List<InventoryGiftItem> listOfMadeUselessThings = new List<InventoryGiftItem>();
//    List<InventoryGameItem> listOfMadeGameKind = new List<InventoryGameItem>();
//
//    int actItemIndex;
//
//    // Start is called before the first frame update
//    void Awake()
//    {
//        instance = this;
//
//        showCoinsPiece = gameObject.SearchChild("ShowCoinsPiece").GetComponent<ShowCoinsPiece>();
//
//        giftPrefab = gameObject.SearchChild("GiftPrefab");
//        characterPrefab = gameObject.SearchChild("CharacterPrefab");
//        gameKindPrefab = gameObject.SearchChild("GameItem");
//
//        contentCaharacters = gameObject.SearchChild("Scroll View Characters/Content").gameObject;
//        contentUselessThings = gameObject.SearchChild("Scroll View UselessThings/Content").gameObject;
//        contentGameKind = gameObject.SearchChild("Scroll View Game Kind/Content").gameObject;
//
//        rayPopUp = GetComponentInChildren<InventoryRayPopUp>(true);
//
//        transform.position = Vector3.zero;
//
//        Hide();
//    }
//
//    public void Show(Common.CallBack callBack)
//    {
//        this.callBack = callBack;
//
//        gameObject.SetActive(true);
//        ShowItems();
//    }
//
//    public void Hide()
//    {
//        gameObject.SetActive(false);
//    }
//
//    /// <summary>
//    /// Kirajzoljuk az elemeket
//    /// </summary>
//    void ShowItems()
//    {
//        showCoinsPiece.SetCount(inventoryData.coinsNumber);
//
//        if (!inventoryData.changed)
//            return;
//
//        // Kitöröljük a korábban létrehozott tartalmakat
//        for (int i = 0; i < listOfMadeCharacters.Count; i++)
//            Destroy(listOfMadeCharacters[i].gameObject);
//        for (int i = 0; i < listOfMadeUselessThings.Count; i++)
//            Destroy(listOfMadeUselessThings[i].gameObject);
//        for (int i = 0; i < listOfMadeGameKind.Count; i++)
//            Destroy(listOfMadeGameKind[i].gameObject);
//
//        listOfMadeCharacters.Clear();
//        listOfMadeUselessThings.Clear();
//        listOfMadeGameKind.Clear();
//
//        // Létrehozzuk a karaktereket
//        for (int i = 0; i < inventoryData.listOfAvailableFrameGameCharacters.Count; i++)
//        {
//            InventoryGiftItem newGift = Instantiate(characterPrefab, contentCaharacters.transform).GetComponent<InventoryGiftItem>();
//            newGift.gameObject.SetActive(true);
//            newGift.Initialize(inventoryData.listOfAvailableFrameGameCharacters[i], $"character:{i}", ButtonClick);
//
//            listOfMadeCharacters.Add(newGift);
//        }
//
//        // Létrehozzuk az ajándékokat
//        for (int i = 0; i < inventoryData.ListOfAvailableFrameGameTreasure.Count; i++)
//        {
//            InventoryGiftItem newGift = Instantiate(giftPrefab, contentUselessThings.transform).GetComponent<InventoryGiftItem>();
//            newGift.gameObject.SetActive(true);
//            newGift.Initialize(inventoryData.ListOfAvailableFrameGameTreasure[i], $"useless:{i}", ButtonClick);
//
//            listOfMadeUselessThings.Add(newGift);
//        }
//
//        // Nem kell létrehozni a játékokat
//        // Létrehozzuk a játékokat
//        /*
//        for (int i = 0; i < inventoryData.ListOfGameDatas.Count; i++)
//        {
//            InventoryGameItem newGame = Instantiate(gameKindPrefab, contentGameKind.transform).GetComponent<InventoryGameItem>();
//            newGame.gameObject.SetActive(true);
//            newGame.Initialize(inventoryData.ListOfGameDatas[i], $"game:{i}", ButtonClick);
//
//            listOfMadeGameKind.Add(newGame);
//        }
//        */
//
//        // Bekapcsoljuk a megfelelő játékok láthatóságát
//        for (int i = 0; i < inventoryData.ListOfGameDatas.Count; i++)
//        {
//            Transform t = contentGameKind.transform.Find("Game" + inventoryData.ListOfGameDatas[i].id);
//
//            if (t == null)
//            {
//                Debug.LogError($"Nincs {inventoryData.ListOfGameDatas[i].id} azonosítóju játék");
//            }
//            else
//            {
//                t.gameObject.SetActive(true);
//                InventoryGameItem game = t.GetComponent<InventoryGameItem>();
//                game.Initialize(inventoryData.ListOfGameDatas[i], $"game:{i}", ButtonClick);
//            }
//        }
//
//
//        inventoryData.changed = false;
//    }
//    public void ButtonClick(string buttonName)
//    {
//        Debug.Log(buttonName);
//        string[] splittedButtonName = buttonName.Split(':');
//        int i = 0;
//        if (splittedButtonName.Length > 1)
//            int.TryParse(splittedButtonName[1], out i);
//
//        switch (splittedButtonName[0])
//        {
//            case "Inventory":
//                Hide();
//                callBack();
//                break;
//
//            case "character":
//                rayPopUp.ShowCharacter(inventoryData.listOfAvailableFrameGameCharacters[i]);
//                break;
//
//            case "game":
//                actItemIndex = i;
//                GameData gameData = inventoryData.ListOfGameDatas[i];
//                rayPopUp.ShowGamePlay(gameData, inventoryData.coinsNumber);
//                break;
//
//            case "useless":
//                actItemIndex = i;
//                rayPopUp.ShowGift(inventoryData.ListOfAvailableFrameGameTreasure[i]);
//                break;
//
//            case "Info":
//                rayPopUp.ShowPergament();
//                break;
//
//            case "RayPopUpClose":
//                rayPopUp.Hide();
//                break;
//
//            case "PlayGame":
//
//                // Mi történjen ha a játék befejeződik
//                Common.CallBack gameFinish = () =>
//                {
//                    Common.screenController.holdingActScreen = false;
//                    CastleGameRoomScreen.ReLoad();
//                };
//
//                Common.screenController.holdingActScreen = true;
//
//                // Elindítjuk a játékot
//                switch (actItemIndex)
//                {
//                    case 0:
//                        Ball_Game.BuyAGame(
//                            (bool success) =>
//                            {
//                                if (success)
//                                {
//                                    // Eltüntetjük a felugró ablakot a képernyőről
//                                    rayPopUp.Hide();
//
//                                    Ball_Game.Load(inventoryData.ListOfGameDatas[i].price, gameFinish);
//                                }
//                            }
//                        );
//
//                        break;
//                }
//
//                break;
//            case "OpenLink":
//                Application.OpenURL(inventoryData.ListOfAvailableFrameGameTreasure[i].extraURL);
//                break;
//            default:
//                break;
//        }
//    }
//
//    // Update is called once per frame
//    void Update()
//    {
//        if (inventoryData.changed)
//            ShowItems();
//    }
//
//    static public void DownloadInventoryData(Common.CallBack_In_Bool callBack)
//    {
//        // Lekérdezzük az inventory adatokat
//        ClassYServerCommunication.instance.GetUserInventory(
//            (bool success, JSONNode response) =>
//            {
//                if (success)
//                    inventoryData = new InventoryData(response[C.JSONKeys.answer]);
//
//                callBack(success);
//            }
//        );
//    }
//
//    public string DataProvider(string token)
//    {
//        switch (token)
//        {
//            case "allCoins": return inventoryData.coinsNumber.ToString(); break;
//            case "gameCoins": return inventoryData.ListOfGameDatas[actItemIndex].price.ToString();
//
//            default: return "unknown key: " + token; break;
//        }
//    }
//
//    public class InventoryData
//    {
//        public int coinsNumber;
//        public List<GiftData> listOfAvailableFrameGameCharacters;
//        public List<GiftData> ListOfAvailableFrameGameTreasure;
//        public List<GameData> ListOfGameDatas;
//
//        public bool changed;
//
//        public InventoryData(JSONNode json)
//        {
//            coinsNumber = json[C.JSONKeys.bonusCoins].AsInt;
//
//            listOfAvailableFrameGameCharacters = GiftData.GetList(json[C.JSONKeys.availableFrameGameCharacters]);
//            ListOfAvailableFrameGameTreasure = GiftData.GetList(json[C.JSONKeys.availableFrameGameTreasure], giftType: GiftType.UselessThing);
//            ListOfGameDatas = GameData.GetList(json[C.JSONKeys.bonusGames]);
//
//            changed = true;
//        }
//
//        public void AddGift(GiftData giftData)
//        {
//            if (giftData.giftType == GiftType.Character)
//                listOfAvailableFrameGameCharacters.Insert(0, giftData);
//            else
//                ListOfAvailableFrameGameTreasure.Insert(0, giftData);
//
//            changed = true;
//        }
//
//        public void RemoveGift(GiftData giftData)
//        {
//            if (giftData.giftType == GiftType.Character)
//                GiftData.Remove(listOfAvailableFrameGameCharacters, giftData);
//            else
//                GiftData.Remove(ListOfAvailableFrameGameTreasure, giftData);
//
//            changed = true;
//        }
//    }
//
//    public class GiftData
//    {
//        public string id { get; private set; }
//        public string userID { get; private set; }
//        public string name { get; private set; }
//        public string image { get; private set; }
//        public string extraURL { get; private set; }
//        public bool isDefault { get; private set; }
//        public bool newGift { get; private set; } // Mutatni kell az értesítést mivel új az ajándék
//
//        public GiftType giftType;
//        public int count { get; private set; }
//
//        public Sprite sprite;
//
//        public GiftData(JSONNode json, GiftType giftType = GiftType.Character, bool newGift = false)
//        {
//            count = 1;
//
//            id = json[C.JSONKeys.id];
//            userID = json[C.JSONKeys.userID];
//            name = json[C.JSONKeys.name];
//            image = json[C.JSONKeys.image];
//            extraURL = json[C.JSONKeys.extraURL];
//            isDefault = json[C.JSONKeys.isDefault].AsBool;
//
//            this.giftType = giftType;
//            this.newGift = newGift;
//
//            if (!string.IsNullOrWhiteSpace(extraURL))
//                    this.giftType = GiftType.InternetLink;
//        }
//
//        static public List<GiftData> GetList(JSONNode json, GiftType giftType = GiftType.Character, bool newGift = false)
//        {
//            List<GiftData> list = new List<GiftData>();
//
//            for (int i = 0; i < json.Count; i++)
//                list.Add(new GiftData(json[i], giftType, newGift));
//
//            return list;
//        }
//
//        static public void Add(List<GiftData> list, GiftData giftData)
//        {
//            // Megkeressük az ajándék azonosító alapján az ajándékot
//            GiftData gift = GetGift(list, giftData);
//
//            // Ha megtaláltuk akkor hozzá adunk egyet, ha nem találtuk meg létrehozzuk
//            if (gift != null)
//            {
//                list.Remove(gift);
//                gift.count++;
//            }
//            else
//                gift = giftData;
//
//            list.Add(gift);
//        }
//
//
//        static public void Remove(List<GiftData> list, GiftData giftData)
//        {
//            // Megkeressük az ajándék azonosító alapján az ajándékot
//            GiftData gift = GetGift(list, giftData);
//
//            // Ha megtaláltuk kivonjuk ha több van mint 1 vagy eltávolítjuk ha már csak egy van
//            if (gift != null)
//            {
//                if (gift.count > 1)
//                    gift.count--;
//                else
//                    list.Remove(gift);
//            }
//        }
//
//        static GiftData GetGift(List<GiftData> list, GiftData giftData)
//        {
//            for (int i = 0; i < list.Count; i++)
//                if (list[i].id == giftData.id)
//                    return list[i];
//
//            return null;
//        }
//    }
//
//    public enum GiftType
//    {
//        Character,
//        UselessThing,
//        InternetLink,
//    }
//
//    public class GameData
//    {
//        public string id { get; private set; }
//        public string name { get; private set; }
//        public int price { get; private set; }
//        public string image { get; private set; }
//
//        public Sprite sprite;
//        public GameData(JSONNode json)
//        {
//            id = json[C.JSONKeys.id];
//            name = json[C.JSONKeys.name];
//            price = json[C.JSONKeys.price].AsInt;
//            image = json[C.JSONKeys.image];
//        }
//
//        public static List<GameData> GetList(JSONNode json)
//        {
//            List<GameData> list = new List<GameData>();
//
//            for (int i = 0; i < json.Count; i++)
//                list.Add(new GameData(json[i]));
//
//            return list;
//        }
//    }
//}
