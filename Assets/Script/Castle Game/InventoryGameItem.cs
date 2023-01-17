using UnityEngine;
using UnityEngine.UI;

public class InventoryGameItem : MonoBehaviour
{
    ShowCoinsPiece showCoinsPiece;
    Image image;

    string buttonName;
    Common.CallBack_In_String callBack;

    // Start is called before the first frame update
    void Awake()
    {
        image = gameObject.SearchChild("GameIcon").GetComponent<Image>();
        showCoinsPiece = GetComponentInChildren<ShowCoinsPiece>();
    }

    public void Initialize(CastleGameInventoryScreen.GameData gameData, string buttonName, Common.CallBack_In_String  callBack)
    {
        this.buttonName = buttonName;
        this.callBack = callBack;

        if (string.IsNullOrWhiteSpace(buttonName))
        {
            // Ha nincs buttonName megadva, akkor a RayPopUp-ban szerepel ez az objektum. Ilyenkor beállítjuk a képet a játék képére
            image.sprite = gameData.sprite;
        }
        else
            // Ha meg van adva a buttonName, akkor a Kincstár játék listában szerepel ez az objektum.
            // Itt már alapból meg van adva a játék képe, amit elmentünk a játék adatok közé, hogy a felbukkanó RayPopUp-ba megutdjuk mutatni.
            gameData.sprite = image.sprite;

        /*
        if (gameData.sprite == null)
        {
            EmailGroupPictureController.instance.GetPictureFromUploadsDir(
                gameData.image,
                (Sprite sprite) =>
                {
                    image.sprite = sprite;
                    gameData.sprite = sprite;
                }
            );
        }
        else
            image.sprite = gameData.sprite;
        */

        showCoinsPiece.SetCount(gameData.price);
    }

    public void ButtonClick()
    {
        callBack(buttonName);
    }
}
