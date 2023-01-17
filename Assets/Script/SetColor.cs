using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ez az osztály beállítja a gameObject-jén található visualis komponens színét, ami lehet egy image, vagy text componens is.
/// A porgram megkeresi valamelyik ős gameObject-jén azt a componenst ami implementálja az IColorProvider interfészt, 
/// ami egy script lesz.
/// Ennek a szkriptnek elküldi a colorToken-t, majd a szkript vissza adja ahhoz a tokenhez tartozó szint.
/// </summary>
public class SetColor : MonoBehaviour
{
    public string colorToken;

    public delegate Color ColorProvider(string s);
    public ColorProvider colorProvider;

    Text text;
    Image image;

    bool start = false;

    // Use this for initialization
    void Awake()
    {
        text = GetComponent<Text>();
        image = GetComponent<Image>();

        IColorProvider iColorProvider = Common.SearchClass<IColorProvider>(transform);

        if (iColorProvider != null)
            colorProvider = iColorProvider.ColorProvider;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Refresh();
    }

    public void Refresh()
    {
        if (colorProvider != null)
        {
            if (text)
                text.color = colorProvider(colorToken);

            if (image)
                image.color = colorProvider(colorToken); ;
        }
    }

    void OnEnable()
    {
        Refresh();
    }
}
