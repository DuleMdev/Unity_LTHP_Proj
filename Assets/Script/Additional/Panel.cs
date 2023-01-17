using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Ez az objektum egy panelt hoz létre amelyen egy egysoros szöveg található.

Meg lehet adni a panel sprite-ját, a színét és a rajta elhelyezendő szöveget
*/


public class Panel : MonoBehaviour {

    public Transform moveTransform;              // A szöveget mozgató gameObject

    SpriteRenderer spriteRenderer;      // A szín beállításához
    TextMesh textMesh;                  // A válasz panel szövegének beállításához

    public string text { get { return textMesh.text; } set { textMesh.text = value; }  }

    void Awake() {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMesh>();

        moveTransform = transform.Find("Move");
    }

    // Beállítjuk a válasz panel színét
    public void SetColor(Color color) {
        spriteRenderer.color = color;
    }

    // Beállítja a válasz panel panelsprite-ját (az az, hogy milyen sprite legyen a háttere)
    public void SetSprite(Sprite sprite) {
        spriteRenderer.sprite = sprite;
    }

    /*
    // Beállítjuk a válasz panelen levő szöveget
    public void SetText(string text) {
        textMesh.text = text;
    }
    */
}
