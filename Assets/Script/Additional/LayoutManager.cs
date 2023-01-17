using UnityEngine;
using System.Collections;

/// <summary>
/// A játékok képi világának váltását segíti elő.
/// 
/// Meg lehet adni neki, hogy melyik képi világot használja.
/// 
/// Majd ha keresünk egy képet, akkor azt a korábban beállított képi világból szedi.
/// </summary>
public class LayoutManager : MonoBehaviour {

    LayoutSet[] layoutSets;

    LayoutSet actLayoutSet;

    static public string actLayoutSetName = "1";

	// Use this for initialization
	void Awake () {
        // Megkeressük a gameObject-en található LayoutSet komponenseket
        layoutSets = GetComponentsInChildren<LayoutSet>(true);

        SetLayoutSet("1"); // Beállítjuk az első layoutot alapnak
        SetLayoutSet("2"); // Beállítjuk a második layoutot alapnak, ha nincs marad az első
    }

    void OnEnable() {
        SetLayoutSet(actLayoutSetName);
    }

    /// <summary>
    /// Beállítja a megadott értékű layout-ra a LayoutManager-t. Az első a nulladik.
    /// </summary>
    /// <param name="layoutSet">A kívánt layoutnak a neve.</param>
    public void SetLayoutSet(string requiredLayoutSet) {
        foreach (LayoutSet layoutSet in layoutSets)
        {
            if (layoutSet.name == requiredLayoutSet) {
                actLayoutSet = layoutSet;
                actLayoutSetName = requiredLayoutSet;
                return;
            }
        }

        Debug.Log("Nem találtuk a megadott design set-et : " + requiredLayoutSet);

        actLayoutSet = null; // Ha nem találjuk meg null lesz az aktuális layout set
        actLayoutSet = layoutSets[0]; // Mégsem lesz null, hanem az első layout set lesz, bár ez dobhat kivételt, ha egy sincs
    }

    /// <summary>
    /// A sorban a következő layoutra vált.
    /// </summary>
    public void ChangeLayout() {
        // Megkeressük az aktuális layout-ot
        int actSetIndex = 0;
        for (int i = 0; i < layoutSets.Length; i++)
        {
            if (layoutSets[i] == actLayoutSet) {
                actSetIndex = i + 1; // Ha megtaláltuk vesszük a következő indexet
                break;
            }
        }

        if (actSetIndex >= layoutSets.Length)
            actSetIndex = 0;

        if (actSetIndex < layoutSets.Length)
            SetLayoutSet(layoutSets[actSetIndex].name);
    }

    /// <summary>
    /// Vissza adja az aktuális layoutSet-ből a kívánt nevű kép Sprite objektumát.
    /// </summary>
    /// <param name="name">A kívánt nevű kép az aktuális layoutSet-ből.</param>
    /// <returns></returns>
    public Sprite GetSprite(string name)
    {
        if (actLayoutSet != null) { // Ha be van állítva egy layoutSet, akkor megkérdezzük van-e benne a megadott nevű kép
            return actLayoutSet.GetSprite(name);
        }

        return null;
    }

    /// <summary>
    /// Vissza adja az aktuális layoutSet-ből a kívánt nevű szint.
    /// </summary>
    /// <param name="name">A kívánt nevű kép az aktuális layoutSet-ből.</param>
    /// <returns></returns>
    public Color GetColor(string name)
    {
        if (actLayoutSet != null)
        { // Ha be van állítva egy layoutSet, akkor megkérdezzük van-e benne a megadott nevű kép
            return actLayoutSet.GetColor(name);
        }

        return Color.white;
    }
}
