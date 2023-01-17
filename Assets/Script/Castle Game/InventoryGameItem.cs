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
            // Ha nincs buttonName megadva, akkor a RayPopUp-ban szerepel ez az objektum. Ilyenkor be�ll�tjuk a k�pet a j�t�k k�p�re
            image.sprite = gameData.sprite;
        }
        else
            // Ha meg van adva a buttonName, akkor a Kincst�r j�t�k list�ban szerepel ez az objektum.
            // Itt m�r alapb�l meg van adva a j�t�k k�pe, amit elment�nk a j�t�k adatok k�z�, hogy a felbukkan� RayPopUp-ba megutdjuk mutatni.
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
