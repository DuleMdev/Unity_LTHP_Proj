using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

/*

Ez az objektum a lecke terv megjelenítő képernyőn található a játék gombok objektumain.

*/
public class MenuGameButton : MonoBehaviour {

    [System.Serializable]
    public class GameTypeAndIcon {
        public GameData.GameEngine gameType;
        public Sprite icon;
    }

    public List<GameTypeAndIcon> gameTypeList;

    Image imageBorder;    // A gomb körül megjelenő keret ha a gomb kiválasztva van
    Image imageGameIcon;          // A gombon megjelenő kép a játék képét tartalmazza

    int index;             // Hányadik az ikon az óra tervben
    Common.CallBack_In_Int buttonCallBack;  // Mit hívjon meg az objektum gombnyomáskor

    void Awake () {
        imageBorder = gameObject.GetComponent<Image>();
        imageGameIcon = Common.SearchGameObject(gameObject, "GameIcon").GetComponent<Image>();
    }

    public void Initialize(GameData gameData, int index, Common.CallBack_In_Int buttonCallBack) {

        // Megkeressük a játék típushoz tartozó képet és elhelyezzük a objektumban
        foreach (GameTypeAndIcon item in gameTypeList)
            if (item.gameType == gameData.gameEngine) {
                imageGameIcon.sprite = item.icon;
                break;
            }

        this.index = index;
        this.buttonCallBack = buttonCallBack;
    }

    /// <summary>
    /// Beállítja a gomb kiválasztóságát.
    /// Ha a gomb ki van választva, akkor van körülötte egy keret.
    /// </summary>
    /// <param name="selected">A gomb körül kell-e keretnek lennie.</param>
    public void Selected(bool selected) {
        imageBorder.enabled = selected;
    }

    /// <summary>
    /// A gameObject-en található Button szkript hívja meg ezt a metódust ha rákattintottak
    /// </summary>
    public void ButtonClick() {
        buttonCallBack(index);
    }
}
