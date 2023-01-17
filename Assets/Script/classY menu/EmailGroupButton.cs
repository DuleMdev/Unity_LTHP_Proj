using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A menüben a csoport böngésző panelben található email csoport kiválasztásához készült ez a script.
/// 
/// Kiválasztottság beállításához hívjuk meg a SetActive metódust és adjuk át neki a csoport azonosítót. Ha egyezik a csoport azonosító
/// akkor activeColor ha nem akkor az inactiveColor szinét fogja felvenni.
/// </summary>
public class EmailGroupButton : MonoBehaviour
{
    public Color inactiveColor;
    public Color activeColor;

    Text text;
    Image image;    // A háttér színének beállításához. Más a szín ha ki van választva a gomb és más ha nincs.

    EmailGroup emailGroup;
    Common.CallBack_In_String callBack;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponentInChildren<Text>();
        image = GetComponentInChildren<Image>();
    }

    public void Initialize(EmailGroup emailGroup, Common.CallBack_In_String callBack)
    {
        this.callBack = callBack;
        this.emailGroup = emailGroup;

        text.text = emailGroup.name;

        SetActive("");
    }

    public void SetActive(string emailGroupID)
    {
        image.color = (emailGroup.id == emailGroupID) ? activeColor : inactiveColor;
    }

    public void ButtonClick()
    {
        if (callBack != null)
            callBack(C.JSONKeys.mailListID + ":" + emailGroup.id);
    }
}
