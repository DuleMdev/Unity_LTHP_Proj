using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Linq;

public class UIHLayoutUserResult : MonoBehaviour {

    [Tooltip("A játék adatainak kiírására való prefab")]
    public GameObject gameObjectGameResult;

    [Tooltip("A maximális világosítás a játékok pontszámainak kiírásánál")]
    public float maxLightening;     // A játékos egyes játékokban elért eredményei egyre világosabban vannak kirajzolva.

    List<UIPanelGameResult> listOfGameResult = new List<UIPanelGameResult>();
    UIPanelGameResult sumGamesResult;

    int[] playerResult;

    //CanvasScreenServerResult.PlayerResult playerResult;
    
    /// <summary>
    /// Beállítjuk, hogy honnan vegye a játékos adatait.
    /// </summary>
    /// <param name="playerResult">A játékos eredményeit tároló int tömb amit meg kell jeleníteni.</param>
    /// <param name="color">A játékos csoportjának a színe</param>
    public void Init(int[] playerResult, Color color) {
        this.playerResult = playerResult;

        // Létrehozzuk az összes pont kirajzolásához a gameObject-et
        sumGamesResult = Instantiate(gameObjectGameResult).GetComponent<UIPanelGameResult>();
        sumGamesResult.transform.SetParent(gameObject.transform, false);
        sumGamesResult.transform.localScale = Vector3.one;
        sumGamesResult.SetColor(color, maxLightening / (playerResult.Length + 1) * 1);

        // Létrehozza a szükséges darabszámú játék megjelenítőt
        for (int i = 0; i < playerResult.Length; i++)
        {
            UIPanelGameResult gameResult = Instantiate(gameObjectGameResult).GetComponent<UIPanelGameResult>();
            gameResult.transform.SetParent(gameObject.transform, false);
            gameResult.transform.localScale = Vector3.one;
            gameResult.SetColor(color, maxLightening / (playerResult.Length + 1) * i + 2);

            listOfGameResult.Add(gameResult);
        }
    }

    /// <summary>
    /// A játékos adatainak frissítése. Az Init metódusban megadott playerResult adatok
    /// alapján frissíti a kiírásokat.
    /// </summary>
    public void Refresh() {
        // Frissítjük az összesítést
        sumGamesResult.SetText(Common.languageController.Translate("sum"), playerResult.Sum().ToString());

        // Frissítjük a játékos eredményét a különböző játékokban
        for (int i = 0; i < playerResult.Length; i++)
        {
            listOfGameResult[i].SetText(Common.languageController.Translate("Game") + (i+1), playerResult[i].ToString());
        }
    }
}
