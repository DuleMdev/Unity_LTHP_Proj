using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ez a script letiltja a megadott gameObjectet ha utasítást kap rá.
/// Ezt egy olyan GameObject-re kell tenni ami nem lesz letiltva különben nem tud működni
/// 
/// A Script megvizsgálja, hogy van-e az ősei között egy olyan script ami implementálja az IBoolProvider interface-t.
/// Ha van azt hívja meg a megadott tokennel majd a választól függően tiltja vagy engedélyezi a GameObject-et.
/// 
/// A wasStart változó arra szolgál, hogy ne hívódjon meg az OnEnable metódusban a Refresh mivel a boolProvider számára
/// szükséges a ConfigurationController és előfordulhat, hogy az még nem inicializálta magát.
/// </summary>
public class SetEnabledGameObject : MonoBehaviour
{
    [Tooltip("A tiltandó/engedélyezendő gameObject")]
    public GameObject adjustGameObject;
    public string boolToken;

    public delegate bool BoolProvider(string s);
    public BoolProvider boolProvider;

    public bool wasStart = false;

    // Use this for initialization
    void Awake()
    {
        IBoolProvider iBoolProvider = Common.SearchClass<IBoolProvider>(transform);

        if (iBoolProvider != null)
            boolProvider = iBoolProvider.BoolProvider;
        else
            Debug.LogError("Nem találtam IBoolProveder-t implementáló scriptet.");
    }

    void Start()
    {
        wasStart = true;
        Refresh(); // Mivel az OnEnable kezdetben lehet, hogy nem hívódik meg
    }

    public void Refresh()
    {
        if (boolProvider != null)
            adjustGameObject.SetActive(boolProvider(boolToken));
    }

    void OnEnable()
    {
        if (wasStart)
            Refresh();
    }
}
