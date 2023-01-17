using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryGiftItem : MonoBehaviour
{
    GameObject notification;
    GameObject link;
    GameObject count;
    Text textCount;

    Image image;

    Common.CallBack_In_String callBack;
    string buttonName;

    // Start is called before the first frame update
    void Awake()
    {
        notification = gameObject.SearchChild("ImageNotification").gameObject;
        link = gameObject.SearchChild("ImageLink").gameObject;
        count = gameObject.SearchChild("ImageCount").gameObject;
        textCount = gameObject.SearchChild("TextCount").GetComponent<Text>();

        image = gameObject.SearchChild("Image").GetComponent<Image>();
    }

    public void Initialize(CastleGameInventoryScreen.GiftData giftData, string buttonName, Common.CallBack_In_String callBack)
    {
        this.buttonName = buttonName;
        this.callBack = callBack;

        notification.SetActive(giftData.newGift);
        link.SetActive(giftData.giftType == CastleGameInventoryScreen.GiftType.InternetLink);
        count.SetActive(giftData.count > 1);
        textCount.text = giftData.count.ToString();

        // Megszerezzük az ajándék képét (minden ajándékhoz tartozik egy kép)
        EmailGroupPictureController.instance.GetPictureFromUploadsDir(giftData.image,
            (Sprite sprite) =>
            {
                image.sprite =  sprite;
                giftData.sprite = sprite;
            }
        );
    }

    public void ButtonClick()
    {
        callBack(buttonName);
    }
}
