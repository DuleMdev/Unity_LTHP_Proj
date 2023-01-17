using UnityEngine;
using System.Collections;

public class CanvasBasic : HHHScreen
{
    GameObject canvas;

    // Use this for initialization
    new void Awake()
    {
        // Összeszedjük az állítandó componensekre a referenciákat
        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;
    }

    // A ScreenController hívja meg ezt a metódust ha ez lesz a következő kép amit meg kell mutatni. 
    // A játéknak alaphelyzetbe kell állítania magát, majd visszaszólni ha végzet
    override public IEnumerator InitCoroutine()
    {
        // Beállítjuk a szövegek szövegét a nyelvnek megfelelően

        // Más szükséges értékeket is beállítunk alapértékekre


        yield return null;
    }

    // Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController
    override public IEnumerator ScreenShowStartCoroutine()
    {
        canvas.SetActive(true);

        yield return null;
    }

    // Ha letiltják a gameObject-et, akkor letiltjuk a Canvast, hogy ha engedélyezik, akkor a Canvas ne legyen látható azonnal
    void OnDisable()
    {
        canvas.SetActive(false);
    }


    // A mentés gombra kattintottak
    public void ButtonClickSomething()
    {
        // Hiba ellenőrzés  ************************************************************



        Debug.Log("Megnyomták a ... gombot!");
        Common.screenController.ChangeScreen("CanvasScreenServerMenu");
    }
}
