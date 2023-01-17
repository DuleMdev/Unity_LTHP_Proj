using UnityEngine;
using System.Collections;

using SimpleJSON;

public class HHHScreen : MonoBehaviour
{
    /// <summary>
    /// Felkészülünk a feladat megmutatására.
    /// </summary>
    /// <returns></returns>
    virtual public IEnumerator PrepareTask()
    {
        yield break;
    }

    /// <summary>
    /// Mielőtt a képernyő láthatóvá válna a ScreenController meghívja ezt a metódust, hogy inicializája magát. 
    /// </summary>
    /// <returns></returns>
    virtual public IEnumerator InitCoroutine()
    {
        yield break;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    virtual public IEnumerator ScreenShowStartCoroutine()
    {
        yield break;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    virtual public IEnumerator ScreenShowFinishCoroutine()
    {
        yield break;
    }

    /*
    // Amikor az új képernyő megjelenítése befejeződött, akkor ezt meghívja a ScreenController
    // Illetve feladat váltásoknál a TaskController is meghívhatja a HideGameElement és az InitCoroutine után
    virtual public IEnumerator ShowGameElement()
    {
        yield return null;
    }
    */

    /// <summary>
    /// A kérdéshez tartozó játék elemeket kell eltávolítani, ha meghívják ezt a metódust. A TaskController fogja új feladatnál ha az új feladatot ugyan annak a képernyőnek kell megjelenítenie. 
    /// </summary>
    /// <returns></returns>
    virtual public IEnumerator HideGameElement()
    {
        yield break;
    }

    /// <summary>
    /// Játéknak vége letelt az idő, vagy a játék befejeződött
    /// </summary>
    /// <returns></returns>
    virtual public IEnumerator GameEnd()
    {
        yield break;
    }

    // A képernyő eltüntetése megkezdődött. Csinálhatunk valamit ilyenkor ha szükséges
    // Ezt a ScreenController hívja meg képernyő váltásnál
    virtual public IEnumerator ScreenHideStart()
    {
        yield break;
    }

    // A képernyő teljesen eltünt. Csinálhatunk valamit ilyenkor ha szükséges.
    // De figyelni kell, mivel a következő pillanatban már inaktív lesz az egész képernyő. 
    // Tehát azonnal meg kell tennünk amit akarunk nem indíthatunk a képernyőn egy coroutine-t mivel úgy sem fog lefutni.
    // Kikapcsolt gameObject-eken nem fut a coroutine.
    // Ezt a ScreenController hívja meg képernyő váltásnál
    virtual public IEnumerator ScreenHideFinish()
    {
        yield break;
    }

    /// <summary>
    /// A tanári tablet óraterv előnézeti képernyője hívja meg ha meg kell mutatni a játék előnézetét.
    /// A task paraméter tartalmazza a játék képernyőjének adatait.
    /// </summary>
    /// <param name="task">A megjelenítendő képernyő adata</param>
    virtual public IEnumerator Preview(TaskAncestor task)
    {
        yield break;
    }

    /// <summary>
    /// TaskController hívja meg ha történt valamilyen esemény amit a játéknak végre kellene hajtania.
    /// </summary>
    /// <remarks>
    /// jsonNode változóban található a történt esemény.
    /// </remarks>
    /// <param name="jsonNode">Esetlegesen további paraméterek lehetnek benne.</param>
    virtual public void EventHappened(JSONNode jsonNode)
    {

    }
}
