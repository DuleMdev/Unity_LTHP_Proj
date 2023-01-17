using UnityEngine;
using System.Collections;

public class InfoPanelBase : MonoBehaviour {

    Common.CallBack_In_String callBack;

    // Use this for initialization
    void Awake()
    {
        // Bekapcsoljuk a panelen levő elemek gyökér elemét
        Common.SearchGameObject(gameObject, "Show").SetActive(true);

        SearchComponents();

        gameObject.SetActive(false);
    }


    virtual protected void SearchComponents() {

    }

    /// <summary>
    /// A panelt megjelenítjük.
    /// </summary>
    public void Show(Common.CallBack_In_String callBack)
    {
        this.callBack = callBack;

        ShowComponents();

        // Megkérjük a MenuInformation szkriptet, hogy jelenítse meg a felúgró ablakot.
        Common.menuInformation.Show(gameObject);
    }

    virtual protected void ShowComponents() {

    }

    /// <summary>
    /// Melyik gombot nyomták meg. A gombok ezt a metódust hívják ha rájuk kattintottak.
    /// </summary>
    /// <param name="buttonName">A megnyomott gomb neve.</param>
    public void ButtonClick(string buttonName)
    {
        // Csak akkor reagálunk a gombnyomásra, ha látható a panel teljesen
        if (Common.menuInformation.show)
        {
            callBack(buttonName);
        }
    }
}
