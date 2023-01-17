using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryRayPopUp : MonoBehaviour
{
    Image imageCharacter;
    InventoryGameItem gamePlay;
    Image imageGift;

    GameObject textPlayConfirmation;
    GameObject textPlayRejected;

    Text textGiftName;
    GameObject linkStuff;

    GameObject pergament;

    Text textMessage;

    // Start is called before the first frame update
    void Awake()
    {
        imageCharacter = gameObject.SearchChild("ImageCharacter").GetComponent<Image>();
        gamePlay = GetComponentInChildren<InventoryGameItem>(true);
        imageGift = gameObject.SearchChild("ImageGift").GetComponent<Image>();

        textPlayConfirmation = gameObject.SearchChild("TextPlayConfirmation").gameObject;
        textPlayRejected = gameObject.SearchChild("TextPlayRejected").gameObject;

        textGiftName = gameObject.SearchChild("TextGiftName").GetComponent<Text>();
        linkStuff = gameObject.SearchChild("LinkStuff").gameObject;

        pergament = gameObject.SearchChild("Pergament").gameObject;

        textMessage = gameObject.SearchChild("Text").GetComponent<Text>();
    }

    public void ShowCharacter(CastleGameInventoryScreen.GiftData giftData)
    {
        ShowSubPanel(ShowType.Character);

        imageCharacter.sprite = giftData.sprite;
    }

    public void ShowGamePlay(CastleGameInventoryScreen.GameData gameData, int coinsNumber)
    {
        ShowSubPanel(ShowType.Game);
        
        gamePlay.Initialize(gameData, "", null);

        bool buyIsPossible = coinsNumber >= gameData.price;

        textPlayConfirmation.SetActive(buyIsPossible);
        textPlayRejected.SetActive(!buyIsPossible);
    }

    public void ShowGift(CastleGameInventoryScreen.GiftData giftData)
    {
        ShowSubPanel(ShowType.Gift);

        imageGift.sprite = giftData.sprite;

        textGiftName.text = giftData.name;
        linkStuff.SetActive(giftData.giftType == CastleGameInventoryScreen.GiftType.InternetLink);
    }

    public void ShowPergament()
    {
        ShowSubPanel(ShowType.Pergament);
    }

    void ShowSubPanel(ShowType showType)
    {
        gameObject.SetActive(true);

        imageCharacter.gameObject.SetActive(false);
        gamePlay.gameObject.SetActive(false);
        imageGift.gameObject.SetActive(false);
        pergament.SetActive(false);

        imageCharacter.gameObject.SetActive(showType == ShowType.Character);
        gamePlay.gameObject.SetActive(showType == ShowType.Game);
        imageGift.gameObject.SetActive(showType == ShowType.Gift);
        pergament.SetActive(showType == ShowType.Pergament);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    enum ShowType
    {
        Character,
        Game,
        Gift,
        Pergament,
    }
}
