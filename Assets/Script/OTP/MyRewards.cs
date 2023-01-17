using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyRewards : MonoBehaviour
{
    ScrollRect scrollRect;

    GameObject prefabPCoin;

    RectTransform content;

    List<GameObject> madeGameObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        scrollRect = gameObject.SearchChild("Scroll View").GetComponent<ScrollRect>();

        prefabPCoin = gameObject.SearchChild("PCoin").gameObject;

        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();

        prefabPCoin.SetActive(false);
    }

    public void Initialize(int pCoin)
    {
        DrawList(pCoin);
    }

    // Kirajzolja a emailGroupList listában található tartalmat a korábbit pedig kitörli
    void DrawList(int pCoin)
    {
        // Kitöröljük a korábban létrehozott GameObject-eket
        for (int i = 0; i < madeGameObjects.Count; i++)
            Destroy(madeGameObjects[i]);

        madeGameObjects.Clear();

        // Létrehozzuk az új tartalmat
        float posX = -190;
        for (int i = 0; i < Mathf.Min(100, pCoin); i++)
        {
            // A pénzeket 5, 10 és 50 essével csoportosítjuk
            posX += 30;
            if (i % 5 == 0)
                posX += 30;
            if (i % 10 == 0)
                posX += 60;
            if (i % 50 == 0)
                posX += 120;

            // PCoin létrehozása
            GameObject newPCoin = Instantiate(prefabPCoin, content);
            newPCoin.SetActive(true);
            madeGameObjects.Add(newPCoin);

            newPCoin.GetComponent<PCoin>().Initialize("Play", ButtonClick);

            RectTransform newPrefabLabelRectTransform = newPCoin.GetComponent<RectTransform>();
            newPrefabLabelRectTransform.anchoredPosition = new Vector2(posX, 0);
        }

        // + 180 A zseton szélessége + 50 a zseton távolsága a végétől
        content.sizeDelta = new Vector2(posX + 180 + 50, 0);

        scrollRect.horizontalNormalizedPosition = 0;
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Play":
                // Biztonsági kérdés
                // Biztos felhasználod a PCoinodat

                // Ha igen, inddul a játék.
                Ball_Game.Load(1, () =>
                {
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                });
                Debug.Log("Play");
                break;
        }
    }

}
