using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlaceMenuItem : MonoBehaviour
{
    GameObject selected;    // A kijelöltséget mutató GameObject
    Text textMenuName;      // A menü neve

	// Use this for initialization
	void Awake () {
        selected = gameObject.SearchChild("Selected").gameObject;
        textMenuName = gameObject.SearchChild("Text").GetComponent<Text>();
	}

    public void Initialize(string menuName)
    {
        textMenuName.text = menuName;
    }

    public void Selected(bool selected)
    {
        this.selected.SetActive(selected);
    }
}
