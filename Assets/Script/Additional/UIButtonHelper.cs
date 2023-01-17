using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI.Button komponensének használatához.
/// Be lehet állítani a segítségével, hogy ha megnyomták a gombot, milyen eljárást hívjon meg és milyen paraméterrel.
/// Ez hasonló amit szerkesztési időben lehet beállítani a gomb komponensen. Tehát az onClick eseményében meg lehet adni egy
/// metódust és egy int vagy string típusu paramétert.
/// 
/// Használata:
/// Tegyük a button komponens mellé ezt a scriptet.
/// A button komponensnek hozzunk létre egy OnClick eseményt és állítsuk be ennek a Scriptnek a ButtonClick metódusát.
/// 
/// Programból keressük meg ezt a scriptet.
/// Majd integer esetben megadjuk az i érétékét és a callBackInteger függvény mutatót ráállítjuk egy int-et fogadó metódusra.
/// 
/// Ha megnyomták a gombot, akkor meg lesz hívva a megadott metódus a beállított értékkel.
/// </summary>
public class UIButtonHelper : MonoBehaviour
{
    public int i;
    public Common.CallBack_In_Int callBackInteger;

    public string s;
    public Common.CallBack_In_String callBackString;

    public void SetInteger(int i, Common.CallBack_In_Int callBackInteger)
    {
        this.i = i;
        this.callBackInteger = callBackInteger;
    }

    public void SetString(string s, Common.CallBack_In_String callBackString)
    {
        this.s = s;
        this.callBackString = callBackString;
    }

    public void ButtonClick()
    {
        if (callBackInteger != null)
            callBackInteger(i);

        if (callBackString != null)
            callBackString(s);
    }
}
